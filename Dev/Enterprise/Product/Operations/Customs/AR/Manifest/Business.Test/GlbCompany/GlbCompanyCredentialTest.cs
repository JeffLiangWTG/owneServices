using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredential))]
	class GlbCompanyCredentialTest : GlbExternalPasswordWithCertificateTest<GlbCompanyCredential>
	{
		protected override GlbCompanyCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory) => factory.New<GlbCompanyCredential>();

		public override void TestSetDefaultValues()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(PasswordTypesList.Codes.ARB, credential.GP_PasswordType);
			AssertNotEquals(ZGuid.Empty, credential.GP_GC);
			AssertEquals(PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, credential.PasswordStatus);
		}

		public void TestPasswordStatus()
		{
			GlbExternalPassword.GP_PasswordStatus = "VAL";
			AssertEquals("Valid", GlbExternalPassword.PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = "INV";
			AssertEquals("Invalid", GlbExternalPassword.PasswordStatus);
		}

		public override void TestReadOnly()
		{
			AssertEquals("GP_PasswordStatus.ReadOnly", true, GlbExternalPassword.GP_PasswordStatusInfo.ReadOnly);
		}

		public void TestCurrentDecryptedCertificatePassphraseMaxLength()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(10, credential.CurrentDecryptedCertificatePassphraseInfo.MaxLength);
		}

		public override void TestDataDefaultFromCertificate()
		{
			void SetValidCertificate()
			{
				GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			}

			var validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);
			SetValidCertificate();
			AssertEquals("Expiry Date has been defaulted from valid certificate", validCertificate.NotAfter, GlbExternalPassword.GP_ExpiryDate);

			SetValidCertificate();
			var invalidCertificate = new byte[] { 1, 2, 3, 4 };
			GlbExternalPassword.GP_Certificate = invalidCertificate;
			AssertEquals("Expiry Date has been cleared (invalid certificate data)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);

			SetValidCertificate();
			var invalidPassword = "123456";
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = invalidPassword;
			AssertEquals("Expiry Date has been cleared (invalid certificate data - password)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);
		}

		public void TestShouldSendCredential()
		{
			var credential = Factory.New<GlbCompanyCredentialForTest>();
			AssertEquals("ShouldSendCredential", false, credential.ShouldSendCredentialExposed());
		}

		public void TestCreateDxTConfigurationInterchange()
		{
			var password = Factory.New<GlbCompanyCredential>();
			password.GP_UserID = "user";
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_PasswordStatus = "VAL";

			Factory.Save();
			var interchangeValid = Factory.GetLatestDxTConfigurationInterchange("ARX");
			CombineAssertions("After setting certificate, Valid interchanged created", () =>
			{
				AssertNotNull("There should be an interchange", interchangeValid);
				AssertContains("Message should be with VAL status and certificate", @"<Group Type=""ARB"" Status=""VAL"">", interchangeValid.EI_BodyText);
				AssertNotContains("Message should not have an Empty File", @"<File />", interchangeValid.EI_BodyText);
			});

			password.GP_Certificate = ZBlob.Empty;
			AssertEquals("[Prerequisite]:Password Status should be invalid", "INV", password.GP_PasswordStatus);
			Factory.Save();
			CombineAssertions("after clearing the certificate, Invalid Interchanged created", () =>
			{
				var interchangeInvalid = Factory.GetLatestDxTConfigurationInterchange("ARX");
				AssertNotNull("There should be new invalid interchange", interchangeInvalid);
				AssertNotEquals("New interchange created", interchangeInvalid.PK, interchangeValid.PK);
				AssertContains("Message should be with INV status and empty certificate", @"<Group Type=""ARB"" Status=""INV"">", interchangeInvalid.EI_BodyText);
				AssertContains("Message should have an Empty File", @"<File />", interchangeInvalid.EI_BodyText);
			});
		}

		public override void TestCredentialRecipient()
		{
			var password = Factory.New<GlbCompanyCredential>();
			AssertEquals("CredentialRecipient should be DirectxT", CredentialRecipient.DirectxT, password.CredentialRecipient);
		}

		public void TestConfigurationName()
		{
			var password = Factory.New<GlbCompanyCredential>();
			AssertEquals("ConfigurationName should be AR Client Certificate", "ARClientCertificate", password.ConfigurationName);
		}

		public void TestInterchangeTypeForSending()
		{
			var password = Factory.New<GlbCompanyCredential>();
			password.GP_UserID = "user";
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			Factory.Save();
			var interchange = Factory.GetLatestDxTConfigurationInterchange("ARX");

			AssertNotNull("There should be an interchange", interchange);

			AssertEquals("Interchange type should be ARB", "ARX", interchange.EI_InterchangeType);
			AssertEquals("Interchange should be of transmit type", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("Interchange's transport type should be XTT", "XTT", interchange.EI_TransportType);
		}

		public override void TestOnlySendCredentialIfNeeded()
		{
			var credential = CreateNewGlbExternalPassword(Factory);
			if (credential.ConfigurationName.IsEmpty)
			{
				Assert("No need to send", true);
				return;
			}

			Factory.Save();
			AssertNull("No credential Send", Factory.GetLatestDxTConfigurationInterchange("ARX"));
			credential.Delete();
			Factory.Save();
			AssertNull("No credential Send", Factory.GetLatestDxTConfigurationInterchange("ARX"));

			credential = CreateNewGlbExternalPassword(Factory);
			SetCredentialData(credential);
			Factory.Save();
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_IsActive, true));
			var interchange1 = Factory.GetLatestDxTConfigurationInterchange("ARX");
			AssertNotNull("Credential Send", interchange1);
			credential.Delete();
			Factory.Save();
			var interchange2 = Factory.GetLatestDxTConfigurationInterchange("ARX");
			AssertNotEquals("Should send credential on delete", interchange1, interchange2);

			interchange1 = Factory.GetLatestDxTConfigurationInterchange("ARX");
			credential = CreateNewGlbExternalPassword(Factory);
			ClearInitialCredentialData(credential);
			Factory.Save();
			AssertEquals("No credential Send", interchange1, Factory.GetLatestDxTConfigurationInterchange("ARX"));

			SetCredentialData(credential);
			Factory.Save();
			interchange2 = Factory.GetLatestDxTConfigurationInterchange("ARX");
			AssertNotEquals("Credential Send", interchange1, interchange2);

			ClearCredentialData(credential);
			Factory.Save();
			interchange1 = Factory.GetLatestDxTConfigurationInterchange("ARX");
			AssertNotEquals("Credential Send", interchange1, interchange2);

			SetCredentialData(credential);
			ClearCredentialData(credential);
			Factory.Save();
			AssertEquals("No new send as data hasn't changed seen saving", interchange1, Factory.GetLatestDxTConfigurationInterchange("ARX"));

			credential.Delete();
			Factory.Save();
			AssertEquals("No new send as data hasn't changed seen saving", interchange1, Factory.GetLatestDxTConfigurationInterchange("ARX"));
		}

		void SetCredentialData(GlbExternalPassword credential)
		{
			credential.GP_UserID = "user";
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		}

		void ClearCredentialData(GlbExternalPassword credential)
		{
			credential.GP_UserID = ZString.Empty;
			credential.GP_Certificate = ZBlob.Empty;
		}

		public void TestLookups()
		{
			var credential = CreateNewGlbExternalPassword(Factory);

			AssertType<GlbCompanyCredentialLookups>(credential.Lookups);
		}
	}

	class GlbCompanyCredentialForTest : GlbCompanyCredential
	{
		public GlbCompanyCredentialForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldSendCredentialExposed() => ShouldSendCredential();
	}
}
