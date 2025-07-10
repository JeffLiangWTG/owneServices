using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Application;
using Microsoft.IdentityModel.Tokens;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public interface IProductRegistrationMessageEncryptor
	{
		string DecryptRegistrationRequest(string encryptedRequest, string signature);

		(string encryptedData, string signature) EncryptRegistrationResponse(string responseMessage);
	}

	public class ProductRegistrationMessageEncryptor : IProductRegistrationMessageEncryptor
	{
		public ProductRegistrationMessageEncryptor(string product)
		{
			var provider = ObjectFactory.New<ICertificatesProvider>(product);

			LocalKey = GetKey(provider.LocalCertificate, true);
			if (LocalKey == null)
			{
				throw new CryptographicException("Local Key Not Found");
			}

			RemoteKey = GetKey(provider.RemoteCertificate, false);
			if (RemoteKey == null)
			{
				throw new CryptographicException("Remote Key Not Found");
			}
		}

		public string DecryptRegistrationRequest(string encryptedRequest, string signature)
		{
			var encryptedRequestAsBytes = Convert.FromBase64String(encryptedRequest);
			if (RemoteKey.Rsa.VerifyData(encryptedRequestAsBytes, Convert.FromBase64String(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
			{
				return Encoding.UTF8.GetString(LocalKey.Rsa.Decrypt(encryptedRequestAsBytes, RSAEncryptionPadding.Pkcs1));
			}
			else
			{
				throw new CryptographicException("Signature Verification Failed");
			}
		}

		public (string encryptedData, string signature) EncryptRegistrationResponse(string responseMessage)
		{
			var encryptedData = RemoteKey.Rsa.Encrypt(Encoding.UTF8.GetBytes(responseMessage), RSAEncryptionPadding.Pkcs1);
			var signature = LocalKey.Rsa.SignData(encryptedData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			return (Convert.ToBase64String(encryptedData), Convert.ToBase64String(signature));
		}

		static RsaSecurityKey GetKey(X509Certificate2 cert, bool includePrivateKey)
		{
			var rsa = includePrivateKey ? cert?.GetRSAPrivateKey() : cert?.GetRSAPublicKey();
			return rsa != null ? new RsaSecurityKey(rsa) : null;
		}

		readonly RsaSecurityKey LocalKey;
		readonly RsaSecurityKey RemoteKey;
	}
}
