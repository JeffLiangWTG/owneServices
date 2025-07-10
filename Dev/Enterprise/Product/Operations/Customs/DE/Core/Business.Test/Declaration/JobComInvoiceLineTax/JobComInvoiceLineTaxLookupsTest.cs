using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class JobComInvoiceLineTaxLookupsTest : TestCaseWithFactory
	{
		public void TestTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var tax = declaration.Invoices.AddNew().InvoiceLines.AddNew().Taxes.AddNew();
			var lookups = tax.Lookups;
			var collection = (CodeDescriptionPairList)lookups.TypeList;

			AssertEquals(20, collection.Count);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 20, 30, 31, 32, 33, 35, 36, 37", collection.CodesAsString);
			AssertSame(collection, lookups.TypeList);
		}

		public void TestMethodOfCalculationList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var tax = declaration.Invoices.AddNew().InvoiceLines.AddNew().Taxes.AddNew();
			var lookups = tax.Lookups;
			var collection = lookups.MethodOfCalculationList;

			AssertEquals(9, collection.Count);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09", collection.CodesAsString);
			AssertSame(collection, lookups.MethodOfCalculationList);
		}
	}
}
