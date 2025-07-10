using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefCusCodeListAttributeTypes = Enterprise.Customs.Universal.RefCusCodeListAttributeTypes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeCodeListForHeaderSupportingDocument_IncludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.Header, RefCusCodeListLevelTypes.Item, nctsHeader.MovementHeader.SupportingDocuments);
		}

		public void TestTypeCodeListForHeaderSupportingDocument_ExcludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure);
			AssertTypeCodeList_ExcludeParentDataGrouping(RefCusCodeListLevelTypes.Header, RefCusCodeListLevelTypes.Item, nctsHeader.MovementHeader.SupportingDocuments);
		}

		public void TestTypeCodeListForHeaderSupportingDocument_WithoutParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure, Core.Constants.CountryCodes.UnitedKingdom);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.Header, RefCusCodeListLevelTypes.Item, nctsHeader.MovementHeader.SupportingDocuments);
		}

		public void TestTypeCodeListForArrivalMovementHeaderSupportingDocument_IncludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Arrival);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.Header, RefCusCodeListLevelTypes.Item, nctsHeader.ArrivalMovementHeader.SupportingDocuments);
		}

		public void TestTypeCodeListForArrivalMovementHeaderSupportingDocument_ExcludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Arrival);
			AssertTypeCodeList_ExcludeParentDataGrouping(RefCusCodeListLevelTypes.Header, RefCusCodeListLevelTypes.Item, nctsHeader.ArrivalMovementHeader.SupportingDocuments);
		}

		public void TestTypeCodeListForArrivalMovementHeaderSupportingDocument_WithoutParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Arrival, Core.Constants.CountryCodes.UnitedKingdom);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.Header, RefCusCodeListLevelTypes.Item, nctsHeader.ArrivalMovementHeader.SupportingDocuments);
		}

		public void TestTypeCodeListForBillSupportingDocument_IncludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.House, RefCusCodeListLevelTypes.Header, nctsHeader.Bills.AddNew().SupportingDocuments);
		}

		public void TestTypeCodeListForBillSupportingDocument_ExcludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure);
			AssertTypeCodeList_ExcludeParentDataGrouping(RefCusCodeListLevelTypes.House, RefCusCodeListLevelTypes.Header, nctsHeader.Bills.AddNew().SupportingDocuments);
		}

		public void TestTypeCodeListForBillSupportingDocument_WithoutParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure, Core.Constants.CountryCodes.UnitedKingdom);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.House, RefCusCodeListLevelTypes.Header, nctsHeader.Bills.AddNew().SupportingDocuments);
		}

		public void TestTypeCodeListForCargoDescSupportingDocument_IncludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.Item, RefCusCodeListLevelTypes.House, nctsHeader.Bills.AddNew().GoodsItems.AddNew().SupportingDocuments);
		}

		public void TestTypeCodeListForCargoDescSupportingDocument_ExcludeParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure);
			AssertTypeCodeList_ExcludeParentDataGrouping(RefCusCodeListLevelTypes.Item, RefCusCodeListLevelTypes.House, nctsHeader.Bills.AddNew().GoodsItems.AddNew().SupportingDocuments);
		}

		public void TestTypeCodeListForCargoDescSupportingDocument_WithoutParentDataGrouping()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure, Core.Constants.CountryCodes.UnitedKingdom);
			AssertTypeCodeList_IncludeParentDataGrouping(RefCusCodeListLevelTypes.Item, RefCusCodeListLevelTypes.House, nctsHeader.Bills.AddNew().GoodsItems.AddNew().SupportingDocuments);
		}

		void AssertTypeCodeList_ExcludeParentDataGrouping(string validLevel, string invalidLevel, INctsSupportingDocumentCollection<NctsSupportingDocument> supportingDocuments)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code.SupportingDocumentOfNCTS, "Supporting Document Of NCTS");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");
			Factory.Save();

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD02", "SD02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD03", "SD03 SD02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD04", "SD04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList4.PK, RefCusCodeListAttributeTypes.Codes.Level, invalidLevel);
			var refCusCodeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList5.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "DC000", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList6.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList7.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList8 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeListAttribute(refCusCodeList8.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			Factory.Save();

			var supportingDocument = supportingDocuments.AddNew();
			var typeCodesList = supportingDocument.Lookups.TypeCodeList;
			var completeFilter = typeCodesList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
				AssertSame("Cached", typeCodesList, supportingDocument.Lookups.TypeCodeList);
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

		void AssertTypeCodeList_IncludeParentDataGrouping(string validLevel, string invalidLevel, INctsSupportingDocumentCollection<NctsSupportingDocument> supportingDocuments)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code.SupportingDocumentOfNCTS, "Supporting Document Of NCTS");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");
			Factory.Save();

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD03", "SD03 SD02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD04", "SD04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList4.PK, RefCusCodeListAttributeTypes.Codes.Level, invalidLevel);
			var refCusCodeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList5.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "DC000", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList6.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList7.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			var refCusCodeList8 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeListAttribute(refCusCodeList8.PK, RefCusCodeListAttributeTypes.Codes.Level, validLevel);
			Factory.Save();

			var supportingDocument = supportingDocuments.AddNew();
			var typeCodesList = supportingDocument.Lookups.TypeCodeList;
			var completeFilter = typeCodesList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
				AssertSame("Cached", typeCodesList, supportingDocument.Lookups.TypeCodeList);
				AssertEquals("Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("No CusCodeListAttribute", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched AttributeValue", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
			});
		}

		public void TestStatusList()
		{
			var nctsHeader = CreateNcts5Header(NctsMovementType.Codes.Departure);
			var supportingDocument = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.MIS;
			CombineAssertions(() =>
			{
				var statusList = supportingDocument.Lookups.StatusList;

				AssertEquals("StatusList CodesAsString", "DEC, MIS", statusList.CodesAsString);
				AssertEquals("DEC Description", "Received from Customs", statusList.GetDescriptionFromCode(SupportingDocumentStatusList.Codes.DEC));
				AssertEquals("MIS Description", "Missing Document", statusList.GetDescriptionFromCode(SupportingDocumentStatusList.Codes.MIS));
				AssertSame("Cached", statusList, supportingDocument.Lookups.StatusList);

				supportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				statusList = supportingDocument.Lookups.StatusList;

				AssertEquals("StatusList CodesAsString", "DEC, MIS, NEW", statusList.CodesAsString);
				AssertEquals("NEW Description", "Registered by User", statusList.GetDescriptionFromCode(SupportingDocumentStatusList.Codes.NEW));
				AssertSame("Cached", statusList, supportingDocument.Lookups.StatusList);
			});
		}

		NctsHeader CreateNcts5Header(string movementType, string countryCode = null)
		{
			var result = Factory.New<NctsHeader>();
			result.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			result.SetMovementType(movementType);
			if (countryCode != null)
			{
				result.Company.GC_RN_NKCountryCode = countryCode;
			}
			return result;
		}
	}
}
