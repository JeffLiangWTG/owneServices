namespace Enterprise.BufferManagement.GUI
{
	partial class LegendForm
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
		protected new void InitializeComponent()
		{
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TaskStatusPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.CloseLegendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 489, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 24, true);
			// 
			// StatusGroupBox
			// 
			this.StatusGroupBox.BackColor = System.Drawing.Color.White;
			this.StatusGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("7c9cc7d8-f478-4d36-8e6b-1667cb49b988", "Task Status Indicators");
			this.StatusGroupBox.Controls.Add(this.TaskStatusPanel);
			this.StatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 489, true);
			this.StatusGroupBox.TabIndex = 2;
			this.StatusGroupBox.TabStop = false;
			// 
			// TaskStatusPanel
			// 
			this.TaskStatusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaskStatusPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.TaskStatusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TaskStatusPanel.Name = "TaskStatusPanel";
			this.TaskStatusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 470, true);
			this.TaskStatusPanel.TabIndex = 1;
			// 
			// CloseLegendButton
			// 
			this.CloseLegendButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("070bb9f5-1f7c-4364-86c2-2d1be66ab090", "Close");
			this.CloseLegendButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CloseLegendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, -38, true);
			this.CloseLegendButton.Name = "CloseLegendButton";
			this.CloseLegendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseLegendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.CloseLegendButton.TabIndex = 6;
			this.CloseLegendButton.TabStop = false;
			this.CloseLegendButton.ToolTipCaption = null;
			this.CloseLegendButton.UseVisualStyleBackColor = true;
			this.CloseLegendButton.Click += new System.EventHandler(this.CloseLegendButton_Click);
			// 
			// LegendForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.CloseLegendButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ff4e081e-c4be-4548-bd0d-61fa90f09401", "Legend");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 400, true);
			this.Controls.Add(this.StatusGroupBox);
			this.Controls.Add(this.CloseLegendButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "LegendForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CloseLegendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.StatusGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		public CargoWise.Windows.UI.KFlowLayoutPanel TaskStatusPanel;
		private ZArchitecture.GUI.ZButton CloseLegendButton;
	}
}
