namespace Enterprise.BufferManagement.GUI
{
	partial class CellTasksControl
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
			this.TaskCardsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CloseTaskCardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ChannelDayHeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TaskCardsPanel
			// 
			this.TaskCardsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.TaskCardsPanel.AutoScroll = true;
			this.TaskCardsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TaskCardsPanel.BackColor = System.Drawing.Color.Transparent;
			this.TaskCardsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 26, true);
			this.TaskCardsPanel.Name = "TaskCardsPanel";
			this.TaskCardsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 264, true);
			this.TaskCardsPanel.TabIndex = 0;
			// 
			// CloseTaskCardButton
			// 
			this.CloseTaskCardButton.BackColor = System.Drawing.Color.Transparent;
			this.CloseTaskCardButton.BackgroundImage = global::Enterprise.BufferManagement.GUI.Properties.Resources.glyphicons_207_remove_2;
			this.CloseTaskCardButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.CloseTaskCardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 0, true);
			this.CloseTaskCardButton.Name = "CloseTaskCardButton";
			this.CloseTaskCardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 25, true);
			this.CloseTaskCardButton.TabIndex = 2;
			this.CloseTaskCardButton.UseVisualStyleBackColor = false;
			this.CloseTaskCardButton.Click += new System.EventHandler(this.CloseTaskCardButton_Click);
			// 
			// ChannelDayHeaderLabel
			// 
			this.ChannelDayHeaderLabel.IsFontBold = true;
			this.ChannelDayHeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.ChannelDayHeaderLabel.Name = "ChannelDayHeaderLabel";
			this.ChannelDayHeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
			this.ChannelDayHeaderLabel.TabIndex = 3;
			// 
			// CellTasksControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChannelDayHeaderLabel);
			this.Controls.Add(this.TaskCardsPanel);
			this.Controls.Add(this.CloseTaskCardButton);
			this.Name = "CellTasksControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 290, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public ZArchitecture.GUI.ZButton CloseTaskCardButton;
		public ZArchitecture.GUI.ZPanel TaskCardsPanel;
		public ZArchitecture.ZLabel ChannelDayHeaderLabel;
	}
}
