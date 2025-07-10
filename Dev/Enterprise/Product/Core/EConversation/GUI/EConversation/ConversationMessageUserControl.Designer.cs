namespace Enterprise.EConversation.GUI
{
	partial class ConversationMessageUserControl
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
			this.components = new System.ComponentModel.Container();
			this.messagePanel = new CargoWise.Windows.UI.KPanel();
			this.ratingUserControl = new Enterprise.EConversation.GUI.MessageRatingUserControl();
			this.timeLabel = new CargoWise.Windows.UI.KLabel();
			this.bodyTextBox = new Enterprise.EConversation.GUI.ConversationTextBox();
			this.usernameLabel = new CargoWise.Windows.UI.KLabel();
			this.usercodeToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.usernamePanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.messagePanel.SuspendLayout();
			this.ratingUserControl.SuspendLayout();
			this.usernamePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.EConversation.Business.IConversationMessage);
			// 
			// messagePanel
			// 
			this.messagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.messagePanel.Controls.Add(this.ratingUserControl);
			this.messagePanel.Controls.Add(this.timeLabel);
			this.messagePanel.Controls.Add(this.bodyTextBox);
			this.messagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 4, true);
			this.messagePanel.Name = "messagePanel";
			this.messagePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 2, 5, 2, true);
			this.messagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 67, true);
			this.messagePanel.TabIndex = 2;
			this.messagePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.MessagePanel_Paint);
			// 
			// ratingUserControl
			// 
			this.ratingUserControl.AllowDrop = true;
			this.ratingUserControl.CaptionRenderingEnabled = true;
			this.ratingUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ratingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 48, true);
			this.ratingUserControl.Name = "ratingUserControl";
			this.ratingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 16, true);
			this.ratingUserControl.TabIndex = 5;
			// 
			// timeLabel
			// 
			this.timeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.timeLabel.BackColor = System.Drawing.Color.White;
			this.timeLabel.ForeColor = System.Drawing.Color.DarkGray;
			this.timeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 51, true);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.timeLabel.TabIndex = 4;
			this.timeLabel.Text = "12:00 PM";
			this.timeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// bodyTextBox
			// 
			this.bodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.bodyTextBox.BackColor = System.Drawing.Color.White;
			this.bodyTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.bodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 5, true);
			this.bodyTextBox.Name = "bodyTextBox";
			this.bodyTextBox.ReadOnly = true;
			this.bodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 43, true);
			this.bodyTextBox.TabIndex = 1;
			this.bodyTextBox.TabStop = false;
			this.bodyTextBox.Text = "";
			// 
			// usernameLabel
			// 
			this.usernameLabel.AutoSize = true;
			this.usernameLabel.BackColor = System.Drawing.Color.Transparent;
			this.usernameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usernameLabel.ForeColor = System.Drawing.Color.Black;
			this.usernameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.usernameLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 0, true);
			this.usernameLabel.Name = "usernameLabel";
			this.usernameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 60, true);
			this.usernameLabel.TabIndex = 3;
			this.usernameLabel.Text = "[Username]";
			this.usernameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// usernamePanel
			// 
			this.usernamePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.usernamePanel.ColumnCount = 1;
			this.usernamePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.usernamePanel.Controls.Add(this.usernameLabel, 0, 0);
			this.usernamePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 9, true);
			this.usernamePanel.Name = "usernamePanel";
			this.usernamePanel.RowCount = 1;
			this.usernamePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.usernamePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 60, true);
			this.usernamePanel.TabIndex = 4;
			// 
			// ConversationMessageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.usernamePanel);
			this.Controls.Add(this.messagePanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "ConversationMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 74, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.messagePanel.ResumeLayout(false);
			this.messagePanel.PerformLayout();
			this.ratingUserControl.ResumeLayout(true);
			this.ratingUserControl.PerformLayout();
			this.usernamePanel.ResumeLayout(false);
			this.usernamePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KPanel messagePanel;
		public Enterprise.EConversation.GUI.ConversationTextBox bodyTextBox;
		private CargoWise.Windows.UI.KLabel usernameLabel;
		private CargoWise.Windows.UI.KLabel timeLabel;
		private System.Windows.Forms.ToolTip usercodeToolTip;
		private CargoWise.Windows.UI.KTableLayoutPanel usernamePanel;
		private MessageRatingUserControl ratingUserControl;
	}
}
