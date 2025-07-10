using System.Collections.Generic;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_FR_FR : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_FR_FR()
		{
			primitiveNumbers = new string[] {
				"zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf",
				"dix", "onze", "douze", "treize", "quatorze", "quinze", "seize", "dix-sept", "dix-huit", "dix-neuf" };

			tens = new string[] { "", "", "vingt", "trente", "quarante", "cinquante", "soixante", "soixante-dix", "quatre-vingt", "quatre-vingt-dix" };

			tensPattern = "{0}-{1}";
			oneHundredsPlacePattern = "cent";
			manyHundredsPlacePattern = "{0} cent";
			oneThosandPlacePattern = "mille";
			manyThosandPlacePattern = "{0} mille";
			oneMillionPlacePattern = "un million";
			manyMillionPlacePattern = "{0} millions";
			oneBillionPlacePattern = "un milliard";
			manyBillionPlacePattern = "{0} milliards";
			hundredsPattern = thousandsPattern = millionsPattern = billionsPattern = "{0} {1}";

			specialNumbers = new Dictionary<long, string>();
			specialNumbers.Add(21, "vingt-et-un");
			specialNumbers.Add(71, "soixante-et-onze");
			specialNumbers.Add(72, "soixante-douze");
			specialNumbers.Add(73, "soixante-treize");
			specialNumbers.Add(74, "soixante-quatorze");
			specialNumbers.Add(75, "soixante-quinze");
			specialNumbers.Add(76, "soixante-seize");
			specialNumbers.Add(80, "quatre-vingts");
			specialNumbers.Add(91, "quatre-vingt-onze");
			specialNumbers.Add(92, "quatre-vingt-douze");
			specialNumbers.Add(93, "quatre-vingt-treize");
			specialNumbers.Add(94, "quatre-vingt-quatorze");
			specialNumbers.Add(95, "quatre-vingt-quinze");
			specialNumbers.Add(96, "quatre-vingt-seize");
			specialNumbers.Add(200, "deux cents");
			specialNumbers.Add(300, "trois cents");
			specialNumbers.Add(400, "quatre cents");
			specialNumbers.Add(500, "cinq cents");
			specialNumbers.Add(600, "six cents");
			specialNumbers.Add(700, "sept cents");
			specialNumbers.Add(800, "huit cents");
			specialNumbers.Add(900, "neuf cents");
		}

		public override string DecimalSeperatorAsString { get { return "virgule"; } }

		#endregion
	}
}
