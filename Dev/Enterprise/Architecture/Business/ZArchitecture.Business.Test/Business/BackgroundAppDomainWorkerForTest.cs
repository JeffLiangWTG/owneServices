using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class BackgroundAppDomainWorkerForTest : BackgroundAppDomainWorker
	{
		public static IDisposable RunInPrimaryAppDomainForTest(int millisecondsToSleep)
		{
			return new PrimaryAppDomainWorkerForTest(millisecondsToSleep);
		}

		public static IDisposable RunInPrimaryAppDomainForTest()
		{
			return new PrimaryAppDomainWorkerForTest();
		}

		public static IDisposable RunInPrimaryAppDomainWorkerWithMultiWorkItemsForTest(Action<IBackgroundAppDomainWorkItem> setQueueWorkItemCompleted)
		{
			return new PrimaryAppDomainWorkerWithMultiWorkItemsForTest(setQueueWorkItemCompleted);
		}

		public static void ClearRecentlyCompletedWorkItem()
		{
			InstanceForTesting.recentlyCompletedWorkItem = null;
		}

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static readonly BackgroundAppDomainWorkerForTest InstanceForTesting = new BackgroundAppDomainWorkerForTest();
	}
}
