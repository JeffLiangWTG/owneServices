using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public static class FilterRegexProvider
	{
		internal static Regex GetFilterWithWildcardsRegex(object filter)
			=> GetFilterWithWildcardsRegex(filter.ToString());

		public static Regex GetFilterWithWildcardsRegex(ZString filter)
		{
			var withNext = Regex.Escape(filter);
			withNext = WildcardMatcher.Replace(withNext, MatchEval);
			withNext = "^" + withNext + "$";
			return new Regex(withNext, RegexOptions.IgnoreCase);
		}

		static string MatchEval(Match match)
		{
			switch (match.Value)
			{
			case @"\*":
				//"*" acts as a wildcard for multiple chars
				return ".*";
			case @"\\\*":
				//"\*" acts as a non-wildcard *
				return @"\*";
			case @"\?":
				//"?" acts as a wildcard for a single char
				return @".";
			case @"\\\?":
				//"\?" acts as non-wildcard ?
				return @"\?";
			default:
				return match.Value;
			}
		}

		internal const string OneCharWildcard = "?";
		internal const string MultiCharWildcard = "*";
		const string Escape = @"\";

		static readonly Regex WildcardMatcher = new Regex(@"[\\]?[\\]?\\(\?|\*)");

		internal const string EscapedOneCharWildcard = Escape + OneCharWildcard;
		internal const string EscapedMultiCharWildcard = Escape + MultiCharWildcard;

		static readonly Regex ShortDateRegex = new Regex(
			@"([0-3|\?]?[0-9|\?]{1}|\*)\-((A|D|F|J|M|N|O|S|\?)(A|C|E|O|P|U|\?)(B|C|G|L|N|P|R|T|V|Y|\?)|\*)\-([0-9|\?]{2,4}|\*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);   //dd-MMM-yy (also accepts d-MMM-yyyy)

		static readonly Regex ReverseShortDateRegex = new Regex(
			@"([0-9|\?]{4}|\*)\-([0-1|\?]?[0-9|\?]{1}|\*)\-([0-3|\?]?[0-9|\?]{1}|\*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);   //yyyy-MM-dd (also accepts yyyy-M-d)

		static readonly Regex ShortTimeRegex = new Regex(
			@"([0-2|\?]?[0-9|\?]{1}|\*)\:([0-6|\?]{1}[0-9|\?]{1}|\*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);   //HH:mm (also accepts H:mm)

		static readonly Regex LongTimeRegex = new Regex(
			@"([0-3|\?]?[0-9|\?]{1}|\*)\-((A|D|F|J|M|N|O|S|\?)(A|C|E|O|P|U|\?)(B|C|G|L|N|P|R|T|V|Y|\?)|\*)\-([0-9|\?]{2,4}|\*)\s([0-2|\?]?[0-9|\?]{1}|\*)\:([0-6|\?]{1}[0-9|\?]{1}|\*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);   //dd-MMM-yy HH:mm (also accepts d-MMM-yyyy H:mm)

		static readonly Regex BestReadableDateRegex = new Regex(
			@"([0-3|\?]?[0-9|\?]{1}|\*)\s((A|D|F|J|M|N|O|S|\?)(A|C|E|O|P|U|\?)(B|C|G|L|N|P|R|T|V|Y|\?)|\*)\s([0-9|\?]{4}|\*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);   //dd MMM yyyy (also accepts d MMM yyyy)

		static readonly Regex BestReadableDateTimeRegex = new Regex(
			@"([0-3]?[0-9]{1}|\*)\s((A|D|F|J|M|N|O|S)(A|C|E|O|P|U)(B|C|G|L|N|P|R|T|V|Y)|\*)\s([0-9|\?]{4}|\*)\s([0-2|\?]?[0-9|\?]{1}|\*)\:([0-6|\?]{1}[0-9|\?]{1}|\*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);   //dd MMM yyyy HH:mm(also accepts d MMM yyyy H:mm)

		static readonly Regex RegexForHourWorkaround = new Regex(
			@"([0-2|\?]{1}[0-9|\?]{1}|\*)\:([0-6|\?]{1}[0-9|\?]{1}|\*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);	//Used to convert H:mm to HH:mm

		public static Match GetDateTimeFilterWithWildcardsRegexMatch(ZString filter, ZDateTime dateTime)
		{
			if (ShortTimeRegex.Match(filter).Success)
			{
				var wildcardFilter = GetFilterWithWildcardsRegex(
					RegexForHourWorkaround.Match(filter).Success ?
					(string)filter :
					ShortTimeRegex.Replace(filter, @"0" + filter)); //Converting any H:mm to HH:mm

				if (LongTimeRegex.Match(filter).Success)
				{
					return wildcardFilter.Match(dateTime.ToLongTimeString());
				}
				else if (BestReadableDateTimeRegex.Match(filter).Success)
				{
					return wildcardFilter.Match(dateTime.ToBestReadableDateTimeString());
				}
				else
				{
					return wildcardFilter.Match(dateTime.ToShortTimeString());
				}
			}
			else
			{
				var wildcardFilter = GetFilterWithWildcardsRegex(filter);

				if (ShortDateRegex.Match(filter).Success)
				{
					return wildcardFilter.Match(dateTime.ToShortDateString());
				}
				else if (ReverseShortDateRegex.Match(filter).Success)
				{
					return wildcardFilter.Match(dateTime.ToISO8601ShortDateString());
				}
				else if (BestReadableDateRegex.Match(filter).Success)
				{
					return wildcardFilter.Match(dateTime.ToBestReadableDateString());
				}
				else
				{
					return Match.Empty;
				}
			}
		}
	}
}
