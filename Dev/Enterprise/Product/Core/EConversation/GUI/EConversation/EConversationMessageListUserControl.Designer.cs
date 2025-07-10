namespace Enterprise.EConversation.GUI
{
	partial class EConversationMessageListUserControl
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
			this.messagesLayoutPanel = new Enterprise.EConversation.GUI.DoubleBufferedStackLayoutPanel();
			this.viewTogglePanel = new CargoWise.Windows.UI.KPanel();
			this.allMessagesRadioButton = new CargoWise.Windows.UI.KRadioButton();
			this.userMessagesRadioButton = new CargoWise.Windows.UI.KRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.viewTogglePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.EConversation.Business.IConversation);
			// 
			// messagesLayoutPanel
			// 
			this.messagesLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messagesLayoutPanel.AutoScroll = true;
			this.messagesLayoutPanel.BackColor = System.Drawing.Color.White;
			this.messagesLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.messagesLayoutPanel.Name = "messagesLayoutPanel";
			this.messagesLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 17, 0, true);
			this.messagesLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 165, true);
			this.messagesLayoutPanel.TabIndex = 0;
			this.messagesLayoutPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MessagesLayoutPanel_MouseClick);
			this.messagesLayoutPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MessagesLayoutPanel_MouseDoubleClick);
			// 
			// viewTogglePanel
			// 
			this.viewTogglePanel.Controls.Add(this.allMessagesRadioButton);
			this.viewTogglePanel.Controls.Add(this.userMessagesRadioButton);
			this.viewTogglePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.viewTogglePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.viewTogglePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.viewTogglePanel.Name = "viewTogglePanel";
			this.viewTogglePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 35, true);
			this.viewTogglePanel.TabIndex = 2;
			// 
			// allMessagesRadioButton
			// 
			this.allMessagesRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.allMessagesRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.allMessagesRadioButton.Checked = true;
			this.allMessagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.allMessagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 9, true);
			this.allMessagesRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.allMessagesRadioButton.Name = "allMessagesRadioButton";
			this.allMessagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 21, true);
			this.allMessagesRadioButton.TabIndex = 1;
			this.allMessagesRadioButton.TabStop = true;
			this.allMessagesRadioButton.Text = "All Messages";
			this.allMessagesRadioButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.allMessagesRadioButton.UseVisualStyleBackColor = true;
			this.allMessagesRadioButton.CheckedChanged += new System.EventHandler(this.AllMessagesRadioButton_CheckedChanged);
			// 
			// userMessagesRadioButton
			// 
			this.userMessagesRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.userMessagesRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.userMessagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.userMessagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 9, true);
			this.userMessagesRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.userMessagesRadioButton.Name = "userMessagesRadioButton";
			this.userMessagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 21, true);
			this.userMessagesRadioButton.TabIndex = 0;
			this.userMessagesRadioButton.Text = "eConversation Only";
			this.userMessagesRadioButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.userMessagesRadioButton.UseVisualStyleBackColor = true;
			this.userMessagesRadioButton.CheckedChanged += new System.EventHandler(this.UserMessagesRadioButton_CheckedChanged);
			// 
			// EConversationMessageListUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.viewTogglePanel);
			this.Controls.Add(this.messagesLayoutPanel);
			this.Name = "EConversationMessageListUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 200, true);
			this.Resize += new System.EventHandler(this.EConversationUserControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.viewTogglePanel.ResumeLayout(false);
			this.viewTogglePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DoubleBufferedStackLayoutPanel messagesLayoutPanel;
		private CargoWise.Windows.UI.KPanel viewTogglePanel;
		private CargoWise.Windows.UI.KRadioButton allMessagesRadioButton;
		private CargoWise.Windows.UI.KRadioButton userMessagesRadioButton;
	}
}
