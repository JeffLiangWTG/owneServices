using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Enterprise.ZArchitecture.Benchmark.Framework.TestInterfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Benchmark.Framework.Test
{
	[TestFixture]
	sealed class BenchmarkRunnerTests
	{
		[Test]
		public void Run_Simple()
		{
			var config = TestHelpers.CreateFastTestConfig();
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_Simple);

			var result = runner.Run(target.HowLong);

			Assert.That(result.RunAt.ToString("o", CultureInfo.InvariantCulture), Is.EqualTo("2025-04-14T15:29:11.0000000+04:00"));
			Assert.That(result.TotalRuntime, Is.GreaterThan(config.TargetTime));
			Assert.That(target.CallCount, Is.EqualTo(result.Timings.Count + config.WarmUpIterations));
		}

		[Test]
		public void Run_WithDifferentRunAtTime()
		{
			var env = new Mock<ILocalEnvironment>();
			env.Setup(x => x.GetLocalTime()).Returns(new DateTimeOffset(2028, 11, 04, 2, 41, 04, TimeSpan.FromHours(-9)));
			var config = TestHelpers.CreateFastTestConfig(localEnvironment: env.Object);
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_WithDifferentRunAtTime);

			var result = runner.Run(target.HowLong);

			Assert.That(result.RunAt.ToString("o", CultureInfo.InvariantCulture), Is.EqualTo("2028-11-04T02:41:04.0000000-09:00"));
			Assert.That(result.TotalRuntime, Is.GreaterThan(config.TargetTime));
			Assert.That(target.CallCount, Is.EqualTo(result.Timings.Count + config.WarmUpIterations));
		}

		[Test]
		public void Run_WhenRunningOnDAT()
		{
			var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "CargoWiseBenchmarkResults"));
			Assert.That(dir.Exists, Is.False, "Precondition: no benchmark summary folder.");

			var config = TestHelpers.CreateFastTestConfig(testContext: new TestContext_ForTest(isRunningOnDat: true))
							.WithSaveResultsToTempFolder(true);
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_WhenRunningOnDAT);

			var result = runner.Run(target.HowLong);

			Assert.That(result.TotalRuntime, Is.EqualTo(TimeSpan.Zero));
			Assert.That(result.Timings.Count, Is.EqualTo(0));
			Assert.That(target.CallCount, Is.EqualTo(1));

			dir.Refresh();
			Assert.That(dir.Exists, Is.False);
		}

		[Test]
		public void Run_WithSetupAndTearDown()
		{
			var setupCallCount = 0;
			var tearDownCallCount = 0;
			var config = TestHelpers.CreateFastTestConfig();
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_WithSetupAndTearDown);

			var result = runner.Run(target.HowLong, setup: SetupMethod, tearDown: TearDownMethod);

			Assert.That(result.TotalRuntime, Is.GreaterThan(config.TargetTime));
			var expectedCallCount = result.Timings.Count + config.WarmUpIterations;
			Assert.That(target.CallCount, Is.EqualTo(expectedCallCount));
			Assert.That(setupCallCount, Is.EqualTo(expectedCallCount));
			Assert.That(tearDownCallCount, Is.EqualTo(expectedCallCount));

			object SetupMethod()
			{
				++setupCallCount;
				return "TheResultOfSetup";
			}
			void TearDownMethod(object setupResult, object lastTestResult)
			{
				Assert.That(setupResult, Is.EqualTo("TheResultOfSetup"));
				Assert.That(lastTestResult, Is.EqualTo("As long as a piece of string"));
				++tearDownCallCount;
			}
		}

		[Test]
		public void Run_WithBenchmarkException_DuringTiming()
		{
			var iterationCount = 0;
			var config = TestHelpers.CreateFastTestConfig();
			var runner = new BenchmarkRunner(config, Run_WithBenchmarkException_DuringTiming);

			Assert.Throws<BenchmarkException>(() => runner.Run(BenchmarkMethod));
			Assert.That(iterationCount, Is.EqualTo(config.WarmUpIterations + 22));

			object BenchmarkMethod(object param)
			{
				++iterationCount;
				if (iterationCount == config.WarmUpIterations + 22)
				{
					return new BenchmarkException("This is an exception from a benchmark. We don't want to throw because that messes with the JIT.");
				}
				System.Threading.Thread.SpinWait(100);
				return null;
			}
		}

		[Test]
		public void Run_WithBenchmarkException_DuringWarmup()
		{
			var iterationCount = 0;
			var config = TestHelpers.CreateFastTestConfig();
			var runner = new BenchmarkRunner(config, Run_WithBenchmarkException_DuringWarmup);

			Assert.Throws<BenchmarkException>(() => runner.Run(BenchmarkMethod));
			Assert.That(iterationCount, Is.EqualTo(config.WarmUpIterations - 2));

			object BenchmarkMethod(object param)
			{
				++iterationCount;
				if (iterationCount == config.WarmUpIterations - 2)
				{
					return new BenchmarkException("This is an exception from a benchmark. We don't want to throw because that messes with the JIT.");
				}
				System.Threading.Thread.SpinWait(100);
				return null;
			}
		}

		[Test]
		public void Run_WithOtherException_DuringTiming()
		{
			var iterationCount = 0;
			var config = TestHelpers.CreateFastTestConfig();
			var runner = new BenchmarkRunner(config, Run_WithOtherException_DuringTiming);

			var result = runner.Run(BenchmarkMethod);

			Assert.That(result.TotalRuntime, Is.GreaterThan(TimeSpan.Zero));
			Assert.That(iterationCount, Is.EqualTo(result.Timings.Count + config.WarmUpIterations));

			object BenchmarkMethod(object param)
			{
				++iterationCount;
				if (iterationCount == config.WarmUpIterations + 22)
				{
					return new ApplicationException("This is an exception from a benchmark. But we don't fail because it isn't the magic BenchmarkException.");
				}
				System.Threading.Thread.SpinWait(100);
				return null;
			}
		}

		[Test]
		public void Run_WithOtherException_DuringWarmup()
		{
			var iterationCount = 0;
			var config = TestHelpers.CreateFastTestConfig();
			var runner = new BenchmarkRunner(config, Run_WithOtherException_DuringWarmup);

			var result = runner.Run(BenchmarkMethod);

			Assert.That(result.TotalRuntime, Is.GreaterThan(TimeSpan.Zero));
			Assert.That(iterationCount, Is.EqualTo(result.Timings.Count + config.WarmUpIterations));

			object BenchmarkMethod(object param)
			{
				++iterationCount;
				if (iterationCount == config.WarmUpIterations - 2)
				{
					return new ApplicationException("This is an exception from a benchmark. But we don't fail because it isn't the magic BenchmarkException.");
				}
				System.Threading.Thread.SpinWait(100);
				return null;
			}
		}

		[Test]
		public void Run_WithConfig_TargetTime()
		{
			var config = TestHelpers.CreateFastTestConfig()
							.WithTargetTime(TimeSpan.FromMilliseconds(500))
							.WithMinimumRuns(1024);
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_WithConfig_TargetTime);

			var result = runner.Run(target.HowLong);

			Assert.That(result.TotalRuntime, Is.GreaterThan(TimeSpan.FromMilliseconds(500)));
			Assert.That(target.CallCount, Is.EqualTo(result.Timings.Count + config.WarmUpIterations));
		}

		[Test]
		public void Run_WithConfig_MinimumRuns()
		{
			var config = TestHelpers.CreateFastTestConfig()
							.WithTargetTime(TimeSpan.FromTicks(1))
							.WithMinimumRuns(16_384);
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_WithConfig_MinimumRuns);

			var result = runner.Run(target.HowLong);

			Assert.That(result.TotalRuntime, Is.GreaterThan(TimeSpan.FromTicks(1)));
			Assert.That(target.CallCount, Is.EqualTo(16_384 + config.WarmUpIterations));
		}

		[Test]
		public void Run_WithConfig_ExecutionsPerIteration()
		{
			var config = TestHelpers.CreateFastTestConfig()
							.WithTargetTime(TimeSpan.FromTicks(1))
							.WithExecutionsPerIteration(32)
							.WithMinimumRuns(2048);
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_WithConfig_ExecutionsPerIteration);

			var result = runner.Run(target.HowLong);

			Assert.That(result.TotalRuntime, Is.GreaterThan(config.TargetTime));
			Assert.That(target.CallCount, Is.EqualTo((32 * 2048) + config.WarmUpIterations));
		}

		[Test]
		public void Run_WithConfig_WarmupIterations()
		{
			var config = TestHelpers.CreateFastTestConfig()
							.WithTargetTime(TimeSpan.FromTicks(1))
							.WithMinimumRuns(1024)
							.WithWarmUpIterations(4096);
			var target = new BenchmarkTarget();
			var runner = new BenchmarkRunner(config, Run_WithConfig_WarmupIterations);

			var result = runner.Run(target.HowLong);

			Assert.That(result.TotalRuntime, Is.GreaterThan(TimeSpan.FromTicks(1)));
			Assert.That(target.CallCount, Is.EqualTo(1024 + 4096));
		}

		[Test]
		public void SaveResultsToTempFolder()
		{
			var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "CargoWiseBenchmarkResults"));
			Assert.That(dir.Exists, Is.False, "Precondition: no benchmark summary folder.");

			var config = TestHelpers.CreateFastTestConfig()
							.WithSaveResultsToTempFolder(true);
			var target = new BenchmarkTarget() { SpinWaitCount = 10000 };
			var runner = new BenchmarkRunner(config, SaveResultsToTempFolder);

			var result = runner.Run(target.HowLong);
			var actualSummary = result.GetSummary();

			dir.Refresh();
			Assert.That(dir.Exists, Is.True);

			var summaryFile = new FileInfo(Path.Combine(dir.FullName, "BenchmarkRunnerTests.SaveResultsToTempFolder.net2.99.txt"));
			Assert.That(summaryFile.Exists, Is.True);
			var summaryContent = File.ReadAllText(summaryFile.FullName);
			Assert.That(summaryContent, Is.EqualTo(actualSummary), "Summary returned should be the same as written to temp file.");

			var aggregateFile = new FileInfo(Path.Combine(dir.FullName, "!TestRuns.txt"));
			Assert.That(aggregateFile.Exists, Is.True);
			var aggregateLines = File.ReadAllLines(aggregateFile.FullName);
			Assert.That(aggregateLines.Length, Is.EqualTo(2));
			Assert.That(aggregateLines[0], Is.EqualTo("Class\tTest\tDate Run\tDateTime Run\tWho Ran\tMean (ms)\tMedian (ms)\tOps/sec\tRuntime"));
			var aggregateFields = aggregateLines[1].Split('\t');
			Assert.Multiple(() =>
			{
				Assert.That(aggregateFields[0], Is.EqualTo("BenchmarkRunnerTests"), "Class");
				Assert.That(aggregateFields[1], Is.EqualTo("SaveResultsToTempFolder"), "Test");
				Assert.That(aggregateFields[2], Is.EqualTo("2025-04-14"), "Date Run");
				Assert.That(aggregateFields[3], Is.EqualTo("2025-04-14 15:29:11 +04:00"), "DateTime Run");
				Assert.That(aggregateFields[4], Is.EqualTo("Bilbo.Baggins"), "Who Ran");

				Assert.That(double.TryParse(aggregateFields[5], NumberStyles.Number, CultureInfo.InvariantCulture, out var mean), Is.EqualTo(true), "Mean (ms) - Is number");
				Assert.That(mean, Is.GreaterThan(0.0), "Mean (ms) - is greater than zero");

				Assert.That(double.TryParse(aggregateFields[6], NumberStyles.Number, CultureInfo.InvariantCulture, out var median), Is.EqualTo(true), "Median (ms) - Is number");
				Assert.That(median, Is.GreaterThan(0.0), "Median (ms) - is greater than zero");

				Assert.That(double.TryParse(aggregateFields[7], NumberStyles.Number, CultureInfo.InvariantCulture, out var ops), Is.EqualTo(true), "Ops/sec - Is number");
				Assert.That(ops, Is.GreaterThan(0.0), "Ops/sec - is greater than zero");

				Assert.That(aggregateFields[8], Is.EqualTo("net2.99"), "Runtime");
			});

			Assert.That(actualSummary, Does.Contain($"Saved To:  {summaryFile.FullName}"));
			Assert.That(actualSummary, Does.Contain($"Aggregate: {aggregateFile.FullName}"));
		}

		[Test]
		public void SaveSummaryToTempFolder_ManyTimes()
		{
			var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "CargoWiseBenchmarkResults"));
			Assert.That(dir.Exists, Is.False, "Precondition: no benchmark summary folder.");

			var net80EnvMock = TestHelpers.CreateLocalEnvironmentMock();
			net80EnvMock.Setup(x => x.RuntimeVersion).Returns("net8.0");
			var net48EnvMock = TestHelpers.CreateLocalEnvironmentMock();
			net48EnvMock.Setup(x => x.RuntimeVersion).Returns("net48");
			var net80Config = TestHelpers.CreateFastTestConfig()
								.WithSaveResultsToTempFolder(true)
								.WithEnvironment(net80EnvMock.Object);
			var net48Config = TestHelpers.CreateFastTestConfig()
								.WithSaveResultsToTempFolder(true)
								.WithEnvironment(net48EnvMock.Object);
			IReadOnlyCollection<(BenchmarkRunner runner, Func<object, object> benchmarkMethod)> benchmarks =
			[
				(new BenchmarkRunner(net80Config, SaveSummaryToTempFolder_ManyTimes), new BenchmarkTarget() { SpinWaitCount = 100   }.HowLong),
				(new BenchmarkRunner(net48Config, SaveSummaryToTempFolder_ManyTimes), new BenchmarkTarget() { SpinWaitCount = 100   }.HowLong),
				(new BenchmarkRunner(net48Config, Run_WithSetupAndTearDown),          new BenchmarkTarget() { SpinWaitCount = 1000  }.HowLong),
				(new BenchmarkRunner(net48Config, Run_Simple),                        new BenchmarkTarget() { SpinWaitCount = 10000 }.HowLong),
			];

			// Simulate running 4 performance tests end-to-end.
			foreach (var (runner, benchmarkMethod) in benchmarks)
			{
				var summary = runner.Run(benchmarkMethod);
				Assert.That(summary, Is.Not.Null.Or.Empty);
			}

			dir.Refresh();
			Assert.That(dir.Exists, Is.True);

			Assert.That(File.Exists(Path.Combine(dir.FullName, "BenchmarkRunnerTests.SaveSummaryToTempFolder_ManyTimes.net8.0.txt")), Is.True);
			Assert.That(File.Exists(Path.Combine(dir.FullName, "BenchmarkRunnerTests.SaveSummaryToTempFolder_ManyTimes.net48.txt")), Is.True);
			Assert.That(File.Exists(Path.Combine(dir.FullName, "BenchmarkRunnerTests.Run_WithSetupAndTearDown.net48.txt")), Is.True);
			Assert.That(File.Exists(Path.Combine(dir.FullName, "BenchmarkRunnerTests.Run_Simple.net48.txt")), Is.True);

			var aggregateFile = new FileInfo(Path.Combine(dir.FullName, "!TestRuns.txt"));
			Assert.That(aggregateFile.Exists, Is.True);
			var aggregateLines = File.ReadAllLines(aggregateFile.FullName);
			Assert.That(aggregateLines.Length, Is.EqualTo(5));
			Assert.That(aggregateLines[0], Is.EqualTo("Class\tTest\tDate Run\tDateTime Run\tWho Ran\tMean (ms)\tMedian (ms)\tOps/sec\tRuntime"));
			var aggregateData = aggregateLines
									.Skip(1)
									.Select(l => l.Split('\t'))
									.Select(fields => new { Class = fields[0], Test = fields[1], Runtime = fields[8] })
									.ToArray();
			Assert.That(aggregateData.Select(x => x.Class), Is.EquivalentTo(Enumerable.Repeat("BenchmarkRunnerTests", aggregateData.Length)));
			Assert.That(aggregateData.Select(x => x.Test), Is.EquivalentTo(new[] { "SaveSummaryToTempFolder_ManyTimes", "SaveSummaryToTempFolder_ManyTimes", "Run_WithSetupAndTearDown", "Run_Simple" }));
			Assert.That(aggregateData.Select(x => x.Runtime), Is.EquivalentTo(new[] { "net8.0", "net48", "net48", "net48" }));
		}

		[SetUp]
		public void SetUp()
		{
			var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "CargoWiseBenchmarkResults"));
			if (dir.Exists)
			{
				dir.Delete(true);
			}
		}

		[TearDown]
		public void TearDown()
		{
			var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "CargoWiseBenchmarkResults"));
			if (dir.Exists)
			{
				dir.Delete(true);
			}
		}
	}
}
