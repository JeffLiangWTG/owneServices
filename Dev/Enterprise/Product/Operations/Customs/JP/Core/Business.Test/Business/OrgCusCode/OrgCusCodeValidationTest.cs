using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(OrgCusCodeValidation))]
	sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo_CCP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeType("JPBLC", "JPBLC", Core.Constants.CountryCodes.Japan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPBLC", "2HDN8", startDate, endDate);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			var invalidCodeMessage = "The entered Bonded Location Code does not exist. To view the complete list, go to Maintain > Customs > Global Codes, then set the Country/Region or Grouping to JP and List Type to JPBLC.";

			customsCode.OK_CustomsRegNo = "2HDN8";
			AssertNoWarningContaining(targetInfo, invalidCodeMessage);
			AssertNoWarning(targetInfo, invalidCodeMessage);

			customsCode.OK_CustomsRegNo = "2HDN9";
			AssertHasWarning(targetInfo, invalidCodeMessage);
		}

		public void TestCheckOK_CustomsRegNo_JAS()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.JAS;
			var errorMessageJAS = "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z].";

			customsCode.OK_CustomsRegNo = "12345678901";
			AssertHasErrorContaining(targetInfo, errorMessageJAS);

			customsCode.OK_CustomsRegNo = "123456789012";
			AssertNoErrorContaining(targetInfo, errorMessageJAS);

			customsCode.OK_CustomsRegNo = "12345678901b";
			AssertHasErrorContaining(targetInfo, errorMessageJAS);

			customsCode.OK_CustomsRegNo = "12345678901L";
			AssertNoErrorContaining(targetInfo, errorMessageJAS);
		}

		public void TestCheckOK_CustomsRegNo_LPC()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.LPC;
			var errorMessageLPC = "Please enter 13 digits, or 17 digits.";

			customsCode.OK_CustomsRegNo = "123456789012";
			AssertHasErrorContaining(targetInfo, errorMessageLPC);

			customsCode.OK_CustomsRegNo = "123456789012L";
			AssertHasErrorContaining(targetInfo, errorMessageLPC);

			customsCode.OK_CustomsRegNo = "1234567890123";
			AssertNoErrorContaining(targetInfo, errorMessageLPC);

			customsCode.OK_CustomsRegNo = "12345678901234567";
			AssertNoErrorContaining(targetInfo, errorMessageLPC);
		}

		public void TestCheckOK_CustomsRegNo_CIE()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.CIE;
			var errorMessageCIE = "Please enter C0000 followed by 8 or 12 digits, or 1 followed by 7 or 11 digits.";

			customsCode.OK_CustomsRegNo = "1234567";
			AssertHasError(targetInfo, errorMessageCIE);

			customsCode.OK_CustomsRegNo = "12345678";
			AssertNoError(targetInfo, errorMessageCIE);

			customsCode.OK_CustomsRegNo = "123456789012";
			AssertNoError(targetInfo, errorMessageCIE);

			customsCode.OK_CustomsRegNo = "1234567890123";
			AssertHasError(targetInfo, errorMessageCIE);

			customsCode.OK_CustomsRegNo = "C00001234567";
			AssertHasError(targetInfo, errorMessageCIE);

			customsCode.OK_CustomsRegNo = "C000012345678";
			AssertNoError(targetInfo, errorMessageCIE);

			customsCode.OK_CustomsRegNo = "C0000123456789";
			AssertHasError(targetInfo, errorMessageCIE);

			customsCode.OK_CustomsRegNo = "C0000123456789012";
			AssertNoError(targetInfo, errorMessageCIE);
		}

		public void TestCheckOK_CustomsRegNo_FSB()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.FSB;
			var errorMessageFSB = "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z]. The first character must be F.";

			customsCode.OK_CustomsRegNo = "12345678901";
			AssertHasErrorContaining(targetInfo, errorMessageFSB);

			customsCode.OK_CustomsRegNo = "123456789012";
			AssertHasErrorContaining(targetInfo, errorMessageFSB);

			customsCode.OK_CustomsRegNo = "F23456789012";
			AssertNoErrorContaining(targetInfo, errorMessageFSB);
		}

		public void TestCheckOK_CustomsRegNo_NUC()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
			var expectedErrorMessage = "Please enter exactly 5 characters consisting solely of digits and capital letters [A-Z].";

			customsCode.OK_CustomsRegNo = "1234";
			AssertHasErrorContaining(targetInfo, expectedErrorMessage);

			customsCode.OK_CustomsRegNo = "123ab";
			AssertHasErrorContaining(targetInfo, expectedErrorMessage);

			customsCode.OK_CustomsRegNo = "123AB";
			AssertNoErrorContaining(targetInfo, expectedErrorMessage);
		}

		public void TestCheckOK_CustomsRegNo_CCC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCarrierCode("20ZE", "WG TRUCKING", Core.Constants.CountryCodes.Japan);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = CodeTypes.CarrierCode;
			var expectedWarningMessage = "The entered value is not a valid Japan Global Carrier Code. To add one, go to Maintain > Customs > Global Carriers.";

			customsCode.OK_CustomsRegNo = "1234";
			AssertHasWarning(targetInfo, expectedWarningMessage);

			customsCode.OK_CustomsRegNo = "20ZE";
			AssertNoWarning(targetInfo, expectedWarningMessage);
		}

		public void TestCheckOK_CustomsRegNo_AAL()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;
			var targetInfo = customsCode.OK_CustomsRegNoInfo;

			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.AAL;
			var expectedErrorMessage = "Please enter no longer than 3 characters consisting solely of digits and capital letters [A-Z].";

			customsCode.OK_CustomsRegNo = "AB12";
			AssertHasError(targetInfo, expectedErrorMessage);

			customsCode.OK_CustomsRegNo = "12a";
			AssertHasError(targetInfo, expectedErrorMessage);

			customsCode.OK_CustomsRegNo = "1AB";
			AssertNoError(targetInfo, expectedErrorMessage);

			customsCode.OK_CustomsRegNo = "2A";
			AssertNoError(targetInfo, expectedErrorMessage);

			customsCode.OK_CustomsRegNo = "A";
			AssertNoError(targetInfo, expectedErrorMessage);
		}

		public void TestCheckOK_OA_PremisesAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cpa = org.CustomsCodes.AddNew();
			cpa.OK_RN_NKCodeCountry = "JP";
			var targetInfo = cpa.OK_OA_PremisesAddressInfo;

			var codes = new[]
			{
				OrgCusCode.JapanCodeTypes.JAS,
				OrgCusCode.JapanCodeTypes.LPC,
				OrgCusCode.JapanCodeTypes.CIE,
				OrgCusCode.JapanCodeTypes.FSB,
				OrgCusCode.CodeTypes.ControlledPremisesID
			};

			CombineAssertions(() =>
			{
				foreach (var code in codes)
				{
					cpa.OK_CodeType = code;
					AssertHasErrorContaining(targetInfo, $"An address is required for code type '{code}'.");
				}

				cpa.OK_OA_PremisesAddress = org.Addresses.FirstOrDefault().PK;

				foreach (var code in codes)
				{
					cpa.OK_CodeType = code;
					AssertNoErrorContaining(targetInfo, $"An address is required for code type '{code}'.");
				}
			});
		}
	}
}
