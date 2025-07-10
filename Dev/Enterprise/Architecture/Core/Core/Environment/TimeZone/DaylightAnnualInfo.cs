using System;

namespace Enterprise.ZArchitecture.Environment
{
	class DaylightAnnualInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ResStringing causes stack overflow")]
		public DaylightAnnualInfo(DateTime startTransitionUtc, DateTime endTransitionUtc)
		{
			CheckSameYear("Start", startTransitionUtc, "End", endTransitionUtc);

			if (startTransitionUtc > endTransitionUtc)
			{
				firstTransition = endTransitionUtc;
				lastTransition = startTransitionUtc;
				firstTransitionType = DstTransitionType.End;
				lastTransitionType = DstTransitionType.Start;
			}
			else
			{
				firstTransition = startTransitionUtc;
				lastTransition = endTransitionUtc;
				firstTransitionType = DstTransitionType.Start;
				lastTransitionType = DstTransitionType.End;
			}
		}

		/// <summary>
		/// Returns a value indicating whether the specified date and time (UTC) is within a daylight saving time period.
		/// </summary>
		/// <param name="utcDateTime">A UTC time.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ResStringing causes stack overflow")]
		public bool IsDaylightSavingBasedOnUtc(DateTime utcDateTime)
		{
			CheckSameYear("DateTime to check", utcDateTime, "Start/End", firstTransition);

			bool result = false;

			if (firstTransition != lastTransition)
			{
				if (utcDateTime < firstTransition)
				{
					result = (firstTransitionType == DstTransitionType.End);
				}
				else if (utcDateTime < lastTransition)
				{
					result = (lastTransitionType == DstTransitionType.End);
				}
				else
				{
					result = (lastTransitionType == DstTransitionType.Start);
				}
			}

			return result;
		}

		void CheckSameYear(string dateTime1Name, DateTime dateTime1, string dateTime2Name, DateTime dateTime2)
		{
			if (dateTime1.Year != dateTime2.Year)
			{
				throw new ArgumentException(string.Format(
					"{0} year ({1}) and {2} year ({3}) MUST be the same.",
					dateTime1Name, dateTime1.Year.ToString(),
					dateTime2Name, dateTime2.Year.ToString()));
			}
		}

		readonly DateTime firstTransition;
		readonly DateTime lastTransition;
		readonly DstTransitionType firstTransitionType;
		readonly DstTransitionType lastTransitionType;
	}
}
