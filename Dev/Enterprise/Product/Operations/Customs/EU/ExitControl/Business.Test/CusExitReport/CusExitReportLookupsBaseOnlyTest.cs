using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class CusExitReportLookupsBaseOnlyTest : CusExitReportLookupsAbstractTest
{
	public void TestTypeList()
	{
		var list = lookups.TypeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("List", new[] { "PRE", "ALT", "EXT" }, list.GetAllCodes());
			AssertSame("Cached", list, lookups.TypeList);
		});
	}

	public void TestDeclarantTypeList()
	{
		var list = lookups.DeclarantTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, list.Count);
			AssertEquals("DIR", RepresentationTypeList.Descriptions._2Direct, list.GetDescriptionFromCode(RepresentationTypeList.Codes._2Direct));
			AssertEquals("IND", RepresentationTypeList.Descriptions._3Indirect, list.GetDescriptionFromCode(RepresentationTypeList.Codes._3Indirect));
			AssertSame("Cached", list, lookups.DeclarantTypeList);
		});
	}

	public void TestDiscrepancyTypeList()
	{
		var list = lookups.DiscrepancyTypeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("List", new[] { "DIS", "STD" }, list.GetAllCodes());
			AssertSame("Cached", Factory.GetCachedValue<ExitReportDiscrepancyTypeList>(), lookups.DiscrepancyTypeList);
		});
	}

	public void TestTransportModeList()
	{
		var list = lookups.TransportModeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInExactOrder("List", new[] { "AIR", "FIX", "IWT", "OWN", "MAI", "RAI", "ROA", "SEA" }, list.GetAllCodes());
			AssertSame("Cached", list, lookups.TransportModeList);
		});
	}

	public void TestOfficeOfExitList()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LV1", "LV1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LV2", "LV2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "INV1", "Invalid country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "INV2", "Invalid codeType", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "INV3", "Invalid role", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "INV4", "No role", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "INV5", "Invalid startDate", ZDateTime.Today.AddDays(1), ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "INV6", "Invalid endDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		Factory.Save();

		var list = lookups.OfficeOfExitList;
		list.Load();
		CombineAssertions(() =>
		{
			AssertType<CustomsOfficeCodeCollection>("Type", list);
			AssertContainsExactElementsInAnyOrder("List", new[] { "LV1", "LV2" }, list.Select(x => x.ZZD_Code));

			report.CER_CXH_Header = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("List", new[] { "LV1", "LV2" }, list.Select(x => x.ZZD_Code));
		});
	}

	public void TestStatusList()
	{
		var list = lookups.StatusList;
		CombineAssertions(() =>
		{
			AssertCollectionContains("REL", AESEntryStatusList.Codes.ReleasedForExport, list.GetAllCodes());
			AssertCollectionContains("EXR", AESEntryStatusList.Codes.ReleasedForExit, list.GetAllCodes());
			AssertCollectionContains("PRD", AESEntryStatusList.Codes.PresentedAtExitAfterReceivingDEC, list.GetAllCodes());
			AssertCollectionContains("COX", AESEntryStatusList.Codes.ControlledForExport, list.GetAllCodes());
			AssertCollectionContains("REJ", AESEntryStatusList.Codes.Rejected, list.GetAllCodes());
			AssertCollectionNotContains("AAA", "AAA", list.GetAllCodes());
			AssertEquals(typeof(AESEntryStatusList), list.GetType());
			AssertSame("StatusList Cached", list, lookups.StatusList);
		});
	}

	public void TestMessageStatusList()
	{
		var list = lookups.MessageStatusList;

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("MessageStatusList", new[] { "ACC", "ACK", "ERR", "FAL", "INV", "SNT" }, list.GetAllCodes());
			AssertSame("MessageStatusList Cached", list, lookups.MessageStatusList);
			AssertEquals(typeof(LogicalStatusList), list.GetType());
		});
	}

	public void TestTransportNationalities()
	{
		var transportNationalities = lookups.TransportNationalities;
		AssertNotNull("Result is not null", transportNationalities);
		AssertEquals("Of type RefCountry", typeof(RefCountryCollection), transportNationalities.GetType());
		var country1 = Factory.New<RefCountry>();
		country1.RN_Code = "A1";
		country1.RN_Desc = "D1";
		var country2 = Factory.New<RefCountry>();
		country2.RN_Code = "B2";
		country2.RN_Desc = "C2";
		_ = transportNationalities.Count;
		var comparer = transportNationalities.SortComparer;
		AssertEquals("Should compare using RN_Desc 'D1' to 'C2'", 1, comparer.Compare(country1, country2));
		AssertEquals("Should compare using RN_Desc 'C2' to 'D1'", -1, comparer.Compare(country2, country1));
		country1.RN_Desc = "C2";
		AssertEquals("Should compare using RN_Desc 'C2' to 'C2'", 0, comparer.Compare(country2, country1));
	}
}
