using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusExitControlHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCustomsOffices()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", startDate, endDate);
			var codeList2 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE11", "DE11 DESC", startDate, endDate, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			var codeList3 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE22", "DE22 DESC", startDate, endDate, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			var codeList4 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE33", "DE33 DESC", startDate, endDate, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfExport);
			var codeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE44", "DE44 DESC", startDate, endDate);
			var codeList6 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT11", "IT11 DESC", startDate, endDate, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();

			var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(codeList1.PK);
			var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(codeList2.PK);
			var zzCodeList3 = Factory.Load<ZZRefCusCodeListCombined>(codeList3.PK);
			var zzCodeList4 = Factory.Load<ZZRefCusCodeListCombined>(codeList4.PK);
			var zzCodeList5 = Factory.Load<ZZRefCusCodeListCombined>(codeList5.PK);
			var zzCodeList6 = Factory.Load<ZZRefCusCodeListCombined>(codeList6.PK);
			var cusExitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var completeFilter = cusExitHeader.Lookups.CustomsOffices.CompleteFilter;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(zzCodeList1.MatchesFilter(completeFilter), Is.EqualTo(false), "zzCodeList1, invalid type");
				NUnit.Framework.Assert.That(zzCodeList2.MatchesFilter(completeFilter), Is.EqualTo(true), "zzCodeList2, valid type with 'EXT' Role attribute");
				NUnit.Framework.Assert.That(zzCodeList3.MatchesFilter(completeFilter), Is.EqualTo(true), "zzCodeList3, valid type with 'EIN' Role attribute");
				NUnit.Framework.Assert.That(zzCodeList4.MatchesFilter(completeFilter), Is.EqualTo(false), "zzCodeList4, valid type with invalid Role attribute");
				NUnit.Framework.Assert.That(zzCodeList5.MatchesFilter(completeFilter), Is.EqualTo(false), "zzCodeList5, valid type without Role attribute");
				NUnit.Framework.Assert.That(zzCodeList6.MatchesFilter(completeFilter), Is.EqualTo(false), "zzCodeList6, invalid dataGroupingCode");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsOffices_FilterBusinessObjectDefaults()
		{
			var cusExitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			NUnit.Framework.Assert.That(cusExitHeader.Lookups.CustomsOffices.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].Value, Is.EqualTo(Core.Constants.CountryCodes.Germany).Using(CustomComparers.TypeComparison));
		}
	}
}
