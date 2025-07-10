using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class HandshakeControllerTest : TestCaseWithFactory
	{
		public void TestHandshake()
		{
			var logger = new NLogWrapperForTest(GetType());
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.GetOrCreateTrustedSystem();
			Factory.Save();
			var certificateProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			RSA privateRsa = certificateProvider.LocalCertificate.GetRSAPrivateKey();
			RSA publicRsa = certificateProvider.RemoteCertificate.GetRSAPublicKey();
			var aes = Aes.Create();
			aes.KeySize = 128;
			aes.Padding = PaddingMode.PKCS7;
			string localKeyString = Convert.ToBase64String(aes.Key);
			string message = $"{{ \"key\": \"{localKeyString}\" }}";
			byte[] msgBytes = Encoding.UTF8.GetBytes(message);
			byte[] encBytes = publicRsa.Encrypt(msgBytes, RSAEncryptionPadding.OaepSHA256);
			byte[] msgSigned = privateRsa.SignData(encBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			string signedString = Convert.ToBase64String(msgSigned);
			string msg = Convert.ToBase64String(encBytes);
			var response = Call(product, db.LD_TenantID, signedString, msg, logger: logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var responseSignature = response.Headers.GetValues("SIGNED").FirstOrDefault();
			AssertNotNull(responseSignature);
			byte[] signatureBytes = Convert.FromBase64String(responseSignature);
			var content = response.Content.ReadAsStringAsync().Result;
			byte[] responseBytes = Convert.FromBase64String(content);
			Assert("Verified signature", publicRsa.VerifyData(responseBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
			byte[] decryptedBytes = privateRsa.Decrypt(responseBytes, RSAEncryptionPadding.OaepSHA256);
			var responseMessage = Encoding.UTF8.GetString(decryptedBytes);
			var jresult = JObject.Parse(responseMessage);
			var remoteKey = jresult["key"].ToString();
			Assert(!string.IsNullOrWhiteSpace(remoteKey));
			db = new BusinessObjectFactory().Load<LicenceDatabase>(db.PK);
			var system = db.GetOrCreateTrustedSystem();
			Assert(!system.ETS_SecretKey.IsEmpty);
			Assert(!system.ETS_SecretKeyExpiryUtc.IsEmpty);
			system.ETS_SystemLastEditTimeUtc = new CargoWise.Types.ZDateTime(2000, 1, 1);
			system.ETS_SystemLastEditUser = "U01";
			db.LD_Product = "CW1";
			db.Factory.Save();

			response = Call(db.LD_Product, db.LD_DatabaseNumber.ToString(), signedString, msg, logger: logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var systemInNewFactory = new BusinessObjectFactory().Load<EdiTrustedSystem>(system.PK);
			AssertEquals("E", systemInNewFactory.ETS_SystemLastEditUser);
			AssertEquals(false, systemInNewFactory.ETS_SystemLastEditTimeUtc.IsEmpty);
			AssertNotEquals(new CargoWise.Types.ZDateTime(2000, 1, 1), systemInNewFactory.ETS_SystemLastEditTimeUtc);
		}

		public void TestHandshake_TrustedService()
		{
			var logger = new NLogWrapperForTest(GetType());
			var serviceCode = "DDD";
			var trustedServices = new CodeDescriptionBoolCollection { { serviceCode, (NoResString)"Demo Service", true } };
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, trustedServices);
			Factory.Save();
			var certificateProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			RSA privateRsa = certificateProvider.LocalCertificate.GetRSAPrivateKey();
			RSA publicRsa = certificateProvider.RemoteCertificate.GetRSAPublicKey();
			var aes = Aes.Create();
			aes.KeySize = 128;
			aes.Padding = PaddingMode.PKCS7;
			string localKeyString = Convert.ToBase64String(aes.Key);
			string message = $"{{ \"key\": \"{localKeyString}\" }}";
			byte[] msgBytes = Encoding.UTF8.GetBytes(message);
			byte[] encBytes = publicRsa.Encrypt(msgBytes, RSAEncryptionPadding.OaepSHA256);
			byte[] msgSigned = privateRsa.SignData(encBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			string signedString = Convert.ToBase64String(msgSigned);
			string msg = Convert.ToBase64String(encBytes);
			var response = Call(serviceCode, string.Empty, signedString, msg, logger: logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var responseSignature = response.Headers.GetValues("SIGNED").FirstOrDefault();
			AssertNotNull(responseSignature);
			byte[] signatureBytes = Convert.FromBase64String(responseSignature);
			var content = response.Content.ReadAsStringAsync().Result;
			byte[] responseBytes = Convert.FromBase64String(content);
			Assert("Verified signature", publicRsa.VerifyData(responseBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
			byte[] decryptedBytes = privateRsa.Decrypt(responseBytes, RSAEncryptionPadding.OaepSHA256);
			var responseMessage = Encoding.UTF8.GetString(decryptedBytes);
			var jresult = JObject.Parse(responseMessage);
			var remoteKey = jresult["key"].ToString();
			Assert(!string.IsNullOrWhiteSpace(remoteKey));
			var config = Factory.LoadTop1<EdiTrustedSystem>(new ZQuery(EdiTrustedSystemSchema.ETS_Product, serviceCode));
			Assert(!config.ETS_SecretKey.IsEmpty);
			AssertNotNull(config.ETS_SecretKeyExpiryUtc);
		}

		public void TestBadRequestCertificateMismatched()
		{
			var logger = new NLogWrapperForTest(GetType());
			const string jsonExpected = "{\"errors\":[{\"code\":\"1004\",\"message\":\"Certificate is mismatched.\"}]}";
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.GetOrCreateTrustedSystem();
			Factory.Save();
			var certificateProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			RSA privateRsa = certificateProvider.LocalCertificate.GetRSAPrivateKey();
			RSA publicRsa = certificateProvider.RemoteCertificate.GetRSAPrivateKey();
			var aes = Aes.Create();
			aes.KeySize = 128;
			aes.Padding = PaddingMode.PKCS7;
			string localKeyString = Convert.ToBase64String(aes.Key);
			string message = $"{{ \"key\": \"{localKeyString}\" }}";
			byte[] msgBytes = Encoding.UTF8.GetBytes(message);
			byte[] encBytes = publicRsa.Encrypt(msgBytes, RSAEncryptionPadding.OaepSHA256);
			byte[] msgSigned = privateRsa.SignData(encBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			string signedString = Convert.ToBase64String(msgSigned);
			string msg = Convert.ToBase64String(encBytes);
			var response = Call(product, db.LD_TenantID, signedString, msg, logger: logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			//server does not have certs
			response = Call(product, db.LD_TenantID, signedString, msg, new EmptyCertificatesProviderForTest(), logger: logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(jsonExpected, response.Content.ReadAsStringAsync().Result);
			//signature verification failed
			msgBytes = Encoding.UTF8.GetBytes(message);
			encBytes = publicRsa.Encrypt(msgBytes, RSAEncryptionPadding.OaepSHA256);
			msgSigned = publicRsa.SignData(encBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			signedString = Convert.ToBase64String(msgSigned);
			msg = Convert.ToBase64String(encBytes);
			response = Call(product, db.LD_TenantID, signedString, msg, logger: logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(jsonExpected, response.Content.ReadAsStringAsync().Result);
			//decryption failed
			msgBytes = Encoding.UTF8.GetBytes(message);
			encBytes = privateRsa.Encrypt(msgBytes, RSAEncryptionPadding.OaepSHA256);
			msgSigned = privateRsa.SignData(encBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			signedString = Convert.ToBase64String(msgSigned);
			msg = Convert.ToBase64String(encBytes);
			response = Call(product, db.LD_TenantID, signedString, msg, logger: logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(jsonExpected, response.Content.ReadAsStringAsync().Result);
		}

		public void TestHandshake_Logging()
		{
			var logger = new NLogWrapperForTest(GetType());
			var product = "SMF";
			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();
			Factory.Save();
			var trustedSystemCertificateProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			RSA privateRsa = trustedSystemCertificateProvider.LocalCertificate.GetRSAPrivateKey();
			RSA publicRsa = trustedSystemCertificateProvider.RemoteCertificate.GetRSAPublicKey();
			var aes = Aes.Create();
			aes.KeySize = 128;
			aes.Padding = PaddingMode.PKCS7;
			string localKeyString = Convert.ToBase64String(aes.Key);
			string message = $"{{ \"key\": \"{localKeyString}\" }}";
			byte[] msgBytes = Encoding.UTF8.GetBytes(message);
			byte[] encBytes = publicRsa.Encrypt(msgBytes, RSAEncryptionPadding.OaepSHA256);
			byte[] msgSigned = privateRsa.SignData(encBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			string signedString = Convert.ToBase64String(msgSigned);
			string msg = Convert.ToBase64String(encBytes);
			CombineAssertions("", () =>
			{
				Call(string.Empty, string.Empty, string.Empty, string.Empty, provider: null, logger: logger);
				var expectedLogMessages = new string[] { "api/Handshake start","404 NotFound" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				Call(product, "SYS_NotExist", "Invalid Signature", string.Empty, provider: null, logger: logger);
				expectedLogMessages = new string[] { "api/Handshake start","The specified system is not found." };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				Call(product, system.ETS_SystemID, "aW52YWxpZCBtZXNzYWdl", string.Empty, provider: null, logger: logger);
				expectedLogMessages = new string[] { "api/Handshake start","Certificate is mismatched." };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				Call(product, system.ETS_SystemID, signedString, "aW52YWxpZCBtZXNzYWdl", provider: null, logger: logger);
				expectedLogMessages = new string[] { "api/Handshake start","Certificate is mismatched." };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.TryGetLock(("SecretKeyGenerationLockKey," + system.PK.ToString()).ToUpperInvariant(), out var appLock);
					using (appLock)
					{
						Call(product, system.ETS_SystemID, signedString, msg, provider: null, logger: logger);
						expectedLogMessages = new string[] { "api/Handshake start","Another handshake is in progress." };
						Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
						logger.ClearLog();
					}
				}

				Call(product, system.ETS_SystemID, signedString, msg, provider: null, logger: logger);
				expectedLogMessages = new string[] { "api/Handshake start","success" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			});
		}

		public void TestHandshake_NoLogAddedToTrustedSystem()
		{
			var logger = new NLogWrapperForTest(GetType());
			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = "SMF";
			system.ETS_SystemID = "production";
			Factory.Save();
			var systemLogsCount = system.Logs.DatabaseCount;
			var certificateProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			RSA privateRsa = certificateProvider.LocalCertificate.GetRSAPrivateKey();
			RSA publicRsa = certificateProvider.RemoteCertificate.GetRSAPublicKey();
			var aes = Aes.Create();
			aes.KeySize = 128;
			aes.Padding = PaddingMode.PKCS7;
			string localKeyString = Convert.ToBase64String(aes.Key);
			string message = $"{{ \"key\": \"{localKeyString}\" }}";
			byte[] msgBytes = Encoding.UTF8.GetBytes(message);
			byte[] encBytes = publicRsa.Encrypt(msgBytes, RSAEncryptionPadding.OaepSHA256);
			byte[] msgSigned = privateRsa.SignData(encBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			string signedString = Convert.ToBase64String(msgSigned);
			string msg = Convert.ToBase64String(encBytes);
			var response = Call(system.ETS_Product, system.ETS_SystemID, signedString, msg, logger: logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("No log for updating secret key", systemLogsCount, system.Logs.DatabaseCount);
		}

		#region Execute
		HttpResponseMessage Execute(HttpRequestMessage request, ICertificatesProvider provider = null, NLogWrapper logger = null)
		{
			using (ObjectFactory.Substitute(() => provider ?? new CertificatesProviderForTest()))
			using (var controller = new HandshakeController(logger))
			{
				return ControllerTestHelper.Execute(controller, request);
			}
		}

		HttpResponseMessage Call(string product, string systemId, string signature, string message, ICertificatesProvider provider = null, NLogWrapper logger = null)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/Handshake/");
			requestMessage.Content = new StringContent(message, Encoding.UTF8, "application/json");
			if (!string.IsNullOrEmpty(product))
			{
				requestMessage.Headers.Add("PRODUCT", product);
			}

			if (!string.IsNullOrEmpty(systemId))
			{
				requestMessage.Headers.Add("SYSTEMID", systemId);
			}

			if (!string.IsNullOrEmpty(signature))
			{
				requestMessage.Headers.Add("SIGNED", signature);
			}

			return Execute(requestMessage, provider, logger);
		}

		#endregion

		class EmptyCertificatesProviderForTest : ICertificatesProvider
		{
			public X509Certificate2 LocalCertificate => null;
			public X509Certificate2 RemoteCertificate => null;
		}
	}
}
