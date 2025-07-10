using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Interop;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class DashboardForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZLabel ZLabel41;
		Enterprise.ZArchitecture.ZLabel ZLabel53;
		protected System.Windows.Forms.Timer RefreshTimer;
		Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox1;
		ZDashboardZLabel OPSLabel;
		ZDashboardZLabel zDashboardZLabel1;
		ZLabel zLabel7;
		ZLabel zLabel17;
		ZLabel zLabel8;
		ZLabel zLabel9;
		ZLabel zLabel10;
		ZDashboardZLabel zDashboardZLabel2;
		ZDashboardZLabel zDashboardZLabel3;
		ZDashboardZLabel zDashboardZLabel4;
		ZDashboardZLabel zDashboardZLabel5;
		ZDashboardZLabel zDashboardZLabel6;
		ZDashboardZLabel zDashboardZLabel7;
		ZDashboardZLabel zDashboardZLabel8;
		ZDashboardZLabel zDashboardZLabel9;
		ZDashboardZLabel zDashboardZLabel10;
		ZDashboardZLabel zDashboardZLabel11;
		ZDashboardZLabel zDashboardZLabel12;
		ZDashboardZLabel zDashboardZLabel13;
		ZDashboardZLabel zDashboardZLabel14;
		ZDashboardZLabel zDashboardZLabel15;
		ZDashboardZLabel zDashboardZLabel16;
		ZDashboardZLabel zDashboardZLabel17;
		ZDashboardZLabel zDashboardZLabel18;
		ZDashboardZLabel zDashboardZLabel19;
		ZDashboardZLabel zDashboardZLabel20;
		ZDashboardZLabel zDashboardZLabel21;
		ZDashboardZLabel zDashboardZLabel22;
		ZDashboardZLabel zDashboardZLabel23;
		ZDashboardZLabel zDashboardZLabel24;
		ZDashboardZLabel zDashboardZLabel25;
		ZDashboardZLabel zDashboardZLabel26;
		ZDashboardZLabel zDashboardZLabel27;
		ZDashboardZLabel zDashboardZLabel28;
		ZDashboardZLabel zDashboardZLabel29;
		ZDashboardZLabel zDashboardZLabel30;
		ZDashboardZLabel zDashboardZLabel31;
		ZDashboardZLabel zDashboardZLabel32;
		ZDashboardZLabel zDashboardZLabel33;
		ZDashboardZLabel zDashboardZLabel34;
		ZDashboardZLabel zDashboardZLabel35;
		ZDashboardZLabel zDashboardZLabel36;
		ZDashboardZLabel zDashboardZLabel37;
		ZDashboardZLabel zDashboardZLabel38;
		ZDashboardZLabel zDashboardZLabel39;
		ZDashboardZLabel zDashboardZLabel40;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardForm));
			this.ZLabel41 = new Enterprise.ZArchitecture.ZLabel();
			this.ZLabel53 = new Enterprise.ZArchitecture.ZLabel();
			this.RefreshTimer = NewTimer();
			this.pictureBox1 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zDashboardZLabel40 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel39 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel38 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel37 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel36 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel35 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel34 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel33 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel32 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel31 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel30 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel29 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel28 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel27 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel26 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel25 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel24 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel23 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel22 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel21 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel20 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel19 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel18 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel17 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel16 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel15 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel14 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel13 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel12 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel11 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel10 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel9 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel8 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel7 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel6 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel5 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel4 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel3 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel2 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.zDashboardZLabel1 = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			this.OPSLabel = new Enterprise.Client.UPE.GUI.ZDashboardZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 695, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 23, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(992);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.Dashboard);
			// 
			// ZLabel41
			// 
			this.ZLabel41.BackColor = System.Drawing.Color.Black;
			this.ZLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 639, true);
			this.ZLabel41.Name = "ZLabel41";
			this.ZLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 3, true);
			this.ZLabel41.TabIndex = 61;
			// 
			// ZLabel53
			// 
			this.ZLabel53.BackColor = System.Drawing.Color.Black;
			this.ZLabel53.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 71, true);
			this.ZLabel53.Name = "ZLabel53";
			this.ZLabel53.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 568, true);
			this.ZLabel53.TabIndex = 63;
			// 
			// RefreshTimer
			// 
			this.RefreshTimer.Interval = 300000;
			this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 19, true);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 94, true);
			this.pictureBox1.TabIndex = 65;
			this.pictureBox1.TabStop = false;
			// 
			// zLabel7
			// 
			this.zLabel7.BackColor = System.Drawing.Color.Black;
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 72, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 569, true);
			this.zLabel7.TabIndex = 69;
			// 
			// zLabel17
			// 
			this.zLabel17.BackColor = System.Drawing.Color.Black;
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(992, 71, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 569, true);
			this.zLabel17.TabIndex = 73;
			// 
			// zLabel8
			// 
			this.zLabel8.BackColor = System.Drawing.Color.Black;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 71, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 3, true);
			this.zLabel8.TabIndex = 74;
			// 
			// zLabel9
			// 
			this.zLabel9.BackColor = System.Drawing.Color.Black;
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 126, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 3, true);
			this.zLabel9.TabIndex = 75;
			// 
			// zLabel10
			// 
			this.zLabel10.BackColor = System.Drawing.Color.Black;
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 233, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 3, true);
			this.zLabel10.TabIndex = 76;
			// 
			// zDashboardZLabel40
			// 
			this.zDashboardZLabel40.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel40, "OpsBrokersFutureArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsBrokersFutureArrivalPieces)));
			this.zDashboardZLabel40.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel40.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 565, true);
			this.zDashboardZLabel40.Name = "zDashboardZLabel40";
			this.zDashboardZLabel40.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel40.TabIndex = 115;
			this.zDashboardZLabel40.Text = "0";
			// 
			// zDashboardZLabel39
			// 
			this.zDashboardZLabel39.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel39, "OpsBrokersFutureArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsBrokersFutureArrivalShipments)));
			this.zDashboardZLabel39.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 513, true);
			this.zDashboardZLabel39.Name = "zDashboardZLabel39";
			this.zDashboardZLabel39.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel39.TabIndex = 114;
			this.zDashboardZLabel39.Text = "0";
			// 
			// zDashboardZLabel38
			// 
			this.zDashboardZLabel38.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel38, "OpsBrokersTodayArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsBrokersTodayArrivalPieces)));
			this.zDashboardZLabel38.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 431, true);
			this.zDashboardZLabel38.Name = "zDashboardZLabel38";
			this.zDashboardZLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel38.TabIndex = 113;
			this.zDashboardZLabel38.Text = "0";
			// 
			// zDashboardZLabel37
			// 
			this.zDashboardZLabel37.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel37, "OpsBrokersTodayArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsBrokersTodayArrivalShipments)));
			this.zDashboardZLabel37.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 377, true);
			this.zDashboardZLabel37.Name = "zDashboardZLabel37";
			this.zDashboardZLabel37.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel37.TabIndex = 112;
			this.zDashboardZLabel37.Text = "0";
			// 
			// zDashboardZLabel36
			// 
			this.zDashboardZLabel36.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel36, "OpsBrokersPastArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsBrokersPastArrivalPieces)));
			this.zDashboardZLabel36.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 288, true);
			this.zDashboardZLabel36.Name = "zDashboardZLabel36";
			this.zDashboardZLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel36.TabIndex = 111;
			this.zDashboardZLabel36.Text = "0";
			// 
			// zDashboardZLabel35
			// 
			this.zDashboardZLabel35.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel35, "OpsBrokersPastArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsBrokersPastArrivalShipments)));
			this.zDashboardZLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 236, true);
			this.zDashboardZLabel35.Name = "zDashboardZLabel35";
			this.zDashboardZLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel35.TabIndex = 110;
			this.zDashboardZLabel35.Text = "0";
			// 
			// zDashboardZLabel34
			// 
			this.zDashboardZLabel34.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel34, "OpsClassifiersFutureArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsClassifiersFutureArrivalPieces)));
			this.zDashboardZLabel34.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 565, true);
			this.zDashboardZLabel34.Name = "zDashboardZLabel34";
			this.zDashboardZLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel34.TabIndex = 109;
			this.zDashboardZLabel34.Text = "0";
			// 
			// zDashboardZLabel33
			// 
			this.zDashboardZLabel33.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel33, "OpsClassifiersFutureArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsClassifiersFutureArrivalShipments)));
			this.zDashboardZLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 513, true);
			this.zDashboardZLabel33.Name = "zDashboardZLabel33";
			this.zDashboardZLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel33.TabIndex = 108;
			this.zDashboardZLabel33.Text = "0";
			// 
			// zDashboardZLabel32
			// 
			this.zDashboardZLabel32.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel32, "OpsClassifiersTodayArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsClassifiersTodayArrivalPieces)));
			this.zDashboardZLabel32.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 431, true);
			this.zDashboardZLabel32.Name = "zDashboardZLabel32";
			this.zDashboardZLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel32.TabIndex = 107;
			this.zDashboardZLabel32.Text = "0";
			// 
			// zDashboardZLabel31
			// 
			this.zDashboardZLabel31.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel31, "OpsClassifiersTodayArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsClassifiersTodayArrivalShipments)));
			this.zDashboardZLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 377, true);
			this.zDashboardZLabel31.Name = "zDashboardZLabel31";
			this.zDashboardZLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel31.TabIndex = 106;
			this.zDashboardZLabel31.Text = "0";
			// 
			// zDashboardZLabel30
			// 
			this.zDashboardZLabel30.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel30, "OpsClassifiersPastArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsClassifiersPastArrivalPieces)));
			this.zDashboardZLabel30.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 288, true);
			this.zDashboardZLabel30.Name = "zDashboardZLabel30";
			this.zDashboardZLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel30.TabIndex = 105;
			this.zDashboardZLabel30.Text = "0";
			// 
			// zDashboardZLabel29
			// 
			this.zDashboardZLabel29.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel29, "OpsClassifiersPastArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).OpsClassifiersPastArrivalShipments)));
			this.zDashboardZLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 236, true);
			this.zDashboardZLabel29.Name = "zDashboardZLabel29";
			this.zDashboardZLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel29.TabIndex = 104;
			this.zDashboardZLabel29.Text = "0";
			// 
			// zDashboardZLabel28
			// 
			this.zDashboardZLabel28.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel28, "AdminHouseFutureArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHouseFutureArrivalPieces)));
			this.zDashboardZLabel28.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 565, true);
			this.zDashboardZLabel28.Name = "zDashboardZLabel28";
			this.zDashboardZLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel28.TabIndex = 103;
			this.zDashboardZLabel28.Text = "0";
			// 
			// zDashboardZLabel27
			// 
			this.zDashboardZLabel27.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel27, "AdminHouseFutureArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHouseFutureArrivalShipments)));
			this.zDashboardZLabel27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 513, true);
			this.zDashboardZLabel27.Name = "zDashboardZLabel27";
			this.zDashboardZLabel27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel27.TabIndex = 102;
			this.zDashboardZLabel27.Text = "0";
			// 
			// zDashboardZLabel26
			// 
			this.zDashboardZLabel26.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel26, "AdminHouseTodayArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHouseTodayArrivalPieces)));
			this.zDashboardZLabel26.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel26.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 431, true);
			this.zDashboardZLabel26.Name = "zDashboardZLabel26";
			this.zDashboardZLabel26.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel26.TabIndex = 101;
			this.zDashboardZLabel26.Text = "0";
			// 
			// zDashboardZLabel25
			// 
			this.zDashboardZLabel25.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel25, "AdminHouseTodayArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHouseTodayArrivalShipments)));
			this.zDashboardZLabel25.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 377, true);
			this.zDashboardZLabel25.Name = "zDashboardZLabel25";
			this.zDashboardZLabel25.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel25.TabIndex = 100;
			this.zDashboardZLabel25.Text = "0";
			// 
			// zDashboardZLabel24
			// 
			this.zDashboardZLabel24.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel24, "AdminHousePastArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHousePastArrivalPieces)));
			this.zDashboardZLabel24.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel24.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 288, true);
			this.zDashboardZLabel24.Name = "zDashboardZLabel24";
			this.zDashboardZLabel24.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel24.TabIndex = 99;
			this.zDashboardZLabel24.Text = "0";
			// 
			// zDashboardZLabel23
			// 
			this.zDashboardZLabel23.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel23, "AdminHousePastArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHousePastArrivalShipments)));
			this.zDashboardZLabel23.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 236, true);
			this.zDashboardZLabel23.Name = "zDashboardZLabel23";
			this.zDashboardZLabel23.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel23.TabIndex = 98;
			this.zDashboardZLabel23.Text = "0";
			// 
			// zDashboardZLabel22
			// 
			this.zDashboardZLabel22.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel22, "AdminHeldFutureArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHeldFutureArrivalPieces)));
			this.zDashboardZLabel22.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 565, true);
			this.zDashboardZLabel22.Name = "zDashboardZLabel22";
			this.zDashboardZLabel22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel22.TabIndex = 97;
			this.zDashboardZLabel22.Text = "0";
			// 
			// zDashboardZLabel21
			// 
			this.zDashboardZLabel21.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel21, "AdminHeldFutureArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHeldFutureArrivalShipments)));
			this.zDashboardZLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 513, true);
			this.zDashboardZLabel21.Name = "zDashboardZLabel21";
			this.zDashboardZLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel21.TabIndex = 96;
			this.zDashboardZLabel21.Text = "0";
			// 
			// zDashboardZLabel20
			// 
			this.zDashboardZLabel20.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel20, "AdminHeldTodayArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHeldTodayArrivalPieces)));
			this.zDashboardZLabel20.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 431, true);
			this.zDashboardZLabel20.Name = "zDashboardZLabel20";
			this.zDashboardZLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel20.TabIndex = 95;
			this.zDashboardZLabel20.Text = "0";
			// 
			// zDashboardZLabel19
			// 
			this.zDashboardZLabel19.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel19, "AdminHeldTodayArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHeldTodayArrivalShipments)));
			this.zDashboardZLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 377, true);
			this.zDashboardZLabel19.Name = "zDashboardZLabel19";
			this.zDashboardZLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel19.TabIndex = 94;
			this.zDashboardZLabel19.Text = "0";
			// 
			// zDashboardZLabel18
			// 
			this.zDashboardZLabel18.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel18, "AdminHeldPastArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHeldPastArrivalPieces)));
			this.zDashboardZLabel18.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 288, true);
			this.zDashboardZLabel18.Name = "zDashboardZLabel18";
			this.zDashboardZLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel18.TabIndex = 93;
			this.zDashboardZLabel18.Text = "0";
			// 
			// zDashboardZLabel17
			// 
			this.zDashboardZLabel17.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel17, "AdminHeldPastArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminHeldPastArrivalShipments)));
			this.zDashboardZLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 236, true);
			this.zDashboardZLabel17.Name = "zDashboardZLabel17";
			this.zDashboardZLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel17.TabIndex = 92;
			this.zDashboardZLabel17.Text = "0";
			// 
			// zDashboardZLabel16
			// 
			this.zDashboardZLabel16.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel16, "AdminNoStatusFutureArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminNoStatusFutureArrivalPieces)));
			this.zDashboardZLabel16.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 565, true);
			this.zDashboardZLabel16.Name = "zDashboardZLabel16";
			this.zDashboardZLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel16.TabIndex = 91;
			this.zDashboardZLabel16.Text = "0";
			// 
			// zDashboardZLabel15
			// 
			this.zDashboardZLabel15.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel15, "AdminNoStatusFutureArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminNoStatusFutureArrivalShipments)));
			this.zDashboardZLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 513, true);
			this.zDashboardZLabel15.Name = "zDashboardZLabel15";
			this.zDashboardZLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel15.TabIndex = 90;
			this.zDashboardZLabel15.Text = "0";
			// 
			// zDashboardZLabel14
			// 
			this.zDashboardZLabel14.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel14, "AdminNoStatusTodayArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminNoStatusTodayArrivalPieces)));
			this.zDashboardZLabel14.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 431, true);
			this.zDashboardZLabel14.Name = "zDashboardZLabel14";
			this.zDashboardZLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel14.TabIndex = 89;
			this.zDashboardZLabel14.Text = "0";
			// 
			// zDashboardZLabel13
			// 
			this.zDashboardZLabel13.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel13, "AdminNoStatusTodayArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminNoStatusTodayArrivalShipments)));
			this.zDashboardZLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 377, true);
			this.zDashboardZLabel13.Name = "zDashboardZLabel13";
			this.zDashboardZLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel13.TabIndex = 88;
			this.zDashboardZLabel13.Text = "0";
			// 
			// zDashboardZLabel12
			// 
			this.zDashboardZLabel12.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel12, "AdminNoStatusPastArrivalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminNoStatusPastArrivalPieces)));
			this.zDashboardZLabel12.ForeColor = System.Drawing.Color.RoyalBlue;
			this.zDashboardZLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 288, true);
			this.zDashboardZLabel12.Name = "zDashboardZLabel12";
			this.zDashboardZLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel12.TabIndex = 87;
			this.zDashboardZLabel12.Text = "0";
			// 
			// zDashboardZLabel11
			// 
			this.zDashboardZLabel11.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zDashboardZLabel11, "AdminNoStatusPastArrivalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Dashboard)(null)).AdminNoStatusPastArrivalShipments)));
			this.zDashboardZLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 236, true);
			this.zDashboardZLabel11.Name = "zDashboardZLabel11";
			this.zDashboardZLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 44, true);
			this.zDashboardZLabel11.TabIndex = 86;
			this.zDashboardZLabel11.Text = "0";
			// 
			// zDashboardZLabel10
			// 
			this.zDashboardZLabel10.AutoSize = true;
			this.zDashboardZLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(825, 178, true);
			this.zDashboardZLabel10.Name = "zDashboardZLabel10";
			this.zDashboardZLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 44, true);
			this.zDashboardZLabel10.TabIndex = 85;
			this.zDashboardZLabel10.Text = "Brokers";
			// 
			// zDashboardZLabel9
			// 
			this.zDashboardZLabel9.AutoSize = true;
			this.zDashboardZLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 178, true);
			this.zDashboardZLabel9.Name = "zDashboardZLabel9";
			this.zDashboardZLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 44, true);
			this.zDashboardZLabel9.TabIndex = 84;
			this.zDashboardZLabel9.Text = "Classifiers";
			// 
			// zDashboardZLabel8
			// 
			this.zDashboardZLabel8.AutoSize = true;
			this.zDashboardZLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 178, true);
			this.zDashboardZLabel8.Name = "zDashboardZLabel8";
			this.zDashboardZLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 44, true);
			this.zDashboardZLabel8.TabIndex = 83;
			this.zDashboardZLabel8.Text = "House";
			// 
			// zDashboardZLabel7
			// 
			this.zDashboardZLabel7.AutoSize = true;
			this.zDashboardZLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 178, true);
			this.zDashboardZLabel7.Name = "zDashboardZLabel7";
			this.zDashboardZLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 44, true);
			this.zDashboardZLabel7.TabIndex = 82;
			this.zDashboardZLabel7.Text = "Held";
			// 
			// zDashboardZLabel6
			// 
			this.zDashboardZLabel6.AutoSize = true;
			this.zDashboardZLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 137, true);
			this.zDashboardZLabel6.Name = "zDashboardZLabel6";
			this.zDashboardZLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 88, true);
			this.zDashboardZLabel6.TabIndex = 81;
			this.zDashboardZLabel6.Text = "No\r\nStatus";
			// 
			// zDashboardZLabel5
			// 
			this.zDashboardZLabel5.AutoSize = true;
			this.zDashboardZLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 525, true);
			this.zDashboardZLabel5.Name = "zDashboardZLabel5";
			this.zDashboardZLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 88, true);
			this.zDashboardZLabel5.TabIndex = 80;
			this.zDashboardZLabel5.Text = "Future\r\nArrivals";
			// 
			// zDashboardZLabel4
			// 
			this.zDashboardZLabel4.AutoSize = true;
			this.zDashboardZLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 390, true);
			this.zDashboardZLabel4.Name = "zDashboardZLabel4";
			this.zDashboardZLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 88, true);
			this.zDashboardZLabel4.TabIndex = 79;
			this.zDashboardZLabel4.Text = "Today\'s\r\nArrivals";
			// 
			// zDashboardZLabel3
			// 
			this.zDashboardZLabel3.AutoSize = true;
			this.zDashboardZLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 247, true);
			this.zDashboardZLabel3.Name = "zDashboardZLabel3";
			this.zDashboardZLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 88, true);
			this.zDashboardZLabel3.TabIndex = 78;
			this.zDashboardZLabel3.Text = "Past\r\nArrivals";
			// 
			// zDashboardZLabel2
			// 
			this.zDashboardZLabel2.AutoSize = true;
			this.zDashboardZLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 74, true);
			this.zDashboardZLabel2.Name = "zDashboardZLabel2";
			this.zDashboardZLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 44, true);
			this.zDashboardZLabel2.TabIndex = 77;
			this.zDashboardZLabel2.Text = "ADMIN";
			// 
			// zDashboardZLabel1
			// 
			this.zDashboardZLabel1.AutoSize = true;
			this.zDashboardZLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 19, true);
			this.zDashboardZLabel1.Name = "zDashboardZLabel1";
			this.zDashboardZLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 44, true);
			this.zDashboardZLabel1.TabIndex = 68;
			this.zDashboardZLabel1.Text = "BORDER CLEARANCE";
			// 
			// OPSLabel
			// 
			this.OPSLabel.AutoSize = true;
			this.OPSLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(738, 74, true);
			this.OPSLabel.Name = "OPSLabel";
			this.OPSLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 44, true);
			this.OPSLabel.TabIndex = 67;
			this.OPSLabel.Text = "OPS";
			// 
			// DashboardForm
			// 

			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 718, true);
			this.Controls.Add(this.zLabel17);
			this.Controls.Add(this.zLabel10);
			this.Controls.Add(this.zLabel9);
			this.Controls.Add(this.zLabel8);
			this.Controls.Add(this.zLabel7);
			this.Controls.Add(this.ZLabel53);
			this.Controls.Add(this.ZLabel41);
			this.Controls.Add(this.zDashboardZLabel40);
			this.Controls.Add(this.zDashboardZLabel39);
			this.Controls.Add(this.zDashboardZLabel38);
			this.Controls.Add(this.zDashboardZLabel37);
			this.Controls.Add(this.zDashboardZLabel36);
			this.Controls.Add(this.zDashboardZLabel35);
			this.Controls.Add(this.zDashboardZLabel34);
			this.Controls.Add(this.zDashboardZLabel33);
			this.Controls.Add(this.zDashboardZLabel32);
			this.Controls.Add(this.zDashboardZLabel31);
			this.Controls.Add(this.zDashboardZLabel30);
			this.Controls.Add(this.zDashboardZLabel29);
			this.Controls.Add(this.zDashboardZLabel28);
			this.Controls.Add(this.zDashboardZLabel27);
			this.Controls.Add(this.zDashboardZLabel26);
			this.Controls.Add(this.zDashboardZLabel25);
			this.Controls.Add(this.zDashboardZLabel24);
			this.Controls.Add(this.zDashboardZLabel23);
			this.Controls.Add(this.zDashboardZLabel22);
			this.Controls.Add(this.zDashboardZLabel21);
			this.Controls.Add(this.zDashboardZLabel20);
			this.Controls.Add(this.zDashboardZLabel19);
			this.Controls.Add(this.zDashboardZLabel18);
			this.Controls.Add(this.zDashboardZLabel17);
			this.Controls.Add(this.zDashboardZLabel16);
			this.Controls.Add(this.zDashboardZLabel15);
			this.Controls.Add(this.zDashboardZLabel14);
			this.Controls.Add(this.zDashboardZLabel13);
			this.Controls.Add(this.zDashboardZLabel12);
			this.Controls.Add(this.zDashboardZLabel11);
			this.Controls.Add(this.zDashboardZLabel10);
			this.Controls.Add(this.zDashboardZLabel9);
			this.Controls.Add(this.zDashboardZLabel8);
			this.Controls.Add(this.zDashboardZLabel7);
			this.Controls.Add(this.zDashboardZLabel6);
			this.Controls.Add(this.zDashboardZLabel5);
			this.Controls.Add(this.zDashboardZLabel4);
			this.Controls.Add(this.zDashboardZLabel3);
			this.Controls.Add(this.zDashboardZLabel2);
			this.Controls.Add(this.zDashboardZLabel1);
			this.Controls.Add(this.OPSLabel);
			this.Controls.Add(this.pictureBox1);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.Dashboard);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.Dashboard";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "DashboardForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.Text = "Dashboard";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.pictureBox1, 0);
			this.Controls.SetChildIndex(this.OPSLabel, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel1, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel2, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel3, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel4, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel5, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel6, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel7, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel8, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel9, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel10, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel11, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel12, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel13, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel14, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel15, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel16, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel17, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel18, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel19, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel20, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel21, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel22, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel23, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel24, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel25, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel26, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel27, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel28, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel29, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel30, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel31, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel32, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel33, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel34, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel35, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel36, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel37, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel38, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel39, 0);
			this.Controls.SetChildIndex(this.zDashboardZLabel40, 0);
			this.Controls.SetChildIndex(this.ZLabel41, 0);
			this.Controls.SetChildIndex(this.ZLabel53, 0);
			this.Controls.SetChildIndex(this.zLabel7, 0);
			this.Controls.SetChildIndex(this.zLabel8, 0);
			this.Controls.SetChildIndex(this.zLabel9, 0);
			this.Controls.SetChildIndex(this.zLabel10, 0);
			this.Controls.SetChildIndex(this.zLabel17, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
