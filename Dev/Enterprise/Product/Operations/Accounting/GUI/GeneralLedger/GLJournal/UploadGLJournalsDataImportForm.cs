using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class UploadGLJournalsDataImportForm : DataImporterForm
	{
		protected UploadGLJournalsDataImportForm()
		{
			InitializeComponent();
			Init();
		}

		public UploadGLJournalsDataImportForm(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
			: base(businessEntity, formCaption, interfaceName) // Interface name for billing purposes
		{
			InitializeComponent();
			Init();
		}

		protected override string ImportFileFilter
		{
			get { return (NoResString)"CSV Files (*.csv)|*.csv"; }
		}

		ZButton CopyOutputToClipboardButton;

		void Init()
		{
			var recordsAddedCalcEdit = Controls.Find("RecordsAddedCalcEdit", true).First();
			var recordsUpdatedCalcEdit = Controls.Find("RecordsUpdatedCalcEdit", true).First();

			Controls.Remove(recordsAddedCalcEdit);
			Controls.Remove(recordsUpdatedCalcEdit);
		}

		void ClipboardCopyButton_Click(object sender, System.EventArgs e)
		{
			var data = new DataObject();
			data.SetData(DataFormats.Text, ProgressTextBox.Text);
			if (!SafeClipboard.SetDataObject(data, true))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

		TextBox ProgressTextBox => Controls.Find("ProgressTextBox", true).First() as TextBox;

		protected override bool OnBeforeImport()
		{
			Importer = new MultiCompaniesGLJournalFlatFileDataImporter();
			return base.OnBeforeImport();
		}
	}
}

