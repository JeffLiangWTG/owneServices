using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNote))]
	public class ARCreditNoteMatchingTest : InvoicingBaseMatchingTest
	{
		protected override InvoicingBase GetNewInvoice()
		{
			return Factory.New<ARCreditNote>();
		}
	}
}
