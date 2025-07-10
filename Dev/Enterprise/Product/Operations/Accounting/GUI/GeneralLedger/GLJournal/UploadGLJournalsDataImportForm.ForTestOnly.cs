#if DEBUG

using System.Windows.Forms;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class UploadGLJournalsDataImportForm
	{
		public string ImportFileFilter_ForTestOnly => ImportFileFilter;

		public void ImportFromFile_ForTestOnly(string fileName)
		{
			ImportFromFile(fileName);
		}

		public void ClipboardCopyButton_Click_ForTestOnly(object sender, System.EventArgs e)
		{
			ClipboardCopyButton_Click(sender, e);
		}

		public TextBox ProgressTextBox_ForTestOnly => ProgressTextBox;
	}
}

#endif
