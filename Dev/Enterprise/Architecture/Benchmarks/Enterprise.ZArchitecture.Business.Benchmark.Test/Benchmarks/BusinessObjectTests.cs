using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Benchmark.Framework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Benchmark.Test.Benchmarks
{
	sealed class BusinessObjectTests : TransactionedTestCase
	{
		public void TestBusinessObjectPropertyRead_Performance()
		{
			var factory = new BusinessObjectFactory();
			var obj = factory.New<StmData>();
			obj.SD_GuidValue = ZGuid.NewZGuid();

			var runner = new BenchmarkRunner(BenchmarkConfig.Default().WithExecutionsPerIteration(256), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return obj.SD_GuidValue;
			}
		}

		public void TestBusinessObjectPropertyWrite_Performance()
		{
			var factory = new BusinessObjectFactory();
			var obj = factory.New<StmData>();

			var runner = new BenchmarkRunner(BenchmarkConfig.Default().WithExecutionsPerIteration(16), RunMethod);
			runner.Run(RunBenchmarkMethod, setup: () => obj.SD_GuidValue = ZGuid.NewZGuid()).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				obj.SD_GuidValue = ZGuid.Empty;
				return obj;
			}
		}

		public void TestBusinessObjectFactoryValidation_Performance()
		{
			var factory = new BusinessObjectFactory();
			var obj = factory.New<StmData>();
			obj.SD_BinaryValue = new byte[4];
			obj.SD_DepartmentGuid = ZGuid.NewZGuid();
			obj.SD_GuidValue = ZGuid.NewZGuid();
			obj.SD_Name = "The name";
			obj.SD_Type = "EDT";

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				obj.RunPreSaveValidation();
				if (obj.HasErrors)
				{
					return new BenchmarkException("Error notifications were present after running validation. Expected a valid business object.");
				}
				return obj.Notifications;
			}
		}

		public void TestBusinessObjectFactoryValidationWithError_Performance()
		{
			var factory = new BusinessObjectFactory();
			var obj = factory.New<StmData>();
			obj.SD_BinaryValue = new byte[4];
			obj.SD_DepartmentGuid = ZGuid.Invalid;
			obj.SD_GuidValue = ZGuid.Invalid;
			obj.SD_Name = "The name";
			obj.SD_Type = "EDT";

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				obj.RunPreSaveValidation();
				if (!obj.HasErrors)
				{
					return new BenchmarkException("Error notifications were NOT present after running validation. Expected an invalid business object.");
				}
				return obj.Notifications;
			}
		}
	}
}
