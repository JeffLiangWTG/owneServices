using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ManifestHeadersWrapperValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateWR_ManifestToDelete()
		{
			ManifestHeadersWrapperTest.SetUpManifestCountries(Factory, "FJ", "ZA", "SG");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ERXXX";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var wrapper = new ManifestHeadersWrapper(consol);
			var manifestZA = wrapper.CreateCountry(Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var manifestFJ = wrapper.CreateCountry(Core.Constants.CountryCodes.Fiji, "ASY");

			wrapper.WR_KeywordCombination = ZString.Empty;
			AssertNoNotifications(wrapper.WR_KeywordCombinationInfo);

			wrapper.WR_KeywordCombination = "XXX";
			AssertHasMessageErrorContaining(wrapper.WR_KeywordCombinationInfo, ListValidation.InvalidCodeMessageError);

			wrapper.WR_KeywordCombination = "ZA|HAB";
			AssertNoNotifications(wrapper.WR_KeywordCombinationInfo);
		}

		public void TestValidateWR_CountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var erCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, "15.3.29.001");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ERXXX";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();

			var wrapper = new ManifestHeadersWrapper(consol);
			Assert(wrapper.CountryCodes.ContainsCode(Core.Constants.CountryCodes.Eritrea));
			ValidationTestHelper.AssertInvalidCodeMessageError(wrapper.WR_CountryCodeInfo, "XX", Core.Constants.CountryCodes.Eritrea);
		}

		public void TestValidateWR_ManifestType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var erCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, "15.3.29.001");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ERXXX";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();

			var wrapper = new ManifestHeadersWrapper(consol);
			wrapper.WR_CountryCode = Core.Constants.CountryCodes.Eritrea;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(wrapper.WR_ManifestTypeInfo, "XX", "ASY");
		}
	}
}
