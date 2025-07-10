using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	/// <summary>
	/// This Macro only works with decimal values and will raise exception if improper data is fed
	/// </summary>
	class GetBusinessObjectNumericValueWithZeroAsEmpty : GetBusinessObjectValue
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetBusinessObjectNumericValueWithZeroAsEmpty({businessobjecttype},{pk},{property})>",
				ResString.GetMultilingualString("5acfb79f-5bde-4671-922f-8aafc58aa46f",
				@"Works identically to the {0} macro but returns a blank string if the value is equal to zero. Please note that this will throw an exception (break badly) if you try and specify a non numeric property.",
				"GetBusinessObjectValue"),
				new List<(string example, object expectedResult)> { ("<GetBusinessObjectNumericValueWithZeroAsEmpty(CusEntryHeader, <EntryHEader.CH_PK>, CustomsValue)>", "") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result = base.GetReplacementCore(macro, report);
			if (result.Equals(ZInt.Zero) || result.Equals(ZDecimal.Zero) || result.Equals(ZLong.Zero))
			{
				result = ZString.Empty;
			}
			return result;
		}

		public override System.Text.RegularExpressions.Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"
			^<\s*GetBusinessObjectNumericValueWithZeroAsEmpty\s*\(\s*(\S+)\s*,\s*(\S+)\s*,\s*(\S+)\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
	}
}
