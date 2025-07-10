namespace Enterprise.Customs.CA.GUI
{
	partial class PackingPlugInUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.GroupBoxContainers = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DGLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.CV_VolumeBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.HazardousGoodsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DangerousGoodGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CV_HarmonisedTariffNumsBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CV_WeightBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CV_PackageCountBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CV_MarksAndNumbersBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CV_GoodsDescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBoxContainers.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).BeginInit();
			this.PackingDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusSCAHouse);
			// 
			// splitter1
			// 
			this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 16, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 556, true);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// GroupBoxContainers
			// 
			this.GroupBoxContainers.Controls.Add(this.PackingGrid);
			this.GroupBoxContainers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBoxContainers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBoxContainers.Name = "GroupBoxContainers";
			this.GroupBoxContainers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 149, true);
			this.GroupBoxContainers.TabIndex = 12;
			this.GroupBoxContainers.TabStop = false;
			// 
			// PackingGrid
			// 
			this.PackingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackingGrid, "PackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_AssociatedContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_HarmonisedTariffNums)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_MarksAndNumbers)));
			this.PackingGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CV_PackageCount";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|bab90dbe-6765-4419-9d68-059825d812bf", "Packages");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CV_PackageType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|bab90dbe-6765-4419-9d68-059825d812bf", "Packages");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CV_Weight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|a761ded8-cb35-418a-8b8a-8349fb5e0ea7", "Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "CV_WeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|a761ded8-cb35-418a-8b8a-8349fb5e0ea7", "Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CV_GoodsDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|d0d73beb-64ad-406c-9756-4cf8778b03d1", "Container");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CV_AssociatedContainer";
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CV_Volume";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|3d1d6207-9e7a-4367-a7cf-c747649f0286", "Volume");
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.ColumnName = "CV_VolumeUQ";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|3d1d6207-9e7a-4367-a7cf-c747649f0286", "Volume");
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|43b88e9f-7a06-44c3-ae90-9f29102ba26b", "HS Code");
			zTextBoxColumnStyleInfo2.ColumnName = "CV_HarmonisedTariffNums";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CV_MarksAndNumbers";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.PackingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackingGrid.CopySelectedRowsAllowed = true;
			this.PackingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingGrid.GridId = "9c7fc6ed-fc52-4c01-b1af-b4feba0128bc";
			this.PackingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingGrid.LayoutKey = "PackingGrid";
			this.PackingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingGrid.Name = "PackingGrid";
			this.PackingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 130, true);
			this.PackingGrid.TabIndex = 0;
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.Controls.Add(this.DGLinkLabel);
			this.PackingDetailsGroupBox.Controls.Add(this.CV_VolumeBoundCalcDropEdit);
			this.PackingDetailsGroupBox.Controls.Add(this.HazardousGoodsCheckBox);
			this.PackingDetailsGroupBox.Controls.Add(this.DangerousGoodGuidFindBox);
			this.PackingDetailsGroupBox.Controls.Add(this.CV_HarmonisedTariffNumsBoundTextBox);
			this.PackingDetailsGroupBox.Controls.Add(this.CV_WeightBoundCalcDropEdit);
			this.PackingDetailsGroupBox.Controls.Add(this.CV_PackageCountBoundCalcDropEdit);
			this.PackingDetailsGroupBox.Controls.Add(this.CV_MarksAndNumbersBoundTextBox);
			this.PackingDetailsGroupBox.Controls.Add(this.CV_GoodsDescriptionBoundTextBox);
			this.PackingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PackingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 149, true);
			this.PackingDetailsGroupBox.Name = "PackingDetailsGroupBox";
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 178, true);
			this.PackingDetailsGroupBox.TabIndex = 11;
			this.PackingDetailsGroupBox.TabStop = false;
			// 
			// DGLinkLabel
			// 
			this.DGLinkLabel.AutoSize = true;
			this.DGLinkLabel.IsFontBold = false;
			this.DGLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 79, true);
			this.DGLinkLabel.Name = "DGLinkLabel";
			this.DGLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 13, true);
			this.DGLinkLabel.TabIndex = 8;
			this.DGLinkLabel.Text = "Dangerous Goods Details";
			// 
			// CV_VolumeBoundCalcDropEdit
			// 
			this.CV_VolumeBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CV_VolumeBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_VolumeUQ)));
			this.CV_VolumeBoundCalcDropEdit.BindToAmount = "PackLines.CV_Volume";
			this.CV_VolumeBoundCalcDropEdit.BindToUnit = "PackLines.CV_VolumeUQ";
			this.CV_VolumeBoundCalcDropEdit.Decimals = 2;
			this.CV_VolumeBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 50, true);
			this.CV_VolumeBoundCalcDropEdit.Name = "CV_VolumeBoundCalcDropEdit";
			this.CV_VolumeBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CV_VolumeBoundCalcDropEdit.TabIndex = 2;
			this.CV_VolumeBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// HazardousGoodsCheckBox
			// 
			this.HazardousGoodsCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.HazardousGoodsCheckBox, "PackLines.CV_HazardousGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_HazardousGoods)));
			this.HazardousGoodsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HazardousGoodsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(727, 74, true);
			this.HazardousGoodsCheckBox.Name = "HazardousGoodsCheckBox";
			this.HazardousGoodsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 24, true);
			this.HazardousGoodsCheckBox.TabIndex = 5;
			this.HazardousGoodsCheckBox.UseVisualStyleBackColor = true;
			// 
			// DangerousGoodGuidFindBox
			// 
			this.DangerousGoodGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DangerousGoodGuidFindBox, "PackLines.UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.DangerousGoodGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 76, true);
			this.DangerousGoodGuidFindBox.Name = "DangerousGoodGuidFindBox";
			this.DangerousGoodGuidFindBox.PopupCaption = null;
			this.DangerousGoodGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 20, true);
			this.DangerousGoodGuidFindBox.TabIndex = 4;
			// 
			// CV_HarmonisedTariffNumsBoundTextBox
			// 
			this.CV_HarmonisedTariffNumsBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CV_HarmonisedTariffNumsBoundTextBox, "PackLines.CV_HarmonisedTariffNums");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_HarmonisedTariffNums)));
			this.CV_HarmonisedTariffNumsBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("PackingPlugInUserControl|3776E695-3ED9-4229-9202-966DA89EE048", "HS Code");
			this.CV_HarmonisedTariffNumsBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 50, true);
			this.CV_HarmonisedTariffNumsBoundTextBox.Name = "CV_HarmonisedTariffNumsBoundTextBox";
			this.CV_HarmonisedTariffNumsBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CV_HarmonisedTariffNumsBoundTextBox.TabIndex = 3;
			// 
			// CV_WeightBoundCalcDropEdit
			// 
			this.CV_WeightBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CV_WeightBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_WeightUQ)));
			this.CV_WeightBoundCalcDropEdit.BindToAmount = "PackLines.CV_Weight";
			this.CV_WeightBoundCalcDropEdit.BindToUnit = "PackLines.CV_WeightUQ";
			this.CV_WeightBoundCalcDropEdit.Decimals = 2;
			this.CV_WeightBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 24, true);
			this.CV_WeightBoundCalcDropEdit.Name = "CV_WeightBoundCalcDropEdit";
			this.CV_WeightBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CV_WeightBoundCalcDropEdit.TabIndex = 1;
			this.CV_WeightBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CV_PackageCountBoundCalcDropEdit
			// 
			this.CV_PackageCountBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CV_PackageCountBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_PackageType)));
			this.CV_PackageCountBoundCalcDropEdit.BindToAmount = "PackLines.CV_PackageCount";
			this.CV_PackageCountBoundCalcDropEdit.BindToUnit = "PackLines.CV_PackageType";
			this.CV_PackageCountBoundCalcDropEdit.Decimals = 2;
			this.CV_PackageCountBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 24, true);
			this.CV_PackageCountBoundCalcDropEdit.Name = "CV_PackageCountBoundCalcDropEdit";
			this.CV_PackageCountBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CV_PackageCountBoundCalcDropEdit.TabIndex = 0;
			this.CV_PackageCountBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CV_MarksAndNumbersBoundTextBox
			// 
			this.CV_MarksAndNumbersBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.CV_MarksAndNumbersBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CV_MarksAndNumbersBoundTextBox, "PackLines.CV_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_MarksAndNumbers)));
			this.CV_MarksAndNumbersBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 106, true);
			this.CV_MarksAndNumbersBoundTextBox.Multiline = true;
			this.CV_MarksAndNumbersBoundTextBox.Name = "CV_MarksAndNumbersBoundTextBox";
			this.CV_MarksAndNumbersBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 60, true);
			this.CV_MarksAndNumbersBoundTextBox.TabIndex = 7;
			// 
			// CV_GoodsDescriptionBoundTextBox
			// 
			this.CV_GoodsDescriptionBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.CV_GoodsDescriptionBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CV_GoodsDescriptionBoundTextBox, "PackLines.CV_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).PackLines)).SyncRoot)).CV_GoodsDescription)));
			this.CV_GoodsDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 106, true);
			this.CV_GoodsDescriptionBoundTextBox.Multiline = true;
			this.CV_GoodsDescriptionBoundTextBox.Name = "CV_GoodsDescriptionBoundTextBox";
			this.CV_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 60, true);
			this.CV_GoodsDescriptionBoundTextBox.TabIndex = 6;
			// 
			// PackingPlugInUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBoxContainers);
			this.Controls.Add(this.PackingDetailsGroupBox);
			this.Name = "PackingPlugInUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 327, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBoxContainers.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).EndInit();
			this.PackingDetailsGroupBox.ResumeLayout(false);
			this.PackingDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitter splitter1;
		protected internal ZArchitecture.GUI.ZGroupBox GroupBoxContainers;
		protected internal ZArchitecture.ZGrid PackingGrid;
		internal ZArchitecture.GUI.ZGroupBox PackingDetailsGroupBox;
		private ZArchitecture.GUI.ZLinkLabel DGLinkLabel;
		protected internal ZArchitecture.GUI.ZCalcDropEdit CV_VolumeBoundCalcDropEdit;
		private ZArchitecture.GUI.ZCheckBox HazardousGoodsCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox DangerousGoodGuidFindBox;
		private ZArchitecture.ZTextBox CV_HarmonisedTariffNumsBoundTextBox;
		protected internal ZArchitecture.GUI.ZCalcDropEdit CV_WeightBoundCalcDropEdit;
		protected internal ZArchitecture.GUI.ZCalcDropEdit CV_PackageCountBoundCalcDropEdit;
		protected internal ZArchitecture.ZTextBox CV_MarksAndNumbersBoundTextBox;
		protected internal ZArchitecture.ZTextBox CV_GoodsDescriptionBoundTextBox;
	}
}
