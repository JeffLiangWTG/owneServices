using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class CusExitReportLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			(var report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			var lookups = report.Lookups;
			var list = lookups.TypeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("List", new[] { "ANT", "EXT", "PRE", "TRA" }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.TypeList);
			});
		}

		public void TestOfficeOfExportList()
		{
			var dateInFuture = ZDate.Today.AddDays(4);
			var dateInPast = ZDate.Today.AddDays(-4);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Desc.");
			var germanCustomsOfficeCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002702", "Desc.", dateInPast, dateInFuture);
			var germanCustomsOfficeCodeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002703", "Desc.", dateInPast, dateInFuture);
			var italyCustomsOfficeCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT002704", "Desc.", dateInPast, dateInFuture);
			var wrongTypeCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "blank", "Desc.", dateInPast, dateInFuture);
			var noAttributeCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002707", "Desc.", dateInPast, dateInPast);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeListAttribute(germanCustomsOfficeCodeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateNewOrGetExistingCusCodeListAttribute(germanCustomsOfficeCodeList2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateNewOrGetExistingCusCodeListAttribute(italyCustomsOfficeCodeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateNewOrGetExistingCusCodeListAttribute(wrongTypeCodeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();

			(var report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			var lookups = report.Lookups;
			var list = lookups.OfficeOfExportList;
			list.Load();

			AssertContainsExactElementsInAnyOrder("List", new[] { "DE002702", "DE002703" }, list.Select(x => x.ZZD_Code));
		}

		public void TestOfficeOfExitList_EXTOfficesReturned()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE1", "DE1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE2", "DE2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			Factory.Save();

			var (report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				var lookupsOfficeOfExitList = report.Lookups.OfficeOfExitList;
				AssertType<CustomsOfficeCodeCollection>("Type", lookupsOfficeOfExitList);
				lookupsOfficeOfExitList.Load();
				AssertContainsExactElementsInAnyOrder("List", new[] { "DE1", "DE2" }, lookupsOfficeOfExitList.Select(x => x.ZZD_Code));
				report.Consignment.AdditionalInfos.AddNew().CSI_Code = "X1002";
				lookupsOfficeOfExitList = report.Lookups.OfficeOfExitList;
				lookupsOfficeOfExitList.Load();
				AssertContainsExactElementsInAnyOrder("List when X1002", new[] { "DE1" }, lookupsOfficeOfExitList.Select(x => x.ZZD_Code));
			});
		}

		public void TestOfficeOfExitList_DEPOfficesReturned()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE1", "DE1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE2", "DE2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE3", "DE3 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);

			Factory.Save();

			var (report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				var lookupsOfficeOfExitList = report.Lookups.OfficeOfExitList;
				AssertType<CustomsOfficeCodeCollection>("Type", lookupsOfficeOfExitList);
				lookupsOfficeOfExitList.Load();
				AssertContainsExactElementsInAnyOrder("List", new[] { "DE1", "DE2" }, lookupsOfficeOfExitList.Select(x => x.ZZD_Code));
				report.Consignment.AdditionalInfos.AddNew().CSI_Code = "X1004";
				lookupsOfficeOfExitList = report.Lookups.OfficeOfExitList;
				lookupsOfficeOfExitList.Load();
				AssertContainsExactElementsInAnyOrder("List when X1004", new[] { "DE3" }, lookupsOfficeOfExitList.Select(x => x.ZZD_Code));
			});
		}

		public void TestTransportNationalities()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT);
			Factory.Save();

			var (report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			var lookups = report.Lookups;

			var countries = lookups.TransportNationalities;
			countries.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countries.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}
	}
}
