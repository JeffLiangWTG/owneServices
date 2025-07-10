using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class UTCTimeOffset : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<UTCTimeOffset>",
				ResString.GetMultilingualString("090780f2-2a69-4a94-9baa-0eb3c9575128", @"Returns the time difference in minutes between current time zone's standard time and UTC."),
				new List<(string example, object expectedResult)> { ("<UTCTimeOffset>", (int)ZDateTimeOffset.Now.Offset.TotalMinutes) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return (int)ZDateTimeOffset.Now.Offset.TotalMinutes;
		}

		public override Regex Regex => fRegex;
		static readonly Regex fRegex = new Regex(@"^<\s*UTCTimeOffset\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
