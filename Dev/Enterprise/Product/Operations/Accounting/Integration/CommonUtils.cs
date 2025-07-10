using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Integration
{
	public static class CommonUtils
	{
		public const string SuffixSeparator = "/";

		/// <summary>
		/// Converts numbers like 1, 2, 3, etc to A, B, C, etc.
		/// if number greater than 26, then we should get AA, AB, AC...BA, BB, BC, etc.
		/// </summary>
		/// <param name="number">Number to convert</param>
		/// <returns>String representation of the number</returns>
		public static string GetLetterRepresentation(int number)
		{
			if (number <= 0)
			{
				return "";
			}
			else if (number >= MaxNumberRepresentation)
			{
				throw new ArgumentOutOfRangeException(nameof(number),
					FormattableString.Invariant($"Acceptable range is 0 to {MaxNumberRepresentation - 1} inclusive."));
			}

			char[] alphabetArray = alphabet.ToCharArray();
			string result = "";
			int x = number / alphabetArray.Length;
			int remainder = number % alphabet.Length;
			if (x >= 1)
			{
				if (remainder == 0)
				{
					x -= 1;
				}

				if (x > 0)
				{
					result += alphabetArray[x - 1];
				}
			}
			result += (remainder == 0) ? alphabetArray[alphabet.Length - 1] : alphabetArray[remainder - 1];

			return result;
		}

		public static int GetNumberRepresentation(string suffix)
		{
			Argument.NotNull(suffix, "suffix");

			string entry = suffix.Replace(SuffixSeparator, "").Trim();
			int result = 0;
			for (int i = entry.Length - 1; i >= 0; i--)
			{
				if (i == entry.Length - 1)
				{
					result += (alphabet.IndexOf(entry[i]) + 1);
				}
				else
				{
					result += alphabet.Length * (alphabet.IndexOf(entry[i]) + 1);
				}
			}

			return result;
		}

		public static string ValidaeSuffixForGetNumberRepresentation(string suffix)
		{
			Argument.NotNull(suffix, "suffix");

			string errorMessage = "";

			if (!string.IsNullOrEmpty(suffix))
			{
				if (!suffix.StartsWith(SuffixSeparator))
				{
					errorMessage = string.Format((NoResString)"Suffix must start with: '{0}'.", SuffixSeparator);
				}
				else
				{
					suffix = suffix.Substring(SuffixSeparator.Length, suffix.Length - SuffixSeparator.Length);
					if (string.IsNullOrEmpty(suffix))
					{
						errorMessage = (NoResString)"Invalid suffix.";
					}
					else
					{
						var invalidCharacters = new String(suffix.Where(character => alphabet.IndexOf(character) == -1).ToArray());
						if (!string.IsNullOrEmpty(invalidCharacters))
						{
							errorMessage = string.Format((NoResString)"Suffix conatins invalid characters: '{0}'.", invalidCharacters);
						}
						else if (suffix.Length > maxSuffix.Length)
						{
							errorMessage = (NoResString)"Maximum length of suffixes is exceeded.";
						}
					}
				}
			}

			return errorMessage;
		}

		const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const string maxSuffix = "ZZ";
		public static readonly int MaxNumberRepresentation = 703;
	}
}
