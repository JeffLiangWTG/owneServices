using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APCreditNote))]
	public class APCreditNoteMatchingTest : InvoicingBaseMatchingTest
	{
		protected override InvoicingBase GetNewInvoice()
		{
			return Factory.New<APCreditNote>();
		}
	}
}
