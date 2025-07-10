using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class ExitSummaryUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NewTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HeaderCarrierOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.HeaderTransportIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderExitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.HeaderArrivalNotificationPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderArrivalNotificationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.HeaderCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.MovementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MovementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MovementDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CarrierOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MovementReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalNotificationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalNotificationPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExitSummaryMainPanelUserControl = new ExitSummaryMainPanelUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.NewTopPanel.SuspendLayout();
			this.HeaderCarrierOrgAddressControl.SuspendLayout();
			this.HeaderExitDateDateEdit.SuspendLayout();
			this.HeaderArrivalNotificationDateDateEdit.SuspendLayout();
			this.HeaderCustomsOfficeCodeFindBox.SuspendLayout();
			this.AgentOrgAddressControl.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.PackingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).BeginInit();
			this.PackingGrid.SuspendLayout();
			this.MovementGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).BeginInit();
			this.MovementsGrid.SuspendLayout();
			this.MovementDetailsPanel.SuspendLayout();
			this.CarrierOrgAddressControl.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.ArrivalNotificationDateDateEdit.SuspendLayout();
			this.ExitDateDateEdit.SuspendLayout();
			this.MessagesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusExitControlHeader);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.HeaderCarrierOrgAddressControl);
			this.TopPanel.Controls.Add(this.HeaderTransportIdTextBox);
			this.TopPanel.Controls.Add(this.HeaderExitDateDateEdit);
			this.TopPanel.Controls.Add(this.HeaderArrivalNotificationPlaceTextBox);
			this.TopPanel.Controls.Add(this.HeaderArrivalNotificationDateDateEdit);
			this.TopPanel.Controls.Add(this.HeaderCustomsOfficeCodeFindBox);
			this.TopPanel.Controls.Add(this.ReferenceNumberTextBox);
			this.TopPanel.Controls.Add(this.AgentOrgAddressControl);
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 171, true);
			this.TopPanel.TabIndex = 2;
			// 
			// NewTopPanel
			// 
			this.NewTopPanel.Controls.Add(this.ExitSummaryMainPanelUserControl);
			this.NewTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NewTopPanel.Name = "NewTopPanel";
			this.NewTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 171, true);
			this.NewTopPanel.TabIndex = 2;
			//
			// ExitSummaryMainPanelUserControl
			//
			this.ExitSummaryMainPanelUserControl.Dock = DockStyle.Fill;
			this.ExitSummaryMainPanelUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExitSummaryMainPanelUserControl.Name = "InvoiceLineDetailsUserControl";
			this.ExitSummaryMainPanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 171, true);
			this.ExitSummaryMainPanelUserControl.TabIndex = 0;
			this.BindingSource.SetBindingMember(this.ExitSummaryMainPanelUserControl, ".");
			// 
			// HeaderCarrierOrgAddressControl
			// 
			this.HeaderCarrierOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderCarrierOrgAddressControl, "CEH_OA_Carrier_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_OA_Carrier_ZAddress)));
			this.HeaderCarrierOrgAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("6e264cdc-590c-47fe-aade-8437501efa51", "Carrier");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HeaderCarrierOrgAddressControl, false);
			this.HeaderCarrierOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(759, 13, true);
			this.HeaderCarrierOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.HeaderCarrierOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.HeaderCarrierOrgAddressControl.Name = "HeaderCarrierOrgAddressControl";
			this.HeaderCarrierOrgAddressControl.OnlyStopOnDebtor = false;
			this.HeaderCarrierOrgAddressControl.PopupCaption = "";
			this.HeaderCarrierOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.HeaderCarrierOrgAddressControl.TabIndex = 9;
			// 
			// HeaderTransportIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.HeaderTransportIdTextBox, "CEH_TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_TransportID)));
			this.HeaderTransportIdTextBox.CaptionResourceString = null;
			this.HeaderTransportIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 143, true);
			this.HeaderTransportIdTextBox.Name = "HeaderTransportIdTextBox";
			this.HeaderTransportIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.HeaderTransportIdTextBox.TabIndex = 8;
			// 
			// HeaderExitDateDateEdit
			// 
			this.HeaderExitDateDateEdit.AllowDrop = true;
			this.HeaderExitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.HeaderExitDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.HeaderExitDateDateEdit, "CEH_ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ExitDate)));
			this.HeaderExitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 117, true);
			this.HeaderExitDateDateEdit.Name = "HeaderExitDateDateEdit";
			this.HeaderExitDateDateEdit.TabIndex = 7;
			// 
			// HeaderArrivalNotificationPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.HeaderArrivalNotificationPlaceTextBox, "CEH_ArrivalNotificationPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ArrivalNotificationPlace)));
			this.HeaderArrivalNotificationPlaceTextBox.CaptionResourceString = null;
			this.HeaderArrivalNotificationPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 91, true);
			this.HeaderArrivalNotificationPlaceTextBox.Name = "HeaderArrivalNotificationPlaceTextBox";
			this.HeaderArrivalNotificationPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.HeaderArrivalNotificationPlaceTextBox.TabIndex = 6;
			// 
			// HeaderArrivalNotificationDateDateEdit
			// 
			this.HeaderArrivalNotificationDateDateEdit.AllowDrop = true;
			this.HeaderArrivalNotificationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.HeaderArrivalNotificationDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.HeaderArrivalNotificationDateDateEdit, "CEH_ArrivalNotificationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ArrivalNotificationDate)));
			this.HeaderArrivalNotificationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 65, true);
			this.HeaderArrivalNotificationDateDateEdit.Name = "HeaderArrivalNotificationDateDateEdit";
			this.HeaderArrivalNotificationDateDateEdit.TabIndex = 5;
			// 
			// HeaderCustomsOfficeCodeFindBox
			// 
			this.HeaderCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderCustomsOfficeCodeFindBox, "CEH_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_CustomsOffice)));
			this.HeaderCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 39, true);
			this.HeaderCustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.HeaderCustomsOfficeCodeFindBox.Name = "HeaderCustomsOfficeCodeFindBox";
			this.HeaderCustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.HeaderCustomsOfficeCodeFindBox.ParentType = null;
			this.HeaderCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.HeaderCustomsOfficeCodeFindBox.TabIndex = 4;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CEH_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ReferenceNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("93fbea06-3e63-4bec-a3bf-406e8cf8eae5", "Reference Number");
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 13, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 0;
			// 
			// AgentOrgAddressControl
			// 
			this.AgentOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgentOrgAddressControl, "CEH_OA_Agent_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_OA_Agent_ZAddress)));
			this.AgentOrgAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("957ef1c2-b860-4fcd-b7f2-52feb55c22e5", "Agent");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AgentOrgAddressControl, false);
			this.AgentOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 13, true);
			this.AgentOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AgentOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AgentOrgAddressControl.Name = "AgentOrgAddressControl";
			this.AgentOrgAddressControl.OnlyStopOnDebtor = false;
			this.AgentOrgAddressControl.PopupCaption = "";
			this.AgentOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AgentOrgAddressControl.TabIndex = 1;
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("D4DF0E6A-B602-4C68-B008-6EDF434F7C1C", "Items");
			this.ItemsGroupBox.Controls.Add(this.ItemsGrid);
			this.ItemsGroupBox.Controls.Add(this.PackingGroupBox);
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(756, 6, true);
			this.ItemsGroupBox.Name = "ItemsGroupBox";
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 285, true);
			this.ItemsGroupBox.TabIndex = 11;
			this.ItemsGroupBox.TabStop = false;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "CusExitDetails.CusExitItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).CXI_LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).CXI_NetMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).CXI_NetMassUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).CXI_GrossMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).CXI_GrossMassUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).CXI_Status)));
			this.ItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CXI_LineNumber";
			zCalcEditColumnStyleInfo1.IsCustomColumn = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CXI_NetMass";
			zCalcEditColumnStyleInfo2.IsCustomColumn = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "CXI_NetMassUQ";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CXI_GrossMass";
			zCalcEditColumnStyleInfo3.IsCustomColumn = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ColumnName = "CXI_GrossMassUQ";
			zDropEditColumnStyleInfo2.IsCustomColumn = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "CXI_Status";
			zDropEditColumnStyleInfo3.IsCustomColumn = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ItemsGrid.GridId = "64b07c4a-5569-4d2e-bd09-0f7a31e7a26c";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "ItemsGrid";
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 124, true);
			this.ItemsGrid.TabIndex = 0;
			// 
			// PackingGroupBox
			// 
			this.PackingGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9DB94698-D18B-4C67-B148-A60CC1014134", "Packing Details");
			this.PackingGroupBox.Controls.Add(this.PackingGrid);
			this.PackingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 148, true);
			this.PackingGroupBox.Name = "PackingGroupBox";
			this.PackingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 131, true);
			this.PackingGroupBox.TabIndex = 0;
			this.PackingGroupBox.TabStop = false;
			// 
			// PackingGrid
			// 
			this.PackingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackingGrid, "CusExitDetails.CusExitItems.Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.EU.Business.CusExitItemPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).Packages)).SyncRoot)).B5_UnitCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitItemPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).Packages)).SyncRoot)).B5_UnitType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitItemPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CusExitItems)).SyncRoot)).Packages)).SyncRoot)).B5_MarksAndNumbers)));
			this.PackingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "B5_UnitCount";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "B5_UnitType";
			zDropEditColumnStyleInfo4.IsCustomColumn = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "B5_MarksAndNumbers";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(310);
			this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackingGrid.GridId = "7600cee4-e568-40ed-9cb2-fd40a5d45430";
			this.PackingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingGrid.LayoutKey = "PackingGrid";
			this.PackingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.PackingGrid.Name = "PackingGrid";
			this.PackingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 106, true);
			this.PackingGrid.TabIndex = 1;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1276, 219, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// MovementGroupBox
			// 
			this.MovementGroupBox.Controls.Add(this.MovementsGrid);
			this.MovementGroupBox.Controls.Add(this.MovementDetailsPanel);
			this.MovementGroupBox.Controls.Add(this.MessagesGroupBox);
			this.MovementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 171, true);
			this.MovementGroupBox.Name = "MovementGroupBox";
			this.MovementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1294, 653, true);
			this.MovementGroupBox.TabIndex = 0;
			this.MovementGroupBox.TabStop = false;
			this.MovementGroupBox.Text = "Movements";
			// 
			// MovementsGrid
			// 
			this.MovementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MovementsGrid, "CusExitDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_MovementReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_CustomsOffice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_ArrivalNotificationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_ArrivalNotificationPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_ExitDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_TransportID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_Status)));
			this.MovementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("cefd6001-b159-4e6e-a92b-b4f38986bedb", "MRN");
			zTextBoxColumnStyleInfo3.ColumnName = "CED_MovementReferenceNumber";
			zTextBoxColumnStyleInfo3.IsCustomColumn = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(229);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("cf2c8421-4edc-4723-b1dc-fba310794286", "Exit Customs Office");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CED_CustomsOffice";
			zCodeFindBoxColumnStyleInfo1.IsCustomColumn = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("d0f0f1a3-eefc-4ab0-8acb-af095c6048a0", "Arrival Notification Date");
			zDateEditColumnStyleInfo1.ColumnName = "CED_ArrivalNotificationDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsCustomColumn = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7bcf3401-48b1-40a9-9cdf-2ecac4fcc38d", "Arrival Notification Place");
			zTextBoxColumnStyleInfo4.ColumnName = "CED_ArrivalNotificationPlace";
			zTextBoxColumnStyleInfo4.IsCustomColumn = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("085a67fa-6ac0-4e68-b72f-cd0bf93e05c3", "Exit Date");
			zDateEditColumnStyleInfo2.ColumnName = "CED_ExitDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsCustomColumn = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("14aacd01-6f07-42e9-8cd3-b0de5d44461c", "Transport ID");
			zTextBoxColumnStyleInfo5.ColumnName = "CED_TransportID";
			zTextBoxColumnStyleInfo5.IsCustomColumn = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(324);
			zDropEditColumnStyleInfo5.Caption = "Status";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("2083ade3-1282-4d28-8a69-0d8d0f26ac5a", "Status");
			zDropEditColumnStyleInfo5.ColumnName = "CED_Status";
			zDropEditColumnStyleInfo5.IsCustomColumn = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			this.MovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MovementsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MovementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MovementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MovementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.MovementsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.MovementsGrid.GridId = "de04df47-f9d8-4d22-8c06-d709a3c16520";
			this.MovementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MovementsGrid.LayoutKey = "MovementsGrid";
			this.MovementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MovementsGrid.Name = "MovementsGrid";
			this.MovementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1288, 92, true);
			this.MovementsGrid.TabIndex = 1;
			// 
			// MovementDetailsPanel
			// 
			this.MovementDetailsPanel.Controls.Add(this.CarrierOrgAddressControl);
			this.MovementDetailsPanel.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.MovementDetailsPanel.Controls.Add(this.StatusDropEdit);
			this.MovementDetailsPanel.Controls.Add(this.MovementReferenceNumberTextBox);
			this.MovementDetailsPanel.Controls.Add(this.TransportIdTextBox);
			this.MovementDetailsPanel.Controls.Add(this.ArrivalNotificationDateDateEdit);
			this.MovementDetailsPanel.Controls.Add(this.ExitDateDateEdit);
			this.MovementDetailsPanel.Controls.Add(this.ArrivalNotificationPlaceTextBox);
			this.MovementDetailsPanel.Controls.Add(this.ItemsGroupBox);
			this.MovementDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 108, true);
			this.MovementDetailsPanel.Name = "MovementDetailsPanel";
			this.MovementDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1288, 300, true);
			this.MovementDetailsPanel.TabIndex = 2;
			// 
			// CarrierOrgAddressControl
			// 
			this.CarrierOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierOrgAddressControl, "CusExitDetails.CED_OA_Carrier_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_OA_Carrier_ZAddress)));
			this.CarrierOrgAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("b723b39f-c43d-421f-8358-65f857ad8801", "Carrier");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CarrierOrgAddressControl, false);
			this.CarrierOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 6, true);
			this.CarrierOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.CarrierOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.CarrierOrgAddressControl.Name = "CarrierOrgAddressControl";
			this.CarrierOrgAddressControl.OnlyStopOnDebtor = false;
			this.CarrierOrgAddressControl.PopupCaption = "";
			this.CarrierOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.CarrierOrgAddressControl.TabIndex = 9;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CusExitDetails.CED_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1d7acd43-13d8-4404-ad63-beb0fd4611ec", "Exit Customs Office");
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 32, true);
			this.CustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeCodeFindBox.ParentType = null;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 3;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "CusExitDetails.CED_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_Status)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("ed46573b-7d81-4550-982f-fdf700042330", "Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 162, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.StatusDropEdit.TabIndex = 8;
			// 
			// MovementReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MovementReferenceNumberTextBox, "CusExitDetails.CED_MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_MovementReferenceNumber)));
			this.MovementReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1e7af3ee-c2f8-43be-8ea7-2afee963943e", "MRN");
			this.MovementReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 6, true);
			this.MovementReferenceNumberTextBox.Name = "MovementReferenceNumberTextBox";
			this.MovementReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.MovementReferenceNumberTextBox.TabIndex = 2;
			// 
			// TransportIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportIdTextBox, "CusExitDetails.CED_TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_TransportID)));
			this.TransportIdTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("bdc7850c-b8e2-4449-bb56-25d711030015", "Transport ID");
			this.TransportIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 136, true);
			this.TransportIdTextBox.Name = "TransportIdTextBox";
			this.TransportIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.TransportIdTextBox.TabIndex = 7;
			// 
			// ArrivalNotificationDateDateEdit
			// 
			this.ArrivalNotificationDateDateEdit.AllowDrop = true;
			this.ArrivalNotificationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalNotificationDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalNotificationDateDateEdit, "CusExitDetails.CED_ArrivalNotificationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_ArrivalNotificationDate)));
			this.ArrivalNotificationDateDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("52547eeb-844a-4d27-ad95-1cfa205403dc", "Arrival Notification Date");
			this.ArrivalNotificationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 58, true);
			this.ArrivalNotificationDateDateEdit.Name = "ArrivalNotificationDateDateEdit";
			this.ArrivalNotificationDateDateEdit.TabIndex = 4;
			// 
			// ExitDateDateEdit
			// 
			this.ExitDateDateEdit.AllowDrop = true;
			this.ExitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExitDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExitDateDateEdit, "CusExitDetails.CED_ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_ExitDate)));
			this.ExitDateDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7171e494-04ad-430c-85bc-923631bd149d", "Exit Date");
			this.ExitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 110, true);
			this.ExitDateDateEdit.Name = "ExitDateDateEdit";
			this.ExitDateDateEdit.TabIndex = 6;
			// 
			// ArrivalNotificationPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalNotificationPlaceTextBox, "CusExitDetails.CED_ArrivalNotificationPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_ArrivalNotificationPlace)));
			this.ArrivalNotificationPlaceTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1d38a375-c13d-4320-9746-136aca768987", "Arrival Notification Place");
			this.ArrivalNotificationPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 84, true);
			this.ArrivalNotificationPlaceTextBox.Name = "ArrivalNotificationPlaceTextBox";
			this.ArrivalNotificationPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.ArrivalNotificationPlaceTextBox.TabIndex = 5;
			// 
			// MessagesGroupBox
			// 
			this.MessagesGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3758CB7C-A753-4B5E-A667-C0AA087A2814", "Messages");
			this.MessagesGroupBox.Controls.Add(this.MessagesUserControl);
			this.MessagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 408, true);
			this.MessagesGroupBox.Name = "MessagesGroupBox";
			this.MessagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1282, 238, true);
			this.MessagesGroupBox.TabIndex = 10;
			this.MessagesGroupBox.TabStop = false;
			// 
			// ExitSummaryUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.NewTopPanel);
			this.Controls.Add(this.MovementGroupBox);
			this.Name = "ExitSummaryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 827, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.NewTopPanel.ResumeLayout(false);
			this.NewTopPanel.PerformLayout();
			this.HeaderCarrierOrgAddressControl.ResumeLayout(true);
			this.HeaderCarrierOrgAddressControl.PerformLayout();
			this.HeaderExitDateDateEdit.ResumeLayout(true);
			this.HeaderExitDateDateEdit.PerformLayout();
			this.HeaderArrivalNotificationDateDateEdit.ResumeLayout(true);
			this.HeaderArrivalNotificationDateDateEdit.PerformLayout();
			this.HeaderCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.HeaderCustomsOfficeCodeFindBox.PerformLayout();
			this.AgentOrgAddressControl.ResumeLayout(true);
			this.AgentOrgAddressControl.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.PackingGroupBox.ResumeLayout(false);
			this.PackingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).EndInit();
			this.PackingGrid.ResumeLayout(false);
			this.PackingGrid.PerformLayout();
			this.MovementGroupBox.ResumeLayout(false);
			this.MovementGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).EndInit();
			this.MovementsGrid.ResumeLayout(false);
			this.MovementsGrid.PerformLayout();
			this.MovementDetailsPanel.ResumeLayout(false);
			this.MovementDetailsPanel.PerformLayout();
			this.CarrierOrgAddressControl.ResumeLayout(true);
			this.CarrierOrgAddressControl.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ArrivalNotificationDateDateEdit.ResumeLayout(true);
			this.ArrivalNotificationDateDateEdit.PerformLayout();
			this.ExitDateDateEdit.ResumeLayout(true);
			this.ExitDateDateEdit.PerformLayout();
			this.MessagesGroupBox.ResumeLayout(false);
			this.MessagesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		MasterFiles.GUI.ZOrgAddressControl AgentOrgAddressControl;
		protected ZArchitecture.GUI.ZGroupBox MovementGroupBox;
		protected ZArchitecture.ZTextBox ReferenceNumberTextBox;
		protected ZArchitecture.ZGrid MovementsGrid;
		protected ZArchitecture.ZTextBox MovementReferenceNumberTextBox;
		protected ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		protected ZArchitecture.GUI.ZDateEdit ArrivalNotificationDateDateEdit;
		protected ZArchitecture.ZTextBox ArrivalNotificationPlaceTextBox;
		protected ZArchitecture.GUI.ZDateEdit ExitDateDateEdit;
		protected ZArchitecture.ZTextBox TransportIdTextBox;
		protected ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		protected MasterFiles.GUI.ZOrgAddressControl CarrierOrgAddressControl;
		protected ZArchitecture.GUI.ZCodeFindBox HeaderCustomsOfficeCodeFindBox;
		protected ZArchitecture.GUI.ZDateEdit HeaderArrivalNotificationDateDateEdit;
		protected ZArchitecture.ZTextBox HeaderArrivalNotificationPlaceTextBox;
		protected ZArchitecture.GUI.ZDateEdit HeaderExitDateDateEdit;
		protected ZArchitecture.ZTextBox HeaderTransportIdTextBox;
		MasterFiles.GUI.ZOrgAddressControl HeaderCarrierOrgAddressControl;
		Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl MessagesUserControl;
		protected ZArchitecture.GUI.ZGroupBox MessagesGroupBox;
		protected ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
		ZArchitecture.GUI.ZGroupBox PackingGroupBox;
		protected ZArchitecture.ZGrid ItemsGrid;
		protected ZArchitecture.ZGrid PackingGrid;
		protected ZArchitecture.GUI.ZPanel TopPanel;
		internal ZArchitecture.GUI.ZPanel NewTopPanel;
		protected ZArchitecture.GUI.ZPanel MovementDetailsPanel;
		protected ExitSummaryMainPanelUserControl ExitSummaryMainPanelUserControl;
	}
}
