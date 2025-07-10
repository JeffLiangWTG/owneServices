using System;
using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ObjectFactoryCleanupListenerTest : TestCase
	{
		public void TestSingletonsDisposed()
		{
			var singleton = ObjectFactory.Get("IPerformanceStatisticsPersister");
			AssertNotNull("IPerformanceStatisticsPersister object defintion expected to exist", singleton);
			Assert("IPerformanceStatisticsPersister expected to be a singleton", ReferenceEquals(singleton, ObjectFactory.Get("IPerformanceStatisticsPersister")));
			new ObjectFactoryCleanupListener().StartTest(null, DateTime.Now);
			Assert("New instance expected after cleanup", !ReferenceEquals(singleton, ObjectFactory.Get("IPerformanceStatisticsPersister")));
			singleton = ObjectFactory.Get("IPerformanceStatisticsPersister");
			Assert("IPerformanceStatisticsPersister expected to be a singleton", ReferenceEquals(singleton, ObjectFactory.Get("IPerformanceStatisticsPersister")));
			new ObjectFactoryCleanupListener().EndTest(null, DateTime.Now);
			Assert("New instance expected after cleanup", !ReferenceEquals(singleton, ObjectFactory.Get("IPerformanceStatisticsPersister")));
		}
	}
}
