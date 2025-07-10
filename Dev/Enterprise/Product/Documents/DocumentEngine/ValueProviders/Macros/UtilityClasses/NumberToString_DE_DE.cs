
namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_DE_DE : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_DE_DE()
		{
			primitiveNumbers = new string[] {
				"null", "eins", "zwei", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun",
				"zehn", "elf", "zwölf", "dreizehn", "vierzehn", "fünfzehn", "sechszehn", "siebzehn", "achtzehn", "neunzehn" };

			tens = new string[] { "", "", "zwanzig", "dreißig", "vierzig", "fünfzig", "sechzig", "siebzig", "achtzig", "neunzig" };

			tensPattern = "{1}und{0}";
			oneHundredsPlacePattern = "einhundert";
			manyHundredsPlacePattern = "{0}hundert";
			hundredsPattern = "{0}{1}";
			oneThosandPlacePattern = "eintausend";
			manyThosandPlacePattern = "{0}tausend";
			thousandsPattern = "{0}{1}";
			oneMillionPlacePattern = "eine Million";
			manyMillionPlacePattern = "{0} Millionen";
			oneBillionPlacePattern = "eine Millarde";
			manyBillionPlacePattern = "{0} Millarden";
			millionsPattern = billionsPattern = "{0} {1}";
		}

		public override string DecimalSeperatorAsString { get { return "Komma"; } }

		#endregion
	}
}
