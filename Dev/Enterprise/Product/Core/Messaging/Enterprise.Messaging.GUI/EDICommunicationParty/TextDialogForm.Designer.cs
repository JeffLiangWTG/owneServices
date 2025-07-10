namespace Enterprise.Messaging.GUI
{
	partial class TextDialogForm
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
		private new void InitializeComponent()
		{
            this.SecretLabel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.Secret = new Enterprise.ZArchitecture.ZTextBox();
            this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SetAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SecretLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 467, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 24, true);
            // 
            // SecretLabel
            // 
            this.SecretLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SecretLabel.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("44c479f9-4b92-48d2-9e22-31424cf649f0", "Secret");
            this.SecretLabel.Controls.Add(this.Secret);
            this.SecretLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
            this.SecretLabel.Name = "SecretLabel";
            this.SecretLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 423, true);
            this.SecretLabel.TabIndex = 18;
            this.SecretLabel.TabStop = false;
            // 
            // Secret
            // 
            this.Secret.AcceptsReturn = true;
            this.Secret.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.Secret, "KeyValue");
            this.Secret.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Secret, false);
            this.Secret.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
            this.Secret.Multiline = true;
			this.Secret.Name = "Secret";
			this.Secret.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Secret.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 403, true);
            this.Secret.TabIndex = 2;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0fd75b0b-7564-49b1-beca-189eec0e0905", "Close", "Close the form.");
            this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 439, true);
            this.CloseButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 23, true);
            this.CloseButton.TabIndex = 20;
            this.CloseButton.ToolTipCaption = null;
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // SetAndCloseButton
            // 
            this.SetAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SetAndCloseButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("e9615706-d7b3-4f7a-8d30-b8cae57c08d6", "Set && Close", "Sets the secret.");
            this.SetAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 439, true);
            this.SetAndCloseButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.SetAndCloseButton.Name = "SetAndCloseButton";
            this.SetAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
            this.SetAndCloseButton.TabIndex = 19;
            this.SetAndCloseButton.ToolTipCaption = null;
            this.SetAndCloseButton.UseVisualStyleBackColor = true;
            this.SetAndCloseButton.Click += new System.EventHandler(this.SetAndCloseButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("fc9f6471-eefe-4bdb-8aff-cdc35a39bf06", "Save as", "Save as a file.");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 439, true);
			this.SaveButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.SaveButton.TabIndex = 19;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// TextDialogForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 491, true);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.SetAndCloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.SecretLabel);
            this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 530, true);
            this.MinimizeBox = false;
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 530, true);
            this.Name = "TextDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.SecretLabel, 0);
            this.Controls.SetChildIndex(this.SetAndCloseButton, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.CloseButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SecretLabel.ResumeLayout(false);
            this.SecretLabel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SecretLabel;
		private ZArchitecture.ZTextBox Secret;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZButton SetAndCloseButton;
		internal ZArchitecture.GUI.ZButton SaveButton;
	}
}
