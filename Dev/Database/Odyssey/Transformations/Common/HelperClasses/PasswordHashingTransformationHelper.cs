using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using WTG.Foundation.Cryptography;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public static class PasswordHashingTransformationHelper
	{
		#region Registries

		public static int GetPasswordHashingIterationsCount()
		{
			var binValue = new RegistryTransformationHelper().GetStmDataValue("PasswordHashingIterationsCount");
			if (binValue != null && int.TryParse(Encoding.Unicode.GetString(binValue), out var result))
			{
				return result;
			}
			return 200_000;
		}

		public static int GetPasswordHistoryCount()
		{
			var binValue = new RegistryTransformationHelper().GetStmDataValue("PasswordHistoryCount");
			if (binValue != null && int.TryParse(Encoding.Unicode.GetString(binValue), out var result))
			{
				return result;
			}
			return 0;
		}

		public static bool IsPasswordHistoryCountRegistrySet() => GetPasswordHistoryCount() > 0;

		public static bool IsADIntegrationEnabled()
		{
			var binValue = new RegistryTransformationHelper().GetStmDataValue("ADConfig");
			return (binValue != null) && string.Equals("Y", XDocument.Parse(Encoding.Unicode.GetString(binValue)).Descendants("IsADIntegrationEnabled").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region TwoWayEncoder Equivalent Cryptography to replace Enterprise's TwoWayEncoder

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")] // We need the same guid in lower case same as TwoWayEncoder
		public static AESCryptographicProvider CreateTwoWayEncoderCrypto(Guid guid)
		{
			var key = Encoding.ASCII.GetBytes("6052D90C81D64D5B8F5677AA055CAE23"); // The key used in TwoWayEncoder
			var iv = Encoding.ASCII.GetBytes(guid.ToString().ToLowerInvariant().Replace("-", "").Substring(5, 16));
			return new AESCryptographicProvider(key, iv);
		}

		public static string Encrypt(AESCryptographicProvider crypto, string textToEncrypt) => Convert.ToBase64String(crypto.Encrypt(Encoding.Unicode.GetBytes(textToEncrypt)));

		public static string EncryptWithTwoWayEncoder(Guid guid, string textToEncrypt) => Convert.ToBase64String(CreateTwoWayEncoderCrypto(guid).Encrypt(Encoding.Unicode.GetBytes(textToEncrypt)));

		public static string Decrypt(AESCryptographicProvider crypto, string textToDecrypt)
		{
			if (string.IsNullOrEmpty(textToDecrypt))
			{
				return "";
			}

			var buffer = new byte[textToDecrypt.Length];
			var cipherTextData = Convert.FromBase64String(textToDecrypt);
			var decipheredData = crypto.Decrypt(new ArraySegment<byte>(cipherTextData), new ArraySegment<byte>(buffer));
			return Encoding.Unicode.GetString(decipheredData.Array, 0, decipheredData.Count);
		}

		public static string DecryptWithTwoWayEncoder(Guid guid, string textToDecrypt) => Decrypt(CreateTwoWayEncoderCrypto(guid), textToDecrypt);

		#endregion
	}
}
