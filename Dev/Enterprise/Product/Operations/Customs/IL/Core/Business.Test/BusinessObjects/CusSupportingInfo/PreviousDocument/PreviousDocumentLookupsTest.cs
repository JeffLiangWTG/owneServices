using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeType = helper.CreateCusCodeType("ENSTY", "IL Entry Style");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeType.ZZK_CodeType, "1", ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeType.ZZK_CodeType, "2", ZDateTime.BrettsBirthday, ZDateTime.Today);
			factory.Save();

			var previousDocument = GetNewBusinessObject();
			var lookups = new PreviousDocumentLookups(previousDocument);

			AssertType<ZZRefCusCodeListCombinedCollection>(lookups.CodeList);
			var values = lookups.CodeList;
			values.Load();
			AssertEquals(2, values.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1", "2" }, values.Select(x => x.ZZD_Code));
		}

		public void TestUnitOfQuantityList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeTypeI = helper.CreateCusCodeType("CUSUQ", "IL Customs Units Of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeI.ZZK_CodeType, "ANN", ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeI.ZZK_CodeType, "T3C", ZDateTime.BrettsBirthday, ZDateTime.Today);
			factory.Save();

			var previousDocument = GetNewBusinessObject();
			var lookups = new PreviousDocumentLookups(previousDocument);

			AssertType<ZZRefCusCodeListCombinedCollection>(lookups.UnitOfQuantityList);
			var values = lookups.UnitOfQuantityList;
			values.Load();
			AssertEquals(2, values.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ANN", "T3C" }, values.Select(x => x.ZZD_Code));
		}

		PreviousDocument GetNewBusinessObject()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			var jobComInvoiceLine = (JobComInvoiceLine)jobComInvoiceHeader.InvoiceLines.AddNew();
			return jobComInvoiceLine.PreviousDocuments.AddNew();
		}
	}
}
