using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class UserIdleWorkerExplorerForm
	{
		SplitContainer splitContainer1;
		GroupBox gbQueued;
		ListBox lbQueued;
		GroupBox gbCompleted;
		ListBox lbCompleted;
		CheckBox chkSuspend;
		Button btnClose;

		void InitializeComponent()
		{
			this.btnClose = new CargoWise.Windows.UI.KButton();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.gbQueued = new CargoWise.Windows.UI.KGroupBox();
			this.lbQueued = new CargoWise.Windows.UI.KListBox();
			this.gbCompleted = new CargoWise.Windows.UI.KGroupBox();
			this.lbCompleted = new CargoWise.Windows.UI.KListBox();
			this.chkSuspend = new CargoWise.Windows.UI.KCheckBox();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.gbQueued.SuspendLayout();
			this.gbCompleted.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnClose
			// 
			this.btnClose.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 549, true);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnClose.TabIndex = 2;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.gbQueued);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.gbCompleted);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 531, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(264);
			this.splitContainer1.TabIndex = 0;
			// 
			// gbQueued
			// 
			this.gbQueued.Controls.Add(this.lbQueued);
			this.gbQueued.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbQueued.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gbQueued.Name = "gbQueued";
			this.gbQueued.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 264, true);
			this.gbQueued.TabIndex = 0;
			this.gbQueued.TabStop = false;
			this.gbQueued.Text = "Queued";
			// 
			// lbQueued
			// 
			this.lbQueued.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbQueued.FormattingEnabled = true;
			this.lbQueued.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.lbQueued.Name = "lbQueued";
			this.lbQueued.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 238, true);
			this.lbQueued.TabIndex = 1;
			this.lbQueued.DoubleClick += new System.EventHandler(this.WorkItem_DoubleClick);
			// 
			// gbCompleted
			// 
			this.gbCompleted.Controls.Add(this.lbCompleted);
			this.gbCompleted.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbCompleted.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gbCompleted.Name = "gbCompleted";
			this.gbCompleted.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 263, true);
			this.gbCompleted.TabIndex = 0;
			this.gbCompleted.TabStop = false;
			this.gbCompleted.Text = "Completed";
			// 
			// lbCompleted
			// 
			this.lbCompleted.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbCompleted.FormattingEnabled = true;
			this.lbCompleted.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.lbCompleted.Name = "lbCompleted";
			this.lbCompleted.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 238, true);
			this.lbCompleted.TabIndex = 3;
			this.lbCompleted.DoubleClick += new System.EventHandler(this.WorkItem_DoubleClick);
			// 
			// chkSuspend
			// 
			this.chkSuspend.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.chkSuspend.AutoSize = true;
			this.chkSuspend.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 553, true);
			this.chkSuspend.Name = "chkSuspend";
			this.chkSuspend.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.chkSuspend.TabIndex = 1;
			this.chkSuspend.Text = "Suspend";
			this.chkSuspend.UseVisualStyleBackColor = true;
			this.chkSuspend.CheckedChanged += new System.EventHandler(this.chkSuspend_CheckChanged);
			// 
			// UserIdleWorkerExplorerForm
			// 
			this.CancelButton = this.btnClose;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 578, true);
			this.Controls.Add(this.chkSuspend);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.btnClose);
			this.Name = "UserIdleWorkerExplorerForm";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.gbQueued.ResumeLayout(false);
			this.gbCompleted.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
