using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	internal class SendingNudgeErrorCounter
	{
		SendingNudgeErrorCounter() { }

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<SendingNudgeErrorCounter> lazy = new Lazy<SendingNudgeErrorCounter>(() => new SendingNudgeErrorCounter());
		public static SendingNudgeErrorCounter Instance => lazy.Value;

		List<ZDateTime> counterTimes;
		List<ZDateTime> CounterTimes => counterTimes ?? (counterTimes = new List<ZDateTime>());

		WebPrintNudgeSuspending NudgeSuspending => DocumentsDataRegistry.Instance.WebPrintNudgeSuspending.Value;

		ZDateTime suspendedUntil;
		readonly object locker = new object();

		public void AddErrorCount()
		{
			lock (locker)
			{
				CounterTimes.Add(ZDateTime.UtcNow);
			}
		}

		public bool IsSuspendSendingNudge()
		{
			lock (locker)
			{
				if (suspendedUntil.IsValid)
				{
					if (CounterTimes.Count > 0)
					{
						CounterTimes.Clear();
					}

					if (suspendedUntil > ZDateTime.UtcNow)
					{
						return true;
					}

					suspendedUntil = ZDateTime.Empty;
				}

				var maxErrorsInHours = NudgeSuspending.MaxErrorsInHours;
				var maxErrorsInMinutes = NudgeSuspending.MaxErrorsInMinutes;

				if (CounterTimes.Count >= maxErrorsInHours)
				{
					CounterTimes.RemoveRange(0, CounterTimes.Count - maxErrorsInHours);

					var last = CounterTimes[CounterTimes.Count - 1];
					var tenthFrom = CounterTimes[CounterTimes.Count - maxErrorsInHours];
					if (tenthFrom.AddHours(NudgeSuspending.IntervalHours) > last)
					{
						suspendedUntil = ZDateTime.UtcNow.AddHours(NudgeSuspending.SuspendHours);
						return true;
					}
				}
				else if (CounterTimes.Count >= maxErrorsInMinutes)
				{
					var last = CounterTimes[CounterTimes.Count - 1];
					var thirdFromLast = CounterTimes[CounterTimes.Count - maxErrorsInMinutes];
					if (thirdFromLast.AddMinutes(NudgeSuspending.IntervalMinutes) > last)
					{
						suspendedUntil = ZDateTime.UtcNow.AddHours(NudgeSuspending.SuspendMinutes);
						return true;
					}
				}

				return false;
			}
		}

#if DEBUG

		internal void ClearCounterTimesForTest()
		{
			CounterTimes.Clear();
			suspendedUntil = ZDateTime.Empty;
		}

		internal void AddErrorCountForTest(ZDateTime time)
		{
			lock (locker)
			{
				CounterTimes.Add(time);
			}
		}

		internal int ErrorCount_Exposed => CounterTimes.Count;

#endif
	}
}
