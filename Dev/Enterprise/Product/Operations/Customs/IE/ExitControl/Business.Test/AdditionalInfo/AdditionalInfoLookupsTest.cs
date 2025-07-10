using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(ExportTransportDocument, "TD44E DESC");
			helper.CreateNewOrGetExistingCusCodeType("Inval", "Inval DESC");

			var refCusCodeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, ExportTransportDocument, "IE002", "IE002 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, ExportTransportDocument, "LV003", "LV003 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "Inval", "IE004", "IE004 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, ExportTransportDocument, "IE005", "IE005 DESC", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, ExportTransportDocument, "IE006", "IE006 DESC", ZDateTime.Today.AddDays(-3), ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var additionalInfo = AdditionalInfoTest.GetNewBusinessObject(Factory);
			var lookups = additionalInfo.Lookups;
			var codeList = lookups.CodeList as ZZRefCusCodeListCombinedCollection;
			var completeFilter = codeList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertSame("Cached", codeList, lookups.CodeList);
				AssertEquals("Matched IE", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched LV", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
			});
		}
	}
}
