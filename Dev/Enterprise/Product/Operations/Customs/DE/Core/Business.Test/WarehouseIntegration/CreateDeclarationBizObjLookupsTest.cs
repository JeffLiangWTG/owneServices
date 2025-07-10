using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CreateDeclarationBizObjLookupsTest : TestCaseWithFactory
	{
		public void TestCustomsOffices()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR00123", "FRENCH OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			Factory.Save();

			var customsOfficeList = lookups.CustomsOffices;
			customsOfficeList.Load();

			AssertContainsExactElementsInAnyOrder("Only DE Customs Office expected", new[] { "DE004323" }, customsOfficeList.Select(x => x.ZZD_Code));
		}

		public void TestCpcList()
		{
			var list = lookups.CpcList;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "40", "42" }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.CpcList);
			});
		}

		public void TestDeclarationTypeList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "EZA", "AZ", "VZA" }, lookups.DeclarationTypeList.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var bizO = new CreateDeclarationBizObj(createFromWarehouseOrder: false);
			lookups = bizO.Lookups;
		}

		CreateDeclarationBizObjLookups lookups;
	}
}
