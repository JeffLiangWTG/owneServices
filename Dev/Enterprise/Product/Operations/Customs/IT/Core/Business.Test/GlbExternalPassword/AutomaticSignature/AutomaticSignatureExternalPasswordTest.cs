using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(AutomaticSignatureExternalPassword))]
sealed class AutomaticSignatureExternalPasswordTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<AutomaticSignatureExternalPassword>
{
	public void TestGP_PasswordType()
	{
		AssertEquals("GP_PasswordType", "ITA", GlbExternalPassword.GP_PasswordType);
	}

	public void TestIsConfigurationActiveGetter()
	{
		CombineAssertions(() =>
		{
			GlbExternalPassword.GP_PasswordStatus = "VAL";
			AssertEquals("When GP_PasswordStatus is 'VAL', IsConfigurationActive", true, GlbExternalPassword.IsConfigurationActive);

			GlbExternalPassword.GP_PasswordStatus = "";
			AssertEquals("When GP_PasswordStatus is not 'VAL', IsConfigurationActive", false, GlbExternalPassword.IsConfigurationActive);
		});
	}

	public void TestIsConfigurationActiveSetter()
	{
		CombineAssertions(() =>
		{
			GlbExternalPassword.IsConfigurationActive = true;
			AssertEquals("When IsConfigurationActive is 'true', GP_PasswordStatus", "VAL", GlbExternalPassword.GP_PasswordStatus);

			GlbExternalPassword.IsConfigurationActive = false;
			AssertEquals("When IsConfigurationActive is 'false', GP_PasswordStatus", "", GlbExternalPassword.GP_PasswordStatus);
		});
	}

	public void TestIsConfigurationActiveAttributes() => CombineAssertions(() =>
		AssertEntity<AutomaticSignatureExternalPassword>()
			.HasProperty(x => x.IsConfigurationActive)
			.WithCaption("Enabled"));

	public void TestGP_MailBoxIDAttributes() => CombineAssertions(() =>
		AssertEntity<AutomaticSignatureExternalPassword>()
			.HasProperty(x => x.GP_MailBoxID)
			.WithCaption("Delegate")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 10)
			.WithList("Lookups.DelegateList"));

	public void TestGP_UserIDAttributes() => CombineAssertions(() =>
		AssertEntity<AutomaticSignatureExternalPassword>()
			.HasProperty(x => x.GP_UserID)
			.WithCaption("User")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 100));

	public void TestGP_NameAttributes() => CombineAssertions(() =>
		AssertEntity<AutomaticSignatureExternalPassword>()
			.HasProperty(x => x.GP_Name)
			.WithCaption("User Fiscal Code")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 16));

	public void TestGP_CertificateAuthority()
	{
		AssertEquals("GP_CertificateAuthority", "ARU", GlbExternalPassword.GP_CertificateAuthority);
	}

	public void TestLookups()
	{
		AssertType<AutomaticSignatureExternalPasswordLookups>("Lookups Type", GlbExternalPassword.Lookups);
	}

	public void TestValidation()
	{
		AssertType<AutomaticSignatureExternalPasswordValidation>("Validation Type", GlbExternalPassword.Validation);
	}

	public void TestDelegateName()
	{
		AutomaticSignatureExternalPasswordLookupsTest.SetUpRefSysConfigs(Factory);

		IAutomaticSignatureExternalPassword automaticSignatureExternalPassword = GlbExternalPassword;
		AssertEquals("When GP_MailBoxID is empty, DelegateName", "", automaticSignatureExternalPassword.DelegateName);

		GlbExternalPassword.GP_MailBoxID = "ITARDSDL02";
		AssertEquals("When GP_MailBoxID is ITARDSDL02, DelegateName", "angela.stecca", automaticSignatureExternalPassword.DelegateName);
	}
}
