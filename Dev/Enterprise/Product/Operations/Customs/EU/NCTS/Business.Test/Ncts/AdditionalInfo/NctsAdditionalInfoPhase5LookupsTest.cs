using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsAdditionalInfoPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeList_GoodsItem_Departure()
		{
			var list = goodsItemLookups.SubTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "INF, REF", list.CodesAsString);
				AssertSame("Cached", list, goodsItemLookups.SubTypeList);
			});
		}

		public void TestSubTypeListDEC_GoodsItem_Arrival() => AssertSubTypeList_GoodsItem_Arrival(NctsUnloadedStateList.Codes.DEC, "INF, REF, TRA");
		public void TestSubTypeListDIF_GoodsItem_Arrival() => AssertSubTypeList_GoodsItem_Arrival(NctsUnloadedStateList.Codes.DIF, "INF, REF, TRA");
		public void TestSubTypeListMIS_GoodsItem_Arrival() => AssertSubTypeList_GoodsItem_Arrival(NctsUnloadedStateList.Codes.MIS, "INF, REF, TRA");
		public void TestSubTypeListNEW_GoodsItem_Arrival() => AssertSubTypeList_GoodsItem_Arrival(NctsUnloadedStateList.Codes.NEW, "REF, TRA");

		void AssertSubTypeList_GoodsItem_Arrival(string unloadedState, string expectedCodeList)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var additionalDocument = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
			additionalDocument.CSI_Status = unloadedState;
			var lookups = additionalDocument.Lookups;
			var subTypeList = lookups.SubTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("List", expectedCodeList, subTypeList.CodesAsString);
				AssertSame("Cached", subTypeList, lookups.SubTypeList);
			});
		}

		public void TestSubTypeList_Header()
		{
			var list = nctsHeaderLookups.SubTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "INF, REF, TRA", list.CodesAsString);
				AssertSame("Cached", list, nctsHeaderLookups.SubTypeList);
			});
		}

		public void TestSubTypeList_CachedSeparately()
		{
			CombineAssertions(() =>
			{
				var goodsItemList = goodsItemLookups.SubTypeList;
				AssertEquals("List for GoodsItem", "INF, REF", goodsItemList.CodesAsString);

				var headerList = nctsHeaderLookups.SubTypeList;
				AssertEquals("List for Header", "INF, REF, TRA", headerList.CodesAsString);
			});
		}

		public void TestCodeList_SubTypeREF_ExcludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, true);
			goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var list = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AR44N_1", "AR44N_2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, goodsItemLookups.CodeList);
			});
		}

		public void TestCodeList_SubTypeREF_IncludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, false);
			goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var list = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AR44N_3" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, goodsItemLookups.CodeList);
			});
		}

		public void TestCodeList_SubTypeTRA_ExcludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, true);
			goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			var list = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "TD44N_1", "TD44N_2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, goodsItemLookups.CodeList);
			});
		}

		public void TestCodeList_SubTypeTRA_IncludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, false);
			goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			var list = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "TD44N_3" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, goodsItemLookups.CodeList);
			});
		}

		public void TestCodeList_SubTypeINF_ExcludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, true);
			goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var list = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AI44N_1", "AI44N_2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, goodsItemLookups.CodeList);
			});
		}

		public void TestCodeList_SubTypeOther_IncludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, false);
			goodsItemAdditionalInfo.CSI_SubType = ZString.Empty;
			var list = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AI44N_3" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, goodsItemLookups.CodeList);
			});
		}

		public void TestCodeList_Header_SubTypeREF_ExcludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, true);
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var list = (ZZRefCusCodeListCombinedCollection)nctsHeaderLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AR44N_1", "AR44N_2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, nctsHeaderLookups.CodeList);
			});
		}

		public void TestCodeList_Header_SubTypeTRA_IncludeParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, false);
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			var list = (ZZRefCusCodeListCombinedCollection)nctsHeaderLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "TD44N_3" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, nctsHeaderLookups.CodeList);
			});
		}

		public void TestCodeList_Header_SubTypeINF()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, true);
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var list = (ZZRefCusCodeListCombinedCollection)nctsHeaderLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AI44N_1", "AI44N_2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, nctsHeaderLookups.CodeList);
			});
		}

		public void TestCodeList_Header_WithoutParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, false);
			nctsHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var list = (ZZRefCusCodeListCombinedCollection)nctsHeaderLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AI44N_3" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, nctsHeaderLookups.CodeList);
			});
		}

		public void TestCodeList_Item_WithoutParentDataGrouping()
		{
			SetupCusCodes(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, false);
			nctsHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			goodsItemAdditionalInfo.CSI_SubType = ZString.Empty;
			var list = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "AI44N_3" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, goodsItemLookups.CodeList);
			});
		}

		public void TestCodeList_Departure_CorrectList() => CombineAssertions(() =>
		{
			var codeListHeader = (ZZRefCusCodeListCombinedCollection)nctsHeaderLookups.CodeList;
			var codeListItem = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;

			AssertNotSame("Separate lists for different levels", codeListHeader, codeListItem);
			AssertSame("MovementHeader level", nctsHeaderAdditionalInfo.Lookups.CodeList, codeListHeader);
			AssertSame("Item level", goodsItemAdditionalInfo.Lookups.CodeList, codeListItem);
		});

		public void TestCodeList_Arrival() => CombineAssertions(() =>
		{
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = arrivalNctsHeader.ArrivalMovementHeader;

			var arrivalMovementHeaderAdditionalInfo = arrivalMovementHeader.AdditionalDocuments.AddNew();
			AssertNotNull("CodeList successfully provided at MovementHeader level", arrivalMovementHeaderAdditionalInfo.Lookups.CodeList);

			var arrivalGoodsItem = arrivalNctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			var arrivalGoodsItemAdditionalInfo = arrivalGoodsItem.AdditionalInfos.AddNew();
			AssertNotNull("CodeList provided successfully at GoodsItem level", arrivalGoodsItemAdditionalInfo.Lookups.CodeList);
		});

		public void TestCodeList_Arrival_CorrectList() => CombineAssertions(() =>
		{
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = arrivalNctsHeader.ArrivalMovementHeader;
			var arrivalMovementHeaderAdditionalInfo = arrivalMovementHeader.AdditionalDocuments.AddNew();
			var arrivalGoodsItem = arrivalNctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			var arrivalGoodsItemAdditionalInfo = arrivalGoodsItem.AdditionalInfos.AddNew();

			var codeListHeader = (ZZRefCusCodeListCombinedCollection)nctsHeaderLookups.CodeList;
			var codeListItem = (ZZRefCusCodeListCombinedCollection)goodsItemLookups.CodeList;

			AssertNotSame("Prerequisite", codeListHeader, codeListItem);
			AssertSame("Header list Cached", arrivalMovementHeaderAdditionalInfo.Lookups.CodeList, codeListHeader);
			AssertSame("Item list cached", arrivalGoodsItemAdditionalInfo.Lookups.CodeList, codeListItem);
		});

		public void TestStatusList()
		{
			CombineAssertions(() =>
			{
				goodsItemAdditionalInfo.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.MIS;
				var statusList = goodsItemLookups.StatusList;

				AssertEquals("StatusList CodesAsString", "DEC, MIS", statusList.CodesAsString);
				AssertEquals("DEC Description", "Declared Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.DEC));
				AssertEquals("MIS Description", "Missing Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.MIS));
				AssertSame("Cached", statusList, goodsItemLookups.StatusList);

				goodsItemAdditionalInfo.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
				statusList = goodsItemLookups.StatusList;

				AssertEquals("StatusList CodesAsString", "DEC, MIS, NEW", statusList.CodesAsString);
				AssertEquals("NEW Description", "New Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.NEW));
				AssertSame("Cached", statusList, goodsItemLookups.StatusList);
			});
		}

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			nctsHeaderAdditionalInfo = nctsHeader.AdditionalDocuments.AddNew();
			nctsHeaderLookups = new NctsAdditionalInfoPhase5Lookups(nctsHeaderAdditionalInfo);
			goodsItemAdditionalInfo = goodsItem.AdditionalInfos.AddNew();
			goodsItemLookups = new NctsAdditionalInfoPhase5Lookups(goodsItemAdditionalInfo);
		}
		NctsHeader nctsHeader;
		NctsAdditionalInfo nctsHeaderAdditionalInfo;
		NctsAdditionalInfoPhase5Lookups nctsHeaderLookups;
		NctsAdditionalInfo goodsItemAdditionalInfo;
		NctsAdditionalInfoPhase5Lookups goodsItemLookups;

		void SetupCusCodes(string correctCodeType, string incorrectCodeType, string level, bool excludeParentDatagrouping)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingCusCodeType(correctCodeType, correctCodeType);
			helper.CreateNewOrGetExistingCusCodeType(incorrectCodeType, incorrectCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", correctCodeType, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", incorrectCodeType, Core.Constants.CountryCodes.Latvia);

			if (excludeParentDatagrouping)
			{
				var valid1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, $"{correctCodeType}_1", "Valid1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(valid1.PK, RefCusCodeListAttributeTypes.Codes.Level, level);
				var valid2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, $"{correctCodeType}_2", "Valid2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(valid2.PK, RefCusCodeListAttributeTypes.Codes.Level, level);
			}
			var valid3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, correctCodeType, $"{correctCodeType}_3", "Valid code from parent grouping", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(valid3.PK, RefCusCodeListAttributeTypes.Codes.Level, level);

			var invalid1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, correctCodeType, "Invalid1", "Invalid country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid1.PK, RefCusCodeListAttributeTypes.Codes.Level, level);
			var invalid2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, "Invalid2", "Invalid attribute Level", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, "Invalid3", "No attribute Level", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var invalid4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, incorrectCodeType, "Invalid4", "Invalid code type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid4.PK, RefCusCodeListAttributeTypes.Codes.Level, level);
			Factory.Save();
		}
	}
}
