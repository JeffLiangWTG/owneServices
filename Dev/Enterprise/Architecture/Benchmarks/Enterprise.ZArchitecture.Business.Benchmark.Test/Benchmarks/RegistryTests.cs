using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Benchmark.Framework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Benchmark.Test.Benchmarks
{
	sealed class RegistryTests : TransactionedTestCase
	{
		public void TestRegistryCachedRead_Performance()
		{
			AssertEquals("Precondition: System level storage", RegistryStorageFlags.System, RawDataRegistry.Instance.FTPConnectionTimeout.Storage);
			AssertEquals("Precondition: Is Cached", RegistryOptions.Default, RawDataRegistry.Instance.FTPConnectionTimeout.Options);
			AssertType<IntRegistryItem>("Precondition: IntRegistryType for all registry tests", RawDataRegistry.Instance.FTPConnectionTimeout);

			var runner = new BenchmarkRunner(BenchmarkConfig.Default().WithExecutionsPerIteration(256), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return RawDataRegistry.Instance.FTPConnectionTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		public void TestRegistryUncachedRead_Performance()
		{
			AssertEquals("Precondition: System level storage", RegistryStorageFlags.System, RawDataRegistry.Instance.DocumentCustomisationVersionNumber.Storage);
			AssertEquals("Precondition: Uncached option", RegistryOptions.IsHidden | RegistryOptions.NotCached, RawDataRegistry.Instance.DocumentCustomisationVersionNumber.Options);
			AssertType<IntRegistryItem>("Precondition: IntRegistryType for all registry tests", RawDataRegistry.Instance.DocumentCustomisationVersionNumber);

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return RawDataRegistry.Instance.DocumentCustomisationVersionNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}
	}
}
