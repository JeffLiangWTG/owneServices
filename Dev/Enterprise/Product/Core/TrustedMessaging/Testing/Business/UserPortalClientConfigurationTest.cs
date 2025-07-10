using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Business;

namespace Enterprise.TrustedMessaging.Testing
{
	public class UserPortalClientConfigurationTest : TestCaseWithFactory
	{
		public void TestImplements()
		{
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.cw1.com/user_portal/");

			var serverCert = NewCertificate();
			var clientCert = NewCertificate();

			WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, serverCert.Export(X509ContentType.Cert));
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientCert.Export(X509ContentType.Pfx, "@12345"));
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "@12345");

			var config = new UserPortalClientConfiguration();
			AssertEquals(5000, config.RetryIntervalInMilliseconds);
			AssertEquals(3, config.MaxRetryCount);
			AssertEquals("https://www.cw1.com/user_portal/api", config.RemoteEndpointRootUrl);

			var pair = config.CertificatePairProvider.GetCertificatePair("CW1");
			AssertEquals(serverCert.Thumbprint, pair.RemoteCertificate.Thumbprint);
			AssertEquals(clientCert.Thumbprint, pair.LocalCertificate.Thumbprint);

			config.SecretKeyStorage.SaveSecretKey(new WTG.TrustedMessaging.Models.SecretKey() { Product = "CW1", RefId = "Ref#", Key = "key123" });
			AssertEquals("key123", config.SecretKeyStorage.LoadSecretKey("CW1", "Ref#1").Key);
			AssertEquals("key123", WebDataRegistry.Instance.TrustedMessagingSecretKey.Value);
		}

		public void TestErrorReporter()
		{
			WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2, 3 });
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2, 3 });
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "@12345");

			var config = new UserPortalClientConfiguration();
			AssertEquals(5000, config.RetryIntervalInMilliseconds);
			AssertEquals(3, config.MaxRetryCount);
			var pair = config.CertificatePairProvider.GetCertificatePair("CW1");
			AssertNull(pair.RemoteCertificate);
			AssertNull(pair.LocalCertificate);
#if NETFRAMEWORK
			AssertEquals("Trusted Messaging Error - Cannot find the requested object.\r\n", ErrorReporter.LastKeyReported);
#elif NET
			AssertEquals("Trusted Messaging Error - Cannot find the requested object.", ErrorReporter.LastKeyReported);
#endif
			AssertContains("Cannot find the requested object.", ErrorReporter.LastMessageReported);
			AssertContains("Cannot find the requested object.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public static X509Certificate2 NewCertificate()
		{
			using (var rsa = RSA.Create(2048))
			{
				var subjectName = Guid.NewGuid().ToString();
				var req = new CertificateRequest($"cn={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
				return cert;
			}
		}
	}
}
