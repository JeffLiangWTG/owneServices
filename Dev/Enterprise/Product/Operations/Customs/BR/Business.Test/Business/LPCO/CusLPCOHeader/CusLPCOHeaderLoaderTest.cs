using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusLPCOHeader.Loader))]
	class CusLPCOHeaderLoaderTest : LoaderTestCase
	{
		public void TestGetPermitByPermitNumber()
		{
			var permitLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			permitLPCO.CPH_Number = "E2100000000";
			permitLPCO.CPH_Type = PermitTypeList.Codes.LPC;
			permitLPCO.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			permitLPCO.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Operational;

			var permitDifferentType = Factory.NewWithValidTestData<CusLPCOHeader>();
			permitDifferentType.CPH_Number = "E2100000001";
			permitDifferentType.CPH_Type = "TES";
			permitDifferentType.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			permitDifferentType.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;

			var permitDifferentApplicationCode = Factory.NewWithValidTestData<CusLPCOHeader>();
			permitDifferentApplicationCode.CPH_Number = "E2100000002";
			permitDifferentApplicationCode.CPH_Type = PermitTypeList.Codes.LPC;
			permitDifferentApplicationCode.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			permitDifferentApplicationCode.CPH_ApplicationCode = "TES";

			var permitDifferentCountryCode = Factory.NewWithValidTestData<CusLPCOHeader>();
			permitDifferentCountryCode.CPH_Number = "E2100000003";
			permitDifferentCountryCode.CPH_Type = PermitTypeList.Codes.LPC;
			permitDifferentCountryCode.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			permitDifferentCountryCode.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Operational;

			var loader = GetNewLoaderToTest() as CusLPCOHeader.Loader;
			CombineAssertions(() =>
			{
				AssertEquals("Loader should be equal to permitLPCO", permitLPCO, loader.GetLPCOByNumber("E2100000000"));
				AssertNull("Should be NULL when Empty", loader.GetLPCOByNumber(string.Empty));
				AssertNull("Loader should be NULL when WRONG_NUMBER", loader.GetLPCOByNumber("WRONG_NUMBER"));

				AssertNull("Should be NULL when Type is not LPC", loader.GetLPCOByNumber("E2100000001"));
				AssertNull("Should be NULL when ApplicationCode is not PER", loader.GetLPCOByNumber("E2100000002"));
				AssertNull("Should be NULL when CountryCode is not BR", loader.GetLPCOByNumber("E2100000003"));
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusLPCOHeader.Loader(Factory);
	}
}
