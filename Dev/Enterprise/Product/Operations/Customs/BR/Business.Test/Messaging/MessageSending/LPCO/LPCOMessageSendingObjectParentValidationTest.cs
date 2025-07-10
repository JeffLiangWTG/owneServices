using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	 class LPCOMessageSendingObjectParentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBrokerCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "PRT";

			var header = Factory.NewWithValidTestData<CusLPCOHeader>();
			var lpco = new LPCOMessageSendingObjectParent(header);

			ValidationTestHelper.AssertErrorIfNotEntered(lpco.BrokerCodeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(lpco.BrokerCodeInfo, "XXX", staff.GS_Code);

			lpco.BrokerCode = staff.GS_Code;
			lpco.Validation.ValidateBrokerCode();
			AssertHasMessageError(lpco.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(-10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			lpco.Validation.ValidateBrokerCode();
			AssertHasMessageError(lpco.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			lpco.Validation.ValidateBrokerCode();
			AssertHasMessageError(lpco.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			lpco.Validation.ValidateBrokerCode();
			AssertNoMessageError(lpco.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");
		}
	}
}
