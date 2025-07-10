using System.Globalization;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	#region SuppressResourceStringsCheckRegion

	public class NumberToString_LVL : IndoEuropeanNumberToString
	{
		public NumberToString_LVL()
		{
			primitiveNumbers = new string[] {
				"nulle", "viens", "divi", "trīs", "četri", "pieci", "seši", "septiņi", "astoņi", "deviņi",
				"desmit", "vienpadsmit", "divpadsmit", "trīspadsmit", "četrpadsmit", "piecpadsmit", "sešpadsmit", "septiņpadsmit", "astoņpadsmit", "deviņpadsmit" };

			tens = new string[] { "", "", "divdesmit", "trīsdesmit", "četrdesmit", "piecdesmit", "sešdesmit", "septiņdesmit", "astoņdesmit", "deviņdesmit" };

			tensPattern = "{0} {1}";
			oneHundredsPlacePattern = "simts";
			manyHundredsPlacePattern = "{0} simti";
			hundredsPattern = "{0} {1}";
			oneThosandPlacePattern = "tūkstotis";
			manyThosandPlacePattern = "{0} tūkstoši";
			oneMillionPlacePattern = "miljons";
			manyMillionPlacePattern = "{0} miljoni";
			oneBillionPlacePattern = "miljards";
			manyBillionPlacePattern = "{0} miljardi";
			thousandsPattern = millionsPattern = billionsPattern = "{0} {1}";
		}

		const string manyThosandPlacePatternInSingular = "{0} tūkstotis";
		const string manyMillionPlacePatternInSingular = "{0} miljons";
		const string manyBillionPlacePatternInSingular = "{0} miljards";

		public override string DecimalSeperatorAsString { get { return "punkts"; } }

		public override string GetNumberAsString(long number)
		{
			if (number > 1000 && number < million)
			{
				return GetNumberAsStringForPluralForm(number, 1000, thousandsPattern, oneThosandPlacePattern, manyThosandPlacePatternInSingular, manyThosandPlacePattern);
			}
			else if (number > million && number < billion)
			{
				return GetNumberAsStringForPluralForm(number, million, millionsPattern, oneMillionPlacePattern, manyMillionPlacePatternInSingular, manyMillionPlacePattern);
			}
			else if (number > billion && number < trillion)
			{
				return GetNumberAsStringForPluralForm(number, billion, billionsPattern, oneBillionPlacePattern, manyBillionPlacePatternInSingular, manyBillionPlacePattern);
			}
			else
			{
				return base.GetNumberAsString(number);
			}
		}

		string GetNumberAsStringForPluralForm(long number, long rank, string rankPattern, string oneRankPatternn, string manyRankPatternInSingular, string manyRanksPattern)
		{
			string groupPlace;

			var numberOfGroups = (int)(number / rank);
			if (numberOfGroups == 1)
			{
				groupPlace = oneRankPatternn;
			}
			else if (!IsSingular(number, rank))
			{
				groupPlace = string.Format(CultureInfo.CurrentCulture, manyRanksPattern, GetNumberAsString(numberOfGroups));
			}
			else
			{
				groupPlace = string.Format(CultureInfo.CurrentCulture, manyRankPatternInSingular, GetNumberAsString(numberOfGroups));
			}

			return number % rank > 0 ? string.Format(CultureInfo.CurrentCulture, rankPattern, groupPlace, GetNumberAsString(number % rank)) : groupPlace;
		}

		public bool IsSingular(long number, long rank)
		{
			var numberOfGroups = (int)(number / rank);
			var tensAndUnitsOfGroups = numberOfGroups % 100;
			int units = numberOfGroups % 10;

			if ((tensAndUnitsOfGroups > 1 && tensAndUnitsOfGroups < 21) || (tensAndUnitsOfGroups > 20 && units != 1))
			{
				return false;
			}
			return true;
		}
	}

	#endregion
}
