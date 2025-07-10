using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class DocumentBrandingControl : ClientAndAgentBrandingControl
	{
		ZLabel NoteLabel;
		ZGroupBox BrandDetailsGroupBox;
		ZTextBox BrandEmailTextBox;
		ZTextBox BrandNameTextBox;
		internal ZCheckBox replaceDomainNamesCheckbox;
		internal ZCheckBox UseGenericCheckBox;

		void InitializeComponent()
		{
			this.NoteLabel = new ZLabel();
			this.BrandDetailsGroupBox = new ZGroupBox();
			this.replaceDomainNamesCheckbox = new ZCheckBox();
			this.BrandEmailTextBox = new ZTextBox();
			this.BrandNameTextBox = new ZTextBox();
			this.UseGenericCheckBox = new ZCheckBox();
			this.ImageGroupBox.SuspendLayout();
			this.ImageControl.SuspendLayout();
			((ISupportInitialize)(this.CodeAndDescriptionGrid)).BeginInit();
			this.CodeAndDescriptionGrid.SuspendLayout();
			this.BrandMappingGroupBox.SuspendLayout();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BrandDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ImageGroupBox
			// 
			this.ImageGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(3, 300, true);
			this.ImageGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(424, 127, true);
			// 
			// ImageControl
			// 
			this.ImageControl.Size = ControlDpiScalingHelper.NewScaledSize(412, 102, true);
			// 
			// CodeAndDescriptionGrid
			// 
			this.CodeAndDescriptionGrid.Size = ControlDpiScalingHelper.NewScaledSize(412, 98, true);
			// 
			// BrandMappingGroupBox
			// 
			this.BrandMappingGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(424, 123, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DocumentBrandingCollection);
			// 
			// NoteLabel
			// 
			this.NoteLabel.CaptionResourceString = Res.GetData("DocumentBrandingControl|d04f7a31-d3fc-4e15-bb40-d693bcb533aa", "* Select a row from the above list to view further details below.");
			this.NoteLabel.Dock = DockStyle.Bottom;
			this.NoteLabel.Location = ControlDpiScalingHelper.NewScaledPoint(3, 126, true);
			this.NoteLabel.Name = "NoteLabel";
			this.NoteLabel.Padding = ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.NoteLabel.Size = ControlDpiScalingHelper.NewScaledSize(424, 23, true);
			this.NoteLabel.TabIndex = 7;
			// 
			// BrandDetailsGroupBox
			// 
			this.BrandDetailsGroupBox.CaptionResourceString = Res.GetData("DocumentBrandingControl|28c8b20e-0604-4031-86b8-2787c8fa69b2", "Brand Details");
			this.BrandDetailsGroupBox.Controls.Add(this.replaceDomainNamesCheckbox);
			this.BrandDetailsGroupBox.Controls.Add(this.BrandEmailTextBox);
			this.BrandDetailsGroupBox.Controls.Add(this.BrandNameTextBox);
			this.BrandDetailsGroupBox.Controls.Add(this.UseGenericCheckBox);
			this.BrandDetailsGroupBox.Dock = DockStyle.Bottom;
			this.BrandDetailsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(3, 149, true);
			this.BrandDetailsGroupBox.Name = "BrandDetailsGroupBox";
			this.BrandDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(424, 151, true);
			this.BrandDetailsGroupBox.TabIndex = 6;
			this.BrandDetailsGroupBox.TabStop = false;
			// 
			// replaceDomainNamesCheckbox
			// 
			this.BindingSource.SetBindingMember(this.replaceDomainNamesCheckbox, "ReplaceDomainNames");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZBool)(((DocumentBrandingBusinessObject)(null)).ReplaceDomainNames)));
			this.replaceDomainNamesCheckbox.CaptionResourceString = Res.GetData("bb149cf7-ad7e-403c-a223-2b1515672271", "Tick this box to update Sender email address with domain from the above email address for all communication");
			this.replaceDomainNamesCheckbox.FlatStyle = FlatStyle.System;
			this.replaceDomainNamesCheckbox.Location = ControlDpiScalingHelper.NewScaledPoint(124, 103, true);
			this.replaceDomainNamesCheckbox.Name = "replaceDomainNamesCheckbox";
			this.replaceDomainNamesCheckbox.Size = ControlDpiScalingHelper.NewScaledSize(283, 43, true);
			this.replaceDomainNamesCheckbox.TabIndex = 7;
			this.replaceDomainNamesCheckbox.UseVisualStyleBackColor = true;
			// 
			// BrandEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandEmailTextBox, "BrandEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((DocumentBrandingBusinessObject)(null)).BrandEmailAddress)));
			this.BrandEmailTextBox.CaptionResourceString = Res.GetData("DocumentBrandingControl|0bceda61-0ac0-4fe2-a0c0-dfd51476cceb", "Generic Email Address");
			this.BrandEmailTextBox.CharacterCasing = CharacterCasing.Normal;
			this.BrandEmailTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(124, 37, true);
			this.BrandEmailTextBox.Name = "BrandEmailTextBox";
			this.BrandEmailTextBox.Size = ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.BrandEmailTextBox.TabIndex = 4;
			this.BrandEmailTextBox.TextChanged += new EventHandler(this.BrandEmailTextBox_TextChanged);
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((DocumentBrandingBusinessObject)(null)).BrandName)));
			this.BrandNameTextBox.CaptionResourceString = Res.GetData("DocumentBrandingControl|dc490d6a-77c7-449a-8194-9126aac2e83d", "Name");
			this.BrandNameTextBox.CharacterCasing = CharacterCasing.Normal;
			this.BrandNameTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(124, 15, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.BrandNameTextBox.TabIndex = 3;
			// 
			// UseGenericCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UseGenericCheckBox, "UseGeneric");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZBool)(((DocumentBrandingBusinessObject)(null)).UseGeneric)));
			this.UseGenericCheckBox.CaptionResourceString = Res.GetData("ec5dc900-f880-4205-8303-3c296d7be74d", "Tick this box to use the above email address for all communications.");
			this.UseGenericCheckBox.FlatStyle = FlatStyle.System;
			this.UseGenericCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(124, 59, true);
			this.UseGenericCheckBox.Name = "UseGenericCheckBox";
			this.UseGenericCheckBox.Size = ControlDpiScalingHelper.NewScaledSize(283, 38, true);
			this.UseGenericCheckBox.TabIndex = 6;
			this.UseGenericCheckBox.UseVisualStyleBackColor = true;
			this.UseGenericCheckBox.CheckedChanged += new EventHandler(this.UseGenericCheckBox_CheckedChanged);
			// 
			// DocumentBrandingControl
			// 
			this.Controls.Add(this.NoteLabel);
			this.Controls.Add(this.BrandDetailsGroupBox);
			this.Name = "DocumentBrandingControl";
			this.Controls.SetChildIndex(this.ImageGroupBox, 0);
			this.Controls.SetChildIndex(this.BrandDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.NoteLabel, 0);
			this.Controls.SetChildIndex(this.BrandMappingGroupBox, 0);
			this.ImageGroupBox.ResumeLayout(false);
			this.ImageGroupBox.PerformLayout();
			this.ImageControl.ResumeLayout(true);
			this.ImageControl.PerformLayout();
			((ISupportInitialize)(this.CodeAndDescriptionGrid)).EndInit();
			this.CodeAndDescriptionGrid.ResumeLayout(false);
			this.CodeAndDescriptionGrid.PerformLayout();
			this.BrandMappingGroupBox.ResumeLayout(false);
			this.BrandMappingGroupBox.PerformLayout();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.BrandDetailsGroupBox.ResumeLayout(false);
			this.BrandDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
