using System;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	internal partial class EmailFormatControl : RegistryBusinessObjectTemplateZUserControl
	{
		private ZGroupBox EmailSignatureGroupBox;
		private ZGroupBox EmailSubjectGroupBox;
		private ZLabel DisclaimerLabel;
		private ZGrid EmailSubjectFieldsGrid;
		private ZGrid EmailSignatureFieldsGrid;
		private ZTextBox DisclaimerTextBox;
		private ZLabel NoteLabel;
		private ZTextBox SeparatorTextBox;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.SeparatorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmailSignatureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DisclaimerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmailSignatureFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DisclaimerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailSubjectGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailSubjectFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EmailSignatureGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EmailSignatureFieldsGrid)).BeginInit();
			this.EmailSignatureFieldsGrid.SuspendLayout();
			this.EmailSubjectGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EmailSubjectFieldsGrid)).BeginInit();
			this.EmailSubjectFieldsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.EmailFormat);
			// 
			// SeparatorTextBox
			// 
			this.BindingSource.SetBindingMember(this.SeparatorTextBox, "Separator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).Separator)));
			this.SeparatorTextBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|cef1f88e-84da-477b-8214-f9ef47f2d1ee", "Field Separator");
			this.SeparatorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 19, true);
			this.SeparatorTextBox.Name = "SeparatorTextBox";
			this.SeparatorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 17, true);
			this.SeparatorTextBox.TabIndex = 2;
			// 
			// EmailSignatureGroupBox
			// 
			this.EmailSignatureGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.EmailSignatureGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|9c7095a1-9013-4282-8b8e-711a48eb42e0", "Email Signature");
			this.EmailSignatureGroupBox.Controls.Add(this.DisclaimerTextBox);
			this.EmailSignatureGroupBox.Controls.Add(this.EmailSignatureFieldsGrid);
			this.EmailSignatureGroupBox.Controls.Add(this.DisclaimerLabel);
			this.EmailSignatureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 160, true);
			this.EmailSignatureGroupBox.Name = "EmailSignatureGroupBox";
			this.EmailSignatureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 268, true);
			this.EmailSignatureGroupBox.TabIndex = 1;
			this.EmailSignatureGroupBox.TabStop = false;
			// 
			// DisclaimerTextBox
			// 
			this.DisclaimerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DisclaimerTextBox, "Disclaimer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).Disclaimer)));
			this.DisclaimerTextBox.CaptionResourceString = null;
			this.DisclaimerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisclaimerTextBox, false);
			this.DisclaimerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 176, true);
			this.DisclaimerTextBox.Multiline = true;
			this.DisclaimerTextBox.MaxLength = new DocumentEngineCore.Registry.EmailFormat().DisclaimerInfo.MaxLength;
			this.DisclaimerTextBox.Name = "DisclaimerTextBox";
			this.DisclaimerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 84, true);
			this.DisclaimerTextBox.TabIndex = 3;
			this.DisclaimerTextBox.TextChanged += OnTextChanged;
			// 
			// EmailSignatureFieldsGrid
			// 
			this.EmailSignatureFieldsGrid.AllowNavigation = false;
			this.EmailSignatureFieldsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EmailSignatureFieldsGrid, "EmailSignatureFields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSignatureFields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailSignatureField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSignatureFields)).SyncRoot)).Index)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailSignatureField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSignatureFields)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailSignatureField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSignatureFields)).SyncRoot)).EmailFieldsPairList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailSignatureField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSignatureFields)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailSignatureField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSignatureFields)).SyncRoot)).EmailFieldsPairList)));
			this.EmailSignatureFieldsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|f62cf456-4f51-4589-9f46-aa2389801f75", "Index");
			zTextBoxColumnStyleInfo1.ColumnName = "Index";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.BindToList = "EmailFieldsPairList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|81068e24-93e2-40d0-b665-fabe602a46d1", "Signature Field Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "EmailFieldsPairList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|833e70da-737b-4b7b-addf-7e78f6b42941", "Signature Field Description");
			zDropEditColumnStyleInfo2.ColumnName = "Description";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EmailSignatureFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EmailSignatureFieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EmailSignatureFieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EmailSignatureFieldsGrid.GridId = "c1f0a358-1e9c-47e9-833e-f8cc0ac8f251";
			this.EmailSignatureFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EmailSignatureFieldsGrid.LayoutKey = "EmailSignatureFieldsGrid";
			this.EmailSignatureFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 26, true);
			this.EmailSignatureFieldsGrid.Name = "EmailSignatureFieldsGrid";
			this.EmailSignatureFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 127, true);
			this.EmailSignatureFieldsGrid.TabIndex = 2;
			// 
			// DisclaimerLabel
			// 
			this.DisclaimerLabel.AutoSize = true;
			this.DisclaimerLabel.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|407bd533-b8f2-4401-8f6a-04e1d64a7bc2", "Disclaimer");
			this.DisclaimerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DisclaimerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 158, true);
			this.DisclaimerLabel.Name = "DisclaimerLabel";
			this.DisclaimerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.DisclaimerLabel.TabIndex = 0;
			OnTextChanged(null, EventArgs.Empty);
			// 
			// EmailSubjectGroupBox
			// 
			this.EmailSubjectGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.EmailSubjectGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|943f9811-e4fd-40c8-aaf7-f08215d30479", "Email Subject");
			this.EmailSubjectGroupBox.Controls.Add(this.NoteLabel);
			this.EmailSubjectGroupBox.Controls.Add(this.EmailSubjectFieldsGrid);
			this.EmailSubjectGroupBox.Controls.Add(this.SeparatorTextBox);
			this.EmailSubjectGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EmailSubjectGroupBox.Name = "EmailSubjectGroupBox";
			this.EmailSubjectGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 154, true);
			this.EmailSubjectGroupBox.TabIndex = 0;
			this.EmailSubjectGroupBox.TabStop = false;
			// 
			// NoteLabel
			// 
			this.NoteLabel.AutoSize = true;
			this.NoteLabel.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|52dc91f6-dfef-4024-9d72-d7a5fa76a7f4", "NOTE: Empty spaces are also included.");
			this.NoteLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 22, true);
			this.NoteLabel.Name = "NoteLabel";
			this.NoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 13, true);
			this.NoteLabel.TabIndex = 4;
			// 
			// EmailSubjectFieldsGrid
			// 
			this.EmailSubjectFieldsGrid.AllowNavigation = false;
			this.EmailSubjectFieldsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EmailSubjectFieldsGrid, "EmailSubjectFields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSubjectFields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailSubjectField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSubjectFields)).SyncRoot)).Index)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailSubjectField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSubjectFields)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailSubjectField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSubjectFields)).SyncRoot)).EmailFieldsPairList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.EmailSubjectField)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.EmailFormat)(null)).EmailSubjectFields)).SyncRoot)).Description)));
			this.EmailSubjectFieldsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|d5628a8b-3d70-4eff-adb9-1dfe9b881665", "Index");
			zTextBoxColumnStyleInfo2.ColumnName = "Index";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo3.BindToList = "EmailFieldsPairList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|dae575af-8e40-473b-bc26-8564e357111c", "Subject Field Code");
			zDropEditColumnStyleInfo3.ColumnName = "Code";
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("EmailFormatControl|275c9729-15f2-4785-98ca-80974858655e", "Subject Field Description");
			zDropEditColumnStyleInfo4.ColumnName = "Description";
			zDropEditColumnStyleInfo4.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EmailSubjectFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EmailSubjectFieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EmailSubjectFieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EmailSubjectFieldsGrid.GridId = "7acc7ea1-fe10-4c28-8b69-906890dabcb9";
			this.EmailSubjectFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EmailSubjectFieldsGrid.LayoutKey = "zGrid1";
			this.EmailSubjectFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 51, true);
			this.EmailSubjectFieldsGrid.Name = "EmailSubjectFieldsGrid";
			this.EmailSubjectFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 97, true);
			this.EmailSubjectFieldsGrid.TabIndex = 3;
			// 
			// EmailFormatControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EmailSubjectGroupBox);
			this.Controls.Add(this.EmailSignatureGroupBox);
			this.Name = "EmailFormatControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 432, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EmailSignatureGroupBox.ResumeLayout(false);
			this.EmailSignatureGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EmailSignatureFieldsGrid)).EndInit();
			this.EmailSignatureFieldsGrid.ResumeLayout(false);
			this.EmailSignatureFieldsGrid.PerformLayout();
			this.EmailSubjectGroupBox.ResumeLayout(false);
			this.EmailSubjectGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EmailSubjectFieldsGrid)).EndInit();
			this.EmailSubjectFieldsGrid.ResumeLayout(false);
			this.EmailSubjectFieldsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
