namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UnloadedItemNewUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.NewItemTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.NewItemTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UnloadedNetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnloadedGrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnloadingNotesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnloadingNotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewItemDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DescriptionOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommodityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewItemTabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainersLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NewItemTabPage3 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackagesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewItemTabPage4 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewItemTabPage5 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SgiCodesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SgiCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NewItemTabControl.SuspendLayout();
			this.NewItemTabPage1.SuspendLayout();
			this.UnloadedNetWeightCalcDropEdit.SuspendLayout();
			this.UnloadedGrossWeightCalcDropEdit.SuspendLayout();
			this.NewItemTabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.NewItemTabPage3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).BeginInit();
			this.PackagesGrid.SuspendLayout();
			this.NewItemTabPage4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.NewItemTabPage5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SgiCodesGrid)).BeginInit();
			this.SgiCodesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// NewItemTabControl
			// 
			this.NewItemTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.NewItemTabControl.Controls.Add(this.NewItemTabPage1);
			this.NewItemTabControl.Controls.Add(this.NewItemTabPage2);
			this.NewItemTabControl.Controls.Add(this.NewItemTabPage3);
			this.NewItemTabControl.Controls.Add(this.NewItemTabPage4);
			this.NewItemTabControl.Controls.Add(this.NewItemTabPage5);
			this.NewItemTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewItemTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NewItemTabControl.Name = "NewItemTabControl";
			this.NewItemTabControl.SelectedIndex = 0;
			this.NewItemTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 687, true);
			this.NewItemTabControl.TabIndex = 2;
			// 
			// NewItemTabPage1
			// 
			this.NewItemTabPage1.BackColor = System.Drawing.SystemColors.Control;
			this.NewItemTabPage1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("af14ad99-ca3d-4c5a-a282-78f2c0bce2c5", "Item Details");
			this.NewItemTabPage1.Controls.Add(this.UnloadedNetWeightCalcDropEdit);
			this.NewItemTabPage1.Controls.Add(this.UnloadedGrossWeightCalcDropEdit);
			this.NewItemTabPage1.Controls.Add(this.UnloadingNotesLabel);
			this.NewItemTabPage1.Controls.Add(this.UnloadingNotesTextBox);
			this.NewItemTabPage1.Controls.Add(this.NewItemDetailsLabel);
			this.NewItemTabPage1.Controls.Add(this.DescriptionOfGoodsTextBox);
			this.NewItemTabPage1.Controls.Add(this.CommodityCodeTextBox);
			this.NewItemTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewItemTabPage1.Name = "NewItemTabPage1";
			this.NewItemTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NewItemTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 660, true);
			this.NewItemTabPage1.TabIndex = 0;
			// 
			// UnloadedNetWeightCalcDropEdit
			// 
			this.UnloadedNetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedNetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_NetWeightUnit)));
			this.UnloadedNetWeightCalcDropEdit.BindToAmount = "UnloadingMovementHeader.GoodsItems.BY_NetWeight";
			this.UnloadedNetWeightCalcDropEdit.BindToUnit = "UnloadingMovementHeader.GoodsItems.BY_NetWeightUnit";
			this.UnloadedNetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 166, true);
			this.UnloadedNetWeightCalcDropEdit.Name = "UnloadedNetWeightCalcDropEdit";
			this.UnloadedNetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.UnloadedNetWeightCalcDropEdit.TabIndex = 4;
			// 
			// UnloadedGrossWeightCalcDropEdit
			// 
			this.UnloadedGrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedGrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_GrossWeightUnit)));
			this.UnloadedGrossWeightCalcDropEdit.BindToAmount = "UnloadingMovementHeader.GoodsItems.BY_GrossWeight";
			this.UnloadedGrossWeightCalcDropEdit.BindToUnit = "UnloadingMovementHeader.GoodsItems.BY_GrossWeightUnit";
			this.UnloadedGrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 140, true);
			this.UnloadedGrossWeightCalcDropEdit.Name = "UnloadedGrossWeightCalcDropEdit";
			this.UnloadedGrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.UnloadedGrossWeightCalcDropEdit.TabIndex = 3;
			// 
			// UnloadingNotesLabel
			// 
			this.UnloadingNotesLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("6b5f9986-01cf-4838-90e9-e1fd55d3a6cd", "Unloading Notes");
			this.UnloadingNotesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnloadingNotesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 18, true);
			this.UnloadingNotesLabel.Name = "UnloadingNotesLabel";
			this.UnloadingNotesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.UnloadingNotesLabel.TabIndex = 5;
			// 
			// UnloadingNotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnloadingNotesTextBox, "UnloadingMovementHeader.GoodsItems.UnloadingNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).UnloadingNotes)));
			this.UnloadingNotesTextBox.CaptionResourceString = null;
			this.UnloadingNotesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnloadingNotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 41, true);
			this.UnloadingNotesTextBox.Multiline = true;
			this.UnloadingNotesTextBox.Name = "UnloadingNotesTextBox";
			this.UnloadingNotesTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.UnloadingNotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 147, true);
			this.UnloadingNotesTextBox.TabIndex = 6;
			// 
			// NewItemDetailsLabel
			// 
			this.NewItemDetailsLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7f2a0167-5eb2-4773-a745-5f28c4680c38", "New Item Details");
			this.NewItemDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NewItemDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 18, true);
			this.NewItemDetailsLabel.Name = "NewItemDetailsLabel";
			this.NewItemDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.NewItemDetailsLabel.TabIndex = 0;
			// 
			// DescriptionOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionOfGoodsTextBox, "UnloadingMovementHeader.GoodsItems.BY_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_Description)));
			this.DescriptionOfGoodsTextBox.CaptionResourceString = null;
			this.DescriptionOfGoodsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 67, true);
			this.DescriptionOfGoodsTextBox.Multiline = true;
			this.DescriptionOfGoodsTextBox.Name = "DescriptionOfGoodsTextBox";
			this.DescriptionOfGoodsTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 67, true);
			this.DescriptionOfGoodsTextBox.TabIndex = 2;
			// 
			// CommodityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommodityCodeTextBox, "UnloadingMovementHeader.GoodsItems.BY_FormattedHarmonisedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_FormattedHarmonisedTariff)));
			this.CommodityCodeTextBox.CaptionResourceString = null;
			this.CommodityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
			this.CommodityCodeTextBox.Name = "CommodityCodeTextBox";
			this.CommodityCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CommodityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.CommodityCodeTextBox.TabIndex = 1;
			// 
			// NewItemTabPage2
			// 
			this.NewItemTabPage2.BackColor = System.Drawing.SystemColors.Control;
			this.NewItemTabPage2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("36dc449c-803a-48ef-a271-d6de6be91873", "Containers");
			this.NewItemTabPage2.Controls.Add(this.ContainersGrid);
			this.NewItemTabPage2.Controls.Add(this.ContainersLabel);
			this.NewItemTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewItemTabPage2.Name = "NewItemTabPage2";
			this.NewItemTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NewItemTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 660, true);
			this.NewItemTabPage2.TabIndex = 1;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.ContainersGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "UnloadingMovementHeader.GoodsItems.Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).Containers)).SyncRoot)).ContainerNumber)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("119ba41a-ccf3-4f98-8a29-0be95bc2a75e", "Container Number");
			zTextBoxColumnStyleInfo1.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "zGrid1";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 41, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 147, true);
			this.ContainersGrid.TabIndex = 23;
			// 
			// ContainersLabel
			// 
			this.ContainersLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("69764a10-b658-4274-8fb8-49a01494252f", "New Item Containers");
			this.ContainersLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContainersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 18, true);
			this.ContainersLabel.Name = "ContainersLabel";
			this.ContainersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 23, true);
			this.ContainersLabel.TabIndex = 19;
			// 
			// NewItemTabPage3
			// 
			this.NewItemTabPage3.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("a93b3e46-14df-466a-ac8b-cbc19c9403dd", "[31] Packages");
			this.NewItemTabPage3.Controls.Add(this.PackagesLabel);
			this.NewItemTabPage3.Controls.Add(this.PackagesGrid);
			this.NewItemTabPage3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewItemTabPage3.Name = "NewItemTabPage3";
			this.NewItemTabPage3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 660, true);
			this.NewItemTabPage3.TabIndex = 2;
			// 
			// PackagesLabel
			// 
			this.PackagesLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("c435152a-e4e3-4eee-a9d6-2260451f4c8d", "New Item Packages");
			this.PackagesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 18, true);
			this.PackagesLabel.Name = "PackagesLabel";
			this.PackagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 23, true);
			this.PackagesLabel.TabIndex = 21;
			// 
			// PackagesGrid
			// 
			this.PackagesGrid.AllowNavigation = false;
			this.PackagesGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.PackagesGrid, "UnloadingMovementHeader.GoodsItems.Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).Packages)).SyncRoot)).B5_UnitType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).Packages)).SyncRoot)).B5_UnitCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).Packages)).SyncRoot)).B5_MarksAndNumbers)));
			this.PackagesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("47d1a836-9779-42f3-9277-bb178ec6c6bf", "Package Type");
			zDropEditColumnStyleInfo1.ColumnName = "B5_UnitType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5050c3ce-a684-444a-b771-4aa956a5eb61", "Package Count");
			zCalcEditColumnStyleInfo1.ColumnName = "B5_UnitCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4e6db46c-006f-41ed-81ea-d7515b3a8327", "Marks & Numbers");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "B5_MarksAndNumbers";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(360);
			this.PackagesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackagesGrid.GridId = "1afd7f99-62b9-4a2b-a6fe-c9c9250710fc";
			this.PackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackagesGrid.LayoutKey = "zGrid1";
			this.PackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 41, true);
			this.PackagesGrid.Name = "PackagesGrid";
			this.PackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 147, true);
			this.PackagesGrid.TabIndex = 2;
			// 
			// NewItemTabPage4
			// 
			this.NewItemTabPage4.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("51fd15bd-8cc3-492b-a298-cb69f9d2586d", "[44] Supporting Documents");
			this.NewItemTabPage4.Controls.Add(this.SupportingDocumentsLabel);
			this.NewItemTabPage4.Controls.Add(this.SupportingDocumentsGrid);
			this.NewItemTabPage4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewItemTabPage4.Name = "NewItemTabPage4";
			this.NewItemTabPage4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 660, true);
			this.NewItemTabPage4.TabIndex = 4;
			// 
			// SupportingDocumentsLabel
			// 
			this.SupportingDocumentsLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("73e5b309-8208-4726-9650-d802116c4dd8", "New Item Documents/Certificates");
			this.SupportingDocumentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SupportingDocumentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 18, true);
			this.SupportingDocumentsLabel.Name = "SupportingDocumentsLabel";
			this.SupportingDocumentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 23, true);
			this.SupportingDocumentsLabel.TabIndex = 23;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.SupportingDocumentsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "UnloadingMovementHeader.GoodsItems.SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Description)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(340);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SupportingDocumentsGrid.GridId = "3b710cba-62e3-49e2-a94f-8370b1f1a0ee";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 41, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 147, true);
			this.SupportingDocumentsGrid.TabIndex = 3;
			// 
			// NewItemTabPage5
			// 
			this.NewItemTabPage5.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5b341746-b6af-4be7-9255-57ce9e77ad8e", "[44] SGI Codes");
			this.NewItemTabPage5.Controls.Add(this.SgiCodesLabel);
			this.NewItemTabPage5.Controls.Add(this.SgiCodesGrid);
			this.NewItemTabPage5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewItemTabPage5.Name = "NewItemTabPage5";
			this.NewItemTabPage5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 660, true);
			this.NewItemTabPage5.TabIndex = 5;
			// 
			// SgiCodesLabel
			// 
			this.SgiCodesLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5598a8a9-6718-450e-8569-084db9694c29", "New Item SGI Codes");
			this.SgiCodesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SgiCodesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 18, true);
			this.SgiCodesLabel.Name = "SgiCodesLabel";
			this.SgiCodesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 23, true);
			this.SgiCodesLabel.TabIndex = 25;
			// 
			// SgiCodesGrid
			// 
			this.SgiCodesGrid.AllowNavigation = false;
			this.SgiCodesGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.SgiCodesGrid, "UnloadingMovementHeader.GoodsItems.AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_NctsExportFromEC)));
			this.SgiCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5bbd1717-7b31-4f82-a218-92b2b476eb54", "Export from other country");
			zTextBoxColumnStyleInfo6.ColumnName = "CSI_RN_NKCountryCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("1bda038b-0a88-4976-9f1d-4b8e39e11b4b", "Export from EC");
			zCheckBoxColumnStyleInfo1.ColumnName = "CSI_NctsExportFromEC";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SgiCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SgiCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SgiCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SgiCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SgiCodesGrid.GridId = "97634e6a-c7d0-48d3-a892-5aa308d1826a";
			this.SgiCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SgiCodesGrid.LayoutKey = "AdditionalInfosGrid";
			this.SgiCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 41, true);
			this.SgiCodesGrid.Name = "SgiCodesGrid";
			this.SgiCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 147, true);
			this.SgiCodesGrid.TabIndex = 8;
			// 
			// UnloadedItemNewUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NewItemTabControl);
			this.Name = "UnloadedItemNewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 687, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NewItemTabControl.ResumeLayout(false);
			this.NewItemTabControl.PerformLayout();
			this.NewItemTabPage1.ResumeLayout(false);
			this.NewItemTabPage1.PerformLayout();
			this.UnloadedNetWeightCalcDropEdit.ResumeLayout(true);
			this.UnloadedNetWeightCalcDropEdit.PerformLayout();
			this.UnloadedGrossWeightCalcDropEdit.ResumeLayout(true);
			this.UnloadedGrossWeightCalcDropEdit.PerformLayout();
			this.NewItemTabPage2.ResumeLayout(false);
			this.NewItemTabPage2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.NewItemTabPage3.ResumeLayout(false);
			this.NewItemTabPage3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).EndInit();
			this.PackagesGrid.ResumeLayout(false);
			this.PackagesGrid.PerformLayout();
			this.NewItemTabPage4.ResumeLayout(false);
			this.NewItemTabPage4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.NewItemTabPage5.ResumeLayout(false);
			this.NewItemTabPage5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SgiCodesGrid)).EndInit();
			this.SgiCodesGrid.ResumeLayout(false);
			this.SgiCodesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl NewItemTabControl;
		protected ZArchitecture.GUI.ZTabPage NewItemTabPage1;
		private ZArchitecture.GUI.ZTabPage NewItemTabPage2;
		protected internal ZArchitecture.ZGrid ContainersGrid;
		private ZArchitecture.ZLabel ContainersLabel;
		private ZArchitecture.GUI.ZTabPage NewItemTabPage3;
		private ZArchitecture.ZLabel PackagesLabel;
		protected ZArchitecture.ZGrid PackagesGrid;
		protected ZArchitecture.GUI.ZTabPage NewItemTabPage4;
		private ZArchitecture.ZLabel SupportingDocumentsLabel;
		protected ZArchitecture.ZGrid SupportingDocumentsGrid;
		protected ZArchitecture.GUI.ZTabPage NewItemTabPage5;
		private ZArchitecture.ZLabel SgiCodesLabel;
		private ZArchitecture.ZGrid SgiCodesGrid;
		private ZArchitecture.ZLabel NewItemDetailsLabel;
		private ZArchitecture.ZTextBox DescriptionOfGoodsTextBox;
		protected ZArchitecture.ZTextBox CommodityCodeTextBox;
		private ZArchitecture.ZTextBox UnloadingNotesTextBox;
		private ZArchitecture.ZLabel UnloadingNotesLabel;
		private ZArchitecture.GUI.ZCalcDropEdit UnloadedNetWeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit UnloadedGrossWeightCalcDropEdit;
	}
}
