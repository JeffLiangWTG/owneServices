namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class SeaCargoUnderbondAndPackingStandAloneUserControl
	{
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.GroupBoxContainers.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.detailsTabControl.SuspendLayout();
			this.packingDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// SACCheckBox
			// 
			this.SACCheckBox.BindTo = "Pivot.CV_IsSAC";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_IsSAC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_IsSACInfo)));
			this.SACCheckBox.Name = "SACCheckBox";
			// 
			// NetWeightBoundCalcEdit
			// 
			this.NetWeightBoundCalcEdit.BindTo = "Pivot.CV_NetWeight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_NetWeight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_NetWeightInfo)));
			this.NetWeightBoundCalcEdit.Name = "NetWeightBoundCalcEdit";
			this.ContainerControlToolTip.SetToolTip(this.NetWeightBoundCalcEdit, "Net weight of goods in selected container\n(in cubic metres)");
			// 
			// GrossWeightLabel
			// 
			this.GrossWeightLabel.Name = "GrossWeightLabel";
			// 
			// GroupBoxContainers
			// 
			this.GroupBoxContainers.Name = "GroupBoxContainers";
			this.GroupBoxContainers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 64, true);
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.BindTo = "Pivot";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)));
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 45, true);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_AssociatedContainerInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_AssociatedContainer)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_OceanBillContainers_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_SealNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_SealNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_ContainerModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_ContainerMode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_ContainerMode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_ContainerTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_ContainerType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_ContainerType_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_ShipperOwnedContainer)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CN_ShipperOwnedContainerInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_ContainerShipmentStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_ContainerShipmentStatus)));
			ContainersGrid.GetColumnStyle("CN_SealNumber").IsReadOnly = true;
			ContainersGrid.GetColumnStyle("CN_ContainerMode").IsReadOnly = true;
			ContainersGrid.GetColumnStyle("CN_ContainerType").IsReadOnly = true;
			ContainersGrid.GetColumnStyle("CN_ShipperOwnedContainer").IsReadOnly = true;
			// 
			// DetailsTabControl
			// 
			this.detailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.detailsTabControl.Name = "DetailsTabControl";
			this.detailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 160, true);
			// 
			// PackingDetailsTabPage
			// 
			this.packingDetailsTabPage.Name = "PackingDetailsTabPage";
			this.packingDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 133, true);
			// 
			// UnderbondMovementTabPage
			// 
			this.underbondMovementTabPage.Name = "UnderbondMovementTabPage";
			// 
			// CV_MarksAndNumbersBoundTextBox
			// 
			this.CV_MarksAndNumbersBoundTextBox.BindTo = "Pivot.CV_MarksAndNumbers";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_MarksAndNumbersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_MarksAndNumbers)));
			this.CV_MarksAndNumbersBoundTextBox.Name = "CV_MarksAndNumbersBoundTextBox";
			this.CV_MarksAndNumbersBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 32, true);
			this.ContainerControlToolTip.SetToolTip(this.CV_MarksAndNumbersBoundTextBox, "Marks and numbers related to the selected container");
			// 
			// CV_GoodsDescriptionBoundTextBox
			// 
			this.CV_GoodsDescriptionBoundTextBox.BindTo = "Pivot.CV_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_GoodsDescription)));
			this.CV_GoodsDescriptionBoundTextBox.Name = "CV_GoodsDescriptionBoundTextBox";
			this.CV_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 32, true);
			this.ContainerControlToolTip.SetToolTip(this.CV_GoodsDescriptionBoundTextBox, "Description of the goods in the selected container");
			// 
			// CV_VolumeBoundCalcEdit
			// 
			this.CV_VolumeBoundCalcEdit.BindTo = "Pivot.CV_Volume";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_Volume)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_VolumeInfo)));
			this.CV_VolumeBoundCalcEdit.Name = "CV_VolumeBoundCalcEdit";
			this.ContainerControlToolTip.SetToolTip(this.CV_VolumeBoundCalcEdit, "Net volume of goods in selected container\n(in cubic metres)");
			// 
			// CV_WeightBoundCalcDropEdit
			// 
			this.CV_WeightBoundCalcDropEdit.BindToAmount = "Pivot.CV_Weight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_WeightInfo)));
			this.CV_WeightBoundCalcDropEdit.BindToList = "Pivot.CV_WeightUQ_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_WeightUQ_List)));
			this.CV_WeightBoundCalcDropEdit.BindToUnit = "Pivot.CV_WeightUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_WeightUQ)));
			this.CV_WeightBoundCalcDropEdit.Name = "CV_WeightBoundCalcDropEdit";
			this.ContainerControlToolTip.SetToolTip(this.CV_WeightBoundCalcDropEdit, "Net weight of goods in selected container");
			// 
			// CV_PackageCountBoundCalcDropEdit
			// 
			this.CV_PackageCountBoundCalcDropEdit.BindToAmount = "Pivot.CV_PackageCount";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PackageCount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PackageCountInfo)));
			this.CV_PackageCountBoundCalcDropEdit.BindToList = "Pivot.Lookups+PackageTypes";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Lookups.PackageTypes)));
			this.CV_PackageCountBoundCalcDropEdit.BindToUnit = "Pivot.CV_PackageType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PackageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PackageType)));
			this.CV_PackageCountBoundCalcDropEdit.Name = "CV_PackageCountBoundCalcDropEdit";
			this.ContainerControlToolTip.SetToolTip(this.CV_PackageCountBoundCalcDropEdit, "Quantity of packages being shipped in current container");
			// 
			// CV_FumigationCertBoundCheckBox
			// 
			this.CV_FumigationCertBoundCheckBox.BindTo = "Pivot.CV_FumigationCert";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_FumigationCert)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_FumigationCertInfo)));
			this.CV_FumigationCertBoundCheckBox.Name = "CV_FumigationCertBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_FumigationCertBoundCheckBox, "Do the goods require a fumigation certificate?");
			// 
			// CV_TimberBoundCheckBox
			// 
			this.CV_TimberBoundCheckBox.BindTo = "Pivot.CV_Timber";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_Timber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_TimberInfo)));
			this.CV_TimberBoundCheckBox.Name = "CV_TimberBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_TimberBoundCheckBox, "Does the shipment contain timber?");
			// 
			// CV_HazardousGoodsBoundCheckBox
			// 
			this.CV_HazardousGoodsBoundCheckBox.BindTo = "Pivot.CV_HazardousGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_HazardousGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_HazardousGoodsInfo)));
			this.CV_HazardousGoodsBoundCheckBox.Name = "CV_HazardousGoodsBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_HazardousGoodsBoundCheckBox, "Does the shipment contain hazardous materials");
			// 
			// CV_PersonalEffectsBoundCheckBox
			// 
			this.CV_PersonalEffectsBoundCheckBox.BindTo = "Pivot.CV_PersonalEffects";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PersonalEffects)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PersonalEffectsInfo)));
			this.CV_PersonalEffectsBoundCheckBox.Name = "CV_PersonalEffectsBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_PersonalEffectsBoundCheckBox, "Are the goods personal effects");
			// 
			// CV_FlammableBoundCheckBox
			// 
			this.CV_FlammableBoundCheckBox.BindTo = "Pivot.CV_Flammable";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_Flammable)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_FlammableInfo)));
			this.CV_FlammableBoundCheckBox.Name = "CV_FlammableBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_FlammableBoundCheckBox, "Are the goods flammable?");
			// 
			// CV_PerishableGoodsBoundCheckBox
			// 
			this.CV_PerishableGoodsBoundCheckBox.BindTo = "Pivot.CV_PerishableGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PerishableGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PerishableGoodsInfo)));
			this.CV_PerishableGoodsBoundCheckBox.Name = "CV_PerishableGoodsBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_PerishableGoodsBoundCheckBox, "Are the goods perishable?");
			// 
			// zLabel34
			// 
			this.zLabel34.Name = "zLabel34";
			// 
			// zLabel35
			// 
			this.zLabel35.Name = "zLabel35";
			// 
			// zLabel37
			// 
			this.zLabel37.Name = "zLabel37";
			// 
			// zLabel38
			// 
			this.zLabel38.Name = "zLabel38";
			// 
			// zLabel39
			// 
			this.zLabel39.Name = "zLabel39";
			// 
			// splitter1
			// 
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 59, true);
			this.splitter1.Name = "splitter1";
			// 
			// UnderbondMovementControl
			// 
			this.underbondMovementControl.Name = "UnderbondMovementControl";
			// 
			// SeaCargoUnderbondAndPackingStandAloneUserControl
			// 
			this.Name = "SeaCargoUnderbondAndPackingStandAloneUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 224, true);
			this.GroupBoxContainers.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.detailsTabControl.ResumeLayout(false);
			this.packingDetailsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		System.ComponentModel.IContainer components = null;
	}
}
