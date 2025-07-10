using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class EMCSARCValidationHelperTest : TestCaseWithFactory
	{
		public void TestARCFormat()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ARC does not contain both letters and numbers", invalidARCError, CheckARCFormatAndGetFirstError("220012340123456789012"));
				AssertEquals("ARC does not contain both letters and numbers", invalidARCError, CheckARCFormatAndGetFirstError("DEBLAHBLAHDEBLAHBLAHA"));
				AssertEquals("ARC contains invalid letters", invalidARCError, CheckARCFormatAndGetFirstError("15DE&2193075022340123"));
				AssertEquals("ARC contains lower case letters", invalidARCError, CheckARCFormatAndGetFirstError("15dE12345678901234561"));
				AssertEquals("Digits 5-20 must be all numeric", invalidARCError, CheckARCFormatAndGetFirstError("11DE123456789012345X9"));
				AssertEquals("digit 20 must be either ‘P’ or ‘S’", invalidARCError, CheckARCFormatAndGetFirstError("33DE123456789012345X4"));
				AssertEquals("21DE123456789012345S7 - invalid reference number", invalidARCError, CheckARCFormatAndGetFirstError("21DE123456789012345S7"));
				AssertEquals("24DE12345678901234561 - invalid reference number", invalidARCError, CheckARCFormatAndGetFirstError("24DE12345678901234561"));

				AssertEquals("33LV123456789012345P6 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("33LV123456789012345P6"));
				AssertEquals("33LV123456789012345S2 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("33LV123456789012345S2"));
				AssertEquals("11LV12345678901234519 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("11LV12345678901234519"));
				AssertEquals("22LV123456789012345S0 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("22LV123456789012345S0"));
				AssertEquals("11SB1234567XQ90123419 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("11SB1234567XQ90123419"));
				AssertEquals("22DE123456789012345S0 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("22DE123456789012345S9"));
				AssertEquals("22DE12345678901234560 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("22DE12345678901234568"));
				AssertEquals("23DE123456789012345S0 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("23DE123456789012345S0"));
				AssertEquals("23DE12345678901234561 - meet all conditions", ZString.Empty, CheckARCFormatAndGetFirstError("23DE12345678901234560"));
			});
		}

		public void TestARCFormat_CheckDigit()
		{
			ZString invalidCheckDigitError = "ARC does not have a valid check (last) digit. The check digit should be {0}";
			CombineAssertions(() =>
			{
				AssertEquals("ARC has the wrong check digit", invalidARCError, CheckARCFormatAndGetFirstError("13DE1234567890123456X"));

				AssertEquals("Expected a message error as ARC has the wrong check digit", string.Format(invalidCheckDigitError, "0"), CheckARCFormatAndGetFirstError("22NB12340123456789012"));
				AssertEquals("Expected a message error as ARC has the wrong check digit", string.Format(invalidCheckDigitError, "8"), CheckARCFormatAndGetFirstError("22SB12340123456789012"));
			});
		}

		public void TestARCFormat_CountryCode()
		{
			ZString invalidCountryError = "Please enter a valid country/region code.";
			CombineAssertions(() =>
			{
				AssertEquals("ARC does not have a valid country/region code", invalidCountryError, CheckARCFormatAndGetFirstError("22SD12340123456789012"));
				AssertEquals("ARC has a valid country/region code", ZString.Empty, CheckARCFormatAndGetFirstError("22NB12340123456789010"));
				AssertEquals("ARC has a valid country/region code", ZString.Empty, CheckARCFormatAndGetFirstError("22SB12340123456789018"));
			});
		}

		ZString CheckARCFormatAndGetFirstError(ZString arcNumber)
		{
			return EMCSARCValidationHelper.ValidateACRNumberIsValid(arcNumber, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC010, "Description");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia,
				UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC010, "NB", "123456",
				ZDateTime.Today, ZDateTime.Today);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia,
				UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC010, "SB", "123456",
				ZDateTime.Today, ZDateTime.Today);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia,
				UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC010, "LV", "123456",
				ZDateTime.Today, ZDateTime.Today);
			Factory.Save();
		}

		const string invalidARCError = @"Structure does not correspond to an EMCS Administrative Reference Code (ARC). Please enter the ARC in the following format with only numbers and uppercase letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• sixteen alphanumeric characters for unique identification and
• one number check digit";
	}
}
