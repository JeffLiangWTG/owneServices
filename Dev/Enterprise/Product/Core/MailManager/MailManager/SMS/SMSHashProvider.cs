using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.MailManager.SMS
{
	public class SMSHashProvider
	{
		public const string KeyDateTimeFormat	= "yyyyMMddHHmmssfff";

		public string GetHash(string id, string recipient, string messageText, string timeStamp)
		{
			return GetHash(id, recipient, messageText, timeStamp, PassKey, AuthKey);
		}

		public string GetHash(string id, string recipient, string messageText, string timeStamp, string passKey, string authKey)
		{
			var fConcatFieldBytes = GetASCIIByteArray(id + messageText + recipient);
			var fHashSoFar = BitConverter.ToString(fConcatFieldBytes).Replace(HashByteSeparator, "");
			var fTimeStampFilled = timeStamp.Substring(1).PadLeft(fHashSoFar.Length, '0');
			var fKeyFilled = authKey.PadLeft(fHashSoFar.Length, '0');
			var fXorCalculation = GetStringXorFromHexStrings(fKeyFilled, fTimeStampFilled);

			if (fHashSoFar.Length < fXorCalculation.Length)
			{
				fHashSoFar = fHashSoFar.PadLeft(fXorCalculation.Length, '0');
			}

			fXorCalculation = GetStringXorFromHexStrings(fXorCalculation, fHashSoFar, true);
			var fMD5HashKey = MD5.Create().ComputeHash(GetBytes(fXorCalculation));
			var fMD5 = BitConverter.ToString(fMD5HashKey).Replace(HashByteSeparator, "");
			var fPassKeyBytes = GetASCIIByteArray(passKey);
			var fPassKeyBits = BitConverter.ToString(fPassKeyBytes).Replace(HashByteSeparator, "").PadLeft(fMD5.Length, '0');
			fHashSoFar = GetStringXorFromHexStrings(fMD5, fPassKeyBits);
			return fHashSoFar.ToLower();
		}

		#region Implementation		
		protected internal const string AuthKey = "0011223344556677";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected internal const string PassKey = "national";
		protected internal const string HashByteSeparator = "-";

		public static byte[] GetBytes(string formattedBitString)
		{
			var fBitString = formattedBitString.Split('-');
			var fConvertedBytes = new byte[fBitString.Length];
			var fIndex = 0;
			foreach (string valueByte in fBitString)
			{
				fConvertedBytes.SetValue(Convert.ToByte(valueByte, 16), fIndex++);
			}
			return fConvertedBytes;
		}

		public static string GetStringXorFromHexStrings(string expr1, string expr2, bool withSeparator)
		{
			var xorResult = new StringBuilder();
			var fIndex = 0;
			foreach (char keyByte in expr1)
			{
				var fKey = Convert.ToInt32(keyByte.ToString(), 16);
				var fTimeStamp = Convert.ToInt32(expr2[fIndex].ToString(), 16);
				var fXor = fKey ^ fTimeStamp;
				if (withSeparator && Math.IEEERemainder(fIndex, 2) == 0 && fIndex > 0)
				{
					xorResult.Append(HashByteSeparator);
				}
				xorResult.Append(fXor.ToString("X"));
				fIndex++;
			}
			return xorResult.ToString();
		}

		protected string GetStringXorFromHexStrings(string expr1, string expr2)
		{
			return GetStringXorFromHexStrings(expr1, expr2, false);
		}

		protected byte[] GetASCIIByteArray(string stringToBytes)
		{
			return (new ASCIIEncoding()).GetBytes(stringToBytes);
		}

		protected string GetStringFromASCII(byte[] bytesToString)
		{
			return (new ASCIIEncoding()).GetString(bytesToString);
		}
		#endregion
	}
}
