using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	internal partial class WatermarkControl : RegistryBusinessObjectTemplateZUserControl
	{
		ZCheckBox TextWatermarkCheckBox;
		ZTextBox TextWatermarkTextBox;
		ZGroupBox TextWatermarkGroupBox;
		ZCalcEdit RotationCalcEdit;
		ZCalcEdit FontSizeCalcEdit;
		ZDropEdit HorizontalAlignmentDropEdit;
		ZDropEdit VerticalAlignmentDropEdit;
		ZPictureBox HelpPictureBox;
		internal ImageSelectionControl ImageWatermarkImageSelectionControl;
		ZCalcEdit OpacityCalcEdit;
		ZLabel OpacityPercentLabel;
		ZLabel FontPtsLabel;
		ZCalcEdit HorizontalOffsetCalcEdit;
		ZCalcEdit VerticalOffsetCalcEdit;
		ZGroupBox ImageWatermarkGroupBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.TextWatermarkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImageWatermarkImageSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			this.TextWatermarkTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImageWatermarkGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TextWatermarkGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FontPtsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OpacityPercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OpacityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FontSizeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RotationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HorizontalAlignmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VerticalAlignmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HelpPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.HorizontalOffsetCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VerticalOffsetCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImageWatermarkGroupBox.SuspendLayout();
			this.TextWatermarkGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.Watermark);
			// 
			// TextWatermarkCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TextWatermarkCheckBox, "UseTextWatermark");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).UseTextWatermark)));
			this.TextWatermarkCheckBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|2a533d9b-fd3d-4859-a1df-2fc68876028f", "Use Text Watermark");
			this.TextWatermarkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TextWatermarkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TextWatermarkCheckBox.Name = "TextWatermarkCheckBox";
			this.TextWatermarkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.TextWatermarkCheckBox.TabIndex = 0;
			// 
			// ImageWatermarkImageSelectionControl
			// 
			this.ImageWatermarkImageSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImageWatermarkImageSelectionControl, "ImageWatermark");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).ImageWatermark)));
			this.ImageWatermarkImageSelectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImageWatermarkImageSelectionControl.FileDialogFilter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
			this.ImageWatermarkImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ImageWatermarkImageSelectionControl.Name = "ImageWatermarkImageSelectionControl";
			this.ImageWatermarkImageSelectionControl.ReadOnly = true;
			this.ImageWatermarkImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 173, true);
			this.ImageWatermarkImageSelectionControl.TabIndex = 0;
			// 
			// TextWatermarkTextBox
			// 
			this.TextWatermarkTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextWatermarkTextBox, "TextWatermark");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).TextWatermark)));
			this.TextWatermarkTextBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|054dc24b-0c7c-4892-8dab-d4f1da2e9729", "Text");
			this.TextWatermarkTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextWatermarkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.TextWatermarkTextBox.Name = "TextWatermarkTextBox";
			this.TextWatermarkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TextWatermarkTextBox.TabIndex = 1;
			// 
			// ImageWatermarkGroupBox
			// 
			this.ImageWatermarkGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ImageWatermarkGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|3ef0efaa-d8e3-4918-9192-79fb1298e4d1", "Image Watermark");
			this.ImageWatermarkGroupBox.Controls.Add(this.ImageWatermarkImageSelectionControl);
			this.ImageWatermarkGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.ImageWatermarkGroupBox.Name = "ImageWatermarkGroupBox";
			this.ImageWatermarkGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 192, true);
			this.ImageWatermarkGroupBox.TabIndex = 11;
			this.ImageWatermarkGroupBox.TabStop = false;
			// 
			// TextWatermarkGroupBox
			// 
			this.TextWatermarkGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TextWatermarkGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|c23c343b-98be-47b9-9676-498057a1f74f", "Text Watermark");
			this.TextWatermarkGroupBox.Controls.Add(this.FontPtsLabel);
			this.TextWatermarkGroupBox.Controls.Add(this.OpacityPercentLabel);
			this.TextWatermarkGroupBox.Controls.Add(this.OpacityCalcEdit);
			this.TextWatermarkGroupBox.Controls.Add(this.TextWatermarkTextBox);
			this.TextWatermarkGroupBox.Controls.Add(this.FontSizeCalcEdit);
			this.TextWatermarkGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.TextWatermarkGroupBox.Name = "TextWatermarkGroupBox";
			this.TextWatermarkGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 92, true);
			this.TextWatermarkGroupBox.TabIndex = 10;
			this.TextWatermarkGroupBox.TabStop = false;
			// 
			// FontPtsLabel
			// 
			this.FontPtsLabel.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|14bd21a7-181f-4d23-b8c9-52dc5dba1378", "pt");
			this.FontPtsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 40, true);
			this.FontPtsLabel.Name = "FontPtsLabel";
			this.FontPtsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.FontPtsLabel.TabIndex = 4;
			// 
			// OpacityPercentLabel
			// 
			this.OpacityPercentLabel.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|d591918b-5dde-409c-9456-6bde29220b15", "%");
			this.OpacityPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 64, true);
			this.OpacityPercentLabel.Name = "OpacityPercentLabel";
			this.OpacityPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.OpacityPercentLabel.TabIndex = 7;
			// 
			// OpacityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OpacityCalcEdit, "Opacity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).Opacity)));
			this.OpacityCalcEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|a88fe9da-cf4d-4763-838d-6826caebc2bb", "Opacity");
			this.OpacityCalcEdit.DecimalPlaces = 0;
			this.OpacityCalcEdit.Decimals = 0;
			this.OpacityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.OpacityCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0 });
			this.OpacityCalcEdit.Name = "OpacityCalcEdit";
			this.OpacityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.OpacityCalcEdit.TabIndex = 6;
			this.OpacityCalcEdit.Text = "0";
			this.OpacityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FontSizeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FontSizeCalcEdit, "FontSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).FontSize)));
			this.FontSizeCalcEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|4850d831-3830-4f0c-9d8d-892ba5c0c59d", "Font Size");
			this.FontSizeCalcEdit.DecimalPlaces = 0;
			this.FontSizeCalcEdit.Decimals = 0;
			this.FontSizeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.FontSizeCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0 });
			this.FontSizeCalcEdit.Name = "FontSizeCalcEdit";
			this.FontSizeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.FontSizeCalcEdit.TabIndex = 3;
			this.FontSizeCalcEdit.Text = "0";
			this.FontSizeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RotationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RotationCalcEdit, "Rotation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).Rotation)));
			this.RotationCalcEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|9c92e3c4-7ea3-46f5-a3de-1e3cb4ee5e68", "Rotation in Degrees");
			this.RotationCalcEdit.DecimalPlaces = 0;
			this.RotationCalcEdit.Decimals = 0;
			this.RotationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 76, true);
			this.RotationCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0 });
			this.RotationCalcEdit.Name = "RotationCalcEdit";
			this.RotationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.RotationCalcEdit.TabIndex = 9;
			this.RotationCalcEdit.Text = "0";
			this.RotationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// HorizontalAlignmentDropEdit
			// 
			this.HorizontalAlignmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HorizontalAlignmentDropEdit, "HorizontalAlignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).HorizontalAlignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).HorizontalAlignmentList)));
			this.HorizontalAlignmentDropEdit.BindToList = "HorizontalAlignmentList";
			this.HorizontalAlignmentDropEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|f2c364ab-42c4-439c-a6e2-1200de97f43c", "Horizontal Alignment");
			this.HorizontalAlignmentDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HorizontalAlignmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 28, true);
			this.HorizontalAlignmentDropEdit.Name = "HorizontalAlignmentDropEdit";
			this.HorizontalAlignmentDropEdit.PreBoundMaxLength = 6;
			this.HorizontalAlignmentDropEdit.ShowDescriptionBox = false;
			this.HorizontalAlignmentDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.HorizontalAlignmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.HorizontalAlignmentDropEdit.TabIndex = 1;
			// 
			// VerticalAlignmentDropEdit
			// 
			this.VerticalAlignmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VerticalAlignmentDropEdit, "VerticalAlignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).VerticalAlignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).VerticalAlignmentList)));
			this.VerticalAlignmentDropEdit.BindToList = "VerticalAlignmentList";
			this.VerticalAlignmentDropEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|a87d3e97-8421-4a3e-aec3-bbc1c1bcdfe3", "Vertical Alignment");
			this.VerticalAlignmentDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.VerticalAlignmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 52, true);
			this.VerticalAlignmentDropEdit.Name = "VerticalAlignmentDropEdit";
			this.VerticalAlignmentDropEdit.PreBoundMaxLength = 6;
			this.VerticalAlignmentDropEdit.ShowDescriptionBox = false;
			this.VerticalAlignmentDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.VerticalAlignmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.VerticalAlignmentDropEdit.TabIndex = 5;
			// 
			// HelpPictureBox
			// 
			this.HelpPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HelpPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 3, true);
			this.HelpPictureBox.Name = "HelpPictureBox";
			this.HelpPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 188, true);
			this.HelpPictureBox.TabIndex = 10;
			this.HelpPictureBox.TabStop = false;
			// 
			// HorizontalOffsetCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.HorizontalOffsetCalcEdit, "HorizontalOffset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).HorizontalOffset)));
			this.HorizontalOffsetCalcEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|89a81f87-6e45-4e43-b9e4-eb5f88c3c741", "Offset");
			this.HorizontalOffsetCalcEdit.DecimalPlaces = 0;
			this.HorizontalOffsetCalcEdit.Decimals = 0;
			this.HorizontalOffsetCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 28, true);
			this.HorizontalOffsetCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0 });
			this.HorizontalOffsetCalcEdit.Name = "HorizontalOffsetCalcEdit";
			this.HorizontalOffsetCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.HorizontalOffsetCalcEdit.TabIndex = 3;
			this.HorizontalOffsetCalcEdit.Text = "0";
			this.HorizontalOffsetCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VerticalOffsetCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VerticalOffsetCalcEdit, "VerticalOffset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngineCore.Registry.Watermark)(null)).VerticalOffset)));
			this.VerticalOffsetCalcEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("WatermarkControl|2901aea0-8489-41b7-81ae-0d2cc3c6660c", "Offset");
			this.VerticalOffsetCalcEdit.DecimalPlaces = 0;
			this.VerticalOffsetCalcEdit.Decimals = 0;
			this.VerticalOffsetCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 52, true);
			this.VerticalOffsetCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0 });
			this.VerticalOffsetCalcEdit.Name = "VerticalOffsetCalcEdit";
			this.VerticalOffsetCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.VerticalOffsetCalcEdit.TabIndex = 7;
			this.VerticalOffsetCalcEdit.Text = "0";
			this.VerticalOffsetCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WatermarkControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VerticalOffsetCalcEdit);
			this.Controls.Add(this.HorizontalAlignmentDropEdit);
			this.Controls.Add(this.TextWatermarkGroupBox);
			this.Controls.Add(this.ImageWatermarkGroupBox);
			this.Controls.Add(this.TextWatermarkCheckBox);
			this.Controls.Add(this.VerticalAlignmentDropEdit);
			this.Controls.Add(this.HelpPictureBox);
			this.Controls.Add(this.HorizontalOffsetCalcEdit);
			this.Controls.Add(this.RotationCalcEdit);
			this.Name = "WatermarkControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 392, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImageWatermarkGroupBox.ResumeLayout(false);
			this.TextWatermarkGroupBox.ResumeLayout(false);
			this.TextWatermarkGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HelpPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
