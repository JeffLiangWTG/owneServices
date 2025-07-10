using System;
using System.Threading;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PrimaryAppDomainWorkerWithMultiWorkItemsForTest : BackgroundAppDomainWorker, IDisposable
	{
		public PrimaryAppDomainWorkerWithMultiWorkItemsForTest(Action<IBackgroundAppDomainWorkItem> setQueueWorkItemCompleted)
		{
			if (Instance is PrimaryAppDomainWorkerWithMultiWorkItemsForTest)
			{
				throw new InvalidOperationException(nameof(PrimaryAppDomainWorkerWithMultiWorkItemsForTest) + " already registered");
			}

			originAppDomainWorker = BackgroundAppDomainWorker.Instance;
			BackgroundAppDomainWorker.Instance = this;
			this.setQueueWorkItemCompleted = setQueueWorkItemCompleted;

			CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		protected override IBackgroundAppDomainWorkItem QueueWorkItemCore(WorkItem workItem)
		{
			EnqueueWorkItem(workItem);
			ThreadPool.QueueUserWorkItem(delegate
			{
				setQueueWorkItemCompleted?.Invoke(workItem);
				workItem.AsyncResult.IsCompleted = true;
				DequeueWorkItem();
			});
			return workItem;
		}

		protected override AppDomainWorker CreateNewWorkerInAppDomain()
		{
			if (appDomainWorker == null)
			{
				appDomainWorker = new AppDomainWorker();
			}
			return appDomainWorker;
		}

		public void Dispose()
		{
			UnloadWorkerAppDomain();

			CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(this);
			Instance = originAppDomainWorker;
		}

		readonly BackgroundAppDomainWorker originAppDomainWorker;
		readonly Action<IBackgroundAppDomainWorkItem> setQueueWorkItemCompleted;
		AppDomainWorker appDomainWorker;
	}
}
