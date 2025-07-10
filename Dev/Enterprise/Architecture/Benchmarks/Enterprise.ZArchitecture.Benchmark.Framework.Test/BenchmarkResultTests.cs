using System;
using Enterprise.ZArchitecture.Benchmark.Framework.TestInterfaces;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Enterprise.ZArchitecture.Benchmark.Framework.Test
{
	[TestFixture]
	sealed class BenchmarkResultTests
	{
		[Test]
		public void Summary()
		{
			var result = new BenchmarkResult(
				config: TestHelpers.CreateTestConfig(),
				testMethod: typeof(BenchmarkResultTests).GetMethod(nameof(Summary)),
				timings: TestHelpers.CreateSequentialTimings(100),
				totalRuntime: TimeSpan.FromSeconds(1.23),
				runAt: new DateTimeOffset(2025, 04, 10, 17, 38, 21, TimeSpan.FromHours(10))
			);

			var actualSummary = result.GetSummary();
			var expectedSummary = """
				** Summary **
				Enterprise.ZArchitecture.Benchmark.Framework.Test.BenchmarkResultTests.Summary
				Run At:    2025-04-10 17:38:21 +10:00
				Run By:    Bilbo.Baggins
				Saved To:  Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save summary to temp folder.
				Aggregate: Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save aggregate data to temp folder.
				See also:  https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/16081/Micro-Benchmark-Framework

				         mm:ss.mmmuuunnnppp
				Median:  00:00.050500000000 | 050.50ms
				Mean:    00:00.050500000000 | 050.50ms
				Ops/Sec: 19.80

				** For Spreadsheets **
				Class	Test	Date Run	DateTime Run	Who Ran	Mean (ms)	Median (ms)	Ops/sec	Runtime
				BenchmarkResultTests	Summary	2025-04-10	2025-04-10 17:38:21 +10:00	Bilbo.Baggins	50.500000000000	50.500000000000	19.80	net2.99

				** General Stats **
				Time Running Benchmark Code: 00:00:05.0500000
				Target Run Time:             00:00:15
				Total Run Time:              00:00:01.2300000
				Overhead Time:               -00:00:03.8200000
				Total Iterations:            100
				Batch Size:                  1
				Total Benchmark Executions:  100

				** Percentiles **
				       mm:ss.mmmuuunnnppp
				Max:   00:00.100000000000 | 100.00ms
				P99.9: 00:00.099900999999 | 099.90ms
				P99:   00:00.099010000000 | 099.01ms
				P95:   00:00.095049999999 | 095.05ms
				P90:   00:00.090100000000 | 090.10ms
				P75:   00:00.075250000000 | 075.25ms
				P50:   00:00.050500000000 | 050.50ms
				P25:   00:00.025750000000 | 025.75ms
				P05:   00:00.005950000000 | 005.95ms
				P01:   00:00.001990000000 | 001.99ms
				Min:   00:00.001000000000 | 001.00ms

				** Environment **
				Computer Name: Fellowship-3175
				CPU Name:      TomBombadil Enterprises, 999.99GHz
				CPU Sockets:   2
				CPU Cores:     4
				Logical CPUs:  8
				Pointer Size:  8 bytes
				Power Profile: UltraSuperHot
				Runtime:       net2.99
				SQL Server:    Rivendell-SQL-02
				SQL Version:
				  PostgreSQL 17.4
				  Feb 28 2025 18:24:49
				  Copyright (C) PostgreSQL Global Development Group
				""";
			Assert.That(actualSummary, Is.EqualTo(expectedSummary));
		}

		[Test]
		public void Summary_WithDifferentLocalEnvironment_AndParameters()
		{
			var env = new Mock<ILocalEnvironment>();
			env.Setup(x => x.Username).Returns("Darth.Vader");
			env.Setup(x => x.MachineName).Returns("DTHSTR7731");
			env.Setup(x => x.GetCPUDetails()).Returns(("Empire Industries, 666.66GHz", 4, 64, 128));
			env.Setup(x => x.DatabaseServerName).Returns("DTHSTR-SQL-99");
			env.Setup(x => x.DatabaseFullVersionText).Returns("""
				MySql 9.2
				Copyright (C) Oracle Corporation
				""");
			env.Setup(x => x.PowerProfileName).Returns("Really Fast");
			env.Setup(x => x.RuntimeVersion).Returns("silverlight");

			var result = new BenchmarkResult(
				config: TestHelpers.CreateTestConfig(localEnvironment: env.Object)
						.WithMinimumRuns(999)
						.WithExecutionsPerIteration(24)
						.WithWarmUpIterations(82)
						.WithTargetTime(TimeSpan.FromSeconds(41.88)),
				testMethod: typeof(BenchmarkResultTests).GetMethod(nameof(Summary_WithDifferentLocalEnvironment_AndParameters)),
				timings: TestHelpers.CreateSequentialTimings(101),
				totalRuntime: TimeSpan.FromSeconds(2.345),
				runAt: new DateTimeOffset(2026, 05, 11, 18, 39, 22, TimeSpan.FromHours(-4))
			);

			var actualSummary = result.GetSummary();
			var expectedSummary = """
				** Summary **
				Enterprise.ZArchitecture.Benchmark.Framework.Test.BenchmarkResultTests.Summary_WithDifferentLocalEnvironment_AndParameters
				Run At:    2026-05-11 18:39:22 -04:00
				Run By:    Darth.Vader
				Saved To:  Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save summary to temp folder.
				Aggregate: Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save aggregate data to temp folder.
				See also:  https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/16081/Micro-Benchmark-Framework

				         mm:ss.mmmuuunnnppp
				Median:  00:00.051000000000 | 051.00ms
				Mean:    00:00.051000000000 | 051.00ms
				Ops/Sec: 19.61

				** For Spreadsheets **
				Class	Test	Date Run	DateTime Run	Who Ran	Mean (ms)	Median (ms)	Ops/sec	Runtime
				BenchmarkResultTests	Summary_WithDifferentLocalEnvironment_AndParameters	2026-05-11	2026-05-11 18:39:22 -04:00	Darth.Vader	51.000000000000	51.000000000000	19.61	silverlight

				** General Stats **
				Time Running Benchmark Code: 00:02:03.6240000
				Target Run Time:             00:00:41.8800000
				Total Run Time:              00:00:02.3450000
				Overhead Time:               -00:02:01.2790000
				Total Iterations:            101
				Batch Size:                  24
				Total Benchmark Executions:  2,424

				** Percentiles **
				       mm:ss.mmmuuunnnppp
				Max:   00:00.101000000000 | 101.00ms
				P99.9: 00:00.100900000000 | 100.90ms
				P99:   00:00.100000000000 | 100.00ms
				P95:   00:00.096000000000 | 096.00ms
				P90:   00:00.091000000000 | 091.00ms
				P75:   00:00.076000000000 | 076.00ms
				P50:   00:00.051000000000 | 051.00ms
				P25:   00:00.026000000000 | 026.00ms
				P05:   00:00.006000000000 | 006.00ms
				P01:   00:00.002000000000 | 002.00ms
				Min:   00:00.001000000000 | 001.00ms

				** Environment **
				Computer Name: DTHSTR7731
				CPU Name:      Empire Industries, 666.66GHz
				CPU Sockets:   4
				CPU Cores:     64
				Logical CPUs:  128
				Pointer Size:  8 bytes
				Power Profile: Really Fast
				Runtime:       silverlight
				SQL Server:    DTHSTR-SQL-99
				SQL Version:
				  MySql 9.2
				  Copyright (C) Oracle Corporation
				""";
			Assert.That(actualSummary, Is.EqualTo(expectedSummary));
		}

		[Test]
		public void Summary_WithNoBenchmarkResults()
		{
			var result = new BenchmarkResult(
				config: TestHelpers.CreateTestConfig(localEnvironment: new NullEnvironment())
						.WithMinimumRuns(0)
						.WithExecutionsPerIteration(0)
						.WithWarmUpIterations(0)
						.WithTargetTime(TimeSpan.Zero),
				testMethod: typeof(BenchmarkResultTests).GetMethod(nameof(Summary_WithNoBenchmarkResults)),
				timings: [],
				totalRuntime: TimeSpan.FromSeconds(0),
				runAt: DateTimeOffset.MinValue
			);

			var actualSummary = result.GetSummary();
			var expectedSummary = """
				** Summary **
				Enterprise.ZArchitecture.Benchmark.Framework.Test.BenchmarkResultTests.Summary_WithNoBenchmarkResults
				Run At:    0001-01-01 00:00:00 +00:00
				Run By:    
				Saved To:  Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save summary to temp folder.
				Aggregate: Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save aggregate data to temp folder.
				See also:  https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/16081/Micro-Benchmark-Framework

				         mm:ss.mmmuuunnnppp
				Median:  00:00.000000000000 | 000ps
				Mean:    00:00.000000000000 | 000ps
				Ops/Sec: 0.00

				** For Spreadsheets **
				Class	Test	Date Run	DateTime Run	Who Ran	Mean (ms)	Median (ms)	Ops/sec	Runtime
				BenchmarkResultTests	Summary_WithNoBenchmarkResults	0001-01-01	0001-01-01 00:00:00 +00:00		0.000000000000	0.000000000000	0.00	

				** General Stats **
				Time Running Benchmark Code: 00:00:00
				Target Run Time:             00:00:00
				Total Run Time:              00:00:00
				Overhead Time:               00:00:00
				Total Iterations:            0
				Batch Size:                  0
				Total Benchmark Executions:  0

				** Percentiles **
				       mm:ss.mmmuuunnnppp
				Max:   00:00.000000000000 | 000ps
				P99.9: 00:00.000000000000 | 000ps
				P99:   00:00.000000000000 | 000ps
				P95:   00:00.000000000000 | 000ps
				P90:   00:00.000000000000 | 000ps
				P75:   00:00.000000000000 | 000ps
				P50:   00:00.000000000000 | 000ps
				P25:   00:00.000000000000 | 000ps
				P05:   00:00.000000000000 | 000ps
				P01:   00:00.000000000000 | 000ps
				Min:   00:00.000000000000 | 000ps

				** Environment **
				Computer Name: 
				CPU Name:      
				CPU Sockets:   0
				CPU Cores:     0
				Logical CPUs:  0
				Pointer Size:  8 bytes
				Power Profile: 
				Runtime:       
				SQL Server:    
				SQL Version:
				  
				""";
			Assert.That(actualSummary, Is.EqualTo(expectedSummary));
		}

		[Test]
		public void Summary_WithOneBenchmarkResult()
		{
			var result = new BenchmarkResult(
				config: TestHelpers.CreateTestConfig(),
				testMethod: typeof(BenchmarkResultTests).GetMethod(nameof(Summary_WithOneBenchmarkResult)),
				timings: TestHelpers.CreateSequentialTimings(1),
				totalRuntime: TimeSpan.FromSeconds(1),
				runAt: new DateTimeOffset(2025, 04, 10, 17, 38, 21, TimeSpan.FromHours(10))
			);

			var actualSummary = result.GetSummary();
			var expectedSummary = """
				** Summary **
				Enterprise.ZArchitecture.Benchmark.Framework.Test.BenchmarkResultTests.Summary_WithOneBenchmarkResult
				Run At:    2025-04-10 17:38:21 +10:00
				Run By:    Bilbo.Baggins
				Saved To:  Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save summary to temp folder.
				Aggregate: Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save aggregate data to temp folder.
				See also:  https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/16081/Micro-Benchmark-Framework

				         mm:ss.mmmuuunnnppp
				Median:  00:00.001000000000 | 001.00ms
				Mean:    00:00.001000000000 | 001.00ms
				Ops/Sec: 1,000.00

				** For Spreadsheets **
				Class	Test	Date Run	DateTime Run	Who Ran	Mean (ms)	Median (ms)	Ops/sec	Runtime
				BenchmarkResultTests	Summary_WithOneBenchmarkResult	2025-04-10	2025-04-10 17:38:21 +10:00	Bilbo.Baggins	1.000000000000	1.000000000000	1,000.00	net2.99

				** General Stats **
				Time Running Benchmark Code: 00:00:00.0010000
				Target Run Time:             00:00:15
				Total Run Time:              00:00:01
				Overhead Time:               00:00:00.9990000
				Total Iterations:            1
				Batch Size:                  1
				Total Benchmark Executions:  1

				** Percentiles **
				       mm:ss.mmmuuunnnppp
				Max:   00:00.001000000000 | 001.00ms
				P99.9: 00:00.001000000000 | 001.00ms
				P99:   00:00.001000000000 | 001.00ms
				P95:   00:00.001000000000 | 001.00ms
				P90:   00:00.001000000000 | 001.00ms
				P75:   00:00.001000000000 | 001.00ms
				P50:   00:00.001000000000 | 001.00ms
				P25:   00:00.001000000000 | 001.00ms
				P05:   00:00.001000000000 | 001.00ms
				P01:   00:00.001000000000 | 001.00ms
				Min:   00:00.001000000000 | 001.00ms

				** Environment **
				Computer Name: Fellowship-3175
				CPU Name:      TomBombadil Enterprises, 999.99GHz
				CPU Sockets:   2
				CPU Cores:     4
				Logical CPUs:  8
				Pointer Size:  8 bytes
				Power Profile: UltraSuperHot
				Runtime:       net2.99
				SQL Server:    Rivendell-SQL-02
				SQL Version:
				  PostgreSQL 17.4
				  Feb 28 2025 18:24:49
				  Copyright (C) PostgreSQL Global Development Group
				""";
			Assert.That(actualSummary, Is.EqualTo(expectedSummary));
		}

		[Test]
		public void Summary_WithBenchmarkResultsFromMultipleIterations()
		{
			var result = new BenchmarkResult(
				config: TestHelpers.CreateTestConfig().WithExecutionsPerIteration(2),
				testMethod: typeof(BenchmarkResultTests).GetMethod(nameof(Summary_WithBenchmarkResultsFromMultipleIterations)),
				timings: TestHelpers.CreateSequentialTimings(100),
				totalRuntime: TimeSpan.FromSeconds(1.23),
				runAt: new DateTimeOffset(2025, 04, 10, 17, 38, 21, TimeSpan.FromHours(10))
			);

			var actualSummary = result.GetSummary();
			var expectedSummary = """
				** Summary **
				Enterprise.ZArchitecture.Benchmark.Framework.Test.BenchmarkResultTests.Summary_WithBenchmarkResultsFromMultipleIterations
				Run At:    2025-04-10 17:38:21 +10:00
				Run By:    Bilbo.Baggins
				Saved To:  Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save summary to temp folder.
				Aggregate: Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save aggregate data to temp folder.
				See also:  https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/16081/Micro-Benchmark-Framework

				         mm:ss.mmmuuunnnppp
				Median:  00:00.050500000000 | 050.50ms
				Mean:    00:00.050500000000 | 050.50ms
				Ops/Sec: 19.80

				** For Spreadsheets **
				Class	Test	Date Run	DateTime Run	Who Ran	Mean (ms)	Median (ms)	Ops/sec	Runtime
				BenchmarkResultTests	Summary_WithBenchmarkResultsFromMultipleIterations	2025-04-10	2025-04-10 17:38:21 +10:00	Bilbo.Baggins	50.500000000000	50.500000000000	19.80	net2.99

				** General Stats **
				Time Running Benchmark Code: 00:00:10.1000000
				Target Run Time:             00:00:15
				Total Run Time:              00:00:01.2300000
				Overhead Time:               -00:00:08.8700000
				Total Iterations:            100
				Batch Size:                  2
				Total Benchmark Executions:  200

				** Percentiles **
				       mm:ss.mmmuuunnnppp
				Max:   00:00.100000000000 | 100.00ms
				P99.9: 00:00.099900999999 | 099.90ms
				P99:   00:00.099010000000 | 099.01ms
				P95:   00:00.095049999999 | 095.05ms
				P90:   00:00.090100000000 | 090.10ms
				P75:   00:00.075250000000 | 075.25ms
				P50:   00:00.050500000000 | 050.50ms
				P25:   00:00.025750000000 | 025.75ms
				P05:   00:00.005950000000 | 005.95ms
				P01:   00:00.001990000000 | 001.99ms
				Min:   00:00.001000000000 | 001.00ms

				** Environment **
				Computer Name: Fellowship-3175
				CPU Name:      TomBombadil Enterprises, 999.99GHz
				CPU Sockets:   2
				CPU Cores:     4
				Logical CPUs:  8
				Pointer Size:  8 bytes
				Power Profile: UltraSuperHot
				Runtime:       net2.99
				SQL Server:    Rivendell-SQL-02
				SQL Version:
				  PostgreSQL 17.4
				  Feb 28 2025 18:24:49
				  Copyright (C) PostgreSQL Global Development Group
				""";
			Assert.That(actualSummary, Is.EqualTo(expectedSummary));
		}

		[Test]
		public void FailWithSummaryOnLocal_WhenRunningOnDAT()
		{
			var testContextFake = new TestContext_ForTest(isRunningOnDat: true);
			var config = TestHelpers.CreateTestConfig(testContext: testContextFake, localEnvironment: new NullEnvironment());
			var result = TestHelpers.CreateTestBenchmarkResult(typeof(BenchmarkResultTests).GetMethod(nameof(FailWithSummaryOnLocal_WhenRunningOnDAT)), config: config);

			result.ReportSummaryOnLocal();

			Assert.That(testContextFake.PassMessage, Is.EqualTo("Performance tests only run on DAT to ensure correctness; benchmark figures are not gathered or asserted."));
			Assert.That(testContextFake.ReportTestMessage, Is.Null.Or.Empty);
		}

		[Test]
		public void FailWithSummaryOnLocal_WhenRunningOnLocal()
		{
			var testContextFake = new TestContext_ForTest(isRunningOnDat: false);
			var config = TestHelpers.CreateTestConfig(testContext: testContextFake, localEnvironment: new NullEnvironment());
			var result = TestHelpers.CreateTestBenchmarkResult(typeof(BenchmarkResultTests).GetMethod(nameof(FailWithSummaryOnLocal_WhenRunningOnLocal)), config: config);
			var summary = result.GetSummary();

			result.ReportSummaryOnLocal();

			Assert.That(testContextFake.PassMessage, Is.Null.Or.Empty);
			var expectedSummary = $"""
				ℹ️ Important: this test never fails on DAT, and always fails locally (so you can see the numbers below).

				-----

				{summary}
				""";
			Assert.That(testContextFake.ReportTestMessage, Is.EqualTo(expectedSummary));
		}
	}
}
