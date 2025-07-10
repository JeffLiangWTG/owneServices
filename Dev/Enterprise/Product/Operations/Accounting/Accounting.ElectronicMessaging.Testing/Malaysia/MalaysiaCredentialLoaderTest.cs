using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia.Testing
{
	class MalaysiaCredentialLoaderTest : TestCaseWithFactory
	{
		public void TestLoadForGEIRequest_ReturnsEmptyCollection_WhenNoCredentialsInDatabase()
		{
			var polandObjectFactory = new MalaysiaEInvoicingObjectFactory();
			var branch = new TestObjectCreator(Factory).CreateBranchWithCompany("MYWRZ");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			var result = new MalaysiaCredentialLoader().LoadForGEIRequest(branch, batch, polandObjectFactory).ToArray();

			Assert(!result.Any());
		}

		public void TestLoadForGEIRequest_ReturnsPasswords_WhenRegistryCredentialsInDatabase()
		{
			var polandObjectFactory = new MalaysiaEInvoicingObjectFactory();
			var branch = new TestObjectCreator(Factory).CreateBranchWithCompany("MYWRZ");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			var credentialClientId = "Client ID";
			var credentialClientSecret = "ClientSecret";

			var credential = AccountingElectronicMessagingRegistry.Instance.MalaysiaEInvoicingCredentials.Value;
			credential.ClientId = credentialClientId;
			credential.ClientSecret = credentialClientSecret;
			using (AccountingElectronicMessagingRegistry.Instance.MalaysiaEInvoicingCredentials.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, credential))
			{
				var result = new MalaysiaCredentialLoader().LoadForGEIRequest(branch, batch, polandObjectFactory).ToArray();

				AssertEquals("Two credentials should be returned", 2, result.Length);
				AssertContainsExactElementsInAnyOrder("Username and Password should be returned", new[] { CredentialKeys.Username, CredentialKeys.Password }, result.Select(x => x.Key));
				Assert("All credentials should have a value", result.All(x => !x.Value.Value.IsEmpty));

				var tokenNameElement = result.Single(x => x.Key == CredentialKeys.Username).Value;
				Assert("Token Name should not be encrypted", !tokenNameElement.Encrypted);
				var tokenNameContent = tokenNameElement.Value;
				AssertEquals("Token Name content", credentialClientId, tokenNameContent);

				var tokenElement = result.Single(x => x.Key == CredentialKeys.Password).Value;
				Assert("Token should be encrypted", tokenElement.Encrypted);
				var tokenContent = tokenElement.Value;
				AssertNoExceptionThrown("Token should be Base64 encrypted for eHub", () => Convert.FromBase64String(tokenContent));
			}
		}

		public void TestLoadForGEIRequest_ReturnsLatestCertificate_WhenCertificatesInDatabase()
		{
			var malaysiaObjectFactory = new MalaysiaEInvoicingObjectFactory();
			var branch = new TestObjectCreator(Factory).CreateBranchWithCompany("PLWRZ");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var longCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddYears(-1), expiryDate: ZDateTime.Today.AddYears(1)))
			using (var shortCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-30), expiryDate: ZDateTime.Today.AddDays(30)))
			{
				CreateCompanyCredential(branch.Company, malaysiaObjectFactory, shortCert);
				CreateCompanyCredential(branch.Company, malaysiaObjectFactory, longCert);
				Factory.Save();

				var result = new MalaysiaCredentialLoader().LoadForGEIRequest(branch, batch, malaysiaObjectFactory).ToArray();

				AssertEquals("Two credentials should be returned", 2, result.Length);
				AssertContainsExactElementsInAnyOrder("Certificate and Password should be returned", new[] { CredentialKeys.Certificate, CredentialKeys.CertificatePassword }, result.Select(x => x.Key));
				Assert("All credentials should have a value", result.All(x => !x.Value.Value.IsEmpty));

				var certificatePassword = result.First(x => x.Key == CredentialKeys.CertificatePassword).Value;
				Assert("Certificate password should be encrypted", certificatePassword.Encrypted);
				AssertNoExceptionThrown("Certificate password should be Base64 encrypted for eHub", () => Convert.FromBase64String(certificatePassword.Value));

				var certificateContentAsBase64 = result.First(x => x.Key == CredentialKeys.Certificate).Value.Value;
				var certificateAsBinary = Convert.FromBase64String(certificateContentAsBase64);
				using (var actualCert = new X509Certificate2(certificateAsBinary, CertificatePassword.SecurePassword))
				{
					AssertEquals("Certificate is the most recent by issue date", shortCert.Thumbprint, actualCert.Thumbprint);
				}
			}
		}

		GlbCompanyEInvoicingCertificateCredential CreateCompanyCredential(GlbCompany company, ICountryEInvoicingObjectFactory objectFactory, X509Certificate2 cert)
		{
			var credential = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
			credential.CredentialSettings = objectFactory.Credentials;
			credential.GP_GB = ZGuid.Empty;
			credential.GP_GC = company.PK;
			if (cert != null)
			{
				credential.GP_Certificate = cert.Export(X509ContentType.Pfx, CertificatePassword.SecurePassword);
				credential.CurrentDecryptedCertificatePassphrase = CertificatePassword.Password;
			}
			return credential;
		}

		const string CertificateRootName = "CN=Test Root Certificate;O=Some Organization Pty Ltd;C=PL";
		const string CertificateChildName = "CN=Test Certificate;O=Wisetech Global Limited;OU=Accounting Team;L=Alexendria;S=NSW;C=AU";

		static readonly NetworkCredential CertificatePassword = new NetworkCredential("user", "password12345678");
	}
}
