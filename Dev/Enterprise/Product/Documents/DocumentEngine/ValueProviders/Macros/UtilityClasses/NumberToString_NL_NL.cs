using System.Collections.Generic;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_NL_NL : IndoEuropeanNumberToString
	{
		#region SuppressResourceStringsCheckRegion

		public NumberToString_NL_NL()
		{
			primitiveNumbers = new string[] {
				"nul", "één", "twee", "drie", "vier", "vijf", "zes", "zeven", "acht", "negen",
				"tien", "elf", "twaalf", "dertien", "veertien", "vijftien", "zestien", "zeventien", "achttien", "negentien" };

			tens = new string[] { "", "", "twintig", "dertig", "veertig", "vijftig", "zestig", "zeventig", "tachtig", "negentig" };

			tensPattern = "{1}en{0}";
			oneHundredsPlacePattern = "honderd";
			manyHundredsPlacePattern = "{0}honderd";
			hundredsPattern = "{0}-en-{1}";
			oneThosandPlacePattern = "duizend";
			manyThosandPlacePattern = "{0}duizend";
			oneMillionPlacePattern = manyMillionPlacePattern = "{0} miljoen";
			oneBillionPlacePattern = manyBillionPlacePattern = "{0} miljard";
			thousandsPattern = millionsPattern = billionsPattern = "{0} {1}";

			specialNumbers = new Dictionary<long, string>();
			specialNumbers.Add(22, "tweeëntwintig");
			specialNumbers.Add(32, "tweeëndertig");
			specialNumbers.Add(42, "tweeënveertig");
			specialNumbers.Add(52, "tweeënvijftig");
			specialNumbers.Add(62, "tweeënzestig");
			specialNumbers.Add(72, "tweeënzeventig");
			specialNumbers.Add(82, "tweeëntachtig");
			specialNumbers.Add(92, "tweeënnegentig");
			specialNumbers.Add(23, "drieëntwintig");
			specialNumbers.Add(33, "drieëndertig");
			specialNumbers.Add(43, "drieënveertig");
			specialNumbers.Add(53, "drieënvijftig");
			specialNumbers.Add(63, "drieënzestig");
			specialNumbers.Add(73, "drieënzeventig");
			specialNumbers.Add(83, "drieëntachtig");
			specialNumbers.Add(93, "drieënnegentig");
		}

		public override string DecimalSeperatorAsString { get { return "komma"; } }

		#endregion
	}
}
