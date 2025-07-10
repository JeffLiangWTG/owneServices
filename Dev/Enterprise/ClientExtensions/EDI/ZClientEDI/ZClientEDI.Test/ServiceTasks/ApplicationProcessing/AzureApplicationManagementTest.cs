using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;
using Moq;
using NUnit.Framework;
using WTG.AzureApplicationIntegration;

namespace ZClientEDI.Test.ServiceTasks.AzureApplicationProcessing.Test
{
	public class AzureApplicationManagementTest : TestCaseWithFactory
	{
		public void TestCreateAzureApplicationManagement()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new AzureApplicationManagement("", ""));
				AssertExceptionThrown<ArgumentNullException>(() => new AzureApplicationManagement("", "graphClientId"));
				AssertExceptionThrown<ArgumentNullException>(() => new AzureApplicationManagement("tenantId", ""));
				AssertNoExceptionThrown(() => new AzureApplicationManagement("tenantId", "graphClientId"));
			});
		}

		public void TestCreateApplication()
		{
			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.CreateApplicationAsync(It.IsAny<string>())).ReturnsAsync(AppId);

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			var appId = azureApplicationManagement.CreateApplication("E000001.CW1.F2");

			AssertEquals(appId, AppId);
			graphServiceMock.Verify(g => g.CreateApplicationAsync("E000001.CW1.F2"), Times.Exactly(1));
			graphServiceMock.VerifyNoOtherCalls();
		}

		public void TestCreateApplication_Failed()
		{
			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.CreateApplicationAsync(It.IsAny<string>()))
				.Throws(new InvalidOperationException("Create application failed"));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			var exception = AssertExceptionThrown<AzureApplicationManagementException>(() => azureApplicationManagement.CreateApplication("E000001.CW1.F2"));

			AssertEquals("Create Application Error", exception.Message);
			AssertEquals(3, exception.InnerExceptions.Count);
			foreach (var item in exception.InnerExceptions)
			{
				AssertEquals(typeof(InvalidOperationException), item.GetType());
				AssertEquals("Create application failed", item.Message);
			}
			graphServiceMock.Verify(g => g.CreateApplicationAsync("E000001.CW1.F2"), Times.Exactly(3));
			graphServiceMock.VerifyNoOtherCalls();
		}

		public void TestAddCertificates()
		{
			var issuedCertificate1 = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
			var issuedCertificate2 = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate2));

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.AddCertificatesToApplicationAsync(It.IsAny<string>(), It.IsAny<X509Certificate2[]>()));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			AssertNoExceptionThrown(() => azureApplicationManagement.AddApplicationCertificates(AppId, issuedCertificate1, issuedCertificate2));
			graphServiceMock.Verify(g => g.AddCertificatesToApplicationAsync(It.IsAny<string>(), It.IsAny<X509Certificate2[]>()), Times.Exactly(1));
			graphServiceMock.VerifyNoOtherCalls();
		}

		public void TestAddCertificates_Failed()
		{
			var issuedCertificate = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a =>
				a.AddCertificatesToApplicationAsync(It.IsAny<string>(), It.IsAny<X509Certificate2[]>())).Throws(new InvalidOperationException("Add certificates failed"));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			var exception = AssertExceptionThrown<AzureApplicationManagementException>(() => azureApplicationManagement.AddApplicationCertificates(AppId, issuedCertificate));

			AssertEquals("Add Certificates Error", exception.Message);
			AssertEquals(3, exception.InnerExceptions.Count);
			foreach (var item in exception.InnerExceptions)
			{
				AssertEquals(typeof(InvalidOperationException), item.GetType());
				AssertEquals("Add certificates failed", item.Message);
			}

			graphServiceMock.Verify(g => g.AddCertificatesToApplicationAsync(AppId, It.IsAny<X509Certificate2[]>()), Times.Exactly(3));
			graphServiceMock.VerifyNoOtherCalls();
		}

		public void TestRemoveCertificatesFromApplication()
		{
			var cert1 = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
			var thumbprint1 = cert1.Thumbprint;

			var cert2 = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate2));
			var thumbprint2 = cert2.Thumbprint;

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.RemoveCertificatesFromApplicationAsync(It.IsAny<string>(), It.IsAny<string[]>()));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			AssertNoExceptionThrown(() => azureApplicationManagement.RemoveCertificatesFromApplication(AppId, thumbprint1, thumbprint2));
			graphServiceMock.Verify(g => g.RemoveCertificatesFromApplicationAsync(AppId, It.IsAny<string[]>()), Times.Exactly(1));
			graphServiceMock.VerifyNoOtherCalls();
		}

		public void TestRemoveCertificatesFromApplication_Failed()
		{
			var issuedCertificate = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock
				.Setup(g => g.RemoveCertificatesFromApplicationAsync(It.IsAny<string>(), It.IsAny<string[]>()))
				.Throws(new InvalidOperationException("Remove certificates failed"));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			AssertExceptionThrown<AzureApplicationManagementException>("Remove certificates failed", () => azureApplicationManagement.RemoveCertificatesFromApplication("", issuedCertificate.Thumbprint));
			graphServiceMock.Verify(g => g.RemoveCertificatesFromApplicationAsync("", It.IsAny<string[]>()), Times.Exactly(3));

			graphServiceMock
				.Setup(g => g.RemoveCertificatesFromApplicationAsync(It.IsAny<string>(), It.IsAny<string[]>()))
				.Throws(new InvalidOperationException("Remove certificates failed"));

			var exception = AssertExceptionThrown<AzureApplicationManagementException>(() => azureApplicationManagement.RemoveCertificatesFromApplication(AppId, issuedCertificate.Thumbprint));

			AssertEquals("Remove Certificates Error", exception.Message);
			AssertEquals(3, exception.InnerExceptions.Count);
			foreach (var item in exception.InnerExceptions)
			{
				AssertEquals(typeof(InvalidOperationException), item.GetType());
				AssertEquals("Remove certificates failed", item.Message);
			}

			graphServiceMock.Verify(g => g.RemoveCertificatesFromApplicationAsync(AppId, It.IsAny<string[]>()), Times.Exactly(3));
			graphServiceMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestOverrideRedirectUrls()
		{
			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a =>
				a.OverrideRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			azureApplicationManagement.OverrideRedirectUrl(AppId, new[] { "https://web.com" }, new[] { "https://SinglePage.com" }, new[] { "https://InstalledClient.com" });

			graphServiceMock.Verify(g =>
				g.OverrideRedirectUrlsAsync(AppId, new[] { "https://web.com" }, new[] { "https://SinglePage.com" }, new[] { "https://InstalledClient.com" }),
				Times.Exactly(1));
			graphServiceMock.VerifyNoOtherCalls();
		}

		public void TestOverrideRedirectUrls_HandleError()
		{
			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.OverrideRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<string[]>(),
				It.IsAny<string[]>(), It.IsAny<string[]>())).Throws(new InvalidOperationException("Mock an exception"));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);

			var exception = AssertExceptionThrown<AzureApplicationManagementException>(() =>
				azureApplicationManagement.OverrideRedirectUrl(AppId, new[] { "https://web.com" }, new[] { "https://SinglePage.com" }, new[] { "https://InstalledClient.com" }));

			AssertEquals("Override Redirect Url Error", exception.Message);
			AssertEquals(3, exception.InnerExceptions.Count);
			foreach (var item in exception.InnerExceptions)
			{
				AssertEquals(typeof(InvalidOperationException), item.GetType());
				AssertEquals("Mock an exception", item.Message);
			}

			graphServiceMock.Verify(g => g.OverrideRedirectUrlsAsync(AppId, It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()), Times.Exactly(3));
			graphServiceMock.VerifyNoOtherCalls();
		}

		public void TestGetGithubActionSecretExpirationDate()
		{
			var clientId = Guid.NewGuid().ToString();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.GetMaxExpirationDateAsync(It.IsAny<string>())).Returns(Task.FromResult(new DateTimeOffset?()));

			var azureApplicationManagement = new AzureApplicationManagementForTest(graphServiceMock.Object);
			var expirationDate = azureApplicationManagement.GetApplicationSecretExpirationDate(clientId);
			AssertEquals("If there is no password credential, it should return DateTime.MinValue.", DateTime.MinValue, expirationDate);

			var expectedDate = new DateTimeOffset(2024, 6, 6, 6, 6, 6, TimeSpan.Zero);
			graphServiceMock.Setup(a => a.GetMaxExpirationDateAsync(It.IsAny<string>())).ReturnsAsync(expectedDate);
			expirationDate = azureApplicationManagement.GetApplicationSecretExpirationDate(clientId);
			Assert("Should return date from PasswordCredentials", expectedDate.Equals(expirationDate));
		}

		public string AppId = Guid.NewGuid().ToString();

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

		const string Certificate2 = @"-----BEGIN CERTIFICATE-----
MIID2jCCAsKgAwIBAgIQR0RemLM7Lo27g7CJGUVSRzANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwNTA4MDYxNDAzWhcNMjQwNTA3MDcxNDAz
WjBjMQswCQYDVQQGEwJBVTEMMAoGA1UECAwDU1lEMQwwCgYDVQQHDANTWUQxDDAK
BgNVBAoMA1dURzEMMAoGA1UECwwDV1RHMRwwGgYDVQQDDBN3aXNldGVjaC5nbG9i
YWwuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA57/KUGd2YHrb
Y1iViSXwGCWe8UsvxxPPsNA5DlzjH1pAy6OJb5dSC8SPInr5xQCvwf360if3XrkS
CJkvJlx9RhcHgpnQQfLiuvVIHfbZQHMRL7+Oq4APzdNXknXBMSdUjjc9eSBjpxRh
DNqfLxHcJlSzGZZNJCjLnjnL3qtK5WBNgju7g+ZPufMKBRfAciNlAJQWMK/CLFCt
tllH9wuFBR8IYY6orzffJ1nocd0wle5lcNg8ihgsGSoqgMcR32ZU3l+MNE8qDcLi
lV2OaW0alou2K3rd3pq8WOtZTgGGMOHaTEOBAXqgXZQbzguiXEe6ijoLBgwmzkgr
Hqw6JgGefQIDAQABo3wwejAJBgNVHRMEAjAAMB8GA1UdIwQYMBaAFMUZFOQcnWCL
2XW0xLCOqDhZ7wLiMB0GA1UdDgQWBBRgVXKxahXMIirYYm0846gHnJeY5jAOBgNV
HQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMA0GCSqG
SIb3DQEBCwUAA4IBAQBcRs+X0iH/K5dOTQkZ5/v13huLOXb3QFExTJW2+tKmRYe1
e3CoQVE6LQJ+cCQ+Tl6UmbyeyTIqEuFpTmm6Yhl9agu41tlgiobP1+YQ/VMjasgh
Vhgr34KA09iVzpLsIlROdNW5Q5rfjRh1WuBAEPcABKKNFgaqVvS2BKzd/a6aXaId
Zu+YuRV232OXOUZP07DkaLhax6wfSf+tfkNLQLvoVNcJmcF6mNgzg2HULuvia77u
HPpykW02IbnSAr3jDVGHezVNLctFrpHpCWRlTUMulW56xc74ZiONSf+N/2WZJo0x
wNakFhJRZGZJpKCx8xrIO4D9y7jt0WGP7ZCvcXeQ
-----END CERTIFICATE-----";
	}

	class AzureApplicationManagementForTest : AzureApplicationManagement
	{
		readonly IGraphService graphService;
		public AzureApplicationManagementForTest(IGraphService graphService) : base("tenantId", "graphClientId")
		{
			this.graphService = graphService;
		}

		protected override IGraphService GetGraphService()
		{
			return graphService;
		}
	}
}

