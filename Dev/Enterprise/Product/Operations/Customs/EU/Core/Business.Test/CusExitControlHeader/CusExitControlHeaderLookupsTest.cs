using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitControlHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsOffices()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT", "Italian Office", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE", "German Office", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EIN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE", "Irish Office - no role", yesterday, tomorrow);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var cusExitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
				var customsOffice = cusExitHeader.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT", "DE" }, customsOffice.Select(x => x.ZZD_Code));
			}
		}
	}
}
