namespace Enterprise.Customs.ES.Business.Testing
{
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.ES.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Customs.XmlCredential;
	using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(Business.GlbExternalPassword))]
	public class GlbExternalPasswordTest : GlbExternalPasswordWithCertificateTest<Business.GlbExternalPassword>
	{
		public void TestAuthorisationsChildEditable()
		{
			AssertEquals(true, GlbExternalPassword.IsRegisteredEditableChildObject(GlbExternalPassword.Authorisations));
		}

		public void TestDelete()
		{
			var authorisation = GlbExternalPassword.Authorisations.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("External Password not deleted", false, GlbExternalPassword.IsDeleted);
				AssertEquals("Authorisation not deleted", false, authorisation.IsDeleted);
				GlbExternalPassword.Delete();
				AssertEquals("External Password deleted", true, GlbExternalPassword.IsDeleted);
				AssertEquals("Authorisation deleted", true, authorisation.IsDeleted);
			});
		}

		public override void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("GP_GC", GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
				AssertEquals("GP_PasswordType", PasswordTypesList.Codes.ESB, GlbExternalPassword.GP_PasswordType);
				AssertEquals("GP_PasswordStatus", PasswordStatusList.Codes.Invalid, GlbExternalPassword.GP_PasswordStatus);
			});
		}

		public void TestGP_PasswordStatus()
		{
			CombineAssertions(() =>
			{
				GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				AssertEquals("Valid Certificate and Passphrase", "VAL", GlbExternalPassword.GP_PasswordStatus);

				GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "TestInvalid";
				AssertEquals("Valid Certificate and Invalid Passphrase", "INV", GlbExternalPassword.GP_PasswordStatus);

				GlbExternalPassword.GP_Certificate = new byte[] { 241, 40 };
				GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				AssertEquals("Invalid Certificate and valid Passphrase", "INV", GlbExternalPassword.GP_PasswordStatus);
			});
		}

		public void TestCreateAndUpdateESCustomsStaffCredentials()
		{
			var validCert = new ZBlob(System.Text.Encoding.UTF8.GetBytes("file"));
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "PWS";
			glbStaff.CompanyName = "DTW";
			var wrapper = Business.GlbStaffWrapper.Get(glbStaff);

			var password1 = wrapper.ESBPasswordCollection.AddNew();
			SetCredentialData(password1, "CBK0123-0", validCert, "1");
			SetCredentialData(wrapper.ESBPasswordCollection.AddNew(), "CBK0124-0", validCert, "2");
			SetCredentialData(wrapper.ESBPasswordCollection.AddNew(), "CBK0125-0", validCert, "3");
			Factory.Save();

			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			CombineAssertions(() =>
			{
				var interchangeText = interchange.EI_BodyText;
				AssertContains("Interchange created with ESCustomsStaffCredentials", "Name=\"ESCustomsStaffCredentials\"", interchangeText);
				AssertContains("Password 1 exists on create", "<UserName>1</UserName>", interchangeText);
				AssertContains("Password 2 exists on create", "<UserName>2</UserName>", interchangeText);
				AssertContains("Password 3 exists on create", "<UserName>3</UserName>", interchangeText);
				interchange.Delete();
				password1.Delete();
				Factory.Save();
				interchange = Factory.GetLatestEHubConfigurationInterchange();
				interchangeText = interchange.EI_BodyText;
				AssertNotContains("Password 1 removed on update", "<UserName>1</UserName>", interchangeText);
				AssertContains("Password 2 exists on update", "<UserName>2</UserName>", interchangeText);
				AssertContains("Password 3 exists on update", "<UserName>3</UserName>", interchangeText);
			});
		}

		public void TestESCustomsStaffCredentialsStructure()
		{
			var validCert = new ZBlob(System.Text.Encoding.UTF8.GetBytes("file"));
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "PWS";
			glbStaff.CompanyName = "DTW";
			var wrapper = Business.GlbStaffWrapper.Get(glbStaff);

			SetCredentialData(wrapper.ESBPasswordCollection.AddNew(), "CBK0126-0", validCert, "1");
			Factory.Save();

			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			CombineAssertions(() =>
			{
				using (var reader = interchange.GetEI_BodyTextReader())
				{
					var configuration = reader.DeserializeToConfiguration();
					AssertEquals("Configuration name should be set", "ESCustomsStaffCredentials", configuration.Name);
					AssertEquals("There should be one configuration group", 1, configuration.Group.Count);

					var system = configuration.Group[0];
					AssertEquals("System type", "System", system.Type);
					AssertEquals("System reference", "EDIDAT", system.Reference);
					AssertEquals("There should be one System Item", 1, system.Items.Length);

					var company = (Group)system.Items[0];
					AssertEquals("Company type", "Company", company.Type);
					AssertEquals("Company reference", "EDI", company.Reference);
					AssertEquals("There should be one company item", 1, company.Items.Length);

					var staff = (Group)company.Items[0];
					AssertEquals("Staff type", "Staff", staff.Type);
					AssertEquals("staff reference should be set", "PWS", staff.Reference);
					AssertEquals("There should be 2 staff credentials", 1, staff.Items.Length);

					var credential = (Group)staff.Items[0];
					AssertEquals("Credential Reference", "CBK0126-0", credential.Reference);
					AssertEquals("Credential type", PasswordTypesList.Codes.ESB, credential.Type);
					AssertEquals("Credential status", "VAL", credential.Status);
					AssertEquals("There should be two credential Items", 2, credential.Items.Length);
					AssertEquals("Credential Username", "1", ((Credential)credential.Items[0]).UserName);

					var certificate = (Certificate)credential.Items[1];
					AssertEquals("Certificate name", "Certificate", certificate.Name);
					AssertEquals("Certificate File", false, certificate.File.Value.IsNullOrEmpty());
					AssertEquals("Certificate Passphrase", false, certificate.Passphrase.IsNullOrEmpty());
				}
			});
		}

		public void TestGetMessageAttrDictionary()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "PWS";
			var wrapper = Business.GlbStaffWrapper.Get(glbStaff);

			var credential = wrapper.ESBPasswordCollection.AddNew();
			credential.GP_PasswordType = PasswordTypesList.Codes.ESB;
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var value = string.Empty;
			var dictionary = credential.GetMessageAttrDictionary();

			dictionary.TryGetValue("cw1.key", out value);
			AssertEquals("key", X509Certificate2TestHelper.ValidCertificate_KeyPEM, value);

			dictionary.TryGetValue("cw1.certificate", out value);
			AssertEquals("certificate", X509Certificate2TestHelper.ValidCertificate_CertPEM, value);
		}

		protected override void SetCredentialData(Business.GlbExternalPassword credential)
		{
			base.SetCredentialData(credential);
			SetCredentialData(credential, "CBK0125-0", new ZBlob(System.Text.Encoding.UTF8.GetBytes("file")), "3");
		}

		protected override void ClearCredentialData(Business.GlbExternalPassword credential)
		{
			base.ClearCredentialData(credential);
			credential.GP_PasswordStatus = ZString.Empty;
			credential.GP_Name = ZString.Empty;
			credential.GP_PasswordType = ZString.Empty;
			credential.GP_Certificate = null;
			credential.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			credential.GP_PasswordStatus = ZString.Empty;
			credential.GP_UserID = ZString.Empty;
		}

		static void SetCredentialData(Business.GlbExternalPassword credential, ZString mailboxID, ZBlob validCertificate, ZString userID)
		{
			credential.GP_PasswordStatus = "XYZ";
			credential.GP_Name = mailboxID;
			credential.GP_PasswordType = PasswordTypesList.Codes.ESB;
			credential.GP_Certificate = validCertificate;
			credential.CurrentDecryptedCertificatePassphrase = "Pass";
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_UserID = userID;
		}
	}
}
