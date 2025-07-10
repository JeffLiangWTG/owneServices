
namespace Enterprise.Customs.CA.GUI
{
	partial class JobDeclarationForm
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
		protected override void InitializeComponent()
		{
			this.BottomButtonPanel.SuspendLayout();
			this.oPostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 679, true);
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1319, 28, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 707, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1319, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// JobDeclarationForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1319, 731, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.CA.Business";
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			this.DataSourceTypeName = "Enterprise.Customs.CA.Business.JobDeclaration";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1335, 770, true);
			this.Name = "JobDeclarationForm";
			this.Text = "JobDeclarationForm";
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.oPostingButtonsUserControl.ResumeLayout(true);
			this.oPostingButtonsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
