using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LastDayOfLastWeek : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LastDayOfLastWeek>",
				ResString.GetMultilingualString("8ea31748-dc6f-4513-ba86-485afd73e5cd", @"Returns a date time value representing the last day of last week starting from the current date and time. 
See also: {0}.", "FirstDayOfLastWeek"),
				new List<(string example, object expectedResult)> { ("<LastDayOfLastWeek>", "2018-11-04") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			DateTime endOfWeek = Env.Time.CurrentLocalDateTime.AddDays(-(int)(Env.Time.CurrentLocalDateTime.DayOfWeek));
			return SqlFormatInfo.ToSqlDateString(endOfWeek);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Last(?:[\s]*)Day(?:[\s]*)Of(?:[\s]*)Last(?:[\s]*)Week(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
