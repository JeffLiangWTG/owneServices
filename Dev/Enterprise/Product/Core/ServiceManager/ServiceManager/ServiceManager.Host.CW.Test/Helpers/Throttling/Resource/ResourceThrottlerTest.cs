using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW.Resource;

namespace Enterprise.ServiceManager.Host.Testing
{
	class ResourceThrottlerTest : TransactionedTestCase
	{
		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestIdleState()
		{
			using (var throttler = new ResourceThrottler(Mock.Of<IHostRegistrySettings>()))
			{
				AssertEquals(false, throttler.WaitForResource().TimedOut);
				var result = throttler.WaitForResource();
				AssertEquals(ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, result.WaitType);
				Assert(result.TimeForWait.Milliseconds < 500);
			}
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestHighCpuNoTimeout()
		{
			try
			{
				using (var resourceThrottler = new ResourceThrottler(CreateHostRegistry(20)))
				{
					CreateAsyncCPULoad(TimeSpan.FromSeconds(10));
					var result = resourceThrottler.WaitForResource();
					AssertEquals(ResourceThrottlerResult.ResourceThrottlerResults.CpuContention, result.WaitType);
					Assert(result.ToString(), !result.TimedOut);
				}
			}
			finally
			{
				killAsyncWait = true;
			}
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestHighCpuTimeout()
		{
			try
			{
				using (var resourceThrottler = new ResourceThrottler(CreateHostRegistry(5)))
				{
					CreateAsyncCPULoad(TimeSpan.FromSeconds(20));
					var result = resourceThrottler.WaitForResource();
					AssertEquals(ResourceThrottlerResult.ResourceThrottlerResults.CpuContention, result.WaitType);
					AssertEquals(true, result.TimedOut);
					AssertGreaterThanOrEqualTo("System should wait for CPU", result.TimeForWait.TotalSeconds, 5);
				}
			}
			finally
			{
				killAsyncWait = true;
			}
		}

		[ExpectNoExceptions]
		public void TestPageFile()
		{
			using (var resourceThrottler = new ResourceThrottler(CreateHostRegistry(10)))
			using (ZSystemInformation.SetInstanceForTesting(new DummySysInfoWithLowPageFile()))
			{
				Thread.Sleep(5000);
				var result = resourceThrottler.WaitForResource();
				AssertEquals(ResourceThrottlerResult.ResourceThrottlerResults.PageFileContention, result.WaitType);
				AssertEquals(true, result.TimedOut);
			}
		}

		volatile bool killAsyncWait;

		void CreateAsyncCPULoad(TimeSpan time)
		{
			Task.Factory.StartNew(() =>
			{
				var sw = Stopwatch.StartNew();

				Parallel.For(long.MinValue, long.MaxValue, (i, loopState) =>
				{
					while (sw.Elapsed < time && !killAsyncWait)
					{
					}
					loopState.Break();
				});
			});
			Thread.Sleep(3000);
		}

		IHostRegistrySettings CreateHostRegistry(int maxWaitInSecnds)
		{
			var hostRegistryMock = new Mock<IHostRegistrySettings>();

			hostRegistryMock.Setup(o => o.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks).Returns(80);
			hostRegistryMock.Setup(o => o.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks).Returns(1);
			hostRegistryMock.Setup(o => o.ServiceTaskMaxWaitForResourceAvailability).Returns(TimeSpan.FromSeconds(1));
			hostRegistryMock.Setup(o => o.ServiceTaskProcessingMaximumBatchSize).Returns(20);
			hostRegistryMock.Setup(o => o.ServiceTaskMaxWaitForResourceAvailability).Returns(TimeSpan.FromSeconds(maxWaitInSecnds));

			return hostRegistryMock.Object;
		}
	}
}
