using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Absolute : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Absolute({decimalamount})>",
				ResString.GetMultilingualString("3833da0d-0dbb-46de-b207-e5ae0b16ad8b", @"Returns the absolute value of specified amount."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Absolute(<ARInvoice.OSTotal>)>", 50m) }
				);
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			string amountAsString = match.Groups[1].Value;

			if (string.IsNullOrEmpty(amountAsString))
			{
				return "";
			}

			return TryParseWithReportOnFail(report, 0m, () => Math.Abs(decimal.Parse(amountAsString)));
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Absolute(?:[\s]*)\((?:[\s]*)([^\s]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
