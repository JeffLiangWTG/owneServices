using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
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

			var refCusCodeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ExportTransportDocument, "EU001", "EU001 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, ExportTransportDocument, "IE002", "IE002 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, ExportTransportDocument, "LV003", "LV003 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "Inval", "IE004", "IE004 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, ExportTransportDocument, "IE005", "IE005 DESC", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, ExportTransportDocument, "IE006", "IE006 DESC", ZDateTime.Today.AddDays(-3), ZDateTime.Today.AddDays(-2));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				(_, _, var exitControlSupportingDocument) = AdditionalInfoTest.GetNewBusinessObject(Factory);
				var lookups = exitControlSupportingDocument.Lookups;
				var codeList = lookups.CodeList as ZZRefCusCodeListCombinedCollection;
				var completeFilter = codeList.CompleteFilter;
				CombineAssertions(() =>
				{
					AssertSame("Cached", codeList, AdditionalInfoTest.GetNewBusinessObject(Factory).cusExitReportItemAdditionalInfo.Lookups.CodeList);
					AssertEquals("Matched Gorup code: EUN", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
					AssertEquals("Matched Country code: IE", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));

					AssertEquals("Unmatched Country code: LV", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));

					AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
					AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
					AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				});
			}
		}
	}
}
