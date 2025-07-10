using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class CusPermitHeaderLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestPermitFullTypesForCountriesUnderFrenchJurisdiction()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "ImportDocument");
			helper.CreateCusCodeListWithAttribute(Enterprise.Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2700", "2700 description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), RefCusCodeListAttributeTypes.Codes.Permit, "Y");

			Factory.Save();
			var permit = Factory.New<CusPermitHeader>();

			foreach (var country in Enterprise.Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				permit.CPH_RN_NKCountryCode = country;
				var fullTypeList = permit.Lookups.PermitFullTypes;
				fullTypeList.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "2700" }, fullTypeList.Select(x => x.ZZD_Code));
			}
		}

		public void TestPermitFullTypes()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800"), new TestSupportingDocumentCodeList("3200"));
			Factory.Save();
			var permit = Factory.New<CusPermitHeader>();
			var fullTypeList = permit.Lookups.PermitFullTypes;
			fullTypeList.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "2800", "3200" }, fullTypeList.Select(x => x.ZZD_Code));
		}
	}
}
