using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	[TestedType(typeof(MultiThreadUserContextManager))]
	sealed class MultiThreadUserContextManagerTest : UserContextManagerTest<MultiThreadUserContextManager>
	{
		public void TestPreventLeakOnBackgroundThread()
		{
			var branchPk = Env.CurrentBranchPK;
			var departmentPk = Env.CurrentDepartment.PK;

			using (var manager = new MultiThreadUserContextManager())
			{
				var factory = new BusinessObjectFactory();
				var staff = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
				factory.Save();
				var initialCount = CountUserContextFactories();

				var masterContext = new UserContext("Test", branchPk, departmentPk);
				manager.SetMasterUserContext(masterContext);
				AssertEquals(masterContext, manager.Context);
				AssertEquals(masterContext, manager.ContextForReadOnly);

				var afterSetMasterCount = CountUserContextFactories();
				AssertEquals(initialCount + 1, afterSetMasterCount);

				Task.WaitAll(CleanUpOnBackgroundThread(manager, branchPk, departmentPk));
				AssertEquals(afterSetMasterCount, CountUserContextFactories());

				ErrorReporter.Clear(); // Eat thread sentry errors.
			}
		}

		static Task CleanUpOnBackgroundThread(IUserContextManager env, Guid branchPk, Guid departmentPk)
		{
			return Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					env.SetCurrentThreadUserContext(new UserContext("Test", branchPk, departmentPk), isRevert: false);
					{
						// Do nothing.
					}
				}
				env.ClearCurrentThread();
				Env.Instance.CleanupUserContextOnCurrentThread();
			});
		}

		int CountUserContextFactories()
		{
			GC.Collect();
			GC.WaitForFullGCComplete();
			GC.WaitForPendingFinalizers();

			var stats = new PerformanceStatistic();
			stats.Load();
			var userContextFactories = stats.FactoryStatistics.Cast<BusinessObjectFactoryStatistic>()
				.Where(s => s.Name.Contains("UserContext", StringComparison.InvariantCultureIgnoreCase))
				.ToArray();
			return userContextFactories.Length;
		}

		[UseSnapshotProtection] // Performance statistics are written in a background thread, snapshot ensures rollback of these.
		[ExpectNoExceptions]
		public void TestSetupConcurrency()
		{
			var manager = new MultiThreadUserContextManager();
			try
			{
				var branchPk = Env.CurrentBranch.PK;
				var departmentPk = Env.CurrentDepartment.PK;

				const int MaxThreads = 40;
				Thread[] threads = new Thread[MaxThreads];
				try
				{
					for (int i = 0; i < MaxThreads; i++)
					{
						threads[i] = new Thread(() =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								DoSetup(manager, branchPk, departmentPk);
							}
						});
						threads[i].Name = i.ToString();
						threads[i].Start();
					}

					Thread.Sleep(5000);
				}
				finally
				{
					stop = true;
					foreach (Thread thread in threads)
					{
						if (thread != null)
						{
							thread.Join();
						}
					}
				}
			}
			finally
			{
				manager.Security.UnHookClientHookChanged();
				manager.Dispose();
			}
		}

		bool stop;

		void DoSetup(MultiThreadUserContextManager manager, Guid branchPk, Guid departmentPk)
		{
			while (!stop)
			{
				manager.SetMasterUserContext(new UserContext(User.WebUserName, branchPk, departmentPk));
			}
		}

		public void TestUserContextIsDuplicatedInOtherThread()
		{
			var manager = new MultiThreadUserContextManager();
			manager.SetMasterUserContext(Env.CurrentUserContext);
			Func<object, long> getFactoryId = (o) => ((IFactoryProvider)o).Factory._Instance;
			long companyId = 0, userId = 0, departmentId = 0, branchId = 0;

			Exception exThrown = null;
			var t = new Thread(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						companyId = getFactoryId(manager.Context.Company);
						branchId = getFactoryId(manager.Context.Branch);
						departmentId = getFactoryId(manager.Context.Department);
						userId = getFactoryId(manager.Context.User);
					}
				}
				catch (Exception ex)
				{
					exThrown = ex;
				}
			});
			t.Start();
			t.Join();

			if (exThrown != null)
			{
				Fail("Exception thrown on other thread: \r\n" + exThrown.ToString());
			}

			using (Db.DisposableActionForDbConnection())
			{
				AssertNotEquals("The user context should be refreshed for each branch", getFactoryId(manager.Context.Company), companyId);
				AssertNotEquals("The user context should be refreshed for each branch", getFactoryId(manager.Context.Branch), branchId);
				AssertNotEquals("The user context should be refreshed for each branch", getFactoryId(manager.Context.Department), departmentId);
				AssertNotEquals("The user context should be refreshed for each branch", getFactoryId(manager.Context.User), userId);
			}

			manager.Dispose();
		}

		public void TestSetCurrentThreadUserContext_ThreadSafe()
		{
			EventWaitHandle waitForThreadToSetNewContext = new EventWaitHandle(false, EventResetMode.AutoReset);
			EventWaitHandle waitForMainToAssert = new EventWaitHandle(false, EventResetMode.AutoReset);

			using (var manager = new MultiThreadUserContextManager())
			{
				var masterContext = Env.Instance.CurrentUserContext;
				manager.SetMasterUserContext(masterContext);

				Thread contextThread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var oldContext = manager.Context;
						var newContext = new UserContext("Test", Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
						manager.SetCurrentThreadUserContext(newContext, isRevert: false);
						AssertEquals(newContext, manager.ContextForReadOnly);
						AssertEquals(newContext, manager.Context);
						waitForThreadToSetNewContext.Set();
						waitForMainToAssert.WaitOne();
						manager.SetCurrentThreadUserContext(oldContext, isRevert: true);
					}
				});
				contextThread.Start();

				waitForThreadToSetNewContext.WaitOne();
				AssertEquals("Context was not affected", masterContext, manager.Context);
				AssertEquals("Context was not affected", masterContext, manager.ContextForReadOnly);
				waitForMainToAssert.Set();

				contextThread.Join();
				AssertEquals("Context was not affected", masterContext, manager.Context);
				AssertEquals("Context was not affected", masterContext, manager.ContextForReadOnly);
			}
		}

		public void TestSetCurrentThreadUserContext_ChecksUserContextIsValidForThisThread()
		{
			ErrorReporter.Clear();
			using (var manager = new MultiThreadUserContextManager())
			{
				var masterContext = Env.Instance.CurrentUserContext;
				manager.SetMasterUserContext(masterContext);

				var contextTask = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						manager.SetCurrentThreadUserContext(masterContext, isRevert: false);
					}
				});

				Assert("Task did not complete", contextTask.Wait(TimeSpan.FromSeconds(5)));
				AssertEquals("No error reported", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Wrong type of error reported", typeof(CrossThreadAccessException), ErrorReporter.LastExceptionReported.GetType());
				ErrorReporter.Clear();
			}
		}

		protected override MultiThreadUserContextManager GetNewUserContextManager()
		{
			return new MultiThreadUserContextManager();
		}
	}
}
