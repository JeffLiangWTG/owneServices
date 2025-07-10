using System;
using Enterprise.ZArchitecture.Benchmark.Framework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Benchmark.Test.Benchmarks
{
	sealed class BaselineSqlTests : TransactionedTestCase
	{
		public void TestLoadRowWithDirectSQL_Performance()
		{
			var pk = TestConnection.ExecuteScalar<Guid>("SELECT TOP 1 SD_PK FROM dbo.StmData WHERE SD_Name = 'DatabaseMajorScriptVersion'");
			var cmd = TestConnection.Command("SELECT * FROM dbo.StmData WHERE SD_PK = @pk");
			cmd.AddParameter("@pk", sqlDbType: System.Data.SqlDbType.UniqueIdentifier, pk);

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					return values;
				}
			}
		}

		public void TestInsertRowWithDirectSQL_Performance()
		{
			int counter = 1;

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				var cmd = TestConnection.Command("INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_GuidValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser) VALUES(@pPK, @pName, @pGuid, GETUTCDATE(), 'E')");
				cmd.AddParameter("@pPK", sqlDbType: System.Data.SqlDbType.UniqueIdentifier, Guid.NewGuid());
				cmd.AddParameter("@pName", sqlDbType: System.Data.SqlDbType.VarChar, "Benchmark" + counter);
				cmd.AddParameter("@pGuid", sqlDbType: System.Data.SqlDbType.UniqueIdentifier, Guid.NewGuid());

				cmd.ExecuteNonQuery();

				++counter;
				return cmd;
			}
		}

		public void TestUpdateRowWithDirectSQL_Performance()
		{
			var pk = TestConnection.ExecuteScalar<Guid>("SELECT TOP 1 SD_PK FROM StmData WHERE SD_Name = 'DatabaseMajorScriptVersion'");

			var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
			runner.Run(RunBenchmarkMethod).ReportSummaryOnLocal();

			object RunBenchmarkMethod(object param)
			{
				var cmd = TestConnection.Command("UPDATE dbo.StmData SET SD_GuidValue = @pGuid, SD_SystemLastEditTimeUtc = @pLastEditDate, SD_SystemLastEditUser = @pLastEditUser WHERE SD_PK = @pk");
				cmd.AddParameter("@pk", sqlDbType: System.Data.SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@pGuid", sqlDbType: System.Data.SqlDbType.UniqueIdentifier, Guid.NewGuid());
				cmd.AddParameter("@pLastEditDate", sqlDbType: System.Data.SqlDbType.SmallDateTime, DateTime.UtcNow);
				cmd.AddParameter("@pLastEditUser", sqlDbType: System.Data.SqlDbType.VarChar, "E");

				cmd.ExecuteNonQuery();

				return cmd;
			}
		}
	}
}
