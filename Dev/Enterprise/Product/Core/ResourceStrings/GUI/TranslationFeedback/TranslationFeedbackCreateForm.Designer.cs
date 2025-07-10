using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.GUI
{
	partial class TranslationFeedbackCreateForm
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
		new void InitializeComponent()
		{
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.commentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.commentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.translationFeedbackMainUserControl = new Enterprise.ResourceStrings.GUI.TranslationFeedbackMainUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 400, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.StmTranslationFeedbackCollection);
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.CaptionResourceString = null;
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 371, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.postingButtonsUserControl.TabIndex = 2;
			// 
			// commentsLabel
			// 
			this.commentsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.commentsLabel.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("6ce4847e-fb93-4e09-8b1d-5ec07c592c99", "Comments");
			this.commentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 329, true);
			this.commentsLabel.Name = "commentsLabel";
			this.commentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 12, true);
			this.commentsLabel.TabIndex = 4;
			// 
			// commentsTextBox
			// 
			this.commentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.commentsTextBox, "XT_Comments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)).XT_Comments)));
			this.commentsTextBox.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("7f4e414f-f498-4fbd-8097-4c2177009a6c", "Comments");
			this.commentsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.commentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 345, true);
			this.commentsTextBox.Name = "commentsTextBox";
			this.commentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 20, true);
			this.commentsTextBox.TabIndex = 5;
			// 
			// translationFeedbackMainUserControl
			// 
			this.translationFeedbackMainUserControl.AllowDrop = true;
			this.translationFeedbackMainUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.translationFeedbackMainUserControl, ".");
			this.translationFeedbackMainUserControl.CaptionResourceString = null;
			this.translationFeedbackMainUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 6, true);
			this.translationFeedbackMainUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 320, true);
			this.translationFeedbackMainUserControl.Name = "translationFeedbackMainUserControl";
			this.translationFeedbackMainUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 320, true);
			this.translationFeedbackMainUserControl.TabIndex = 3;
			// 
			// TranslationFeedbackCreateForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("20ea3798-14ab-4231-9fbc-2e10e4f0995a", "Translation Feedback");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 424, true);
			this.Controls.Add(this.commentsTextBox);
			this.Controls.Add(this.postingButtonsUserControl);
			this.Controls.Add(this.translationFeedbackMainUserControl);
			this.Controls.Add(this.commentsLabel);
			this.DataSourceType = typeof(Enterprise.ResourceStrings.Business.StmTranslationFeedbackCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 462, true);
			this.Name = "TranslationFeedbackCreateForm";
			this.Controls.SetChildIndex(this.commentsLabel, 0);
			this.Controls.SetChildIndex(this.translationFeedbackMainUserControl, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.commentsTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		internal TranslationFeedbackMainUserControl translationFeedbackMainUserControl;
		private ZArchitecture.ZLabel commentsLabel;
		private ZArchitecture.ZTextBox commentsTextBox;
	}
}