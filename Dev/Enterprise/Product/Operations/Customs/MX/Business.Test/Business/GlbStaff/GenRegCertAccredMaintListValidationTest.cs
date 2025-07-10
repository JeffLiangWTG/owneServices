using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Business.Testing
{
	sealed class GenRegCertAccredMaintListValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXZ_RefNumber()
		{
			var glbStaff = Factory.New<GlbStaff>();
			var certificate = glbStaff.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;

			CombineAssertions(() =>
			{
				certificate.XZ_RefNumber = ZString.Empty;
				certificate.Validation.ValidateXZ_RefNumber();
				AssertNoWarningContaining("The property should have a Warning if empty", certificate.XZ_RefNumberInfo, "The Broker’s Patent Number that allows their operation in Customs Clearance Areas is mandatory.");

				certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
				certificate.Validation.ValidateXZ_RefNumber();
				AssertHasWarningContaining("The property should have a Warning if empty", certificate.XZ_RefNumberInfo, "The Broker’s Patent Number that allows their operation in Customs Clearance Areas is mandatory.");

				certificate.XZ_RefNumber = "123";
				certificate.Validation.ValidateXZ_RefNumber();
				AssertHasError("The property should have an Error if the Length is not 4", certificate.XZ_RefNumberInfo, "A Patent number must be 4 digits long.");

				certificate.XZ_RefNumber = "1234";
				certificate.Validation.ValidateXZ_RefNumber();
				AssertNoNotifications("The property should not have Notifications", certificate.XZ_RefNumberInfo);
			});

			var glbStaff2 = Factory.New<GlbStaff>();
			var certificate2 = glbStaff2.Certificates.AddNew();
			certificate2.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificate2.XZ_RefNumber = "1234";
			certificate2.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;
			certificate2.Validation.ValidateXZ_RefNumber();
			AssertHasError("The property should have an Error if there are more than one Patent with the same number", certificate2.XZ_RefNumberInfo, "A Patent number must be unique per Staff.");

			certificate2.XZ_RefNumber = "1235";
			certificate2.Validation.ValidateXZ_RefNumber();
			AssertNoNotifications("The property should not have Notifications", certificate2.XZ_RefNumberInfo);
		}

		public void TestCheckXZ_Type()
		{
			var glbStaff = Factory.New<GlbStaff>();
			var certificate = glbStaff.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;

			CombineAssertions(() =>
			{
				certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
				certificate.Validation.ValidateXZ_Type();
				AssertNoNotifications("The property should not have any Notification", certificate.XZ_TypeInfo);

				var certificate2 = glbStaff.Certificates.AddNew();
				certificate2.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
				certificate2.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;
				certificate2.Validation.ValidateXZ_Type();
				AssertHasError("The property should have an Error if there are more than 1 BRK Type inserted", certificate2.XZ_TypeInfo, "A Broker (BRK) Type must be unique per Staff");
			});
		}

		public void TestRegisteredOnMXGlbStaff()
		{
			var glbStaff = Factory.New<GlbStaff>();
			var certificate = glbStaff.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;
			AssertType<GenRegCertAccredMaintListValidation>(certificate.Validation);
		}
	}
}
