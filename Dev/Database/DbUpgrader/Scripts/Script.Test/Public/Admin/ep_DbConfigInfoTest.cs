using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_DbConfigInfo))]
	class ep_DbConfigInfoTest : DbCreateScriptTest
	{
		[UseSnapshotProtection]
		public void TestSystemInfo()
		{
			using (var connection = Db.NewAdminConnection())
			using (var otherConnection = Db.NewAdminConnection())
			{
				var traceStatus = GetTraceStatus(connection);
				try
				{
					AssertReader(connection, traceStatus);
					var traceOn = $@"
DBCC TRACEON(205, 610, -1)
DBCC TRACEON(260)
";
					otherConnection.ExecuteNonQuery(traceOn);
					traceStatus.Add(205);
					traceStatus.Add(610);
					AssertReader(connection, traceStatus);
				}
				finally
				{
					var traceOff = $@"
DBCC TRACEOFF(205, 610,  -1)
DBCC TRACEOFF(260)";
					otherConnection.ExecuteNonQuery(traceOff);
				}
			}
		}

		List<short> GetTraceStatus(DbConnection connection)
		{
			var result = new List<short>();
			using (var reader = connection.Command("dbcc tracestatus").ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add((short)reader["TraceFlag"]);
				}
			}

			return result;
		}

		void AssertReader(AdminConnection connection, List<short> expectedTraces)
		{
			var maxdop = connection.ExecuteScalar("select value_in_use FROM sys.configurations where name = 'max degree of parallelism'");
			var cost = connection.ExecuteScalar("select value_in_use FROM sys.configurations where name = 'cost threshold for parallelism'");
			var adhoc = connection.ExecuteScalar("select CONVERT(BIT, value_in_use) FROM sys.configurations where name = 'optimize for ad hoc workloads'");
			var min = connection.ExecuteScalar("select value_in_use FROM sys.configurations where name = 'min server memory (MB)'");
			var max = connection.ExecuteScalar("select value_in_use FROM sys.configurations where name = 'max server memory (MB)'");
			var tot = connection.ExecuteScalar("select maximum FROM sys.configurations where name = 'max server memory (MB)'");
			var alwayson = CargoWise.Data.SqlServer.AlwaysOn.IsDbPartOfAlwaysOn(connection, connection.CurrentDatabase);

			byte compatibilityLevel;
			using (var cmd = connection.Command("SELECT compatibility_level FROM sys.databases WHERE name = @dbname"))
			{
				cmd.AddParameter("@dbname", SqlDbType.VarChar, 128, connection.CurrentDatabase);
				compatibilityLevel = (byte)cmd.ExecuteScalar();
			}

			using (var cmd = connection.Command($"EXEC {ScriptToTest.Name} '{connection.CurrentDatabase}'"))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals("Must return a row => Read()?", true, reader.Read());

				AssertEquals(maxdop, reader["MAXDOP"]);
				AssertEquals(cost, reader["CostOfParallelism"]);
				AssertEquals(adhoc, reader["OptimizeForAdHocWorkloads"]);
				AssertEquals(min, reader["MinMemory"]);
				AssertEquals(max, reader["MaxMemory"]);
				AssertEquals(tot, reader["TotalAvailableMemory"]);
				AssertEquals(tot, reader["TotalAvailableMemory"]);
				AssertEquals(alwayson ? 1 : 0, reader["DbAlwaysOn"]);
				AssertEquals(false, reader["ParameterizationForced"]);
				AssertEquals(true, reader["AutoCreate"]);
				AssertEquals(true, reader["AutoUpdate"]);
				AssertEquals(true, reader["AutoUpdateAsync"]);
				AssertEquals(compatibilityLevel, reader["CompatibilityLevel"]);
				AssertEquals(false, reader["Cardinality"]);

				var actualTraceFlags = reader["TraceFlags"].ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => short.Parse(v));
				AssertContainsExactElementsInAnyOrder("We only expect global traces", expectedTraces, actualTraceFlags);

				AssertEquals("Return only one row => Read()?", false, reader.Read());
			}
		}
	}
}

