namespace Enterprise.BufferManagement.GUI
{
	partial class WorkflowTransferDiagnosisForm
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
			this.TransferDiagnosisControl = new Enterprise.BufferManagement.GUI.WorkflowTransferDiagnosisUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransferDiagnosisControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 590, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel);
			// 
			// TransferDiagnosisControl
			// 
			this.TransferDiagnosisControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransferDiagnosisControl, ".");
			this.TransferDiagnosisControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransferDiagnosisControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransferDiagnosisControl.Name = "TransferDiagnosisControl";
			this.TransferDiagnosisControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 590, true);
			this.TransferDiagnosisControl.TabIndex = 1;
			// 
			// WorkflowTransferDiagnosisForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 614, true);
			this.Controls.Add(this.TransferDiagnosisControl);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 650, true);
			this.Name = "WorkflowTransferDiagnosisForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TransferDiagnosisControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransferDiagnosisControl.ResumeLayout(true);
			this.TransferDiagnosisControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private WorkflowTransferDiagnosisUserControl TransferDiagnosisControl;

	}
}