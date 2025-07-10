using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class SendAccessCodeViewModelLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DEO", "Germany Office",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FRO", "France Office",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ROL", "Other Role",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExport);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "TYP", "Other Code Type",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ATT", "Different Attribute",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.DistrictOffices, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
			Factory.Save();

			var guarantee = Factory.New<DE.Business.CusGuaranteeHeader>();
			var viewModel = new SendAccessCodeViewModel(guarantee);
			var offices = viewModel.Lookups.Offices;
			offices.Load();
			var descriptions = offices.Select(x => x.ZZD_Description);
			AssertContainsExactElementsInAnyOrder(new[] { "Germany Office" }, descriptions);
		}
	}
}
