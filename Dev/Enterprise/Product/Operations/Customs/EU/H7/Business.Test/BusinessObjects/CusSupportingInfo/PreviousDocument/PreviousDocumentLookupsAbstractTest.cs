using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	public abstract class PreviousDocumentLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		public void TestPreviousDocumentCodeList()
		{
			var codeList = (ZZRefCusCodeListCombinedCollection)GetLookupsForTesting().CodeList;
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(ExpectedDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "270", "Delivery note", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(ExpectedDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "271", "Packing list", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "325", "Proforma invoice", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "465", "House waybill", yesterday, tomorrow);
			var correctCusCode = helper.CreateNewOrGetExistingCusCodeList(ExpectedDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "235", "Container list", yesterday, tomorrow);
			Factory.Save();
			codeList.Load();

			CombineAssertions(() =>
			{
				AssertEquals("There should only be one CusCode that meets the conditions", 1, codeList.Count);
				Assert("Should include the correct CusCode", codeList.Contains(correctCusCode));
			});
		}

		protected abstract PreviousDocumentLookups GetLookupsForTesting();

		protected abstract ZString ExpectedDataGrouping { get; }
	}
}
