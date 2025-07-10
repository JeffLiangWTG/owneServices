namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class NctsArrivalUserControl
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
			if (header != null)
			{
				header.BH_OverrideFreightDefaultsInfo.ValueChanged -= BH_OverrideFreightDefaultsInfo_ValueChanged;
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HeaderAndEventsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ArrivalNotificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OverrideFreightDefaults = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DestinationCustomsOfficeCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBoxCRN = new Enterprise.ZArchitecture.ZTextBox();
			this.IsSimplifiedNctsProcedureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DestinationTraderDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.GoodsLocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AgreedLocationOfGoodsCodeCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsSubPlaceCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PlaceOfUnloadingCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BottomTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.IncidentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EnRouteIncidentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TranshipmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.EnRouteTransshipmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TranshipmentContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SealsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SealsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EnRouteSealsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SealNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderAndEventsSplitContainer)).BeginInit();
			this.HeaderAndEventsSplitContainer.Panel1.SuspendLayout();
			this.HeaderAndEventsSplitContainer.Panel2.SuspendLayout();
			this.HeaderAndEventsSplitContainer.SuspendLayout();
			this.ArrivalNotificationGroupBox.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.DestinationCustomsOfficeCodeCodeFindBox.SuspendLayout();
			this.DestinationTraderDocAddressControl.SuspendLayout();
			this.GoodsLocationGroupBox.SuspendLayout();
			this.PlaceOfUnloadingCodeCodeFindBox.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			this.IncidentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnRouteIncidentsGrid)).BeginInit();
			this.EnRouteIncidentsGrid.SuspendLayout();
			this.TranshipmentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnRouteTransshipmentsGrid)).BeginInit();
			this.EnRouteTransshipmentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TranshipmentContainersGrid)).BeginInit();
			this.TranshipmentContainersGrid.SuspendLayout();
			this.SealsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsSplitContainer)).BeginInit();
			this.SealsSplitContainer.Panel1.SuspendLayout();
			this.SealsSplitContainer.Panel2.SuspendLayout();
			this.SealsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnRouteSealsGrid)).BeginInit();
			this.EnRouteSealsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersGrid)).BeginInit();
			this.SealNumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// HeaderAndEventsSplitContainer
			// 
			this.HeaderAndEventsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderAndEventsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderAndEventsSplitContainer.Name = "HeaderAndEventsSplitContainer";
			this.HeaderAndEventsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// HeaderAndEventsSplitContainer.Panel1
			// 
			this.HeaderAndEventsSplitContainer.Panel1.Controls.Add(this.ArrivalNotificationGroupBox);
			this.HeaderAndEventsSplitContainer.Panel1.Controls.Add(this.DestinationTraderDocAddressControl);
			this.HeaderAndEventsSplitContainer.Panel1.Controls.Add(this.GoodsLocationGroupBox);
			this.HeaderAndEventsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 818, true);
			this.HeaderAndEventsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(297);
			// 
			// HeaderAndEventsSplitContainer.Panel2
			// 
			this.HeaderAndEventsSplitContainer.Panel2.Controls.Add(this.BottomTabControl);
			this.HeaderAndEventsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(300);
			this.HeaderAndEventsSplitContainer.TabIndex = 4;
			// 
			// ArrivalNotificationGroupBox
			// 
			this.ArrivalNotificationGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("fb7b011c-91d4-4820-ba5e-564b9f68d7d1", "Arrival Notification Details");
			this.ArrivalNotificationGroupBox.Controls.Add(this.OverrideFreightDefaults);
			this.ArrivalNotificationGroupBox.Controls.Add(this.MessageStatusDropEdit);
			this.ArrivalNotificationGroupBox.Controls.Add(this.StatusDropEdit);
			this.ArrivalNotificationGroupBox.Controls.Add(this.DestinationCustomsOfficeCodeCodeFindBox);
			this.ArrivalNotificationGroupBox.Controls.Add(this.zTextBox1);
			this.ArrivalNotificationGroupBox.Controls.Add(this.zTextBoxCRN);
			this.ArrivalNotificationGroupBox.Controls.Add(this.IsSimplifiedNctsProcedureCheckBox);
			this.ArrivalNotificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ArrivalNotificationGroupBox.Name = "ArrivalNotificationGroupBox";
			this.ArrivalNotificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 180, true);
			this.ArrivalNotificationGroupBox.TabIndex = 0;
			this.ArrivalNotificationGroupBox.TabStop = false;
			// 
			// OverrideFreightDefaults
			// 
			this.BindingSource.SetBindingMember(this.OverrideFreightDefaults, "BH_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_OverrideFreightDefaults)));
			this.OverrideFreightDefaults.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("a55931ee-72ee-444f-8374-b6b499f9589b", "Override Freight Defaults?");
			this.OverrideFreightDefaults.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideFreightDefaults.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 19, true);
			this.OverrideFreightDefaults.Name = "OverrideFreightDefaults";
			this.OverrideFreightDefaults.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 16, true);
			this.OverrideFreightDefaults.TabIndex = 1;
			this.OverrideFreightDefaults.UseVisualStyleBackColor = true;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "EffectiveMessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EffectiveMessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 155, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.ShouldResizeByMaxLength = true;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.MessageStatusDropEdit.TabIndex = 6;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "ArrivalMovementHeader.BM_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_CustomsStatus)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ba7332eb-e44a-438a-9b2d-1309bd1f781d", "Arrival Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 128, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.StatusDropEdit.TabIndex = 5;
			// 
			// DestinationCustomsOfficeCodeCodeFindBox
			// 
			this.DestinationCustomsOfficeCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCustomsOfficeCodeCodeFindBox, "DestinationCustomsOfficeCodeForArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DestinationCustomsOfficeCodeForArrival)));
			this.DestinationCustomsOfficeCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 102, true);
			this.DestinationCustomsOfficeCodeCodeFindBox.Name = "DestinationCustomsOfficeCodeCodeFindBox";
			this.DestinationCustomsOfficeCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.DestinationCustomsOfficeCodeCodeFindBox.TabIndex = 4;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "ArrivalMrnFromUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMrnFromUser)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 73, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zTextBoxCRN
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxCRN, "LocalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).LocalReferenceNumber)));
			this.zTextBoxCRN.CaptionResourceString = null;
			this.zTextBoxCRN.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBoxCRN.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 45, true);
			this.zTextBoxCRN.Name = "zTextBoxCRN";
			this.zTextBoxCRN.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.zTextBoxCRN.TabIndex = 2;
			// 
			// IsSimplifiedNctsProcedureCheckBox
			// 
			this.IsSimplifiedNctsProcedureCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSimplifiedNctsProcedureCheckBox, "ArrivalMovementHeader.IsSimplifiedNctsProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.IsSimplifiedNctsProcedure)));
			this.IsSimplifiedNctsProcedureCheckBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ad4f3434-6a32-492a-8eac-7ab5cece7e6f", "Simplified Procedure at Destination?");
			this.IsSimplifiedNctsProcedureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSimplifiedNctsProcedureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
			this.IsSimplifiedNctsProcedureCheckBox.Name = "IsSimplifiedNctsProcedureCheckBox";
			this.IsSimplifiedNctsProcedureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 17, true);
			this.IsSimplifiedNctsProcedureCheckBox.TabIndex = 0;
			this.IsSimplifiedNctsProcedureCheckBox.UseVisualStyleBackColor = true;
			// 
			// DestinationTraderDocAddressControl
			// 
			this.DestinationTraderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationTraderDocAddressControl, "DestinationTrader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DestinationTrader)));
			this.DestinationTraderDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.DestinationTraderDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ea934059-04e3-4fb7-9cf8-b8303142486a", "Destination Trader");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DestinationTraderDocAddressControl, false);
			this.DestinationTraderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 3, true);
			this.DestinationTraderDocAddressControl.Name = "DestinationTraderDocAddressControl";
			this.DestinationTraderDocAddressControl.ReadOnly = false;
			this.DestinationTraderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DestinationTraderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DestinationTraderDocAddressControl.TabIndex = 2;
			this.DestinationTraderDocAddressControl.ValidationJustForced = false;
			// 
			// GoodsLocationGroupBox
			// 
			this.GoodsLocationGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("fe56f401-7bbd-48bc-8d4a-4dd54b4269bc", "Goods Location at Destination");
			this.GoodsLocationGroupBox.Controls.Add(this.AgreedLocationOfGoodsCodeCodeTextBox);
			this.GoodsLocationGroupBox.Controls.Add(this.CustomsSubPlaceCodeTextBox);
			this.GoodsLocationGroupBox.Controls.Add(this.PlaceOfUnloadingCodeCodeFindBox);
			this.GoodsLocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 189, true);
			this.GoodsLocationGroupBox.Name = "GoodsLocationGroupBox";
			this.GoodsLocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 102, true);
			this.GoodsLocationGroupBox.TabIndex = 1;
			this.GoodsLocationGroupBox.TabStop = false;
			// 
			// AgreedLocationOfGoodsCodeCodeTextBox
			// 
			this.AgreedLocationOfGoodsCodeCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedLocationOfGoodsCodeCodeTextBox, "ArrivalMovementHeader.BM_LocationOfGoodsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_LocationOfGoodsCode)));
			this.AgreedLocationOfGoodsCodeCodeTextBox.CaptionResourceString = null;
			this.AgreedLocationOfGoodsCodeCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 71, true);
			this.AgreedLocationOfGoodsCodeCodeTextBox.Name = "AgreedLocationOfGoodsCodeCodeTextBox";
			this.AgreedLocationOfGoodsCodeCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 20, true);
			this.AgreedLocationOfGoodsCodeCodeTextBox.TabIndex = 2;
			// 
			// CustomsSubPlaceCodeTextBox
			// 
			this.CustomsSubPlaceCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsSubPlaceCodeTextBox, "ArrivalMovementHeader.BM_CustomsSubPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_CustomsSubPlace)));
			this.CustomsSubPlaceCodeTextBox.CaptionResourceString = null;
			this.CustomsSubPlaceCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 45, true);
			this.CustomsSubPlaceCodeTextBox.Name = "CustomsSubPlaceCodeTextBox";
			this.CustomsSubPlaceCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 20, true);
			this.CustomsSubPlaceCodeTextBox.TabIndex = 1;
			// 
			// PlaceOfUnloadingCodeCodeFindBox
			// 
			this.PlaceOfUnloadingCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingCodeCodeFindBox, "ArrivalMovementHeader.BM_PlaceOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_PlaceOfUnloading)));
			this.PlaceOfUnloadingCodeCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("9e38253e-7397-4dd1-8314-3eab098d5324", "Place of Unloading");
			this.PlaceOfUnloadingCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 19, true);
			this.PlaceOfUnloadingCodeCodeFindBox.Name = "PlaceOfUnloadingCodeCodeFindBox";
			this.PlaceOfUnloadingCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 20, true);
			this.PlaceOfUnloadingCodeCodeFindBox.TabIndex = 0;
			// 
			// BottomTabControl
			// 
			this.BottomTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BottomTabControl.Controls.Add(this.IncidentsTabPage);
			this.BottomTabControl.Controls.Add(this.TranshipmentsTabPage);
			this.BottomTabControl.Controls.Add(this.SealsTabPage);
			this.BottomTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomTabControl.Name = "BottomTabControl";
			this.BottomTabControl.SelectedIndex = 0;
			this.BottomTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 514, true);
			this.BottomTabControl.TabIndex = 3;
			// 
			// IncidentsTabPage
			// 
			this.IncidentsTabPage.Controls.Add(this.EnRouteIncidentsGrid);
			this.IncidentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d7ea9455-5bf8-41c5-98f3-da90d7eb7543", "Incidents");
			this.IncidentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IncidentsTabPage.Name = "IncidentsTabPage";
			this.IncidentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IncidentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 487, true);
			this.IncidentsTabPage.TabIndex = 0;
			this.IncidentsTabPage.UseVisualStyleBackColor = true;
			// 
			// EnRouteIncidentsGrid
			// 
			this.EnRouteIncidentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EnRouteIncidentsGrid, "EnRouteIncidents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).BN_EventPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).BN_EventCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).IsInNCTS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).BN_Information)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).BN_EndorsementDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).BN_EndorsementAuthority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).BN_EndorsementPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteIncidents)).SyncRoot)).BN_EndorsementCountryCode)));
			this.EnRouteIncidentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "BN_EventPlace";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BN_EventCountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsInNCTS";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "BN_Information";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "BN_EndorsementDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.ColumnName = "BN_EndorsementAuthority";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo4.ColumnName = "BN_EndorsementPlace";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "BN_EndorsementCountryCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EnRouteIncidentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.EnRouteIncidentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnRouteIncidentsGrid.GridId = "a3018e30-709c-4dd7-abba-a61864ae6eed";
			this.EnRouteIncidentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EnRouteIncidentsGrid.LayoutKey = "zGrid1";
			this.EnRouteIncidentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EnRouteIncidentsGrid.Name = "EnRouteIncidentsGrid";
			this.EnRouteIncidentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 481, true);
			this.EnRouteIncidentsGrid.TabIndex = 5;
			// 
			// TranshipmentsTabPage
			// 
			this.TranshipmentsTabPage.Controls.Add(this.splitContainer2);
			this.TranshipmentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("f6defc77-100f-4450-a2cd-b25e951ff09e", "Transhipments");
			this.TranshipmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TranshipmentsTabPage.Name = "TranshipmentsTabPage";
			this.TranshipmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TranshipmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 487, true);
			this.TranshipmentsTabPage.TabIndex = 1;
			this.TranshipmentsTabPage.UseVisualStyleBackColor = true;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.EnRouteTransshipmentsGrid);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.TranshipmentContainersGrid);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 481, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(237);
			this.splitContainer2.TabIndex = 2;
			// 
			// EnRouteTransshipmentsGrid
			// 
			this.EnRouteTransshipmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EnRouteTransshipmentsGrid, "EnRouteTransshipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_EventPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_EventCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).IsInNCTS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_TransportID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_EndorsementDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_EndorsementAuthority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_EndorsementPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_EndorsementCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).BN_TransportCountryCode)));
			this.EnRouteTransshipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("e78cfc61-6450-4eee-befe-8e5b208159a4", "Event Place");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo5.ColumnName = "BN_EventPlace";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "BN_EventCountryCode";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("13814afc-ff37-412e-a2c8-df282f7249db", "In NCTS?");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsInNCTS";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("f7da4473-5713-4feb-8801-bdc32921c793", "New Transport ID");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo6.ColumnName = "BN_TransportID";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo2.ColumnName = "BN_EndorsementDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("3814a3ad-33b8-4565-9eed-990ceccfad75", "Reported By");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo7.ColumnName = "BN_EndorsementAuthority";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo8.ColumnName = "BN_EndorsementPlace";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "BN_EndorsementCountryCode";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zCodeFindBoxColumnStyleInfo5.ColumnName = "BN_TransportCountryCode";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.EnRouteTransshipmentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.EnRouteTransshipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnRouteTransshipmentsGrid.GridId = "a3018e30-709c-4dd7-abba-a61864ae6eed";
			this.EnRouteTransshipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EnRouteTransshipmentsGrid.LayoutKey = "zGrid1";
			this.EnRouteTransshipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EnRouteTransshipmentsGrid.Name = "EnRouteTransshipmentsGrid";
			this.EnRouteTransshipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 237, true);
			this.EnRouteTransshipmentsGrid.TabIndex = 5;
			// 
			// TranshipmentContainersGrid
			// 
			this.TranshipmentContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TranshipmentContainersGrid, "EnRouteTransshipments.Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteTransshipment)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteTransshipments)).SyncRoot)).Containers)).SyncRoot)).ContainerNumber)));
			this.TranshipmentContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4eb0529f-e8da-47d1-89ab-b257b45b3d20", "Container");
			zTextBoxColumnStyleInfo9.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TranshipmentContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.TranshipmentContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TranshipmentContainersGrid.GridId = "75590492-1fa7-4017-9e99-658c30d08c1c";
			this.TranshipmentContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TranshipmentContainersGrid.LayoutKey = "zGrid2";
			this.TranshipmentContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TranshipmentContainersGrid.Name = "TranshipmentContainersGrid";
			this.TranshipmentContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 240, true);
			this.TranshipmentContainersGrid.TabIndex = 6;
			// 
			// SealsTabPage
			// 
			this.SealsTabPage.Controls.Add(this.SealsSplitContainer);
			this.SealsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7d95999e-98da-4579-9ec3-42249d88efc2", "Seals");
			this.SealsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SealsTabPage.Name = "SealsTabPage";
			this.SealsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SealsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 487, true);
			this.SealsTabPage.TabIndex = 2;
			this.SealsTabPage.UseVisualStyleBackColor = true;
			// 
			// SealsSplitContainer
			// 
			this.SealsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SealsSplitContainer.Name = "SealsSplitContainer";
			this.SealsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SealsSplitContainer.Panel1
			// 
			this.SealsSplitContainer.Panel1.Controls.Add(this.EnRouteSealsGrid);
			// 
			// SealsSplitContainer.Panel2
			// 
			this.SealsSplitContainer.Panel2.Controls.Add(this.SealNumbersGrid);
			this.SealsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 481, true);
			this.SealsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(236);
			this.SealsSplitContainer.TabIndex = 2;
			// 
			// EnRouteSealsGrid
			// 
			this.EnRouteSealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EnRouteSealsGrid, "EnRouteSeals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteSeals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteSeals)).SyncRoot)).BN_EventPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteSeals)).SyncRoot)).BN_EventCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((Enterprise.Customs.EU.NCTS.Business.EnRouteSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteSeals)).SyncRoot)).BN_NoOfSeals)));
			this.EnRouteSealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7798780C-A012-4032-9E27-2752F737EBEB", "Event Place");
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo10.ColumnName = "BN_EventPlace";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo6.ColumnName = "BN_EventCountryCode";
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("3D52F2C1-8FCC-4280-9D0B-9427BF070433", "No. of Seals");
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "BN_NoOfSeals";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			this.EnRouteSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.EnRouteSealsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.EnRouteSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.EnRouteSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnRouteSealsGrid.GridId = "3B94E964-33EB-452C-8314-716496A70EBE";
			this.EnRouteSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EnRouteSealsGrid.LayoutKey = "zGrid1";
			this.EnRouteSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EnRouteSealsGrid.Name = "EnRouteSealsGrid";
			this.EnRouteSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 236, true);
			this.EnRouteSealsGrid.TabIndex = 5;
			// 
			// SealNumbersGrid
			// 
			this.SealNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SealNumbersGrid, "EnRouteSeals.SealContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteSeals)).SyncRoot)).SealContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.SealContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EnRouteSeals)).SyncRoot)).SealContainers)).SyncRoot)).BC_Seal1)));
			this.SealNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "BC_Seal1";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129);
			this.SealNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.SealNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumbersGrid.GridId = "6E172484-AE58-47C2-B1B7-1DC719991824";
			this.SealNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SealNumbersGrid.LayoutKey = "zGrid2";
			this.SealNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealNumbersGrid.Name = "SealNumbersGrid";
			this.SealNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 241, true);
			this.SealNumbersGrid.TabIndex = 6;
			// 
			// NctsArrivalUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HeaderAndEventsSplitContainer);
			this.Name = "NctsArrivalUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 818, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderAndEventsSplitContainer.Panel1.ResumeLayout(false);
			this.HeaderAndEventsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HeaderAndEventsSplitContainer)).EndInit();
			this.HeaderAndEventsSplitContainer.ResumeLayout(false);
			this.HeaderAndEventsSplitContainer.PerformLayout();
			this.ArrivalNotificationGroupBox.ResumeLayout(false);
			this.ArrivalNotificationGroupBox.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.DestinationCustomsOfficeCodeCodeFindBox.ResumeLayout(true);
			this.DestinationCustomsOfficeCodeCodeFindBox.PerformLayout();
			this.DestinationTraderDocAddressControl.ResumeLayout(true);
			this.DestinationTraderDocAddressControl.PerformLayout();
			this.GoodsLocationGroupBox.ResumeLayout(false);
			this.GoodsLocationGroupBox.PerformLayout();
			this.PlaceOfUnloadingCodeCodeFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingCodeCodeFindBox.PerformLayout();
			this.BottomTabControl.ResumeLayout(false);
			this.BottomTabControl.PerformLayout();
			this.IncidentsTabPage.ResumeLayout(false);
			this.IncidentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnRouteIncidentsGrid)).EndInit();
			this.EnRouteIncidentsGrid.ResumeLayout(false);
			this.EnRouteIncidentsGrid.PerformLayout();
			this.TranshipmentsTabPage.ResumeLayout(false);
			this.TranshipmentsTabPage.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnRouteTransshipmentsGrid)).EndInit();
			this.EnRouteTransshipmentsGrid.ResumeLayout(false);
			this.EnRouteTransshipmentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TranshipmentContainersGrid)).EndInit();
			this.TranshipmentContainersGrid.ResumeLayout(false);
			this.TranshipmentContainersGrid.PerformLayout();
			this.SealsTabPage.ResumeLayout(false);
			this.SealsTabPage.PerformLayout();
			this.SealsSplitContainer.Panel1.ResumeLayout(false);
			this.SealsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SealsSplitContainer)).EndInit();
			this.SealsSplitContainer.ResumeLayout(false);
			this.SealsSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnRouteSealsGrid)).EndInit();
			this.EnRouteSealsGrid.ResumeLayout(false);
			this.EnRouteSealsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersGrid)).EndInit();
			this.SealNumbersGrid.ResumeLayout(false);
			this.SealNumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		
		protected Enterprise.ZArchitecture.GUI.ZGroupBox GoodsLocationGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox IsSimplifiedNctsProcedureCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationCustomsOfficeCodeCodeFindBox;
		protected ZArchitecture.ZTextBox zTextBox1;
		protected ZArchitecture.ZTextBox zTextBoxCRN;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox PlaceOfUnloadingCodeCodeFindBox;
		protected Enterprise.ZArchitecture.ZTextBox CustomsSubPlaceCodeTextBox;
		protected Enterprise.ZArchitecture.ZTextBox AgreedLocationOfGoodsCodeCodeTextBox;
		protected Enterprise.ZArchitecture.ZGrid EnRouteSealsGrid;
		protected Enterprise.ZArchitecture.ZGrid EnRouteTransshipmentsGrid;
		protected Enterprise.ZArchitecture.ZGrid EnRouteIncidentsGrid;
		protected Enterprise.ZArchitecture.GUI.ZTabControl BottomTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage IncidentsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage TranshipmentsTabPage;
		protected Enterprise.ZArchitecture.ZGrid SealNumbersGrid;
		protected Enterprise.ZArchitecture.ZGrid TranshipmentContainersGrid;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private CargoWise.Windows.UI.KSplitContainer SealsSplitContainer;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		protected MasterFiles.GUI.ZDocAddressControl DestinationTraderDocAddressControl;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OverrideFreightDefaults;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ArrivalNotificationGroupBox;
		private ZArchitecture.GUI.ZTabPage SealsTabPage;
		protected CargoWise.Windows.UI.KSplitContainer HeaderAndEventsSplitContainer;
	}
}
