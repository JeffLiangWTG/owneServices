namespace Enterprise.ResourceStrings.GUI
{
	partial class TranslationFeedbackViewForm
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
			this.translationFeedbackInfoControl = new Enterprise.ResourceStrings.GUI.TranslationFeedback.TranslationFeedbackInfoControl();
			this.translationFeedbackMainUserControl = new Enterprise.ResourceStrings.GUI.TranslationFeedbackMainUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 474, true);
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
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 445, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.postingButtonsUserControl.TabIndex = 2;
			// 
			// translationFeedbackInfoControl
			// 
			this.translationFeedbackInfoControl.AllowDrop = true;
			this.translationFeedbackInfoControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.translationFeedbackInfoControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(((Enterprise.ResourceStrings.Business.StmTranslationFeedback)(null)))));
			this.translationFeedbackInfoControl.CaptionResourceString = null;
			this.translationFeedbackInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 332, true);
			this.translationFeedbackInfoControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 108, true);
			this.translationFeedbackInfoControl.Name = "translationFeedbackInfoControl";
			this.translationFeedbackInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 108, true);
			this.translationFeedbackInfoControl.TabIndex = 4;
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
			// TranslationFeedbackViewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("c0e05b2b-a972-4659-836d-4d1a84dbac71", "Translation Feedback");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 498, true);
			this.Controls.Add(this.postingButtonsUserControl);
			this.Controls.Add(this.translationFeedbackInfoControl);
			this.Controls.Add(this.translationFeedbackMainUserControl);
			this.DataSourceType = typeof(Enterprise.ResourceStrings.Business.StmTranslationFeedbackCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 536, true);
			this.Name = "TranslationFeedbackViewForm";
			this.Controls.SetChildIndex(this.translationFeedbackMainUserControl, 0);
			this.Controls.SetChildIndex(this.translationFeedbackInfoControl, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		internal TranslationFeedbackMainUserControl translationFeedbackMainUserControl;
		private TranslationFeedback.TranslationFeedbackInfoControl translationFeedbackInfoControl;
	}
}
