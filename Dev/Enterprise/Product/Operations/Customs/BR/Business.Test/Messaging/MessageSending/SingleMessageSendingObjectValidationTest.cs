using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SingleMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBrokerCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "PRT";

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var sendingObjectParent = new GoodsCatalogMessageSendingObject(goodsCatalog);

			ValidationTestHelper.AssertErrorIfNotEntered(sendingObjectParent.BrokerCodeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(sendingObjectParent.BrokerCodeInfo, "XXX", staff.GS_Code);

			sendingObjectParent.BrokerCode = staff.GS_Code;
			sendingObjectParent.Validation.ValidateBrokerCode();
			AssertHasMessageError(sendingObjectParent.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(-10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			sendingObjectParent.Validation.ValidateBrokerCode();
			AssertHasMessageError(sendingObjectParent.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Invalid;
			sendingObjectParent.Validation.ValidateBrokerCode();
			AssertHasMessageError(sendingObjectParent.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			sendingObjectParent.Validation.ValidateBrokerCode();
			AssertNoMessageError(sendingObjectParent.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");
		}
	}
}

