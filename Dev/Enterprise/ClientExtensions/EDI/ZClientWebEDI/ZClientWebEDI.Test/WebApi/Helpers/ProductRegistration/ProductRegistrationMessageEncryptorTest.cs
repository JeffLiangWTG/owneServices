using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Microsoft.IdentityModel.Tokens;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class ProductRegistrationMessageEncryptorTest : TestCaseWithFactory
	{
		public void TestDecryptRegistrationRequest()
		{
			var msg = "123456abc";
			var encryptedMsg = TestHelper.EncryptRegistrationRequest(msg);
			AssertEquals(msg, Encryptor.DecryptRegistrationRequest(encryptedMsg.encryptedData, encryptedMsg.signature));
		}

		public void TestEncryptRegistrationResponse()
		{
			var msg = "123456abc";
			var (encryptedData, signature) = Encryptor.EncryptRegistrationResponse(msg);
			AssertEquals(msg, TestHelper.DecryptRegistrationResponse(encryptedData, signature));
		}

		ProductRegistrationMessageEncryptor Encryptor;
		ProductRegistrationMessageEncryptorTestHelper TestHelper;
		protected override void SetUp()
		{
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				Encryptor = new ProductRegistrationMessageEncryptor("ABU");
			}

			TestHelper = new ProductRegistrationMessageEncryptorTestHelper();
			base.SetUp();
		}
	}

	class ProductRegistrationMessageEncryptorTestHelper
	{
		readonly RsaSecurityKey PublicKey;
		readonly RsaSecurityKey PrivateKey;

		public ProductRegistrationMessageEncryptorTestHelper()
		{
			var provider = new CertificatesProviderForTest();
			PublicKey = new RsaSecurityKey(provider.LocalCertificate.GetRSAPublicKey());
			PrivateKey = new RsaSecurityKey(provider.RemoteCertificate.GetRSAPrivateKey());
		}

		public (string encryptedData, string signature) EncryptRegistrationRequest(string request)
		{
			var encryptedData = PublicKey.Rsa.Encrypt(Encoding.UTF8.GetBytes(request), RSAEncryptionPadding.Pkcs1);
			var signature = PrivateKey.Rsa.SignData(encryptedData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			return (Convert.ToBase64String(encryptedData), Convert.ToBase64String(signature));
		}

		public string DecryptRegistrationResponse(string encryptedResponse, string signature)
		{
			var encryptedResponseAsBytes = Convert.FromBase64String(encryptedResponse);
			if (!PublicKey.Rsa.VerifyData(encryptedResponseAsBytes, Convert.FromBase64String(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
			{
				throw new CryptographicException("Signature Verification Failed");
			}
			return Encoding.UTF8.GetString(PrivateKey.Rsa.Decrypt(encryptedResponseAsBytes, RSAEncryptionPadding.Pkcs1));
		}
	}
}
