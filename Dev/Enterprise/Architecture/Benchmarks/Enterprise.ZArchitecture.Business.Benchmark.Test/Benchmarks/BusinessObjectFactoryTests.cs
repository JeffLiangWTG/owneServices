using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Benchmark.Framework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Benchmark.Test.Benchmarks
{
	sealed class BusinessObjectFactoryTests : TransactionedTestCase
	{
		public void TestCreateBusinessObjectFactory_Performance()
		{
			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return new BusinessObjectFactory();
			}
		}

		public void TestCreateBusinessObject_Performance()
		{
			var factory = new BusinessObjectFactory();

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return factory.New<StmData>();
			}
		}

		public void TestLoadBusinessObjectByPKCached_Performance()
		{
			var pk = (ZGuid)TestConnection.ExecuteScalar<Guid>("SELECT TOP 1 SD_PK FROM StmData WHERE SD_Name = 'DatabaseMajorScriptVersion'");
			var factory = new BusinessObjectFactory();

			var runner = new BenchmarkRunner(BenchmarkConfig.Default().WithExecutionsPerIteration(64), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return factory.Load<StmData>(pk);
			}
		}

		public void TestLoadBusinessObjectByPKWithReload_Performance()
		{
			var pk = (ZGuid)TestConnection.ExecuteScalar<Guid>("SELECT TOP 1 SD_PK FROM StmData WHERE SD_Name = 'DatabaseMajorScriptVersion'");
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(StmDataSchema.PK, pk);
			query.ReLoadExistingRows = true;

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return factory.Load<StmData>(query)[0];
			}
		}

		public void TestLoadManyBusinessObjects_Performance()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, "Database");
			query.ReLoadExistingRows = true;

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				return factory.Load<StmData>(query);
			}
		}

		public void TestUpdateBusinessObject_Performance()
		{
			var pk = (ZGuid)TestConnection.ExecuteScalar<Guid>("SELECT TOP 1 SD_PK FROM StmData WHERE SD_Name = 'DatabaseMajorScriptVersion'");
			var factory = new BusinessObjectFactory();
			var data = factory.Load<StmData>(pk);

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				data.SD_GuidValue = ZGuid.NewZGuid();
				factory.Save();
				return data;
			}
		}

		public void TestUpdateManyBusinessObjects_Performance()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, "Database");
			query.ReLoadExistingRows = true;
			var data = factory.Load<StmData>(query);

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				foreach (var d in data)
				{
					d.SD_GuidValue = ZGuid.NewZGuid();
				}
				factory.Save();
				return data;
			}
		}

		public void TestInsertBusinessObject_Performance()
		{
			int counter = 1;

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod, setup: () => new BusinessObjectFactory()).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				var factory = (BusinessObjectFactory)param;
				var data = factory.New<StmData>();
				data.SD_Name = "Benchmark" + counter;
				data.SD_GuidValue = ZGuid.NewZGuid();
				factory.Save();
				++counter;
				return data;
			}
		}

		public void TestInsertManyBusinessObjects_Performance()
		{
			int counter = 1;

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod, setup: () => new BusinessObjectFactory()).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				var factory = (BusinessObjectFactory)param;
				for (int i = 0; i < 20; i++)
				{
					var data = factory.New<StmData>();
					data.SD_Name = $"Benchmark_{counter}_{i}";
					data.SD_GuidValue = ZGuid.NewZGuid();
				}
				factory.Save();
				++counter;
				return factory;
			}
		}
	}
}
