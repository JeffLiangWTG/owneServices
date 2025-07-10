using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros.UtilityClasses;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class DateDiff : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:\s*)DateDiff(?:\s*)\((?:\s*)(?<operands>(.*))\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DateDiff(\"startDate\", \"endDate\", \"unitType\")>",
				ResString.GetMultilingualString("82B1EBFF-9406-4053-A886-85B0DAD50E9C", "Returns the time between the two dates in the specified unit, rounded down to a whole number."),
				new List<(string example, object expectedResult)> {
					((NoResString)"<DateDiff(\"2023-02-22\", \"2023-02-25\", \"DAYS\")>", new ZString("3")),
					((NoResString)"<DateDiff(\"2016-12-20 4:00 PM\", \"2016-12-21 1:00 AM\", \"HOURS\")>", new ZString("9")),
					((NoResString)"<DateDiff(\"March 01 2008 7:44 PM\", \"2008-03-01 9:56 PM\", \"MINUTES\")>", new ZString("132")),
					((NoResString)"<DateDiff(\"14-07-2026 22:10:44\", \"14-07-2026 22:10:16\", \"SECONDS\")>", new ZString("-28")),
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			var match = Regex.Match(macro);
			string[] args = match.Groups["operands"].Value.Split(',');

			if (args.Length != 3)
			{
				ReportMacroError(report, Res.GetString("D8432F67-2A5B-487D-B267-7F5BAF1BAD04", "Unexpected number of arguments"));
			}
			else
			{
				string startDateAsString = args[0].Trim();
				string endDateAsString = args[1].Trim();
				string timeUnit = args[2].Trim();
				var quotesRegex = new Regex(@"^""(?<value>(.*))""$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

				if (!quotesRegex.Match(startDateAsString).Success)
				{
					ReportMacroError(report, Res.GetString("1FEA0679-71C1-4CBE-BF4F-1A9C6C56CDEC", "Start date must be within quotation marks"));
				}
				else if (!quotesRegex.Match(endDateAsString).Success)
				{
					ReportMacroError(report, Res.GetString("63EB4120-32CA-466E-84CC-3DCAD6F3DBB0", "End date must be within quotation marks"));
				}
				else if (!quotesRegex.Match(timeUnit).Success)
				{
					ReportMacroError(report, Res.GetString("8BBEBF67-C6D8-4624-A636-1C8264A280FD", "Time unit must be within quotation marks"));
				}
				else
				{
					startDateAsString = quotesRegex.Match(startDateAsString).Groups["value"].Value;
					endDateAsString = quotesRegex.Match(endDateAsString).Groups["value"].Value;
					timeUnit = quotesRegex.Match(timeUnit).Groups["value"].Value.Trim();
					DateTime startDate;
					DateTime endDate;

					if (!DateTime.TryParse(startDateAsString, out startDate))
					{
						ReportMacroError(report, Res.GetString("80B7382E-7FB7-43D9-9CDA-6DB5CAE75B8B", "Invalid start date"));
					}
					else if (!DateTime.TryParse(endDateAsString, out endDate))
					{
						ReportMacroError(report, Res.GetString("7D4F7E30-87CA-4157-8A27-E1A09A37D72F", "Invalid end date"));
					}
					else
					{
						TimeSpan diff = endDate - startDate;

						if (timeUnit == TimeUnits.Days)
						{
							result = Math.Floor(diff.TotalDays).ToString();
						}
						else if (timeUnit == TimeUnits.Hours)
						{
							result = Math.Floor(diff.TotalHours).ToString();
						}
						else if (timeUnit == TimeUnits.Minutes)
						{
							result = Math.Floor(diff.TotalMinutes).ToString();
						}
						else if (timeUnit == TimeUnits.Seconds)
						{
							result = Math.Floor(diff.TotalSeconds).ToString();
						}
						else if (timeUnit == TimeUnits.Milliseconds)
						{
							result = Math.Floor(diff.TotalMilliseconds).ToString();
						}
						else
						{
							ReportMacroError(report, Res.GetString("FED05F03-DE93-4D49-AFB1-88BA59018B45", "Invalid time unit"));
						}
					}
				}
			}

			return result;
		}
	}
}
