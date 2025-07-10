using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APAdjustmentNote))]
	public class APAdjustmentNoteMatchingTest : InvoicingBaseMatchingTest
	{
		protected override InvoicingBase GetNewInvoice()
		{
			return Factory.New<APAdjustmentNote>();
		}
	}
}
