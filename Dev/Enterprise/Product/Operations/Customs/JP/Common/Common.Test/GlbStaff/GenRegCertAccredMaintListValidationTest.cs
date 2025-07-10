using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class GenRegCertAccredMaintListValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXZ_RefNumber()
		{
			var glbStaff = Factory.New<GlbStaff>();
			var certificate = glbStaff.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Japan;
			CombineAssertions(() =>
			{
				certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
				certificate.Validation.ValidateXZ_RefNumber();
				AssertHasMessageError(certificate.XZ_RefNumberInfo, "Please enter a Certificate Number.");
				certificate.XZ_RefNumber = "123";
				AssertHasMessageError(certificate.XZ_RefNumberInfo, "Please enter exactly 5 uppercase alphanumeric characters.");
				certificate.XZ_RefNumber = "123456";
				AssertHasMessageError(certificate.XZ_RefNumberInfo, "Please enter exactly 5 uppercase alphanumeric characters.");
				certificate.XZ_RefNumber = "ab123";
				AssertHasMessageError(certificate.XZ_RefNumberInfo, "Please enter exactly 5 uppercase alphanumeric characters.");
				certificate.XZ_RefNumber = "AB123";
				AssertNoMessageErrors(certificate.XZ_RefNumberInfo);
			});
		}

		public void TestRegisteredOnJPGlbStaff()
		{
			var glbStaff = Factory.New<GlbStaff>();
			var certificate = glbStaff.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Japan;
			AssertType<GenRegCertAccredMaintListValidation>(certificate.Validation);
		}
	}
}
