using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.PeriodApportionment
{
	[TestedType(typeof(PeriodApportionmentLine))]
	public class PeriodApportionmentLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, 100);

			return new PeriodApportionmentLine(invoiceLine, 20201010, 350);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
