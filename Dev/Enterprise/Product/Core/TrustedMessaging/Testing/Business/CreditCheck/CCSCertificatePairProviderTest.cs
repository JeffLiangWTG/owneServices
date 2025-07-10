using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Business;
using Enterprise.TrustedMessaging.Business.CreditCheck;
using Newtonsoft.Json;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Testing.Business.CreditCheck
{
	public class CCSCertificatePairProviderTest : TestCaseWithFactory
	{
		public void TestGetCertificatePair()
		{
			var serverCert = CCSCertificatePairProviderForTest.NewCertificate();
			var clientCert = CCSCertificatePairProviderForTest.NewCertificate();
			var provider = new CCSCertificatePairProviderForTest(null, string.Empty, string.Empty);

			using (OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serverCert.Export(X509ContentType.Cert)))
			using (WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "@12345"))
			using (WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, clientCert.Export(X509ContentType.Pfx, "@12345")))
			{
				var pair = provider.GetCertificatePair("CCS");
				AssertEquals(serverCert.Thumbprint, pair.RemoteCertificate.Thumbprint);
				AssertEquals(clientCert.Thumbprint, pair.LocalCertificate.Thumbprint);
			}

			var certificateAuthority = new CertificateAuthorityForTest();
			provider = new CCSCertificatePairProviderForTest(certificateAuthority, string.Empty, string.Empty);
			using (OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<byte>()))
			using (WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "@12345"))
			using (WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, clientCert.Export(X509ContentType.Pfx, "@12345")))
			{
				var pair = provider.GetCertificatePair("CCS");
				AssertEquals(certificateAuthority.PublicCertificate.Thumbprint, pair.RemoteCertificate.Thumbprint);
				AssertEquals(clientCert.Thumbprint, pair.LocalCertificate.Thumbprint);
			}
		}

		public void TestGetCertificatePairWhenServerSideThrewException()
		{
			var clientCert = CCSCertificatePairProviderForTest.NewCertificate();
			var certificateAuthority = new CertificateAuthorityThrowExceptionForTest();
			var provider = new CCSCertificatePairProviderForTest(certificateAuthority, "PRDCOD", "SYSID");

			using (OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
			using (WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "@12345"))
			using (WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, clientCert.Export(X509ContentType.Pfx, "@12345")))
			{
				try
				{
					_ = provider.GetCertificatePair("CCS");
				}
				catch (Exception ex)
				{
					AssertContains(@"Failed to retrieve public certificate for product:PRDCOD, system:SYSID.
Is success:False.
No certificate data in response.", ex.Message);
					AssertContains(@"Error Code & Messages:
Code/Message:1234,Test Message1
Code/Message:2345,Test Message2", ex.Message);
					AssertContains(@"Flatten inner exceptions:
Inner exception message: Test Exception
Inner exception stack trace:    at Enterprise.TrustedMessaging.Testing.Business.CreditCheck.CertificateAuthorityThrowExceptionForTest.GetCertificate(CertificateInfo request)", ex.Message);
					AssertContains(@"Inner exception message: Inner Exception
Inner exception stack trace:    at Enterprise.TrustedMessaging.Testing.Business.CreditCheck.CertificateAuthorityThrowExceptionForTest.GetCertificate(CertificateInfo request)", ex.Message);
				}
			}
		}
	}

	#region Implementation

	class CCSCertificatePairProviderForTest : CCSCertificatePairProvider
	{
		public CCSCertificatePairProviderForTest(ICertificateAuthority certificateAuthority, string productCode, string systemId) : base(certificateAuthority, productCode, systemId) { }

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

	class CertificateAuthorityForTest : ICertificateAuthority
	{
		public X509Certificate2 PublicCertificate => certificate ?? (certificate = CCSCertificatePairProviderForTest.NewCertificate());
		X509Certificate2 certificate;

		public Task<TrustedResponse<CertificateResponse>> GetCertificate(CertificateInfo request)
		{
			return Task.FromResult(new TrustedResponse<CertificateResponse>(true,
				JsonConvert.SerializeObject(new CertificateResponse()
				{
					CertificateData = PublicCertificate
						.Export(X509ContentType.Cert)
				})));
		}
	}

	class CertificateAuthorityThrowExceptionForTest : ICertificateAuthority
	{
		public Task<TrustedResponse<CertificateResponse>> GetCertificate(CertificateInfo request)
		{
			try
			{
				try
				{
					throw new Exception("Inner Exception");
				}
				catch (Exception ex)
				{
					throw new Exception("Test Exception", ex);
				}
			}
			catch (Exception ex)
			{
				return Task.FromResult(new TrustedResponse<CertificateResponse>()
				{
					Success = false,
					Messages = new[] { new ErrorMessage { Code = "1234", Message = "Test Message1" }, new ErrorMessage() { Code = "2345", Message = "Test Message2" } },
					InnerException = ex,
					Response = null
				});
			}
		}
	}

	#endregion
}
