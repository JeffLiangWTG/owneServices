using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using NctsRefCusCodeListLevelTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListLevelTypes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillAdditionalDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeCodeList_SubTypeTRA()
		{
			AssertTypeCodeList(AdditionalInfoSubTypeList.Codes.TransportDocument);
		}

		public void TestTypeCodeList_SubTypeTRA_IncludeParentDataGrouping()
		{
			AssertTypeCodeList_IncludeParentDataGouping(AdditionalInfoSubTypeList.Codes.TransportDocument);
		}

		public void TestTypeCodeList_SubTypeREF()
		{
			AssertTypeCodeList(AdditionalInfoSubTypeList.Codes.AdditionalReference);
		}

		public void TestTypeCodeList_SubTypeREF_IncludeParentDataGrouping()
		{
			AssertTypeCodeList_IncludeParentDataGouping(AdditionalInfoSubTypeList.Codes.AdditionalReference);
		}

		public void TestTypeCodeList_SubTypeINF()
		{
			AssertTypeCodeList(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
		}

		public void TestTypeCodeList_SubTypeINF_IncludeParentDataGrouping()
		{
			AssertTypeCodeList_IncludeParentDataGouping(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
		}

		public void TestTypeCodeList_WithoutParentDataGrouping()
		{
			nctsHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertTypeCodeList_IncludeParentDataGouping(AdditionalInfoSubTypeList.Codes.TransportDocument);
			AssertTypeCodeList_IncludeParentDataGouping(AdditionalInfoSubTypeList.Codes.AdditionalReference);
			AssertTypeCodeList_IncludeParentDataGouping(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
		}

		public void TestTypeCodeList_InvalidSubType()
		{
			additionalDocument.CSI_SubType = "INV";
			AssertNotNull("When subType is not valid, return full list", lookups.TypeCodeList);
		}

		public void TestSubTypeList()
		{
			CombineAssertions(() =>
			{
				var entryStyleList = lookups.SubTypeList;
				AssertEquals("SubTypeList CodesAsString", "INF, REF, TRA", entryStyleList.CodesAsString);
				AssertEquals("INF Description", "Additional Information", entryStyleList.GetDescriptionFromCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation));
				AssertEquals("REF Description", "Additional Reference", entryStyleList.GetDescriptionFromCode(AdditionalInfoSubTypeList.Codes.AdditionalReference));
				AssertEquals("TRA Description", "Transport Document", entryStyleList.GetDescriptionFromCode(AdditionalInfoSubTypeList.Codes.TransportDocument));
			});
		}

		public void TestSubTypeListDEC_Arrival() => AssertSubTypeList(NctsUnloadedStateList.Codes.DEC, "INF, REF, TRA");
		public void TestSubTypeListDIF_Arrival() => AssertSubTypeList(NctsUnloadedStateList.Codes.DIF, "INF, REF, TRA");
		public void TestSubTypeListMIS_Arrival() => AssertSubTypeList(NctsUnloadedStateList.Codes.MIS, "INF, REF, TRA");
		public void TestSubTypeListNEW_Arrival() => AssertSubTypeList(NctsUnloadedStateList.Codes.NEW, "REF, TRA");

		void AssertSubTypeList(string unloadedState, string expectedCodeList)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var additionalDocument = nctsHeader.Bills.AddNew().AdditionalDocuments.AddNew();
			additionalDocument.CSI_Status = unloadedState;
			var subTypeList = new NctsBillAdditionalDocumentLookups(additionalDocument).SubTypeList;
			AssertEquals("SubTypeList CodesAsString", expectedCodeList, subTypeList.CodesAsString);
		}

		public void TestStatusList()
		{
			CombineAssertions(() =>
			{
				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.MIS;
				var statusList = lookups.StatusList;

				AssertEquals("StatusList CodesAsString", "DEC, MIS", statusList.CodesAsString);
				AssertEquals("DEC Description", "Declared Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.DEC));
				AssertEquals("MIS Description", "Missing Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.MIS));
				AssertSame("Cached", statusList, lookups.StatusList);

				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
				statusList = lookups.StatusList;

				AssertEquals("StatusList CodesAsString", "DEC, MIS, NEW", statusList.CodesAsString);
				AssertEquals("NEW Description", "New Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.NEW));
				AssertSame("Cached", statusList, lookups.StatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
			lookups = new NctsBillAdditionalDocumentLookups(additionalDocument);
		}
		NctsHeader nctsHeader;
		NctsBillAdditionalDocument additionalDocument;
		NctsBillAdditionalDocumentLookups lookups;

		void AssertTypeCodeList(string subType)
		{
			additionalDocument.CSI_SubType = subType;
			var codeType = additionalDocument.GetCodeTypeBySubType();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(codeType, $"CusCodeType{codeType}");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, codeType + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "02", "02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "03", "03 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "04", "04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList4.PK, RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
			var refCusCodeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, codeType, codeType + "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList5.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "DC000", codeType + "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList6.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList7.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList8 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeListAttribute(refCusCodeList8.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
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

		void AssertTypeCodeList_IncludeParentDataGouping(string subType)
		{
			additionalDocument.CSI_SubType = subType;
			var codeType = additionalDocument.GetCodeTypeBySubType();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingCusCodeType(codeType, $"CusCodeType{codeType}");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, codeType + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "03", "03 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "04", "04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList4.PK, RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
			var refCusCodeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, codeType, codeType + "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList5.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "DC000", codeType + "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList6.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList7.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			var refCusCodeList8 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeListAttribute(refCusCodeList8.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
			Factory.Save();

			var typeCodesList = lookups.TypeCodeList;
			var completeFilter = typeCodesList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
				AssertSame("Cached", typeCodesList, lookups.TypeCodeList);
				AssertEquals("Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("No CusCodeListAttribute", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched AttributeValue", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
			});
		}
	}
}
