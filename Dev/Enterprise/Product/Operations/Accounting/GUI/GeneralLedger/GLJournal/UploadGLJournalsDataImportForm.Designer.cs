using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class UploadGLJournalsDataImportForm
	{
new void InitializeComponent()
		{
			this.CopyOutputToClipboardButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CopyOutputToClipboardButton
			// 
			this.CopyOutputToClipboardButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyOutputToClipboardButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UploadGLJournalsDataImporterForm|57CF9FCF-E467-4BB2-BDEE-4987BC3A4553", "Copy output to Clipboard");
			this.CopyOutputToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 498, true);
			this.CopyOutputToClipboardButton.Name = "CopyOutputToClipboardButton";
			this.CopyOutputToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 21, true);
			this.CopyOutputToClipboardButton.TabIndex = 13;
			this.CopyOutputToClipboardButton.ToolTipCaption = null;
			this.CopyOutputToClipboardButton.Click += new System.EventHandler(this.ClipboardCopyButton_Click);
			// 
			// UploadGLJournalsDataImportForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Controls.Add(this.CopyOutputToClipboardButton);
			this.Name = "UploadGLJournalsDataImportForm";
			this.Controls.SetChildIndex(this.ImportFromFileButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox, 0);
			this.Controls.SetChildIndex(this.CopyOutputToClipboardButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}