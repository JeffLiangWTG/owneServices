using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using xTMessagingConstants = Enterprise.xTMessaging.Shared.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(GlbILExternalPassword))]
	public class GlbILExternalPasswordTest : GlbExternalPasswordWithCertificateTest<GlbILExternalPassword>
	{
		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			var password = Factory.New<GlbILExternalPassword>();
			AssertEquals("Password Type should be ILC", "ILC", password.GP_PasswordType);

			AssertNotEquals(ZGuid.Empty, password.GP_GC);
			AssertEquals(PasswordStatusList.Codes.Invalid, password.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, password.PasswordStatus);
		}

		public void TestGetMessageAttrDictionary()
		{
			var password = Factory.New<GlbILExternalPassword>();
			password.GP_UserID = "user";
			var expected = new Dictionary<string, string>
			{
				{ xTMessagingConstants.xTMsgAttributes.anycertificate, "xt-certificate:user" }
			};
			AssertContainsExactElementsInAnyOrder("Attributes dictionary should contain specific entries", expected, password.GetMessageAttrDictionary());
		}

		public void TestGetValidation()
		{
			var password = Factory.New<GlbILExternalPassword>();
			AssertType<GlbExternalPasswordWithCertificateValidation>("Validation of GlbILExternalPassword must be of specific type", password.Validation);
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
			AssertEquals("GP_MailBoxID.ReadOnly", true, GlbExternalPassword.GP_MailBoxIDInfo.ReadOnly);
			AssertEquals("GP_ExpiryDate.ReadOnly", true, GlbExternalPassword.GP_ExpiryDateInfo.ReadOnly);
		}

		public void TestCaptions()
		{
			var propertyInfo = typeof(GlbILExternalPassword).GetProperty("PasswordStatus");
			var attributePasswordStatus = propertyInfo.GetCustomAttribute<ResourceStringDataAttribute>();
			AssertEquals("GP_MailBoxID caption", "Private Key Status", attributePasswordStatus.Caption);

			AssertEquals("CurrentDecryptedCertificatePassphrase caption", "Private Key Password", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo).Caption);

			AssertEquals("GP_MailBoxID caption", "Sender VAT", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.GP_MailBoxIDInfo).Caption);
			AssertEquals("GP_ExpiryDate caption", "Expiry Date", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.GP_ExpiryDateInfo).Caption);
			AssertEquals("UserPasswordStatus caption", "Password Status", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.UserPasswordStatusInfo).Caption);
			AssertEquals("GP_UserID caption", "User Code", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.GP_UserIDInfo).Caption);
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
			AssertEquals("VAT has been defaulted from valid certificate", "Agenzia delle Dogane", GlbExternalPassword.GP_MailBoxID);

			SetValidCertificate();
			var invalidCertificate = new byte[] { 1, 2, 3, 4 };
			GlbExternalPassword.GP_Certificate = invalidCertificate;
			AssertEquals("Expiry Date has been cleared (invalid certificate data)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);
			AssertEquals("VAT has been cleared (invalid certificate data)", ZString.Empty, GlbExternalPassword.GP_MailBoxID);

			SetValidCertificate();
			var invalidPassword = "123456";
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = invalidPassword;
			AssertEquals("Expiry Date has been cleared (invalid certificate data - password)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);
			AssertEquals("VAT has been cleared (invalid certificate data - password)", ZString.Empty, GlbExternalPassword.GP_MailBoxID);
		}

		public override void TestCredentialRecipient()
		{
			var password = Factory.New<GlbILExternalPassword>();
			AssertEquals("CredentialRecipient should be DirectxT", CredentialRecipient.DirectxT, password.CredentialRecipient);
		}

		public void TestConfigurationName()
		{
			var password = Factory.New<GlbILExternalPassword>();
			AssertEquals("ConfigurationName should be IL Client Certificate", "ILClientCertificate", password.ConfigurationName);
		}

		public void TestInterchangeTypeForSending()
		{
			var password = Factory.New<GlbILExternalPassword>();
			password.GP_UserID = "user";
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			Factory.Save();
			var interchange = Factory.GetLatestDxTConfigurationInterchange("ILC");

			AssertNotNull("There should be an interchange", interchange);

			AssertEquals("Interchange type should be ILC", "ILC", interchange.EI_InterchangeType);
			AssertEquals("Interchange should be of transmit type", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("Interchange's transport type should be XTT", "XTT", interchange.EI_TransportType);
		}

		public void TestCreateDxTConfigurationInterchange()
		{
			var expectedInterchangeMessageValid = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GlbExternalPassword_Interchange_ValidMessage.xml"));
			var expectedInterchangeMessageInvalid = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GlbExternalPassword_Interchange_InvalidMessage.xml"));
			var password = Factory.New<GlbILExternalPassword>();
			password.GP_UserID = "user";
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_PasswordStatus = "VAL";

			Factory.Save();
			var interchangeValid = Factory.GetLatestDxTConfigurationInterchange("ILC");
			CombineAssertions("After setting certificate, Valid interchanged created", () =>
			{
				AssertNotNull("There should be an interchange", interchangeValid);
				AssertEquals("Message should be with VAL status and certificate", expectedInterchangeMessageValid, interchangeValid.EI_BodyText);
			});

			password.GP_Certificate = ZBlob.Empty;
			AssertEquals("[Prerequisite]:Password Status should be invalid", "INV", password.GP_PasswordStatus);
			Factory.Save();
			CombineAssertions("after clearing the certificate, Invalid Interchanged created", () =>
			{
				var interchangeInvalid = Factory.GetLatestDxTConfigurationInterchange("ILC");
				AssertNotNull("There should be new invalid interchange", interchangeInvalid);
				AssertNotEquals("New interchange created", interchangeInvalid.PK, interchangeValid.PK);
				AssertEquals("Message should be with INV status and empty certificate", expectedInterchangeMessageInvalid, interchangeInvalid.EI_BodyText);
			});
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
			AssertNull("No credential Send", Factory.GetLatestDxTConfigurationInterchange("ILC"));
			credential.Delete();
			Factory.Save();
			AssertNull("No credential Send", Factory.GetLatestDxTConfigurationInterchange("ILC"));

			credential = CreateNewGlbExternalPassword(Factory);
			SetCredentialData(credential);
			Factory.Save();
			var interchanges = Factory.Load<ILEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_IsActive, true));
			var interchange1 = Factory.GetLatestDxTConfigurationInterchange("ILC");
			AssertNotNull("Credential Send", interchange1);
			credential.Delete();
			Factory.Save();
			var interchange2 = Factory.GetLatestDxTConfigurationInterchange("ILC");
			AssertNotEquals("Should send credential on delete", interchange1, interchange2);

			interchange1 = Factory.GetLatestDxTConfigurationInterchange("ILC");
			credential = CreateNewGlbExternalPassword(Factory);
			ClearInitialCredentialData(credential);
			Factory.Save();
			AssertEquals("No credential Send", interchange1, Factory.GetLatestDxTConfigurationInterchange("ILC"));

			SetCredentialData(credential);
			Factory.Save();
			interchange2 = Factory.GetLatestDxTConfigurationInterchange("ILC");
			AssertNotEquals("Credential Send", interchange1, interchange2);

			ClearCredentialData(credential);
			Factory.Save();
			interchange1 = Factory.GetLatestDxTConfigurationInterchange("ILC");
			AssertNotEquals("Credential Send", interchange1, interchange2);

			SetCredentialData(credential);
			ClearCredentialData(credential);
			Factory.Save();
			AssertEquals("No new send as data hasn't changed seen saving", interchange1, Factory.GetLatestDxTConfigurationInterchange("ILC"));

			credential.Delete();
			Factory.Save();
			AssertEquals("No new send as data hasn't changed seen saving", interchange1, Factory.GetLatestDxTConfigurationInterchange("ILC"));
		}

		public void TestGP_PasswordStatus()
		{
			var glbcompany = Factory.NewWithValidTestData<GlbCompany>();
			glbcompany.GC_Code = "CMP";
			var glbgroup = Factory.NewWithValidTestData<GlbGroup>();
			glbgroup.GG_Code = "GG";
			var glbstaff = Factory.NewWithValidTestData<GlbStaff>();
			glbstaff.GS_Code = "XXX";

			var password = Factory.New<GlbILExternalPassword>();
			password.GP_UserID = "user";
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_GC = glbcompany.PK;
			password.GP_GG = glbgroup.PK;
			password.GP_GS = glbstaff.PK;

			AssertEquals("Password Status should be VAL", "VAL", password.GP_PasswordStatus);
			Factory.Save();

			var interchanges = Factory.GetQueuedDxTConfigurationInterchangeMessages("ILC");
			AssertEquals("There should be 1 interchange", 1, interchanges.Count);
			var interchange = interchanges.First();
			AssertNotNull("There should be an interchange", interchange);
			AssertConfiguration(interchange, "ILClientCertificate", "CMP", "GG", "XXX", 1);
			AssertEquals("Password Status should be AWA", "AWA", password.GP_PasswordStatus);

			password.GP_Certificate = ZBlob.Empty;
			AssertEquals("Password Status should be INV", "INV", password.GP_PasswordStatus);

			Factory.Save();
			AssertEquals("Password Status should remain INV after save", "INV", password.GP_PasswordStatus);
		}

		public void TestLookups()
		{
			AssertType<GlbILExternalPasswordLookups>(GlbExternalPassword.Lookups);
		}

		public void TestUserId_ReadOnly()
		{
			var password = Factory.New<GlbILExternalPassword>();
			Assert("GP_UserID should not be read only", !password.GP_UserIDInfo.ReadOnly);
		}

		public void TestUserId_List()
		{
			var password = Factory.New<GlbILExternalPassword>();
			var listAttr = password.GP_UserIDInfo.GetAttribute<ListAttribute>();
			AssertNotNull("GP_UserID should have list attribute", listAttr);
			AssertEquals("GP_UserID should have expected list definition", "Lookups.Staff", listAttr.ListDataSourceMember);
		}

		protected override void SetCredentialData(GlbILExternalPassword credential)
		{
			credential.GP_UserID = "user";
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		}

		protected override void ClearCredentialData(GlbILExternalPassword credential)
		{
			credential.GP_UserID = ZString.Empty;
			credential.GP_Certificate = ZBlob.Empty;
		}

		static void AssertConfiguration(Messaging.Integration.IEDIInterchange interchange, ZString configurationName, ZString companyCode, ZString groupCode, ZString staffCode, int passwordLength = 0)
		{
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var configuration = reader.DeserializeToConfiguration();
				AssertEquals("Configuration name should be set", configurationName, configuration.Name);

				var systemGroup = configuration.Group[0];
				var group = AssertConfigurationGroup(companyCode, groupCode, staffCode, systemGroup);

				AssertEquals("There should be password", passwordLength, group.Items?.Length ?? 0);

				if (passwordLength > 0)
				{
					AssertEquals("Password status should be VAL", "VAL", ((Group)group.Items[0]).Status);
				}
			}
		}

		static Group AssertConfigurationGroup(ZString companyCode, ZString groupCode, ZString staffCode, Group systemGroup)
		{
			var group = systemGroup;

			var groupList = new List<ZString>();
			if (!companyCode.IsEmpty)
			{
				groupList.Add("Company");
			}
			if (!groupCode.IsEmpty)
			{
				groupList.Add("Group");
			}
			if (!staffCode.IsEmpty)
			{
				groupList.Add("Staff");
			}

			AssertEquals("Group type should be set", "System", systemGroup.Type);
			AssertEquals("There should be one subgroup", groupList.Count > 0 ? 1 : 0, systemGroup.Items?.Length ?? 0);

			foreach (var groupName in groupList)
			{
				group = (Group)group.Items[0];
				AssertEquals("Group type should be set", groupName, group.Type);
				switch (groupName)
				{
					case "Company":
						AssertEquals(groupName, companyCode, group.Reference);
						break;
					case "Group":
						AssertEquals(groupName, groupCode, group.Reference);
						break;
					case "Staff":
						AssertEquals(groupName, staffCode, group.Reference);
						break;
				}
			}

			return group;
		}
	}
}
