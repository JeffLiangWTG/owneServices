namespace Enterprise.ResourceStrings.GUI
{
	partial class HelpDataStringForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.editReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.classNameTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.docBuilderUsageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.checkedOutCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SourceFileTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MediumCaptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.KeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CaptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShortCaptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonFixRN = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LanguageDropEdit.SuspendLayout();
			this.panel1.SuspendLayout();
			this.editReasonDropEdit.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 437, true);
			this.MainStatusBar.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 24, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(711);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.HelpDataString);
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.LanguageDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "HD_Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_Language)));
			this.LanguageDropEdit.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|135bf777-6379-4f1d-9dd3-fc89acb873eb", "Language", "Language for the resource string.");
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 304, true);
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 20, true);
			this.LanguageDropEdit.TabIndex = 5;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.editReasonDropEdit);
			this.panel1.Controls.Add(this.classNameTextbox);
			this.panel1.Controls.Add(this.docBuilderUsageButton);
			this.panel1.Controls.Add(this.postingButtonsUserControl);
			this.panel1.Controls.Add(this.checkedOutCheckbox);
			this.panel1.Controls.Add(this.SourceFileTextBox);
			this.panel1.Controls.Add(this.MediumCaptionTextBox);
			this.panel1.Controls.Add(this.KeyTextBox);
			this.panel1.Controls.Add(this.CaptionTextBox);
			this.panel1.Controls.Add(this.LanguageDropEdit);
			this.panel1.Controls.Add(this.ShortCaptionTextBox);
			this.panel1.Controls.Add(this.DescriptionTextBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 461, true);
			this.panel1.TabIndex = 0;
			// 
			// editReasonDropEdit
			// 
			this.editReasonDropEdit.AllowDrop = true;
			this.editReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.editReasonDropEdit, "HD_EditReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_EditReason)));
			this.editReasonDropEdit.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("3abad5a3-bb3d-4804-bb56-6e9f01edf451", "Edit Reason");
			this.editReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 441, true);
			this.editReasonDropEdit.Name = "editReasonDropEdit";
			this.editReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.editReasonDropEdit.TabIndex = 14;
			// 
			// classNameTextbox
			// 
			this.classNameTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.classNameTextbox, "HD_ContextClassName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_ContextClassName)));
			this.classNameTextbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("8720f26c-84e3-4463-ae46-4e66dc8f3708", "Source Class");
			this.classNameTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.classNameTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 332, true);
			this.classNameTextbox.Name = "classNameTextbox";
			this.classNameTextbox.ReadOnly = true;
			this.classNameTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 20, true);
			this.classNameTextbox.TabIndex = 12;
			// 
			// docBuilderUsageButton
			// 
			this.docBuilderUsageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.docBuilderUsageButton.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("6aab3e1d-3b7d-4e25-87dc-7273fbcbd1da", "Enable DocBuilder Usage Lookup");
			this.docBuilderUsageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 384, true);
			this.docBuilderUsageButton.Name = "docBuilderUsageButton";
			this.docBuilderUsageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 23, true);
			this.docBuilderUsageButton.TabIndex = 11;
			this.docBuilderUsageButton.UseVisualStyleBackColor = true;
			this.docBuilderUsageButton.Click += new System.EventHandler(this.docBuilderUsageButton_Click);
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 423, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 26, true);
			this.postingButtonsUserControl.TabIndex = 10;
			// 
			// checkedOutCheckbox
			// 
			this.checkedOutCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.checkedOutCheckbox, "HD_IsCheckedOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_IsCheckedOut)));
			this.checkedOutCheckbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|13c36b25-b717-48a2-98d3-f48ea8569605", "Checked Out");
			this.checkedOutCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkedOutCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 413, true);
			this.checkedOutCheckbox.Name = "checkedOutCheckbox";
			this.checkedOutCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.checkedOutCheckbox.TabIndex = 8;
			this.checkedOutCheckbox.UseVisualStyleBackColor = true;
			// 
			// SourceFileTextBox
			// 
			this.SourceFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SourceFileTextBox, "HD_ContextSourceFile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_ContextSourceFile)));
			this.SourceFileTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("56d9fd37-5d94-4127-8a2e-346e7e2c70bc", "Source File");
			this.SourceFileTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SourceFileTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 358, true);
			this.SourceFileTextBox.Name = "SourceFileTextBox";
			this.SourceFileTextBox.ReadOnly = true;
			this.SourceFileTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 20, true);
			this.SourceFileTextBox.TabIndex = 7;
			// 
			// MediumCaptionTextBox
			// 
			this.MediumCaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MediumCaptionTextBox, "HD_MidCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_MidCaption)));
			this.MediumCaptionTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|e1b50ced-889c-4c28-8a61-3b4c83a46665", "Medium Caption", "A Medium Caption should be the business name of a field with no abbreviations. It should be in title case. E.g. \"Port of First Arrival\". It is used to render field label captions.");
			this.MediumCaptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MediumCaptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 59, true);
			this.MediumCaptionTextBox.Name = "MediumCaptionTextBox";
			this.MediumCaptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 20, true);
			this.MediumCaptionTextBox.TabIndex = 2;
			// 
			// KeyTextBox
			// 
			this.KeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.KeyTextBox, "HD_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_Code)));
			this.KeyTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|41dbd5c9-007b-4789-9406-1dc93ed448d2", "Key", "Resource string key. This is system generated and should not be edited.");
			this.KeyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.KeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 7, true);
			this.KeyTextBox.Name = "KeyTextBox";
			this.KeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 20, true);
			this.KeyTextBox.TabIndex = 0;
			// 
			// CaptionTextBox
			// 
			this.CaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CaptionTextBox, "HD_Caption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_Caption)));
			this.CaptionTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|5dd0df4a-474f-4442-82d9-212481c491f5", "Caption", "A Caption should be the full business name of a field with no abbreviations. It should be in title case. E.g. \"Port of First Arrival\". It is displayed as the title of help balloons and used to render field label captions.");
			this.CaptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CaptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 85, true);
			this.CaptionTextBox.Name = "CaptionTextBox";
			this.CaptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 20, true);
			this.CaptionTextBox.TabIndex = 3;
			// 
			// ShortCaptionTextBox
			// 
			this.ShortCaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShortCaptionTextBox, "HD_ShortCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_ShortCaption)));
			this.ShortCaptionTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|23631386-bfd9-4885-85d8-217b98e0ca6b", "Short", "Short Caption", "");
			this.ShortCaptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ShortCaptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 33, true);
			this.ShortCaptionTextBox.Name = "ShortCaptionTextBox";
			this.ShortCaptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 20, true);
			this.ShortCaptionTextBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "HD_FullDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.HelpDataString)(null)).HD_FullDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|643e1edb-e2b2-47e8-b89c-e6895daea154", "Description", "The resource string description should be a description of the business function of the field. Rather than writing \"Enter a port.\", instead write \"The port of first arrival is the port at which the goods first entered Australia.\" All descriptions should be at least once sentence long, and should start with a capital and end with a full stop. Descriptions can be multiple sentences long, but should always start with a single sentence that describes the use of the field.");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 111, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 187, true);
			this.DescriptionTextBox.TabIndex = 4;
			// 
			// buttonFixRN
			// 
			this.buttonFixRN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonFixRN.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|0dc31797-66dc-46de-b5e4-868d07273e59", "Fix New Line marks");
			this.buttonFixRN.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 415, true);
			this.buttonFixRN.Name = "buttonFixRN";
			this.buttonFixRN.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.buttonFixRN.TabIndex = 5;
			this.buttonFixRN.UseVisualStyleBackColor = true;
			this.buttonFixRN.Click += new System.EventHandler(this.buttonFixRN_Click);
			// 
			// HelpDataStringForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("HelpDataStringForm|df84cf53-40d6-41fb-a431-3784967c1739", "Resource String Record");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 461, true);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.buttonFixRN);
			this.DataSourceAssemblyName = "Enterprise.ResourceStrings.Business";
			this.DataSourceType = typeof(Enterprise.ResourceStrings.Business.HelpDataString);
			this.DataSourceTypeName = "Enterprise.ResourceStrings.Business.HelpDataString";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			this.Name = "HelpDataStringForm";
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Controls.SetChildIndex(this.buttonFixRN, 0);
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.editReasonDropEdit.ResumeLayout(true);
			this.editReasonDropEdit.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit LanguageDropEdit;
		private CargoWise.Windows.UI.KPanel panel1;
		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private ZArchitecture.GUI.ZCheckBox checkedOutCheckbox;
		private ZArchitecture.GUI.ZButton buttonFixRN;
		private ZArchitecture.ZTextBox SourceFileTextBox;
		private ZArchitecture.ZTextBox MediumCaptionTextBox;
		private ZArchitecture.ZTextBox KeyTextBox;
		private ZArchitecture.ZTextBox CaptionTextBox;
		private ZArchitecture.ZTextBox ShortCaptionTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZButton docBuilderUsageButton;
		private ZArchitecture.ZTextBox classNameTextbox;
		private ZArchitecture.GUI.ZDropEdit editReasonDropEdit;

	}
}
