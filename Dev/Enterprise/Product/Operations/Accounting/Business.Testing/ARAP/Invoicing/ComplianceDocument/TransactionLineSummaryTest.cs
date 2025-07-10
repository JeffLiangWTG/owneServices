using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(TransactionLineSummary))]
	public class TransactionLineSummaryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			return new TransactionLineSummary(line);
		}
	}
}
