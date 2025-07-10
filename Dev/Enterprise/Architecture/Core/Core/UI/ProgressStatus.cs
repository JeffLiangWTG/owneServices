using System;
using System.Diagnostics;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public class ProgressStatus
	{
		readonly int numberToProcess;
		readonly Stopwatch timeElapsed;

		public ProgressStatus(int numberToProcess)
		{
			this.numberToProcess = numberToProcess;
			timeElapsed = Stopwatch.StartNew();
		}

		public string GetStatusReport(int itemsProcessed, bool isShortMessage = false)
		{
			var remainingTimeString = itemsProcessed == 0 ?
				Res.GetString("D6EC1544-C3D3-45A7-8A96-354E0485BBE6", "Unknown") :
				FormatTime(EstimateRemainingTime(timeElapsed.Elapsed, itemsProcessed, numberToProcess));

			return isShortMessage
				? Res.GetString("7623D643-376D-490C-91D9-BB469DE52A71", "{0} of {1} records, {2} left.", itemsProcessed, numberToProcess, remainingTimeString)
				: Res.GetString("22125e85-409d-4616-8783-b0d92cb38015", "Items Processed: {0} / {1}.\r\nEstimated Time Remaining: {2}.", itemsProcessed, numberToProcess, remainingTimeString);
		}

#if DEBUG
		internal
#endif
		static TimeSpan EstimateRemainingTime(TimeSpan ellapsed, int itemsProcessed, int amountToProcess)
		{
			var averageTimePerItem = ellapsed.Ticks / itemsProcessed;
			var itemsRemaining = amountToProcess - itemsProcessed;

			return TimeSpan.FromTicks(averageTimePerItem * itemsRemaining);
		}

		internal static string FormatTime(TimeSpan time)
		{
			if (time.TotalDays >= 2)
			{
				return Res.GetString("Time|C9270FE8-08F2-4EA8-8883-A9DAB1546F3C", "{0} days", time.TotalDays.ToString("#0.#", Culture.Current));
			}
			else if (time.TotalHours >= 2)
			{
				return Res.GetString("Time|16B1FF62-30E2-4F17-85F8-228B504326B7", "{0} hours", time.TotalHours.ToString("#0.#", Culture.Current));
			}
			else if (time.TotalMinutes >= 2)
			{
				return Res.GetString("Time|E7E3AB83-21CC-45B8-B363-51D42CA222B1", "{0} minutes", time.TotalMinutes.ToString("#0.#", Culture.Current));
			}
			else
			{
				return Res.GetString("Time|B5CDE8F2-A7F0-448C-9874-4F3170370696", "{0} seconds", time.TotalSeconds.ToString("#0", Culture.Current));
			}
		}
	}
}
