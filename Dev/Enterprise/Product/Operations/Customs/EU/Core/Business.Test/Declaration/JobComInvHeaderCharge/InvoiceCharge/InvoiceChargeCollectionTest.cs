using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceChargeCollection<InvoiceCharge>))]
	public class InvoiceChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<InvoiceChargeCollection<InvoiceCharge>, InvoiceCharge>
	{
		protected override InvoiceChargeCollection<InvoiceCharge> GetCollectionToTest()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			return (InvoiceChargeCollection<InvoiceCharge>)invoice.Charges;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(InvoiceCharge));
		}
	}
}
