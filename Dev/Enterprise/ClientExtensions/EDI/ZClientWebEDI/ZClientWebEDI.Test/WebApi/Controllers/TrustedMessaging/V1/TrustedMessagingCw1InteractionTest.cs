using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[UseSnapshotProtection]
	[HttpContextEnabledTest]
	class TrustedMessagingCw1InteractionTest : TestCase
	{
		[SnailTest]
		public void TestCw1Interactions()
		{
			var central1 = CreateCert();
			var client1 = CreateCert();
			var key1 = CreateKey();
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 8000;
				reg.KeyForTest.PasswordForTest = "123";
				foreach (var clientSideCerts in new[] { (Central: central1, Client: client1) })
				{
					foreach (var clientSideKey in new[] { key1 })
					{
						foreach (var serverSideCentralCert in new[] { central1 })
						{
							foreach (var serverSideClientCert in new[] { client1 })
							{
								foreach (var serverSideKey in new[] { key1 })
								{
									foreach (var keyExpiryUtc in new[] { ZDateTime.Empty, ZDateTime.UtcNow.AddDays(-10), ZDateTime.UtcNow.AddDays(10) })
									{
										SetupSystems(clientSideCerts.Central, clientSideCerts.Client, clientSideKey, serverSideCentralCert, serverSideClientCert, serverSideKey, keyExpiryUtc);
										var expectedSuccess = clientSideCerts.Central != null && clientSideCerts.Client != null && serverSideCentralCert != null && serverSideClientCert != null && clientSideCerts.Central == serverSideCentralCert && clientSideCerts.Client == serverSideClientCert;
										AssertInteraction(expectedSuccess);
									}
								}
							}
						}
					}
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		[SnailTest]
		public void TestCw1Interactions_NewClientSystem()
		{
			var central1 = CreateCert();
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 8000;
				reg.KeyForTest.PasswordForTest = "123";
				foreach (var clientSideCerts in new[] { (Central: default(X509Certificate2), Client: default(X509Certificate2))  })
				{
					foreach (var clientSideKey in new[] { default(byte[]) })
					{
						foreach (var serverSideCentralCert in new[] { central1 })
						{
							foreach (var serverSideClientCert in new[] { default(X509Certificate2) })
							{
								foreach (var serverSideKey in new[] { default(byte[]) })
								{
									foreach (var keyExpiryUtc in new[] { ZDateTime.Empty, ZDateTime.UtcNow.AddDays(-10), ZDateTime.UtcNow.AddDays(10) })
									{
										SetupSystems(clientSideCerts.Central, clientSideCerts.Client, clientSideKey, serverSideCentralCert, serverSideClientCert, serverSideKey, keyExpiryUtc);
										var expectedSuccess = clientSideCerts.Central != null && clientSideCerts.Client != null && serverSideCentralCert != null && serverSideClientCert != null && clientSideCerts.Central == serverSideCentralCert && clientSideCerts.Client == serverSideClientCert;
										AssertInteraction(expectedSuccess);
									}
								}
							}
						}
					}
				}
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		void SetupSystems(X509Certificate2 clientSideCentralCert, X509Certificate2 clientSideClientCert, byte[] clientSideClientSecretKey, X509Certificate2 serverSideCentralCert, X509Certificate2 serverSideClientCert, byte[] serverSideClientSecretKey, ZDateTime serverSideClientSecretKeyExpiryUtc)
		{
			WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientSideCentralCert?.Export(X509ContentType.Cert) ?? Array.Empty<byte>());
			var clientSidePassword = Guid.NewGuid().ToString();
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientSideClientCert?.Export(X509ContentType.Pfx, clientSidePassword) ?? Array.Empty<byte>());
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientSideClientCert != null ? clientSidePassword : "");
			WebDataRegistry.Instance.TrustedMessagingSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientSideClientSecretKey != null ? Convert.ToBase64String(clientSideClientSecretKey) : "");
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var centralSystemConfig = factory.Load<EdiTrustedMessagingConfig>(CentralSystemConfigPK);
			var serverSidePassword = Guid.NewGuid().ToString();
			centralSystemConfig.ETM_CertificateData = serverSideCentralCert?.Export(X509ContentType.Pfx, serverSidePassword) ?? Array.Empty<byte>();
			centralSystemConfig.ETM_CertificatePassword = serverSideCentralCert != null ? serverSidePassword : "";
			var database = factory.Load<LicenceDatabase>(DatabasePK);
			if (serverSideClientCert == null)
			{
				var system = database.GetOrCreateTrustedSystem();
				var serverSideClientConfig = database.GetOrCreateTrustedSystem().GetOrCreateCertificateConfig();
				serverSideClientConfig.ETM_CertificateData = Array.Empty<byte>();
				system.ETS_SecretKey = Array.Empty<byte>();
				system.ETS_SecretKeyExpiryUtc = ZDateTime.Empty;
			}
			else
			{
				var system = database.GetOrCreateTrustedSystem();
				var serverSideClientConfig = database.GetOrCreateTrustedSystem().GetOrCreateCertificateConfig();
				serverSideClientConfig.ETM_CertificateData = serverSideClientCert.Export(X509ContentType.Cert);
				system.ETS_SecretKey = serverSideClientSecretKey ?? Array.Empty<byte>();
				system.ETS_SecretKeyExpiryUtc = serverSideClientSecretKeyExpiryUtc;
			}

			factory.Save();
		}

		void AssertInteraction(bool expectedSuccess)
		{
			using (var rsa = new RSACryptoServiceProvider(4096))
			{
				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var client = new UserPortalClientForTest(rsa);
				if (expectedSuccess)
				{
					AssertNotEquals(0, WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value.Length);
					AssertNotEquals(0, WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value.Length);
				}

				var info = new UserAgreementInfo()
				{
					Product = "CW1",
					SystemId = client.DatabaseNumber,
					UserId = "TU1",
					FullName = "User1",
					Email = "user1@cw1.com",
					UserCountry = "AU",
					InfoExpires = ZDateTime.UtcNow.AddDays(1).ToDateTime(),
					UserAgreementType = "MYA",
					ShouldSendAgreementCopy = false
				};

				var rsp = Task.Run(() => client.GetUserAgreementAsync(info)).GetAwaiter().GetResult();
				AssertEquals("Successful call", expectedSuccess, rsp.Success);
				if (rsp.Success)
				{
					AssertEquals(false, rsp.Response.Required);
					var config = new UserPortalClientConfigurationForTest();
					var certs = config.CertificatePairProvider.GetCertificatePair("CW1");
					AssertNotNull(certs.LocalCertificate);
					AssertNotNull(certs.RemoteCertificate);
					AssertNotEquals("", config.SecretKeyStorage.LoadSecretKey("CW1", "123").Key);
					var db = factory.Load<LicenceDatabase>(DatabasePK);
					AssertNotNull(db.TrustedSystem.CertificateConfig.GetCertificate());
					AssertEquals(false, db.TrustedSystem.ETS_SecretKey.IsEmpty);
					var centralSystemConfig = factory.Load<EdiTrustedMessagingConfig>(CentralSystemConfigPK);
					AssertEquals(certs.LocalCertificate.Thumbprint, db.TrustedSystem.CertificateConfig.GetCertificate().Thumbprint);
					AssertEquals(certs.RemoteCertificate.Thumbprint, centralSystemConfig.GetCertificate().Thumbprint);
					AssertEquals(config.SecretKeyStorage.LoadSecretKey("CW1", "123").Key, Convert.ToBase64String(db.TrustedSystem.ETS_SecretKey));
				}
			}
		}

		protected override void SetUp()
		{
			TransactionedTestCase.RunClientDbCreateScripts();
			base.SetUp();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var product = "CW1";
			var licence = BillingTestHelper.CreateLicence(factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_DatabaseNumber = 8000;
			database.LD_Product = product;
			database.LD_Password = "DE-3E-CC-F6-D3-2B-3E-58-E3-E1-55-80-F5-77-67-95-73-D9-1D-CD-C7-52-37-9A-C8-1A-E9-24-1B-23-75-7D-E9-B2-93-AB-BE-0E-2B-58-A8-8D-8B-64-03-33-95-DB-72-C2-1A-9C-1B-3F-BB-1E-8F-FB-5D-BE-B0-71-74-12";
			database.LD_IsActive = true;
			database.LD_Status = "REG";
			var centralSystemConfig = factory.New<EdiTrustedMessagingConfig>();
			centralSystemConfig.ETM_Product = product;
			centralSystemConfig.ETM_CertificateType = "CSC";
			var cert = CreateCert();
			centralSystemConfig.ETM_CertificateData = cert.Export(X509ContentType.Pfx, "123");
			centralSystemConfig.ETM_CertificatePassword = "123";
			factory.Save();
			DatabasePK = database.PK;
			CentralSystemConfigPK = centralSystemConfig.PK;
		}

		ZGuid DatabasePK;
		ZGuid CentralSystemConfigPK;
		class UserPortalClientForTest : RemoteApiService
		{
			public UserPortalClientForTest(RSACryptoServiceProvider rsa) : base(new UserPortalClientConfigurationForTest())
			{
				CurrentRSA = rsa;
			}

			readonly RSACryptoServiceProvider CurrentRSA;
			protected override HttpClient NewHttpClient() => new HttpClientForTest(CurrentRSA);
			protected override RSACryptoServiceProvider GetRSAProvider() => CurrentRSA;
		}

		class HttpClientForTest : HttpClient
		{
			public HttpClientForTest(RSACryptoServiceProvider rsa)
			{
				CurrentRSA = rsa;
			}

			readonly RSACryptoServiceProvider CurrentRSA;
			public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var url = request.RequestUri.LocalPath;
				var requestBody = await request.Content.ReadAsStringSmartAsync(CancellationToken.None);
				if (url.Contains("api/TrustedMessaging/Activation"))
				{
					var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://unit-testing/api/TrustedMessaging/Activation");
					requestMessage.Content = new StringContent(requestBody);
					using (var controller = new TrustedMessagingControllerForTest(CurrentRSA))
					{
						var rsp = await ControllerTestHelper.ExecuteAsync(controller, requestMessage, typeof(TrustedMessagingV1Controller)).AwaitSmart();
						return rsp;
					}
				}
				else if (url.Contains("api/handshake"))
				{
					var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://unit-testing/api/handshake");
					requestMessage.Headers.Add("PRODUCT", this.DefaultRequestHeaders.GetValues("PRODUCT").Single());
					requestMessage.Headers.Add("SYSTEMID", this.DefaultRequestHeaders.GetValues("SYSTEMID").Single());
					requestMessage.Headers.Add("SIGNED", this.DefaultRequestHeaders.GetValues("SIGNED").Single());
					requestMessage.Content = new StringContent(requestBody);
					using (var controller = new HandshakeController())
					{
						var rsp = await ControllerTestHelper.ExecuteAsync(controller, requestMessage, typeof(HandshakeController)).AwaitSmart();
						return rsp;
					}
				}
				else if (url.Contains("api/UserAgreement/Agreement"))
				{
					var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://unit-testing/api/UserAgreement/Agreement");
					requestMessage.Headers.Add("SIGNED", this.DefaultRequestHeaders.GetValues("SIGNED").Single());
					requestMessage.Headers.Add("WTG_I", this.DefaultRequestHeaders.GetValues("WTG_I").Single());
					requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
					using (var controller = new UserAgreementV1Controller())
					{
						var rsp = await ControllerTestHelper.ExecuteAsync(controller, requestMessage, typeof(UserAgreementV1Controller)).AwaitSmart();
						return rsp;
					}
				}

				return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented)).AwaitSmart();
			}
		}

		class TrustedMessagingControllerForTest : TrustedMessagingV1Controller
		{
			public TrustedMessagingControllerForTest(RSACryptoServiceProvider rsa) : base(new NLogWrapperForTest(typeof(TrustedMessagingControllerForTest)))
			{
				CurrentRSA = rsa;
			}

			readonly RSACryptoServiceProvider CurrentRSA;
			protected override IEDICertRequest GetCertRequest() => new EDICertRequestForTest(CurrentRSA);
		}

		class EDICertRequestForTest : IEDICertRequest
		{
			public EDICertRequestForTest(RSACryptoServiceProvider rsa)
			{
				CurrentRSA = rsa;
			}

			readonly RSACryptoServiceProvider CurrentRSA;
			public bool TrySubmitSafe(string soapRequestTemplate, string subjectName, byte[] keyBlob, ICredentials credentials, EdiCertRequestContext context, out string output)
			{
				var req = new CertificateRequest($"cn={subjectName}", CurrentRSA, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				using (var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5)))
				{
					output = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
				}

				return true;
			}
		}

		class UserPortalClientConfigurationForTest : UserPortalClientConfiguration
		{
			public override int RetryIntervalInMilliseconds => 1;
		}

		static X509Certificate2 CreateCert()
		{
			using (var rsa = new RSACryptoServiceProvider(4096))
			{
				var req = new CertificateRequest($"cn={Guid.NewGuid()}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
			}
		}

		static byte[] CreateKey()
		{
			using (var aes = Aes.Create())
			{
				aes.KeySize = 256;
				aes.Padding = PaddingMode.PKCS7;
				aes.GenerateKey();
				return aes.Key;
			}
		}
	}
}
