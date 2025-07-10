using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.Accounting.Business.RSACryptography
{
	public class RSASignatureSigner
	{
		#region Member Variables

		readonly string privateKeyXml;

		#endregion

		#region Constructor

		public RSASignatureSigner(string rsaCryptoServicePrivateKeyXml)
		{
			privateKeyXml = rsaCryptoServicePrivateKeyXml;
		}

		#endregion

		public string SignData(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				throw new ArgumentNullException(nameof(text));
			}

			var data = Encoding.UTF8.GetBytes(text);

			using (var rsa = new RSACryptoServiceProvider(RSACryptographyConstants.KeySize))
			{
				rsa.FromXmlString(privateKeyXml);
				var signatureAsBytes = rsa.SignData(data, SHA1.Create());
				return Convert.ToBase64String(signatureAsBytes);
			}
		}

		public byte[] SignHashAsBytes(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				throw new ArgumentNullException(nameof(text));
			}

			var data = Encoding.UTF8.GetBytes(text);

			using (var rsa = new RSACryptoServiceProvider(RSACryptographyConstants.KeySize))
			{
				rsa.FromXmlString(privateKeyXml);
				var sha1 = SHA1.Create();
				var hashedData = sha1.ComputeHash(data);
				return rsa.SignHash(hashedData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
			}
		}

		public string SignHashAsBase64String(string text)
		{
			return Convert.ToBase64String(SignHashAsBytes(text));
		}
	}
}
