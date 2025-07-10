using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(GlbCertificatePassword))]
sealed class GlbCertificatePasswordTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<GlbCertificatePassword>
{
	public void TestSetDefaultValues()
	{
		AssertEquals("INX", GlbExternalPassword.GP_PasswordType);
	}

	public void TestCaptions()
	{
		CombineAssertions("Caption for", () =>
		{
			var glbExternalPassword = GlbExternalPassword;
			AssertEquals("GP_CertificateAuthority", "Certificate Authority", DataBoundResourceStrings.GetDataForProperty(glbExternalPassword.GP_CertificateAuthorityInfo).Caption);
			AssertEquals("GP_Name", "Chipset Manufacturer", DataBoundResourceStrings.GetDataForProperty(glbExternalPassword.GP_NameInfo).Caption);
			AssertEquals("GP_CertificateSerialNumber", "Certificate SN", DataBoundResourceStrings.GetDataForProperty(glbExternalPassword.GP_CertificateSerialNumberInfo).Caption);
		});
	}

	public void TestLookups()
	{
		AssertType<GlbCertificatePasswordLookups>("Type", GlbExternalPassword.Lookups);
	}

	public void TestValidation()
	{
		AssertType<GlbCertificatePasswordValidation>("Type", GlbExternalPassword.Validation);
	}

	public void TestLibraryName()
	{
		CombineAssertions(() =>
		{
			RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
			var glbExternalPassword = GlbExternalPassword;
			AssertEquals("Default", ZString.Empty, glbExternalPassword.LibraryName);

			glbExternalPassword.GP_Name = "WatchData";
			AssertEquals("Valid chipset entered", "TRUSTKEYP11_ND_v34.dll", glbExternalPassword.LibraryName);

			glbExternalPassword.GP_Name = "XYZ";
			AssertEquals("Unknown chipset entered", ZString.Empty, glbExternalPassword.LibraryName);
		});
	}

	public void TestClearDetails()
	{
		var glbExternalPassword = GlbExternalPassword;
		glbExternalPassword.GP_CertificateAuthority = "Authority";
		glbExternalPassword.GP_Name = "Chipset";
		glbExternalPassword.GP_CertificateSerialNumber = "Serial";

		CombineAssertions("Assigned values", () =>
		{
			AssertEquals("GP_CertificateAuthority", "Authority", glbExternalPassword.GP_CertificateAuthority);
			AssertEquals("GP_Name", "Chipset", glbExternalPassword.GP_Name);
			AssertEquals("GP_CertificateSerialNumber", "Serial", glbExternalPassword.GP_CertificateSerialNumber);
		});

		glbExternalPassword.ClearDetails();
		CombineAssertions("After ClearDetails", () =>
		{
			AssertEquals("GP_CertificateAuthority", ZString.Empty, glbExternalPassword.GP_CertificateAuthority);
			AssertEquals("GP_Name", ZString.Empty, glbExternalPassword.GP_Name);
			AssertEquals("GP_CertificateSerialNumber", ZString.Empty, glbExternalPassword.GP_CertificateSerialNumber);
		});
	}

	public void TestGP_CertificateSerialNumberReadOnly()
	{
		AssertEquals(true, GlbExternalPassword.GP_CertificateSerialNumberInfo.ReadOnly);
	}

	public void TestGP_CertificateAuthorityReadOnly()
	{
		var glbExternalPassword = GlbExternalPassword;
		AssertEquals("When serial number empty", false, glbExternalPassword.GP_CertificateAuthorityInfo.ReadOnly);
		glbExternalPassword.GP_CertificateSerialNumber = "123";
		AssertEquals("When serial number filled", true, glbExternalPassword.GP_CertificateAuthorityInfo.ReadOnly);
	}

	public void TestGP_NameReadOnly()
	{
		var glbExternalPassword = GlbExternalPassword;
		AssertEquals("When serial number empty", false, glbExternalPassword.GP_NameInfo.ReadOnly);
		glbExternalPassword.GP_CertificateSerialNumber = "123";
		AssertEquals("When serial number filled", true, glbExternalPassword.GP_NameInfo.ReadOnly);
	}

	public void TestGP_NameChangeClearsGP_CertificateSerialNumber()
	{
		var glbExternalPassword = GlbExternalPassword;
		glbExternalPassword.GP_CertificateSerialNumber = "123";
		glbExternalPassword.GP_Name = "Watchdata";
		AssertEquals("GP_CertificateSerialNumber", ZString.Empty, glbExternalPassword.GP_CertificateSerialNumber);
	}

	public void TestHumanReadableName()
	{
		AssertEquals("Credentials > Digital Signature Certificate Token Profile", GlbExternalPassword.HumanReadableName);
	}
}
