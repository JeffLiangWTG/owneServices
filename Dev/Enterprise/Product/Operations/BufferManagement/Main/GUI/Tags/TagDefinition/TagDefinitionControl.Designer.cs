namespace Enterprise.BufferManagement.GUI
{
	partial class TagDefinitionControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.TagMagnitudeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MagnitudeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MagnitudeExaplanationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsExclusiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UsageScopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ScopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TagMagnitudeGrid)).BeginInit();
			this.MagnitudeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.TagDefinition);
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "TGD_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).TGD_Code)));
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 4, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.CodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "TGD_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).TGD_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 30, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.DescriptionTextBox.TabIndex = 4;
			// 
			// TagMagnitudeGrid
			// 
			this.TagMagnitudeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TagMagnitudeGrid, "Magnitudes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).TGM_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).TGM_NudgeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).TGM_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).TGM_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).TGM_GG_OwnerGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).TGM_RuleRunSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).VisualStylePriority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).Color)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).BorderStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).ApplyColorToBorder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagMagnitude)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).Magnitudes)).SyncRoot)).ApplyColorToBackground)));
			this.TagMagnitudeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "TGM_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TGM_NudgeAmount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "TGM_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo1.ColumnName = "TGM_IsActive";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TGM_GG_OwnerGroup";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TGM_RuleRunSequence";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "VisualStylePriority";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "Color";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "BorderStyle";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "ApplyColorToBorder";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo3.ColumnName = "ApplyColorToBackground";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TagMagnitudeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TagMagnitudeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TagMagnitudeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TagMagnitudeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TagMagnitudeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TagMagnitudeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TagMagnitudeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TagMagnitudeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TagMagnitudeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TagMagnitudeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.TagMagnitudeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.TagMagnitudeGrid.CopySelectedRowsAllowed = true;
			this.TagMagnitudeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TagMagnitudeGrid.GridId = "5b4e7ed3-10f7-47e5-845c-30402c313eef";
			this.TagMagnitudeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TagMagnitudeGrid.LayoutKey = "TagMagnitudeGrid";
			this.TagMagnitudeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TagMagnitudeGrid.Name = "TagMagnitudeGrid";
			this.TagMagnitudeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 151, true);
			this.TagMagnitudeGrid.TabIndex = 7;
			this.TagMagnitudeGrid.RowsDeleting += new System.EventHandler<Enterprise.ZArchitecture.RowsDeletingEventArgs>(this.TagMagnitudeGrid_RowsDeleting);
			// 
			// MagnitudeGroupBox
			// 
			this.MagnitudeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MagnitudeGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0c7c2c90-738f-482c-8bc9-fcee8a4a23ff", "Tags", "Tag groups contain tags that can be applied to workflows.");
			this.MagnitudeGroupBox.Controls.Add(this.TagMagnitudeGrid);
			this.MagnitudeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 142, true);
			this.MagnitudeGroupBox.Name = "MagnitudeGroupBox";
			this.MagnitudeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 170, true);
			this.MagnitudeGroupBox.TabIndex = 3;
			this.MagnitudeGroupBox.TabStop = false;
			// 
			// MagnitudeExaplanationLabel
			// 
			this.MagnitudeExaplanationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MagnitudeExaplanationLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d6604326-c99c-4d28-a14e-19f8ce4a3622", "", "The tags defined below can be applied to workflows. Tags can be used to apply visual effects or to enable custom transfer rules.");
			this.MagnitudeExaplanationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 108, true);
			this.MagnitudeExaplanationLabel.Name = "MagnitudeExaplanationLabel";
			this.MagnitudeExaplanationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 32, true);
			this.MagnitudeExaplanationLabel.TabIndex = 3;
			// 
			// IsExclusiveCheckBox
			// 
			this.IsExclusiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsExclusiveCheckBox, "TGD_IsExclusive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).TGD_IsExclusive)));
			this.IsExclusiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsExclusiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 6, true);
			this.IsExclusiveCheckBox.Name = "IsExclusiveCheckBox";
			this.IsExclusiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.IsExclusiveCheckBox.TabIndex = 1;
			this.IsExclusiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "TGD_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).TGD_IsSystem)));
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 6, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.IsSystemCheckBox.TabIndex = 3;
			this.IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// UsageScopeDropEdit
			// 
			this.UsageScopeDropEdit.AllowDrop = true;
			this.UsageScopeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UsageScopeDropEdit, "TGD_UsageScope");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).TGD_UsageScope)));
			this.UsageScopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 56, true);
			this.UsageScopeDropEdit.Name = "UsageScopeDropEdit";
			this.UsageScopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.UsageScopeDropEdit.TabIndex = 5;
			// 
			// ScopeDropEdit
			// 
			this.ScopeDropEdit.AllowDrop = true;
			this.ScopeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ScopeDropEdit, "TGD_Scope");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.TagDefinition)(null)).TGD_Scope)));
			this.ScopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 82, true);
			this.ScopeDropEdit.Name = "ScopeDropEdit";
			this.ScopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.ScopeDropEdit.TabIndex = 6;
			// 
			// TagDefinitionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ScopeDropEdit);
			this.Controls.Add(this.UsageScopeDropEdit);
			this.Controls.Add(this.IsSystemCheckBox);
			this.Controls.Add(this.IsExclusiveCheckBox);
			this.Controls.Add(this.MagnitudeExaplanationLabel);
			this.Controls.Add(this.MagnitudeGroupBox);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.CodeTextBox);
			this.Name = "TagDefinitionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 315, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TagMagnitudeGrid)).EndInit();
			this.MagnitudeGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.ZTranslatableTextControl DescriptionTextBox;
		public ZArchitecture.ZGrid TagMagnitudeGrid;
		private ZArchitecture.GUI.ZGroupBox MagnitudeGroupBox;
		private ZArchitecture.ZLabel MagnitudeExaplanationLabel;
		private ZArchitecture.GUI.ZCheckBox IsExclusiveCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		private ZArchitecture.GUI.ZDropEdit UsageScopeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ScopeDropEdit;

	}
}
