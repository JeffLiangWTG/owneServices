using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Registry.Business;

namespace Enterprise.Client.OIA.Business
{
	internal class OIAGLTransactionBusinessObject : GLTransactionBusinessObject
	{
		public OIAGLTransactionBusinessObject(BusinessObjectFactory factory) : base(factory) { }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ExportDirectory = SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.Value;
			ExportExistingBatch = true;
		}
	}
}
