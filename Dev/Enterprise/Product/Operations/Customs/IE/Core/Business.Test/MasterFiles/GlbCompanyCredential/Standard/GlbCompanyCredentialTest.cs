using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredential))]
	public class GlbCompanyCredentialTest : GlbExternalPasswordWithCertificateTest<GlbCompanyCredential>
	{
		public void TestIsCertificateValid()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			credential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			credential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			IGlbExternalPasswordWithCertificate iCredential = credential;
			AssertEquals("IsCertificateValid", true, iCredential.IsCertificateValid);
			credential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			AssertEquals("IsCertificateValid", false, iCredential.IsCertificateValid);
		}

		public void TestIxTMessageAttributeProviderMembers()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			credential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			credential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			IxTMessageAttributeProvider provider = credential;
			var dictionary = provider.GetMessageAttrDictionary();
			AssertEquals("cw1.key", ROSCertificateTestHelper.ValidCertificate_KeyPEM, dictionary["cw1.key"]);
			AssertEquals("cw1.certificate", ROSCertificateTestHelper.ValidCertificate_CertPEM, dictionary["cw1.certificate"]);
		}

		public override void TestSetDefaultValues()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			AssertEquals(PasswordTypesList.Codes.IER, credential.GP_PasswordType);
			AssertNotEquals(ZGuid.Empty, credential.GP_GC);
			AssertEquals(PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, credential.PasswordStatus);
		}

		public void TestHashedPasswordIsStored()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			credential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			AssertEquals("Value stored should be hashed", ROSCertificateTestHelper.ValidHashedPassword, credential.CurrentDecryptedCertificatePassphrase);
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

			var company1Credential = InterchangeProcessorTestHelper.CreateValidCredential(company1);
			var company2Credential = InterchangeProcessorTestHelper.CreateValidCredential(company2);

			var company1TID1 = CreateTransactionNumber(company1, true);
			var company1TID2 = CreateTransactionNumber(company1, false);
			var company1TID3 = CreateTransactionNumber(company1, false);
			var company2TID1 = CreateTransactionNumber(company2, true);
			var company2TID2 = CreateTransactionNumber(company2, false);
			var company2TID3 = CreateTransactionNumber(company2, false);

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

		protected override GlbCompanyCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory) => factory.New<GlbCompanyCredential>();
		protected override byte[] ValidCertificate => ROSCertificateTestHelper.ValidCertificate;
		protected override string ValidPassword => ROSCertificateTestHelper.ValidPassword;

		CusTransactionNumber CreateTransactionNumber(GlbCompany company, ZBool isUsed)
		{
			var transactionNumber = Factory.New<CusTransactionNumber>();
			transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber.TN_GC_Company = company.PK;
			transactionNumber.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber.TN_IsUsed = isUsed;
			return transactionNumber;
		}
	}
}
