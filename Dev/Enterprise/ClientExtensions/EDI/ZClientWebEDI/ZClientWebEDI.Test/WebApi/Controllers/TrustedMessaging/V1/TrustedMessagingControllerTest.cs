using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Newtonsoft.Json;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class TrustedMessagingControllerTest : TestCaseWithFactory
	{
		#region Activation
		public void TestActivation_ConcurrencyDatabase()
		{
			var logger = new NLogWrapperForTest(GetType());
			AssertEquals(null, Database.TrustedSystem);
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var info = new CW1ActivationInfo()
				{ DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", PublicKey = rsa.ExportCspBlob(false), };
				var qs = new SecureQueryString();
				qs[nameof(CW1ActivationInfo)] = JsonConvert.SerializeObject(info);
				var rspMsg = CallActivation(qs.ToString(), logger, true);
				AssertEquals(HttpStatusCode.OK, rspMsg.StatusCode);
				var rspObj = JsonConvert.DeserializeObject<TrustedSystemRegistrationResponse>(rspMsg.Content.ReadAsStringAsync().Result);
				var certs = rspObj.Export(rsa);
				AssertCertificate(certs.RemoteCertificate, CentralSystemConfig.GetCertificate().GetRSAPrivateKey());
				AssertCertificate(certs.LocalCertificate, rsa);
				var database = new BusinessObjectFactory().Load<LicenceDatabase>(Database.PK);
				AssertCertificate(database.TrustedSystem.CertificateConfig.GetCertificate(), rsa);
				var log = database.Logs.Find(x => x.SL_SE_NKEvent == Events.CertificateReceived.Code).Single();
				AssertEquals(log.SL_Reference, $"|Thumbprint={certs.LocalCertificate.Thumbprint}");
				var expectedLogMessages = new string[] { "Request received", "success | 200" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		public void TestActivation()
		{
			var logger = new NLogWrapperForTest(GetType());
			AssertEquals(null, Database.TrustedSystem);
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var info = new CW1ActivationInfo()
				{ DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", PublicKey = rsa.ExportCspBlob(false), };
				var qs = new SecureQueryString();
				qs[nameof(CW1ActivationInfo)] = JsonConvert.SerializeObject(info);
				var rspMsg = CallActivation(qs.ToString(), logger);
				AssertEquals(HttpStatusCode.OK, rspMsg.StatusCode);
				var rspObj = JsonConvert.DeserializeObject<TrustedSystemRegistrationResponse>(rspMsg.Content.ReadAsStringAsync().Result);
				var certs = rspObj.Export(rsa);
				AssertCertificate(certs.RemoteCertificate, CentralSystemConfig.GetCertificate().GetRSAPrivateKey());
				AssertCertificate(certs.LocalCertificate, rsa);
				var database = new BusinessObjectFactory().Load<LicenceDatabase>(Database.PK);
				AssertCertificate(database.TrustedSystem.CertificateConfig.GetCertificate(), rsa);
				var log = database.Logs.Find(x => x.SL_SE_NKEvent == Events.CertificateReceived.Code).Single();
				AssertEquals(log.SL_Reference, $"|Thumbprint={certs.LocalCertificate.Thumbprint}");
				var expectedLogMessages = new string[] { "Request received", "success | 200" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		public void TestActivation_BadRequests()
		{
			var logger = new NLogWrapperForTest(GetType());
			AssertEquals(null, Database.TrustedSystem);
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var info = new CW1ActivationInfo()
				{ DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "@#$", PublicKey = rsa.ExportCspBlob(false), };
				var qs = new SecureQueryString();
				qs[nameof(CW1ActivationInfo)] = JsonConvert.SerializeObject(info);
				var rspMsg = CallActivation(qs.ToString(), logger);
				AssertEquals(HttpStatusCode.BadRequest, rspMsg.StatusCode);
				var rspObj = JsonConvert.DeserializeObject<ErrorMessages>(rspMsg.Content.ReadAsStringAsync().Result).Messages.Single();
				AssertEquals(rspObj.Code, "500");
				AssertEquals(rspObj.Message, "Password Mismatch");
				var expectedLogMessages = new string[] { "Request received", "Password Mismatch" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
			}

			Database.LD_IsActive = false;
			Factory.Save();
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var info = new CW1ActivationInfo()
				{ DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "@#$", PublicKey = rsa.ExportCspBlob(false), };
				var qs = new SecureQueryString();
				qs[nameof(CW1ActivationInfo)] = JsonConvert.SerializeObject(info);
				var rspMsg = CallActivation(qs.ToString(), logger);
				AssertEquals(HttpStatusCode.BadRequest, rspMsg.StatusCode);
				var rspObj = JsonConvert.DeserializeObject<ErrorMessages>(rspMsg.Content.ReadAsStringAsync().Result).Messages.Single();
				AssertEquals(rspObj.Code, "500");
				AssertEquals(rspObj.Message, "Database Not Active");
				var expectedLogMessages = new string[] { "Request received", "Database Not Active" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
			}

			Database.LD_IsActive = true;
			Database.LD_Status = "NON";
			Factory.Save();
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var info = new CW1ActivationInfo()
				{ DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "@#$", PublicKey = rsa.ExportCspBlob(false), };
				var qs = new SecureQueryString();
				qs[nameof(CW1ActivationInfo)] = JsonConvert.SerializeObject(info);
				var rspMsg = CallActivation(qs.ToString(), logger);
				AssertEquals(HttpStatusCode.BadRequest, rspMsg.StatusCode);
				var rspObj = JsonConvert.DeserializeObject<ErrorMessages>(rspMsg.Content.ReadAsStringAsync().Result).Messages.Single();
				AssertEquals(rspObj.Code, "500");
				AssertEquals(rspObj.Message, "Database Not Registered");
				var expectedLogMessages = new string[] { "Request received", "Database Not Registered" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		public void TestActivation_ProductDiscrepancy()
		{
			var logger = new NLogWrapperForTest(GetType());
			Database.LD_Product = ProductTypes.Codes.ProductivityWise;
			Factory.Save();
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var info = new CW1ActivationInfo()
				{ DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", PublicKey = rsa.ExportCspBlob(false), };
				var qs = new SecureQueryString();
				qs[nameof(CW1ActivationInfo)] = JsonConvert.SerializeObject(info);
				var rspMsg = CallActivation(qs.ToString(), logger);
				AssertEquals(HttpStatusCode.OK, rspMsg.StatusCode);
				var expectedLogMessages = new string[] { "Request received", "success | 200" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				Database.Reload();
				var system = Database.TrustedSystem;
				var config = system.CertificateConfig;
				AssertEquals(ProductTypes.Codes.CargoWiseOne, system.ETS_Product);
				AssertEquals(ProductTypes.Codes.CargoWiseOne, config.ETM_Product);
			}
		}

		public void TestActivation_InternalEnterprise()
		{
			var logger = new NLogWrapperForTest(GetType());
			using (var rsa = new RSACryptoServiceProvider(4096))
			{
				var testingCertWithPrivateKey = new CertificatesProviderForTest()
				{ IsServer = false }.LocalCertificate;
				EDIDataRegistry.Instance.InternalCW1ActivationCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testingCertWithPrivateKey.Export(X509ContentType.Pfx));
				Database.LicEnterprise.LE_IsInternal = true;
				Factory.Save();
				var info = new CW1ActivationInfo()
				{ DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", PublicKey = rsa.ExportCspBlob(false), };
				var qs = new SecureQueryString();
				qs[nameof(CW1ActivationInfo)] = JsonConvert.SerializeObject(info);
				var rspMsg = CallActivation(qs.ToString(), logger);
				AssertEquals(HttpStatusCode.OK, rspMsg.StatusCode);
				var rspObj = JsonConvert.DeserializeObject<TrustedSystemRegistrationResponse>(rspMsg.Content.ReadAsStringAsync().Result);
				var certs = rspObj.Export(rsa);
				AssertCertificate(certs.RemoteCertificate, CentralSystemConfig.GetCertificate().GetRSAPrivateKey());
				AssertCertificate(certs.LocalCertificate, testingCertWithPrivateKey.GetRSAPrivateKey(), true);
				var database = new BusinessObjectFactory().Load<LicenceDatabase>(Database.PK);
				AssertCertificate(database.TrustedSystem.CertificateConfig.GetCertificate(), testingCertWithPrivateKey.GetRSAPrivateKey());
				var log = database.Logs.Find(x => x.SL_SE_NKEvent == Events.CertificateReceived.Code).Single();
				AssertEquals(log.SL_Reference, $"|Thumbprint={certs.LocalCertificate.Thumbprint}");
				var expectedLogMessages = new string[] { "Request received", "success | 200" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		#endregion Activation
		#region Certificates
		public void TestCertificates_Database()
		{
			var logger = new NLogWrapperForTest(GetType());
			var certDb = SetupDbWithCertificate();
			var licence = BillingTestHelper.CreateLicence(Factory, "EEE", "DEF", "MEL");
			var requestDb = licence.Database;
			requestDb.LD_TenantID = "CSP573";
			requestDb.LD_DatabaseNumber = 4821;
			requestDb.LD_Product = "CSP";
			securityKeyTestHelper.SetSecretKey(requestDb);
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, requestDb.LD_Product, requestDb.LD_TenantID, certDb.LD_Product, certDb.LD_TenantID);
			var response = CallCertificate(request, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, requestDb.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<CertificateResponse>(content);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals(certDb.TrustedSystem.CertificateConfig.ETM_CertificateData, data.CertificateData);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestCertificates_TrustedService()
		{
			var logger = new NLogWrapperForTest(GetType());
			var certDb = SetupDbWithCertificate();
			var serviceCode = "DDD";
			var trustedServices = new CodeDescriptionBoolCollection { { serviceCode, (NoResString)"Demo Service", true } };
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, trustedServices);
			var trustedService = Factory.New<EdiTrustedSystem>();
			trustedService.ETS_Product = serviceCode;
			securityKeyTestHelper.SetSecretKey(trustedService);
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, serviceCode, string.Empty, certDb.LD_Product, certDb.LD_TenantID);
			var response = CallCertificate(request, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, trustedService);
			var data = JsonConvert.DeserializeObject<CertificateResponse>(content);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals(certDb.TrustedSystem.CertificateConfig.ETM_CertificateData, data.CertificateData);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		LicenceDatabase SetupDbWithCertificate()
		{
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 23400;
			db.LD_Product = product;
			var remoteCert = db.GetOrCreateTrustedSystem().GetOrCreateCertificateConfig();
			remoteCert.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Client.cer");
			Factory.Save();
			return db;
		}

		static TrustedRequestForTest CreateRequest(SecurityKeyTestHelper securityKeyTestHelper, string product, string systemId, string certRequestProduct, string certRequestSystemId)
		{
			var infoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime();
			var certInfo = new CertificateInfo()
			{ Product = product, SystemId = systemId, CertificateOwnerProduct = certRequestProduct, CertificateOwnerSystemId = certRequestSystemId, InfoExpires = infoExpires };
			var json = JsonConvert.SerializeObject(certInfo);
			var certProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(certProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var trustedRequest = new TrustedRequest()
			{ EncryptedContent = trustedMessage.EncryptedContent, Product = product, SystemId = systemId };
			return new TrustedRequestForTest()
			{ Request = trustedRequest, Signature = trustedMessage.Signature, IV = trustedMessage.IV };
		}

		#endregion Certificates
		#region Execute
		HttpResponseMessage Execute(HttpRequestMessage request, NLogWrapper logger, bool doConcurrency = false)
		{
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			using (var controller = new TrustedMessagingControllerForTest(logger))
			{
				controller.DoConcurrency = doConcurrency;
				return ControllerTestHelper.Execute(controller, request, typeof(TrustedMessagingV1Controller));
			}
		}

		HttpResponseMessage CallActivation(string queryString, NLogWrapper logger, bool doConcurrency = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"http://unit-testing/api/TrustedMessaging/Activation");
			requestMessage.Content = new StringContent(queryString);
			return Execute(requestMessage, logger, doConcurrency);
		}

		HttpResponseMessage CallCertificate(TrustedRequestForTest request, NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://unit-testing/api/TrustedMessaging/Certificate");
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request.Request), Encoding.UTF8, "application/json");
			requestMessage.Headers.Add("SIGNED", request.Signature);
			requestMessage.Headers.Add("WTG_I", request.IV);
			return Execute(requestMessage, logger);
		}

		#endregion Execute

		#region Implements
		readonly SecurityKeyTestHelper securityKeyTestHelper = new SecurityKeyTestHelper();
		void AssertCertificate(X509Certificate2 cert, RSA privateKey, bool expectPrivateKey = false)
		{
			if (!expectPrivateKey)
			{
				AssertNull(cert.GetRSAPrivateKey());
			}
			else
			{
				AssertNotNull(cert.GetRSAPrivateKey());
			}

			var msg = ZGuid.NewZGuid().ToString();
			var encryptedBytes = cert.GetRSAPublicKey().Encrypt(Encoding.UTF8.GetBytes(msg), RSAEncryptionPadding.Pkcs1);
			var decryptedMsg = Encoding.UTF8.GetString(privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1));
			AssertEquals(msg, decryptedMsg);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var product = "CW1";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			Database = licence.Database;
			Database.LD_DatabaseNumber = 8000;
			Database.LD_Product = product;
			Database.LD_Password = "DE-3E-CC-F6-D3-2B-3E-58-E3-E1-55-80-F5-77-67-95-73-D9-1D-CD-C7-52-37-9A-C8-1A-E9-24-1B-23-75-7D-E9-B2-93-AB-BE-0E-2B-58-A8-8D-8B-64-03-33-95-DB-72-C2-1A-9C-1B-3F-BB-1E-8F-FB-5D-BE-B0-71-74-12";
			Database.LD_IsActive = true;
			Database.LD_Status = "REG";
			CentralSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			CentralSystemConfig.ETM_Product = product;
			CentralSystemConfig.ETM_CertificateType = "CSC";
			CentralSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			Factory.Save();
		}

		EdiTrustedMessagingConfig CentralSystemConfig;
		LicenceDatabase Database;
		class TrustedMessagingControllerForTest : TrustedMessagingV1Controller
		{
			public TrustedMessagingControllerForTest() : base()
			{
			}

			public TrustedMessagingControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			public bool DoConcurrency;
			protected override IEDICertRequest GetCertRequest() => new EDICertRequestForTest();
			protected override void DoAdditionalStuffForTesting(LicenceDatabase database)
			{
				base.DoAdditionalStuffForTesting(database);
				if (DoConcurrency)
				{
					var factory = new BusinessObjectFactory()
					{ RefreshEnabled = false };
					var db = factory.Load<LicenceDatabase>(database.PK);
					db.LD_LastHeartbeat = ZDateTime.Now;
					factory.Save();
				}
			}
		}

		class EDICertRequestForTest : IEDICertRequest
		{
			public static void SetOutput(RSA rsa)
			{
				var subjectName = Guid.NewGuid().ToString();
				var req = new CertificateRequest($"cn={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				using (var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5)))
				{
					Output = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
				}
			}

			static string Output { get; set; }

			public bool TrySubmitSafe(string soapRequestTemplate, string subjectName, byte[] keyBlob, ICredentials credentials, EdiCertRequestContext context, out string output)
			{
				output = Output;
				return true;
			}
		}
		#endregion Implements
	}
}
