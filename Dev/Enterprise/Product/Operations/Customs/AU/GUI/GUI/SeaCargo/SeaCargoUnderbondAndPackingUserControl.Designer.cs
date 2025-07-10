using Enterprise.ZArchitecture.GUI;
using System.ComponentModel;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoUnderbondAndPackingUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.GroupBoxContainers = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.detailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.packingDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CV_WeightBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CV_MarksAndNumbersBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CV_GoodsDescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CV_VolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CV_PackageCountBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CV_FumigationCertBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CV_TimberBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CV_HazardousGoodsBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CV_PersonalEffectsBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CV_FlammableBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CV_PerishableGoodsBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel35 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel37 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel38 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel39 = new Enterprise.ZArchitecture.ZLabel();
			this.underbondMovementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.underbondMovementControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoHouseUnderbondControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.GroupBoxContainers.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.detailsTabControl.SuspendLayout();
			this.packingDetailsTabPage.SuspendLayout();
			this.underbondMovementTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// GroupBoxContainers
			// 
			this.GroupBoxContainers.Controls.Add(this.ContainersGrid);
			this.GroupBoxContainers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBoxContainers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBoxContainers.Name = "GroupBoxContainers";
			this.GroupBoxContainers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 107, true);
			this.GroupBoxContainers.TabIndex = 0;
			this.GroupBoxContainers.TabStop = false;
			this.GroupBoxContainers.Text = "Packing";
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.ContainersGrid.BindTo = "Pivot";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)));
			this.ContainersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "CV_OceanBillContainers_List";
			zDropEditColumnStyleInfo1.Caption = "Container Number";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CV_AssociatedContainer";
			zDropEditColumnStyleInfo1.ToolTip = "Associated Container.  Select one from the list, or add a new container by enteri" +
				"ng the container number.  To enter Bulk or Break Bulk, enter BULK or BBK";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.Caption = "Seal Number";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CN_SealNumber";
			zTextBoxColumnStyleInfo1.ToolTip = "The seal number of the container";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "CV_ContainerMode_List";
			zDropEditColumnStyleInfo2.Caption = "Mode";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CN_ContainerMode";
			zDropEditColumnStyleInfo2.ToolTip = "Select the mode of the container";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.BindToList = "CV_ContainerType_List";
			zCodeFindBoxColumnStyleInfo1.Caption = "Type";
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CN_ContainerType";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefContainer;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Select the Container Type";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.Caption = "Shipper Owned";
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zCheckBoxColumnStyleInfo1.ColumnName = "CN_ShipperOwnedContainer";
			zCheckBoxColumnStyleInfo1.ToolTip = "Does the shipper own this container?";
			zTextBoxColumnStyleInfo2.Caption = "Status";
			zTextBoxColumnStyleInfo2.ColumnName = "CV_ContainerShipmentStatus";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "ac89356b-0c55-4501-a3ae-1f501e2aa80a";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 88, true);
			this.ContainersGrid.TabIndex = 0;
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
			this.detailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.detailsTabControl.Controls.Add(this.packingDetailsTabPage);
			this.detailsTabControl.Controls.Add(this.underbondMovementTabPage);
			this.detailsTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.detailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.detailsTabControl.Name = "DetailsTabControl";
			this.detailsTabControl.SelectedIndex = 0;
			this.detailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 150, true);
			this.detailsTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 150, true);
			this.detailsTabControl.TabIndex = 11;
			// 
			// PackingDetailsTabPage
			// 
			this.packingDetailsTabPage.Controls.Add(this.CV_WeightBoundCalcDropEdit);
			this.packingDetailsTabPage.Controls.Add(this.CV_MarksAndNumbersBoundTextBox);
			this.packingDetailsTabPage.Controls.Add(this.CV_GoodsDescriptionBoundTextBox);
			this.packingDetailsTabPage.Controls.Add(this.CV_VolumeBoundCalcEdit);
			this.packingDetailsTabPage.Controls.Add(this.CV_PackageCountBoundCalcDropEdit);
			this.packingDetailsTabPage.Controls.Add(this.CV_FumigationCertBoundCheckBox);
			this.packingDetailsTabPage.Controls.Add(this.CV_TimberBoundCheckBox);
			this.packingDetailsTabPage.Controls.Add(this.CV_HazardousGoodsBoundCheckBox);
			this.packingDetailsTabPage.Controls.Add(this.CV_PersonalEffectsBoundCheckBox);
			this.packingDetailsTabPage.Controls.Add(this.CV_FlammableBoundCheckBox);
			this.packingDetailsTabPage.Controls.Add(this.CV_PerishableGoodsBoundCheckBox);
			this.packingDetailsTabPage.Controls.Add(this.zLabel34);
			this.packingDetailsTabPage.Controls.Add(this.zLabel35);
			this.packingDetailsTabPage.Controls.Add(this.zLabel37);
			this.packingDetailsTabPage.Controls.Add(this.zLabel38);
			this.packingDetailsTabPage.Controls.Add(this.zLabel39);
			this.packingDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.packingDetailsTabPage.Name = "PackingDetailsTabPage";
			this.packingDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 165, true);
			this.packingDetailsTabPage.TabIndex = 0;
			this.packingDetailsTabPage.Text = "Packing Details";
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
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_WeightUQ_List)));
			this.CV_WeightBoundCalcDropEdit.BindToUnit = "Pivot.CV_WeightUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_WeightUQ)));
			this.CV_WeightBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.CV_WeightBoundCalcDropEdit.Name = "CV_WeightBoundCalcDropEdit";
			this.CV_WeightBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CV_WeightBoundCalcDropEdit.TabIndex = 3;
			this.ContainerControlToolTip.SetToolTip(this.CV_WeightBoundCalcDropEdit, "Net weight of goods in selected container");
			this.CV_WeightBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CV_MarksAndNumbersBoundTextBox
			// 
			this.CV_MarksAndNumbersBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.CV_MarksAndNumbersBoundTextBox.BindTo = "Pivot.CV_MarksAndNumbers";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_MarksAndNumbersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_MarksAndNumbers)));
			this.CV_MarksAndNumbersBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 96, true);
			this.CV_MarksAndNumbersBoundTextBox.Multiline = true;
			this.CV_MarksAndNumbersBoundTextBox.Name = "CV_MarksAndNumbersBoundTextBox";
			this.CV_MarksAndNumbersBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 64, true);
			this.ContainerControlToolTip.SetToolTip(this.CV_MarksAndNumbersBoundTextBox, "Marks and numbers related to the selected container");
			this.CV_MarksAndNumbersBoundTextBox.TabIndex = 15;
			// 
			// CV_GoodsDescriptionBoundTextBox
			// 
			this.CV_GoodsDescriptionBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.CV_GoodsDescriptionBoundTextBox.BindTo = "Pivot.CV_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_GoodsDescription)));
			this.CV_GoodsDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 96, true);
			this.CV_GoodsDescriptionBoundTextBox.Multiline = true;
			this.CV_GoodsDescriptionBoundTextBox.Name = "CV_GoodsDescriptionBoundTextBox";
			this.CV_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 64, true);
			this.CV_GoodsDescriptionBoundTextBox.TabIndex = 7;
			this.ContainerControlToolTip.SetToolTip(this.CV_GoodsDescriptionBoundTextBox, "Description of the goods in the selected container");
			// 
			// CV_VolumeBoundCalcEdit
			// 
			this.CV_VolumeBoundCalcEdit.BindTo = "Pivot.CV_Volume";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_Volume)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_VolumeInfo)));
			this.CV_VolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 56, true);
			this.CV_VolumeBoundCalcEdit.Name = "CV_VolumeBoundCalcEdit";
			this.CV_VolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.CV_VolumeBoundCalcEdit.TabIndex = 5;
			this.CV_VolumeBoundCalcEdit.Text = "0.000";
			this.CV_VolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ContainerControlToolTip.SetToolTip(this.CV_VolumeBoundCalcEdit, "Net volume of goods in selected container\n(in cubic metres)");
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
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Lookups.PackageTypes)));
			this.CV_PackageCountBoundCalcDropEdit.BindToUnit = "Pivot.CV_PackageType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PackageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PackageType)));
			this.CV_PackageCountBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.CV_PackageCountBoundCalcDropEdit.Name = "CV_PackageCountBoundCalcDropEdit";
			this.CV_PackageCountBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CV_PackageCountBoundCalcDropEdit.TabIndex = 1;
			this.ContainerControlToolTip.SetToolTip(this.CV_PackageCountBoundCalcDropEdit, "Quantity of packages being shipped in current container");
			this.CV_PackageCountBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CV_FumigationCertBoundCheckBox
			// 
			this.CV_FumigationCertBoundCheckBox.BindTo = "Pivot.CV_FumigationCert";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_FumigationCert)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_FumigationCertInfo)));
			this.CV_FumigationCertBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CV_FumigationCertBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 12, true);
			this.CV_FumigationCertBoundCheckBox.Name = "CV_FumigationCertBoundCheckBox";
			this.CV_FumigationCertBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 24, true);
			this.CV_FumigationCertBoundCheckBox.TabIndex = 9;
			this.CV_FumigationCertBoundCheckBox.Text = "Fumigation Certificate";
			this.ContainerControlToolTip.SetToolTip(this.CV_FumigationCertBoundCheckBox, "Do the goods require a fumigation certificate?");
			// 
			// CV_TimberBoundCheckBox
			// 
			this.CV_TimberBoundCheckBox.BindTo = "Pivot.CV_Timber";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_Timber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_TimberInfo)));
			this.CV_TimberBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CV_TimberBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 40, true);
			this.CV_TimberBoundCheckBox.Name = "CV_TimberBoundCheckBox";
			this.CV_TimberBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.CV_TimberBoundCheckBox.TabIndex = 13;
			this.CV_TimberBoundCheckBox.Text = "Timber";
			this.ContainerControlToolTip.SetToolTip(this.CV_TimberBoundCheckBox, "Does the shipment contain timber?");
			// 
			// CV_HazardousGoodsBoundCheckBox
			// 
			this.CV_HazardousGoodsBoundCheckBox.BindTo = "Pivot.CV_HazardousGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_HazardousGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_HazardousGoodsInfo)));
			this.CV_HazardousGoodsBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CV_HazardousGoodsBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 16, true);
			this.CV_HazardousGoodsBoundCheckBox.Name = "CV_HazardousGoodsBoundCheckBox";
			this.CV_HazardousGoodsBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 16, true);
			this.CV_HazardousGoodsBoundCheckBox.TabIndex = 8;
			this.CV_HazardousGoodsBoundCheckBox.Text = "Hazardous Goods";
			this.ContainerControlToolTip.SetToolTip(this.CV_HazardousGoodsBoundCheckBox, "Does the shipment contain hazardous materials");
			// 
			// CV_PersonalEffectsBoundCheckBox
			// 
			this.CV_PersonalEffectsBoundCheckBox.BindTo = "Pivot.CV_PersonalEffects";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PersonalEffects)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PersonalEffectsInfo)));
			this.CV_PersonalEffectsBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CV_PersonalEffectsBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 16, true);
			this.CV_PersonalEffectsBoundCheckBox.Name = "CV_PersonalEffectsBoundCheckBox";
			this.CV_PersonalEffectsBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.CV_PersonalEffectsBoundCheckBox.TabIndex = 10;
			this.CV_PersonalEffectsBoundCheckBox.Text = "Personal Effects";
			this.ContainerControlToolTip.SetToolTip(this.CV_PersonalEffectsBoundCheckBox, "Are the goods personal effects");
			// 
			// CV_FlammableBoundCheckBox
			// 
			this.CV_FlammableBoundCheckBox.BindTo = "Pivot.CV_Flammable";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_Flammable)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_FlammableInfo)));
			this.CV_FlammableBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CV_FlammableBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 40, true);
			this.CV_FlammableBoundCheckBox.Name = "CV_FlammableBoundCheckBox";
			this.CV_FlammableBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.CV_FlammableBoundCheckBox.TabIndex = 11;
			this.CV_FlammableBoundCheckBox.Text = "Flammable";
			this.ContainerControlToolTip.SetToolTip(this.CV_FlammableBoundCheckBox, "Are the goods flammable?");
			// 
			// CV_PerishableGoodsBoundCheckBox
			// 
			this.CV_PerishableGoodsBoundCheckBox.BindTo = "Pivot.CV_PerishableGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PerishableGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).CV_PerishableGoodsInfo)));
			this.CV_PerishableGoodsBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CV_PerishableGoodsBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 40, true);
			this.CV_PerishableGoodsBoundCheckBox.Name = "CV_PerishableGoodsBoundCheckBox";
			this.CV_PerishableGoodsBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.CV_PerishableGoodsBoundCheckBox.TabIndex = 12;
			this.CV_PerishableGoodsBoundCheckBox.Text = "Perishable";
			this.ContainerControlToolTip.SetToolTip(this.CV_PerishableGoodsBoundCheckBox, "Are the goods perishable?");
			// 
			// zLabel34
			// 
			this.zLabel34.AutoSize = true;
			this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.zLabel34.TabIndex = 4;
			this.zLabel34.Text = "Volume (m3):";
			// 
			// zLabel35
			// 
			this.zLabel35.AutoSize = true;
			this.zLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 33, true);
			this.zLabel35.Name = "zLabel35";
			this.zLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.zLabel35.TabIndex = 2;
			this.zLabel35.Text = "Weight:";
			// 
			// zLabel37
			// 
			this.zLabel37.AutoSize = true;
			this.zLabel37.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.zLabel37.Name = "zLabel37";
			this.zLabel37.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.zLabel37.TabIndex = 0;
			this.zLabel37.Text = "Packages:";
			// 
			// zLabel38
			// 
			this.zLabel38.AutoSize = true;
			this.zLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 80, true);
			this.zLabel38.Name = "zLabel38";
			this.zLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.zLabel38.TabIndex = 14;
			this.zLabel38.Text = "Marks & Numbers";
			// 
			// zLabel39
			// 
			this.zLabel39.AutoSize = true;
			this.zLabel39.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.zLabel39.Name = "zLabel39";
			this.zLabel39.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.zLabel39.TabIndex = 6;
			this.zLabel39.Text = "Goods Description";
			// 
			// UnderbondMovementTabPage
			// 
			this.underbondMovementTabPage.Controls.Add(this.underbondMovementControl);
			this.underbondMovementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.underbondMovementTabPage.Name = "UnderbondMovementTabPage";
			this.underbondMovementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 165, true);
			this.underbondMovementTabPage.TabIndex = 1;
			this.underbondMovementTabPage.Text = "Underbond Movements";
			// 
			// UnderbondMovementControl
			// 
			this.underbondMovementControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.underbondMovementControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.underbondMovementControl.Name = "UnderbondMovementControl";
			this.underbondMovementControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 165, true);
			this.underbondMovementControl.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 5, true);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// SeaCargoUnderbondAndPackingUserControl
			// 
			this.Controls.Add(this.GroupBoxContainers);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.detailsTabControl);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusSCAHouse";
			this.Name = "SeaCargoUnderbondAndPackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 304, true);
			this.GroupBoxContainers.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.detailsTabControl.ResumeLayout(false);
			this.packingDetailsTabPage.ResumeLayout(false);
			this.packingDetailsTabPage.PerformLayout();
			this.underbondMovementTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		protected internal ZGroupBox GroupBoxContainers;
		protected internal ZArchitecture.ZGrid ContainersGrid;
		protected ZTemplateTabControl detailsTabControl;
		protected ZTabPage packingDetailsTabPage;
		protected ZTabPage underbondMovementTabPage;
		protected internal ZArchitecture.ZTextBox CV_MarksAndNumbersBoundTextBox;
		protected internal ZArchitecture.ZTextBox CV_GoodsDescriptionBoundTextBox;
		protected internal ZArchitecture.ZCalcEdit CV_VolumeBoundCalcEdit;
		protected internal ZCalcDropEdit CV_WeightBoundCalcDropEdit;
		protected internal ZCalcDropEdit CV_PackageCountBoundCalcDropEdit;
		protected internal ZCheckBox CV_FumigationCertBoundCheckBox;
		protected internal ZCheckBox CV_TimberBoundCheckBox;
		protected internal ZCheckBox CV_HazardousGoodsBoundCheckBox;
		protected internal ZCheckBox CV_PersonalEffectsBoundCheckBox;
		protected internal ZCheckBox CV_FlammableBoundCheckBox;
		protected internal ZCheckBox CV_PerishableGoodsBoundCheckBox;
		protected internal ZArchitecture.ZLabel zLabel34;
		protected internal ZArchitecture.ZLabel zLabel35;
		protected internal ZArchitecture.ZLabel zLabel37;
		protected internal ZArchitecture.ZLabel zLabel38;
		protected internal ZArchitecture.ZLabel zLabel39;
		protected CargoWise.Windows.UI.KSplitter splitter1;
		protected SeaCargoHouseUnderbondControl underbondMovementControl;
		private IContainer components;
	}
}
