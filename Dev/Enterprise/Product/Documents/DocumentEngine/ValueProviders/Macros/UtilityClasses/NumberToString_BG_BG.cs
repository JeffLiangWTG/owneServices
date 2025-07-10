
using System.Globalization;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_BG_BG : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_BG_BG()
		{
			primitiveNumbers = new string[] { "нула", "едно", "две", "три", "четири", "пет", "шест", "седем", "осем", "девет", "десет",
				"единадесет", "дванадесет", "тринадесет", "четиринадесет", "петнадесет", "шестнадесет", "седемнадесет", "осемнадесет", "деветнадесет" };
			tens = new string[] { "", "десет", "двадесет", "тридесет", "четиридесет", "петдесет", "шестдесет", "седемдесет", "осемдесет", "деветдесет" };
			hundreds = new string[] { "", "сто", "двеста", "триста", "четиристотин", "петстотин", "шестстотин", "седемстотин", "осемстотин", "деветстотин" };

			tensPattern = "{0} и {1}";
			hundredsPattern = "{0} {1}";

			oneThosandPlacePattern = "хиляда";
			manyThosandPlacePattern = "{0} хиляди";
			thousandsPattern = "{0} {1}";

			oneMillionPlacePattern = "един милион";
			manyMillionPlacePattern = "{0} милиона";
			millionsPattern = "{0} {1}";

			oneBillionPlacePattern = "един милиард";
			manyBillionPlacePattern = "{0} милиарда";
			billionsPattern = "{0} {1}";
		}

		public override string GetNumberAsString(long number)
		{
			if (number > 100 && number < 1000 && number % 100 > 0 && (number % 100 < 20 || number % 100 % 10 == 0))
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} и {1}", GetHundredsPlace(number), GetNumberAsString(number % 100));
			}
			else if (number > 1000 && number < million && number % 1000 > 0 && number % 1000 < 100)
			{
				string hundredsPlace = string.Format(CultureInfo.InvariantCulture, number / 1000 > 1 ? manyThosandPlacePattern : oneThosandPlacePattern, GetNumberAsString(number / 1000));
				return string.Format(CultureInfo.InvariantCulture, "{0} и {1}", hundredsPlace, GetNumberAsString(number % 1000));
			}
			else
			{
				return base.GetNumberAsString(number);
			}
		}

		public override string DecimalSeperatorAsString
		{
			get { return "запетая"; }
		}

		#endregion
	}
}
