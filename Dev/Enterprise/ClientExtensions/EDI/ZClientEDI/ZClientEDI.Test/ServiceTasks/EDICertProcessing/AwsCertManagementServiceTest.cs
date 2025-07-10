using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Amazon.ACMPCA.Model;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ServiceTasks.EDICertProcessing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Moq;
using Moq.Protected;
using WTG.AWSCertificateIntegration;

namespace ZClientEDI.Test.ServiceTasks.EDICertProcessing.Test
{
	public class AwsCertManagementServiceTest : TestCaseWithFactory
	{
		public void TestIssueCertificateSuccess()
		{
			var ediIdentityCertificate = CreateIdentityCertificate();
			var certificate = awsCertManagementService.IssueCertificate(ediIdentityCertificate);

			CombineAssertions(() =>
			{
				AssertNotNull(certificate);
				AssertContains("CN=NJG1", certificate.Subject);
				AssertContains("CN=NJG1", certificate.Issuer);
				AssertEquals("0153A7C4B112DE00D8E50756C5249D5789C64B03", certificate.Thumbprint);

				AssertEquals(new DateTime(2023, 2, 24, 2, 31, 59), certificate.NotBefore.ToUniversalTime());
				AssertEquals(new DateTime(2024, 2, 24, 3, 31, 59), certificate.NotAfter.ToUniversalTime());
			});
		}

		public void TestIssueCertificateFailed()
		{
			var ediIdentityCertificate = CreateIdentityCertificate();

			mockAwsPcaManager.Protected().Setup<string>("IssueCertificate", ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None)
				.Throws(new InvalidOperationException("Get certificate failed"));

			var exception = AssertExceptionThrown<AwsCertManagementServiceException>(() => awsCertManagementService.IssueCertificate(ediIdentityCertificate));

			CombineAssertions(() =>
			{
				AssertEquals("Issue Certificate Error", exception.Message);
				AssertEquals(3, exception.InnerExceptions.Count);

				foreach (var innerEx in exception.InnerExceptions)
				{
					AssertEquals(typeof(InvalidOperationException), innerEx.GetType());
					AssertEquals("Get certificate failed", innerEx.Message);
				}

				mockAwsPcaManager.Protected().Verify("IssueCertificate", Times.Exactly(3), ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None);
			});
		}

		public void TestRevokeCertificate()
		{
			var issuedCertificate = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
			var ediIdentityCertificate = CreateIdentityCertificate();
			ediIdentityCertificate.ICE_IsCertificateRevoked = false;
			ediIdentityCertificate.ICE_CertificateData = issuedCertificate.RawData;
			var isCertificateRevoked = awsCertManagementService.RevokeCertificate(ediIdentityCertificate);
			AssertEquals(true, isCertificateRevoked);

			ediIdentityCertificate.ICE_IsCertificateRevoked = true;
			isCertificateRevoked = awsCertManagementService.RevokeCertificate(ediIdentityCertificate);
			AssertEquals(true, isCertificateRevoked);
		}

		public void TestRevokeCertificateFailed()
		{
			var issuedCertificate = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
			var ediIdentityCertificate = CreateIdentityCertificate();
			ediIdentityCertificate.ICE_IsCertificateRevoked = false;
			ediIdentityCertificate.ICE_CertificateData = issuedCertificate.RawData;

			mockAwsPcaManager.Protected().Setup<RevokeCertificateResponse>("RevokeCertificate", ItExpr.IsAny<RevokeCertificateRequest>(), CancellationToken.None).Throws(new InvalidOperationException("Revoke certificate failed"));

			var exception = AssertExceptionThrown<AwsCertManagementServiceException>(() => awsCertManagementService.RevokeCertificate(ediIdentityCertificate));
			CombineAssertions(() =>
			{
				AssertEquals("Revoke Certificate Error", exception.Message);
				AssertEquals(3, exception.InnerExceptions.Count);

				foreach (var innerEx in exception.InnerExceptions)
				{
					AssertEquals(typeof(InvalidOperationException), innerEx.GetType());
					AssertEquals("Revoke certificate failed", innerEx.Message);
				}

				mockAwsPcaManager.Protected().Verify("RevokeCertificate", Times.Exactly(3), ItExpr.IsAny<RevokeCertificateRequest>(), CancellationToken.None);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			var collection = new AWSPrivateCACollection();
			var awsPrivateCaArn = collection.AddNew();
			awsPrivateCaArn.IsEnabled = true;
			awsPrivateCaArn.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			awsPrivateCaArn.Arn = Arn;
			awsPrivateCaArn.AccessKey = AccessKey;
			awsPrivateCaArn.SecretKey = SecretKey;

			certProcessingNotificationGroupOverride = EDIDataRegistry.Instance.CertProcessingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			awsPrivateCAListManagerOverride = EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var revokeCertificateResponse = new RevokeCertificateResponse()
			{
				ContentLength = 3,
				ResponseMetadata = null,
				HttpStatusCode = HttpStatusCode.OK
			};

			mockAwsPcaManager = new Mock<AwsPcaManager>(AccessKey, SecretKey, "ap-southeast-2");
			mockAwsPcaManager.Protected().Setup<string>("IssueCertificate", ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None).Returns(Arn);
			mockAwsPcaManager.Protected().Setup<string>("GetCertificate", ItExpr.IsAny<GetCertificateRequest>(), CancellationToken.None).Returns(Certificate);
			mockAwsPcaManager.Protected().Setup<RevokeCertificateResponse>("RevokeCertificate", ItExpr.IsAny<RevokeCertificateRequest>(), CancellationToken.None).Returns(revokeCertificateResponse);

			var caRootDictionary = new Dictionary<string, (AwsPcaManager mockAwsPcaManager, string ARN)>
			{
				{ CARootCodeDescriptionList.Codes.SystemToSystemTrust, (mockAwsPcaManager.Object, Arn) }
			};

			awsCertManagementService = new AwsCertManagementService(caRootDictionary);
		}

		protected override void TearDown()
		{
			certProcessingNotificationGroupOverride.Dispose();
			awsPrivateCAListManagerOverride.Dispose();
		}

		EdiIdentityCertificate CreateIdentityCertificate()
		{
			var ediIdentityCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediIdentityCertificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			return ediIdentityCertificate;
		}

		IDisposable certProcessingNotificationGroupOverride;
		IDisposable awsPrivateCAListManagerOverride;

		Mock<AwsPcaManager> mockAwsPcaManager;
		AwsCertManagementService awsCertManagementService;

		const string AccessKey = "GFDGSDGDFSGDFGDFGDFG";

		const string SecretKey = "pik4+se9HK6aoBDfb4nl1z3S1xqfJ+dSDGSDGfgg";

		const string Arn = "arn:aws:acm-pca:ap-southeast-2:079973481859:certificate-authority/84dc84fe-e734-4281-84e3-45fdgfdfg";

		const string Certificate = @"-----BEGIN CERTIFICATE-----
MIID6DCCAtCgAwIBAgIQROOIIrCQ7LRi0hKk4rLEdDANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwMjI0MDIzMTU5WhcNMjQwMjI0MDMzMTU5
WjBxMQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5K
RzExEDAOBgNVBAcMB1dURyBOSkcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQCVJt0UDpMaAOMxUiBtstzgTVtBC541t5+mGCS8wOmOdMCspwU1jkC6w0VB
sh0ZwFkGJyu51aEOkqES2oTR/G7/ISj7iZO2Hx4C7dlShm31gDlp2sgNkWls+Acg
HsAvu1GpHEXQjMv1gtPZr5uC3ys9uY5zm5XyirCno4+AEzntZ9Bxvupu1cmuS9Z7
xFdFXuBq4pmb7s6vGp3bMMabEimvlRkg1EiaIJLlLVxbb4til3jmh+wkf6o3R2KS
V65+f4F6XZvA5vsB7rUv7HDvuSNNGVdtFSQ9RxnzKOSyemypSM+CPQyDNcWJZSjd
51o58wEEv7jQrvu5NmeggXMiFDR3AgMBAAGjfDB6MAkGA1UdEwQCMAAwHwYDVR0j
BBgwFoAUxRkU5BydYIvZdbTEsI6oOFnvAuIwHQYDVR0OBBYEFMUZFOQcnWCL2XW0
xLCOqDhZ7wLiMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAQYI
KwYBBQUHAwIwDQYJKoZIhvcNAQELBQADggEBAFd9ujf8VlE2UxAqjTmaAddX/FKU
NHSWILGSjOZm6Lb2nz0267Al8G71NiLdwDAPEH7sBwKUIXZBKoLJU9pBxSshxkf0
lX+4bKUvHkiDf/GDg9X1RqX7sVgglPaR3FmQNxEvbs6lD+rWar6ZtHOX2Kaqrb/7
szao5Hnoo7U8CL4Lm0ZDZ+nxP9gwq3W83KDLsGHHNld0i9zl55Lzt1vCoUsqJlsJ
XPyDaHxT4m7lrI+fv8HAC2KclNkEf9xYTqDfwHpbWKQtV54gYG568D+kyptHDsgv
7qiJtBmvKf+O4g7p9NPjcQdfoHq2IrKzZO9kOANlcVE0UCXBk4Msi4VguJ0=
-----END CERTIFICATE-----";
	}
}
