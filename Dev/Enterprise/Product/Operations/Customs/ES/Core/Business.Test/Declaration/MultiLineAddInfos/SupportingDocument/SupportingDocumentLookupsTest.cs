using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUOMList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList(CountryCodes.Spain, "ZZ", "AH3", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList("DGC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var suppDoc = invLine.SupportingDocuments.AddNew();
			AssertEquals("ABC, DEF", suppDoc.Lookups.CustomsUQList.CodesAsString);
		}

		public void TestSupportingDocumentProcedureList()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			AssertType<SupportingDocumentProcedureCodeList>("Should be SupportingDocumentProcedureCodeList", supportingDocument.Lookups.SupportingDocumentProcedureList);
		}
	}
}
