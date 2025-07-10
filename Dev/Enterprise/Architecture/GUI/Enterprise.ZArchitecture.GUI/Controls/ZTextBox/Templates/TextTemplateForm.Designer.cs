namespace Enterprise.ZArchitecture.GUI
{
	partial class TextTemplateForm
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
			if (previewForm != null)
			{
				previewForm.Dispose();
			}
			if (mapTreePresenter != null)
			{
				mapTreePresenter.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.templateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TemplateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.deleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.previewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.insertMacroButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.publicCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.allCompaniesCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 364, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmNoteTemplate);
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "S8_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmNoteTemplate)(null)).S8_Description)));
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|b7e66063-fdf3-467a-8f5c-e78f0321603c", "Template Name");
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 13, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.descriptionTextBox.TabIndex = 1;
			// 
			// templateTextBox
			// 
			this.templateTextBox.AllowDrop = true;			
			this.templateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.templateTextBox, "TemplateText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmNoteTemplate)(null)).TemplateText)));
			this.templateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.templateTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|79c7fbac-058a-4cd1-808b-893b16c03813", "Template Text");
			this.templateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 66, true);
			this.templateTextBox.Multiline = true;
			this.templateTextBox.Name = "templateTextBox";
			this.templateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 222, true);
			this.templateTextBox.TabIndex = 5;
			// 
			// TemplateLabel
			// 
			this.TemplateLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|6d40f1a6-4a70-4f3f-a270-c832e4e93da3", "Template Text");
			this.TemplateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 40, true);
			this.TemplateLabel.Name = "TemplateLabel";
			this.TemplateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
			this.TemplateLabel.TabIndex = 2;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 335, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.postingButtonsUserControl.TabIndex = 9;
			this.postingButtonsUserControl.TabStop = true;
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|0d2b2160-0161-4885-83ae-953970ae8afa", "Delete");
			this.deleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 335, true);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.deleteButton.TabIndex = 8;
			this.deleteButton.UseVisualStyleBackColor = false;
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// previewButton
			// 
			this.previewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.previewButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|e5f89789-26d6-4b0c-a414-8db7995b7d19", "Preview");
			this.previewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 37, true);
			this.previewButton.Name = "previewButton";
			this.previewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.previewButton.TabIndex = 4;
			this.previewButton.UseVisualStyleBackColor = false;
			this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
			// 
			// insertMacroButton
			// 
			this.insertMacroButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.insertMacroButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|989c1dfb-4b00-451d-b8ef-0a66d462f41c", "Insert Macro");
			this.insertMacroButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 37, true);
			this.insertMacroButton.Name = "insertMacroButton";
			this.insertMacroButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 23, true);
			this.insertMacroButton.TabIndex = 3;
			this.insertMacroButton.UseVisualStyleBackColor = false;
			this.insertMacroButton.Click += new System.EventHandler(this.insertMacroButton_Click);
			// 
			// publicCheckbox
			// 
			this.publicCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.publicCheckbox, "IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmNoteTemplate)(null)).IsPublished)));
			this.publicCheckbox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|48901db1-ab8b-4dd9-82e5-d69e263fb911", "Publish template for all users");
			this.publicCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.publicCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 295, true);
			this.publicCheckbox.Name = "publicCheckbox";
			this.publicCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 34, true);
			this.publicCheckbox.TabIndex = 6;
			this.publicCheckbox.UseVisualStyleBackColor = true;
			// 
			// allCompaniesCheckbox
			// 
			this.allCompaniesCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.allCompaniesCheckbox, "IsAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmNoteTemplate)(null)).IsAllCompanies)));
			this.allCompaniesCheckbox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|4d920f34-10e7-4f0c-ad0a-4fc86b5a8a8b", "Publish across all companies");
			this.allCompaniesCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.allCompaniesCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 295, true);
			this.allCompaniesCheckbox.Name = "allCompaniesCheckbox";
			this.allCompaniesCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 34, true);
			this.allCompaniesCheckbox.TabIndex = 7;
			this.allCompaniesCheckbox.UseVisualStyleBackColor = true;
			// 
			// TextTemplateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 388, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TextTemplateForm|113987cc-5a10-47ce-8bb6-2c19e50a165e", "Text Template");
			this.Controls.Add(this.previewButton);
			this.Controls.Add(this.publicCheckbox);
			this.Controls.Add(this.insertMacroButton);
			this.Controls.Add(this.allCompaniesCheckbox);
			this.Controls.Add(this.deleteButton);
			this.Controls.Add(this.postingButtonsUserControl);
			this.Controls.Add(this.TemplateLabel);
			this.Controls.Add(this.templateTextBox);
			this.Controls.Add(this.descriptionTextBox);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmNoteTemplate);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 320, true);
			this.Name = "TextTemplateForm";
			this.Controls.SetChildIndex(this.descriptionTextBox, 0);
			this.Controls.SetChildIndex(this.templateTextBox, 0);
			this.Controls.SetChildIndex(this.TemplateLabel, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.deleteButton, 0);
			this.Controls.SetChildIndex(this.allCompaniesCheckbox, 0);
			this.Controls.SetChildIndex(this.insertMacroButton, 0);
			this.Controls.SetChildIndex(this.publicCheckbox, 0);
			this.Controls.SetChildIndex(this.previewButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZTextBox descriptionTextBox;
		protected ZTextBox templateTextBox;
		protected ZLabel TemplateLabel;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		internal ZButton deleteButton;
		internal ZButton previewButton;
		internal ZButton insertMacroButton;
		internal ZCheckBox publicCheckbox;
		internal ZCheckBox allCompaniesCheckbox;

	}
}
