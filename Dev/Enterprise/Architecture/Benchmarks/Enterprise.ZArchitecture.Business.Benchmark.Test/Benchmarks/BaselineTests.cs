using CargoWise.Types;
using Enterprise.ZArchitecture.Benchmark.Framework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Benchmark.Test.Benchmarks
{
	[TestFixture]
	sealed class BaselineTests
	{
		[Test]
		public void TestEmptyMethod_Performance()
		{
			var config = BenchmarkConfig.Default()
							.WithExecutionsPerIteration(4_096)
							.WithNUnit4TestContext()
							.WithNoDatabaseEnvironment();
			var runner = new BenchmarkRunner(config, TestEmptyMethod_Performance);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return null;
			}
		}

		[Test]
		public void TestNewObject_Performance()
		{
			var config = BenchmarkConfig.Default()
							.WithExecutionsPerIteration(4_096)
							.WithNUnit4TestContext()
							.WithNoDatabaseEnvironment();
			var runner = new BenchmarkRunner(config, TestNewObject_Performance);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return new PropertyReadAndWriteTestClass();
			}
		}

		[Test]
		public void TestPropertyRead_Performance()
		{
			var config = BenchmarkConfig.Default()
							.WithExecutionsPerIteration(4_096)
							.WithNUnit4TestContext()
							.WithNoDatabaseEnvironment();
			var runner = new BenchmarkRunner(config, TestPropertyRead_Performance);
			runner.Run(RunBenchmarkMethod, setup: () => new PropertyReadAndWriteTestClass { TheProperty = ZGuid.NewZGuid() }).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				var obj = (PropertyReadAndWriteTestClass)param;
				return obj.TheProperty;
			}
		}

		[Test]
		public void TestPropertyWrite_Performance()
		{
			var config = BenchmarkConfig.Default()
							.WithExecutionsPerIteration(4_096)
							.WithNUnit4TestContext()
							.WithNoDatabaseEnvironment();
			var runner = new BenchmarkRunner(config, TestPropertyWrite_Performance);
			runner.Run(RunBenchmarkMethod, setup: () => new PropertyReadAndWriteTestClass { TheProperty = ZGuid.NewZGuid() }).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				var obj = (PropertyReadAndWriteTestClass)param;
				obj.TheProperty = ZGuid.Empty;
				return obj;
			}
		}

		class PropertyReadAndWriteTestClass
		{
			public ZGuid TheProperty { get; set; }
		}
	}
}
