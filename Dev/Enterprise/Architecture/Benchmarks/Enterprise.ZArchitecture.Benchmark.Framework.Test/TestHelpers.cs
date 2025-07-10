using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.ZArchitecture.Benchmark.Framework.TestInterfaces;
using Moq;

namespace Enterprise.ZArchitecture.Benchmark.Framework.Test
{
	sealed class TestHelpers
	{
		public static BenchmarkConfig CreateTestConfig(ITestContext testContext = null, ILocalEnvironment localEnvironment = null)
			=> BenchmarkConfig.Default()
				.WithTestContext(testContext ?? new TestContext_ForTest(isRunningOnDat: false))
				.WithEnvironment(localEnvironment ?? CreateLocalEnvironmentMock().Object)
				.WithSaveResultsToTempFolder(false);

		public static BenchmarkConfig CreateFastTestConfig(ITestContext testContext = null, ILocalEnvironment localEnvironment = null)
			=> CreateTestConfig(testContext, localEnvironment)
				.WithTargetTime(TimeSpan.FromMilliseconds(10))
				.WithMinimumRuns(128);

		public static BenchmarkResult CreateTestBenchmarkResult(MethodInfo testMethod, BenchmarkConfig config = null)
			=> new BenchmarkResult(
				config: config ?? CreateTestConfig(),
				testMethod: testMethod,
				timings: CreateSequentialTimings(100),
				totalRuntime: TimeSpan.FromSeconds(1.23),
				runAt: new DateTimeOffset(2025, 04, 10, 17, 38, 21, TimeSpan.FromHours(10))
			);

		public static Mock<ILocalEnvironment> CreateLocalEnvironmentMock()
		{
			var env = new Mock<ILocalEnvironment>();
			env.Setup(x => x.GetLocalTime()).Returns(new DateTimeOffset(2025, 04, 14, 15, 29, 11, TimeSpan.FromHours(4)));
			env.Setup(x => x.Username).Returns("Bilbo.Baggins");
			env.Setup(x => x.MachineName).Returns("Fellowship-3175");
			env.Setup(x => x.GetCPUDetails()).Returns(("TomBombadil Enterprises, 999.99GHz", 2, 4, 8));
			env.Setup(x => x.DatabaseServerName).Returns("Rivendell-SQL-02");
			env.Setup(x => x.DatabaseFullVersionText).Returns("""
				PostgreSQL 17.4
				Feb 28 2025 18:24:49
				Copyright (C) PostgreSQL Global Development Group
				""");
			env.Setup(x => x.PowerProfileName).Returns("UltraSuperHot");
			env.Setup(x => x.RuntimeVersion).Returns("net2.99");
			return env;
		}

		public static IReadOnlyList<BenchmarkTiming> CreateSequentialTimings(int count)
			=> Enumerable.Range(1, count)
			.Select(x => new BenchmarkTiming(x, HighPrecisionTimeSpan.FromMilliseconds(x)))
			.ToArray();
	}

	class TestContext_ForTest(bool isRunningOnDat) : ITestContext
	{
		public bool IsRunningOnDAT { get; } = isRunningOnDat;

		public string PassMessage { get; private set; }
		public void PassWithMessage(string message)
		{
			PassMessage = message;
		}

		public string ReportTestMessage { get; private set; }
		public void ReportTestResults(string message)
		{
			ReportTestMessage = message;
		}
	}

	class BenchmarkTarget
	{
		public int SpinWaitCount { get; set; } = 500;

		public int CallCount { get; private set; }

		public object HowLong(object param)
		{
			++CallCount;
			System.Threading.Thread.SpinWait(SpinWaitCount);
			return "As long as a piece of string";
		}
	}
}
