using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.Common
{
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DES")]
	public static class TripleDESCryptoServiceProviderExtensions
	{
		public static void InitializeForCargoWise(this SymmetricAlgorithm provider)
		{
			Argument.NotNull(provider, nameof(provider)); // Suggested By ReviewBot
			var md5 = MD5.Create();
			provider.Key = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes("w;lerhpoihpOIYO&)(*&LKJ%$#%$TDFGLKJ;lwkjer;tlkwh")); // CargoWise TripleDES Crypto Key
			provider.IV = new byte[8] { 240, 3, 45, 29, 0, 76, 173, 59 };
		}

		public static byte[] CreateSecurityHash(this SymmetricAlgorithm provider, byte[] data)
		{
			Argument.NotNull(provider, nameof(provider)); // Suggested By ReviewBot
			Argument.NotNull(data, nameof(data)); // Suggested By ReviewBot
			var md5Hash = MD5.Create().ComputeHash(data);
			var encryptor = provider.CreateEncryptor();
			return encryptor.TransformFinalBlock(md5Hash, 0, md5Hash.Length);
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "provider")]
		public static bool IsValidSecurityHash(this SymmetricAlgorithm provider, byte[] data, byte[] securityHash)
		{
			Argument.NotNull(securityHash, nameof(securityHash)); // Suggested By ReviewBot
			Argument.NotNull(data, nameof(data)); // Suggested By ReviewBot
			var result = false;
			try
			{
				var expectedMD5Hash = MD5.Create().ComputeHash(data);

				var tripleDes = TripleDES.Create();
				tripleDes.InitializeForCargoWise();
				var decryptor = tripleDes.CreateDecryptor();
				var actualMD5Hash = decryptor.TransformFinalBlock(securityHash, 0, securityHash.Length);
				result = Convert.ToBase64String(expectedMD5Hash) == Convert.ToBase64String(actualMD5Hash);
			}
			catch (CryptographicException)
			{ }

			return result;
		}
	}
}
