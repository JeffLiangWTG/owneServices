using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.GUI
{
	partial class CustomisedControlCommonDetailsControl
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
			this.PropertyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PropertyIsBoldCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PropertyReadOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PropertyFontSizeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PropertyForegroundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PropertyBackgroundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PropertyLeftCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PropertyTopCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PropertyHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PropertyWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LabelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FontDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShowOnTopCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AlignmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrientationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RestoreDefaultButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PlayButtonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SuspendButtonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CloseTaskButtonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.flowLayoutPanel1 = new CargoWise.Windows.UI.KFlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PropertyTypeDropEdit.SuspendLayout();
			this.PropertyForegroundDropEdit.SuspendLayout();
			this.PropertyBackgroundDropEdit.SuspendLayout();
			this.FontDropEdit.SuspendLayout();
			this.AlignmentDropEdit.SuspendLayout();
			this.OrientationDropEdit.SuspendLayout();
			this.PlayButtonDropEdit.SuspendLayout();
			this.SuspendButtonDropEdit.SuspendLayout();
			this.CloseTaskButtonDropEdit.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ControlCustomisationBase);
			// 
			// PropertyTypeDropEdit
			// 
			this.PropertyTypeDropEdit.AllowDrop = true;
			this.PropertyTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PropertyTypeDropEdit, "ControlType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).ControlType)));
			this.PropertyTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PropertyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 3, true);
			this.PropertyTypeDropEdit.Name = "PropertyTypeDropEdit";
			this.PropertyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.PropertyTypeDropEdit.TabIndex = 23;
			// 
			// PropertyIsBoldCheckBox
			// 
			this.PropertyIsBoldCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PropertyIsBoldCheckBox, "IsBold");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).IsBold)));
			this.PropertyIsBoldCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PropertyIsBoldCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 188, true);
			this.PropertyIsBoldCheckBox.Name = "PropertyIsBoldCheckBox";
			this.PropertyIsBoldCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 17, true);
			this.PropertyIsBoldCheckBox.TabIndex = 22;
			this.PropertyIsBoldCheckBox.UseVisualStyleBackColor = true;
			// 
			// PropertyReadOnlyCheckBox
			// 
			this.PropertyReadOnlyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PropertyReadOnlyCheckBox, "IsReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).IsReadOnly)));
			this.PropertyReadOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PropertyReadOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 188, true);
			this.PropertyReadOnlyCheckBox.Name = "PropertyReadOnlyCheckBox";
			this.PropertyReadOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.PropertyReadOnlyCheckBox.TabIndex = 23;
			this.PropertyReadOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// PropertyFontSizeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PropertyFontSizeCalcEdit, "FontSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).FontSize)));
			this.PropertyFontSizeCalcEdit.DecimalPlaces = 2;
			this.PropertyFontSizeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 186, true);
			this.PropertyFontSizeCalcEdit.Name = "PropertyFontSizeCalcEdit";
			this.PropertyFontSizeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.PropertyFontSizeCalcEdit.TabIndex = 21;
			this.PropertyFontSizeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PropertyForegroundDropEdit
			// 
			this.PropertyForegroundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PropertyForegroundDropEdit, "ForegroundColor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).ForegroundColor)));
			this.PropertyForegroundDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PropertyForegroundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 134, true);
			this.PropertyForegroundDropEdit.Name = "PropertyForegroundDropEdit";
			this.PropertyForegroundDropEdit.ShowDescriptionBox = false;
			this.PropertyForegroundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.PropertyForegroundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PropertyForegroundDropEdit.TabIndex = 19;
			// 
			// PropertyBackgroundDropEdit
			// 
			this.PropertyBackgroundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PropertyBackgroundDropEdit, "BackgroundColor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).BackgroundColor)));
			this.PropertyBackgroundDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PropertyBackgroundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 107, true);
			this.PropertyBackgroundDropEdit.Name = "PropertyBackgroundDropEdit";
			this.PropertyBackgroundDropEdit.ShowDescriptionBox = false;
			this.PropertyBackgroundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.PropertyBackgroundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PropertyBackgroundDropEdit.TabIndex = 18;
			// 
			// PropertyLeftCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PropertyLeftCalcEdit, "Left");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Left)));
			this.PropertyLeftCalcEdit.DecimalPlaces = 2;
			this.PropertyLeftCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 55, true);
			this.PropertyLeftCalcEdit.Name = "PropertyLeftCalcEdit";
			this.PropertyLeftCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.PropertyLeftCalcEdit.TabIndex = 15;
			this.PropertyLeftCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PropertyTopCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PropertyTopCalcEdit, "Top");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Top)));
			this.PropertyTopCalcEdit.DecimalPlaces = 2;
			this.PropertyTopCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 55, true);
			this.PropertyTopCalcEdit.Name = "PropertyTopCalcEdit";
			this.PropertyTopCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.PropertyTopCalcEdit.TabIndex = 16;
			this.PropertyTopCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PropertyHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PropertyHeightCalcEdit, "Height");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Height)));
			this.PropertyHeightCalcEdit.DecimalPlaces = 2;
			this.PropertyHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 81, true);
			this.PropertyHeightCalcEdit.Name = "PropertyHeightCalcEdit";
			this.PropertyHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.PropertyHeightCalcEdit.TabIndex = 18;
			this.PropertyHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PropertyWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PropertyWidthCalcEdit, "Width");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Width)));
			this.PropertyWidthCalcEdit.DecimalPlaces = 2;
			this.PropertyWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 81, true);
			this.PropertyWidthCalcEdit.Name = "PropertyWidthCalcEdit";
			this.PropertyWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.PropertyWidthCalcEdit.TabIndex = 17;
			this.PropertyWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LabelTextBox
			// 
			this.LabelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LabelTextBox, "Label");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Label)));
			this.LabelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 29, true);
			this.LabelTextBox.Name = "LabelTextBox";
			this.LabelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.LabelTextBox.TabIndex = 13;
			// 
			// FontDropEdit
			// 
			this.FontDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FontDropEdit, "Font");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Font)));
			this.FontDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FontDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 160, true);
			this.FontDropEdit.Name = "FontDropEdit";
			this.FontDropEdit.ShowDescriptionBox = false;
			this.FontDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FontDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.FontDropEdit.TabIndex = 20;
			// 
			// ShowOnTopCheckBox
			// 
			this.ShowOnTopCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowOnTopCheckBox, "BringToFront");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).BringToFront)));
			this.ShowOnTopCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowOnTopCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 214, true);
			this.ShowOnTopCheckBox.Name = "ShowOnTopCheckBox";
			this.ShowOnTopCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
			this.ShowOnTopCheckBox.TabIndex = 24;
			this.ShowOnTopCheckBox.UseVisualStyleBackColor = true;
			// 
			// AlignmentDropEdit
			// 
			this.AlignmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlignmentDropEdit, "Alignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Alignment)));
			this.AlignmentDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AlignmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 212, true);
			this.AlignmentDropEdit.Name = "AlignmentDropEdit";
			this.AlignmentDropEdit.ShowDescriptionBox = false;
			this.AlignmentDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.AlignmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AlignmentDropEdit.TabIndex = 25;
			// 
			// OrientationDropEdit
			// 
			this.OrientationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrientationDropEdit, "Orientation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).Orientation)));
			this.OrientationDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OrientationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 238, true);
			this.OrientationDropEdit.Name = "OrientationDropEdit";
			this.OrientationDropEdit.ShowDescriptionBox = false;
			this.OrientationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OrientationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.OrientationDropEdit.TabIndex = 26;
			// 
			// RestoreDefaultButton
			// 
			this.RestoreDefaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RestoreDefaultButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 29, true);
			this.RestoreDefaultButton.Name = "RestoreDefaultButton";
			this.RestoreDefaultButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RestoreDefaultButton.TabIndex = 14;
			this.RestoreDefaultButton.Text = Res.GetString("7CE9D31B-9837-48EC-A4C8-D7401FEBE04F", "Restore Default");
			this.RestoreDefaultButton.UseVisualStyleBackColor = true;
			this.RestoreDefaultButton.Click += new System.EventHandler(this.RestoreDefaultButton_Click);
			// 
			// PlayButtonDropEdit
			// 
			this.PlayButtonDropEdit.AllowDrop = true;
			this.PlayButtonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PlayButtonDropEdit, "PlayButtonBehavior");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).PlayButtonBehavior)));
			this.PlayButtonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.PlayButtonDropEdit.Name = "PlayButtonDropEdit";
			this.PlayButtonDropEdit.ShouldResizeByMaxLength = true;
			this.PlayButtonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.PlayButtonDropEdit.TabIndex = 27;
			// 
			// SuspendButtonDropEdit
			// 
			this.SuspendButtonDropEdit.AllowDrop = true;
			this.SuspendButtonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SuspendButtonDropEdit, "SuspendButtonBehavior");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).SuspendButtonBehavior)));
			this.SuspendButtonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 29, true);
			this.SuspendButtonDropEdit.Name = "SuspendButtonDropEdit";
			this.SuspendButtonDropEdit.ShouldResizeByMaxLength = true;
			this.SuspendButtonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.SuspendButtonDropEdit.TabIndex = 28;
			// 
			// CloseTaskButtonDropEdit
			// 
			this.CloseTaskButtonDropEdit.AllowDrop = true;
			this.CloseTaskButtonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CloseTaskButtonDropEdit, "CloseTaskButtonBehavior");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ControlCustomisationBase)(null)).CloseTaskButtonBehavior)));
			this.CloseTaskButtonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 55, true);
			this.CloseTaskButtonDropEdit.Name = "CloseTaskButtonDropEdit";
			this.CloseTaskButtonDropEdit.ShouldResizeByMaxLength = true;
			this.CloseTaskButtonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.CloseTaskButtonDropEdit.TabIndex = 29;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.PlayButtonDropEdit);
			this.flowLayoutPanel1.Controls.Add(this.SuspendButtonDropEdit);
			this.flowLayoutPanel1.Controls.Add(this.CloseTaskButtonDropEdit);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 264, true);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 80, true);
			this.flowLayoutPanel1.TabIndex = 30;
			// 
			// CustomisedControlCommonDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.RestoreDefaultButton);
			this.Controls.Add(this.OrientationDropEdit);
			this.Controls.Add(this.AlignmentDropEdit);
			this.Controls.Add(this.ShowOnTopCheckBox);
			this.Controls.Add(this.FontDropEdit);
			this.Controls.Add(this.PropertyTypeDropEdit);
			this.Controls.Add(this.PropertyIsBoldCheckBox);
			this.Controls.Add(this.PropertyReadOnlyCheckBox);
			this.Controls.Add(this.PropertyFontSizeCalcEdit);
			this.Controls.Add(this.PropertyForegroundDropEdit);
			this.Controls.Add(this.PropertyBackgroundDropEdit);
			this.Controls.Add(this.PropertyLeftCalcEdit);
			this.Controls.Add(this.PropertyTopCalcEdit);
			this.Controls.Add(this.PropertyHeightCalcEdit);
			this.Controls.Add(this.PropertyWidthCalcEdit);
			this.Controls.Add(this.LabelTextBox);
			this.Name = "CustomisedControlCommonDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 356, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PropertyTypeDropEdit.ResumeLayout(true);
			this.PropertyTypeDropEdit.PerformLayout();
			this.PropertyForegroundDropEdit.ResumeLayout(true);
			this.PropertyForegroundDropEdit.PerformLayout();
			this.PropertyBackgroundDropEdit.ResumeLayout(true);
			this.PropertyBackgroundDropEdit.PerformLayout();
			this.FontDropEdit.ResumeLayout(true);
			this.FontDropEdit.PerformLayout();
			this.AlignmentDropEdit.ResumeLayout(true);
			this.AlignmentDropEdit.PerformLayout();
			this.OrientationDropEdit.ResumeLayout(true);
			this.OrientationDropEdit.PerformLayout();
			this.PlayButtonDropEdit.ResumeLayout(true);
			this.PlayButtonDropEdit.PerformLayout();
			this.SuspendButtonDropEdit.ResumeLayout(true);
			this.SuspendButtonDropEdit.PerformLayout();
			this.CloseTaskButtonDropEdit.ResumeLayout(true);
			this.CloseTaskButtonDropEdit.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit PropertyTypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox PropertyIsBoldCheckBox;
		private ZArchitecture.GUI.ZCheckBox PropertyReadOnlyCheckBox;
		private ZArchitecture.ZCalcEdit PropertyFontSizeCalcEdit;
		private ZArchitecture.GUI.ZDropEdit PropertyForegroundDropEdit;
		private ZArchitecture.GUI.ZDropEdit PropertyBackgroundDropEdit;
		private ZArchitecture.ZCalcEdit PropertyLeftCalcEdit;
		private ZArchitecture.ZCalcEdit PropertyTopCalcEdit;
		private ZArchitecture.ZCalcEdit PropertyHeightCalcEdit;
		private ZArchitecture.ZCalcEdit PropertyWidthCalcEdit;
		private ZArchitecture.ZTextBox LabelTextBox;
		private ZArchitecture.GUI.ZDropEdit FontDropEdit;
		private ZArchitecture.GUI.ZCheckBox ShowOnTopCheckBox;
		private ZArchitecture.GUI.ZDropEdit AlignmentDropEdit;
		private ZArchitecture.GUI.ZDropEdit OrientationDropEdit;
		private ZArchitecture.GUI.ZButton RestoreDefaultButton;
		private ZArchitecture.GUI.ZDropEdit PlayButtonDropEdit;
		private ZArchitecture.GUI.ZDropEdit SuspendButtonDropEdit;
		private ZArchitecture.GUI.ZDropEdit CloseTaskButtonDropEdit;
		private CargoWise.Windows.UI.KFlowLayoutPanel flowLayoutPanel1;
	}
}
