using System.Globalization;
using Enterprise.DocumentEngine.Exceptions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	#region SuppressResourceStringsCheckRegion

	public class NumberToString_VI_VN : INumberToWords
	{
		[ThreadSafe]
		static readonly string[] primitiveNumbers = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
		[ThreadSafe]
		static readonly string[] primitiveNumbersSpecialCase = { "không", "mốt", "hai", "ba", "bốn", "lăm", "sáu", "bảy", "tám", "chín" };
		[ThreadSafe]
		static readonly string[] tens = { "mươi", "trăm", "nghìn", "triệu", "tỷ" };
		const string ten = "mười";
		const string fifteen = "mười lăm";
		const string extraWordForMissingTensPlace = "lẻ ";
		const string extraWordsForMissingHundredsPlace = "không trăm ";
		[ThreadSafe]
		readonly static long[] tensNumbers = { 10, 100, 1000, 1000000, 1000000000, 1000000000000 };

		public long Number { get; set; }

		public string DecimalSeperatorAsString { get { return ""; } }

		public string GetNumberAsString(long number)
		{
			return VietnameseConvertNumberToWords(number);
		}

		public static string VietnameseConvertNumberToWords(long number)
		{
			if (number < 0)
			{
				throw new FormulaProviderException("You cannot pass a negative number to the <NumberToWords> or <CurrencyToWords> macro.");
			}
			if (number < 10)
			{
				return primitiveNumbers[number];
			}
			if (number == 10)
			{
				return ten;
			}
			if (number == 15)
			{
				return fifteen;
			}
			if (number < 20)
			{
				return ten + " " + primitiveNumbers[number % 10];
			}
			for (int i = 1; i < tensNumbers.Length; i++)
			{
				if (number == tensNumbers[i])
				{
					return primitiveNumbers[1] + " " + tens[i];
				}
				else if (number < tensNumbers[i])
				{
					string secondPart = "";
					long secondPartNumber = number % tensNumbers[i - 1];
					if (secondPartNumber != 0)
					{
						if (i == 1)
						{
							secondPart = primitiveNumbersSpecialCase[secondPartNumber];
						}
						else
						{
							if (i > 2)
							{
								if (secondPartNumber / (tensNumbers[i - 1] / 10) == 0)
								{
									secondPart += extraWordsForMissingHundredsPlace;
								}
								if (secondPartNumber / (tensNumbers[i - 1] / 100) == 0)
								{
									secondPart += extraWordForMissingTensPlace;
								}
							}
							else if (i > 1 && secondPartNumber / (tensNumbers[i - 1] / 10) == 0)
							{
								secondPart += extraWordForMissingTensPlace;
							}
							secondPart += VietnameseConvertNumberToWords(number % tensNumbers[i - 1]);
						}
					}
					return VietnameseConvertNumberToWords(number / tensNumbers[i - 1]) + " " + tens[i - 1] + " " + secondPart;
				}
			}
			return number.ToString(CultureInfo.InvariantCulture);
		}
	}

	#endregion
}
