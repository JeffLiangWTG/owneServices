using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbBrokerExternalPassword))]
sealed class GlbBrokerExternalPasswordTest : GlbExternalPasswordTest<GlbBrokerExternalPassword>
{
	protected override ZString PasswordType => "ITB";

	public override void TestSetDefaultValues()
	{
		base.TestSetDefaultValues();

		AssertEquals("GP_GC", ZGuid.Empty, GlbExternalPassword.GP_GC);
	}

	public void TestAccountNumber()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		GlbExternalPassword.GP_UserID = "";
		AssertEquals(ZString.Empty, GlbExternalPassword.AccountNumber);

		GlbExternalPassword.GP_UserID = "0000";
		AssertEquals("", GlbExternalPassword.AccountNumber);

		GlbExternalPassword.GP_UserID = "1234";
		AssertEquals("11111111111-001", GlbExternalPassword.AccountNumber);
	}

	public void TestLookups()
	{
		AssertType<GlbBrokerExternalPasswordLookups>(nameof(GlbExternalPassword.Lookups), GlbExternalPassword.Lookups);
	}

	public void TestValidation()
	{
		AssertType<GlbBrokerExternalPasswordValidation>(nameof(GlbExternalPassword.Validation), GlbExternalPassword.Validation);
	}

	public override void TestOnlySendCredentialIfNeeded()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "USER")
			.AppendAccountDetail("USER-DEC1", "DEC1")
			.Build();

		var credential = CreateNewGlbExternalPassword(Factory);
		Factory.Save();
		AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());
		credential.Delete();
		Factory.Save();
		AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());

		credential = CreateNewGlbExternalPassword(Factory);
		credential.GP_UserID = "USER";
		credential.GP_Certificate = new ZBlob(System.Text.Encoding.UTF8.GetBytes("CERT"));
		credential.CurrentDecryptedCertificatePassphrase = "PASSWORD";
		Factory.Save();
		var interchange1 = Factory.GetLatestEHubConfigurationInterchange();
		AssertNotNull("Credential Send", interchange1);
		credential.Delete();
		Factory.Save();
		var interchange2 = Factory.GetLatestEHubConfigurationInterchange();
		AssertNotEquals("Should send credential on delete", interchange1, interchange2);
		interchange1.Delete();
		interchange2.Delete();
		Factory.Save();

		foreach (var data in new[]
		{
			new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.GP_UserID, (ZString)"USER"),
			new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.GP_Certificate, new ZBlob(System.Text.Encoding.UTF8.GetBytes("CERT"))),
			new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.CurrentDecryptedCertificatePassphrase, (ZString)"PASSWORD")
		})
		{
			credential = CreateNewGlbExternalPassword(Factory);
			credential.GP_UserID = ZString.Empty;
			credential.GP_Certificate = ZBlob.Empty;
			credential.GP_CertificatePassPhrase = ZString.Empty;
			Factory.Save();
			AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());

			var info = credential.ZPropertyInfoHash.GetPropertySafe(data.Item1);
			info.Value = data.Item2;
			Factory.Save();
			interchange1 = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull("Credential Send", interchange1);

			info.Value = info.Value.Default;
			System.Threading.Thread.Sleep(1);
			Factory.Save();
			interchange2 = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotEquals("Credential Send", interchange1, interchange2);

			info.Value = data.Item2;
			info.Value = info.Value.Default;
			System.Threading.Thread.Sleep(1);
			Factory.Save();
			AssertEquals("No new send as data hasn't changed seen saving", interchange2, Factory.GetLatestEHubConfigurationInterchange());

			credential.Delete();
			Factory.Save();
			AssertEquals("No new send as data hasn't changed seen saving", interchange2, Factory.GetLatestEHubConfigurationInterchange());
			interchange1.Delete();
			interchange2.Delete();
			Factory.Save();
		}
	}

	public void TestITAddToStaffCredential_ValPwd()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "USER")
			.AppendAccountDetail("INTCODE1", "DEC1")
			.Build();

		GlbExternalPassword.GP_PasswordStatus = "XYZ";
		GlbExternalPassword.GP_UserID = "USER";
		GlbExternalPassword.GP_PasswordType = PasswordType;
		GlbExternalPassword.GP_Certificate = new ZBlob(System.Text.Encoding.UTF8.GetBytes("file"));
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "Pass";
		GlbExternalPassword.GP_PasswordStatus = "VAL";

		Factory.Save();
		var interchange = Factory.GetLatestEHubConfigurationInterchange();
		using (var reader = interchange.GetEI_BodyTextReader())
		{
			AssertConfigurationValues(reader.DeserializeToConfiguration(), "VAL");
		}
	}

	public void TestITAddToStaffCredential_InvPwd()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "USER")
			.AppendAccountDetail("INTCODE1", "DEC1")
			.Build();

		GlbExternalPassword.GP_PasswordStatus = "XYZ";
		GlbExternalPassword.GP_UserID = "USER";
		GlbExternalPassword.GP_PasswordType = PasswordType;
		GlbExternalPassword.GP_Certificate = new ZBlob(System.Text.Encoding.UTF8.GetBytes("file"));
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "Pass";
		GlbExternalPassword.GP_PasswordStatus = "INV";

		Factory.Save();
		var interchange = Factory.GetLatestEHubConfigurationInterchange();
		using (var reader = interchange.GetEI_BodyTextReader())
		{
			AssertConfigurationValues(reader.DeserializeToConfiguration(), "INV");
		}
	}

	void AssertConfigurationValues(Configuration configuration, ZString status)
	{
		AssertEquals("Configuration name should be set", "ITCustomsSubscribers", configuration.Name);
		AssertEquals("There should be one group", 1, configuration.Group.Count);

		var systemGroup = configuration.Group[0];
		AssertEquals("Group type should be set", "System", systemGroup.Type);
		AssertEquals("Group reference should be set", "EDIDAT", systemGroup.Reference);
		AssertEquals("There should be one subgroup", 1, systemGroup.Items.Length);

		var group = (Group)systemGroup.Items[0];
		AssertEquals("Group type should be set", "Staff", group.Type);
		AssertEquals("Group reference should be set", "ZAC", group.Reference);
		AssertEquals("There should be one subgroup", 1, group.Items.Length);

		var subgroup = (Group)group.Items[0];
		AssertEquals("Group type should be set", "ITB", subgroup.Type);
		AssertEquals("Group status should be set", status, subgroup.Status);
		AssertEquals("There should be two subgroups", 3, subgroup.Items.Length);

		var item = (Item)subgroup.Items[0];
		AssertEquals("Item name should be set", "MailBoxID", item.Name);
		AssertEquals("Item user name should be set", "11111111111-001", item.Value);

		var currentCredential = (Credential)subgroup.Items[1];
		AssertEquals("Credential name should be set", "Current", currentCredential.Name);
		AssertEquals("Credential user name should be set", "USER", currentCredential.UserName);

		var certificate = (Certificate)subgroup.Items[2];
		AssertEquals("Certificate name should be set", "Certificate", certificate.Name);
		AssertNotNull("Certificate file should be set", certificate.File);
		AssertNotNull("Certificate passphrase should be set", certificate.Passphrase);
	}

	IEnumerable<EDIInterchange> GetLatestEHubConfigurationInterchanges(BusinessObjectFactory factory, int count)
	{
		var dbQuery = new ZDBOnlyQuery(typeof(IEDIInterchange));
		dbQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.eHub);
		dbQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.Configuration);
		dbQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
		dbQuery.AddToFilter(EDIInterchangeSchema.EI_From, ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier);
		dbQuery.AddToFilter(EDIInterchangeSchema.EI_To, MasterFiles.Business.Customs.XmlCredential.Constants.Configuration.EHubRecipient);
		dbQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued);
		dbQuery.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.eHub);
		dbQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
		return factory.Load<EDIInterchange>(dbQuery).Take(count);
	}

	public void TestCreateCredentialXml()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "0001").AppendAccountDetail("0001-DEC1", "DEC1")
			.AppendAccount("11111111111-002", "0002").AppendAccountDetail("0002-DEC1", "DEC1")
			.AppendAccount("11111111111-003", "0003").AppendAccountDetail("0003-DEC1", "DEC1")
			.Build();

		var validCert = new ZBlob(System.Text.Encoding.UTF8.GetBytes("file"));
		var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
		glbStaff.GS_Code = "ZAC";

		var wrapper = GlbStaffWrapper.Get(glbStaff);
		var password1 = wrapper.PasswordCollection.AddNew();
		password1.GP_PasswordStatus = "XYZ";
		password1.GP_UserID = "0001";
		password1.GP_PasswordType = PasswordTypesList.Codes.ITB;
		password1.GP_Certificate = validCert;
		password1.CurrentDecryptedCertificatePassphrase = "Pass";
		password1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

		var password2 = wrapper.PasswordCollection.AddNew();
		password2.GP_PasswordStatus = "XYZ";
		password2.GP_UserID = "0002";
		password2.GP_PasswordType = PasswordTypesList.Codes.ITB;
		password2.GP_Certificate = validCert;
		password2.CurrentDecryptedCertificatePassphrase = "Pass";
		password2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

		var password3 = wrapper.PasswordCollection.AddNew();
		password3.GP_PasswordStatus = "XYZ";
		password3.GP_UserID = "0003";
		password3.GP_PasswordType = PasswordTypesList.Codes.ITB;
		password3.GP_Certificate = validCert;
		password3.CurrentDecryptedCertificatePassphrase = "Pass";
		password3.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

		Factory.Save();

		var interchanges = GetLatestEHubConfigurationInterchanges(Factory, 3).ToArray();
		AssertEquals(1, interchanges.Length);

		var interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"ITCustomsSubscribers\""));
		AssertConfigurationValuesWithCertificates(interchange, 3);
		AssertContains("<UserName>0001</UserName>", interchange.EI_BodyText);
		AssertContains("<UserName>0002</UserName>", interchange.EI_BodyText);
		AssertContains("<UserName>0003</UserName>", interchange.EI_BodyText);

		interchanges.ForEach(x => x.Delete());
		Factory.Save();

		password1.Delete();
		Factory.Save();
		interchanges = GetLatestEHubConfigurationInterchanges(Factory, 3).ToArray();
		AssertEquals(1, interchanges.Length);
		interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"ITCustomsSubscribers\""));
		AssertConfigurationValuesWithCertificates(interchange, 2);
		AssertNotContains("<UserName>0001</UserName>", interchange.EI_BodyText);
		AssertContains("<UserName>0002</UserName>", interchange.EI_BodyText);
		AssertContains("<UserName>0003</UserName>", interchange.EI_BodyText);

		interchanges.ForEach(x => x.Delete());
		Factory.Save();

		password2.Delete();
		password3.Delete();
		Factory.Save();
		interchanges = GetLatestEHubConfigurationInterchanges(Factory, 3).ToArray();
		AssertEquals(1, interchanges.Length);
		interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"ITCustomsSubscribers\""));
		AssertConfigurationValuesWithCertificates(interchange, 0);
		AssertNotContains("<UserName>0001</UserName>", interchange.EI_BodyText);
		AssertNotContains("<UserName>0002</UserName>", interchange.EI_BodyText);
		AssertNotContains("<UserName>0003</UserName>", interchange.EI_BodyText);
	}

	public void TestCertificateInfoDefaulting()
	{
		var validCertificateBinary = X509Certificate2TestHelper.ValidCertificate;
		var validCertificatePassword = X509Certificate2TestHelper.ValidPassword;
		var validCertificate = new X509Certificate2(validCertificateBinary, validCertificatePassword);
		var validCertificateExpiryDate = validCertificate.NotAfter;

		var emptyExpiryDate = ZDate.Empty;
		var emptyCertificateBinary = new ZBlob(Array.Empty<byte>());

		(var externalPassword1, var externalPassword2, var externalPassword3) = GetExternalPasswords(emptyCertificateBinary, emptyExpiryDate);

		CombineAssertions("PRE-CONDITION: the suspender works", () =>
		{
			AssertCertificateInfo(externalPassword1, certificate: emptyCertificateBinary, certificatePassword: "superman", expiryDate: emptyExpiryDate);
			AssertCertificateInfo(externalPassword2, certificate: emptyCertificateBinary, certificatePassword: "batman", expiryDate: emptyExpiryDate);
			AssertCertificateInfo(externalPassword3, certificate: emptyCertificateBinary, certificatePassword: "wolverine", expiryDate: emptyExpiryDate);
		});

		externalPassword1.GP_Certificate = validCertificateBinary;
		CombineAssertions("GP_Certificate has been updated", () =>
		{
			AssertCertificateInfo(externalPassword1, certificate: validCertificateBinary, certificatePassword: "superman", expiryDate: emptyExpiryDate);
			AssertCertificateInfo(externalPassword2, certificate: validCertificateBinary, certificatePassword: "batman", expiryDate: emptyExpiryDate);
			AssertCertificateInfo(externalPassword3, certificate: validCertificateBinary, certificatePassword: "wolverine", expiryDate: emptyExpiryDate);
		});

		externalPassword1.CurrentDecryptedCertificatePassphrase = validCertificatePassword;
		CombineAssertions("CurrentDecryptedCertificatePassphrase has been updated. GP_ExpiryDate has been defaulted", () =>
		{
			AssertCertificateInfo(externalPassword1, certificate: validCertificateBinary, certificatePassword: validCertificatePassword, expiryDate: validCertificateExpiryDate);
			AssertCertificateInfo(externalPassword2, certificate: validCertificateBinary, certificatePassword: validCertificatePassword, expiryDate: validCertificateExpiryDate);
			AssertCertificateInfo(externalPassword3, certificate: validCertificateBinary, certificatePassword: validCertificatePassword, expiryDate: validCertificateExpiryDate);
		});

		externalPassword1.GP_Certificate = emptyCertificateBinary;
		CombineAssertions("GP_ExpiryDate has been defaulted to empty", () =>
		{
			AssertCertificateInfo(externalPassword1, certificate: emptyCertificateBinary, certificatePassword: validCertificatePassword, expiryDate: emptyExpiryDate);
			AssertCertificateInfo(externalPassword2, certificate: emptyCertificateBinary, certificatePassword: validCertificatePassword, expiryDate: emptyExpiryDate);
			AssertCertificateInfo(externalPassword3, certificate: emptyCertificateBinary, certificatePassword: validCertificatePassword, expiryDate: emptyExpiryDate);
		});
	}

	#region Implementation

	(GlbExternalPassword externalPassword1, GlbExternalPassword externalPassword2, GlbExternalPassword externalPassword3) GetExternalPasswords(ZBlob emptyCertificateBinary, ZDate emptyExpiryDate)
	{
		var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(glbStaff);

		var passwordCollection = staffWrapper.PasswordCollection;
		using (passwordCollection.SuspendCertificateInfoPropagation())
		{
			var externalPassword1 = CreateExternalPassword(passwordCollection, userID: "1357", certificate: emptyCertificateBinary, certificatePassword: "superman", expiryDate: emptyExpiryDate);
			var externalPassword2 = CreateExternalPassword(passwordCollection, userID: "2468", certificate: emptyCertificateBinary, certificatePassword: "batman", expiryDate: emptyExpiryDate);
			var externalPassword3 = CreateExternalPassword(passwordCollection, userID: "3579", certificate: emptyCertificateBinary, certificatePassword: "wolverine", expiryDate: emptyExpiryDate);

			return (externalPassword1, externalPassword2, externalPassword3);
		}
	}

	void AssertCertificateInfo(GlbExternalPassword externalPassword, ZBlob certificate, ZString certificatePassword, ZDateTime expiryDate)
	{
		AssertEquals($"Expected GP_Certificate for GP_UserID={externalPassword.GP_UserID}", certificate, externalPassword.GP_Certificate);
		AssertEquals($"Expected CurrentDecryptedCertificatePassphrase for GP_UserID={externalPassword.GP_UserID}", certificatePassword, externalPassword.CurrentDecryptedCertificatePassphrase);
		AssertEquals($"Expected GP_ExpiryDate for GP_UserID={externalPassword.GP_UserID}", expiryDate, externalPassword.GP_ExpiryDate);
	}

	void AssertConfigurationValuesWithCertificates(IEDIInterchange interchange, int certificateCount)
	{
		using (var reader = interchange.GetEI_BodyTextReader())
		{
			var configuration = reader.DeserializeToConfiguration();
			AssertEquals("Configuration name should be set", "ITCustomsSubscribers", configuration.Name);
			AssertEquals("There should be one group", 1, configuration.Group.Count);

			var systemGroup = configuration.Group[0];
			AssertEquals("Group type should be set", "System", systemGroup.Type);
			AssertEquals("Group reference should be set", "EDIDAT", systemGroup.Reference);
			AssertEquals("There should be one subgroup", 1, systemGroup.Items.Length);

			var group = (Group)systemGroup.Items[0];
			AssertEquals("Group type should be set", "Staff", group.Type);
			AssertEquals("Group reference should be set", "ZAC", group.Reference);
			var subGroupCount = group.Items?.Length ?? 0;
			AssertEquals("There should be one subgroup", certificateCount, subGroupCount);

			if (subGroupCount > 0)
			{
				var subgroup = (Group)group.Items[0];
				AssertEquals("Group type should be set", "ITB", subgroup.Type);
				AssertEquals("There should be two subgroups", 3, subgroup.Items.Length);

				var item = (Item)subgroup.Items[0];
				AssertEquals("Item name should be set", "MailBoxID", item.Name);

				var currentCredential = (Credential)subgroup.Items[1];
				AssertEquals("Credential name should be set", "Current", currentCredential.Name);

				var certificate = (Certificate)subgroup.Items[2];
				AssertEquals("Certificate name should be set", "Certificate", certificate.Name);
			}
		}
	}

	GlbExternalPassword CreateExternalPassword(GlbBrokerExternalPasswordCollection passwordCollection, ZString userID, ZBlob certificate, ZString certificatePassword, ZDateTime expiryDate)
	{
		var externalPassword = passwordCollection.AddNew();
		externalPassword.GP_UserID = userID;
		externalPassword.GP_PasswordType = PasswordTypesList.Codes.ITB;
		externalPassword.GP_Certificate = certificate;
		externalPassword.CurrentDecryptedCertificatePassphrase = certificatePassword;
		externalPassword.GP_ExpiryDate = expiryDate;
		return externalPassword;
	}

	#endregion
}
