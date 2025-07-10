using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CodeType()
		{
			organisation.OH_IsConsignor = false;
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.ARN;
			AssertHasWarning(cusCode.OK_CodeTypeInfo, "The organisation does not have a Consignor role.");
			organisation.OH_IsConsignor = true;
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.ARN;
			AssertNoWarning(cusCode.OK_CodeTypeInfo, "The organisation does not have a Consignor role.");
		}

		public void TestValidateAEO()
		{
			var invalidCodeMessage = "AU AEO number should consist of 11 numeric characters.";

			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AEO;

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertHasWarning("Invalid Length", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "123456789012";
			AssertHasWarning("Invalid Length", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "1234567890X";
			AssertHasWarning("Invalid character", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "12345678901";
			AssertNoWarning("Valid Code", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
		}

		OrgCusCode cusCode;
		OrgHeader organisation;

		#endregion
	}
}
