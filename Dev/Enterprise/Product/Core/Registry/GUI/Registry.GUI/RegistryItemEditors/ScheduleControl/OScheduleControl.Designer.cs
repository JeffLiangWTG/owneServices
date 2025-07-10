using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OScheduleControl : ZUserControl
	{
		private ZLabel SundayLabel;
		private ZLabel SaturdayLabel;
		private ZLabel FridayLabel;
		private ZLabel ThursdayLabel;
		private ZLabel WednesdayLabel;
		private ZLabel TuesdayLabel;
		private ZLabel MondayLabel;
		protected Enterprise.ZArchitecture.ZTextBox MondayTextBox;
		protected Enterprise.ZArchitecture.ZTextBox TuesdayTextBox;
		protected Enterprise.ZArchitecture.ZTextBox WednesdayTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ThursdayTextBox;
		protected Enterprise.ZArchitecture.ZTextBox FridayTextBox;
		protected Enterprise.ZArchitecture.ZTextBox SaturdayTextBox;
		protected Enterprise.ZArchitecture.ZTextBox SundayTextBox;
		private ZLabel oLabel27;
		private ZLabel oLabel28;
		private ZLabel oLabel29;
		private ZLabel oLabel30;
		private ZLabel oLabel31;
		private ZLabel oLabel32;
		private ZLabel oLabel33;
		private ZLabel oLabel34;
		private ZLabel oLabel35;
		private ZLabel oLabel36;
		private ZLabel oLabel37;
		private ZLabel oLabel38;
		private ZLabel oLabel39;
		private ZLabel oLabel40;
		private ZLabel oLabel41;
		private ZLabel oLabel42;
		private ZLabel oLabel43;
		private ZLabel oLabel44;
		private ZLabel oLabel45;
		private ZLabel oLabel46;
		private ZLabel oLabel47;
		private ZLabel oLabel48;
		private ZLabel oLabel49;
		private ZLabel oLabel50;
		private ZLabel oLabel51;
		private ZLabel oLabel52;
		private CargoWise.Windows.UI.KPanel GridPanel;

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OScheduleControl));
			this.SundayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SaturdayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FridayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ThursdayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WednesdayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TuesdayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MondayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MondayTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TuesdayTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WednesdayTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ThursdayTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FridayTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SaturdayTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SundayTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.oLabel27 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel28 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel32 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel33 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel35 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel36 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel37 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel38 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel39 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel40 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel41 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel42 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel43 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel44 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel45 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel46 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel47 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel48 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel49 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel50 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel51 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel52 = new Enterprise.ZArchitecture.ZLabel();
			this.GridPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SundayLabel
			// 
			this.SundayLabel.AutoSize = true;
			this.SundayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 225, true);
			this.SundayLabel.Name = "SundayLabel";
			this.SundayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 13, true);
			this.SundayLabel.TabIndex = 117;
			this.SundayLabel.Text = "Sun";
			// 
			// SaturdayLabel
			// 
			this.SaturdayLabel.AutoSize = true;
			this.SaturdayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 197, true);
			this.SaturdayLabel.Name = "SaturdayLabel";
			this.SaturdayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 13, true);
			this.SaturdayLabel.TabIndex = 116;
			this.SaturdayLabel.Text = "Sat";
			// 
			// FridayLabel
			// 
			this.FridayLabel.AutoSize = true;
			this.FridayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 169, true);
			this.FridayLabel.Name = "FridayLabel";
			this.FridayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 13, true);
			this.FridayLabel.TabIndex = 115;
			this.FridayLabel.Text = "Fri";
			// 
			// ThursdayLabel
			// 
			this.ThursdayLabel.AutoSize = true;
			this.ThursdayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 141, true);
			this.ThursdayLabel.Name = "ThursdayLabel";
			this.ThursdayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 13, true);
			this.ThursdayLabel.TabIndex = 114;
			this.ThursdayLabel.Text = "Thu";
			// 
			// WednesdayLabel
			// 
			this.WednesdayLabel.AutoSize = true;
			this.WednesdayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 113, true);
			this.WednesdayLabel.Name = "WednesdayLabel";
			this.WednesdayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.WednesdayLabel.TabIndex = 113;
			this.WednesdayLabel.Text = "Wed";
			// 
			// TuesdayLabel
			// 
			this.TuesdayLabel.AutoSize = true;
			this.TuesdayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			this.TuesdayLabel.Name = "TuesdayLabel";
			this.TuesdayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 13, true);
			this.TuesdayLabel.TabIndex = 112;
			this.TuesdayLabel.Text = "Tue";
			// 
			// MondayLabel
			// 
			this.MondayLabel.AutoSize = true;
			this.MondayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.MondayLabel.Name = "MondayLabel";
			this.MondayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 13, true);
			this.MondayLabel.TabIndex = 111;
			this.MondayLabel.Text = "Mon";
			// 
			// MondayTextBox
			// 
			this.MondayTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 55, true);
			this.MondayTextBox.Name = "MondayTextBox";
			this.MondayTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.MondayTextBox.TabIndex = 118;
			this.MondayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HoursTextBox_KeyPress);
			// 
			// TuesdayTextBox
			// 
			this.TuesdayTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 83, true);
			this.TuesdayTextBox.Name = "TuesdayTextBox";
			this.TuesdayTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.TuesdayTextBox.TabIndex = 119;
			this.TuesdayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HoursTextBox_KeyPress);
			// 
			// WednesdayTextBox
			// 
			this.WednesdayTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 111, true);
			this.WednesdayTextBox.Name = "WednesdayTextBox";
			this.WednesdayTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.WednesdayTextBox.TabIndex = 120;
			this.WednesdayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HoursTextBox_KeyPress);
			// 
			// ThursdayTextBox
			// 
			this.ThursdayTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 139, true);
			this.ThursdayTextBox.Name = "ThursdayTextBox";
			this.ThursdayTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.ThursdayTextBox.TabIndex = 121;
			this.ThursdayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HoursTextBox_KeyPress);
			// 
			// FridayTextBox
			// 
			this.FridayTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 167, true);
			this.FridayTextBox.Name = "FridayTextBox";
			this.FridayTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.FridayTextBox.TabIndex = 122;
			this.FridayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HoursTextBox_KeyPress);
			// 
			// SaturdayTextBox
			// 
			this.SaturdayTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 195, true);
			this.SaturdayTextBox.Name = "SaturdayTextBox";
			this.SaturdayTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.SaturdayTextBox.TabIndex = 123;
			this.SaturdayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HoursTextBox_KeyPress);
			// 
			// SundayTextBox
			// 
			this.SundayTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 223, true);
			this.SundayTextBox.Name = "SundayTextBox";
			this.SundayTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.SundayTextBox.TabIndex = 124;
			this.SundayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HoursTextBox_KeyPress);
			// 
			// oLabel27
			// 
			this.oLabel27.Image = ((System.Drawing.Image)(resources.GetObject("oLabel27.Image")));
			this.oLabel27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 14, true);
			this.oLabel27.Name = "oLabel27";
			this.oLabel27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel27.TabIndex = 176;
			// 
			// oLabel28
			// 
			this.oLabel28.Image = ((System.Drawing.Image)(resources.GetObject("oLabel28.Image")));
			this.oLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 14, true);
			this.oLabel28.Name = "oLabel28";
			this.oLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel28.TabIndex = 175;
			// 
			// oLabel29
			// 
			this.oLabel29.Image = ((System.Drawing.Image)(resources.GetObject("oLabel29.Image")));
			this.oLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 14, true);
			this.oLabel29.Name = "oLabel29";
			this.oLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel29.TabIndex = 174;
			// 
			// oLabel30
			// 
			this.oLabel30.Image = ((System.Drawing.Image)(resources.GetObject("oLabel30.Image")));
			this.oLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 14, true);
			this.oLabel30.Name = "oLabel30";
			this.oLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel30.TabIndex = 173;
			// 
			// oLabel31
			// 
			this.oLabel31.Image = ((System.Drawing.Image)(resources.GetObject("oLabel31.Image")));
			this.oLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 14, true);
			this.oLabel31.Name = "oLabel31";
			this.oLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel31.TabIndex = 172;
			// 
			// oLabel32
			// 
			this.oLabel32.Image = ((System.Drawing.Image)(resources.GetObject("oLabel32.Image")));
			this.oLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 14, true);
			this.oLabel32.Name = "oLabel32";
			this.oLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel32.TabIndex = 171;
			// 
			// oLabel33
			// 
			this.oLabel33.Image = ((System.Drawing.Image)(resources.GetObject("oLabel33.Image")));
			this.oLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 14, true);
			this.oLabel33.Name = "oLabel33";
			this.oLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel33.TabIndex = 170;
			// 
			// oLabel34
			// 
			this.oLabel34.Image = ((System.Drawing.Image)(resources.GetObject("oLabel34.Image")));
			this.oLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 14, true);
			this.oLabel34.Name = "oLabel34";
			this.oLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel34.TabIndex = 169;
			// 
			// oLabel35
			// 
			this.oLabel35.Image = ((System.Drawing.Image)(resources.GetObject("oLabel35.Image")));
			this.oLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 14, true);
			this.oLabel35.Name = "oLabel35";
			this.oLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel35.TabIndex = 168;
			// 
			// oLabel36
			// 
			this.oLabel36.Image = ((System.Drawing.Image)(resources.GetObject("oLabel36.Image")));
			this.oLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 14, true);
			this.oLabel36.Name = "oLabel36";
			this.oLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel36.TabIndex = 167;
			// 
			// oLabel37
			// 
			this.oLabel37.Image = ((System.Drawing.Image)(resources.GetObject("oLabel37.Image")));
			this.oLabel37.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 14, true);
			this.oLabel37.Name = "oLabel37";
			this.oLabel37.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel37.TabIndex = 166;
			// 
			// oLabel38
			// 
			this.oLabel38.Image = ((System.Drawing.Image)(resources.GetObject("oLabel38.Image")));
			this.oLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 14, true);
			this.oLabel38.Name = "oLabel38";
			this.oLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel38.TabIndex = 165;
			// 
			// oLabel39
			// 
			this.oLabel39.Image = ((System.Drawing.Image)(resources.GetObject("oLabel39.Image")));
			this.oLabel39.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 14, true);
			this.oLabel39.Name = "oLabel39";
			this.oLabel39.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel39.TabIndex = 164;
			// 
			// oLabel40
			// 
			this.oLabel40.Image = ((System.Drawing.Image)(resources.GetObject("oLabel40.Image")));
			this.oLabel40.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 14, true);
			this.oLabel40.Name = "oLabel40";
			this.oLabel40.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel40.TabIndex = 163;
			// 
			// oLabel41
			// 
			this.oLabel41.Image = ((System.Drawing.Image)(resources.GetObject("oLabel41.Image")));
			this.oLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 14, true);
			this.oLabel41.Name = "oLabel41";
			this.oLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel41.TabIndex = 162;
			// 
			// oLabel42
			// 
			this.oLabel42.Image = ((System.Drawing.Image)(resources.GetObject("oLabel42.Image")));
			this.oLabel42.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 14, true);
			this.oLabel42.Name = "oLabel42";
			this.oLabel42.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel42.TabIndex = 161;
			// 
			// oLabel43
			// 
			this.oLabel43.Image = ((System.Drawing.Image)(resources.GetObject("oLabel43.Image")));
			this.oLabel43.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 14, true);
			this.oLabel43.Name = "oLabel43";
			this.oLabel43.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel43.TabIndex = 160;
			// 
			// oLabel44
			// 
			this.oLabel44.Image = ((System.Drawing.Image)(resources.GetObject("oLabel44.Image")));
			this.oLabel44.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 14, true);
			this.oLabel44.Name = "oLabel44";
			this.oLabel44.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel44.TabIndex = 159;
			// 
			// oLabel45
			// 
			this.oLabel45.Image = ((System.Drawing.Image)(resources.GetObject("oLabel45.Image")));
			this.oLabel45.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 14, true);
			this.oLabel45.Name = "oLabel45";
			this.oLabel45.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel45.TabIndex = 158;
			// 
			// oLabel46
			// 
			this.oLabel46.Image = ((System.Drawing.Image)(resources.GetObject("oLabel46.Image")));
			this.oLabel46.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 14, true);
			this.oLabel46.Name = "oLabel46";
			this.oLabel46.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel46.TabIndex = 157;
			// 
			// oLabel47
			// 
			this.oLabel47.Image = ((System.Drawing.Image)(resources.GetObject("oLabel47.Image")));
			this.oLabel47.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 14, true);
			this.oLabel47.Name = "oLabel47";
			this.oLabel47.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel47.TabIndex = 156;
			// 
			// oLabel48
			// 
			this.oLabel48.Image = ((System.Drawing.Image)(resources.GetObject("oLabel48.Image")));
			this.oLabel48.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 14, true);
			this.oLabel48.Name = "oLabel48";
			this.oLabel48.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel48.TabIndex = 155;
			// 
			// oLabel49
			// 
			this.oLabel49.Image = ((System.Drawing.Image)(resources.GetObject("oLabel49.Image")));
			this.oLabel49.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 14, true);
			this.oLabel49.Name = "oLabel49";
			this.oLabel49.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel49.TabIndex = 154;
			// 
			// oLabel50
			// 
			this.oLabel50.Image = ((System.Drawing.Image)(resources.GetObject("oLabel50.Image")));
			this.oLabel50.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 14, true);
			this.oLabel50.Name = "oLabel50";
			this.oLabel50.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 32, true);
			this.oLabel50.TabIndex = 153;
			// 
			// oLabel51
			// 
			this.oLabel51.AutoSize = true;
			this.oLabel51.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 0, true);
			this.oLabel51.Name = "oLabel51";
			this.oLabel51.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 13, true);
			this.oLabel51.TabIndex = 152;
			this.oLabel51.Text = "PM";
			// 
			// oLabel52
			// 
			this.oLabel52.AutoSize = true;
			this.oLabel52.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 0, true);
			this.oLabel52.Name = "oLabel52";
			this.oLabel52.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 13, true);
			this.oLabel52.TabIndex = 151;
			this.oLabel52.Text = "AM";
			// 
			// GridPanel
			// 
			this.GridPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("GridPanel.BackgroundImage")));
			this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 49, true);
			this.GridPanel.Name = "GridPanel";
			this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 202, true);
			this.GridPanel.TabIndex = 177;
			// 
			// OScheduleControl
			// 
			this.Controls.Add(this.oLabel27);
			this.Controls.Add(this.oLabel28);
			this.Controls.Add(this.oLabel29);
			this.Controls.Add(this.oLabel30);
			this.Controls.Add(this.oLabel31);
			this.Controls.Add(this.oLabel32);
			this.Controls.Add(this.oLabel33);
			this.Controls.Add(this.oLabel34);
			this.Controls.Add(this.oLabel35);
			this.Controls.Add(this.oLabel36);
			this.Controls.Add(this.oLabel37);
			this.Controls.Add(this.oLabel38);
			this.Controls.Add(this.oLabel39);
			this.Controls.Add(this.oLabel40);
			this.Controls.Add(this.oLabel41);
			this.Controls.Add(this.oLabel42);
			this.Controls.Add(this.oLabel43);
			this.Controls.Add(this.oLabel44);
			this.Controls.Add(this.oLabel45);
			this.Controls.Add(this.oLabel46);
			this.Controls.Add(this.oLabel47);
			this.Controls.Add(this.oLabel48);
			this.Controls.Add(this.oLabel49);
			this.Controls.Add(this.oLabel50);
			this.Controls.Add(this.oLabel51);
			this.Controls.Add(this.oLabel52);
			this.Controls.Add(this.SundayTextBox);
			this.Controls.Add(this.SaturdayTextBox);
			this.Controls.Add(this.FridayTextBox);
			this.Controls.Add(this.ThursdayTextBox);
			this.Controls.Add(this.WednesdayTextBox);
			this.Controls.Add(this.TuesdayTextBox);
			this.Controls.Add(this.MondayTextBox);
			this.Controls.Add(this.SundayLabel);
			this.Controls.Add(this.SaturdayLabel);
			this.Controls.Add(this.FridayLabel);
			this.Controls.Add(this.ThursdayLabel);
			this.Controls.Add(this.WednesdayLabel);
			this.Controls.Add(this.TuesdayLabel);
			this.Controls.Add(this.MondayLabel);
			this.Controls.Add(this.GridPanel);
			this.Name = "OScheduleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 251, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
