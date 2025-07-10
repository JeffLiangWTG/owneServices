using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository
{
	public class EntitySetDefinitionCacheTest : TestCaseWithFactory
	{
		public void TestFindEntityWithSameNameParallel_ShouldNotThrowException()
		{
			var cache = EntitySetDefinitionCache.GetInstance();
			var threads = new List<Thread>();
			var startEvent = new ManualResetEventSlim(false);
			var result = string.Empty;
			var definitionName = "Dummy";

			for (var i = 0; i < 20; i++)
			{
				var thread = new Thread(() =>
				{
					startEvent.Wait();
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							cache.Find(definitionName);
						}
					}
					catch (Exception e)
					{
						result += e.ToString() + System.Environment.NewLine;
					}
				});

				thread.Start();
				threads.Add(thread);
			}

			try
			{
				startEvent.Set();
				WaitAll(threads);
				AssertEquals(string.Empty, result);
			}
			catch (Exception ex)
			{
				HtmlAssert("Error: " + ex.Message + "Stack: " + ex.StackTrace, false);
			}
		}

		void WaitAll(IEnumerable<Thread> threads)
		{
			if (threads != null)
			{
				foreach (var thread in threads)
				{
					if (thread.IsAlive)
					{
						thread.Join(1000);
					}
				}
			}
		}
	}
}
