using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.Accounting.Business.RSACryptography
{
	public class RSASignatureVerifier
	{
		#region Member Variables

		readonly string publicKeyXml;

		#endregion

		#region Constructor

		public RSASignatureVerifier(string rsaCryptoServicePublicKeyXml)
		{
			publicKeyXml = rsaCryptoServicePublicKeyXml;
		}

		#endregion

		public bool VerifyData(string dataToVerify, string signature)
		{
			var data = Encoding.UTF8.GetBytes(dataToVerify);
			var signatureAsBytes = Convert.FromBase64String(signature);

			using (var rsa = new RSACryptoServiceProvider(RSACryptographyConstants.KeySize))
			{
				rsa.FromXmlString(publicKeyXml);
				return rsa.VerifyData(data, SHA1.Create(), signatureAsBytes);
			}
		}

		public bool VerifyHash(string dataToVerify, string signature)
		{
			var data = Encoding.UTF8.GetBytes(dataToVerify);
			var signatureAsBytes = Convert.FromBase64String(signature);

			using (var rsa = new RSACryptoServiceProvider(RSACryptographyConstants.KeySize))
			{
				rsa.FromXmlString(publicKeyXml);
				var sha1 = SHA1.Create();
				var hashedData = sha1.ComputeHash(data);
				return rsa.VerifyHash(hashedData, signatureAsBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
			}
		}
	}
}
