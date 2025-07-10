using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class ExitControlAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Code_AI44X, "AI44X DESC");
			helper.CreateNewOrGetExistingCusCodeType(Code_2AAA, "2AAA DESC");

			var refCusCodeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Code_AI44X, "EU001", "EU001 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Code_AI44X, "DE002", "DE002 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Code_AI44X, "LV003", "LV003 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Code_2AAA, "DE004", "DE004 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Code_AI44X, "DE005", "DE005 DESC", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Code_AI44X, "DE006", "DE006 DESC", ZDateTime.Today.AddDays(-3), ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var codeList = lookups.CodeList as ZZRefCusCodeListCombinedCollection;
			var completeFilter = codeList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertSame("Cached", codeList, lookups.CodeList);
				AssertEquals("Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched DE", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_JobReference = exitHeader.PK.ToString().Substring(0, 35);
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			exitControlAdditionalInfo = consignment.AdditionalInfos.AddNew();
			lookups = exitControlAdditionalInfo.Lookups;
		}
		ExitControlAdditionalInfo exitControlAdditionalInfo;
		ExitControlAdditionalInfoLookups lookups;
	}
}
