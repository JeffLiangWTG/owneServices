using System.ComponentModel;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class BaseACAMAWBUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.UpperPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MasterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FlightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CM_RL_NKPortOfDischargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FlightNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ArivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HouseBillsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MasterTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.houseBillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseBillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.houseBillsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HAWBTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.HouseDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZDetailsTabPage();
			this.airCagoHouseBillPartiesUserControl = new Enterprise.Customs.AU.GUI.AirCagoHouseBillPartiesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UpperPanel.SuspendLayout();
			this.MasterGroupBox.SuspendLayout();
			this.ArivalDateDateEdit.SuspendLayout();
			this.PortOfDischargeCodeFindBox.SuspendLayout();
			this.HouseBillsPanel.SuspendLayout();
			this.MasterTabControl.SuspendLayout();
			this.houseBillsTabPage.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.houseBillsSplitContainer)).BeginInit();
			this.houseBillsSplitContainer.Panel2.SuspendLayout();
			this.houseBillsSplitContainer.SuspendLayout();
			this.HAWBTabControl.SuspendLayout();
			this.HouseDetailsTabPage.SuspendLayout();
			this.airCagoHouseBillPartiesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusMAWB);
			// 
			// UpperPanel
			// 
			this.UpperPanel.Controls.Add(this.MasterGroupBox);
			this.UpperPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.UpperPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UpperPanel.Name = "UpperPanel";
			this.UpperPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 104, true);
			this.UpperPanel.TabIndex = 0;
			// 
			// MasterGroupBox
			// 
			this.MasterGroupBox.Controls.Add(this.FlightNumberTextBox);
			this.MasterGroupBox.Controls.Add(this.ArrivalDateLabel);
			this.MasterGroupBox.Controls.Add(this.CM_RL_NKPortOfDischargeLabel);
			this.MasterGroupBox.Controls.Add(this.FlightNumberLabel);
			this.MasterGroupBox.Controls.Add(this.ArivalDateDateEdit);
			this.MasterGroupBox.Controls.Add(this.PortOfDischargeCodeFindBox);
			this.MasterGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.MasterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterGroupBox.Name = "MasterGroupBox";
			this.MasterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 104, true);
			this.MasterGroupBox.TabIndex = 0;
			this.MasterGroupBox.TabStop = false;
			this.MasterGroupBox.Text = "Master Details";
			// 
			// FlightNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNumberTextBox, "CM_FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_FlightNo)));
			this.FlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 48, true);
			this.FlightNumberTextBox.Name = "FlightNumberTextBox";
			this.FlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.FlightNumberTextBox.TabIndex = 1;
			// 
			// ArrivalDateLabel
			// 
			this.ArrivalDateLabel.AutoSize = true;
			this.ArrivalDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 73, true);
			this.ArrivalDateLabel.Name = "ArrivalDateLabel";
			this.ArrivalDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.ArrivalDateLabel.TabIndex = 4;
			this.ArrivalDateLabel.Text = "ATA:";
			this.ArrivalDateLabel.UseMnemonic = false;
			// 
			// CM_RL_NKPortOfDischargeLabel
			// 
			this.CM_RL_NKPortOfDischargeLabel.AutoSize = true;
			this.CM_RL_NKPortOfDischargeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CM_RL_NKPortOfDischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 48, true);
			this.CM_RL_NKPortOfDischargeLabel.Name = "CM_RL_NKPortOfDischargeLabel";
			this.CM_RL_NKPortOfDischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.CM_RL_NKPortOfDischargeLabel.TabIndex = 2;
			this.CM_RL_NKPortOfDischargeLabel.Text = "Arrival:";
			this.CM_RL_NKPortOfDischargeLabel.UseMnemonic = false;
			// 
			// FlightNumberLabel
			// 
			this.FlightNumberLabel.AutoSize = true;
			this.FlightNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FlightNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 51, true);
			this.FlightNumberLabel.Name = "FlightNumberLabel";
			this.FlightNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.FlightNumberLabel.TabIndex = 0;
			this.FlightNumberLabel.Text = "Flight:";
			this.FlightNumberLabel.UseMnemonic = false;
			// 
			// ArivalDateDateEdit
			// 
			this.ArivalDateDateEdit.AllowDrop = true;
			this.ArivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArivalDateDateEdit, "CM_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_ArrivalDate)));
			this.ArivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 70, true);
			this.ArivalDateDateEdit.Name = "ArivalDateDateEdit";
			this.ArivalDateDateEdit.TabIndex = 5;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "CM_RL_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_RL_NKDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Lookups.PortOfDischargeList)));
			this.PortOfDischargeCodeFindBox.BindToList = "Lookups+PortOfDischargeList";
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 45, true);
			this.PortOfDischargeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.PortOfDischargeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDischargeCodeFindBox.ParentType = null;
			this.PortOfDischargeCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.PortOfDischargeCodeFindBox.TabIndex = 3;
			// 
			// HouseBillsPanel
			// 
			this.HouseBillsPanel.Controls.Add(this.MasterTabControl);
			this.HouseBillsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.HouseBillsPanel.Name = "HouseBillsPanel";
			this.HouseBillsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 613, true);
			this.HouseBillsPanel.TabIndex = 1;
			// 
			// MasterTabControl
			// 
			this.MasterTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MasterTabControl.Controls.Add(this.houseBillsTabPage);
			this.MasterTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterTabControl.Name = "MasterTabControl";
			this.MasterTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 613, true);
			this.MasterTabControl.TabIndex = 0;
			// 
			// houseBillsTabPage
			// 
			this.houseBillsTabPage.Controls.Add(this.HouseBillsGroupBox);
			this.houseBillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.houseBillsTabPage.Name = "houseBillsTabPage";
			this.houseBillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 586, true);
			this.houseBillsTabPage.TabIndex = 0;
			this.houseBillsTabPage.Text = "House Bills";
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.Controls.Add(this.houseBillsSplitContainer);
			this.HouseBillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillsGroupBox.Name = "HouseBillsGroupBox";
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 586, true);
			this.HouseBillsGroupBox.TabIndex = 0;
			this.HouseBillsGroupBox.TabStop = false;
			this.HouseBillsGroupBox.Text = "House Bills";
			// 
			// houseBillsSplitContainer
			// 
			this.houseBillsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.houseBillsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.houseBillsSplitContainer.Name = "houseBillsSplitContainer";
			this.houseBillsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// houseBillsSplitContainer.Panel2
			// 
			this.houseBillsSplitContainer.Panel2.Controls.Add(this.HAWBTabControl);
			this.houseBillsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 567, true);
			this.houseBillsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(296);
			this.houseBillsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(241);
			this.houseBillsSplitContainer.SplitterWidth = 5;
			this.houseBillsSplitContainer.TabIndex = 0;
			// 
			// HAWBTabControl
			// 
			this.HAWBTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.HAWBTabControl.Controls.Add(this.HouseDetailsTabPage);
			this.HAWBTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HAWBTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HAWBTabControl.Name = "HAWBTabControl";
			this.HAWBTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 321, true);
			this.HAWBTabControl.TabIndex = 1;
			// 
			// HouseDetailsTabPage
			// 
			this.HouseDetailsTabPage.AdditionalText = "Details";
			this.HouseDetailsTabPage.AutoScroll = true;
			this.HouseDetailsTabPage.Controls.Add(this.airCagoHouseBillPartiesUserControl);
			this.HouseDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseDetailsTabPage.Name = "HouseDetailsTabPage";
			this.HouseDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 294, true);
			this.HouseDetailsTabPage.TabIndex = 0;
			this.HouseDetailsTabPage.Text = "House Bill Details";
			// 
			// airCagoHouseBillPartiesUserControl
			// 
			this.airCagoHouseBillPartiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.airCagoHouseBillPartiesUserControl, "ChildBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)).SyncRoot)))));
			this.airCagoHouseBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 3, true);
			this.airCagoHouseBillPartiesUserControl.Name = "airCagoHouseBillPartiesUserControl";
			this.airCagoHouseBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 265, true);
			this.airCagoHouseBillPartiesUserControl.TabIndex = 0;
			// 
			// BaseACAMAWBUserControl
			// 
			this.Controls.Add(this.HouseBillsPanel);
			this.Controls.Add(this.UpperPanel);
			this.Name = "BaseACAMAWBUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 717, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UpperPanel.ResumeLayout(false);
			this.UpperPanel.PerformLayout();
			this.MasterGroupBox.ResumeLayout(false);
			this.MasterGroupBox.PerformLayout();
			this.ArivalDateDateEdit.ResumeLayout(true);
			this.ArivalDateDateEdit.PerformLayout();
			this.PortOfDischargeCodeFindBox.ResumeLayout(true);
			this.PortOfDischargeCodeFindBox.PerformLayout();
			this.HouseBillsPanel.ResumeLayout(false);
			this.HouseBillsPanel.PerformLayout();
			this.MasterTabControl.ResumeLayout(false);
			this.MasterTabControl.PerformLayout();
			this.houseBillsTabPage.ResumeLayout(false);
			this.houseBillsTabPage.PerformLayout();
			this.HouseBillsGroupBox.ResumeLayout(false);
			this.HouseBillsGroupBox.PerformLayout();
			this.houseBillsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.houseBillsSplitContainer)).EndInit();
			this.houseBillsSplitContainer.ResumeLayout(false);
			this.houseBillsSplitContainer.PerformLayout();
			this.HAWBTabControl.ResumeLayout(false);
			this.HAWBTabControl.PerformLayout();
			this.HouseDetailsTabPage.ResumeLayout(false);
			this.HouseDetailsTabPage.PerformLayout();
			this.airCagoHouseBillPartiesUserControl.ResumeLayout(true);
			this.airCagoHouseBillPartiesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public ZPanel UpperPanel;
		public ZPanel HouseBillsPanel;
		public ZGroupBox HouseBillsGroupBox;
		public ZArchitecture.ZTextBox FlightNumberTextBox;
		public ZArchitecture.ZLabel ArrivalDateLabel;
		public ZArchitecture.ZLabel CM_RL_NKPortOfDischargeLabel;
		public ZArchitecture.ZLabel FlightNumberLabel;
		public ZDateEdit ArivalDateDateEdit;
		public ZCodeFindBox PortOfDischargeCodeFindBox;
		public ZGroupBox MasterGroupBox;
		protected internal ZTemplateTabControl MasterTabControl;
		protected ZTabPage houseBillsTabPage;
		protected CargoWise.Windows.UI.KSplitContainer houseBillsSplitContainer;
		public ZTemplateTabControl HAWBTabControl;
		public ZDetailsTabPage HouseDetailsTabPage;
		protected AirCagoHouseBillPartiesUserControl airCagoHouseBillPartiesUserControl;
		private IContainer components;
	}
}
