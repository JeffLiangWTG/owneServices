#if DEBUG

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Common.MemoryManagement;
using NUnit.Framework;

namespace CargoWise.Data
{
	public sealed class DbCommandExecuteTracker
	{
		static DbCommandExecuteTracker()
		{
			MemoryManager.Register("Db Command Execute Tracker", FlushCallback.OnAnyThread, delegate(FlushAction action) // diagnostic tool name
			{
				Instance.Clear();
				return FlushResult.Exhausted;
			});
		}

		internal DbCommandExecuteTracker()
		{
			Interlocked.Exchange(ref executeCount, 0L);
		}

		public void BeforeExecute_AddExecuteCountForTest(DbCommand command)
		{
			if (TestingState.IsRunningTests)
			{
				// don't count DataRegGetValueNOD: every ut execute this command 3~4 times
				// don't count DataRegGetValueFallback: some GUI ut execute this command 0~2 times
				if (command.CommandText == "DataRegGetValueNOD" || command.CommandText == "DataRegGetValueFallback")
				{
					return;
				}

				Interlocked.Increment(ref executeCount);
			}
		}

		public void Clear()
		{
			Interlocked.Exchange(ref executeCount, 0L);
		}

		public long ExecuteCount
		{
			get
			{
				return Interlocked.Read(ref executeCount);
			}
		}
		[SuppressMessage("CargoWiseOne", "CW1021")]
		public static readonly DbCommandExecuteTracker Instance = new ();
		long executeCount;
	}
}
#endif
