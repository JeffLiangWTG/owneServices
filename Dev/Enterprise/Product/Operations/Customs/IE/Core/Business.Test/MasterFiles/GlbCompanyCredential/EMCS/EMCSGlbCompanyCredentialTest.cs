using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(EMCSGlbCompanyCredential))]
	public class EMCSGlbCompanyCredentialTest : GlbExternalPasswordWithCertificateTest<EMCSGlbCompanyCredential>
	{
		public void TestIsCertificateValid()
		{
			var credential = GlbExternalPassword;
			credential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			credential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			IGlbExternalPasswordWithCertificate iCredential = credential;
			AssertEquals("IsCertificateValid", true, iCredential.IsCertificateValid);
			credential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			AssertEquals("IsCertificateValid", false, iCredential.IsCertificateValid);
		}

		public void TestIxTMessageAttributeProviderMembers()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			GlbExternalPassword.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			IxTMessageAttributeProvider provider = GlbExternalPassword;
			var dictionary = provider.GetMessageAttrDictionary();
			AssertEquals("cw1.key", ROSCertificateTestHelper.ValidCertificate_KeyPEM, dictionary["cw1.key"]);
			AssertEquals("cw1.certificate", ROSCertificateTestHelper.ValidCertificate_CertPEM, dictionary["cw1.certificate"]);
		}

		public void TestHashedPasswordIsStored()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			AssertEquals("Value stored should be hashed", ROSCertificateTestHelper.ValidHashedPassword, GlbExternalPassword.CurrentDecryptedCertificatePassphrase);
		}

		public void TestPasswordStatus()
		{
			GlbExternalPassword.GP_PasswordStatus = "VAL";
			AssertEquals("Valid", GlbExternalPassword.PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = "INV";
			AssertEquals("Invalid", GlbExternalPassword.PasswordStatus);
		}

		public void TestCanDelete()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_CustomsProfile = "ABCD";
			GlbExternalPassword.GP_MailBoxID = "Certificate Identifier1";
			Factory.Save();

			AssertEquals("CanDelete", true, GlbExternalPassword.CanDelete);
			declaration.JE_CustomsProfile = "Certificate Identifier1";
			AssertEquals("CanDelete", false, GlbExternalPassword.CanDelete);
		}

		public void TestMaxLength()
		{
			AssertEquals("GP_MailBoxIDInfo.MaxLength", 35, GlbExternalPassword.GP_MailBoxIDInfo.MaxLength);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Certificate Identifier", GlbExternalPassword.GP_MailBoxIDInfo.HumanReadableName);
			AssertEquals("Certificate Password", GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo.HumanReadableName);
			AssertEquals("Expiry Date", GlbExternalPassword.GP_ExpiryDateInfo.HumanReadableName);
		}

		public override void TestReadOnly()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_CustomsProfile = "ABCD";
			GlbExternalPassword.GP_MailBoxID = "Certificate Identifier1";
			Factory.Save();

			AssertEquals("GP_ExpireDate.ReadOnly", true, GlbExternalPassword.GP_ExpiryDateInfo.ReadOnly);
			AssertEquals("GP_PasswordStatus.ReadOnly", true, GlbExternalPassword.GP_PasswordStatusInfo.ReadOnly);

			AssertEquals("GP_MailBoxIDInfo.ReadOnly", false, GlbExternalPassword.GP_MailBoxIDInfo.ReadOnly);
			declaration.JE_CustomsProfile = "Certificate Identifier1";
			AssertEquals("GP_MailBoxIDInfo.ReadOnly", true, GlbExternalPassword.GP_MailBoxIDInfo.ReadOnly);
		}

		public override void TestSetDefaultValues()
		{
			var credential = Factory.New<EMCSGlbCompanyCredential>();
			AssertEquals(PasswordTypesList.Codes.IEM, credential.GP_PasswordType);
			AssertEquals(GlbCompany.CurrentCompany.PK, credential.GP_GC);
			AssertEquals(PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, credential.PasswordStatus);
		}

		public override void TestDataDefaultFromCertificate()
		{
			void SetValidCertificate()
			{
				GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
				GlbExternalPassword.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			}

			var validCertificate = new X509Certificate2(ROSCertificateTestHelper.ValidCertificate, ROSCertificateTestHelper.ValidHashedPassword);
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

		public void TestTransactionNumberSetToIsUsedOnCredentialChanged()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "YAC";
			company1.GC_Name = "TEST IE COMP 1";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "ZAC";
			company2.GC_Name = "TEST IE COMP 2";
			company2.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var wrapper1 = IE.Business.GlbCompanyWrapper.Get(company1);
			var company1Credential = wrapper1.EMCSGlbExternalPasswordCollection.AddNew();
			company1Credential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			company1Credential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;

			var wrapper2 = IE.Business.GlbCompanyWrapper.Get(company2);
			var company2Credential = wrapper2.EMCSGlbExternalPasswordCollection.AddNew();
			company2Credential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			company2Credential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;

			var company1TID1 = CreateTransactionNumber(company1.PK, company1Credential.PK, true);
			var company1TID2 = CreateTransactionNumber(company1.PK, company1Credential.PK, false);
			var company1TID3 = CreateTransactionNumber(company1.PK, company1Credential.PK, false);
			var company2TID1 = CreateTransactionNumber(company2.PK, company2Credential.PK, true);
			var company2TID2 = CreateTransactionNumber(company2.PK, company2Credential.PK, false);
			var company2TID3 = CreateTransactionNumber(company2.PK, company2Credential.PK, false);

			Factory.Save();

			CombineAssertions("No changes", () =>
			{
				AssertEquals(true, company1TID1.TN_IsUsed);
				AssertEquals(false, company1TID2.TN_IsUsed);
				AssertEquals(false, company1TID3.TN_IsUsed);
				AssertEquals(true, company2TID1.TN_IsUsed);
				AssertEquals(false, company2TID2.TN_IsUsed);
				AssertEquals(false, company2TID3.TN_IsUsed);
			});

			company1Credential.GP_Certificate = new byte[] { 1, 2, 3, 4 };
			Factory.Save();

			CombineAssertions("Company 1 - GP_Certificate Changed", () =>
			{
				AssertEquals(true, company1TID1.TN_IsUsed);
				AssertEquals(true, company1TID2.TN_IsUsed);
				AssertEquals(true, company1TID3.TN_IsUsed);
				AssertEquals(true, company2TID1.TN_IsUsed);
				AssertEquals(false, company2TID2.TN_IsUsed);
				AssertEquals(false, company2TID3.TN_IsUsed);
			});

			company2Credential.GP_MailBoxID = "NEWID";
			Factory.Save();

			CombineAssertions("Company 2 - GP_MailBoxID Changed", () =>
			{
				AssertEquals(true, company1TID1.TN_IsUsed);
				AssertEquals(true, company1TID2.TN_IsUsed);
				AssertEquals(true, company1TID3.TN_IsUsed);
				AssertEquals(true, company2TID1.TN_IsUsed);
				AssertEquals(true, company2TID2.TN_IsUsed);
				AssertEquals(true, company2TID3.TN_IsUsed);
			});
		}

		CusTransactionNumber CreateTransactionNumber(ZGuid companyPK, ZGuid credentialPK, ZBool isUsed)
		{
			var transactionNumber = Factory.New<CusTransactionNumber>();
			transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber.TN_GC_Company = companyPK;
			transactionNumber.TN_GP_ExternalPassword = credentialPK;
			transactionNumber.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber.TN_IsUsed = isUsed;
			return transactionNumber;
		}

		protected override EMCSGlbCompanyCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory) => factory.New<EMCSGlbCompanyCredential>();

		protected override byte[] ValidCertificate => ROSCertificateTestHelper.ValidCertificate;
		protected override string ValidPassword => ROSCertificateTestHelper.ValidPassword;
	}
}
