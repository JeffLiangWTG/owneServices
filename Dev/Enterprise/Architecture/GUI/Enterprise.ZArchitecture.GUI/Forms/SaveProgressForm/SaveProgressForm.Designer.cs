namespace Enterprise.ZArchitecture.GUI
{
	public partial class SaveProgressForm
	{

		#region Windows Form Designer generated code

		protected CargoWise.Windows.UI.KPanel MainPanel;
		protected CargoWise.Windows.UI.KPanel BottomPanel;
		internal CargoWise.Windows.UI.KProgressBar ProgressBar;
		protected ZAnimationBox Animation;
		internal ProgressFormTextBox ProgressTextBox;

		private void InitializeComponent()
		{
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveProgressForm));
			this.MainPanel = new CargoWise.Windows.UI.KPanel();
			this.BottomPanel = new CargoWise.Windows.UI.KPanel();
			this.ProgressTextBox = new ProgressFormTextBox();
			this.Animation = new Enterprise.ZArchitecture.GUI.ZAnimationBox();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.BackColor = System.Drawing.Color.White;
			this.MainPanel.Controls.Add(this.BottomPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 137, true);
			this.MainPanel.TabIndex = 4;
			this.MainPanel.UseWaitCursor = true;
			// 
			// BottomPanel
			// 
			this.BottomPanel.BackColor = System.Drawing.Color.White;
			this.BottomPanel.Controls.Add(this.ProgressTextBox);
			this.BottomPanel.Controls.Add(this.Animation);
			this.BottomPanel.Controls.Add(this.ProgressBar);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 131, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 131, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 131, true);
			this.BottomPanel.TabIndex = 9;
			this.BottomPanel.UseWaitCursor = true;
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.BackColor = System.Drawing.Color.White;
			this.ProgressTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 66, true);
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 13, true);
			this.ProgressTextBox.TabIndex = 2;
			this.ProgressTextBox.Text = "Saving...";
			this.ProgressTextBox.UseWaitCursor = true;
			this.ProgressTextBox.TrackDisposedAccess = true;
			// 
			// Animation
			// 
			this.Animation.Cursor = System.Windows.Forms.Cursors.WaitCursor;
			this.Animation.Image = ((System.Drawing.Image)(resources.GetObject("Animation.Image")));
			this.Animation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 11, true);
			this.Animation.Name = "Animation";
			this.Animation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 40, true);
			this.Animation.TabIndex = 1;
			this.Animation.TabStop = false;
			this.Animation.UseWaitCursor = true;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 94, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 18, true);
			this.ProgressBar.TabIndex = 0;
			this.ProgressBar.UseWaitCursor = true;
			// 
			// SaveProgressForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 137, true);
			this.ControlBox = false;
			this.Controls.Add(this.MainPanel);
			this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 163, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 163, true);
			this.Name = "SaveProgressForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = Enterprise.ZArchitecture.GUI.Res.GetString("012133D7-4C59-4C42-8CA1-9B653E509CCA", "Please Wait...");
			this.UseWaitCursor = true;
			this.MainPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
