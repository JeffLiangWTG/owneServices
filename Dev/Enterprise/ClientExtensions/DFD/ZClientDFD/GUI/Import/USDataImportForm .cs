using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.GUI;

namespace Enterprise.Client.DFD.GUI.Import
{
	class USDataImportForm : DataImporterForm
	{
		public USDataImportForm(string caption) : base(caption, BillingInterfaceName.ClientSpecifiedImport) { }

		protected override string ImportFileFilter
		{
			get { return "CSV Files (*.csv)|*.csv"; }
		}
	}
}
