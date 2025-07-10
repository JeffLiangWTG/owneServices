namespace Enterprise.ResourceStrings.GUI
{
	partial class TranslationSearchControl
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
		void InitializeComponent()
		{
			this.searchSourceCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.sourceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.targetTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.targetLanguageDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.matchCaseCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.useRegularExpressionsCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.searchTargetCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.replaceCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.replaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.matchWordCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.TranslationSearchCriteria);
			// 
			// searchSourceCheckbox
			// 
			this.BindingSource.SetBindingMember(this.searchSourceCheckbox, "SearchSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).SearchSource)));
			this.searchSourceCheckbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("891593db-6f36-4f50-b331-e68dbeeb575a", "English");
			this.searchSourceCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.searchSourceCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.searchSourceCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.searchSourceCheckbox.Name = "searchSourceCheckbox";
			this.searchSourceCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 24, true);
			this.searchSourceCheckbox.TabIndex = 0;
			this.searchSourceCheckbox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.searchSourceCheckbox.UseVisualStyleBackColor = true;
			this.searchSourceCheckbox.CheckedChanged += new System.EventHandler(this.searchSourceCheckbox_CheckedChanged);
			// 
			// sourceTextBox
			// 
			this.sourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.sourceTextBox, "SourceText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).SourceText)));
			this.sourceTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("82168d45-8f86-4d75-bfd7-5546459c986e", "", "Source Search Text");
			this.sourceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.sourceTextBox.Enabled = false;
			this.sourceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 6, true);
			this.sourceTextBox.Name = "sourceTextBox";
			this.sourceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 20, true);
			this.sourceTextBox.TabIndex = 1;
			// 
			// targetTextBox
			// 
			this.targetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.targetTextBox, "TargetText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).TargetText)));
			this.targetTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("13184a25-0d8a-46a6-92d7-38cfdf9e9873", "", "Translation Search Text");
			this.targetTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.targetTextBox.Enabled = false;
			this.targetTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 36, true);
			this.targetTextBox.Name = "targetTextBox";
			this.targetTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.targetTextBox.TabIndex = 3;
			// 
			// targetLanguageDropEdit
			// 
			this.targetLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.targetLanguageDropEdit, "TargetLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).TargetLanguage)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.targetLanguageDropEdit, false);
			this.targetLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 36, true);
			this.targetLanguageDropEdit.Name = "targetLanguageDropEdit";
			this.targetLanguageDropEdit.PreBoundMaxLength = 3;
			this.targetLanguageDropEdit.ShowDescriptionBox = false;
			this.targetLanguageDropEdit.BindToList = "Languages";
			this.targetLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.targetLanguageDropEdit.TabIndex = 4;
			// 
			// matchCaseCheckbox
			// 
			this.BindingSource.SetBindingMember(this.matchCaseCheckbox, "MatchCase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).MatchCase)));
			this.matchCaseCheckbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("2e24f5bc-dfb4-41b7-b7cc-b589fd782915", "Match Case");
			this.matchCaseCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.matchCaseCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 63, true);
			this.matchCaseCheckbox.Name = "matchCaseCheckbox";
			this.matchCaseCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 24, true);
			this.matchCaseCheckbox.TabIndex = 5;
			this.matchCaseCheckbox.UseVisualStyleBackColor = true;
			// 
			// useRegularExpressionsCheckbox
			// 
			this.useRegularExpressionsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.useRegularExpressionsCheckbox, "UseRegularExpressions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).UseRegularExpressions)));
			this.useRegularExpressionsCheckbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("c17521e7-2ac0-40dc-ae51-974df988964c", "Use Reg. Exp.", "Use Regular Expressions", "");
			this.useRegularExpressionsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useRegularExpressionsCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 62, true);
			this.useRegularExpressionsCheckbox.Name = "useRegularExpressionsCheckbox";
			this.useRegularExpressionsCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 24, true);
			this.useRegularExpressionsCheckbox.TabIndex = 6;
			this.useRegularExpressionsCheckbox.UseVisualStyleBackColor = true;
			this.useRegularExpressionsCheckbox.Visible = false;
			// 
			// searchTargetCheckbox
			// 
			this.BindingSource.SetBindingMember(this.searchTargetCheckbox, "SearchTarget");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).SearchTarget)));
			this.searchTargetCheckbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("49341f2d-6c68-4e56-896f-d70ccaaac535", "Translation");
			this.searchTargetCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.searchTargetCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.searchTargetCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.searchTargetCheckbox.Name = "searchTargetCheckbox";
			this.searchTargetCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 24, true);
			this.searchTargetCheckbox.TabIndex = 2;
			this.searchTargetCheckbox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.searchTargetCheckbox.UseVisualStyleBackColor = true;
			this.searchTargetCheckbox.CheckedChanged += new System.EventHandler(this.searchTargetCheckbox_CheckedChanged);
			// 
			// replaceCheckbox
			// 
			this.BindingSource.SetBindingMember(this.replaceCheckbox, "Replace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).Replace)));
			this.replaceCheckbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("026d47d8-ca58-40a5-9a08-202dbdb69afb", "Replace");
			this.replaceCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.replaceCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.replaceCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 93, true);
			this.replaceCheckbox.Name = "replaceCheckbox";
			this.replaceCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 24, true);
			this.replaceCheckbox.TabIndex = 7;
			this.replaceCheckbox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.replaceCheckbox.UseVisualStyleBackColor = true;
			this.replaceCheckbox.CheckedChanged += new System.EventHandler(this.replaceCheckbox_CheckedChanged);
			// 
			// replaceTextBox
			// 
			this.replaceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.replaceTextBox, "ReplacementText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).ReplacementText)));
			this.replaceTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("f03a0595-f446-4a5e-9efc-e66bdee643ce", "", "Replacement Text");
			this.replaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.replaceTextBox.Enabled = false;
			this.replaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 94, true);
			this.replaceTextBox.Name = "replaceTextBox";
			this.replaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 20, true);
			this.replaceTextBox.TabIndex = 8;
			// 
			// matchWordCheckbox
			// 
			this.BindingSource.SetBindingMember(this.matchWordCheckbox, "MatchWord");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.TranslationSearchCriteria)(null)).MatchWord)));
			this.matchWordCheckbox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("b9271883-713a-4dfb-af8c-a872a69844c1", "Match Whole Word");
			this.matchWordCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.matchWordCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 62, true);
			this.matchWordCheckbox.Name = "matchWordCheckbox";
			this.matchWordCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 24, true);
			this.matchWordCheckbox.TabIndex = 9;
			this.matchWordCheckbox.UseVisualStyleBackColor = true;
			// 
			// TranslationSearchControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.matchWordCheckbox);
			this.Controls.Add(this.replaceTextBox);
			this.Controls.Add(this.replaceCheckbox);
			this.Controls.Add(this.useRegularExpressionsCheckbox);
			this.Controls.Add(this.matchCaseCheckbox);
			this.Controls.Add(this.targetTextBox);
			this.Controls.Add(this.sourceTextBox);
			this.Controls.Add(this.searchTargetCheckbox);
			this.Controls.Add(this.searchSourceCheckbox);
			this.Controls.Add(this.targetLanguageDropEdit);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 120, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 120, true);
			this.Name = "TranslationSearchControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox searchSourceCheckbox;
		internal ZArchitecture.ZTextBox sourceTextBox;
		internal ZArchitecture.ZTextBox targetTextBox;
		internal ZArchitecture.GUI.ZDropEdit targetLanguageDropEdit;
		private ZArchitecture.GUI.ZCheckBox matchCaseCheckbox;
		private ZArchitecture.GUI.ZCheckBox useRegularExpressionsCheckbox;
		internal ZArchitecture.GUI.ZCheckBox searchTargetCheckbox;
		internal ZArchitecture.GUI.ZCheckBox replaceCheckbox;
		internal ZArchitecture.ZTextBox replaceTextBox;
		private ZArchitecture.GUI.ZCheckBox matchWordCheckbox;
	}
}
