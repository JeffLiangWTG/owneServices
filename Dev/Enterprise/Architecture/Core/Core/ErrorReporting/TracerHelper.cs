using System;
using System.Text.RegularExpressions;

namespace Enterprise.ZArchitecture.Core
{
	static class TracerHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		internal static string RemoveReportElements(string trace)
		{
			Regex regex = new Regex(@"(HandleThreadException|ErrorReporter\.Report|ShowDeveloperException)");
			MatchCollection matches = regex.Matches(trace);

			if (matches.Count > 0)
			{
				var previousMatchIndex = -1;
				var usePrevious = false;
				foreach (var matchObject in matches)
				{
					var match = matchObject as Match;
					if (string.Equals(match.ToString(), "ErrorReporter.Report", StringComparison.Ordinal))
					{
						if (previousMatchIndex > 0)
						{
							if (trace.Substring(previousMatchIndex, match.Index - previousMatchIndex).CountMatches("   at ") > 1)
							{
								usePrevious = true;
								break;
							}
						}
						previousMatchIndex = match.Index;
					}
				}
				// keep the first report element
				string reportElements = trace.Substring(0, usePrevious ? previousMatchIndex : matches[matches.Count - 1].Index);
				trace = trace.Remove(0, reportElements.LastIndexOf("   at"));
			}

			return trace;
		}
	}
}
