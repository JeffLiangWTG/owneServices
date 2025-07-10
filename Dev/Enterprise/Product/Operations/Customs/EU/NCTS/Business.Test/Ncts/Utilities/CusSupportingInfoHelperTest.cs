using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using RefCusCodeListAttributeTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeTypes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusSupportingInfoHelperTest : TestCaseWithFactory
	{
		public void TestMissesAttribute_NullParameter()
		{
			AssertEquals(true, CusSupportingInfoHelper.MissesAttribute(null, RefCusCodeListAttributeTypes.Reference));
		}

		public void TestMissesAttribute()
		{
			var (refCusCodeList1, _, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Factory, RefCusCodeListAttributeTypes.Reference);
			var refCusCodeListCombinedWithAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, refCusCodeList1.ZZD_Code, refCusCodeList1.ZZD_ZZZ_NKDataGrouping, refCusCodeList1.ZZD_ZZK_NKCodeType, ZDateTime.Today);
			var refCusCodeListCombinedWithoutAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, refCusCodeList3.ZZD_Code, refCusCodeList3.ZZD_ZZZ_NKDataGrouping, refCusCodeList3.ZZD_ZZK_NKCodeType, ZDateTime.Today);

			CombineAssertions(() =>
			{
				AssertEquals("Has attribute", false, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Reference));
				AssertEquals("Attribute missing", true, refCusCodeListCombinedWithoutAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Reference));
			});
		}

		public void TestMissesAttribute_Levels()
		{
			var attributeNames = new ZString[]
			{
				RefCusCodeListAttributeTypes.Complement,
				$"{RefCusCodeListAttributeTypes.Reference};{UniversalReferenceConstants.RefCusCodeListLevelTypes.House}",
				$"{RefCusCodeListAttributeTypes.ItemNumber};{UniversalReferenceConstants.RefCusCodeListLevelTypes.Item}"
			};

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, "CusCodeType");

			var refCusCodeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, "ABCD", "ABCD DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			foreach (var attributeName in attributeNames)
			{
				helper.CreateCusCodeListAttribute(refCusCodeList.PK, attributeName, "Y");
			}
			Factory.Save();

			var refCusCodeListCombinedWithAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, refCusCodeList.ZZD_Code, refCusCodeList.ZZD_ZZZ_NKDataGrouping, refCusCodeList.ZZD_ZZK_NKCodeType, ZDateTime.Today);

			CombineAssertions(() =>
			{
				AssertEquals("Complement, no level specified", false, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Complement));
				AssertEquals("Complement, House", false, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Complement, UniversalReferenceConstants.RefCusCodeListLevelTypes.House));
				AssertEquals("Complement, Item", false, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Complement, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item));

				AssertEquals("Reference, no level specified", true, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Reference));
				AssertEquals("Reference, House", false, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListLevelTypes.House));
				AssertEquals("Reference, Item", true, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item));

				AssertEquals("ItemNumber, no level specified", true, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.ItemNumber));
				AssertEquals("ItemNumber, House", true, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.ItemNumber, UniversalReferenceConstants.RefCusCodeListLevelTypes.House));
				AssertEquals("ItemNumber, Item", false, refCusCodeListCombinedWithAttribute.MissesAttribute(RefCusCodeListAttributeTypes.ItemNumber, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item));
			});
		}

		public void TestHasAttributeForMandatoryValidation_NullParameter()
		{
			AssertEquals(false, CusSupportingInfoHelper.HasAttributeForMandatoryValidation(null, RefCusCodeListAttributeTypes.Reference));
		}

		public void TestHasAttributeForMandatoryValidation()
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Factory, RefCusCodeListAttributeTypes.Reference);
			var refCusCodeListCombinedWithAttributeAndValueY = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, refCusCodeList1.ZZD_Code, refCusCodeList1.ZZD_ZZZ_NKDataGrouping, refCusCodeList1.ZZD_ZZK_NKCodeType, ZDateTime.Today);
			var refCusCodeListCombinedWithAttributeAndValueN = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, refCusCodeList2.ZZD_Code, refCusCodeList2.ZZD_ZZZ_NKDataGrouping, refCusCodeList2.ZZD_ZZK_NKCodeType, ZDateTime.Today);
			var refCusCodeListCombinedWithoutAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, refCusCodeList3.ZZD_Code, refCusCodeList3.ZZD_ZZZ_NKDataGrouping, refCusCodeList3.ZZD_ZZK_NKCodeType, ZDateTime.Today);

			CombineAssertions(() =>
			{
				AssertEquals("Has attribute with value 'Y'", true, refCusCodeListCombinedWithAttributeAndValueY.HasAttributeForMandatoryValidation(RefCusCodeListAttributeTypes.Reference));
				AssertEquals("Has attribute with value 'N'", false, refCusCodeListCombinedWithAttributeAndValueN.HasAttributeForMandatoryValidation(RefCusCodeListAttributeTypes.Reference));
				AssertEquals("Attribute missing", false, refCusCodeListCombinedWithoutAttribute.HasAttributeForMandatoryValidation(RefCusCodeListAttributeTypes.Reference));
			});
		}

		public void TestHasAttributeForMandatoryValidation_Levels()
		{
			var attributes = new (ZString, ZString)[]
			{
				(RefCusCodeListAttributeTypes.Reference, "Y"),
				($"{RefCusCodeListAttributeTypes.Reference};{UniversalReferenceConstants.RefCusCodeListLevelTypes.House}", "N"),
				($"{RefCusCodeListAttributeTypes.Complement};{UniversalReferenceConstants.RefCusCodeListLevelTypes.Item}", "Y")
			};

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, "CusCodeType");

			var refCusCodeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, "ABCD", "ABCD DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			foreach (var (attributeName, attributeValue) in attributes)
			{
				helper.CreateCusCodeListAttribute(refCusCodeList.PK, attributeName, attributeValue);
			}
			Factory.Save();

			var refCusCodeListCombinedWithAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, refCusCodeList.ZZD_Code, refCusCodeList.ZZD_ZZZ_NKDataGrouping, refCusCodeList.ZZD_ZZK_NKCodeType, ZDateTime.Today);

			CombineAssertions(() =>
			{
				AssertEquals("Specific attribute has preference", false, refCusCodeListCombinedWithAttribute.HasAttributeForMandatoryValidation(RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListLevelTypes.House));
				AssertEquals("Attribute with no level", true, refCusCodeListCombinedWithAttribute.HasAttributeForMandatoryValidation(RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item));
				AssertEquals("Attribute missing for level", false, refCusCodeListCombinedWithAttribute.HasAttributeForMandatoryValidation(RefCusCodeListAttributeTypes.Complement, UniversalReferenceConstants.RefCusCodeListLevelTypes.House));
			});
		}

		public void TestGetLevelAttributeValue()
		{
			CombineAssertions(() =>
			{
				AssertLevelAttributeValue(Factory.New<NctsHeader>(), UniversalReferenceConstants.RefCusCodeListLevelTypes.Header);
				AssertLevelAttributeValue(Factory.New<NctsArrivalMovementHeader>(), UniversalReferenceConstants.RefCusCodeListLevelTypes.Header);
				AssertLevelAttributeValue(Factory.New<NctsBill>(), UniversalReferenceConstants.RefCusCodeListLevelTypes.House);
				AssertLevelAttributeValue(Factory.New<NctsArrivalCargoDesc>(), UniversalReferenceConstants.RefCusCodeListLevelTypes.Item);
				AssertLevelAttributeValue(Factory.New<NctsDepartureCargoDesc>(), UniversalReferenceConstants.RefCusCodeListLevelTypes.Item);
			});

			void AssertLevelAttributeValue(BusinessObject parent, ZString expectedLevelAttributeValue)
			{
				var collection = new CusSupportingInfoCollection<CusSupportingInfo>(parent, ZString.Empty);
				var cusSupportingInfo = collection.AddNew();
				AssertEquals($"{parent.GetType().Name}", expectedLevelAttributeValue, CusSupportingInfoHelper.GetLevelAttributeValue(cusSupportingInfo));
			}
		}

		public void TestGetTypeCodeList()
		{
			CombineAssertions(() =>
			{
				var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Factory);
				var (refCusCodeList4, refCusCodeList5, refCusCodeList6) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Factory);
				var list = CusSupportingInfoHelper.GetTypeCodeList(Factory,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, false,
					UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Core.Constants.CountryCodes.Latvia);
				list.Load();
				AssertContainsExactElementsInAnyOrder("Not include parent data grouping", new[] { refCusCodeList1.PK, refCusCodeList2.PK, refCusCodeList3.PK }, list.Select(x => x.PK));

				list = CusSupportingInfoHelper.GetTypeCodeList(Factory,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, true,
					UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Core.Constants.CountryCodes.Latvia);
				list.Load();
				AssertContainsExactElementsInAnyOrder("Include parent data grouping", new[] { refCusCodeList1.PK, refCusCodeList2.PK, refCusCodeList3.PK, refCusCodeList4.PK, refCusCodeList5.PK, refCusCodeList6.PK }, list.Select(x => x.PK));

				list = CusSupportingInfoHelper.GetTypeCodeList(Factory,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, false,
					UniversalReferenceConstants.RefCusCodeListLevelTypes.Header, Core.Constants.CountryCodes.Latvia,
					applyLevelAttributeToChild: false);
				list.Load();
				AssertContainsExactElementsInAnyOrder("levelAttributeValue: Header, applyLevelAttributeToChild: false", new[] { refCusCodeList1.PK, refCusCodeList2.PK, refCusCodeList3.PK }, list.Select(x => x.PK));

				list = CusSupportingInfoHelper.GetTypeCodeList(Factory,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, false,
					UniversalReferenceConstants.RefCusCodeListLevelTypes.Header, Core.Constants.CountryCodes.Latvia);
				list.Load();
				AssertContainsExactElementsInAnyOrder("levelAttributeValue: Header, applyLevelAttributeToChild: true", [], list.Select(x => x.PK));
			});
		}

		public void TestGetTypeCodeList_LevelAttributeDates()
		{
			var dataGrouping = Core.Constants.CountryCodes.Latvia;
			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping, dataGrouping, eun);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CusCodeType");

			var refCusCodeList1 = helper.CreateCusCodeList(dataGrouping, codeType, "VALUE1", "VALUE1 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10));
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, ZDateTime.Today, ZDateTime.Today.AddDays(10));
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var refCusCodeList2 = helper.CreateCusCodeList(dataGrouping, codeType, "VALUE2", "VALUE2 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10));
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header, ZDateTime.Today, ZDateTime.Today.AddDays(10));
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = CusSupportingInfoHelper.GetTypeCodeList(Factory,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, false,
					UniversalReferenceConstants.RefCusCodeListLevelTypes.Header, Core.Constants.CountryCodes.Latvia);
				list.Load();
				AssertContainsExactElementsInAnyOrder("levelAttributeValue: Header", new[] { refCusCodeList2.PK }, list.Select(x => x.PK));

				list = CusSupportingInfoHelper.GetTypeCodeList(Factory,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, false,
					UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Core.Constants.CountryCodes.Latvia);
				list.Load();
				AssertContainsExactElementsInAnyOrder("levelAttributeValue: House", new[] { refCusCodeList1.PK }, list.Select(x => x.PK));

				list = CusSupportingInfoHelper.GetTypeCodeList(Factory,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, false,
					UniversalReferenceConstants.RefCusCodeListLevelTypes.Item, Core.Constants.CountryCodes.Latvia);
				list.Load();
				AssertContainsExactElementsInAnyOrder("levelAttributeValue: Item", new[] { refCusCodeList1.PK, refCusCodeList2.PK }, list.Select(x => x.PK));
			});
		}

		public void TestGetTypeCodeListUniqueFromChildFirstThenParent()
		{
			var levelAttribute = (Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.House);
			_ = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTesting(Factory,
				dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				codes: new ZString[] { "CODE1", "CODE2", "CODE3", "CODE4" },
				levelAttribute).ToArray();
			_ = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTesting(Factory,
				dataGrouping: Core.Constants.CountryCodes.Italy,
				codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				codes: new ZString[] { "CODE1", "CODE2", "IT_CODE3" }).ToArray();
			_ = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTesting(Factory,
				dataGrouping: Core.Constants.CountryCodes.Italy,
				codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				codes: new ZString[] { "IT_CODE4" },
				levelAttribute).ToArray();
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = CusSupportingInfoHelper.GetTypeCodeListUniqueFromChildFirstThenParent(Factory,
					dataGrouping: Core.Constants.CountryCodes.Italy,
					codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
					levelAttributeValue: UniversalReferenceConstants.RefCusCodeListLevelTypes.House, applyLevelAttributeToChild: false);
				list.Load();

				var expectedCodes = new ZString[] { "CODE1", "CODE2", "IT_CODE3", "CODE3", "CODE4", "IT_CODE4" };
				AssertContainsExactElementsInAnyOrder("Includes Child Codes and Codes from parent which are not present in child and without attribute filter on child level", expectedCodes, list.Select(c => c.ZZD_Code));

				list = CusSupportingInfoHelper.GetTypeCodeListUniqueFromChildFirstThenParent(Factory,
					dataGrouping: Core.Constants.CountryCodes.Italy,
					codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
					levelAttributeValue: UniversalReferenceConstants.RefCusCodeListLevelTypes.House, applyLevelAttributeToChild: true);
				list.Load();

				expectedCodes = new ZString[] { "IT_CODE4", "CODE2", "CODE1", "CODE3", "CODE4" };
				AssertContainsExactElementsInAnyOrder("Includes Child Codes and Codes from parent which are not present in child and with Attribute Filter on child level.", expectedCodes, list.Select(c => c.ZZD_Code));
			});
		}
	}
}
