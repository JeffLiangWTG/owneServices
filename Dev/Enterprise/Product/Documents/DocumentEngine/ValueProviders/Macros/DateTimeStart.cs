using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	partial class DateTimeStart : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DateTimeStart>",
				ResString.GetMultilingualString("7660b711-9c81-477e-a00d-f2a29cdd1605", "Returns the starting Date/Time when the Report or Document is generated."),
				new List<(string example, object expectedResult)> { ("<DateTimeStart>", new DateTime(2018, 11, 7)) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (report.StartTime == DateTime.MinValue)
			{
				report.SetStartTime();
			}
			return report.StartTime;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Date(?:[\s]*)Time(?:[\s]*)Start(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
