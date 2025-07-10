using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public static class TokenizedAccessControlExtensions
	{
		public static string CreateLimitedToken(this ITokenizedAccessControl accessControl, string type, AccessTokenInfo info, TimeSpan? time = null, int maxUses = -1)
		{
			var result = CreateLimitedToken(accessControl, type, info, time, maxUses, DefaultCharacterSet, DefaultMaxLength);
			return result;
		}

		public static string CreateLimitedToken(this ITokenizedAccessControl accessControl, string type, AccessTokenInfo info, TimeSpan? time, int maxUses, IList<char> characterSet, int length)
		{
			const bool isPermanent = false;
			bool created;
			string token;

			// DateTime + TimeSpan = DateTime
			// DateTime + null = null
			var expiry = DateTime.UtcNow + time;
			int numberOfAttempts = 0;

			do
			{
				token = CreateToken(characterSet, length);
				created = accessControl.TryCreate(token, type, isPermanent, expiry, maxUses, info);
				numberOfAttempts++;
			} while (!created && numberOfAttempts < 100);

			if (!created)
			{
				throw new TokenGenerationException("Unable to generate access token.");
			}

			return token;
		}

		// Avoid using letters or numbers that can look identical.
		public static IList<char> DefaultCharacterSet => new List<char>("ABCDEFHKMNPQRTWXY34578");

		public static int DefaultMaxLength => 30;

		static string CreateToken(IList<char> characterSet, int length)
		{
			var sb = new StringBuilder(length);
			var numCharsToChooseFrom = characterSet.Count;

			for (int i = 0; i < length; i++)
			{
				var index = GetRandomNumber((uint)numCharsToChooseFrom);
				var randomChar = characterSet[index];
				sb.Append(randomChar);
			}

			return sb.ToString();
		}

		static int GetRandomNumber(uint maxNumber)
		{
			if (maxNumber <= 0 || maxNumber > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(maxNumber));
			}

			// Note that this is a biased implementation i.e. if your number does not divide evenly into maxNumber, the results
			// when graphed will skew towards zero. since we are dividing (0-uint.Max) by a number that does not divide evenly.
			// Consider what happens if you take random numbers between 0 and 5 and modulo by 3.
			//   0 % 3 = 0
			//   1 % 3 = 1
			//   2 % 3 = 2
			//   3 % 3 = 0
			//   4 % 3 = 1
			// There is a bias towards the values 0 and 1 - it is twice as likely that you will get a 0 or a 1 than getting a 2.
			// In practice the bias here should be very small, since we're generating numbers up to 30 (StmAccessToken.SAT_Token's max length)
			// from an enormous pool of possible numbers (2^64 - 1).

			var randomData = new byte[sizeof(ulong)];

			using var random = RandomNumberGenerator.Create();
			random.GetBytes(randomData);

			var integerValue = BitConverter.ToUInt64(randomData, 0);
			var randomNumber = integerValue % maxNumber;
			return (int)randomNumber;
		}
	}
}
