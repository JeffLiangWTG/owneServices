using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class BaseHouseDetailsUserControl
	{
		void InitializeComponent()
		{
			this.HouseGroupBox = new ZGroupBox();
			this.shipmentTypeDropEdit = new ZDropEdit();
			this.shipmentTypeLabel = new ZArchitecture.ZLabel();
			this.ServiceLevelCodeFindBox = new ZCodeFindBox();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.WeightCalcDropEdit = new ZCalcDropEdit();
			this.PiecesManifestedCalcEdit = new ZArchitecture.ZCalcEdit();
			this.GoodsDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.DestinationFindBox = new ZCodeFindBox();
			this.OriginFindBox = new ZCodeFindBox();
			this.HouseBillTextBox = new ZArchitecture.ZTextBox();
			this.MasterHouseBillCheckBox = new ZCheckBox();
			this.zLabel13 = new ZArchitecture.ZLabel();
			this.zLabel14 = new ZArchitecture.ZLabel();
			this.zLabel6 = new ZArchitecture.ZLabel();
			this.WarehouseLocationTextBox = new ZArchitecture.ZTextBox();
			this.FolioReferenceTextBox = new ZArchitecture.ZTextBox();
			this.PrepaidCollectLabel = new ZArchitecture.ZLabel();
			this.zLabel31 = new ZArchitecture.ZLabel();
			this.WarehouseLocationLabel = new ZArchitecture.ZLabel();
			this.zLabel33 = new ZArchitecture.ZLabel();
			this.PrepaidCollectDropEdit = new ZDropEdit();
			this.zLabel12 = new ZArchitecture.ZLabel();
			this.zLabel10 = new ZArchitecture.ZLabel();
			this.zLabel9 = new ZArchitecture.ZLabel();
			this.ChargableWeightLabel = new ZArchitecture.ZLabel();
			this.ChargeableWeightCalcEdit = new ZArchitecture.ZCalcEdit();
			this.WeightUnitLabel = new ZArchitecture.ZLabel();
			this.HouseGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// HouseGroupBox
			// 
			this.HouseGroupBox.Controls.Add(this.shipmentTypeDropEdit);
			this.HouseGroupBox.Controls.Add(this.shipmentTypeLabel);
			this.HouseGroupBox.Controls.Add(this.ServiceLevelCodeFindBox);
			this.HouseGroupBox.Controls.Add(this.zLabel1);
			this.HouseGroupBox.Controls.Add(this.WeightCalcDropEdit);
			this.HouseGroupBox.Controls.Add(this.PiecesManifestedCalcEdit);
			this.HouseGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
			this.HouseGroupBox.Controls.Add(this.DestinationFindBox);
			this.HouseGroupBox.Controls.Add(this.OriginFindBox);
			this.HouseGroupBox.Controls.Add(this.HouseBillTextBox);
			this.HouseGroupBox.Controls.Add(this.MasterHouseBillCheckBox);
			this.HouseGroupBox.Controls.Add(this.zLabel13);
			this.HouseGroupBox.Controls.Add(this.zLabel14);
			this.HouseGroupBox.Controls.Add(this.zLabel6);
			this.HouseGroupBox.Controls.Add(this.WarehouseLocationTextBox);
			this.HouseGroupBox.Controls.Add(this.FolioReferenceTextBox);
			this.HouseGroupBox.Controls.Add(this.PrepaidCollectLabel);
			this.HouseGroupBox.Controls.Add(this.zLabel31);
			this.HouseGroupBox.Controls.Add(this.WarehouseLocationLabel);
			this.HouseGroupBox.Controls.Add(this.zLabel33);
			this.HouseGroupBox.Controls.Add(this.PrepaidCollectDropEdit);
			this.HouseGroupBox.Controls.Add(this.zLabel12);
			this.HouseGroupBox.Controls.Add(this.zLabel10);
			this.HouseGroupBox.Controls.Add(this.zLabel9);
			this.HouseGroupBox.Controls.Add(this.ChargableWeightLabel);
			this.HouseGroupBox.Controls.Add(this.ChargeableWeightCalcEdit);
			this.HouseGroupBox.Controls.Add(this.WeightUnitLabel);
			this.HouseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseGroupBox.Name = "HouseGroupBox";
			this.HouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 224, true);
			this.HouseGroupBox.TabIndex = 0;
			this.HouseGroupBox.TabStop = false;
			this.HouseGroupBox.Text = "Details";
			// 
			// ShipmentTypeDropEdit
			// 
			this.shipmentTypeDropEdit.BindTo = "CS_ShipmentTypeForBinding";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_ShipmentTypeForBindingInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_ShipmentTypeForBinding)));
			this.shipmentTypeDropEdit.BindToList = "Lookups+ShipmentTypeList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusHAWBBase)(null)).Lookups.ShipmentTypeList)));
			this.shipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 195, true);
			this.shipmentTypeDropEdit.Name = "ShipmentTypeDropEdit";
			this.shipmentTypeDropEdit.ShowDescriptionBox = false;
			this.shipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.shipmentTypeDropEdit.TabIndex = 26;
			// 
			// shipmentTypeLabel
			// 
			this.shipmentTypeLabel.AutoSize = true;
			this.shipmentTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 199, true);
			this.shipmentTypeLabel.Name = "shipmentTypeLabel";
			this.shipmentTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 13, true);
			this.shipmentTypeLabel.TabIndex = 25;
			this.shipmentTypeLabel.Text = "Shipment Type:";
			// 
			// ServiceLevelCodeFindBox
			// 
			this.ServiceLevelCodeFindBox.BindTo = "CS_RS_NK_ServiceLevel";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_RS_NK_ServiceLevelInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_RS_NK_ServiceLevel)));
			this.ServiceLevelCodeFindBox.BindToList = "Lookups+RefServiceLevelList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusHAWBBase)(null)).Lookups.RefServiceLevelList)));
			this.ServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 171, true);
			this.ServiceLevelCodeFindBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.ServiceLevelCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.ServiceLevelCodeFindBox.Name = "ServiceLevelCodeFindBox";
			this.ServiceLevelCodeFindBox.PreBoundMaxLength = 3;
			this.ServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.ServiceLevelCodeFindBox.TabIndex = 10;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 174, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.zLabel1.TabIndex = 9;
			this.zLabel1.Text = "Service Level:";
			this.zLabel1.Click += new System.EventHandler(this.zLabel1_Click);
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.BindToAmount = "CS_Weight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((CusHAWBBase)(null)).CS_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_WeightInfo)));
			this.WeightCalcDropEdit.BindToList = "Lookups+UnitOfWeightList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusHAWBBase)(null)).Lookups.UnitOfWeightList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusHAWBBase)(null)).Lookups.UnitOfWeightList)));
			this.WeightCalcDropEdit.BindToUnit = "CS_WeightUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_WeightUQ)));
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 123, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.WeightCalcDropEdit.TabIndex = 7;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// PiecesManifestedCalcEdit
			// 
			this.PiecesManifestedCalcEdit.BindTo = "CS_PiecesManifested";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((CusHAWBBase)(null)).CS_PiecesManifested)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_PiecesManifestedInfo)));
			this.PiecesManifestedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 72, true);
			this.PiecesManifestedCalcEdit.Name = "PiecesManifestedCalcEdit";
			this.PiecesManifestedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.PiecesManifestedCalcEdit.TabIndex = 15;
			this.PiecesManifestedCalcEdit.Text = "0";
			this.PiecesManifestedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.BindTo = "CS_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_GoodsDescription)));
			this.GoodsDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 195, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 12;
			// 
			// DestinationFindBox
			// 
			this.DestinationFindBox.BindTo = "CS_RL_NKDestination";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_RL_NKDestinationInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_RL_NKDestination)));
			this.DestinationFindBox.BindToList = "Lookups+DestinationList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusHAWBBase)(null)).Lookups.DestinationList)));
			this.DestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 75, true);
			this.DestinationFindBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.DestinationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.DestinationFindBox.Name = "DestinationFindBox";
			this.DestinationFindBox.PreBoundMaxLength = 5;
			this.DestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.DestinationFindBox.TabIndex = 5;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.BindTo = "CS_RL_NKOrigin";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_RL_NKOriginInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_RL_NKOrigin)));
			this.OriginFindBox.BindToList = "Lookups+OriginList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusHAWBBase)(null)).Lookups.OriginList)));
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 49, true);
			this.OriginFindBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.OriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.OriginFindBox.Name = "OriginFindBox";
			this.OriginFindBox.PreBoundMaxLength = 5;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.OriginFindBox.TabIndex = 3;
			// 
			// HouseBillTextBox
			// 
			this.HouseBillTextBox.BindTo = "CS_HAWB";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_HAWBInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_HAWB)));
			this.HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 24, true);
			this.HouseBillTextBox.Name = "HouseBillTextBox";
			this.HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.HouseBillTextBox.TabIndex = 1;
			// 
			// MasterHouseBillCheckBox
			// 
			this.MasterHouseBillCheckBox.BindTo = "CS_IsMasterHouse";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZBool)(((CusHAWBBase)(null)).CS_IsMasterHouse)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_IsMasterHouseInfo)));
			this.MasterHouseBillCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MasterHouseBillCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 48, true);
			this.MasterHouseBillCheckBox.Name = "MasterHouseBillCheckBox";
			this.MasterHouseBillCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.MasterHouseBillCheckBox.TabIndex = 13;
			this.MasterHouseBillCheckBox.Text = "Is a Master House?";
			// 
			// zLabel13
			// 
			this.zLabel13.AutoSize = true;
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 78, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.zLabel13.TabIndex = 4;
			this.zLabel13.Text = "Destination:";
			// 
			// zLabel14
			// 
			this.zLabel14.AutoSize = true;
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 53, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			this.zLabel14.TabIndex = 2;
			this.zLabel14.Text = "Origin:";
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 28, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.zLabel6.TabIndex = 0;
			this.zLabel6.Text = "House Bill:";
			// 
			// WarehouseLocationTextBox
			// 
			this.WarehouseLocationTextBox.BindTo = "CS_WarehouseLocation";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_WarehouseLocationInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_WarehouseLocation)));
			this.WarehouseLocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.WarehouseLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 123, true);
			this.WarehouseLocationTextBox.Name = "WarehouseLocationTextBox";
			this.WarehouseLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.WarehouseLocationTextBox.TabIndex = 19;
			// 
			// FolioReferenceTextBox
			// 
			this.FolioReferenceTextBox.BindTo = "CS_FolioReference";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_FolioReferenceInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_FolioReference)));
			this.FolioReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FolioReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 147, true);
			this.FolioReferenceTextBox.Name = "FolioReferenceTextBox";
			this.FolioReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.FolioReferenceTextBox.TabIndex = 21;
			// 
			// PrepaidCollectLabel
			// 
			this.PrepaidCollectLabel.AutoSize = true;
			this.PrepaidCollectLabel.BindTo = "CS_PaymentTypeCaption";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_PaymentTypeCaptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_PaymentTypeCaption)));
			this.PrepaidCollectLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 102, true);
			this.PrepaidCollectLabel.Name = "PrepaidCollectLabel";
			this.PrepaidCollectLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.PrepaidCollectLabel.TabIndex = 16;
			this.PrepaidCollectLabel.Text = "Prepaid or Collect:";
			// 
			// zLabel31
			// 
			this.zLabel31.AutoSize = true;
			this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 150, true);
			this.zLabel31.Name = "zLabel31";
			this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.zLabel31.TabIndex = 8;
			this.zLabel31.Text = "Goods Value:";
			this.zLabel31.Click += new System.EventHandler(this.zLabel31_Click);
			// 
			// WarehouseLocationLabel
			// 
			this.WarehouseLocationLabel.BindTo = "WarehouseLocationCaption";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).WarehouseLocationCaptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).WarehouseLocationCaption)));
			this.WarehouseLocationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 121, true);
			this.WarehouseLocationLabel.Name = "WarehouseLocationLabel";
			this.WarehouseLocationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 24, true);
			this.WarehouseLocationLabel.TabIndex = 18;
			this.WarehouseLocationLabel.Text = "Warehouse Location:";
			// 
			// zLabel33
			// 
			this.zLabel33.AutoSize = true;
			this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 150, true);
			this.zLabel33.Name = "zLabel33";
			this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.zLabel33.TabIndex = 20;
			this.zLabel33.Text = "Folio Reference:";
			// 
			// PrepaidCollectDropEdit
			// 
			this.PrepaidCollectDropEdit.BindTo = "CS_FreightPrepaidCollectForBinding";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_FreightPrepaidCollectForBindingInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).CS_FreightPrepaidCollectForBinding)));
			this.PrepaidCollectDropEdit.BindToList = "Lookups+PrepaidCollectList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusHAWBBase)(null)).Lookups.PrepaidCollectList)));
			this.PrepaidCollectDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 99, true);
			this.PrepaidCollectDropEdit.Name = "PrepaidCollectDropEdit";
			this.PrepaidCollectDropEdit.PreBoundMaxLength = 3;
			this.PrepaidCollectDropEdit.ShowDescriptionBox = false;
			this.PrepaidCollectDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.PrepaidCollectDropEdit.TabIndex = 17;
			// 
			// zLabel12
			// 
			this.zLabel12.AutoSize = true;
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 126, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.zLabel12.TabIndex = 6;
			this.zLabel12.Text = "Weight:";
			// 
			// zLabel10
			// 
			this.zLabel10.AutoSize = true;
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 75, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 13, true);
			this.zLabel10.TabIndex = 14;
			this.zLabel10.Text = "Pieces Manifested:";
			// 
			// zLabel9
			// 
			this.zLabel9.AutoSize = true;
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 198, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.zLabel9.TabIndex = 11;
			this.zLabel9.Text = "Goods Desc:";
			// 
			// ChargableWeightLabel
			// 
			this.ChargableWeightLabel.AutoSize = true;
			this.ChargableWeightLabel.BindTo = "ChargableWeightCaption";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).ChargableWeightCaptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((CusHAWBBase)(null)).ChargableWeightCaption)));
			this.ChargableWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 175, true);
			this.ChargableWeightLabel.Name = "ChargableWeightLabel";
			this.ChargableWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 13, true);
			this.ChargableWeightLabel.TabIndex = 22;
			this.ChargableWeightLabel.Text = "Chargeable Weight:";
			// 
			// ChargeableWeightCalcEdit
			// 
			this.ChargeableWeightCalcEdit.BindTo = "CS_ChargableWeight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((CusHAWBBase)(null)).CS_ChargableWeight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusHAWBBase)(null)).CS_ChargableWeightInfo)));
			this.ChargeableWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 171, true);
			this.ChargeableWeightCalcEdit.Name = "ChargeableWeightCalcEdit";
			this.ChargeableWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ChargeableWeightCalcEdit.TabIndex = 23;
			this.ChargeableWeightCalcEdit.Text = "0.000";
			this.ChargeableWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeightUnitLabel
			// 
			this.WeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 175, true);
			this.WeightUnitLabel.Name = "WeightUnitLabel";
			this.WeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 13, true);
			this.WeightUnitLabel.TabIndex = 24;
			this.WeightUnitLabel.Text = "KG";
			// 
			// BaseHouseDetailsUserControl
			// 
			this.Controls.Add(this.HouseGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.AirCargo.Business.CusHAWBBase";
			this.Name = "BaseHouseDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 224, true);
			this.HouseGroupBox.ResumeLayout(false);
			this.HouseGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		protected internal ZGroupBox HouseGroupBox;
		protected internal ZCalcDropEdit WeightCalcDropEdit;
		protected internal ZArchitecture.ZCalcEdit PiecesManifestedCalcEdit;
		protected internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		protected internal ZCodeFindBox DestinationFindBox;
		protected internal ZCodeFindBox OriginFindBox;
		protected internal ZArchitecture.ZTextBox HouseBillTextBox;
		protected internal ZCheckBox MasterHouseBillCheckBox;
		protected internal ZArchitecture.ZLabel zLabel13;
		protected internal ZArchitecture.ZLabel zLabel14;
		protected internal ZArchitecture.ZLabel zLabel6;
		protected internal ZArchitecture.ZTextBox WarehouseLocationTextBox;
		protected internal ZArchitecture.ZTextBox FolioReferenceTextBox;
		protected internal ZArchitecture.ZLabel zLabel31;
		protected internal ZArchitecture.ZLabel zLabel33;
		protected internal ZDropEdit PrepaidCollectDropEdit;
		protected internal ZArchitecture.ZLabel zLabel12;
		protected internal ZArchitecture.ZLabel zLabel10;
		protected internal ZArchitecture.ZLabel zLabel1;
		protected internal ZArchitecture.ZLabel WeightUnitLabel;
		protected internal ZCodeFindBox ServiceLevelCodeFindBox;
		protected internal ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit;
		protected internal ZArchitecture.ZLabel WarehouseLocationLabel;
		protected internal ZArchitecture.ZLabel ChargableWeightLabel;
		protected internal ZArchitecture.ZLabel PrepaidCollectLabel;
		protected internal ZDropEdit shipmentTypeDropEdit;
		protected ZArchitecture.ZLabel shipmentTypeLabel;
		protected internal ZArchitecture.ZLabel zLabel9;
	}
}
