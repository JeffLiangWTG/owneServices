using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.Client.OIA.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.OIA.GUI
{
	internal partial class OIAGLTransactionsForm : CsvExportGLTransactionForm
	{
		public OIAGLTransactionsForm(OIAGLTransactionBusinessObject businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		protected override GLTransactionExporter NewExporter()
		{
			return new OIAGLTransactionExporter((OIAGLTransactionBusinessObject)BusinessEntity, new NotificationBuffer(this));
		}
	}
}
