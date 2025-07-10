using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BROrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZO_BSBNumber()
		{
			var oOrgHeader = Factory.New<OrgCountryData>();
			var oBRAddinfo = new BROrgImpAddInfo((ZPropertyInfoString)oOrgHeader.OV_ImportCustomsDefaultAddInfoInfo);

			oBRAddinfo.ZO_BSBNumber = "XXXX";
			AssertHasMessageErrorContaining(oBRAddinfo.ZO_BSBNumberInfo, "BSB number must be numeric");

			oBRAddinfo.ZO_BSBNumber = "1234";
			AssertNoMessageErrorContaining(oBRAddinfo.ZO_BSBNumberInfo, "BSB number must be numeric");
		}

		public void TestCheckZO_BrokerCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			var addInfo = new BROrgImpAddInfo(Factory);
			ValidationTestHelper.AssertErrorIfInvalidCode(addInfo.ZO_BrokerCodeInfo, "XXX", staff.GS_Code);

			addInfo.ZO_BrokerCode = staff.GS_Code;
			AssertHasMessageError(addInfo.ZO_BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(-10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			addInfo.Validation.ValidateZO_BrokerCode();
			AssertHasMessageError(addInfo.ZO_BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Invalid;
			addInfo.Validation.ValidateZO_BrokerCode();
			AssertHasMessageError(addInfo.ZO_BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			addInfo.Validation.ValidateZO_BrokerCode();
			AssertNoMessageError(addInfo.ZO_BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");
		}
	}
}
