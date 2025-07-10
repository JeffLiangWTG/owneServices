namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_EN : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_EN()
		{
			primitiveNumbers = new string[] {
				"zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
				"ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

			tens = new string[] { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

			tensPattern = "{0} {1}";
			oneHundredsPlacePattern = manyHundredsPlacePattern = "{0} hundred";
			hundredsPattern = "{0} and {1}";
			oneThosandPlacePattern = manyThosandPlacePattern = "{0} thousand";
			oneMillionPlacePattern = manyMillionPlacePattern = "{0} million";
			oneBillionPlacePattern = manyBillionPlacePattern = "{0} billion";
			thousandsPattern = millionsPattern = billionsPattern = "{0}, {1}";
		}

		public override string DecimalSeperatorAsString { get { return "point"; } }

		public static string ConvertNumberToWords(long number)
		{
			return new NumberToString_EN().GetNumberAsString(number);
		}

		#endregion
	}
}
