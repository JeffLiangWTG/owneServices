namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukMainMasterUserControlHelpers
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
			var zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo AgentBadgeColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo ShipmentDescriptionCodeColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo AirportOfOriginColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo AirportOfArrivalColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo AirportOfDestinationColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo CargoTerminalOperatorColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo ProfileColumnStyleInfo17 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ForwardingNumberColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainCcsukTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.HousesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChildrenGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ccsukMessagesUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukMessagesUserControl();
			this.RemovalsAndFallbackTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.underbondUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.UnderbondUserControl();
			this.SplitsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitConsignmentUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.SplitConsignmentGridUserControlBasic();
			this.DeliveryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommunityHandlingCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.deliveryUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.DeliveryUserControl();
			this.communityHandlingCodesUserControl1 = new CommunityHandlingCodesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainCcsukTabControl.SuspendLayout();
			this.HousesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildrenGrid)).BeginInit();
			this.MessagesTabPage.SuspendLayout();
			this.RemovalsAndFallbackTabPage.SuspendLayout();
			this.SplitsTabPage.SuspendLayout();
			this.DeliveryTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB);
			// 
			// MainCcsukTabControl
			// 
			this.MainCcsukTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainCcsukTabControl.Controls.Add(this.HousesTabPage);
			this.MainCcsukTabControl.Controls.Add(this.SplitsTabPage);
			this.MainCcsukTabControl.Controls.Add(this.RemovalsAndFallbackTabPage);
			this.MainCcsukTabControl.Controls.Add(this.MessagesTabPage);
			this.MainCcsukTabControl.Controls.Add(this.DeliveryTabPage);
			this.MainCcsukTabControl.Controls.Add(this.CommunityHandlingCodesTabPage);
			this.MainCcsukTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainCcsukTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainCcsukTabControl.Name = "MainCcsukTabControl";
			this.MainCcsukTabControl.SelectedIndex = 0;
			this.MainCcsukTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(991, 703, true);
			this.MainCcsukTabControl.TabIndex = 0;
			// 
			// HousesTabPage
			// 
			this.HousesTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0cecd100-0e98-4da4-8baa-7892b1571a50", "Houses");
			this.HousesTabPage.Controls.Add(this.ChildrenGrid);
			this.HousesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HousesTabPage.Name = "HousesTabPage";
			this.HousesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HousesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 676, true);
			this.HousesTabPage.TabIndex = 0;
			this.HousesTabPage.UseVisualStyleBackColor = true;
			// 
			// ChildrenGrid
			// 
			this.ChildrenGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildrenGrid, "ChildBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_HAWB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).AgentBadge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).ShipmentDescriptionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).AirportOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).AirportOfArrival)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).AirportOfDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CargoTerminalOperatorAirportAndShed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_PiecesManifested)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_PiecesLanded)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CustomsActionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).LatestCustomsActionText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CustomsActionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_CustomsMainStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_RX_NKGoodsCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).ConsignmentOrEntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).CS_TranshipmentEntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).HasSplits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).IsThroughAwb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).Status1Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).Profile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).PresenceOnNetworkStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).ChildBills)).SyncRoot)).TemporaryStorageEndDate)));
			this.ChildrenGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("36f4733c-0ac9-473e-b22c-717ed536b81c", "House");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CS_HAWB";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			AgentBadgeColumnStyleInfo2.Caption = null;
			AgentBadgeColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0f4b0ee9-0e6a-47ff-b01c-4c955d3b65f9", "Agent");
			AgentBadgeColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			AgentBadgeColumnStyleInfo2.ColumnName = "AgentBadge";
			AgentBadgeColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(51);
			ShipmentDescriptionCodeColumnStyleInfo3.Caption = null;
			ShipmentDescriptionCodeColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("843dd939-0981-4906-a195-4c1eb322689f", "SDC");
			ShipmentDescriptionCodeColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ShipmentDescriptionCodeColumnStyleInfo3.ColumnName = "ShipmentDescriptionCode";
			AirportOfOriginColumnStyleInfo4.Caption = null;
			AirportOfOriginColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9394ab45-e16c-47b1-93a4-83bc0854cf7c", "Origin Airport");
			AirportOfOriginColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			AirportOfOriginColumnStyleInfo4.ColumnName = "AirportOfOrigin";
			AirportOfArrivalColumnStyleInfo5.Caption = null;
			AirportOfArrivalColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9d441d1c-c43f-4c35-bee6-bf7708622da3", "Arrival Airport");
			AirportOfArrivalColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			AirportOfArrivalColumnStyleInfo5.ColumnName = "AirportOfArrival";
			AirportOfDestinationColumnStyleInfo6.Caption = null;
			AirportOfDestinationColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("953fdd85-d012-46d2-ae1c-869d4207f72b", "Dest. Airport");
			AirportOfDestinationColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			AirportOfDestinationColumnStyleInfo6.ColumnName = "AirportOfDestination";
			CargoTerminalOperatorColumnStyleInfo8.Caption = null;
			CargoTerminalOperatorColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("252528a0-6778-4085-9210-0113238dd064", "CTO");
			CargoTerminalOperatorColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			CargoTerminalOperatorColumnStyleInfo8.ColumnName = "CargoTerminalOperatorAirportAndShed";
			CargoTerminalOperatorColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5b1f3b1d-4473-43ff-8004-9f40ac0b498b", "NPX");
			zCalcEditColumnStyleInfo1.ColumnName = "CS_PiecesManifested";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f04699a2-a402-4207-a2e6-0a65e92384e1", "NPR");
			zCalcEditColumnStyleInfo2.ColumnName = "CS_PiecesLanded";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;  // Do not allow this to be removed from the designer
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5102482f-ff20-45d3-b856-b2851b8ecad2", "Weight");
			zCalcEditColumnStyleInfo3.ColumnName = "CS_Weight";
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("e169910f-59f3-4716-ba58-4d95fde20a8c", "CAC");
			zTextBoxColumnStyleInfo10.ColumnName = "CustomsActionCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("88b7972e-3a61-4048-96bb-f4c0fe33aff1", "Latest CAT");
			zTextBoxColumnStyleInfo11.ColumnName = "LatestCustomsActionText";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8370b72f-be8a-45ad-ab1f-7ccf0ac2c7d6", "Customs Action Date");
			zDateEditColumnStyleInfo1.ColumnName = "CustomsActionDate";
			zTextBoxColumnStyleInfo12.Caption = null;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("49a23f3a-401f-4ad0-9fa7-125da8c1d830", "Customs Status");
			zTextBoxColumnStyleInfo12.ColumnName = "CS_CustomsStatus";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zTextBoxColumnStyleInfo13.Caption = null;
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a041d245-35b6-4a72-911c-4e24efb7fd42", "Main Status");
			zTextBoxColumnStyleInfo13.ColumnName = "CS_CustomsMainStatus";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9b1cf824-4ff6-47b9-ad3d-10ac9c490702", "Goods\' Value");
			zCalcEditColumnStyleInfo4.ColumnName = "CS_GoodsValue";
			zTextBoxColumnStyleInfo14.Caption = null;
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ea552f81-853f-418e-a503-43de9d6a8f15", "Currency");
			zTextBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.ColumnName = "CS_RX_NKGoodsCurrency";
			zTextBoxColumnStyleInfo15.Caption = null;
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("918cb8e5-9a67-480e-b5db-99dd2f286e36", "Type");
			zTextBoxColumnStyleInfo15.ColumnName = "ConsignmentOrEntryType";
			zTextBoxColumnStyleInfo16.Caption = null;
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2f6b7dd9-41ec-413c-b624-0e0f7f2670c9", "Transhipment #");
			zTextBoxColumnStyleInfo16.ColumnName = "CS_TranshipmentEntryNum";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("cf730690-c8b2-426d-b287-72bdbf1a4dca", "Splits?");
			zCheckBoxColumnStyleInfo1.ColumnName = "HasSplits";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(54);
			zCheckBoxColumnStyleInfo2.Caption = null;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("634a6855-6037-44dd-9353-0e3dbd92b8d3", "TAWB?");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsThroughAwb";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d16a6c1f-e755-4c39-8c93-05311b20e141", "Status 1");
			zDateEditColumnStyleInfo2.ColumnName = "Status1Date";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			ProfileColumnStyleInfo17.Caption = null;
			ProfileColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f3ef2b7c-8eaa-477e-8b21-c8f9adb4d811", "Profile");
			ProfileColumnStyleInfo17.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ProfileColumnStyleInfo17.ColumnName = "Profile";
			ProfileColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d7362471-f717-4d42-9c37-898f81fb69e1", "Presence");
			zDropEditColumnStyleInfo1.ColumnName = "PresenceOnNetworkStatus";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.IsReadOnly = true;

			ForwardingNumberColumnStyleInfo.Caption = null;
			ForwardingNumberColumnStyleInfo.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("12345678-e755-4c39-8c93-05311b20e141", "Forwarding #");
			ForwardingNumberColumnStyleInfo.ColumnName = "ForwardingNumber";
			ForwardingNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);

			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("dc6b072e-1098-48e1-bb73-a1a4e50987c5", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "CS_GoodsDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(77);
			zTextBoxColumnStyleInfo17.Caption = null;
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c710f9d1-c361-442e-be76-82689e1c81c6", "Temporary Storage End Date");
			zTextBoxColumnStyleInfo17.ColumnName = "TemporaryStorageEndDate";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);


			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChildrenGrid.ColumnStyles.Add(AgentBadgeColumnStyleInfo2);
			this.ChildrenGrid.ColumnStyles.Add(ShipmentDescriptionCodeColumnStyleInfo3);
			this.ChildrenGrid.ColumnStyles.Add(AirportOfOriginColumnStyleInfo4);
			this.ChildrenGrid.ColumnStyles.Add(AirportOfArrivalColumnStyleInfo5);
			this.ChildrenGrid.ColumnStyles.Add(AirportOfDestinationColumnStyleInfo6);
			this.ChildrenGrid.ColumnStyles.Add(CargoTerminalOperatorColumnStyleInfo8);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChildrenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChildrenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChildrenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ChildrenGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ChildrenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ChildrenGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChildrenGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ChildrenGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ChildrenGrid.ColumnStyles.Add(ProfileColumnStyleInfo17);
			this.ChildrenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChildrenGrid.ColumnStyles.Add(ForwardingNumberColumnStyleInfo);
			this.ChildrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ChildrenGrid.CopySelectedRowsAllowed = true;
			this.ChildrenGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildrenGrid.GridId = "ba8e12a0-9072-4bf7-b2a5-d6e852294f6d";
			this.ChildrenGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildrenGrid.LayoutKey = "ChildrenGrid";
			this.ChildrenGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ChildrenGrid.Name = "ChildrenGrid";
			this.ChildrenGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(977, 670, true);
			this.ChildrenGrid.TabIndex = 7;
			this.ChildrenGrid.DoubleClick += new System.EventHandler(this.ChildrenGrid_DoubleClick);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("90c3002b-87a2-4fce-83b6-d93cde22956e", "Messages");
			this.MessagesTabPage.Controls.Add(this.ccsukMessagesUserControl1);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 676, true);
			this.MessagesTabPage.TabIndex = 2;
			// 
			// ccsukMessagesUserControl1
			// 
			this.ccsukMessagesUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukMessagesUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)))));
			this.ccsukMessagesUserControl1.CaptionResourceString = null;
			this.ccsukMessagesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ccsukMessagesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ccsukMessagesUserControl1.Name = "ccsukMessagesUserControl1";
			this.ccsukMessagesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 676, true);
			this.ccsukMessagesUserControl1.TabIndex = 0;
			// 
			// RemovalsAndFallbackTabPage
			// 
			this.RemovalsAndFallbackTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("159bcf4a-57eb-42dd-abf1-ead8f1181f65", "Removals and Fallback");
			this.RemovalsAndFallbackTabPage.Controls.Add(this.underbondUserControl1);
			this.RemovalsAndFallbackTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RemovalsAndFallbackTabPage.Name = "RemovalsAndFallbackTabPage";
			this.RemovalsAndFallbackTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RemovalsAndFallbackTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 676, true);
			this.RemovalsAndFallbackTabPage.TabIndex = 1;
			this.RemovalsAndFallbackTabPage.UseVisualStyleBackColor = true;
			// 
			// underbondUserControl1
			// 
			this.underbondUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.underbondUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.underbondUserControl1.CaptionResourceString = null;
			this.underbondUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.underbondUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.underbondUserControl1.Name = "underbondUserControl1";
			this.underbondUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(977, 670, true);
			this.underbondUserControl1.TabIndex = 0;
			// 
			// SplitsTabPage
			// 
			this.SplitsTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d170df55-4da7-4f41-9dc0-e84910146d38", "Splits");
			this.SplitsTabPage.Controls.Add(this.splitConsignmentUserControl1);
			this.SplitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SplitsTabPage.Name = "SplitsTabPage";
			this.SplitsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SplitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 676, true);
			this.SplitsTabPage.TabIndex = 3;
			this.SplitsTabPage.UseVisualStyleBackColor = true;
			// 
			// splitConsignmentUserControl1
			// 
			this.splitConsignmentUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.splitConsignmentUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)))));
			this.splitConsignmentUserControl1.CaptionResourceString = null;
			this.splitConsignmentUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitConsignmentUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitConsignmentUserControl1.Name = "splitConsignmentUserControl1";
			this.splitConsignmentUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(977, 670, true);
			this.splitConsignmentUserControl1.TabIndex = 0;
			// 
			// DeliveryTabPage
			// 
			this.DeliveryTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6409b87f-c991-4a39-9476-d14053521cec", "Receipt/Delivery");
			this.DeliveryTabPage.Controls.Add(this.deliveryUserControl1);
			this.DeliveryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeliveryTabPage.Name = "DeliveryTabPage";
			this.DeliveryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeliveryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 676, true);
			this.DeliveryTabPage.TabIndex = 4;
			this.DeliveryTabPage.UseVisualStyleBackColor = true;


			this.CommunityHandlingCodesTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d2f99297-568b-4968-b277-a9eeba22f84b", "Handling Codes");
			this.CommunityHandlingCodesTabPage.Controls.Add(this.communityHandlingCodesUserControl1);
			this.CommunityHandlingCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CommunityHandlingCodesTabPage.Name = "CommunityHandlingCodesTabPage";
			this.CommunityHandlingCodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CommunityHandlingCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 676, true);
			this.CommunityHandlingCodesTabPage.TabIndex = 5;
			this.CommunityHandlingCodesTabPage.UseVisualStyleBackColor = true;
			// 
			// deliveryUserControl1
			// 
			this.deliveryUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.deliveryUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)))));
			this.deliveryUserControl1.CaptionResourceString = null;
			this.deliveryUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.deliveryUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.deliveryUserControl1.Name = "deliveryUserControl1";
			this.deliveryUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(977, 670, true);
			this.deliveryUserControl1.TabIndex = 0;


			this.communityHandlingCodesUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.communityHandlingCodesUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.communityHandlingCodesUserControl1.CaptionResourceString = null;
			this.communityHandlingCodesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.communityHandlingCodesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.communityHandlingCodesUserControl1.Name = "communityHandlingCodesUserControl1";
			this.communityHandlingCodesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(977, 670, true);
			this.communityHandlingCodesUserControl1.TabIndex = 0;
			// 
			// CcsukMainMasterUserControlHelpers
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainCcsukTabControl);
			this.Name = "CcsukMainMasterUserControlHelpers";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(991, 703, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainCcsukTabControl.ResumeLayout(false);
			this.HousesTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChildrenGrid)).EndInit();
			this.MessagesTabPage.ResumeLayout(false);
			this.RemovalsAndFallbackTabPage.ResumeLayout(false);
			this.SplitsTabPage.ResumeLayout(false);
			this.DeliveryTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl MainCcsukTabControl;
		private ZArchitecture.GUI.ZTabPage HousesTabPage;
		private ZArchitecture.GUI.ZTabPage RemovalsAndFallbackTabPage;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private UnderbondUserControl underbondUserControl1;
		private CcsukMessagesUserControl ccsukMessagesUserControl1;
		private SplitConsignmentGridUserControl splitConsignmentUserControl1;
		private ZArchitecture.GUI.ZTabPage SplitsTabPage;
		private ZArchitecture.GUI.ZTabPage DeliveryTabPage;
		private ZArchitecture.GUI.ZTabPage CommunityHandlingCodesTabPage;
		private DeliveryUserControl deliveryUserControl1;
		private CommunityHandlingCodesUserControl communityHandlingCodesUserControl1;
		private ZArchitecture.ZGrid ChildrenGrid; 
	}
}
