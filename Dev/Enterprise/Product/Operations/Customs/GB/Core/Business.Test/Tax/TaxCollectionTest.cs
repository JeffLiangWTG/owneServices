using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxCollection))]
	public class TaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementType()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var gbTax = invLine.Taxes.AddNew();
			AssertType<JobComInvoiceLineTax>(gbTax);
			AssertType<JobComInvoiceLineTax>(invLine.Taxes[0]);
		}

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert(true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			return invLine.Taxes;
		}
	}
}
