using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using CHPasswordStatusList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.PasswordStatusList;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbCompanyWrapper))]
sealed class GlbCompanyWrapperTest : GlbCompanyWrapperTest<GlbCompanyWrapper>
{
	public void TestIsValidWrapper()
	{
		Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

		var wrapper = GetWrapper(Company);
		Assert("Should be true as the country code is CH.", wrapper.IsValidWrapper);
	}

	public void TestGlbExternalPassword() => CombineAssertions(() =>
	{
		Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

		var wrapper = GetWrapper(Company);
		var credential = wrapper.GlbExternalPassword;
		credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		credential.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
		credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
		Factory.Save();

		var cctCredential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company).GlbExternalPassword;
		AssertEquals("GP_PK should be", credential.PK, cctCredential.PK);
		AssertEquals("GP_PasswordType should be", PasswordTypesList.Codes.CHC, cctCredential.GP_PasswordType);
		AssertEquals("GP_ExpiryDate should be", true, cctCredential.GP_ExpiryDate.IsInTheFuture());
		AssertEquals("GP_PasswordStatus should be", PasswordStatusList.Codes.Valid, cctCredential.GP_PasswordStatus);
	});

	public void TestGetCachedValue()
	{
		var wrapper1 = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company);
		var wrapper2 = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company);
		AssertSame(wrapper1, wrapper2);
	}

	public void TestTokenCredentialsEnabled_Setter() => CombineAssertions(() =>
	{
		var glbCompanyWrapper = GetWrapper(Company);
		AssertNull("TokenCredentials do not exist", glbCompanyWrapper.TokenCredentials);

		glbCompanyWrapper.TokenCredentialsEnabled = false;
		AssertNull("TokenCredentials not created", glbCompanyWrapper.TokenCredentials);

		glbCompanyWrapper.TokenCredentialsEnabled = true;
		var tokenCredentials = glbCompanyWrapper.TokenCredentials;
		AssertNotNull("Enabled - TokenCredentials created", tokenCredentials);
		AssertEquals("Enabled - GP_PasswordStatus", ZString.Empty, tokenCredentials.GP_PasswordStatus);

		glbCompanyWrapper.TokenCredentialsEnabled = false;
		AssertSame("Disabled - TokenCredentials not changed", tokenCredentials, glbCompanyWrapper.TokenCredentials);
		AssertEquals("Disabled - GP_PasswordStatus", CHPasswordStatusList.Suspended, tokenCredentials.GP_PasswordStatus);

		glbCompanyWrapper.TokenCredentialsEnabled = true;
		AssertSame("Re-enabled - TokenCredentials not changed", tokenCredentials, glbCompanyWrapper.TokenCredentials);
		AssertEquals("Re-enabled - GP_PasswordStatus", ZString.Empty, tokenCredentials.GP_PasswordStatus);

		tokenCredentials.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
		glbCompanyWrapper.TokenCredentialsEnabled = true;
		AssertEquals("Enable when already enabled - GP_PasswordStatus no change", PasswordStatusList.Codes.Valid, tokenCredentials.GP_PasswordStatus);
	});

	public void TestTokenCredentialsEnabled_Getter() => CombineAssertions(() =>
	{
		var glbCompanyWrapper = GetWrapper(Company);
		AssertNull("TokenCredentials do not exist (pre-condition)", glbCompanyWrapper.TokenCredentials);
		AssertEquals("TokenCredentials do not exist", false, glbCompanyWrapper.TokenCredentialsEnabled);
		AssertNull("TokenCredentials not created", glbCompanyWrapper.TokenCredentials);

		var tokenCredentials = glbCompanyWrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyTokenCredentials>(PasswordTypesList.Codes.CHT);

		tokenCredentials.GP_PasswordStatus = ZString.Empty;
		AssertEquals($"GP_PasswordStatus={tokenCredentials.GP_PasswordStatus}", true, glbCompanyWrapper.TokenCredentialsEnabled);

		tokenCredentials.GP_PasswordStatus = CHPasswordStatusList.Suspended;
		AssertEquals($"GP_PasswordStatus={tokenCredentials.GP_PasswordStatus}", false, glbCompanyWrapper.TokenCredentialsEnabled);

		tokenCredentials.GP_PasswordStatus = CHPasswordStatusList.Received;
		AssertEquals($"GP_PasswordStatus={tokenCredentials.GP_PasswordStatus}", true, glbCompanyWrapper.TokenCredentialsEnabled);
	});

	public void TestCurrentCompanyTokenCredentialsEnabled() => CombineAssertions(() =>
	{
		AssertEquals(false, GlbCompanyWrapper.CurrentCompanyTokenCredentialsEnabled);

		var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
		wrapper.TokenCredentialsEnabled = true;
		AssertEquals(true, GlbCompanyWrapper.CurrentCompanyTokenCredentialsEnabled);
		GlbCompany.CurrentCompany.Factory.Save();
		AssertEquals(true, wrapper.TokenCredentials.IsInDatabase);

		var companyInAnotherFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(companyInAnotherFactory).TokenCredentialsEnabled = false;
		companyInAnotherFactory.Factory.Save();

		AssertEquals(true, wrapper.TokenCredentialsEnabled);
		AssertEquals(false, GlbCompanyWrapper.CurrentCompanyTokenCredentialsEnabled);
	});

	public void TestConfigurationStructure() => CombineAssertions(() =>
	{
		var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company);

		Company.GC_CustomsRegistrationNo = "4321";
		Factory.Save();

		var interchange = Factory.GetLatestDxTConfigurationInterchange(MessagingConstants.xTCustomConfiguration.InterchangeTypes);
		AssertConfiguration(MessagingConstants.xTCustomConfiguration.Status.Update);

		Company.GC_CustomsRegistrationNo = ZString.Empty;
		Factory.Save();

		interchange = Factory.GetLatestDxTConfigurationInterchange(MessagingConstants.xTCustomConfiguration.InterchangeTypes);
		AssertConfiguration(MessagingConstants.xTCustomConfiguration.Status.Delete);

		void AssertConfiguration(ZString status)
		{
			using var reader = interchange.GetEI_BodyTextReader();
			var configuration = reader.DeserializeToConfiguration();
			AssertEquals("Configuration Name", MessagingConstants.xTCustomConfiguration.Names.RegistrationNumber, configuration.Name);
			AssertEquals("Configuration Group.Count", 1, configuration.Group.Count);

			var systemGroup = configuration.Group[0];
			AssertEquals("System Type", "System", systemGroup.Type);
			AssertEquals("System Reference", "EDIDAT", systemGroup.Reference);
			AssertEquals("System Items.Length", 1, systemGroup.Items.Length);

			var companyGroup = systemGroup.Items[0] as Group;
			AssertEquals("Company type", "Company", companyGroup.Type);
			AssertEquals("Company reference", Company.GC_Code, companyGroup.Reference);
			AssertEquals("Company Status", status, companyGroup.Status);
			AssertEquals("Company Items.Length", 1, companyGroup.Items.Length);

			var credential = companyGroup.Items[0] as Credential;
			AssertEquals("Credential Username", Company.GC_CustomsRegistrationNo, credential.UserName);
		}
	});
}
