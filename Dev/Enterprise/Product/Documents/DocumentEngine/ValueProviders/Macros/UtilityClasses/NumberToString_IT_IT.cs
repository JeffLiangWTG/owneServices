using System.Collections.Generic;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_IT_IT : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_IT_IT()
		{
			primitiveNumbers = new string[] {
				"zero", "uno", "due", "tre", "quattro", "cinque", "sei", "sette", "otto", "nove",
				"dieci", "undici", "dodici", "tredici", "quattordici", "quindici", "sedici", "diciassette", "diciotto", "diciannove" };

			tens = new string[] { "", "", "venti", "trenta", "quaranta", "cinquanta", "sessanta", "settanta", "ottanta", "novanta" };

			tensPattern = "{0}{1}";
			oneHundredsPlacePattern = "cento";
			manyHundredsPlacePattern = "{0}cento";
			hundredsPattern = "{0}{1}";
			oneThosandPlacePattern = "mille";
			manyThosandPlacePattern = "{0}mila";
			oneMillionPlacePattern = "un milione";
			manyMillionPlacePattern = "{0} milioni";
			oneBillionPlacePattern = "un miliardo";
			manyBillionPlacePattern = "{0} miliardi";
			thousandsPattern = millionsPattern = billionsPattern = "{0} {1}";

			specialNumbers = new Dictionary<long, string>();
			specialNumbers.Add(21, "ventuno");
			specialNumbers.Add(31, "trentuno");
			specialNumbers.Add(41, "quarantuno");
			specialNumbers.Add(51, "cinquantuno");
			specialNumbers.Add(61, "sessantuno");
			specialNumbers.Add(71, "settantuno");
			specialNumbers.Add(81, "ottantuno");
			specialNumbers.Add(91, "novantuno");
			specialNumbers.Add(23, "ventitré");
			specialNumbers.Add(33, "trentatré");
			specialNumbers.Add(43, "quarantatré");
			specialNumbers.Add(53, "cinquantatré");
			specialNumbers.Add(63, "sessantatré");
			specialNumbers.Add(73, "settantatré");
			specialNumbers.Add(83, "ottantatré");
			specialNumbers.Add(93, "novantatré");
			specialNumbers.Add(28, "ventotto");
			specialNumbers.Add(38, "trentotto");
			specialNumbers.Add(48, "quarantotto");
			specialNumbers.Add(58, "cinquantotto");
			specialNumbers.Add(68, "sessantotto");
			specialNumbers.Add(78, "settantotto");
			specialNumbers.Add(88, "ottantotto");
			specialNumbers.Add(98, "novantotto");
		}

		public override string DecimalSeperatorAsString { get { return "virgola"; } }

		#endregion
	}
}
