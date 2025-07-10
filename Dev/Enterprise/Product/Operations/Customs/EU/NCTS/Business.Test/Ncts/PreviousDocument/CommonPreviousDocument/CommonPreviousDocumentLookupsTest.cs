using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using NctsRefCusCodeListLevelTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListLevelTypes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CommonPreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeCodeListForBillPreviousDocument_ExcludeParentDataGrouping()
		{
			var lookups = nctsHeader.Bills.AddNew().PreviousDocuments.AddNew().Lookups;
			AssertTypeCodeList_ExcludeParentDataGrouping(lookups, NctsRefCusCodeListLevelTypes.House);
		}

		public void TestTypeCodeListForHeaderPreviousDocument_ExcludeParentDataGrouping()
		{
			var lookups = nctsHeader.PreviousDocuments.AddNew().Lookups;
			AssertTypeCodeList_ExcludeParentDataGrouping(lookups, NctsRefCusCodeListLevelTypes.Header);
		}

		public void TestTypeCodeListForBillPreviousDocument_IncludeParentDataGrouping()
		{
			var lookups = nctsHeader.Bills.AddNew().PreviousDocuments.AddNew().Lookups;
			AssertTypeCodeList_IncludeParentDataGrouping(lookups, NctsRefCusCodeListLevelTypes.House);
		}

		public void TestTypeCodeListForHeaderPreviousDocument_IncludeParentDataGrouping()
		{
			var lookups = nctsHeader.PreviousDocuments.AddNew().Lookups;
			AssertTypeCodeList_IncludeParentDataGrouping(lookups, NctsRefCusCodeListLevelTypes.Header);
		}

		public void TestTypeCodeListForBillPreviousDocument_WithoutParentDataGrouping()
		{
			nctsHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var lookups = nctsHeader.Bills.AddNew().PreviousDocuments.AddNew().Lookups;
			AssertTypeCodeList_IncludeParentDataGrouping(lookups, NctsRefCusCodeListLevelTypes.House);
		}

		public void TestTypeCodeListForHeaderPreviousDocument_WithoutParentDataGrouping()
		{
			nctsHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var lookups = nctsHeader.PreviousDocuments.AddNew().Lookups;
			AssertTypeCodeList_IncludeParentDataGrouping(lookups, NctsRefCusCodeListLevelTypes.Header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;

		void AssertTypeCodeList_ExcludeParentDataGrouping(CommonPreviousDocumentLookups lookups, string validAttributeValue)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Codes.PreviousDocumentOfNCTS, "Previous Document Of NCTS");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Codes.PreviousDocumentOfNCTS, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Codes.PreviousDocumentOfNCTS, "SD02", "SD02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Codes.PreviousDocumentOfNCTS, "SD03", "SD03 SD02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Codes.PreviousDocumentOfNCTS, "SD04", "SD04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList4.PK, RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
			var refCusCodeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Codes.PreviousDocumentOfNCTS, "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList5.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "DC000", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList6.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Codes.PreviousDocumentOfNCTS, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList7.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList8 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Codes.PreviousDocumentOfNCTS, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeListAttribute(refCusCodeList8.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			Factory.Save();

			var typeCodesList = lookups.TypeCodeList;
			var completeFilter = typeCodesList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
				AssertSame("Cached", typeCodesList, lookups.TypeCodeList);
				AssertEquals("Matched EUN", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched LV", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("No CusCodeListAttribute", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched AttributeValue", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
			});
		}

		void AssertTypeCodeList_IncludeParentDataGrouping(CommonPreviousDocumentLookups lookups, string validAttributeValue)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingCusCodeType(Codes.PreviousDocumentOfNCTS, "Previous Document Of NCTS");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Codes.PreviousDocumentOfNCTS, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Codes.PreviousDocumentOfNCTS, "SD03", "SD03 SD02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Codes.PreviousDocumentOfNCTS, "SD04", "SD04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList3.PK, RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
			var refCusCodeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Codes.PreviousDocumentOfNCTS, "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList4.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList5 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DC000", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList5.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList6 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Codes.PreviousDocumentOfNCTS, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList6.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			var refCusCodeList7 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Codes.PreviousDocumentOfNCTS, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeListAttribute(refCusCodeList7.PK, RefCusCodeListAttributeTypes.Codes.Level, validAttributeValue);
			Factory.Save();

			var typeCodesList = lookups.TypeCodeList;
			var completeFilter = typeCodesList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
				AssertSame("Cached", typeCodesList, lookups.TypeCodeList);
				AssertEquals("Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("No CusCodeListAttribute", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched AttributeValue", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
			});
		}
	}
}
