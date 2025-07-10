using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRBaseHouseDetailsUserControl
	{
		void InitializeComponent()
		{
			this.HouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConRefLabel = new Enterprise.ZArchitecture.ZLabel();
			this.conRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.populateABNButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.responsiblePartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.responsiblePartyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.underbondStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.detailsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TranshipmentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TranshipmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.GoodsDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NonCustomsItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ServiceLevelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.WarehouseLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FolioReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WarehouseLocationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel33 = new Enterprise.ZArchitecture.ZLabel();
			this.ChargableWeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChargeableWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WeightUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PrepaidCollectDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PersonalEffectsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SACCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.goodsValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.statusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.shipmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.shipmentTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PiecesManifestedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DestinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HouseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HouseBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PrepaidCollectLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GoodsValLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackagesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.underbondStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HouseGroupBox.SuspendLayout();
			this.GoodsDescriptionTextBox.SuspendLayout();
			this.NonCustomsItemsGroupBox.SuspendLayout();
			this.ServiceLevelCodeFindBox.SuspendLayout();
			this.PrepaidCollectDropEdit.SuspendLayout();
			this.goodsValueCalcDropEdit.SuspendLayout();
			this.shipmentTypeDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.DestinationFindBox.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusHAWB);
			// 
			// HouseGroupBox
			// 
			this.HouseGroupBox.Controls.Add(this.ConRefLabel);
			this.HouseGroupBox.Controls.Add(this.conRefTextBox);
			this.HouseGroupBox.Controls.Add(this.populateABNButton);
			this.HouseGroupBox.Controls.Add(this.responsiblePartyLabel);
			this.HouseGroupBox.Controls.Add(this.responsiblePartyTextBox);
			this.HouseGroupBox.Controls.Add(this.underbondStatusTextBox);
			this.HouseGroupBox.Controls.Add(this.detailsButton);
			this.HouseGroupBox.Controls.Add(this.TranshipmentLabel);
			this.HouseGroupBox.Controls.Add(this.TranshipmentTextBox);
			this.HouseGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
			this.HouseGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.HouseGroupBox.Controls.Add(this.zLabel4);
			this.HouseGroupBox.Controls.Add(this.GoodsDescriptionLabel);
			this.HouseGroupBox.Controls.Add(this.NonCustomsItemsGroupBox);
			this.HouseGroupBox.Controls.Add(this.PrepaidCollectDropEdit);
			this.HouseGroupBox.Controls.Add(this.PersonalEffectsCheckBox);
			this.HouseGroupBox.Controls.Add(this.SACCheckBox);
			this.HouseGroupBox.Controls.Add(this.goodsValueCalcDropEdit);
			this.HouseGroupBox.Controls.Add(this.StatusTextBox);
			this.HouseGroupBox.Controls.Add(this.statusLabel);
			this.HouseGroupBox.Controls.Add(this.shipmentTypeDropEdit);
			this.HouseGroupBox.Controls.Add(this.shipmentTypeLabel);
			this.HouseGroupBox.Controls.Add(this.WeightCalcDropEdit);
			this.HouseGroupBox.Controls.Add(this.PiecesManifestedCalcEdit);
			this.HouseGroupBox.Controls.Add(this.DestinationFindBox);
			this.HouseGroupBox.Controls.Add(this.OriginFindBox);
			this.HouseGroupBox.Controls.Add(this.HouseBillTextBox);
			this.HouseGroupBox.Controls.Add(this.DestLabel);
			this.HouseGroupBox.Controls.Add(this.OriginLabel);
			this.HouseGroupBox.Controls.Add(this.HouseBillLabel);
			this.HouseGroupBox.Controls.Add(this.PrepaidCollectLabel);
			this.HouseGroupBox.Controls.Add(this.GoodsValLabel);
			this.HouseGroupBox.Controls.Add(this.WeightLabel);
			this.HouseGroupBox.Controls.Add(this.PackagesLabel);
			this.HouseGroupBox.Controls.Add(this.underbondStatusLabel);
			this.HouseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseGroupBox.Name = "HouseGroupBox";
			this.HouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 243, true);
			this.HouseGroupBox.TabIndex = 0;
			this.HouseGroupBox.TabStop = false;
			this.HouseGroupBox.Text = "Details";
			// 
			// ConRefLabel
			// 
			this.ConRefLabel.AutoSize = true;
			this.ConRefLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 104, true);
			this.ConRefLabel.Name = "ConRefLabel";
			this.ConRefLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.ConRefLabel.TabIndex = 33;
			this.ConRefLabel.Text = "Con Ref:";
			// 
			// ConRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.conRefTextBox, "CS_fPartShipConsignmentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_fPartShipConsignmentReference)));
			this.conRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 101, true);
			this.conRefTextBox.Name = "ConRefTextBox";
			this.conRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.conRefTextBox.TabIndex = 19;
			// 
			// PopulateABNButton
			// 
			this.populateABNButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 201, true);
			this.populateABNButton.Name = "PopulateABNButton";
			this.populateABNButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 21, true);
			this.populateABNButton.TabIndex = 30;
			this.populateABNButton.Text = "Populate from Consignee";
			this.populateABNButton.UseVisualStyleBackColor = true;
			this.populateABNButton.Click += new System.EventHandler(this.PopulateABNButton_Click);
			// 
			// ResponsiblePartyLabel
			// 
			this.responsiblePartyLabel.AutoSize = true;
			this.responsiblePartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 203, true);
			this.responsiblePartyLabel.Name = "ResponsiblePartyLabel";
			this.responsiblePartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.responsiblePartyLabel.TabIndex = 31;
			this.responsiblePartyLabel.Text = "Resp. Id:";
			// 
			// ResponsiblePartyTextBox
			// 
			this.BindingSource.SetBindingMember(this.responsiblePartyTextBox, "CS_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_ResponsiblePartyID)));
			this.responsiblePartyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 201, true);
			this.responsiblePartyTextBox.Name = "ResponsiblePartyTextBox";
			this.responsiblePartyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.responsiblePartyTextBox.TabIndex = 12;
			// 
			// UnderbondStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.underbondStatusTextBox, "UnderbondStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).UnderbondStatus)));
			this.underbondStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 55, true);
			this.underbondStatusTextBox.Name = "UnderbondStatusTextBox";
			this.underbondStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.underbondStatusTextBox.TabIndex = 17;
			// 
			// DetailsButton
			// 
			this.detailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 11, true);
			this.detailsButton.Name = "DetailsButton";
			this.detailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.detailsButton.TabIndex = 15;
			this.detailsButton.Text = "Details";
			this.detailsButton.Click += new System.EventHandler(this.DetailsButton_Click);
			// 
			// TranshipmentLabel
			// 
			this.TranshipmentLabel.AutoSize = true;
			this.TranshipmentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 79, true);
			this.TranshipmentLabel.Name = "TranshipmentLabel";
			this.TranshipmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 13, true);
			this.TranshipmentLabel.TabIndex = 26;
			this.TranshipmentLabel.Text = "Transhipment:";
			// 
			// TranshipmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.TranshipmentTextBox, "CS_TranshipmentEntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_TranshipmentEntryNum)));
			this.TranshipmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 77, true);
			this.TranshipmentTextBox.Name = "TranshipmentTextBox";
			this.TranshipmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TranshipmentTextBox.TabIndex = 18;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 128, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 23, true);
			this.GoodsDescriptionTextBox.TabIndex = 6;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "CMRMessageStatus.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CMRMessageStatus.Description)));
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 33, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.MessageStatusTextBox.TabIndex = 16;
			// 
			// zLabel4
			// 
			this.zLabel4.AutoSize = true;
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 36, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.zLabel4.TabIndex = 24;
			this.zLabel4.Text = "Message Status:";
			// 
			// GoodsDescriptionLabel
			// 
			this.GoodsDescriptionLabel.AutoSize = true;
			this.GoodsDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 132, true);
			this.GoodsDescriptionLabel.Name = "GoodsDescriptionLabel";
			this.GoodsDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.GoodsDescriptionLabel.TabIndex = 11;
			this.GoodsDescriptionLabel.Text = "Goods Desc:";
			// 
			// NonCustomsItemsGroupBox
			// 
			this.NonCustomsItemsGroupBox.Controls.Add(this.zLabel1);
			this.NonCustomsItemsGroupBox.Controls.Add(this.WarehouseLocationTextBox);
			this.NonCustomsItemsGroupBox.Controls.Add(this.FolioReferenceTextBox);
			this.NonCustomsItemsGroupBox.Controls.Add(this.WarehouseLocationLabel);
			this.NonCustomsItemsGroupBox.Controls.Add(this.zLabel33);
			this.NonCustomsItemsGroupBox.Controls.Add(this.ChargableWeightLabel);
			this.NonCustomsItemsGroupBox.Controls.Add(this.ChargeableWeightCalcEdit);
			this.NonCustomsItemsGroupBox.Controls.Add(this.WeightUnitLabel);
			this.NonCustomsItemsGroupBox.Controls.Add(this.ServiceLevelCodeFindBox);
			this.NonCustomsItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 128, true);
			this.NonCustomsItemsGroupBox.Name = "NonCustomsItemsGroupBox";
			this.NonCustomsItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 111, true);
			this.NonCustomsItemsGroupBox.TabIndex = 28;
			this.NonCustomsItemsGroupBox.TabStop = false;
			this.NonCustomsItemsGroupBox.Text = "Non Customs Items";
			// 
			// ServiceLevelCodeFindBox
			// 
			this.ServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelCodeFindBox, "CS_RS_NK_ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_RS_NK_ServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.RefServiceLevelList)));
			this.ServiceLevelCodeFindBox.BindToList = "Lookups+RefServiceLevelList";
			this.ServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 86, true);
			this.ServiceLevelCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.ServiceLevelCodeFindBox.Name = "ServiceLevelCodeFindBox";
			this.ServiceLevelCodeFindBox.PreBoundMaxLength = 3;
			this.ServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ServiceLevelCodeFindBox.TabIndex = 8;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 89, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.zLabel1.TabIndex = 7;
			this.zLabel1.Text = "Service Level:";
			// 
			// WarehouseLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.WarehouseLocationTextBox, "CS_WarehouseLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_WarehouseLocation)));
			this.WarehouseLocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.WarehouseLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 18, true);
			this.WarehouseLocationTextBox.Name = "WarehouseLocationTextBox";
			this.WarehouseLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.WarehouseLocationTextBox.TabIndex = 0;
			// 
			// FolioReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.FolioReferenceTextBox, "CS_FolioReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_FolioReference)));
			this.FolioReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FolioReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 42, true);
			this.FolioReferenceTextBox.Name = "FolioReferenceTextBox";
			this.FolioReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.FolioReferenceTextBox.TabIndex = 1;
			// 
			// WarehouseLocationLabel
			// 
			this.BindingSource.SetBindingMember(this.WarehouseLocationLabel, "WarehouseLocationCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).WarehouseLocationCaption)));
			this.WarehouseLocationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 14, true);
			this.WarehouseLocationLabel.Name = "WarehouseLocationLabel";
			this.WarehouseLocationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 28, true);
			this.WarehouseLocationLabel.TabIndex = 0;
			this.WarehouseLocationLabel.Text = "W/house Location:";
			// 
			// zLabel33
			// 
			this.zLabel33.AutoSize = true;
			this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 46, true);
			this.zLabel33.Name = "zLabel33";
			this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.zLabel33.TabIndex = 2;
			this.zLabel33.Text = "Folio Reference:";
			// 
			// ChargableWeightLabel
			// 
			this.ChargableWeightLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChargableWeightLabel, "ChargableWeightCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).ChargableWeightCaption)));
			this.ChargableWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 67, true);
			this.ChargableWeightLabel.Name = "ChargableWeightLabel";
			this.ChargableWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 13, true);
			this.ChargableWeightLabel.TabIndex = 4;
			this.ChargableWeightLabel.Text = "Chargeable Weight:";
			// 
			// ChargeableWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit, "CS_ChargableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_ChargableWeight)));
			this.ChargeableWeightCalcEdit.DecimalPlaces = 2;
			this.ChargeableWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.ChargeableWeightCalcEdit.Name = "ChargeableWeightCalcEdit";
			this.ChargeableWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ChargeableWeightCalcEdit.TabIndex = 2;
			this.ChargeableWeightCalcEdit.Text = "0.000";
			this.ChargeableWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.WeightUnitLabel, "CS_WeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_WeightUQ)));
			this.WeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 66, true);
			this.WeightUnitLabel.Name = "WeightUnitLabel";
			this.WeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 14, true);
			this.WeightUnitLabel.TabIndex = 3;
			this.WeightUnitLabel.Text = "KG";
			// 
			// PrepaidCollectDropEdit
			// 
			this.PrepaidCollectDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrepaidCollectDropEdit, "CS_FreightPrepaidCollectForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_FreightPrepaidCollectForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.PrepaidCollectList)));
			this.PrepaidCollectDropEdit.BindToList = "Lookups+PrepaidCollectList";
			this.PrepaidCollectDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 176, true);
			this.PrepaidCollectDropEdit.Name = "PrepaidCollectDropEdit";
			this.PrepaidCollectDropEdit.PreBoundMaxLength = 3;
			this.PrepaidCollectDropEdit.ShowDescriptionBox = false;
			this.PrepaidCollectDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PrepaidCollectDropEdit.TabIndex = 9;
			// 
			// PersonalEffectsCheckBox
			// 
			this.PersonalEffectsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PersonalEffectsCheckBox, "CS_IsPersonalEffects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_IsPersonalEffects)));
			this.PersonalEffectsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PersonalEffectsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 177, true);
			this.PersonalEffectsCheckBox.Name = "PersonalEffectsCheckBox";
			this.PersonalEffectsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 17, true);
			this.PersonalEffectsCheckBox.TabIndex = 11;
			this.PersonalEffectsCheckBox.Text = "P/E";
			// 
			// SACCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SACCheckBox, "CS_IsSelfAssessedClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_IsSelfAssessedClearance)));
			this.SACCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SACCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 153, true);
			this.SACCheckBox.Name = "SACCheckBox";
			this.SACCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 18, true);
			this.SACCheckBox.TabIndex = 8;
			this.SACCheckBox.Text = "Self Assessed Clearance";
			this.SACCheckBox.CheckedChanged += new System.EventHandler(this.SACCheckBox_CheckedChanged);
			// 
			// GoodsValueCalcDropEdit
			// 
			this.goodsValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.goodsValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_RX_NKGoodsCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.CMRCurrencyList)));
			this.goodsValueCalcDropEdit.BindToAmount = "CS_GoodsValue";
			this.goodsValueCalcDropEdit.BindToList = "Lookups+CMRCurrencyList";
			this.goodsValueCalcDropEdit.BindToUnit = "CS_RX_NKGoodsCurrency";
			this.goodsValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 152, true);
			this.goodsValueCalcDropEdit.Name = "GoodsValueCalcDropEdit";
			this.goodsValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.goodsValueCalcDropEdit.TabIndex = 7;
			this.goodsValueCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "CMRCargoStatus.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CMRCargoStatus.Description)));
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 11, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.StatusTextBox.TabIndex = 14;
			// 
			// StatusLabel
			// 
			this.statusLabel.AutoSize = true;
			this.statusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 14, true);
			this.statusLabel.Name = "StatusLabel";
			this.statusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.statusLabel.TabIndex = 21;
			this.statusLabel.Text = "Customs Status:";
			// 
			// ShipmentTypeDropEdit
			// 
			this.shipmentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.shipmentTypeDropEdit, "CS_ShipmentTypeForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_ShipmentTypeForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.ShipmentTypeList)));
			this.shipmentTypeDropEdit.BindToList = "Lookups+ShipmentTypeList";
			this.shipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 176, true);
			this.shipmentTypeDropEdit.Name = "ShipmentTypeDropEdit";
			this.shipmentTypeDropEdit.ShowDescriptionBox = false;
			this.shipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.shipmentTypeDropEdit.TabIndex = 10;
			// 
			// shipmentTypeLabel
			// 
			this.shipmentTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 175, true);
			this.shipmentTypeLabel.Name = "shipmentTypeLabel";
			this.shipmentTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.shipmentTypeLabel.TabIndex = 18;
			this.shipmentTypeLabel.Text = "Shp Type:";
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.UnitOfWeightList)));
			this.WeightCalcDropEdit.BindToAmount = "CS_Weight";
			this.WeightCalcDropEdit.BindToList = "Lookups+UnitOfWeightList";
			this.WeightCalcDropEdit.BindToUnit = "CS_WeightUQ";
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 105, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.WeightCalcDropEdit.TabIndex = 3;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// PiecesManifestedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PiecesManifestedCalcEdit, "CS_PiecesManifested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_PiecesManifested)));
			this.PiecesManifestedCalcEdit.DecimalPlaces = 2;
			this.PiecesManifestedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 104, true);
			this.PiecesManifestedCalcEdit.Name = "PiecesManifestedCalcEdit";
			this.PiecesManifestedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.PiecesManifestedCalcEdit.TabIndex = 4;
			this.PiecesManifestedCalcEdit.Text = "0";
			this.PiecesManifestedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DestinationFindBox
			// 
			this.DestinationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationFindBox, "CS_RL_NKDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_RL_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.DestinationList)));
			this.DestinationFindBox.BindToList = "Lookups+DestinationList";
			this.DestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 82, true);
			this.DestinationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.DestinationFindBox.Name = "DestinationFindBox";
			this.DestinationFindBox.PreBoundMaxLength = 5;
			this.DestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.DestinationFindBox.TabIndex = 2;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginFindBox, "CS_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_RL_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).Lookups.OriginList)));
			this.OriginFindBox.BindToList = "Lookups+OriginList";
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 58, true);
			this.OriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.OriginFindBox.Name = "OriginFindBox";
			this.OriginFindBox.PreBoundMaxLength = 5;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.OriginFindBox.TabIndex = 1;
			// 
			// HouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.HouseBillTextBox, "CS_HAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_HAWB)));
			this.HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 11, true);
			this.HouseBillTextBox.Name = "HouseBillTextBox";
			this.HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.HouseBillTextBox.TabIndex = 0;
			// 
			// DestLabel
			// 
			this.DestLabel.AutoSize = true;
			this.DestLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 85, true);
			this.DestLabel.Name = "DestLabel";
			this.DestLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.DestLabel.TabIndex = 5;
			this.DestLabel.Text = "Dest:";
			// 
			// OriginLabel
			// 
			this.OriginLabel.AutoSize = true;
			this.OriginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.OriginLabel.Name = "OriginLabel";
			this.OriginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			this.OriginLabel.TabIndex = 3;
			this.OriginLabel.Text = "Origin:";
			// 
			// HouseBillLabel
			// 
			this.HouseBillLabel.AutoSize = true;
			this.HouseBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.HouseBillLabel.Name = "HouseBillLabel";
			this.HouseBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.HouseBillLabel.TabIndex = 0;
			this.HouseBillLabel.Text = "House Bill:";
			// 
			// PrepaidCollectLabel
			// 
			this.BindingSource.SetBindingMember(this.PrepaidCollectLabel, "CS_PaymentTypeCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).CS_PaymentTypeCaption)));
			this.PrepaidCollectLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 172, true);
			this.PrepaidCollectLabel.Name = "PrepaidCollectLabel";
			this.PrepaidCollectLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 27, true);
			this.PrepaidCollectLabel.TabIndex = 16;
			this.PrepaidCollectLabel.Text = "Freight Paid:";
			// 
			// GoodsValLabel
			// 
			this.GoodsValLabel.AutoSize = true;
			this.GoodsValLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 155, true);
			this.GoodsValLabel.Name = "GoodsValLabel";
			this.GoodsValLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.GoodsValLabel.TabIndex = 13;
			this.GoodsValLabel.Text = "Goods Val:";
			// 
			// WeightLabel
			// 
			this.WeightLabel.AutoSize = true;
			this.WeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 108, true);
			this.WeightLabel.Name = "WeightLabel";
			this.WeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.WeightLabel.TabIndex = 7;
			this.WeightLabel.Text = "Weight:";
			// 
			// PackagesLabel
			// 
			this.PackagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 104, true);
			this.PackagesLabel.Name = "PackagesLabel";
			this.PackagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.PackagesLabel.TabIndex = 9;
			this.PackagesLabel.Text = "Packs:";
			// 
			// UnderbondStatusLabel
			// 
			this.underbondStatusLabel.AutoSize = true;
			this.underbondStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 58, true);
			this.underbondStatusLabel.Name = "UnderbondStatusLabel";
			this.underbondStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.underbondStatusLabel.TabIndex = 30;
			this.underbondStatusLabel.Text = "Underbond Status:";
			// 
			// CMRBaseHouseDetailsUserControl
			// 
			this.Controls.Add(this.HouseGroupBox);
			this.Name = "CMRBaseHouseDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 243, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HouseGroupBox.ResumeLayout(false);
			this.HouseGroupBox.PerformLayout();
			this.GoodsDescriptionTextBox.ResumeLayout(true);
			this.GoodsDescriptionTextBox.PerformLayout();
			this.NonCustomsItemsGroupBox.ResumeLayout(false);
			this.NonCustomsItemsGroupBox.PerformLayout();
			this.ServiceLevelCodeFindBox.ResumeLayout(true);
			this.ServiceLevelCodeFindBox.PerformLayout();
			this.PrepaidCollectDropEdit.ResumeLayout(true);
			this.PrepaidCollectDropEdit.PerformLayout();
			this.goodsValueCalcDropEdit.ResumeLayout(true);
			this.goodsValueCalcDropEdit.PerformLayout();
			this.shipmentTypeDropEdit.ResumeLayout(true);
			this.shipmentTypeDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.DestinationFindBox.ResumeLayout(true);
			this.DestinationFindBox.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZTextBox underbondStatusTextBox;
		private ZLabel underbondStatusLabel;
		protected ZLabel responsiblePartyLabel;
		protected ZTextBox responsiblePartyTextBox;
		private ZButton populateABNButton;
		internal ZTextBox conRefTextBox;
		protected internal ZLabel ConRefLabel;
		protected internal ZTextBox MessageStatusTextBox;
		protected internal ZLabel WeightUnitLabel;
		protected internal ZTextBox TranshipmentTextBox;
		protected internal ZLabel TranshipmentLabel;
		private ZButton detailsButton;
		protected internal ZGroupBox HouseGroupBox;
		protected ZDropEdit shipmentTypeDropEdit;
		protected ZLabel shipmentTypeLabel;
		protected internal ZCalcDropEdit WeightCalcDropEdit;
		protected internal ZCalcEdit PiecesManifestedCalcEdit;
		protected internal Customs.GUI.LongTextControl GoodsDescriptionTextBox;
		protected internal ZCodeFindBox DestinationFindBox;
		protected internal ZCodeFindBox OriginFindBox;
		protected internal ZTextBox HouseBillTextBox;
		protected internal ZLabel HouseBillLabel;
		protected internal ZLabel PrepaidCollectLabel;
		protected internal ZDropEdit PrepaidCollectDropEdit;
		protected internal ZTextBox StatusTextBox;
		private ZLabel statusLabel;
		protected internal ZLabel DestLabel;
		protected internal ZLabel OriginLabel;
		protected ZCalcDropEdit goodsValueCalcDropEdit;
		protected internal ZLabel GoodsValLabel;
		protected internal ZLabel WeightLabel;
		protected internal ZCheckBox SACCheckBox;
		protected internal ZLabel PackagesLabel;
		protected internal ZCheckBox PersonalEffectsCheckBox;
		protected internal ZGroupBox NonCustomsItemsGroupBox;
		protected internal ZCodeFindBox ServiceLevelCodeFindBox;
		protected internal ZLabel zLabel1;
		protected internal ZTextBox WarehouseLocationTextBox;
		protected internal ZTextBox FolioReferenceTextBox;
		protected internal ZLabel WarehouseLocationLabel;
		protected internal ZLabel zLabel33;
		protected internal ZLabel ChargableWeightLabel;
		protected internal ZCalcEdit ChargeableWeightCalcEdit;
		protected internal ZLabel GoodsDescriptionLabel;
		internal ZLabel zLabel4;
		private System.ComponentModel.Container components = null;
	}
}
