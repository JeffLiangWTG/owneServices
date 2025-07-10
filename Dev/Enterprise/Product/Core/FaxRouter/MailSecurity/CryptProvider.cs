using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;

namespace Enterprise.FaxRouter.MailSecurity
{
	public class CryptProvider : ICryptographicProvider
	{
		public string GetDateTimeFormat() => Constants.CRYPT_DATETIME_FORMAT;
		public string GetCryptKey(CryptKeyType type) => type == CryptKeyType.FAX ? Constants.EDI_CRYPT_FAX_KEY : Constants.EDI_CRYPT_ACK_KEY;

		public string GenerateKey(byte[] hashFileElement, string dateTime, CryptKeyType keyType)
		{
			_ = Argument.NotNull(hashFileElement, nameof(hashFileElement));
			_ = Argument.GreaterThanZero(hashFileElement.Length, nameof(hashFileElement));
			_ = Argument.NotNullOrEmpty(dateTime, nameof(dateTime));

			var keyPrefix = keyType == CryptKeyType.FAX ? Constants.EDI_CRYPT_FAX_KEY : Constants.EDI_CRYPT_ACK_KEY;
			var hashPasswordAndDateTimeElements = ConvertStringToByteArray(keyPrefix + dateTime);

			using var md5 = MD5.Create();
			var hashKey = md5.ComputeHash(Append(hashPasswordAndDateTimeElements, hashFileElement));
			return BitConverter.ToString(hashKey);
		}

		Byte[] ConvertStringToByteArray(String s)
		{
			Argument.NotNull(s, nameof(s));
			return new UnicodeEncoding().GetBytes(s);
		}

		public string GenerateKeyFromFile(string filePath, DateTime dateTimeElement, CryptKeyType keyType = CryptKeyType.FAX)
		{
			_ = Argument.NotNullOrEmpty(filePath, nameof(filePath));

			var fileBytes = File.ReadAllBytes(filePath);
			var dateTimeFormatted = dateTimeElement.ToString(GetDateTimeFormat());
			return GenerateKey(fileBytes, dateTimeFormatted, keyType);
		}

		internal static byte[] Append(byte[] sourceArray, byte[] destinationArray)
		{
			_ = Argument.NotNull(sourceArray, nameof(sourceArray));
			_ = Argument.NotNull(destinationArray, nameof(destinationArray));
			_ = Argument.GreaterThanZero(sourceArray.Length, nameof(sourceArray));
			_ = Argument.GreaterThanZero(destinationArray.Length, nameof(destinationArray));

			var tempArray = new byte[sourceArray.Length + destinationArray.Length];
			Buffer.BlockCopy(sourceArray, 0, tempArray, 0, sourceArray.Length);
			Buffer.BlockCopy(destinationArray, 0, tempArray, sourceArray.Length, destinationArray.Length);
			return tempArray;
		}
	}
}
