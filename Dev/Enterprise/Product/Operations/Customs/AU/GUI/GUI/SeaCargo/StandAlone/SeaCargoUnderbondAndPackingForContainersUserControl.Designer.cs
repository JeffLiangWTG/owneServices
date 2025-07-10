namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoUnderbondAndPackingForContainersUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackingHouseGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GroupBoxContainers.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.detailsTabControl.SuspendLayout();
			this.packingDetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingHouseGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// SACCheckBox
			// 
			this.SACCheckBox.BindTo = "Containers.Pivots.CV_IsSAC";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_IsSAC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_IsSACInfo)));
			this.SACCheckBox.Name = "SACCheckBox";
			this.SACCheckBox.TabIndex = 15;
			// 
			// NetWeightBoundCalcEdit
			// 
			this.NetWeightBoundCalcEdit.BindTo = "Containers.Pivots.CV_NetWeight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_NetWeight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_NetWeightInfo)));
			this.NetWeightBoundCalcEdit.Name = "NetWeightBoundCalcEdit";
			this.NetWeightBoundCalcEdit.TabIndex = 3;
			this.ContainerControlToolTip.SetToolTip(this.NetWeightBoundCalcEdit, "Net weight of goods in selected container\n(in cubic metres)");
			// 
			// GrossWeightLabel
			// 
			this.GrossWeightLabel.Name = "GrossWeightLabel";
			this.GrossWeightLabel.TabIndex = 4;
			// 
			// GroupBoxContainers
			// 
			this.GroupBoxContainers.Controls.Add(this.PackingHouseGrid);
			this.GroupBoxContainers.Name = "GroupBoxContainers";
			this.GroupBoxContainers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 136, true);
			this.GroupBoxContainers.Controls.SetChildIndex(this.ContainersGrid, 0);
			this.GroupBoxContainers.Controls.SetChildIndex(this.PackingHouseGrid, 0);
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.BindTo = "Containers.Pivots";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)));
			zDropEditColumnStyleInfo1.BindToList = "CV_OceanBillHouseBills_List";
			zDropEditColumnStyleInfo1.Caption = "Associated House";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CV_AssociatedHouse";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 117, true);
			this.ContainersGrid.Visible = false;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_AssociatedContainerInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_AssociatedContainer)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_OceanBillContainers_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_SealNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_SealNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_ContainerModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_ContainerMode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_ContainerMode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_ContainerTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_ContainerType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_ContainerType_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_ShipperOwnedContainer)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CN_ShipperOwnedContainerInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_ContainerShipmentStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_ContainerShipmentStatus)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_AssociatedHouseInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_AssociatedHouse)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_OceanBillHouseBills_List)));
			// 
			// DetailsTabControl
			// 
			this.detailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.detailsTabControl.Name = "DetailsTabControl";
			this.detailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 168, true);
			this.detailsTabControl.TabIndex = 2;
			// 
			// PackingDetailsTabPage
			// 
			this.packingDetailsTabPage.Name = "PackingDetailsTabPage";
			this.packingDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 141, true);
			// 
			// UnderbondMovementTabPage
			// 
			this.underbondMovementTabPage.Name = "UnderbondMovementTabPage";
			// 
			// CV_MarksAndNumbersBoundTextBox
			// 
			this.CV_MarksAndNumbersBoundTextBox.BindTo = "Containers.Pivots.CV_MarksAndNumbers";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_MarksAndNumbersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_MarksAndNumbers)));
			this.CV_MarksAndNumbersBoundTextBox.Name = "CV_MarksAndNumbersBoundTextBox";
			this.CV_MarksAndNumbersBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 40, true);
			this.CV_MarksAndNumbersBoundTextBox.TabIndex = 17;
			this.ContainerControlToolTip.SetToolTip(this.CV_MarksAndNumbersBoundTextBox, "Marks and numbers related to the selected container");
			// 
			// CV_GoodsDescriptionBoundTextBox
			// 
			this.CV_GoodsDescriptionBoundTextBox.BindTo = "Containers.Pivots.CV_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_GoodsDescription)));
			this.CV_GoodsDescriptionBoundTextBox.Name = "CV_GoodsDescriptionBoundTextBox";
			this.CV_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 40, true);
			this.CV_GoodsDescriptionBoundTextBox.TabIndex = 9;
			this.ContainerControlToolTip.SetToolTip(this.CV_GoodsDescriptionBoundTextBox, "Description of the goods in the selected container");
			// 
			// CV_VolumeBoundCalcEdit
			// 
			this.CV_VolumeBoundCalcEdit.BindTo = "Containers.Pivots.CV_Volume";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_Volume)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_VolumeInfo)));
			this.CV_VolumeBoundCalcEdit.Name = "CV_VolumeBoundCalcEdit";
			this.CV_VolumeBoundCalcEdit.TabIndex = 7;
			this.ContainerControlToolTip.SetToolTip(this.CV_VolumeBoundCalcEdit, "Net volume of goods in selected container\n(in cubic metres)");
			// 
			// CV_WeightBoundCalcDropEdit
			// 
			this.CV_WeightBoundCalcDropEdit.BindToAmount = "Containers.Pivots.CV_Weight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_WeightInfo)));
			this.CV_WeightBoundCalcDropEdit.BindToList = "Containers.Pivots.CV_WeightUQ_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_WeightUQ_List)));
			this.CV_WeightBoundCalcDropEdit.BindToUnit = "Containers.Pivots.CV_WeightUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_WeightUQ)));
			this.CV_WeightBoundCalcDropEdit.Name = "CV_WeightBoundCalcDropEdit";
			this.CV_WeightBoundCalcDropEdit.TabIndex = 5;
			this.ContainerControlToolTip.SetToolTip(this.CV_WeightBoundCalcDropEdit, "Net weight of goods in selected container");
			// 
			// CV_PackageCountBoundCalcDropEdit
			// 
			this.CV_PackageCountBoundCalcDropEdit.BindToAmount = "Containers.Pivots.CV_PackageCount";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PackageCount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PackageCountInfo)));
			this.CV_PackageCountBoundCalcDropEdit.BindToList = "Containers.Pivots.Lookups+PackageTypes";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).Lookups.PackageTypes)));
			this.CV_PackageCountBoundCalcDropEdit.BindToUnit = "Containers.Pivots.CV_PackageType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PackageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PackageType)));
			this.CV_PackageCountBoundCalcDropEdit.Name = "CV_PackageCountBoundCalcDropEdit";
			this.CV_PackageCountBoundCalcDropEdit.TabIndex = 1;
			this.ContainerControlToolTip.SetToolTip(this.CV_PackageCountBoundCalcDropEdit, "Quantity of packages being shipped in current container");
			// 
			// CV_FumigationCertBoundCheckBox
			// 
			this.CV_FumigationCertBoundCheckBox.BindTo = "Containers.Pivots.CV_FumigationCert";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_FumigationCert)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_FumigationCertInfo)));
			this.CV_FumigationCertBoundCheckBox.Name = "CV_FumigationCertBoundCheckBox";
			this.CV_FumigationCertBoundCheckBox.TabIndex = 11;
			this.ContainerControlToolTip.SetToolTip(this.CV_FumigationCertBoundCheckBox, "Do the goods require a fumigation certificate?");
			// 
			// CV_TimberBoundCheckBox
			// 
			this.CV_TimberBoundCheckBox.BindTo = "Containers.Pivots.CV_Timber";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_Timber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_TimberInfo)));
			this.CV_TimberBoundCheckBox.Name = "CV_TimberBoundCheckBox";
			this.CV_TimberBoundCheckBox.TabIndex = 13;
			this.ContainerControlToolTip.SetToolTip(this.CV_TimberBoundCheckBox, "Does the shipment contain timber?");
			// 
			// CV_HazardousGoodsBoundCheckBox
			// 
			this.CV_HazardousGoodsBoundCheckBox.BindTo = "Containers.Pivots.CV_HazardousGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_HazardousGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_HazardousGoodsInfo)));
			this.CV_HazardousGoodsBoundCheckBox.Name = "CV_HazardousGoodsBoundCheckBox";
			this.CV_HazardousGoodsBoundCheckBox.TabIndex = 10;
			this.ContainerControlToolTip.SetToolTip(this.CV_HazardousGoodsBoundCheckBox, "Does the shipment contain hazardous materials");
			// 
			// CV_PersonalEffectsBoundCheckBox
			// 
			this.CV_PersonalEffectsBoundCheckBox.BindTo = "Containers.Pivots.CV_PersonalEffects";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PersonalEffects)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PersonalEffectsInfo)));
			this.CV_PersonalEffectsBoundCheckBox.Name = "CV_PersonalEffectsBoundCheckBox";
			this.CV_PersonalEffectsBoundCheckBox.TabIndex = 12;
			this.ContainerControlToolTip.SetToolTip(this.CV_PersonalEffectsBoundCheckBox, "Are the goods personal effects");
			// 
			// CV_FlammableBoundCheckBox
			// 
			this.CV_FlammableBoundCheckBox.BindTo = "Containers.Pivots.CV_Flammable";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_Flammable)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_FlammableInfo)));
			this.CV_FlammableBoundCheckBox.Name = "CV_FlammableBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_FlammableBoundCheckBox, "Are the goods flammable?");
			// 
			// CV_PerishableGoodsBoundCheckBox
			// 
			this.CV_PerishableGoodsBoundCheckBox.BindTo = "Containers.Pivots.CV_PerishableGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PerishableGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PerishableGoodsInfo)));
			this.CV_PerishableGoodsBoundCheckBox.Name = "CV_PerishableGoodsBoundCheckBox";
			this.CV_PerishableGoodsBoundCheckBox.TabIndex = 14;
			this.ContainerControlToolTip.SetToolTip(this.CV_PerishableGoodsBoundCheckBox, "Are the goods perishable?");
			// 
			// zLabel34
			// 
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.TabIndex = 6;
			// 
			// zLabel35
			// 
			this.zLabel35.Name = "zLabel35";
			this.zLabel35.TabIndex = 2;
			// 
			// zLabel37
			// 
			this.zLabel37.Name = "zLabel37";
			this.zLabel37.TabIndex = 0;
			// 
			// zLabel38
			// 
			this.zLabel38.Name = "zLabel38";
			this.zLabel38.TabIndex = 16;
			// 
			// zLabel39
			// 
			this.zLabel39.Name = "zLabel39";
			this.zLabel39.TabIndex = 8;
			// 
			// splitter1
			// 
			this.splitter1.Enabled = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.splitter1.Name = "splitter1";
			// 
			// UnderbondMovementControl
			// 
			this.underbondMovementControl.Name = "UnderbondMovementControl";
			// 
			// PackingHouseGrid
			// 
			this.PackingHouseGrid.AllowNavigation = false;
			this.PackingHouseGrid.BindTo = "Containers.Pivots";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)));
			this.PackingHouseGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "CV_OceanBillHouseBills_List";
			zDropEditColumnStyleInfo2.Caption = "Associated House Bill";
			zDropEditColumnStyleInfo2.ColumnName = "CV_AssociatedHouse";
			zDropEditColumnStyleInfo2.MaxDropDownItems = 12;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Package Count";
			zCalcEditColumnStyleInfo1.ColumnName = "CV_PackageCount";
			zDropEditColumnStyleInfo3.BindToList = "Lookups+PackageTypes";
			zDropEditColumnStyleInfo3.Caption = "Package Type";
			zDropEditColumnStyleInfo3.ColumnName = "CV_PackageType";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Weight";
			zCalcEditColumnStyleInfo2.ColumnName = "CV_Weight";
			zDropEditColumnStyleInfo4.BindToList = "CV_WeightUQ_List";
			zDropEditColumnStyleInfo4.Caption = "Weight Unit";
			zDropEditColumnStyleInfo4.ColumnName = "CV_WeightUQ";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Volume";
			zCalcEditColumnStyleInfo3.ColumnName = "CV_Volume";
			zCheckBoxColumnStyleInfo1.Caption = "Hazardous Goods";
			zCheckBoxColumnStyleInfo1.ColumnName = "CV_HazardousGoods";
			zCheckBoxColumnStyleInfo2.Caption = "Fumigation Cert";
			zCheckBoxColumnStyleInfo2.ColumnName = "CV_FumigationCert";
			zCheckBoxColumnStyleInfo3.Caption = "Is SAC";
			zCheckBoxColumnStyleInfo3.ColumnName = "CV_IsSAC";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Caption = "Flammable";
			zCheckBoxColumnStyleInfo4.ColumnName = "CV_Flammable";
			zCheckBoxColumnStyleInfo5.Caption = "Personal Effects";
			zCheckBoxColumnStyleInfo5.ColumnName = "CV_PersonalEffects";
			zCheckBoxColumnStyleInfo6.Caption = "Perishable Goods";
			zCheckBoxColumnStyleInfo6.ColumnName = "CV_PerishableGoods";
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo7.Caption = "Timber";
			zCheckBoxColumnStyleInfo7.ColumnName = "CV_Timber";
			zCheckBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Goods Description";
			zTextBoxColumnStyleInfo1.ColumnName = "CV_GoodsDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.Caption = "Marks And Numbers";
			zTextBoxColumnStyleInfo2.ColumnName = "CV_MarksAndNumbers";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.PackingHouseGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackingHouseGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackingHouseGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackingHouseGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackingHouseGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackingHouseGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackingHouseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PackingHouseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PackingHouseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.PackingHouseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.PackingHouseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.PackingHouseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.PackingHouseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.PackingHouseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackingHouseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackingHouseGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingHouseGrid.EnableToolTips = false;
			this.PackingHouseGrid.GridId = "99506348-5032-49c9-9414-e1d8373a56bf";
			this.PackingHouseGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingHouseGrid.LayoutKey = "zGrid1";
			this.PackingHouseGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingHouseGrid.Name = "PackingHouseGrid";
			this.PackingHouseGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 117, true);
			this.PackingHouseGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_AssociatedHouseInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_AssociatedHouse)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_OceanBillHouseBills_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PackageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PackageType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).Lookups.PackageTypes)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_WeightUQ)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_WeightUQ_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_HazardousGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_HazardousGoodsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_FumigationCert)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_FumigationCertInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_IsSAC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_IsSACInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_Flammable)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_FlammableInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PersonalEffects)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PersonalEffectsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PerishableGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_PerishableGoodsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_Timber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_TimberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_GoodsDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_MarksAndNumbersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)))).Pivots)))).CV_MarksAndNumbers)));
			// 
			// SeaCargoUnderbondAndPackingForContainersUserControl
			// 
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill";
			this.Name = "SeaCargoUnderbondAndPackingForContainersUserControl";
			this.GroupBoxContainers.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.detailsTabControl.ResumeLayout(false);
			this.packingDetailsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackingHouseGrid)).EndInit();
			this.ResumeLayout(false);
		}

		protected internal ZArchitecture.ZGrid PackingHouseGrid;
	}
}
