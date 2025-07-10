using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceLineForTesting : JobComInvoiceLine
	{
		public JobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public DutyAndTaxCollection dutiesAndTaxesExposed
		{
			get => dutiesAndTaxes;
		}
	}
}
