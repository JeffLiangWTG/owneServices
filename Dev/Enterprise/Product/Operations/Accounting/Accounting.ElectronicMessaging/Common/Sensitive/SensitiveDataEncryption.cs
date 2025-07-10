using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Sensitive
{
	public class SensitiveDataEncryption
	{
		public SensitiveDataEncryption(RandomNumberGenerator randomGenerator)
		{
			RandomGenerator = randomGenerator;
		}

		readonly RandomNumberGenerator RandomGenerator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is only for encryption")]
		const string MagicIdentifier = "SENSITIVE$$";
		const string DelimiterString = "$$";
		const string TimestampFormat = "yyyyMMddTHHmmssZ";
		const string VersionOne = "1";

		public string EncryptStringAndSerialize(string message, ReadOnlyMemory<byte> secret, string secretId = "", ReadOnlyMemory<byte>? iv = null)
		{
			var actualAlgorithm = new AES256_HMACSHA256();
			actualAlgorithm.ValidateParameters(secret, iv);

			ReadOnlyMemory<byte> data = Encoding.UTF8.GetBytes(message);
			var now = ZDateTime.UtcNow;
			var actualIv = actualAlgorithm.CreateIV(iv, RandomGenerator);
			var cypherText = actualAlgorithm.Encrypt(data.Span, secret.Span, actualIv.Span);

			var estimatedStringSize
				= MagicIdentifier.Length
				+ DelimiterString.Length * 7
				+ secretId.Length
				+ TimestampFormat.Length
				+ (int)(actualAlgorithm.BlockSizeBytes * 1.3) + 1
				+ (int)((data.Length + actualAlgorithm.BlockSizeBytes) * 1.3) + 1
				+ actualAlgorithm.HashSizeBytes
				+ 64
				;

			var result = new StringBuilder(estimatedStringSize);
			result.Append(MagicIdentifier);
			result.Append(VersionOne[0]);
			result.Append(DelimiterString);
			result.Append(actualAlgorithm.Name);
			result.Append(DelimiterString);
			result.Append(secretId);
			result.Append(DelimiterString);
			result.AppendFormat(CultureInfo.InvariantCulture, "{0:" + TimestampFormat + "}", now);
			result.Append(DelimiterString);
			result.Append(Convert.ToBase64String(actualIv.ToArray()));
			result.Append(DelimiterString);
			result.Append(Convert.ToBase64String(cypherText.ToArray()));

			var bytesToHash = Encoding.UTF8.GetBytes(result.ToString());
			var hash = actualAlgorithm.Hash(bytesToHash, secret.Span);

			result.Append(DelimiterString);
			result.Append(ToHexString(hash.ToArray()));

			return result.ToString();
		}

		string ToHexString(byte[] bytes)
			=> string.Concat((bytes ?? Array.Empty<byte>()).Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
	}
}
