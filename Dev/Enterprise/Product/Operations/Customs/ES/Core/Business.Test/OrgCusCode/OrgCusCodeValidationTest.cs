using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateControlledPremisesCodeLength()
		{
			CombineAssertions(() =>
			{
				customsCode.OK_CustomsRegNo = ZString.Empty;
				AssertHasErrors("Error if empty, already implemeted by parent", customsCode.OK_CustomsRegNoInfo);
				customsCode.OK_CustomsRegNo = "T";
				AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "A Customs Controlled Premises Code must be 12 characters long");
				customsCode.OK_CustomsRegNo = "123456789AB";
				AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "A Customs Controlled Premises Code must be 12 characters long");
				customsCode.OK_CustomsRegNo = "123456789ABCD";
				AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "A Customs Controlled Premises Code must be 12 characters long");
				customsCode.OK_CustomsRegNo = "123456789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "A Customs Controlled Premises Code must be 12 characters long");
			});
		}

		public void TestValidateControlledPremisesCodeFirstSecondChars()
		{
			CombineAssertions(() =>
			{
				customsCode.OK_CustomsRegNo = "123456789ABC";
				AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "A Customs Controlled Premises Code in ES must start with ES");

				customsCode.OK_CustomsRegNo = "ES3456789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "A Customs Controlled Premises Code in ES must start with ES");
			});
		}

		public void TestValidateControlledPremisesCodeThirdChar()
		{
			CombineAssertions(() =>
			{
				customsCode.OK_CustomsRegNo = "ES3456789ABC";
				AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "The third character of a Customs Controlled Premises Code must be 'X', 'I' or 'V'");

				customsCode.OK_CustomsRegNo = "ESX456789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The third character of a Customs Controlled Premises Code must be 'X', 'I' or 'V'");

				customsCode.OK_CustomsRegNo = "ESI456789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The third character of a Customs Controlled Premises Code must be 'X', 'I' or 'V'");

				customsCode.OK_CustomsRegNo = "ESV456789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The third character of a Customs Controlled Premises Code must be 'X', 'I' or 'V'");
			});
		}

		public void TestValidateControlledPremisesCodeFourthChar()
		{
			CombineAssertions(() =>
			{
				customsCode.OK_CustomsRegNo = "ESX456789ABC";
				AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "The fourth character of a Customs Controlled Premises Code must be 'A', 'B', 'C', 'D' or 'E'");

				customsCode.OK_CustomsRegNo = "ESXA56789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The fourth character of a Customs Controlled Premises Code must be 'A', 'B', 'C', 'D' or 'E'");

				customsCode.OK_CustomsRegNo = "ESXB56789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The fourth character of a Customs Controlled Premises Code must be 'A', 'B', 'C', 'D' or 'E'");

				customsCode.OK_CustomsRegNo = "ESXC56789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The fourth character of a Customs Controlled Premises Code must be 'A', 'B', 'C', 'D' or 'E'");

				customsCode.OK_CustomsRegNo = "ESXD56789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The fourth character of a Customs Controlled Premises Code must be 'A', 'B', 'C', 'D' or 'E'");

				customsCode.OK_CustomsRegNo = "ESXE56789ABC";
				AssertNoWarningContaining(customsCode.OK_CustomsRegNoInfo, "The fourth character of a Customs Controlled Premises Code must be 'A', 'B', 'C', 'D' or 'E'");
			});
		}

		public void TestValidateControlledPremisesCodeAlphanumeric()
		{
			CombineAssertions(() =>
			{
				customsCode.OK_CustomsRegNo = "ESXA56789AB?";
				AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, "A CCP Code can only have alphanumeric characters");

				customsCode.OK_CustomsRegNo = "ESXA56789ABC";
				AssertNoErrorContaining(customsCode.OK_CustomsRegNoInfo, "A CCP Code can only have alphanumeric characters");
			});
		}

		public void TestAutoUpperCase()
		{
			CombineAssertions(() =>
			{
				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
				customsCode.OK_CodeType = ZString.Empty;
				customsCode.OK_CustomsRegNo = "esxa12345678";
				AssertEquals("When entering RegNo, country equals to Spain and code is not CCP, RegNo should not be Auto Capitalized.",
							 "esxa12345678", customsCode.OK_CustomsRegNo);

				customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				customsCode.OK_CustomsRegNo = "esxa12345678";
				AssertEquals("When entering RegNo, country equals to Spain and code is CCP, RegNo should be Auto Capitalized.",
							 "ESXA12345678", customsCode.OK_CustomsRegNo);
				AssertNoWarnings(customsCode.OK_CustomsRegNoInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "MYADDRESS";
			address.OA_IsActive = true;
			customsCode = address.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			customsCode.OK_CustomsRegNo = "ESXA12345678";
		}
		OrgAddress address;
		OrgCusCode customsCode;
	}
}
