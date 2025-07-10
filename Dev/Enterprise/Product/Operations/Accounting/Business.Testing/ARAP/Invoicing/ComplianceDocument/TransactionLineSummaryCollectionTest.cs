using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(TransactionLineSummaryCollection))]
	public class TransactionLineSummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TransactionLineSummaryCollection>
	{
		protected override TransactionLineSummaryCollection GetCollectionToTest()
		{
			return new TransactionLineSummaryCollection(new BusinessObjectFactory());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			return new TransactionLineSummary(line);
		}
	}
}
