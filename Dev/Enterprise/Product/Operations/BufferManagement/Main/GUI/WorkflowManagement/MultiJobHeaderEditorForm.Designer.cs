namespace Enterprise.BufferManagement.GUI
{
	partial class MultiJobHeaderEditorForm
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
			this.JobHeaderEditorControl = new Enterprise.BufferManagement.GUI.MultiJobHeaderEditorUserControl();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.JobHeaderEditorControl);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 584, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel);
			// 
			// JobHeaderEditorControl
			// 
			this.JobHeaderEditorControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobHeaderEditorControl, ".");
			this.JobHeaderEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobHeaderEditorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobHeaderEditorControl.Name = "JobHeaderEditorControl";
			this.JobHeaderEditorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 584, true);
			this.JobHeaderEditorControl.TabIndex = 0;
			// 
			// MultiJobHeaderEditorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ca27c2eb-a494-4f9a-9bf9-2a19bad2742e", "Multi Job Scheduling Editor");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 640, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 650, true);
			this.Name = "MultiJobHeaderEditorForm";
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private MultiJobHeaderEditorUserControl JobHeaderEditorControl;
	}
}