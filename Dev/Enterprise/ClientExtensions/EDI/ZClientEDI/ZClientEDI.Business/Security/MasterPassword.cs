using System;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI
{
	public static class MasterPassword
	{
		public static string GenerateForCurrentUserToday()
		{
			return GenerateForDate(EnvProxy.Instance.CurrentUser.Initials, ZDateTime.UtcNow.ToDateTime());
		}

		static string GenerateForDate(string staffInitials, DateTime dateTime)
		{
			var prefix = staffInitials.Trim();
			if (prefix.Length > InitialsLength)
			{
				prefix = prefix.Substring(0, InitialsLength);
			}
			else if (prefix.Length < InitialsLength)
			{
				prefix = prefix.PadRight(InitialsLength, '0');
			}

			prefix = prefix.ToLowerInvariant();

			var hashInput = prefix + dateTime.Year.ToString() + dateTime.Month.ToString("d2");
			// 16 byte hash
			var hash = MD5.Create().ComputeHash(Encoding.Default.GetBytes(hashInput));
			// XOR it down to size
			for (var i = EncodedLength; i < hash.Length; ++i)
			{
				hash[i % EncodedLength] ^= hash[i];
			}
			var result = new StringBuilder(prefix, Length);
			for (var i = 0; i < EncodedLength; ++i)
			{
				result.Append(Base36Chars[hash[i] % 36]);
			}
			return result.ToString();
		}

		const int Length = 9;
		const int InitialsLength = 3;
		const int EncodedLength = 6;
		const string Base36Chars = "0123456789abcdefghijklmnopqrstuvwxyz";
	}
}
