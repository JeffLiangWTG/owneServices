namespace Enterprise.VisualBoards.GUI
{
	partial class StopWatchControl
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
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.TotalElapsedTimeDisplayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StartButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ResetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviousChannelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NextChannelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.TotalElapsedTimeDisplayLabel);
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 58, true);
			this.panel1.TabIndex = 7;
			// 
			// TotalElapsedTimeDisplayLabel
			// 
			this.TotalElapsedTimeDisplayLabel.IsFontBold = true;
			this.TotalElapsedTimeDisplayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.TotalElapsedTimeDisplayLabel.Name = "TotalElapsedTimeDisplayLabel";
			this.TotalElapsedTimeDisplayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 55, true);
			this.TotalElapsedTimeDisplayLabel.TabIndex = 10;
			this.TotalElapsedTimeDisplayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// StartButton
			// 
			this.StartButton.BackgroundImage = global::Enterprise.VisualBoards.GUI.Properties.Resources.play;
			this.StartButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 2, true);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 30, true);
			this.StartButton.TabIndex = 8;
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// ResetButton
			// 
			this.ResetButton.BackgroundImage = global::Enterprise.VisualBoards.GUI.Properties.Resources.restart;
			this.ResetButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.ResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 2, true);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 30, true);
			this.ResetButton.TabIndex = 9;
			this.ResetButton.UseVisualStyleBackColor = true;
			this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
			// 
			// PreviousChannelButton
			// 
			this.PreviousChannelButton.BackgroundImage = global::Enterprise.VisualBoards.GUI.Properties.Resources.step_back;
			this.PreviousChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.PreviousChannelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 32, true);
			this.PreviousChannelButton.Name = "PreviousChannelButton";
			this.PreviousChannelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 30, true);
			this.PreviousChannelButton.TabIndex = 10;
			this.PreviousChannelButton.UseVisualStyleBackColor = true;
			this.PreviousChannelButton.Click += new System.EventHandler(this.PreviousChannelButton_Click);
			// 
			// NextChannelButton
			// 
			this.NextChannelButton.BackgroundImage = global::Enterprise.VisualBoards.GUI.Properties.Resources.step_forward;
			this.NextChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.NextChannelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 32, true);
			this.NextChannelButton.Name = "NextChannelButton";
			this.NextChannelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 30, true);
			this.NextChannelButton.TabIndex = 11;
			this.NextChannelButton.UseVisualStyleBackColor = true;
			this.NextChannelButton.Click += new System.EventHandler(this.NextChannelButton_Click);
			// 
			// StopWatchControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NextChannelButton);
			this.Controls.Add(this.PreviousChannelButton);
			this.Controls.Add(this.ResetButton);
			this.Controls.Add(this.StartButton);
			this.Controls.Add(this.panel1);
			this.Name = "StopWatchControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 66, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KPanel panel1;
		private ZArchitecture.GUI.ZButton StartButton;
		private ZArchitecture.GUI.ZButton ResetButton;
		private ZArchitecture.ZLabel TotalElapsedTimeDisplayLabel;
		private ZArchitecture.GUI.ZButton PreviousChannelButton;
		private ZArchitecture.GUI.ZButton NextChannelButton;
	}
}
