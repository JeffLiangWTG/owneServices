using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UACreditNote))]
	public class UACreditNoteMatchingTest : InvoicingBaseMatchingTest
	{
		protected override InvoicingBase GetNewInvoice()
		{
			return Factory.New<UACreditNote>();
		}
	}
}
