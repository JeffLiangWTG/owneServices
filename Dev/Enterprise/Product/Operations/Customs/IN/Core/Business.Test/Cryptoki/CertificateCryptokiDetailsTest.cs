using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CertificateCryptokiDetails))]
sealed class CertificateCryptokiDetailsTest : TestCaseWithFactory
{
	public void TestLibraryName()
	{
		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Certificate LibraryName is empty", cryptokiDetails.LibraryName);

			password.GP_Name = "WatchData";
			AssertEquals("Certificate LibraryName is set", "TRUSTKEYP11_ND_v34.dll", cryptokiDetails.LibraryName);
		});
	}

	public void TestCertificateSerialNumber()
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Certificate Serial Number is empty", cryptokiDetails.CertificateSerialNumber);

			password.GP_CertificateSerialNumber = "ABC123";
			AssertEquals("Certificate Serial Number is set", "ABC123", cryptokiDetails.CertificateSerialNumber);
		});
	}

	public void TestTokenPinStore()
	{
		AssertType<CertificateTokenPinStore>(cryptokiDetails.TokenPinStore);
	}

	protected override void SetUp()
	{
		base.SetUp();
		password = Factory.New<GlbCertificatePassword>();
		cryptokiDetails = new CertificateCryptokiDetails(password);
	}
	GlbCertificatePassword password;
	ICryptokiDetails cryptokiDetails;
}
