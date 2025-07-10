namespace Enterprise.ResourceStrings.GUI
{
	partial class TranslationFeedbackMainUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.translationFeedbackEntriesListControl = new Enterprise.ResourceStrings.GUI.TranslationFeedbackEntriesListControl();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.selectOneContextLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.selectAllContextsLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.allContextsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.translationFeedbackContextsControl = new Enterprise.ResourceStrings.GUI.TranslationFeedbackContextsControl();
			this.otherTranslationsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.otherTranslationsControl = new Enterprise.ResourceStrings.GUI.TranslationFeedbackEntriesListControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.StmTranslationFeedbackCollection);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.translationFeedbackEntriesListControl);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 320, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(119);
			this.splitContainer1.TabIndex = 7;
			// 
			// translationFeedbackEntriesListControl
			// 
			this.translationFeedbackEntriesListControl.AllowDrop = true;
			this.translationFeedbackEntriesListControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.translationFeedbackEntriesListControl, ".");
			this.translationFeedbackEntriesListControl.CaptionResourceString = null;
			this.translationFeedbackEntriesListControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.translationFeedbackEntriesListControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 0, true);
			this.translationFeedbackEntriesListControl.Name = "translationFeedbackEntriesListControl";
			this.translationFeedbackEntriesListControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 116, true);
			this.translationFeedbackEntriesListControl.TabIndex = 2;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.selectOneContextLink);
			this.splitContainer2.Panel1.Controls.Add(this.selectAllContextsLink);
			this.splitContainer2.Panel1.Controls.Add(this.allContextsLabel);
			this.splitContainer2.Panel1.Controls.Add(this.translationFeedbackContextsControl);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.otherTranslationsLabel);
			this.splitContainer2.Panel2.Controls.Add(this.otherTranslationsControl);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 197, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94);
			this.splitContainer2.TabIndex = 0;
			// 
			// selectOneContextLink
			// 
			this.selectOneContextLink.AutoSize = true;
			this.selectOneContextLink.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("c801598c-2d83-4423-9135-4c92a7008689", "Select Active Context Only");
			this.selectOneContextLink.IsFontBold = false;
			this.selectOneContextLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 4, true);
			this.selectOneContextLink.Name = "selectOneContextLink";
			this.selectOneContextLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 13, true);
			this.selectOneContextLink.TabIndex = 7;
			this.selectOneContextLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.selectOneContextLink_LinkClicked);
			// 
			// selectAllContextsLink
			// 
			this.selectAllContextsLink.AutoSize = true;
			this.selectAllContextsLink.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("c538363b-66ed-429c-b0cc-0aa5ed020534", "Select All");
			this.selectAllContextsLink.IsFontBold = false;
			this.selectAllContextsLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 4, true);
			this.selectAllContextsLink.Name = "selectAllContextsLink";
			this.selectAllContextsLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.selectAllContextsLink.TabIndex = 6;
			this.selectAllContextsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.selectAllContextsLink_LinkClicked);
			// 
			// allContextsLabel
			// 
			this.allContextsLabel.AutoSize = true;
			this.allContextsLabel.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("989514e5-364a-4014-a837-c97dece1675d", "All Contexts");
			this.allContextsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.allContextsLabel.Name = "allContextsLabel";
			this.allContextsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.allContextsLabel.TabIndex = 5;
			// 
			// translationFeedbackContextsControl
			// 
			this.translationFeedbackContextsControl.AllowDrop = true;
			this.translationFeedbackContextsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.translationFeedbackContextsControl, "AllContexts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ResourceStrings.Business.StmTranslationFeedbackResourceCollection)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).AllContexts)));
			this.translationFeedbackContextsControl.CaptionResourceString = null;
			this.translationFeedbackContextsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			this.translationFeedbackContextsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 0, true);
			this.translationFeedbackContextsControl.Name = "translationFeedbackContextsControl";
			this.translationFeedbackContextsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 71, true);
			this.translationFeedbackContextsControl.TabIndex = 4;
			// 
			// otherTranslationsLabel
			// 
			this.otherTranslationsLabel.AutoSize = true;
			this.otherTranslationsLabel.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("3d2ec199-f718-44b0-be9e-d4e95beaf624", "Other Translations");
			this.otherTranslationsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.otherTranslationsLabel.Name = "otherTranslationsLabel";
			this.otherTranslationsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.otherTranslationsLabel.TabIndex = 9;
			// 
			// otherTranslationsControl
			// 
			this.otherTranslationsControl.AllowDrop = true;
			this.otherTranslationsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.otherTranslationsControl, "OtherTranslations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ResourceStrings.Business.StmTranslationFeedbackCollection)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).OtherTranslations)));
			this.otherTranslationsControl.CaptionResourceString = null;
			this.otherTranslationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			this.otherTranslationsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 0, true);
			this.otherTranslationsControl.Name = "otherTranslationsControl";
			this.otherTranslationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 76, true);
			this.otherTranslationsControl.TabIndex = 8;
			// 
			// TranslationFeedbackMainUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 320, true);
			this.Name = "TranslationFeedbackMainUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 320, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal TranslationFeedbackEntriesListControl translationFeedbackEntriesListControl;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		internal TranslationFeedbackContextsControl translationFeedbackContextsControl;
		internal TranslationFeedbackEntriesListControl otherTranslationsControl;
		private Enterprise.ZArchitecture.ZLabel allContextsLabel;
		private Enterprise.ZArchitecture.ZLabel otherTranslationsLabel;
		private ZArchitecture.GUI.ZLinkLabel selectAllContextsLink;
		private ZArchitecture.GUI.ZLinkLabel selectOneContextLink;
	}
}
