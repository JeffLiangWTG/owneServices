using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ImportDirectoryForm : ZChildForm
	{
		public ImportDirectoryForm(FileImporter importer)
			: base(importer)
		{
			InitializeComponent();
#if WINZOR
			DirectoryBrowseButton.Visible = false;
#endif
		}

		protected ImportDirectoryForm()
			: base()
		{
			InitializeComponent();
		}

		protected FileImporter FileImporter
		{
			get { return (FileImporter)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Actions

		void DirectoryBrowseButton_Click(object sender, System.EventArgs e)
		{
			ImportDirectoryDialog.RequireMappablePath = true;
			ImportDirectoryDialog.SelectedPath = FilePathTextBox.Text;

			if (ImportDirectoryDialog.ShowDialog() == DialogResult.OK)
			{
				FilePathTextBox.Text = ImportDirectoryDialog.MappedSelectedPath;
			}

			ActiveControl = FilePathTextBox;
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			if (ValidateForm())
			{
				try
				{
					FileImporter.DefaultImportDirectory = FileSystem.ConvertToTerminalSyntaxInRemoteAppSession(FilePathTextBox.Text);
					ValidateAndSave();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				if (!FileImporter.HasErrors)
				{
					FileImporter.SaveRegistrySettings();

					// manually taking care of setting the dialog result because 
					// the presavevalidation() on the BizO wasn't stopping the dialog result
					// going back to the parent :(
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		protected virtual bool ValidateForm()
		{
			return true;
		}

		#endregion
	}
}
