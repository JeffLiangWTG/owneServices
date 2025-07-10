using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.VisualBoards.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class SystemSchematicServiceTaskNonTransactionedTestCase<T> : NonTransactionedTestCase where T : SystemSchematicServiceTaskBase, new()
	{
		public void TestRunTask_WithMultipleSystems_ShouldNotReportDisposableActionForDbConnectionError()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "DUM");
			var system2 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system2.FS_Name = "No duplicate names";
			Factory.Save();

			AsyncStrategy.OverriddenStrategy_ForTest.Value = new AlwaysNewThreadAsyncStrategy_ForTest();
			var serviceTask = new T { ServiceLogger = new BufferManagementLogger() };

			AssertNoExceptionThrown("Running these service tasks multi-threaded should not throw exceptions. SAD!", serviceTask.RunTask);
			AssertEquals("Running these service tasks multi-threaded should not report errors. SAD!", 0, ErrorReporter.TotalErrorCount);
		}

		#region Implementation

		class AlwaysNewThreadAsyncStrategy_ForTest : IAsyncStrategy
		{
			public void ParallelForEach<T1>(IEnumerable<T1> source, Action<T1> body)
			{
				foreach (var thing in source)
				{
					var thread = new Thread(doWithThing =>
					{
						body((T1)doWithThing);
					});

					thread.Start(thing);
					thread.Join(TimeSpan.FromMinutes(1)); // won't be parallel BUT will run each thing in a new thread, which is what causes the error we're trying to prevent.
				}
			}

			#region Not Implemented

			public Task DoAsync(Action action, IThreadSentry threadSentry = null, string threadName = "")
			{
				throw new NotImplementedException();
			}

			public Thread DoAsyncAsThread(Action action, ApartmentState apartmentState = ApartmentState.MTA, IThreadSentry threadSentry = null, string threadName = "")
			{
				throw new NotImplementedException();
			}

			public Task<T1> GetAsync<T1>(Func<T1> func, IThreadSentry threadSentry = null, string threadName = "", CancellationTokenSource cancellationTokenSource = null)
			{
				throw new NotImplementedException();
			}

			public IAutoRefresher GetAutoRefresher(AutoRefreshAction updateAction, TimeSpan delay, Func<bool> shouldRefresh = null)
			{
				throw new NotImplementedException();
			}

			public IWindowsTimer GetTimer(IContainer container = null)
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		#endregion
	}
}
