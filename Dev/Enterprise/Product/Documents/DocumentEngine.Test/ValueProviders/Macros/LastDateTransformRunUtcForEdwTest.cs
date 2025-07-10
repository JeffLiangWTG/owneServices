using System;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LastDateTransformRunUtcForEdw))]
	sealed class LastDateTransformRunUtcForEdwTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertEquals("should not match <>", false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals("should not match <Lastt Date Transform Run Utc For Edw>", false, ValueProviderToTest.IsResponsibleForReplacing("<Lastt Date Transform Run Utc For Edw>", Passes.FirstPass));
			AssertEquals("should match <Last Date Transform Run Utc For Edw>", true, ValueProviderToTest.IsResponsibleForReplacing("<Last Date Transform Run Utc For Edw>", Passes.FirstPass));
			AssertEquals("should match <LastDateTransformRunUtcForEdw>", true, ValueProviderToTest.IsResponsibleForReplacing("<LastDateTransformRunUtcForEdw>", Passes.FirstPass));
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestReplacement()
		{
			using (SystemDataRegistry.Instance.ForcefullyDisabledTasks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EET"))
			{
				var task = Factory.New<StmScheduleTask>();
				task.S5_IsActive = true;
				task.S5_ParentTableCode = "SH";
				task.S5_ScheduleDescription = "ETL Execution Task For Test";
				task.S5_TypeOfDocument = "BI";
				task.S5_ScheduleType = "EET";
				Factory.Save();

				var edwServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
				var edWConnection = !string.IsNullOrEmpty(edwServerName) ? Db.NewExtraConnectionWithMainDbCredentials(edwServerName, Db.EdwDatabaseName) : null;
				AssertNotNull(edWConnection);

				var sql = string.Format($"INSERT INTO {Db.EdwDatabaseName}.biadmin.MasterState (ParamName, ParamValue) VALUES ('{BiConstants.LastDateTransformRunUtc}', '2024-01-01 00:00:00.000')");
				var cmd = edWConnection.Command(sql);
				_ = cmd.ExecuteScalar();

				using (SystemDataRegistry.Instance.ForcefullyDisabledTasks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					AssertEquals("2024-01-01 00:00", ValueProviderToTest.GetReplacement("<ChinaBalanceSheetStartingAccount>", Report));
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LastDateTransformRunUtcForEdw();
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public override void TestDocumentation()
		{
			var task = Factory.New<StmScheduleTask>();
			task.S5_IsActive = true;
			task.S5_ParentTableCode = "SH";
			task.S5_ScheduleDescription = "ETL Execution Task For Test";
			task.S5_TypeOfDocument = "BI";
			task.S5_ScheduleType = "EET";
			Factory.Save();

			base.TestDocumentation();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var edwServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			var edWConnection = !string.IsNullOrEmpty(edwServerName) ? Db.NewExtraConnectionWithMainDbCredentials(edwServerName, Db.EdwDatabaseName) : null;
			AssertNotNull(edWConnection);

			var sql = string.Format($"INSERT INTO {Db.EdwDatabaseName}.biadmin.MasterState (ParamName, ParamValue) VALUES ('{BiConstants.LastDateTransformRunUtc}', '2024-01-01 00:00:00.000')");
			var cmd = edWConnection.Command(sql);
			_ = cmd.ExecuteScalar();
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestReplacement_EDWConnectionIsNull()
		{
			var edwServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			var edWConnection = !string.IsNullOrEmpty(edwServerName) ? Db.NewExtraConnectionWithMainDbCredentials(edwServerName, Db.EdwDatabaseName) : null;
			AssertNotNull(edWConnection);

			var sql = string.Format($"INSERT INTO {Db.EdwDatabaseName}.biadmin.MasterState (ParamName, ParamValue) VALUES ('{BiConstants.LastDateTransformRunUtc}', '2024-01-01 00:00:00.000')");
			var cmd = edWConnection.Command(sql);
			_ = cmd.ExecuteScalar();

			var lastDateTransformRunUtcForEdwSql = string.Format("SELECT CONVERT(varchar(16), [ParamValue], 120) AS LastDateTransformRunUtc FROM [{0}].[biadmin].[MasterState] WHERE [ParamName] = '{1}'", Db.EdwDatabaseName, BiConstants.LastDateTransformRunUtc);
			var cmdQuery = edWConnection.Command(lastDateTransformRunUtcForEdwSql);
			var result = cmdQuery.ExecuteScalar();
			AssertEquals("2024-01-01 00:00", (string)result);

			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			{
				AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ChinaBalanceSheetStartingAccount>", Report));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestReplacement_EDWConnectionIsEmptyWhenEETServiceTaskIsDisabledOrInActive()
		{
			using (SystemDataRegistry.Instance.ForcefullyDisabledTasks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EET"))
			{
				var task = Factory.New<StmScheduleTask>();
				task.S5_IsActive = true;
				task.S5_ParentTableCode = "SH";
				task.S5_ScheduleDescription = "ETL Execution Task For Test";
				task.S5_TypeOfDocument = "BI";
				task.S5_ScheduleType = "EET";
				Factory.Save();

				var edwServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
				var edWConnection = !string.IsNullOrEmpty(edwServerName) ? Db.NewExtraConnectionWithMainDbCredentials(edwServerName, Db.EdwDatabaseName) : null;
				AssertNotNull(edWConnection);

				AssertEquals("When EET is disabled and S5_isActive is true, GetReplacement should return String.Empty", string.Empty, ValueProviderToTest.GetReplacement("<ChinaBalanceSheetStartingAccount>", Report));
			}

			using (SystemDataRegistry.Instance.ForcefullyDisabledTasks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var task = Factory.New<StmScheduleTask>();
				task.S5_IsActive = false;
				task.S5_ParentTableCode = "SH";
				task.S5_ScheduleDescription = "ETL Execution Task For Test";
				task.S5_TypeOfDocument = "BI";
				task.S5_ScheduleType = "EET";
				Factory.Save();

				var edwServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
				var edWConnection = !string.IsNullOrEmpty(edwServerName) ? Db.NewExtraConnectionWithMainDbCredentials(edwServerName, Db.EdwDatabaseName) : null;
				AssertNotNull(edWConnection);

				AssertEquals("When EET is enabled and S5_isActive is false, GetReplacement should return String.Empty", string.Empty, ValueProviderToTest.GetReplacement("<ChinaBalanceSheetStartingAccount>", Report));
			}
		}
	}
}
