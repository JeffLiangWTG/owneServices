namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class ExportProductsToCSVForm : ExportToCSVForm
	{
		new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.ReadOnly = false;
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.ReadOnly = false;
			//
			// FileNameTextBox
			//
			this.FileNameTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ExportProductsToCSVForm|5270371f-d9e2-45a8-9a27-6c31dc843952", "Export Products To CSV File");
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			// 
			// ExportProductsToCSVForm
			// 
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("KNAExportProductsToCSVForm|70cc6085-fbac-4b37-9e51-ba18508059bc", "Export Products To CSV File", "Export products to CSV file format.");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 551, true);
			this.Name = "ExportProductsToCSVForm";
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
