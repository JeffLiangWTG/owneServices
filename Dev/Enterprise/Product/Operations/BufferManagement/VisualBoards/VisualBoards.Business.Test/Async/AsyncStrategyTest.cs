using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	class AsyncStrategyTest : TestCase
	{
#if !WINZOR
		public void TestDoNotLeakUserContextFactory()
		{
			var startNumber = CountUserContextFactories();

			AccessUserContextInFiveDifferentTasks();

			AssertLessThanOrEqualTo(CountUserContextFactories(), startNumber);
		}

		void AccessUserContextInFiveDifferentTasks()
		{
			const int tasksCount = 5;
			var tasks = new List<Thread>(tasksCount);

			for (int i = 0; i < tasksCount; i++)
			{
				tasks.Add(AsyncStrategy.Default.DoAsyncAsThread(() => _ = Env.Instance.CurrentUser));
			}

			foreach (var thread in tasks)
			{
				thread.Join();
			}
		}

		int CountUserContextFactories()
		{
			GC.Collect();
			GC.WaitForFullGCComplete();
			GC.WaitForPendingFinalizers();

			var stats = new PerformanceStatistic();
			stats.Load();
			return stats.FactoryStatistics.Cast<BusinessObjectFactoryStatistic>().Count(s => s.Name.Contains("UserContext", StringComparison.InvariantCultureIgnoreCase));
		}
#endif
	}
}
