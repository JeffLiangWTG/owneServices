using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	partial class PrincipalBrandingControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BrandingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BrandEmailAddressBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImageBoundImageSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			this.ImageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BrandDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.useDomainCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UseGenericBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BrandMappingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoteLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BrandingGrid)).BeginInit();
			this.BrandingGrid.SuspendLayout();
			this.ImageBoundImageSelectionControl.SuspendLayout();
			this.ImageGroupBox.SuspendLayout();
			this.BrandDetailsGroupBox.SuspendLayout();
			this.BrandMappingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.PrincipalBrandingCollection);
			// 
			// BrandingGrid
			// 
			this.BrandingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BrandingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).Principals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).Code)));
			this.BrandingGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Principals";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|9fdc381c-5a9e-4c52-95d1-75bdb7bd288c", "Principal");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|72bf88f4-5776-4a9b-a79f-d3a4cefa3db2", "OBOL Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BrandingGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BrandingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BrandingGrid.CopySelectedRowsAllowed = true;
			this.BrandingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrandingGrid.GridId = "3872b0fc-a1fa-4819-8ff6-1b4f9a8a019f";
			this.BrandingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BrandingGrid.LayoutKey = "BrandingGrid";
			this.BrandingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.BrandingGrid.Name = "BrandingGrid";
			this.BrandingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 50, true);
			this.BrandingGrid.TabIndex = 2;
			// 
			// BrandEmailAddressBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandEmailAddressBoundTextBox, "BrandEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).BrandEmailAddress)));
			this.BrandEmailAddressBoundTextBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|fbc33bad-4092-43c5-92a6-944b8787e151", "Generic Email Address");
			this.BrandEmailAddressBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 40, true);
			this.BrandEmailAddressBoundTextBox.Name = "BrandEmailAddressBoundTextBox";
			this.BrandEmailAddressBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.BrandEmailAddressBoundTextBox.TabIndex = 0;
			// 
			// BrandNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameBoundTextBox, "BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).BrandName)));
			this.BrandNameBoundTextBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|6872cd2e-3910-450c-b4fb-c87af2cf2343", "Name");
			this.BrandNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.BrandNameBoundTextBox.Name = "BrandNameBoundTextBox";
			this.BrandNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.BrandNameBoundTextBox.TabIndex = 0;
			// 
			// ImageBoundImageSelectionControl
			// 
			this.ImageBoundImageSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImageBoundImageSelectionControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).Image)));
			this.ImageBoundImageSelectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImageBoundImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ImageBoundImageSelectionControl.Name = "ImageBoundImageSelectionControl";
			this.ImageBoundImageSelectionControl.ReadOnly = false;
			this.ImageBoundImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 102, true);
			this.ImageBoundImageSelectionControl.TabIndex = 3;
			// 
			// ImageGroupBox
			// 
			this.ImageGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|2fb7e284-3f1d-4d9e-aa45-e9c84b89e244", "Brand Letterhead");
			this.ImageGroupBox.Controls.Add(this.ImageBoundImageSelectionControl);
			this.ImageGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ImageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 241, true);
			this.ImageGroupBox.Name = "ImageGroupBox";
			this.ImageGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.ImageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 127, true);
			this.ImageGroupBox.TabIndex = 4;
			this.ImageGroupBox.TabStop = false;
			// 
			// BrandDetailsGroupBox
			// 
			this.BrandDetailsGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|00afb997-c464-41d3-95ed-471995a7ae8a", "Branding Details");
			this.BrandDetailsGroupBox.Controls.Add(this.useDomainCheckbox);
			this.BrandDetailsGroupBox.Controls.Add(this.UseGenericBoundCheckBox);
			this.BrandDetailsGroupBox.Controls.Add(this.BrandNameBoundTextBox);
			this.BrandDetailsGroupBox.Controls.Add(this.BrandEmailAddressBoundTextBox);
			this.BrandDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BrandDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 101, true);
			this.BrandDetailsGroupBox.Name = "BrandDetailsGroupBox";
			this.BrandDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 140, true);
			this.BrandDetailsGroupBox.TabIndex = 4;
			this.BrandDetailsGroupBox.TabStop = false;
			// 
			// useDomainCheckbox
			// 
			this.BindingSource.SetBindingMember(this.useDomainCheckbox, "ReplaceDomainNames");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).ReplaceDomainNames)));
			this.useDomainCheckbox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("4432e75c-feb2-4fa1-b6c5-5e1350410fa3", "Tick this box to update Sender email address with domain from the above email address for all communication");
			this.useDomainCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useDomainCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 102, true);
			this.useDomainCheckbox.Name = "useDomainCheckbox";
			this.useDomainCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 32, true);
			this.useDomainCheckbox.TabIndex = 8;
			this.useDomainCheckbox.UseVisualStyleBackColor = true;
			// 
			// UseGenericBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UseGenericBoundCheckBox, "UseGeneric");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngineCore.Registry.PrincipalBranding)(null)).UseGeneric)));
			this.UseGenericBoundCheckBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|b9ed2bdb-abbe-4cf1-9303-970c505f2d54", "Tick this box to use the above email address for all communications.");
			this.UseGenericBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseGenericBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 64, true);
			this.UseGenericBoundCheckBox.Name = "UseGenericBoundCheckBox";
			this.UseGenericBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 32, true);
			this.UseGenericBoundCheckBox.TabIndex = 7;
			this.UseGenericBoundCheckBox.UseVisualStyleBackColor = true;
			// 
			// BrandMappingGroupBox
			// 
			this.BrandMappingGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|1f9273b2-c18a-480e-8248-a8d3cd952c75", "Brand Mapping");
			this.BrandMappingGroupBox.Controls.Add(this.BrandingGrid);
			this.BrandMappingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrandMappingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BrandMappingGroupBox.Name = "BrandMappingGroupBox";
			this.BrandMappingGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.BrandMappingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 75, true);
			this.BrandMappingGroupBox.TabIndex = 4;
			this.BrandMappingGroupBox.TabStop = false;
			// 
			// NoteLabel
			// 
			this.NoteLabel.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("PrincipalBrandingControl|0c2882a1-f846-438d-9a6c-a6ea43a37e52", "* Select a row from the above list to view further details below.");
			this.NoteLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 78, true);
			this.NoteLabel.Name = "NoteLabel";
			this.NoteLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.NoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 23, true);
			this.NoteLabel.TabIndex = 5;
			// 
			// PrincipalBrandingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrandMappingGroupBox);
			this.Controls.Add(this.NoteLabel);
			this.Controls.Add(this.BrandDetailsGroupBox);
			this.Controls.Add(this.ImageGroupBox);
			this.Name = "PrincipalBrandingControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 371, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BrandingGrid)).EndInit();
			this.BrandingGrid.ResumeLayout(false);
			this.BrandingGrid.PerformLayout();
			this.ImageBoundImageSelectionControl.ResumeLayout(true);
			this.ImageBoundImageSelectionControl.PerformLayout();
			this.ImageGroupBox.ResumeLayout(false);
			this.ImageGroupBox.PerformLayout();
			this.BrandDetailsGroupBox.ResumeLayout(false);
			this.BrandDetailsGroupBox.PerformLayout();
			this.BrandMappingGroupBox.ResumeLayout(false);
			this.BrandMappingGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid BrandingGrid;
		private Enterprise.ZArchitecture.ZTextBox BrandNameBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox BrandEmailAddressBoundTextBox;
		private ImageSelectionControl ImageBoundImageSelectionControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ImageGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BrandDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BrandMappingGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UseGenericBoundCheckBox;
		private Enterprise.ZArchitecture.ZLabel NoteLabel;
		private ZCheckBox useDomainCheckbox;

	}
}
