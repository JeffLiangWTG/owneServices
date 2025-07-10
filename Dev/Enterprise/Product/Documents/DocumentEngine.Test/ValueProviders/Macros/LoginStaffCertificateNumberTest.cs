using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginStaffCertificateNumber))]
	sealed class LoginStaffCertificateNumberTest : ValueProviderWithLoadControlFactoryTest<LoginStaffCertificateNumber>
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
				AssertEquals("Replaced Result when User is Null", "", ValueProviderToTest.GetReplacement("<LoginStaffCertificateNumber('PAS')>", Report))
			);
		}

		public override void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<LoginStaffCertificateNumber>");
			AssertNotResponsibleForReplacing("<LoginStaffCertificateNumber('XYZ', 'ABC')>");
			AssertIsResponsibleForReplacing("<LoginStaffCertificateNumber('XYZ')>");
			AssertIsResponsibleForReplacing("<LoginStaffCertificateNumber(ABC)>");
		}

		public override void TestReplacement()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Zayden12";
			var cert = staff.Certificates.AddNew();
			cert.XZ_Type = "PAS";
			cert.XZ_RefNumber = "123456";

			factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				AssertIsReplacedWith("123456", "<LoginStaffCertificateNumber(PAS)>");
				AssertIsReplacedWith("", "<LoginStaffCertificateNumber(XYZ)>");
			}
		}

		#region Implementation

		GlbStaff exampleTestStaff;
		protected override void PrepareDataForExamplesEvaluate()
		{
			var factory = new BusinessObjectFactory();
			exampleTestStaff = factory.NewWithValidTestData<GlbStaff>();
			exampleTestStaff.GS_LoginName = "Zayden12";
			var cert = exampleTestStaff.Certificates.AddNew();
			cert.XZ_Type = "PAS";
			cert.XZ_RefNumber = "123456";
			factory.Save();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			using (CurrentUserChanger.SwitchToNewUserTemporarily(exampleTestStaff.GS_LoginName))
			{
				AssertIsReplacedWith(expectedResult, example);
			}
		}

		#endregion
	}
}
