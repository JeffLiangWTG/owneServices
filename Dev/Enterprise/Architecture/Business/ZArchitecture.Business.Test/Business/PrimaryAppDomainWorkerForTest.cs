using System;
using System.Threading;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PrimaryAppDomainWorkerForTest : BackgroundAppDomainWorker, IDisposable
	{
		public PrimaryAppDomainWorkerForTest()
			: this(500)
		{
		}

		public PrimaryAppDomainWorkerForTest(int millisecondsToSleep)
		{
			this.millisecondsToSleep = millisecondsToSleep;
			if (Instance is PrimaryAppDomainWorkerForTest)
			{
				throw new InvalidOperationException(nameof(PrimaryAppDomainWorkerForTest) + " already registered");
			}
			oldAppDomainWorker = BackgroundAppDomainWorker.Instance;
			BackgroundAppDomainWorker.Instance = this;

			CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(this);
		}
		readonly int millisecondsToSleep;

		protected override IBackgroundAppDomainWorkItem QueueWorkItemCore(WorkItem workItem)
		{
			try
			{
				workItem.AsyncResult.AsyncState = workItem.Method.DynamicInvoke(workItem.Args);
				TestWorkItemCrossDomainSerialization(workItem);
			}
			finally
			{
				UnloadWorkerAppDomain();
			}

			// simulate the work item being run asynchronously, even though has already completed at this stage
			ThreadPool.QueueUserWorkItem(delegate
			{ Thread.Sleep(millisecondsToSleep); workItem.AsyncResult.IsCompleted = true; });

			return workItem;
		}

		void TestWorkItemCrossDomainSerialization(WorkItem workItem)
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomainSerializationTester serializationTester = (AppDomainSerializationTester)WorkerAppDomain.CreateInstanceAndUnwrap(typeof(AppDomainSerializationTester).Assembly.FullName, typeof(AppDomainSerializationTester).FullName);
			serializationTester.TestObjectSerialization(workItem);
			serializationTester.TestObjectSerialization(workItem.AsyncResult.AsyncState);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		protected override AppDomainWorker CreateNewWorkerInAppDomain()
		{
			return new AppDomainWorker();
		}

		protected override string AppDomainFriendlyName
		{
			get { return GetType().Name; }
		}

		void IDisposable.Dispose()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(this);
			Instance = oldAppDomainWorker;
		}

		readonly BackgroundAppDomainWorker oldAppDomainWorker;
	}
}
