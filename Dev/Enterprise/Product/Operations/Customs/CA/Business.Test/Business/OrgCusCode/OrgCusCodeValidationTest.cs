using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo()
		{
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.SocialInsuranceNumber;
			cusCode.OK_CustomsRegNo = "ABC###";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The number should be alphanumeric.");

			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense;
			cusCode.OK_CustomsRegNo = "ABC###";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The number should be alphanumeric.");

			cusCode.OK_CustomsRegNo = "ABC123";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The number should be alphanumeric.");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			org.OH_IsForwarder = true;
			var message = "An incorrectly formatted CCC number has been entered, please use 8XXX, where XXX is alpha-numeric characters only.";
			cusCode.OK_CustomsRegNo = "8";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "8AB34";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "8A#3";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "9030";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "8AB9";
			AssertNoWarningContaining(cusCode.OK_CustomsRegNoInfo, message);
		}

		public void TestCheckOK_CodeType()
		{
			var message1 = "Enter a valid Type.";
			var message2 = "CA AGT Registration Code is no longer valid. Use CA CCC instead.";

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			cusCode.OK_CodeType = "AGT";
			cusCode.OK_CustomsRegNo = "8000";
			AssertHasErrorContaining(cusCode.OK_CodeTypeInfo, message1);
			AssertHasErrorContaining(cusCode.OK_CodeTypeInfo, message2);

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoErrorContaining(cusCode.OK_CodeTypeInfo, message1);
			AssertNoErrorContaining(cusCode.OK_CodeTypeInfo, message2);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			org = Factory.New<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
		}

		OrgCusCode cusCode;
		OrgHeader org;

		#endregion
	}
}
