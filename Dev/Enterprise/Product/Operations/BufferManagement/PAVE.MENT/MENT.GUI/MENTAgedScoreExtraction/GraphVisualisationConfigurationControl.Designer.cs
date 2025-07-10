namespace Enterprise.PAVE.MENT.GUI
{
	partial class GraphVisualisationConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.previewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.smoothCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.graphTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.allowArrowZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.xAxisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.xAxisUnitsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xAxisLabelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.showVerticalGridLinesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.yAxisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.yAxisUnitsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.yAxisLabelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.showHorizontalGridLinesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.showLegendCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.allowZoomCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.categorySequenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.upperBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UpperBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.lowerBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.performLowerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.refreshCategorySequenceZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.overrideCategorySequenceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.isNormalisedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.graphTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.showBandsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.graphTypeDropEdit.SuspendLayout();
			this.xAxisGroupBox.SuspendLayout();
			this.yAxisGroupBox.SuspendLayout();
			this.categorySequenceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation);
			// 
			// previewButton
			// 
			this.previewButton.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("d877190a-aa13-468a-85db-9b0d99782c63", "Preview");
			this.previewButton.Enabled = false;
			this.previewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 16, true);
			this.previewButton.Name = "previewButton";
			this.previewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.previewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.previewButton.TabIndex = 13;
			this.previewButton.UseVisualStyleBackColor = true;
			this.previewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("eb4def46-6976-4fb3-a279-25fca69eec96", "Configuration");
			this.zGroupBox1.Controls.Add(this.previewButton);
			this.zGroupBox1.Controls.Add(this.showBandsCheckBox);
			this.zGroupBox1.Controls.Add(this.smoothCheckBox);
			this.zGroupBox1.Controls.Add(this.graphTypeDropEdit);
			this.zGroupBox1.Controls.Add(this.allowArrowZCheckBox);
			this.zGroupBox1.Controls.Add(this.xAxisGroupBox);
			this.zGroupBox1.Controls.Add(this.yAxisGroupBox);
			this.zGroupBox1.Controls.Add(this.showLegendCheckBox);
			this.zGroupBox1.Controls.Add(this.allowZoomCheckBox);
			this.zGroupBox1.Controls.Add(this.categorySequenceGroupBox);
			this.zGroupBox1.Controls.Add(this.isNormalisedCheckBox);
			this.zGroupBox1.Controls.Add(this.graphTitleTextBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 363, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// smoothCheckBox
			// 
			this.smoothCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.smoothCheckBox, "SmoothCurve");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).SmoothCurve)));
			this.smoothCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.smoothCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 45, true);
			this.smoothCheckBox.Name = "smoothCheckBox";
			this.smoothCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			this.smoothCheckBox.TabIndex = 11;
			this.smoothCheckBox.UseVisualStyleBackColor = true;
			// 
			// graphTypeDropEdit
			// 
			this.graphTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.graphTypeDropEdit, "MVI_GraphType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).MVI_GraphType)));
			this.graphTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 19, true);
			this.graphTypeDropEdit.Name = "graphTypeDropEdit";
			this.graphTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.graphTypeDropEdit.TabIndex = 10;
			// 
			// allowArrowZCheckBox
			// 
			this.allowArrowZCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.allowArrowZCheckBox, "AllowArrowAnnotations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).AllowArrowAnnotations)));
			this.allowArrowZCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.allowArrowZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 131, true);
			this.allowArrowZCheckBox.Name = "allowArrowZCheckBox";
			this.allowArrowZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 17, true);
			this.allowArrowZCheckBox.TabIndex = 9;
			this.allowArrowZCheckBox.UseVisualStyleBackColor = true;
			// 
			// xAxisGroupBox
			// 
			this.xAxisGroupBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("a089fcb3-f31c-40f2-927a-0ce5124a62f4", "X Axis");
			this.xAxisGroupBox.Controls.Add(this.xAxisUnitsTextBox);
			this.xAxisGroupBox.Controls.Add(this.xAxisLabelTextBox);
			this.xAxisGroupBox.Controls.Add(this.showVerticalGridLinesCheckBox);
			this.xAxisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 45, true);
			this.xAxisGroupBox.Name = "xAxisGroupBox";
			this.xAxisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 100, true);
			this.xAxisGroupBox.TabIndex = 8;
			this.xAxisGroupBox.TabStop = false;
			// 
			// xAxisUnitsTextBox
			// 
			this.BindingSource.SetBindingMember(this.xAxisUnitsTextBox, "XAxisUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).XAxisUnits)));
			this.xAxisUnitsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.xAxisUnitsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 75, true);
			this.xAxisUnitsTextBox.Name = "xAxisUnitsTextBox";
			this.xAxisUnitsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.xAxisUnitsTextBox.TabIndex = 8;
			// 
			// xAxisLabelTextBox
			// 
			this.BindingSource.SetBindingMember(this.xAxisLabelTextBox, "XAxisLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).XAxisLabel)));
			this.xAxisLabelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.xAxisLabelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 49, true);
			this.xAxisLabelTextBox.Name = "xAxisLabelTextBox";
			this.xAxisLabelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.xAxisLabelTextBox.TabIndex = 7;
			// 
			// showVerticalGridLinesCheckBox
			// 
			this.showVerticalGridLinesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showVerticalGridLinesCheckBox, "ShowVerticalGridLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).ShowVerticalGridLines)));
			this.showVerticalGridLinesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showVerticalGridLinesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.showVerticalGridLinesCheckBox.Name = "showVerticalGridLinesCheckBox";
			this.showVerticalGridLinesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 17, true);
			this.showVerticalGridLinesCheckBox.TabIndex = 6;
			this.showVerticalGridLinesCheckBox.UseVisualStyleBackColor = true;
			// 
			// yAxisGroupBox
			// 
			this.yAxisGroupBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("8a00c678-195d-47fe-b627-292d83281d5c", "Y Axis");
			this.yAxisGroupBox.Controls.Add(this.yAxisUnitsTextBox);
			this.yAxisGroupBox.Controls.Add(this.yAxisLabelTextBox);
			this.yAxisGroupBox.Controls.Add(this.showHorizontalGridLinesCheckBox);
			this.yAxisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 47, true);
			this.yAxisGroupBox.Name = "yAxisGroupBox";
			this.yAxisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 98, true);
			this.yAxisGroupBox.TabIndex = 7;
			this.yAxisGroupBox.TabStop = false;
			// 
			// yAxisUnitsTextBox
			// 
			this.BindingSource.SetBindingMember(this.yAxisUnitsTextBox, "YAxisUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).YAxisUnits)));
			this.yAxisUnitsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.yAxisUnitsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 73, true);
			this.yAxisUnitsTextBox.Name = "yAxisUnitsTextBox";
			this.yAxisUnitsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.yAxisUnitsTextBox.TabIndex = 7;
			// 
			// yAxisLabelTextBox
			// 
			this.BindingSource.SetBindingMember(this.yAxisLabelTextBox, "YAxisLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).YAxisLabel)));
			this.yAxisLabelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.yAxisLabelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 49, true);
			this.yAxisLabelTextBox.Name = "yAxisLabelTextBox";
			this.yAxisLabelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.yAxisLabelTextBox.TabIndex = 6;
			// 
			// showHorizontalGridLinesCheckBox
			// 
			this.showHorizontalGridLinesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showHorizontalGridLinesCheckBox, "ShowHorizontalGridLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).ShowHorizontalGridLines)));
			this.showHorizontalGridLinesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showHorizontalGridLinesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.showHorizontalGridLinesCheckBox.Name = "showHorizontalGridLinesCheckBox";
			this.showHorizontalGridLinesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 16, true);
			this.showHorizontalGridLinesCheckBox.TabIndex = 5;
			this.showHorizontalGridLinesCheckBox.UseVisualStyleBackColor = true;
			// 
			// showLegendCheckBox
			// 
			this.showLegendCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showLegendCheckBox, "ShowLegend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).ShowLegend)));
			this.showLegendCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showLegendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 102, true);
			this.showLegendCheckBox.Name = "showLegendCheckBox";
			this.showLegendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
			this.showLegendCheckBox.TabIndex = 4;
			this.showLegendCheckBox.UseVisualStyleBackColor = true;
			// 
			// allowZoomCheckBox
			// 
			this.allowZoomCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.allowZoomCheckBox, "AllowZoom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).AllowZoom)));
			this.allowZoomCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.allowZoomCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 73, true);
			this.allowZoomCheckBox.Name = "allowZoomCheckBox";
			this.allowZoomCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			this.allowZoomCheckBox.TabIndex = 3;
			this.allowZoomCheckBox.UseVisualStyleBackColor = true;
			// 
			// categorySequenceGroupBox
			// 
			this.categorySequenceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.categorySequenceGroupBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("8e2f383b-bfed-4450-9466-d4681edff097", "Category Sequence");
			this.categorySequenceGroupBox.Controls.Add(this.upperBoundCalcEdit);
			this.categorySequenceGroupBox.Controls.Add(this.UpperBoundCheckBox);
			this.categorySequenceGroupBox.Controls.Add(this.lowerBoundCalcEdit);
			this.categorySequenceGroupBox.Controls.Add(this.performLowerCheckBox);
			this.categorySequenceGroupBox.Controls.Add(this.refreshCategorySequenceZButton);
			this.categorySequenceGroupBox.Controls.Add(this.overrideCategorySequenceCheckBox);
			this.categorySequenceGroupBox.Controls.Add(this.zGrid1);
			this.categorySequenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 151, true);
			this.categorySequenceGroupBox.Name = "categorySequenceGroupBox";
			this.categorySequenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 206, true);
			this.categorySequenceGroupBox.TabIndex = 2;
			this.categorySequenceGroupBox.TabStop = false;
			// 
			// upperBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.upperBoundCalcEdit, "UpperBoundAggregationSequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).UpperBoundAggregationSequence)));
			this.upperBoundCalcEdit.DecimalPlaces = 2;
			this.upperBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 69, true);
			this.upperBoundCalcEdit.Name = "upperBoundCalcEdit";
			this.upperBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.upperBoundCalcEdit.TabIndex = 7;
			this.upperBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UpperBoundCheckBox
			// 
			this.UpperBoundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UpperBoundCheckBox, "PerformUpperBoundAggregation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).PerformUpperBoundAggregation)));
			this.UpperBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpperBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 72, true);
			this.UpperBoundCheckBox.Name = "UpperBoundCheckBox";
			this.UpperBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.UpperBoundCheckBox.TabIndex = 6;
			this.UpperBoundCheckBox.UseVisualStyleBackColor = true;
			// 
			// lowerBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.lowerBoundCalcEdit, "LowerBoundAggregationSequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).LowerBoundAggregationSequence)));
			this.lowerBoundCalcEdit.DecimalPlaces = 2;
			this.lowerBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 42, true);
			this.lowerBoundCalcEdit.Name = "lowerBoundCalcEdit";
			this.lowerBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.lowerBoundCalcEdit.TabIndex = 5;
			this.lowerBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// performLowerCheckBox
			// 
			this.performLowerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.performLowerCheckBox, "PerformLowerBoundAggregation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).PerformLowerBoundAggregation)));
			this.performLowerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.performLowerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.performLowerCheckBox.Name = "performLowerCheckBox";
			this.performLowerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.performLowerCheckBox.TabIndex = 4;
			this.performLowerCheckBox.UseVisualStyleBackColor = true;
			// 
			// refreshCategorySequenceZButton
			// 
			this.refreshCategorySequenceZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.refreshCategorySequenceZButton.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("65a1d3d3-cff5-4087-8092-6ba515628111", "Populate");
			this.refreshCategorySequenceZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(552, 13, true);
			this.refreshCategorySequenceZButton.Name = "refreshCategorySequenceZButton";
			this.refreshCategorySequenceZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.refreshCategorySequenceZButton.TabIndex = 1;
			this.refreshCategorySequenceZButton.UseVisualStyleBackColor = true;
			// 
			// overrideCategorySequenceCheckBox
			// 
			this.overrideCategorySequenceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.overrideCategorySequenceCheckBox, "UseOverriddenCategorySequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).UseOverriddenCategorySequence)));
			this.overrideCategorySequenceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.overrideCategorySequenceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.overrideCategorySequenceCheckBox.Name = "overrideCategorySequenceCheckBox";
			this.overrideCategorySequenceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 17, true);
			this.overrideCategorySequenceCheckBox.TabIndex = 0;
			this.overrideCategorySequenceCheckBox.UseVisualStyleBackColor = true;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "CategorySequenceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).CategorySequenceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.VisualisationColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).CategorySequenceCollection)).SyncRoot)).Column)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.VisualisationColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).CategorySequenceCollection)).SyncRoot)).ColumnDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.VisualisationColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).CategorySequenceCollection)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.VisualisationColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).CategorySequenceCollection)).SyncRoot)).Sequence)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Column";
			zTextBoxColumnStyleInfo2.ColumnName = "ColumnDisplay";
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid1.CopySelectedRowsAllowed = true;
			this.zGrid1.GridId = "875bf22d-08fa-4d60-936f-9e2dcb166fef";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 95, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 105, true);
			this.zGrid1.TabIndex = 8;
			// 
			// isNormalisedCheckBox
			// 
			this.isNormalisedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isNormalisedCheckBox, "IsNormalised");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).IsNormalised)));
			this.isNormalisedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isNormalisedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.isNormalisedCheckBox.Name = "isNormalisedCheckBox";
			this.isNormalisedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.isNormalisedCheckBox.TabIndex = 2;
			this.isNormalisedCheckBox.UseVisualStyleBackColor = true;
			// 
			// graphTitleTextBox
			// 
			this.graphTitleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | (System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.graphTitleTextBox, "GraphTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).GraphTitle)));
			this.graphTitleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.graphTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 19, true);
			this.graphTitleTextBox.Name = "graphTitleTextBox";
			this.graphTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.graphTitleTextBox.TabIndex = 1;
			// 
			// showBandsCheckBox
			// 
			this.showBandsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showBandsCheckBox, "ShowAcceptabilityBands");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(null)).ShowAcceptabilityBands)));
			this.showBandsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showBandsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 72, true);
			this.showBandsCheckBox.Name = "showBandsCheckBox";
			this.showBandsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
			this.showBandsCheckBox.TabIndex = 12;
			this.showBandsCheckBox.UseVisualStyleBackColor = true;
			// 
			// GraphVisualisationConfigurationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "GraphVisualisationConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 363, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.graphTypeDropEdit.ResumeLayout(true);
			this.graphTypeDropEdit.PerformLayout();
			this.xAxisGroupBox.ResumeLayout(false);
			this.xAxisGroupBox.PerformLayout();
			this.yAxisGroupBox.ResumeLayout(false);
			this.yAxisGroupBox.PerformLayout();
			this.categorySequenceGroupBox.ResumeLayout(false);
			this.categorySequenceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZTextBox graphTitleTextBox;
		private ZArchitecture.GUI.ZCheckBox isNormalisedCheckBox;
		private ZArchitecture.GUI.ZGroupBox categorySequenceGroupBox;
		private ZArchitecture.GUI.ZCheckBox overrideCategorySequenceCheckBox;
		private ZArchitecture.GUI.ZButton refreshCategorySequenceZButton;
		private ZArchitecture.ZGrid zGrid1;
		private ZArchitecture.GUI.ZCheckBox showLegendCheckBox;
		private ZArchitecture.GUI.ZCheckBox allowZoomCheckBox;
		private ZArchitecture.GUI.ZCheckBox showVerticalGridLinesCheckBox;
		private ZArchitecture.GUI.ZCheckBox showHorizontalGridLinesCheckBox;
		private ZArchitecture.GUI.ZGroupBox xAxisGroupBox;
		private ZArchitecture.ZTextBox xAxisUnitsTextBox;
		private ZArchitecture.ZTextBox xAxisLabelTextBox;
		private ZArchitecture.GUI.ZGroupBox yAxisGroupBox;
		private ZArchitecture.ZTextBox yAxisUnitsTextBox;
		private ZArchitecture.ZTextBox yAxisLabelTextBox;
		private ZArchitecture.GUI.ZCheckBox allowArrowZCheckBox;
		private ZArchitecture.ZCalcEdit upperBoundCalcEdit;
		private ZArchitecture.GUI.ZCheckBox UpperBoundCheckBox;
		private ZArchitecture.ZCalcEdit lowerBoundCalcEdit;
		private ZArchitecture.GUI.ZCheckBox performLowerCheckBox;
		private ZArchitecture.GUI.ZDropEdit graphTypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox smoothCheckBox;
		private ZArchitecture.GUI.ZCheckBox showBandsCheckBox;
		private ZArchitecture.GUI.ZButton previewButton;
	}
}
