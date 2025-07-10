using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.PeriodApportionment
{
	[TestedType(typeof(PeriodApportionmentLinesCollection))]
	public class PeriodApportionmentLinesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PeriodApportionmentLinesCollection>
	{
		protected override PeriodApportionmentLinesCollection GetCollectionToTest()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, 100);

			return invoiceLine.PeriodApportionmentLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, 100);

			return new PeriodApportionmentLine(invoiceLine, 20201010, 350, 11);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		public void TestOverrides()
		{
			Assert(!Collection.AllowNew);
			Assert(!Collection.AllowRemove);
			Assert(!((IBindingList)Collection).SupportsSorting);
		}
	}
}
