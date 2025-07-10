using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceMiscInvoicesFilterBusinessObject))]
	public class PeriodicInvoiceMiscInvoicesFilterBusinessObject_ForBulkInvoiceTest : PeriodicInvoiceMiscInvoicesFilterBusinessObjectTest
	{
		protected override PeriodicInvoiceBase CreatePeriodicInvoice()
		{
			return new PeriodicInvoiceBulk(Factory);
		}
	}
}
