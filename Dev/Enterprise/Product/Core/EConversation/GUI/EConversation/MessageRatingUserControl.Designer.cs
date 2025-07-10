namespace Enterprise.EConversation.GUI
{
	partial class MessageRatingUserControl
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
			this.dislikeLinkLabel = new CargoWise.Windows.UI.KLinkLabel();
			this.likeLinkLabel = new CargoWise.Windows.UI.KLinkLabel();
			this.ratingLabel = new CargoWise.Windows.UI.KLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// dislikeLinkLabel
			// 
			this.dislikeLinkLabel.BackColor = System.Drawing.Color.Transparent;
			this.dislikeLinkLabel.DisabledLinkColor = System.Drawing.Color.White;
			this.dislikeLinkLabel.ForeColor = System.Drawing.Color.White;
			this.dislikeLinkLabel.Image = global::Enterprise.EConversation.GUI.Properties.Resources.dislike_icon_0;
			this.dislikeLinkLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.dislikeLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 0, true);
			this.dislikeLinkLabel.Name = "dislikeLinkLabel";
			this.dislikeLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.dislikeLinkLabel.TabIndex = 1;
			this.dislikeLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.dislikeLinkLabel.Click += new System.EventHandler(this.DislikeLinkLabel_ClickOrDoubleClick);
			this.dislikeLinkLabel.DoubleClick += new System.EventHandler(this.DislikeLinkLabel_ClickOrDoubleClick);
			this.dislikeLinkLabel.MouseEnter += new System.EventHandler(this.DislikeLinkLabel_MouseEnter);
			this.dislikeLinkLabel.MouseLeave += new System.EventHandler(this.DislikeLinkLabel_MouseLeave);
			// 
			// likeLinkLabel
			// 
			this.likeLinkLabel.BackColor = System.Drawing.Color.Transparent;
			this.likeLinkLabel.DisabledLinkColor = System.Drawing.Color.White;
			this.likeLinkLabel.ForeColor = System.Drawing.Color.White;
			this.likeLinkLabel.Image = global::Enterprise.EConversation.GUI.Properties.Resources.like_icon_0;
			this.likeLinkLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.likeLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 0, true);
			this.likeLinkLabel.Name = "likeLinkLabel";
			this.likeLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.likeLinkLabel.TabIndex = 0;
			this.likeLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.likeLinkLabel.Click += new System.EventHandler(this.LikeLinkLabel_ClickOrDoubleClick);
			this.likeLinkLabel.DoubleClick += new System.EventHandler(this.LikeLinkLabel_ClickOrDoubleClick);
			this.likeLinkLabel.MouseEnter += new System.EventHandler(this.LikeLinkLabel_MouseEnter);
			this.likeLinkLabel.MouseLeave += new System.EventHandler(this.LikeLinkLabel_MouseLeave);
			// 
			// ratingLabel
			// 
			this.ratingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ratingLabel.Name = "ratingLabel";
			this.ratingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.ratingLabel.TabIndex = 2;
			// 
			// MessageRatingUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.likeLinkLabel);
			this.Controls.Add(this.dislikeLinkLabel);
			this.Controls.Add(this.ratingLabel);
			this.Name = "MessageRatingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 16, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KLinkLabel dislikeLinkLabel;
		private CargoWise.Windows.UI.KLinkLabel likeLinkLabel;
		private CargoWise.Windows.UI.KLabel ratingLabel;
	}
}
