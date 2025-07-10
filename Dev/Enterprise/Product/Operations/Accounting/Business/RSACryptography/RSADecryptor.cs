using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.Accounting.Business.RSACryptography
{
	public class RSADecryptor
	{
		#region Member Variables

		readonly string keyXml;

		#endregion

		#region Constructor

		public RSADecryptor(string rsaCryptoServiceKeyXml)
		{
			keyXml = rsaCryptoServiceKeyXml;
		}

		#endregion

		#region Methods

		public string Decrypt(string text)
		{
			var decryptedData = DecryptBinary(Convert.FromBase64String(text));
			var decryptedText = Encoding.UTF8.GetString(decryptedData);

			return decryptedText;
		}

		internal byte[] DecryptBinary(byte[] data)
		{
			return Decrypt(data, RSACryptographyConstants.KeySize, RSACryptographyConstants.WithOAEPPadding);
		}

		#endregion

		#region Helpers

		byte[] Decrypt(byte[] data, int keySize, bool withOAEPPadding)
		{
			using (var rsa = new RSACryptoServiceProvider(keySize))
			{
				rsa.FromXmlString(keyXml);
				return rsa.Decrypt(data, withOAEPPadding);
			}
		}

		#endregion
	}
}
