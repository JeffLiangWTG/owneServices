using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Schema;
using Moq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public sealed class ObjectFactoryCredentialLoaderTest : PasswordTypeCredentialLoaderTest<ObjectFactoryCredentialLoader>
	{
		public void TestConstructor_SetsProperty_CertificateOrderByColumn()
		{
			var loader = CreateObjectForTest();
			AssertEquals(nameof(loader.CertificateOrderByColumn), GlbExternalPasswordSchema.GP_IssueDate.Name, loader.CertificateOrderByColumn.Name);

			var loader2 = CreateObjectForTest(GlbExternalPasswordSchema.GP_ExpiryDate);
			AssertEquals(nameof(loader.CertificateOrderByColumn), GlbExternalPasswordSchema.GP_ExpiryDate.Name, loader2.CertificateOrderByColumn.Name);

			var loader3 = CreateObjectForTest(GlbExternalPasswordSchema.GP_SystemCreateTimeUtc);
			AssertEquals(nameof(loader.CertificateOrderByColumn), GlbExternalPasswordSchema.GP_SystemCreateTimeUtc.Name, loader3.CertificateOrderByColumn.Name);
		}

		#region Certificate Tests

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenNoCertificates()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();
			Factory.Save();

			var result = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object);
			Assert(!result.Any());
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenCertificateIsExpired()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();

			var issueDate = ZDateTime.Today.AddDays(-30);
			var expiryDate = ZDateTime.Today.AddDays(-7);
			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: issueDate, expiryDate: expiryDate))
			{
				var credential = CreateBranchCredential(branch, countryFactoryMock.Object, cert);
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();
				AssertEquals("No credentials were found as they are expired", 0, results.Length);
			}
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenCertificateIsIssuedInFuture()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();

			var issueDate = ZDateTime.Today.AddDays(30);
			var expiryDate = ZDateTime.Today.AddDays(90);
			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: issueDate, expiryDate: expiryDate))
			{
				var credential = CreateBranchCredential(branch, countryFactoryMock.Object, cert);
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();
				AssertEquals("No credentials were found as they are in the future", 0, results.Length);
			}
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenDifferentPasswordType()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var credential = CreateBranchCredential(branch, countryFactoryMock.Object, cert);
				credential.GP_PasswordType = "ZZZ";
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();
				AssertEquals("No credentials were found as the password type does not match country factory credential config", 0, results.Length);
			}
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenNoCertificateInDatabase()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();
			var credential = CreateBranchCredential(branch, countryFactoryMock.Object, null);
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_IssueDate = ZDateTime.Today.AddDays(-30);
			credential.GP_ExpiryDate = ZDateTime.Today.AddDays(90);
			Factory.Save();

			var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();
			AssertEquals("No credentials were found as there is no certificate content in the database", 0, results.Length);
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenPasswordStatusIsNotValid()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var credential = CreateBranchCredential(branch, countryFactoryMock.Object, cert);
				credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();
				AssertEquals("No credentials were found as the status is not valid", 0, results.Length);
			}
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenDatabaseIsBranchAndConfigIsCompany()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate(enableBranch: false, enableCompany: true);
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var credential = CreateBranchCredential(branch, countryFactoryMock.Object, cert);
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();
				AssertEquals("No credentials were found as the country factory is enabled for company not branch", 0, results.Length);
			}
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenDatabaseIsCompanyAndConfigIsBranch()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate(enableBranch: true, enableCompany: false);
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var credential = CreateCompanyCredential(branch.Company, countryFactoryMock.Object, cert);
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();
				AssertEquals("No credentials were found as the country factory is enabled for branch not company", 0, results.Length);
			}
		}

		public void TestLoadForGEIRequest_ReturnsCertificateAndEncryptedPassword_WhenOneValidCertificate()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();

			var userCredentials = new NetworkCredential("user", "password12345678");
			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var credential = CreateBranchCredential(branch, countryFactoryMock.Object, cert);
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();

				AssertEquals("Two credentials should be returned", 2, results.Length);
				AssertContainsExactElementsInAnyOrder("Certificate and Password should be returned", new[] { CredentialKeys.Certificate, CredentialKeys.CertificatePassword }, results.Select(x => x.Key));
				Assert("All credentials should have a value", results.All(x => !x.Value.Value.IsEmpty));
				var certificatePassword = results.First(x => x.Key == CredentialKeys.CertificatePassword).Value;
				Assert("Certificate password should be encrypted", certificatePassword.Encrypted);

				var certificateContentAsBase64 = results.First(x => x.Key == CredentialKeys.Certificate).Value.Value;
				var certificateAsBinary = Convert.FromBase64String(certificateContentAsBase64);
				using (var actualCert = new X509Certificate2(certificateAsBinary, CertificatePassword.SecurePassword))
				{
					AssertEquals("Certificate should be the same", cert.Thumbprint, actualCert.Thumbprint);
				}
			}
		}

		public void TestLoadForGEIRequest_ReturnsCertificateAndEncryptedPassword_WhenCertificateBase64EncodedTwice()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany("SAABT");  // use SA as the login country which supports base64 encoded twice

			var userCredentials = new NetworkCredential("user", "password12345678");
			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var credential = CreateBranchCredential(branch, countryFactoryMock.Object, cert, true, true);
				Factory.Save();

				var results = CreateObjectForTest(null, true).LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();

				AssertEquals("Two credentials should be returned", 2, results.Length);
				AssertContainsExactElementsInAnyOrder("Certificate and Password should be returned", new[] { CredentialKeys.Certificate, CredentialKeys.CertificatePassword }, results.Select(x => x.Key));
				Assert("All credentials should have a value", results.All(x => !x.Value.Value.IsEmpty));
				var certificatePassword = results.First(x => x.Key == CredentialKeys.CertificatePassword).Value;
				Assert("Certificate password should be encrypted", certificatePassword.Encrypted);

				var certificateContentAsBase64 = results.First(x => x.Key == CredentialKeys.Certificate).Value.Value;
				var certificateAsBinary = Convert.FromBase64String(certificateContentAsBase64);
				using (var actualCert = new X509Certificate2(certificateAsBinary, CertificatePassword.SecurePassword))
				{
					AssertEquals("Certificate should be the same", cert.Thumbprint, actualCert.Thumbprint);
				}
			}
		}

		public void TestLoadForGEIRequest_ReturnsMostRecentCertByIssueDate_WhenManyValidCertificates()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var longCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddYears(-1), expiryDate: ZDateTime.Today.AddYears(1)))
			using (var shortCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-30), expiryDate: ZDateTime.Today.AddDays(30)))
			{
				var longCredential = CreateBranchCredential(branch, countryFactoryMock.Object, longCert);
				var shortCredential = CreateBranchCredential(branch, countryFactoryMock.Object, shortCert);
				Factory.Save();

				var results = CreateObjectForTest(GlbExternalPasswordSchema.GP_IssueDate).LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();

				AssertEquals("Two credentials should be returned", 2, results.Length);
				AssertContainsExactElementsInAnyOrder("Certificate and Password should be returned", new[] { CredentialKeys.Certificate, CredentialKeys.CertificatePassword }, results.Select(x => x.Key));
				Assert("All credentials should have a value", results.All(x => !x.Value.Value.IsEmpty));
				var certificatePassword = results.First(x => x.Key == CredentialKeys.CertificatePassword).Value;
				Assert("Certificate password should be encrypted", certificatePassword.Encrypted);

				var certificateContentAsBase64 = results.First(x => x.Key == CredentialKeys.Certificate).Value.Value;
				var certificateAsBinary = Convert.FromBase64String(certificateContentAsBase64);
				using (var actualCert = new X509Certificate2(certificateAsBinary, CertificatePassword.SecurePassword))
				{
					AssertEquals("Certificate is the most recent by issue date", shortCert.Thumbprint, actualCert.Thumbprint);
				}
			}
		}

		public void TestLoadForGEIRequest_ReturnsMostRecentCertByExpiryDate_WhenManyValidCertificates()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate();
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var longCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddYears(-1), expiryDate: ZDateTime.Today.AddYears(1)))
			using (var shortCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-30), expiryDate: ZDateTime.Today.AddDays(30)))
			{
				var longCredential = CreateBranchCredential(branch, countryFactoryMock.Object, longCert);
				var shortCredential = CreateBranchCredential(branch, countryFactoryMock.Object, shortCert);
				Factory.Save();

				var results = CreateObjectForTest(GlbExternalPasswordSchema.GP_ExpiryDate).LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();

				AssertEquals("Two credentials should be returned", 2, results.Length);
				AssertContainsExactElementsInAnyOrder("Certificate and Password should be returned", new[] { CredentialKeys.Certificate, CredentialKeys.CertificatePassword }, results.Select(x => x.Key));
				Assert("All credentials should have a value", results.All(x => !x.Value.Value.IsEmpty));
				var certificatePassword = results.First(x => x.Key == CredentialKeys.CertificatePassword).Value;
				Assert("Certificate password should be encrypted", certificatePassword.Encrypted);

				var certificateContentAsBase64 = results.First(x => x.Key == CredentialKeys.Certificate).Value.Value;
				var certificateAsBinary = Convert.FromBase64String(certificateContentAsBase64);
				using (var actualCert = new X509Certificate2(certificateAsBinary, CertificatePassword.SecurePassword))
				{
					AssertEquals("Certificate is the longest duration by expiry date", longCert.Thumbprint, actualCert.Thumbprint);
				}
			}
		}

		public void TestLoadForGEIRequest_ReturnsBranchCertificate_WhenBranchAndCompanyAvailable()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate(enableCompany: true, enableBranch: true);
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var companyCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var branchCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var companyCredential = CreateCompanyCredential(branch.Company, countryFactoryMock.Object, companyCert);
				var branchCredential = CreateBranchCredential(branch, countryFactoryMock.Object, branchCert);
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();

				AssertEquals("Two credentials should be returned", 2, results.Length);
				AssertContainsExactElementsInAnyOrder("Certificate and Password should be returned", new[] { CredentialKeys.Certificate, CredentialKeys.CertificatePassword }, results.Select(x => x.Key));
				Assert("All credentials should have a value", results.All(x => !x.Value.Value.IsEmpty));
				var certificatePassword = results.First(x => x.Key == CredentialKeys.CertificatePassword).Value;
				Assert("Certificate password should be encrypted", certificatePassword.Encrypted);

				var certificateContentAsBase64 = results.First(x => x.Key == CredentialKeys.Certificate).Value.Value;
				var certificateAsBinary = Convert.FromBase64String(certificateContentAsBase64);
				using (var actualCert = new X509Certificate2(certificateAsBinary, CertificatePassword.SecurePassword))
				{
					AssertEquals("Certificate is the branch one", branchCert.Thumbprint, actualCert.Thumbprint);
				}
			}
		}

		public void TestLoadForGEIRequest_ReturnsCompanyCertificate_WhenOnlyCompanyAvailable()
		{
			var countryFactoryMock = CreateCountryFactoryMockForCertificate(enableCompany: true, enableBranch: true);
			var branch = CreateBranchAndCompany();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var companyCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				var companyCredential = CreateCompanyCredential(branch.Company, countryFactoryMock.Object, companyCert);
				Factory.Save();

				var results = CreateObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();

				AssertEquals("Two credentials should be returned", 2, results.Length);
				AssertContainsExactElementsInAnyOrder("Certificate and Password should be returned", new[] { CredentialKeys.Certificate, CredentialKeys.CertificatePassword }, results.Select(x => x.Key));
				Assert("All credentials should have a value", results.All(x => !x.Value.Value.IsEmpty));
				var certificatePassword = results.First(x => x.Key == CredentialKeys.CertificatePassword).Value;
				Assert("Certificate password should be encrypted", certificatePassword.Encrypted);

				var certificateContentAsBase64 = results.First(x => x.Key == CredentialKeys.Certificate).Value.Value;
				var certificateAsBinary = Convert.FromBase64String(certificateContentAsBase64);
				using (var actualCert = new X509Certificate2(certificateAsBinary, CertificatePassword.SecurePassword))
				{
					AssertEquals("Certificate is the company one", companyCert.Thumbprint, actualCert.Thumbprint);
				}
			}
		}

		#endregion

		#region Password Tests

		public void TestLoadForGEIRequest_DoesNotThrow_WhenLoadingPasswordCredentials()
		{
			var branch = Factory.NewCompanyAndBranchWith();
			Factory.Save();

			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			var definitions = new List<IEInvoicingPasswordCredentialDefinition>().AsReadOnly();
			var credentialsMock = new Mock<IEInvoicingPasswordCredentialSettings>();
			credentialsMock.Setup(x => x.PasswordDefinitions).Returns(definitions);
			var countryFactory = CreateCountryFactoryMockForPassword(credentialsMock).Object;

			var results = CreateObjectForTest().LoadForGEIRequest(branch, batch, countryFactory);
			AssertEquals("No matching results, expect no exception", 0, results.Count());
		}

		public void TestLoadForGEIRequest_ReturnsPasswordCredential_WhenBranchCredentialsRequired()
		{
			// Arrange
			var definitionMock = new Mock<IEInvoicingPasswordCredentialDefinition>();
			definitionMock.Setup(x => x.UniqueKey).Returns("UniqueKEY123");
			var definitions = new List<IEInvoicingPasswordCredentialDefinition>() { definitionMock.Object }.AsReadOnly();
			var credentialsMock = new Mock<IEInvoicingPasswordCredentialSettings>();
			credentialsMock.Setup(x => x.PasswordDefinitions).Returns(definitions);
			credentialsMock.Setup(x => x.PasswordType).Returns(PasswordTypesList.Codes.EIM);
			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(true);
			var countryFactory = CreateCountryFactoryMockForPassword(credentialsMock).Object;

			var branch = Factory.NewCompanyAndBranchWith();
			var branchCredential = Factory.New<EInvoicingPasswordCredential>();
			branchCredential.CredentialSettings = credentialsMock.Object;
			branchCredential.GP_GB = branch.PK;
			branchCredential.GP_GC = branch.GB_GC;
			branchCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			branchCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			branchCredential.GP_MailBoxID = "UniqueKEY123";

			var wrongBranchCredential = Factory.New<EInvoicingPasswordCredential>();
			wrongBranchCredential.CredentialSettings = credentialsMock.Object;
			wrongBranchCredential.GP_GB = GlbBranch.CurrentBranch.PK;
			wrongBranchCredential.GP_GC = branch.GB_GC;
			wrongBranchCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			wrongBranchCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			wrongBranchCredential.GP_MailBoxID = "UniqueKEY123";
			Factory.Save();

			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			// Act
			var results = CreateObjectForTest().LoadForGEIRequest(branch, batch, countryFactory).ToArray();

			// Assert
			var expectedResultKeys = new[] { "UniqueKEY123.Username", "UniqueKEY123.Password" };
			AssertContainsExactElementsInAnyOrder(expectedResultKeys, results.Select(x => x.Key));

			AssertEquals(false, results.Single(x => x.Key == "UniqueKEY123.Username").Value.Encrypted);
			AssertEquals(true, results.Single(x => x.Key == "UniqueKEY123.Password").Value.Encrypted);
		}

		public void TestLoadForGEIRequest_ReturnsPasswordCredential_WhenCompanyCredentialsRequired()
		{
			// Arrange
			var definitionMock = new Mock<IEInvoicingPasswordCredentialDefinition>();
			definitionMock.Setup(x => x.UniqueKey).Returns("UniqueKEY123");
			var definitions = new List<IEInvoicingPasswordCredentialDefinition>() { definitionMock.Object }.AsReadOnly();
			var credentialsMock = new Mock<IEInvoicingPasswordCredentialSettings>();
			credentialsMock.Setup(x => x.PasswordDefinitions).Returns(definitions);
			credentialsMock.Setup(x => x.PasswordType).Returns(PasswordTypesList.Codes.EIM);
			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			var countryFactory = CreateCountryFactoryMockForPassword(credentialsMock).Object;

			var branch = Factory.NewCompanyAndBranchWith();
			var companyCredential = Factory.New<EInvoicingPasswordCredential>();
			companyCredential.CredentialSettings = credentialsMock.Object;
			companyCredential.GP_GC = branch.GB_GC;
			companyCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			companyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			companyCredential.GP_MailBoxID = "bad mailbox";
			companyCredential.GP_UserID = "Beep Boop";
			Assert("Pre-condition", companyCredential.GP_GB.IsEmpty);

			var credentialWithBranch = Factory.New<EInvoicingPasswordCredential>();
			credentialWithBranch.CredentialSettings = credentialsMock.Object;
			credentialWithBranch.GP_GC = branch.GB_GC;
			credentialWithBranch.GP_GB = branch.PK;
			credentialWithBranch.GP_PasswordType = PasswordTypesList.Codes.EIM;
			credentialWithBranch.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credentialWithBranch.GP_MailBoxID = "UniqueKEY123";

			var matchedCompanyCredential = Factory.New<EInvoicingPasswordCredential>();
			matchedCompanyCredential.CredentialSettings = credentialsMock.Object;
			matchedCompanyCredential.CurrentDecryptedPassword = "SuperSafe";
			matchedCompanyCredential.GP_GB = ZGuid.Empty;
			matchedCompanyCredential.GP_GC = branch.GB_GC;
			matchedCompanyCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			matchedCompanyCredential.GP_MailBoxID = "UniqueKEY123";
			matchedCompanyCredential.GP_UserID = "Beep Boop";
			matchedCompanyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
			AssertEquals("Pre-condition", PasswordStatusList.Codes.Valid, matchedCompanyCredential.GP_PasswordStatus);

			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			// Act
			var results = CreateObjectForTest().LoadForGEIRequest(branch, batch, countryFactory).ToArray();

			// Assert
			var expectedResultKeys = new[] { "UniqueKEY123.Username", "UniqueKEY123.Password" };
			AssertContainsExactElementsInAnyOrder(expectedResultKeys, results.Select(x => x.Key));

			var usernameResult = results.Single(x => x.Key == "UniqueKEY123.Username");
			var passwordResult = results.Single(x => x.Key == "UniqueKEY123.Password");
			AssertEquals(false, usernameResult.Value.Encrypted);
			AssertEquals(true, passwordResult.Value.Encrypted);
			AssertEquals("Beep Boop", usernameResult.Value.Value);
			AssertEquals("SuperSafe", passwordResult.Value.Value);
		}

		#endregion

		#region Implementation

		ObjectFactoryCredentialLoader CreateObjectForTest(SchemaDateTimeColumn certificateOrderByColumn = null, bool isCertBase64EncodedTwice = false)
			=> new ObjectFactoryCredentialLoader(certificateOrderByColumn: certificateOrderByColumn, supportCertificateBase64EncodedTwice: isCertBase64EncodedTwice);

		protected override ObjectFactoryCredentialLoader CreateDefaultObjectForTest() => CreateObjectForTest();

		GlbBranchEInvoicingCertificateCredential CreateBranchCredential(GlbBranch branch, ICountryEInvoicingObjectFactory objectFactory, X509Certificate2 cert, bool useBranchCompany = false, bool useBase64EncodedTwice = false)
		{
			var credential = Factory.New<GlbBranchEInvoicingCertificateCredential>();
			credential.CredentialSettings = objectFactory.Credentials;
			credential.GP_GB = branch.PK;
			credential.GP_GC = useBranchCompany ? branch.GB_GC : ZGuid.Empty;
			if (cert != null)
			{
				if (!useBase64EncodedTwice)
				{
					credential.GP_Certificate = cert.Export(X509ContentType.Pfx, CertificatePassword.SecurePassword);
				}
				else
				{
					var bytes = cert.Export(X509ContentType.Cert, CertificatePassword.SecurePassword);  // use Cert instead of pfx, please note SA certificate is not in pfx format.
					var encodedStr = Convert.ToBase64String(bytes);
					var encodedBytes = System.Text.Encoding.UTF8.GetBytes(encodedStr);

					//base64 encode twice
					var encodedStrTwice = Convert.ToBase64String(encodedBytes);
					var encodedBytesTwice = System.Text.Encoding.UTF8.GetBytes(encodedStrTwice);
					credential.GP_Certificate = encodedBytesTwice;
				}
				credential.CurrentDecryptedCertificatePassphrase = CertificatePassword.Password;
			}
			return credential;
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

		Mock<ICountryEInvoicingObjectFactory> CreateCountryFactoryMockForCertificate(bool enableCompany = false, bool enableBranch = true, string passwordType = PasswordTypesList.Codes.EIM)
		{
			var credentialsMock = new Mock<IEInvoicingCertificateCredentialSettings>();
			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(enableBranch);
			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(enableCompany);
			credentialsMock.Setup(x => x.PasswordType).Returns(passwordType);

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.Credentials).Returns(credentialsMock.Object);
			return countryFactoryMock;
		}

		Mock<ICountryEInvoicingObjectFactory> CreateCountryFactoryMockForPassword(Mock<IEInvoicingPasswordCredentialSettings> credentialsMock)
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.Credentials).Returns(credentialsMock.Object);
			return countryFactoryMock;
		}

		const string CertificateRootName = "CN = VMS ICA1 Staging;O = FRCS;C = FJ";
		const string CertificateChildName = "CN = PTY7 Wisetech Global Limited;OU = Wisetech Global Limited;O = Wisetech Global Limited;STREET = 72 O'Riodran St Post Code 2015;L = Alexendria NSW;S = UNKNOWN;C = AU";

		static readonly NetworkCredential CertificatePassword = new NetworkCredential("user", "password12345678");

		#endregion
	}
}
