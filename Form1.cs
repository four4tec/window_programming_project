using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2
{
    public partial class Form1 : Form
    {
        //parameter
        private const int player_speed = 3;
        private const int form_width = 500;
        private const int form_height = 600;
        private const int max_enemy = 30;
        private player_u player;
        private enemy_u[] enemy;
        private Timer maintimer;
        private Label hit_status;
        private Label remain_status;
        private Label start;
        private RadioButton difficult_select_easy;
        private RadioButton difficult_select_hard;
        private List<bullet> bullet_list;
        private List<bullet> bullet_remove;
        private List<bullet> bullet_enemy;
        private int hit_cnt;
        private int remain_cnt;
        private int clock_cnt;
        private int now_enemy;
        private int start_status;//0->start,1->pause,2->restart
        private bool game_over_flag;
        //
        public Form1()
        {
            InitializeComponent();
            //event set
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            //form set
            this.Width = form_width + 16;
            this.Height = form_height + 39;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //player set
            player = new player_u();
            player.lx = form_width / 2;
            player.ly = form_height - 100;
            this.Controls.Add(player.u);
            player.update();
            //enemy set
            enemy = new enemy_u[max_enemy];
            for (int i = 0; i < max_enemy; ++i)
            {
                enemy[i] = new enemy_u();
                this.Controls.Add(enemy[i].u);
                enemy[i].update();
            }
            //timer set
            maintimer = new Timer();
            maintimer.Interval = 1000 / 30;
            maintimer.Tick += new System.EventHandler(fps);
            //status set
            hit_status = new Label();
            hit_status.Location = new Point(223, 33);
            hit_status.Size = new Size(100, 33);
            hit_status.Text = "擊中數：0";
            hit_status.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(hit_status);
            remain_status = new Label();
            remain_status.Location = new Point(103, 33);
            remain_status.Size = new Size(100, 33);
            remain_status.Text = "目標剩餘：" + max_enemy;
            remain_status.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(remain_status);
            //start button set
            start = new Label();
            this.Controls.Add(start);
            start.Location = new Point(33, 33);
            start.Size = new Size(70, 33);
            start.Text = "開始";
            start.TextAlign = ContentAlignment.MiddleCenter;
            start.BackColor = Color.Yellow;
            start.BringToFront();
            this.start.Click += new System.EventHandler(this.start_cli);
            //difficult select set
            difficult_select_easy = new RadioButton();
            difficult_select_easy.Location = new Point(353,33);
            difficult_select_easy.Size = new Size(33,33);
            difficult_select_easy.Text = "簡單";
            difficult_select_easy.Checked=true;
            this.Controls.Add(difficult_select_easy);
            difficult_select_hard = new RadioButton();
            difficult_select_hard.Location = new Point(420, 33);
            difficult_select_hard.Size = new Size(33, 33);
            difficult_select_hard.Text = "困難";
            difficult_select_hard.Checked = false;
            this.Controls.Add(difficult_select_hard);
            //bullet list set
            bullet_list = new List<bullet>();
            bullet_list.Clear();
            bullet_remove = new List<bullet>();
            bullet_remove.Clear();
            bullet_enemy = new List<bullet>();
            bullet_enemy.Clear();
            //parameter set
            hit_cnt = 0;
            remain_cnt = max_enemy;
            clock_cnt = 0;
            now_enemy = 0;
            start_status = 0;
            game_over_flag = false;
        }
        private void fps(object sender, EventArgs e)
        {
            //player shoot
            if (player.shoot_flag && player.shoot_cold_down == 0)
            {
                bullet tmp = new bullet();
                tmp.lx = player.lx;
                tmp.ly = player.ly;
                tmp.vy = -10;
                bullet_list.Add(tmp);
                tmp.update();
                this.Controls.Add(tmp.u);
                player.shoot_cold_down = player.shoot_interval;
            }
            if (player.shoot_cold_down > 0) --player.shoot_cold_down;
            //enemy shoot
            for (int i=0;i<max_enemy;++i) {
                if (enemy[i].shoot_flag &&
                    enemy[i].shoot_cold_down == 0 &&
                    enemy[i].display &&
                    difficult_select_hard.Checked)
                {
                    Random rand = new Random(Guid.NewGuid().GetHashCode());
                    bullet tmp = new bullet();
                    tmp.lx = enemy[i].lx;
                    tmp.ly = enemy[i].ly;
                    tmp.vy = 4;
                    tmp.vx = rand.Next() % 5 - 2;
                    bullet_enemy.Add(tmp);
                    tmp.update();
                    this.Controls.Add(tmp.u);
                    enemy[i].shoot_cold_down = enemy[i].shoot_interval;
                }
                if (enemy[i].shoot_cold_down > 0 && enemy[i].display) {
                    --enemy[i].shoot_cold_down;
                }
            }
            //player x,y
            player.lx += (player.right_flag - player.left_flag) * player_speed;
            if (player.lx >= form_width) player.lx = form_width;
            if (player.lx <= 0) player.lx = 0;
            player.ly += (player.down_flag - player.up_flag) * player_speed;
            if (player.ly >= form_height) player.ly = form_height;
            if (player.ly <= 100) player.ly = 100;
            player.update();
            //player bullet x,y
            foreach (bullet tmp in bullet_list)
            {
                tmp.lx += tmp.vx;
                tmp.ly += tmp.vy;
                if (tmp.lx > form_width ||
                    tmp.lx < 0 ||
                    tmp.ly > form_height ||
                    tmp.ly < 100)
                {
                    bullet_remove.Add(tmp);
                }
                tmp.update();
            }
            foreach (bullet tmp in bullet_remove)
            {
                this.Controls.Remove(tmp.u);
                bullet_list.Remove(tmp);
            }
            bullet_remove.Clear();
            //enemy bullet x,y
            foreach (bullet tmp in bullet_enemy)
            {
                tmp.lx += tmp.vx;
                tmp.ly += tmp.vy;
                if (tmp.lx > form_width ||
                    tmp.lx < 0 ||
                    tmp.ly > form_height ||
                    tmp.ly < 100)
                {
                    bullet_remove.Add(tmp);
                }
                tmp.update();
            }
            foreach (bullet tmp in bullet_remove)
            {
                this.Controls.Remove(tmp.u);
                bullet_enemy.Remove(tmp);
            }
            bullet_remove.Clear();
            //enemy x,y
            Random r = new Random(Guid.NewGuid().GetHashCode());
            if (clock_cnt > (now_enemy + 1) * 30 && now_enemy < max_enemy)
            {
                bool t = (player.lx >= form_width / 2);
                enemy[now_enemy].display = true;
                enemy[now_enemy].lx = t?0:form_width;
                enemy[now_enemy].ly = 150;
                enemy[now_enemy].vx = (r.Next() % 5 + 3) * (t ? 1 : -1);
                ++now_enemy;
            }
            for (int i = 0; i < max_enemy; ++i)
            {
                if (enemy[i].display)
                {
                    enemy[i].lx += enemy[i].vx;
                    enemy[i].ly += enemy[i].vy;
                    if (enemy[i].is_hitted(bullet_list) && enemy[i].display)
                    {
                        ++hit_cnt;
                        --remain_cnt;
                        enemy[i].display = false;
                        enemy[i].lx = -50;
                        enemy[i].ly = -50;
                    }
                    if ((enemy[i].lx > form_width ||
                        enemy[i].lx < 0 ||
                        enemy[i].ly > form_height ||
                        enemy[i].ly < 0) &&
                        enemy[i].display)
                    {
                        --remain_cnt;
                        enemy[i].display = false;
                        enemy[i].lx = -50;
                        enemy[i].ly = -50;
                    }
                    enemy[i].update();
                }
            }
            //player
            if (player.lx >= form_width) player.lx = form_width;
            if (player.lx <= 0) player.lx = 0;
            if (player.is_hitted(bullet_enemy)) {
                game_over_flag=true;
            }
            player.update();
            //update
            hit_status.Text = "擊中數：" + hit_cnt + "\n"+bullet_enemy.Count;
            remain_status.Text = "目標剩餘：" + remain_cnt;
            ++clock_cnt;
            //game game
            if (remain_cnt == 0 || game_over_flag)
            {
                maintimer.Stop();
                start.Text = "重新開始";
                start_status = 2;
                MessageBox.Show("遊戲結束！\n擊中數：" + hit_cnt);
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    player.left_flag = 1;
                    break;
                case Keys.Right:
                    player.right_flag = 1;
                    break;
                case Keys.Down:
                    player.down_flag = 1;
                    break;
                case Keys.Up:
                    player.up_flag = 1;
                    break;
                case Keys.Space:
                    player.shoot_flag = true;
                    break;
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    player.left_flag = 0;
                    break;
                case Keys.Right:
                    player.right_flag = 0;
                    break;
                case Keys.Down:
                    player.down_flag = 0;
                    break;
                case Keys.Up:
                    player.up_flag = 0;
                    break;
                case Keys.Space:
                    player.shoot_flag = false;
                    break;
            }
        }
        private void start_cli(object sender, EventArgs e)
        {
            if (start_status == 0)
            {
                this.Controls.Remove(difficult_select_easy);
                this.Controls.Remove(difficult_select_hard);
                start_status = 1;
                start.Text = "暫停";
                maintimer.Start();
            }
            else if (start_status == 1)
            {
                start_status = 0;
                start.Text = "繼續";
                maintimer.Stop();
            }
            else if (start_status == 2)
            {
                //player set
                player.lx = form_width / 2;
                player.ly = form_height - 100;
                player.update();
                //enemy set
                for (int i = 0; i < max_enemy; ++i)
                {
                    enemy[i].lx = -50;
                    enemy[i].ly = -50;
                    enemy[i].vx = 0;
                    enemy[i].vy = 0;
                    enemy[i].display = false;
                    enemy[i].update();
                }
                //timer set
                maintimer.Stop();
                //status set
                hit_status.Text = "擊中數：0";
                remain_status.Text = "目標剩餘："+max_enemy;
                //start button set
                start.Text = "開始";
                start.BringToFront();
                //bullet list set
                foreach (bullet tmp in bullet_list)
                {
                    tmp.u.Hide();
                }
                bullet_list.Clear();
                foreach (bullet tmp in bullet_enemy)
                {
                    tmp.u.Hide();
                }
                bullet_enemy.Clear();
                bullet_remove.Clear();
                //parameter set
                hit_cnt = 0;
                remain_cnt = max_enemy;
                player.right_flag = 0;
                player.left_flag = 0;
                player.up_flag = 0;
                player.down_flag = 0;
                player.shoot_cold_down = 0;
                player.shoot_flag = false;
                clock_cnt = 0;
                now_enemy = 0;
                start_status = 0;
                game_over_flag = false;
                this.Controls.Add(difficult_select_easy);
                this.Controls.Add(difficult_select_hard);
            }
        }
    }
    public class bullet
    {
        public Label u;//PictureBox may better?
        public int lx;
        public int ly;
        public int vx;
        public int vy;
        public int ax;
        public int ay;
        public int sizex = 10;
        public int sizey = 10;
        public int hitbox_size = 5;
        //
        public bullet()
        {
            u = new Label();
            lx = 0;
            ly = 0;
            vx = 0;
            vy = 0;
            ax = 0;
            ay = 0;
            this.u.Size = new Size(10, 10);
            this.u.Image = Image.FromFile(@"..\..\src\1.png");
            this.u.BackColor = System.Drawing.Color.Transparent;
        }
        //
        public bool change_lva(int x, int y, int type)
        {
            //change location -> type 1, velocity -> type 2, acceleration -> type 3
            switch (type)
            {
                case 1:
                    this.lx = x;
                    this.ly = y;
                    break;
                case 2:
                    this.vx = x;
                    this.vy = y;
                    break;
                case 3:
                    this.ax = x;
                    this.ay = y;
                    break;
                default:
                    return false;
            }
            return true;
        }
        //
        public bool update()
        {
            this.u.Location = new Point(this.lx - sizex / 2, this.ly - sizey / 2);
            return true;
        }
        //
    }
    public class enemy_u : bullet
    {
        public int shoot_interval;
        public int health_point;
        public bool display;
        public int shoot_cold_down;
        public bool shoot_flag;
        public enemy_u()
        {
            this.hitbox_size = 5;
            this.lx = -50;
            this.ly = -50;
            this.display = false;
            health_point = 1;
            this.u.Image = Image.FromFile(@"..\..\src\3.png");
            this.shoot_interval = 3;
            this.shoot_flag = true;
            this.shoot_cold_down = shoot_interval;
        }
        public bool is_hitted(List<bullet> bullet_list)
        {
            foreach (bullet tmp in bullet_list)
            {
                if ((Math.Pow(tmp.lx - this.lx, 2) + Math.Pow(tmp.ly - this.ly, 2)) <=
                    Math.Pow(this.hitbox_size + tmp.hitbox_size, 2))
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class player_u : enemy_u
    {
        public int right_flag;
        public int left_flag;
        public int up_flag;
        public int down_flag;
        public player_u()
        {
            this.lx = 0;
            this.ly = 0;
            this.display = true;
            health_point = 1;
            this.u.Image = Image.FromFile(@"..\..\src\5.png");
            this.shoot_interval = 4;
            this.shoot_flag = false;
            this.shoot_cold_down = shoot_interval;
            this.hitbox_size = 4;
        }
    }
}
