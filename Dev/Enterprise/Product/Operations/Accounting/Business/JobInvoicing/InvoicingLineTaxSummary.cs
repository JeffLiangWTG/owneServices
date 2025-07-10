using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class InvoicingLineTaxSummary : NonPersistentBusinessObject
	{
		public ZString TaxID { get; set; }

		public ZString Message { get; set; }

		public ZDecimal LocalExTaxAmount { get; set; }

		public ZDecimal LocalTaxAmount { get; set; }

		public ZDecimal LocalTotalAmount { get; set; }

		public ZDecimal TaxRate { get; set; }

		public ZString TaxIDDescription { get; set; }
	}
}
