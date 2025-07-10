using System.Globalization;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_RU_RU : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_RU_RU()
		{
			primitiveNumbers = new string[] { "ноль", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять",
				"десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" };
			tens = new string[] { "", "десять", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто" };
			hundreds = new string[] { "", "сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот" };

			tensPattern = "{0} {1}";
			hundredsPattern = "{0} {1}";

			oneThosandPlacePattern = "одна тысяча";
			manyThosandPlacePattern = "{0} тысяч";
			thousandsPattern = "{0} {1}";

			oneMillionPlacePattern = "один миллион";
			manyMillionPlacePattern = "{0} миллионов";
			millionsPattern = "{0} {1}";

			oneBillionPlacePattern = "один миллиард";
			manyBillionPlacePattern = "{0} миллиардов";
			billionsPattern = "{0} {1}";
		}

		const string twoThousandPlacePattern = "две тысячи";
		const string xAndOneThousandPlacePattern = "{0} одна тысяча";
		const string twoToFourThousandPlacePattern = "{0} тысячи";

		const string xAndOneMillionPlacePattern = "{0} один миллион";
		const string twoToFourMillionPlacePattern = "{0} миллиона";

		const string xAndOneBillionPlacePattern = "{0} один миллиард";
		const string twoToFourBillionPlacePattern = "{0} миллиарда";

		public override string DecimalSeperatorAsString
		{
			get { return "запятая"; }
		}

		public override string GetNumberAsString(long number)
		{
			if (number >= 1000 && number < million)
			{
				return GetNumberAsStringFor2To4Groups(number, 1000, twoToFourThousandPlacePattern, thousandsPattern, xAndOneThousandPlacePattern);
			}
			else if (number >= million && number < billion)
			{
				return GetNumberAsStringFor2To4Groups(number, million, twoToFourMillionPlacePattern, millionsPattern, xAndOneMillionPlacePattern);
			}
			else if (number >= billion && number < trillion)
			{
				return GetNumberAsStringFor2To4Groups(number, billion, twoToFourBillionPlacePattern, billionsPattern, xAndOneBillionPlacePattern);
			}
			else
			{
				return base.GetNumberAsString(number);
			}
		}

		string GetNumberAsStringFor2To4Groups(long number, long rank, string twoToFourPattern, string rankPattern, string xAndOnePlacePattern)
		{
			string result;

			int numberOfGroups = (int)(number / rank);
			int units = numberOfGroups % 10;
			int tens = numberOfGroups / 10;

			if (rank == 1000 && numberOfGroups == 2)
			{
				var groupPlace = twoThousandPlacePattern;
				result = number % rank > 0 ? string.Format(CultureInfo.CurrentCulture, thousandsPattern, twoThousandPlacePattern, GetNumberAsString(number % rank)) : groupPlace;
			}
			else if (units == 1 && tens >= 2)
			{
				var groupPlace = string.Format(CultureInfo.CurrentCulture, xAndOnePlacePattern, GetNumberAsString(numberOfGroups - 1));
				result = number % rank > 0 ? string.Format(CultureInfo.CurrentCulture, rankPattern, groupPlace, GetNumberAsString(number % rank)) : groupPlace;
			}
			else if (units >= 2 && units <= 4 && tens != 1)
			{
				var groupPlace = string.Format(CultureInfo.CurrentCulture, twoToFourPattern, GetNumberAsString(numberOfGroups));
				result = number % rank > 0 ? string.Format(CultureInfo.CurrentCulture, rankPattern, groupPlace, GetNumberAsString(number % rank)) : groupPlace;
			}
			else
			{
				result = base.GetNumberAsString(number);
			}

			return result;
		}

		#endregion
	}
}
