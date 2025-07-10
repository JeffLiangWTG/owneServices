using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.Accounting.Business.RSACryptography
{
	public class RSAEncryptor
	{
		#region Member Variables

		readonly string publicKeyXml;

		#endregion

		#region Constructor

		public RSAEncryptor(string rsaCryptoServicePublicKeyXml)
		{
			publicKeyXml = rsaCryptoServicePublicKeyXml;
		}

		#endregion

		#region Methods

		public string Encrypt(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				throw new ArgumentNullException(nameof(text));
			}

			var data = Encoding.UTF8.GetBytes(text);

			var encryptedData = EncryptBinary(data);
			var encryptedText = Convert.ToBase64String(encryptedData);

			return encryptedText;
		}

		byte[] EncryptBinary(byte[] data)
		{
			return Encrypt(data, RSACryptographyConstants.KeySize, RSACryptographyConstants.WithOAEPPadding);
		}

		#endregion

		#region Helpers

		byte[] Encrypt(byte[] data, int keySize, bool withOAEPPadding)
		{
			using (var rsa = new RSACryptoServiceProvider(keySize))
			{
				rsa.FromXmlString(publicKeyXml);
				return rsa.Encrypt(data, withOAEPPadding);
			}
		}

		#endregion
	}
}
