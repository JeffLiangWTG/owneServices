using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FirstDayOfLastWeek : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FirstDayOfLastWeek>",
				ResString.GetMultilingualString("4c50f6f7-9cd7-46f4-8594-51da82756fa0", @"Returns a date time value representing the first day of last week starting from the current date and time. 
See also: {0}.", "LastDayOfLastWeek"),
				new List<(string example, object expectedResult)> { ("<FirstDayOfLastWeek>", "2018-10-29") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			DateTime startOfWeek = (Env.Time.CurrentLocalDateTime.AddDays(-(int)(Env.Time.CurrentLocalDateTime.DayOfWeek))).AddDays(-6);
			return SqlFormatInfo.ToSqlDateString(startOfWeek);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)First(?:[\s]*)Day(?:[\s]*)Of(?:[\s]*)Last(?:[\s]*)Week(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
