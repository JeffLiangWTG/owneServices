using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class TestEInvoicingCertificateCredentialCreator : IDisposable
	{
		public TestEInvoicingCertificateCredentialCreator(GlbBranch branch, ZDateTime issueDate, ZDateTime expiryDate)
		{
			Branch = branch;
			IssueDate = issueDate;
			ExpiryDate = expiryDate;
		}
		public GlbBranch Branch { get; }

		public ZDateTime IssueDate { get; }

		public ZDateTime ExpiryDate { get; }

		public EInvoicingCertificateCredential CreateCertificateCredential(string userName = "user", string password = "password12345678", bool saveToDB = false, bool base64EncodedTwice = false)
		{
			var userCredentials = new NetworkCredential(userName, password);
			byte[] certificateData;
			if (!base64EncodedTwice)
			{
				certificateData = ChildCertificate.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
			}
			else
			{
				var rawData = ChildCertificate.Export(X509ContentType.Cert, userCredentials.SecurePassword);
				var encodedStr = Convert.ToBase64String(rawData);
				var encodedBytes = System.Text.Encoding.UTF8.GetBytes(encodedStr);

				//base64 encode twice
				var encodedStrTwice = Convert.ToBase64String(encodedBytes);
				var encodedBytesTwice = System.Text.Encoding.UTF8.GetBytes(encodedStrTwice);
				certificateData = encodedBytesTwice;
			}
			var certificateCredential = Branch.Factory.NewWithValidTestData<GlbBranchEInvoicingCertificateCredential>();
			certificateCredential.GP_GB = Branch.PK;
			certificateCredential.GP_GC = Branch.Company.PK;
			certificateCredential.GP_Certificate = certificateData;
			certificateCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
			certificateCredential.GP_MailBoxID = ZString.Empty;
			certificateCredential.GP_PasswordType = PasswordTypesList.Codes.FPC;
			if (saveToDB)
			{
				Branch.Factory.Save();
			}
			return certificateCredential;
		}

		public void Dispose()
		{
			rootCertificate?.Dispose();
			childCertificate?.Dispose();
		}

		X509Certificate2 RootCertificate => rootCertificate ?? (rootCertificate = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("CN=Root"), IssueDate.AddYears(-1), ExpiryDate.AddYears(5)));
		X509Certificate2 rootCertificate;

		X509Certificate2 ChildCertificate => childCertificate ?? (childCertificate = SampleCertificateHelper.CreateSampleChildCertificate(RootCertificate, new X500DistinguishedName("CN=Child"), IssueDate, ExpiryDate));
		X509Certificate2 childCertificate;
	}
}
