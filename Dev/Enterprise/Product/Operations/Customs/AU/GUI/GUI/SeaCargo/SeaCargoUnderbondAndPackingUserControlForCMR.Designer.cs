using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoUnderbondAndPackingUserControlForCMR
	{
		private void InitializeComponent()
		{
			this.SACCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GrossWeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NetWeightBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GroupBoxContainers.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.detailsTabControl.SuspendLayout();
			this.packingDetailsTabPage.SuspendLayout();
			this.underbondMovementTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// GroupBoxContainers
			// 
			this.GroupBoxContainers.Name = "GroupBoxContainers";
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.Name = "ContainersGrid";
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
			// 
			// DetailsTabControl
			// 
			this.detailsTabControl.Name = "DetailsTabControl";
			this.detailsTabControl.TabIndex = 2;
			// 
			// PackingDetailsTabPage
			// 
			this.packingDetailsTabPage.Controls.Add(this.GrossWeightLabel);
			this.packingDetailsTabPage.Controls.Add(this.NetWeightBoundCalcEdit);
			this.packingDetailsTabPage.Controls.Add(this.SACCheckBox);
			this.packingDetailsTabPage.Name = "PackingDetailsTabPage";
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_FlammableBoundCheckBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_TimberBoundCheckBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.SACCheckBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.zLabel39, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.zLabel38, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.zLabel37, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.zLabel35, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.zLabel34, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_PerishableGoodsBoundCheckBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_PersonalEffectsBoundCheckBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_HazardousGoodsBoundCheckBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_FumigationCertBoundCheckBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_PackageCountBoundCalcDropEdit, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_WeightBoundCalcDropEdit, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_VolumeBoundCalcEdit, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_GoodsDescriptionBoundTextBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.CV_MarksAndNumbersBoundTextBox, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.NetWeightBoundCalcEdit, 0);
			this.packingDetailsTabPage.Controls.SetChildIndex(this.GrossWeightLabel, 0);
			// 
			// UnderbondMovementTabPage
			// 
			this.underbondMovementTabPage.Name = "UnderbondMovementTabPage";
			// 
			// CV_MarksAndNumbersBoundTextBox
			// 
			this.CV_MarksAndNumbersBoundTextBox.Name = "CV_MarksAndNumbersBoundTextBox";
			this.CV_MarksAndNumbersBoundTextBox.TabIndex = 17;
			this.ContainerControlToolTip.SetToolTip(this.CV_MarksAndNumbersBoundTextBox, "Marks and numbers related to the selected container");
			// 
			// CV_GoodsDescriptionBoundTextBox
			// 
			this.CV_GoodsDescriptionBoundTextBox.Name = "CV_GoodsDescriptionBoundTextBox";
			this.CV_GoodsDescriptionBoundTextBox.TabIndex = 9;
			this.ContainerControlToolTip.SetToolTip(this.CV_GoodsDescriptionBoundTextBox, "Description of the goods in the selected container");
			// 
			// CV_VolumeBoundCalcEdit
			// 
			this.CV_VolumeBoundCalcEdit.Name = "CV_VolumeBoundCalcEdit";
			this.CV_VolumeBoundCalcEdit.TabIndex = 7;
			this.ContainerControlToolTip.SetToolTip(this.CV_VolumeBoundCalcEdit, "Net volume of goods in selected container\n(in cubic metres)");
			// 
			// CV_WeightBoundCalcDropEdit
			// 
			this.CV_WeightBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 32, true);
			this.CV_WeightBoundCalcDropEdit.Name = "CV_WeightBoundCalcDropEdit";
			this.CV_WeightBoundCalcDropEdit.TabIndex = 5;
			this.ContainerControlToolTip.SetToolTip(this.CV_WeightBoundCalcDropEdit, "Gross weight of goods in selected container");
			// 
			// CV_PackageCountBoundCalcDropEdit
			// 
			this.CV_PackageCountBoundCalcDropEdit.Name = "CV_PackageCountBoundCalcDropEdit";
			this.ContainerControlToolTip.SetToolTip(this.CV_PackageCountBoundCalcDropEdit, "Quantity of packages being shipped in current container");
			// 
			// CV_FumigationCertBoundCheckBox
			// 
			this.CV_FumigationCertBoundCheckBox.Name = "CV_FumigationCertBoundCheckBox";
			this.CV_FumigationCertBoundCheckBox.TabIndex = 11;
			this.ContainerControlToolTip.SetToolTip(this.CV_FumigationCertBoundCheckBox, "Do the goods require a fumigation certificate?");
			// 
			// CV_TimberBoundCheckBox
			// 
			this.CV_TimberBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 40, true);
			this.CV_TimberBoundCheckBox.Name = "CV_TimberBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_TimberBoundCheckBox, "Does the shipment contain timber?");
			// 
			// CV_HazardousGoodsBoundCheckBox
			// 
			this.CV_HazardousGoodsBoundCheckBox.Name = "CV_HazardousGoodsBoundCheckBox";
			this.CV_HazardousGoodsBoundCheckBox.TabIndex = 10;
			this.ContainerControlToolTip.SetToolTip(this.CV_HazardousGoodsBoundCheckBox, "Does the shipment contain hazardous materials");
			// 
			// CV_PersonalEffectsBoundCheckBox
			// 
			this.CV_PersonalEffectsBoundCheckBox.Name = "CV_PersonalEffectsBoundCheckBox";
			this.CV_PersonalEffectsBoundCheckBox.TabIndex = 12;
			this.ContainerControlToolTip.SetToolTip(this.CV_PersonalEffectsBoundCheckBox, "Are the goods personal effects");
			// 
			// CV_FlammableBoundCheckBox
			// 
			this.CV_FlammableBoundCheckBox.Name = "CV_FlammableBoundCheckBox";
			this.ContainerControlToolTip.SetToolTip(this.CV_FlammableBoundCheckBox, "Are the goods flammable?");
			this.CV_FlammableBoundCheckBox.Visible = false;
			// 
			// CV_PerishableGoodsBoundCheckBox
			// 
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
			this.zLabel35.Text = "Weight - Net:";
			// 
			// zLabel37
			// 
			this.zLabel37.Name = "zLabel37";
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
			this.splitter1.Name = "splitter1";
			// 
			// UnderbondMovementControl
			// 
			this.underbondMovementControl.Name = "UnderbondMovementControl";
			this.underbondMovementControl.Visible = false;
			// 
			// SACCheckBox
			// 
			this.SACCheckBox.BindTo = "Pivot.CV_IsSAC";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_IsSAC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_IsSACInfo)));
			this.SACCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SACCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 40, true);
			this.SACCheckBox.Name = "SACCheckBox";
			this.SACCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 16, true);
			this.SACCheckBox.TabIndex = 15;
			this.SACCheckBox.Text = "Self Assessed Clearance";
			this.SACCheckBox.CheckedChanged += new System.EventHandler(this.SACCheckBox_CheckedChanged);
			// 
			// GrossWeightLabel
			// 
			this.GrossWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 32, true);
			this.GrossWeightLabel.Name = "GrossWeightLabel";
			this.GrossWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.GrossWeightLabel.TabIndex = 4;
			this.GrossWeightLabel.Text = "Gross:";
			// 
			// NetWeightBoundCalcEdit
			// 
			this.NetWeightBoundCalcEdit.BindTo = "Pivot.CV_NetWeight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_NetWeight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_NetWeightInfo)));
			this.NetWeightBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.NetWeightBoundCalcEdit.Name = "NetWeightBoundCalcEdit";
			this.NetWeightBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.NetWeightBoundCalcEdit.TabIndex = 3;
			this.NetWeightBoundCalcEdit.Text = "0.000";
			this.NetWeightBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ContainerControlToolTip.SetToolTip(this.NetWeightBoundCalcEdit, "Net weight of goods in selected container\n(in cubic metres)");
			// 
			// SeaCargoUnderbondAndPackingUserControlForCMR
			// 
			this.Name = "SeaCargoUnderbondAndPackingUserControlForCMR";
			this.GroupBoxContainers.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.detailsTabControl.ResumeLayout(false);
			this.packingDetailsTabPage.ResumeLayout(false);
			this.underbondMovementTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		protected internal ZCheckBox SACCheckBox;
		protected internal ZArchitecture.ZCalcEdit NetWeightBoundCalcEdit;
		protected internal ZArchitecture.ZLabel GrossWeightLabel;
	}
}
