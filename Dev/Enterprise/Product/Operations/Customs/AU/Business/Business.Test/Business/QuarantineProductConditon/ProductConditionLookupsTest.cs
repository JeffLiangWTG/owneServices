using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ProductConditionLookupsTest : TestCaseWithFactory
	{
		public void TestProductConditionList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.EXDOCSProductCondition, "EXDOCS Code Set - E43 Product Condition");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "T1", "Test1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "T2", "Test2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "7", "DEBARKED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "8", "BARK-FREE", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "9", "DRIED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "10", "CHOPPED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "11", "PEELED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var exdocHeader = invoiceHeader.QuarantineExDocHeader;
			exdocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var exdocLine = invoiceLine.QuarantineExDocLine;
			var productCondition = exdocLine.ProductConditions.AddNew();
			var lookups = productCondition.Lookups;

			AssertEquals("ProductConditionList", 7, lookups.CY_CodeList.Count);
			AssertEquals("Ordered alphabetically", "10, 11, 7, 8, 9, T1, T2", lookups.CY_CodeList.CodesAsString);
		}
	}
}
