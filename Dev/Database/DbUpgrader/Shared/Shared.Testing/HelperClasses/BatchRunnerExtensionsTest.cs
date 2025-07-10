using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	class BatchRunnerExtensionsTest : TestCase
	{
		public void TestRunCommandsGeneratedByCommandOuterCommandTimeout()
		{
			// Arrange
			var testRunner = new BatchRunner();

			// Act
			var exception = AssertExceptionThrown<SqlException>(() =>
			{
				using (var cmd = Db.Connection.Command(sqlText))
				{
					testRunner.RunCommandsGeneratedByCommand(cmd);
				}
			});

			// Assert
			AssertContains("Execution Timeout Expired", exception.Message);
		}

		public void TestRunCommandsGeneratedByCommandInnerCommandTimeout()
		{
			// Arrange
			var testRunner = new BatchRunner();

			// Act
			var exception = AssertExceptionThrown<SqlException>(() =>
			{
				using (var cmd = Db.Connection.Command($"SELECT '{sqlText.Replace("'", "''")}'"))
				{
					testRunner.RunCommandsGeneratedByCommand(cmd);
				}
			});

			// Assert
			AssertContains("Execution Timeout Expired", exception.Message);
		}

		public void TestRunCommandsGeneratedByQueryFails()
		{
			// Arrange
			var testRunner = new BatchRunner();
			const string sqlText =
				"SELECT TOP 2 'SELECT TOP 1 * FROM ' + case name when @TableName1 then name else @InvalidTableName end" +
				" FROM sys.objects WHERE name in (@TableName1, @TableName2) ORDER BY name";

			var exception = AssertExceptionThrown<SqlException>(() =>
			{
				using (var cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddParameter("@TableName1", SqlDbType.NVarChar, "RefCountry");
					cmd.AddParameter("@TableName2", SqlDbType.NVarChar, "RefUNLOCO");
					cmd.AddParameter("@InvalidTableName", SqlDbType.NVarChar, "InvalidTableName");

					// Act
					testRunner.RunCommandsGeneratedByCommand(cmd);
				}
			});

			// Assert
			AssertNotNull(exception);
			AssertEquals("Invalid object name 'InvalidTableName'.", exception.Message);
		}

		public void TestRunCommandsGeneratedByQuerySuccessful()
		{
			// Arrange
			var testRunner = new BatchRunner();
			const string sqlText = "SELECT TOP 2 'SELECT TOP 1 * FROM ' + name FROM sys.objects WHERE name in (@TableName1, @TableName2) ORDER BY name";
			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@TableName1", SqlDbType.NVarChar, "RefCountry");
				cmd.AddParameter("@TableName2", SqlDbType.NVarChar, "RefUNLOCO");
				cmd.AddParameter("@InvalidTableName", SqlDbType.NVarChar, "InvalidTableName");

				// Act
				// Assert
				AssertNoExceptionThrown(() => testRunner.RunCommandsGeneratedByCommand(cmd));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			setTimeoutDisposable = Db.Connection.TemporarySetDefaultCommandTimeOut(defaultCommandTimeoutForTest);
		}

		protected override void TearDown()
		{
			setTimeoutDisposable.Dispose();
			base.TearDown();
		}

		IDisposable setTimeoutDisposable;
		readonly int defaultCommandTimeoutForTest = 1;
		readonly string sqlText = @"WAITFOR DELAY '00:00:02';";
	}
}
