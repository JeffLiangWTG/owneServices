using System;
using System.Runtime.CompilerServices;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BackgroundAppDomainWorkerTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestWhenAppDomainUnloaded()
		{
#pragma warning disable CW1157, SYSLIB0024 // WI00669071 - Do not use System.AppDomain.
			void HandleWorkerAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
			{
			}

			//if the current appDomain is disposed, what things will throw AppDomainUnloadedException?
			//Turns out the answer is 'basically anything'
			var appDomain = AppDomain.CreateDomain("a");
			appDomain.UnhandledException += HandleWorkerAppDomainUnhandledException;
			AppDomain.Unload(appDomain);
			AssertExceptionThrown(typeof(AppDomainUnloadedException), () => { appDomain.UnhandledException -= HandleWorkerAppDomainUnhandledException; });
			AssertExceptionThrown(typeof(AppDomainUnloadedException), () => { AssertEquals("a", appDomain.FriendlyName); });
			AssertExceptionThrown(typeof(AppDomainUnloadedException), () => { AssertEquals(false, appDomain.IsDefaultAppDomain()); });
#pragma warning restore CW1157, SYSLIB0024 // WI00669071 - Do not use System.AppDomain.
		}

#if NETFRAMEWORK
		public void TestWorkerRemoteObjectHasNoLifetimeService_SoWorkerDoesntCollectAfter5Minutes()
		{
			using (TestBackgroundAppDomainWorker worker = new TestBackgroundAppDomainWorker())
			{
				MarshalByRefObject remoteWorker = worker.CreateNewWorkerInAppDomain();
				AssertEquals("No lifetime manager can be present on worker otherwise object will be collected in other app domain", null, remoteWorker.InitializeLifetimeService());
			}
		}
#endif

		public static void WaitForCompletion(IAsyncResult asyncResult)
		{
			for (int i = 0; i < 5000 && !asyncResult.IsCompleted; i++)
			{
				Thread.Sleep(10);
			}
		}

		#region Error event

		public void TestErrorEvent()
		{
			bool errorRaised = false;
			AppDomainWorker.UnhandledError += delegate
			{ errorRaised = true; };
			WaitForCompletion(AppDomainWorker.QueueWorkItem("Testing Error Event", delegate
			{
				throw new InvalidOperationException();
			}));
			AssertEquals("Error raised", true, errorRaised);
		}

		#endregion

		#region QueueWorkItem

		public void TestQueueWorkItem()
		{
			Globals.IsUserInteractive = false;
			try
			{
				IAsyncResult asyncResult = AppDomainWorker.QueueWorkItem("TestWorkItem", new GetAppDomainInfoDelegate(delegate
				{
					return new AppDomainInfo(
						AppDomain.CurrentDomain.FriendlyName,
						Thread.CurrentThread.GetApartmentState(),
						EnvProxy.Instance.CurrentUser.LoginName,
						EnvProxy.Instance.CurrentBranch.Code,
						EnvProxy.Instance.CurrentDepartment.Code,
						true,
						Globals.IsUserInteractive);
				}));
				WaitForCompletion(asyncResult);

				AppDomainInfo info = (AppDomainInfo)asyncResult.AsyncState;
				AssertEquals("AppDomain.FriendlyName", "TestBackgroundAppDomainWorker", info.AppDomainName);
				AssertEquals("Thread.ApartmentState", ApartmentState.STA, info.ThreadApartmentState);
				AssertEquals("EnvProxy.Instance.CurrentUser.LoginName", EnvProxy.Instance.CurrentUser.LoginName, info.UserLogin);
				AssertEquals("EnvProxy.Instance.CurrentBranch.Code", EnvProxy.Instance.CurrentBranch.Code, info.BranchCode);
				AssertEquals("EnvProxy.Instance.CurrentDepartment.Code", EnvProxy.Instance.CurrentDepartment.Code, info.DepartmentCode);
				AssertEquals("Initialiser.InitialiseBatchProcessor() called", true, info.InitialiseBatchProcessorCalled);
				AssertEquals("IsUserInteractive", false, info.IsUserInteractive);
			}
			finally
			{
				Globals.IsUserInteractive = true;
			}
		}

		public void TestQueueWorkItem_NewAppDomainNotCreatedForMultipleWorkItems()
		{
			IAsyncResult asyncResult1 = AppDomainWorker.QueueWorkItem("TestWorkItem1", delegate
			{ });
			IAsyncResult asyncResult2 = AppDomainWorker.QueueWorkItem("TestWorkItem2", delegate
			{ });

			WaitForCompletion(asyncResult1);
			WaitForCompletion(asyncResult2);
			for (int i = 0; i < 1000 && !AppDomainWorker.AppDomainUnloaded; i++)
			{
				Thread.Sleep(10);
			}

			AssertEquals("Only 1 app domain should have been created", 1, AppDomainWorker.AppDomainCreateCount);
			AssertEquals("AppDomain should be unloaded after both jobs complete", true, AppDomainWorker.AppDomainUnloaded);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestQueueWorkItem_WhenPassingDelegateOnMarshalByRefObject()
		{
#if NETFRAMEWORK
			CrossAppDomainDelegate delegateToMarshalByRefObject = new System.ComponentModel.Component().Dispose;
#else
			Action delegateToMarshalByRefObject = new System.ComponentModel.Component().Dispose;
#endif
			AppDomainWorker.QueueWorkItem("TestWorkItem", delegateToMarshalByRefObject);
		}

		public void TestQueueWorkItem_QueuesInPrimaryAppDomainWhenAlreadyInWorkerAppDomain()
		{
			// this line runs in the primary app domain
			IAsyncResult asyncResult = AppDomainWorker.QueueWorkItem("TestWorkItem", new GetNestedAsyncResultDelegate(delegate
			{
				// this line runs in a secondary app domain
				IAsyncResult nestedResult = BackgroundAppDomainWorker.QueueWorkItem("NestedWorkItem", new GetAppDomainIdDelegate(delegate
				{
					return AppDomain.CurrentDomain.Id;
				}));
				AssertEquals("Nested work item has been queued", false, nestedResult.IsCompleted);
				return nestedResult;
			}));
			WaitForCompletion(asyncResult);

			IAsyncResult nestedAsyncResult = (IAsyncResult)asyncResult.AsyncState;
			WaitForCompletion(nestedAsyncResult);
			AssertEquals("Nested work item has completed", true, nestedAsyncResult.IsCompleted);

			int workerAppDomainId = AppDomainWorker.AppDomainIdLastWorkItemRanIn;
			int nestedWorkerAppDomainId = (int)nestedAsyncResult.AsyncState;
			AssertEquals("Nested work item should run within the same worker app domain and not create a new one", workerAppDomainId, nestedWorkerAppDomainId);
		}
		delegate int GetAppDomainIdDelegate();
		delegate IAsyncResult GetNestedAsyncResultDelegate();

		public void TestQueueWorkItem_ChildExceptionsDoNotBubbleToParent()
		{
			IAsyncResult asyncResult = AppDomainWorker.QueueWorkItem("TestWorkItem", delegate
			{
				throw new InvalidOperationException("Test Exception in Child AppDomain");
			});

			WaitForCompletion(asyncResult);
			AssertNull("No exception should be raised in parent appdomain", ErrorReporter.LastExceptionReported);
		}

		public void TestQueueWorkItem_WorkItemExceptionDoesntPreventOtherWorkItems()
		{
			IAsyncResult asyncResult = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem", delegate
			{
				throw new InvalidOperationException("Test Exception in Child AppDomain");
			});
			IAsyncResult asyncResult2 = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem", delegate
			{ });

			WaitForCompletion(asyncResult);
			WaitForCompletion(asyncResult2);
			AssertEquals("An exception shouldn't starve other AppDomain work items", true, asyncResult2.IsCompleted);
		}

		[ExpectNoExceptions]
		public void TestQueueWorkItem_IgnoreThreadAbortedExceptionForWhenBatchProcessorCloses()
		{
			bool errorRaised = false;
			BackgroundAppDomainWorker.Error += delegate
			{ errorRaised = true; };
			IAsyncResult asyncResult = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem", delegate
			{
				throw NewThreadAbortException();
			});
			WaitForCompletion(asyncResult);
			AssertEquals(false, errorRaised);
		}

		static ThreadAbortException NewThreadAbortException()
		{
			ThreadAbortException result = null;
			Thread t = new Thread(new ThreadStart(delegate
			{
				try
				{
					Thread.Sleep(1000);
				}
				catch (ThreadAbortException ex)
				{
					result = ex;
				}
			}));
			t.Start();
			Thread.Sleep(100);
#if NETFRAMEWORK
			t.Abort();
#endif
			t.Join();
			return result;
		}

		public void TestCreateNewWorkerInAppDomainWhenDomainUnloaded()
		{
#pragma warning disable CW1157, SYSLIB0024 // WI00669071 - Do not use System.AppDomain.
			using (var worker = new TestBackgroundAppDomainWorker())
			{
				var domain = worker.WorkerAppDomainForTest;

				AssertEquals("TestBackgroundAppDomainWorker", domain.FriendlyName);
				AppDomain.Unload(domain);

				AssertNoExceptionThrown(() =>
				{
					worker.CreateNewWorkerInAppDomain();
				});
			}
#pragma warning restore CW1157, SYSLIB0024 // WI00669071 - Do not use System.AppDomain.
		}

#endregion

		#region WorkItemsInProgress

		public void TestWorkItemsInProgress()
		{
			IAsyncResult asyncResult1 = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem1", new ThreadStart(delegate
			{ Thread.Sleep(250); }));
			IAsyncResult asyncResult2 = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem2", new ThreadStart(delegate
			{ Thread.Sleep(500); }));

			AssertEquals("2 work items queued", 2, BackgroundAppDomainWorker.WorkItemsInProgress.Length);

			IBackgroundAppDomainWorkItem workItem1 = BackgroundAppDomainWorker.WorkItemsInProgress[0];
			IBackgroundAppDomainWorkItem workItem2 = BackgroundAppDomainWorker.WorkItemsInProgress[1];

			AssertEquals("TestWorkItem1", workItem1.Description);
			AssertEquals("TestWorkItem2", workItem2.Description);
			AssertEquals(BackgroundAppDomainWorkItemStatus.Queued, workItem1.Status);
			AssertEquals(BackgroundAppDomainWorkItemStatus.Queued, workItem2.Status);

			WaitForCompletion(asyncResult1);
			AssertEquals("1 work item still in progress", 1, BackgroundAppDomainWorker.WorkItemsInProgress.Length);
			AssertEquals("TestWorkItem2", BackgroundAppDomainWorker.WorkItemsInProgress[0].Description);

			AssertEquals(BackgroundAppDomainWorkItemStatus.Running, workItem1.Status);

			WaitForCompletion(asyncResult2);
			AssertEquals("After all jobs have completed", 0, BackgroundAppDomainWorker.WorkItemsInProgress.Length);
		}

		#endregion

		#region RunInPrimaryAppDomainForTest

		public void TestRunInPrimaryAppDomainForTest()
		{
			using (TestBackgroundAppDomainWorker.RunInPrimaryAppDomainForTest())
			{
				IAsyncResult asyncResult = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem", new GetAppDomainIdDelegate(delegate
				{ return AppDomain.CurrentDomain.Id; }));
				AssertEquals("For testing, we should simulate the invoke isn't happening synchronously", false, asyncResult.IsCompleted);
				AssertEquals("Should run in the invoking AppDomain for testing so that TransactionedTestCase can roll back effectively", AppDomain.CurrentDomain.Id, asyncResult.AsyncState);

				WaitForCompletion(asyncResult);
				AssertEquals("IsCompleted should be true eventually", true, asyncResult.IsCompleted);
			}
		}

		public void TestRunInPrimaryAppDomainForTest_DoesntCauseMemoryLeak()
		{
			using (TestBackgroundAppDomainWorker.RunInPrimaryAppDomainForTest())
			{
				WeakReference workerRef = QueueAndRunTestWorkItem();
				GC.Collect();
				AssertEquals("Worker method instance should be elligle for garbage collection", false, workerRef.IsAlive);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference QueueAndRunTestWorkItem()
		{
			Worker worker = new Worker();
			WeakReference workerRef = new WeakReference(worker);
			IAsyncResult asyncResult = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem", worker.DoWork);
			WaitForCompletion(asyncResult);
			return workerRef;
		}

		[Serializable]
		class Worker
		{
			public void DoWork()
			{
			}
		}

		#endregion

		#region Test Classes

		class TestBackgroundAppDomainWorker : BackgroundAppDomainWorkerForTest, IDisposable
		{
			public TestBackgroundAppDomainWorker()
			{
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

#if NETFRAMEWORK
			public new IAsyncResult QueueWorkItem(string workItemDescription, CrossAppDomainDelegate method)
			{
				return base.QueueWorkItemCore(new WorkItem(workItemDescription, method, Array.Empty<object>())).AsyncResult;
			}
#else
			public new IAsyncResult QueueWorkItem(string workItemDescription, Action method)
			{
				return base.QueueWorkItemCore(new WorkItem(workItemDescription, method, Array.Empty<object>())).AsyncResult;
			}
#endif

			public new IAsyncResult QueueWorkItem(string workItemDescription, Delegate method, params object[] args)
			{
				return base.QueueWorkItemCore(new WorkItem(workItemDescription, method, args)).AsyncResult;
			}

			public new IBackgroundAppDomainWorkItem[] WorkItemsInProgressCore
			{
				get { return base.WorkItemsInProgressCore; }
			}

			protected override string AppDomainFriendlyName
			{
				get { return GetType().Name; }
			}

			public int AppDomainCreateCount;
			public int AppDomainIdLastWorkItemRanIn;

			public new MarshalByRefObject CreateNewWorkerInAppDomain()
			{
				return base.CreateNewWorkerInAppDomain();
			}

			protected override void OnAppDomainCreated(AppDomain appDomain)
			{
				base.OnAppDomainCreated(appDomain);
				AppDomainIdLastWorkItemRanIn = appDomain.Id;
				AppDomainCreateCount++;
			}

			public bool AppDomainUnloaded;

			protected override void OnAppDomainUnloaded()
			{
				base.OnAppDomainUnloaded();
				AppDomainUnloaded = true;
			}

			public AppDomain WorkerAppDomainForTest => base.WorkerAppDomain;

			#region IDisposable Members

			public void Dispose()
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				var retries = 500;
				while (WorkItemsInProgressCore.Length > 0 && retries > 0)
				{
					Thread.Sleep(10);
					retries--;
				}
			}

			#endregion
		}

		[Serializable]
		struct AppDomainInfo
		{
			public AppDomainInfo(string appDomainName, ApartmentState threadApartmentState, string userLogin, string branchCode, string departmentCode, bool initialiseBatchProcessorCalled, bool isUserInteractive)
			{
				this.AppDomainName = appDomainName;
				this.ThreadApartmentState = threadApartmentState;
				this.UserLogin = userLogin;
				this.BranchCode = branchCode;
				this.DepartmentCode = departmentCode;
				this.InitialiseBatchProcessorCalled = initialiseBatchProcessorCalled;
				this.IsUserInteractive = isUserInteractive;
			}

			public readonly string AppDomainName;
			public readonly ApartmentState ThreadApartmentState;
			public readonly string UserLogin;
			public readonly string BranchCode;
			public readonly string DepartmentCode;
			public readonly bool InitialiseBatchProcessorCalled;
			public readonly bool IsUserInteractive;
		}
		delegate AppDomainInfo GetAppDomainInfoDelegate();

#endregion

		#region Implementation

		TestBackgroundAppDomainWorker AppDomainWorker
		{
			get
			{
				if (appDomainWorker == null)
				{
					appDomainWorker = new TestBackgroundAppDomainWorker();
				}
				return appDomainWorker;
			}
		}
		TestBackgroundAppDomainWorker appDomainWorker;

		protected override void TearDown()
		{
			base.TearDown();
			if (appDomainWorker != null)
			{
				appDomainWorker.Dispose();
			}
		}

		#endregion
	}
}
