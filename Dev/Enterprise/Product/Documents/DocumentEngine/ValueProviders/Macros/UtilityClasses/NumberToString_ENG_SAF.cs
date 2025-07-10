namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_ENG_SAF : SouthAsiaNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_ENG_SAF()
		{
			primitiveNumbers = new string[] {
				"zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
				"ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

			tens = new string[] { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

			tensPattern = "{0} {1}";
			oneHundredsPlacePattern = manyHundredsPlacePattern = "{0} hundred";
			hundredsPattern = "{0} and {1}";
			oneThousandPlacePattern = manyThousandPlacePattern = "{0} thousand";
			oneLakhPlacePattern = manyLakhPlacePattern = "{0} lakh";
			oneCrorePlacePattern = manyCrorePlacePattern = "{0} crore";
			thousandsPattern = LakhsPattern = CroresPattern = "{0}, {1}";
		}

		public override string DecimalSeperatorAsString { get { return "point"; } }

		public static string ConvertNumberToWords(long number)
		{
			return new NumberToString_ENG_SAF().GetNumberAsString(number);
		}

		#endregion
	}
}
