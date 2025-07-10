#region SuppressResourceStringsCheckRegion

using System.Globalization;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	abstract public class ChineseNumberToWords : INumberToWords
	{
		protected string[] PrimitiveNumbers;

		protected string[] Tens;

		protected string Sign;

		readonly long[] TensNumbers = { 10, 100, 1000, 10000, 100000000, 100000000000 };

		public string GetNumberAsString(long number)
		{
			var prefix = "";
			if (number < 0)
			{
				prefix = Sign;
				number *= -1;
			}

			if (number < 10)
			{
				return prefix + PrimitiveNumbers[number];
			}

			for (int i = 1; i < TensNumbers.Length; i++)
			{
				if (number == TensNumbers[i])
				{
					return prefix + PrimitiveNumbers[1] + Tens[i];
				}
				else if (number < TensNumbers[i])
				{
					string secondPart = "";
					long secondPartNumber = number % TensNumbers[i - 1];
					if (secondPartNumber != 0)
					{
						secondPart = GetNumberAsString(secondPartNumber);
					}
					long firstDigitInSecondPart = secondPartNumber * 10 / TensNumbers[i - 1];

					return prefix + GetNumberAsString(number / TensNumbers[i - 1]) + Tens[i - 1] + (secondPartNumber != 0 && firstDigitInSecondPart == 0 ? PrimitiveNumbers[0] : "") + secondPart;
				}
			}

			return number.ToString(CultureInfo.CurrentCulture);
		}

		public string DecimalSeperatorAsString
		{
			get { return ""; }
		}
	}

	public class NumberToString_ZH_CN : ChineseNumberToWords
	{
		public NumberToString_ZH_CN()
		{
			PrimitiveNumbers = new string[] { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
			Tens = new string[] { "拾", "佰", "仟", "万", "亿", "仟亿" };
			Sign = "负";
		}
	}
}

#endregion
