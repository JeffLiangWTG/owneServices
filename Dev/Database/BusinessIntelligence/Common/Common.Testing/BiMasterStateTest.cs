namespace CargoWise.Bi.Common.Testing
{
	using System;
	using System.Globalization;
	using CargoWise.Data;
	using CargoWise.Integration;
	using NUnit.Framework;

	abstract class BiMasterStateTest : TestCase
	{
		public void TestSetParameterValue()
		{
			AssertEquals(paramName, string.Empty, BiMasterState.GetParameter(TestBiConnection, paramName));

			var expectedParamValue = "TEST_VALUE";

			BiMasterState.SetParameter(TestBiConnection, paramName, expectedParamValue);
			AssertEquals(paramName, expectedParamValue, BiMasterState.GetParameter(TestBiConnection, paramName));

			BiMasterState.DeleteParameter(TestBiConnection, paramName);
			AssertEquals(paramName, string.Empty, BiMasterState.GetParameter(TestBiConnection, paramName));
		}

		public void TestSetParameterValue_LengthMoreThan128Characters()
		{
			AssertEquals(paramName, string.Empty, BiMasterState.GetParameter(TestBiConnection, paramName));

			var expectedParamValue = new string('*', 1000);

			BiMasterState.SetParameter(TestBiConnection, paramName, expectedParamValue);
			AssertEquals(paramName, expectedParamValue, BiMasterState.GetParameter(TestBiConnection, paramName));

			BiMasterState.DeleteParameter(TestBiConnection, paramName);
			AssertEquals(paramName, string.Empty, BiMasterState.GetParameter(TestBiConnection, paramName));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
			TestBiConnection = Db.NewAdminConnection(auditServer, DbName);

			token = TestBiConnection.BeginTransactionWithManager();
		}

		protected abstract string DbName { get; }

		protected override void TearDown()
		{
			if (token != null)
			{
				token.Dispose();
			}

			if (TestBiConnection != null)
			{
				TestBiConnection.Dispose();
			}

			base.TearDown();
		}

		protected DbConnection TestBiConnection;
		ITransactionManager token;

		const string paramName = "TEST_PARAMETER";

		#endregion
	}

	class AuditBiMasterStateTest : BiMasterStateTest
	{
		protected override string DbName => Db.AuditDatabaseName;

		public void TestGetLastMaxLsnProcessedPeriodUtc()
		{
			var date = new DateTime(2024, 04, 1, 0, 0, 0);
			var expectedPeriod = 2404;
			TestBiConnection.ExecuteNonQuery($"INSERT INTO biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x05, '{date.ToString(CultureInfo.InvariantCulture)}')");
			using (BiTemporaryMasterState.SetParameterTemporaryValue(TestBiConnection, BiConstants.LastMaxLsnProcessed, "0x05"))
			{
				var result = BiMasterState.GetLastMaxLsnProcessedPeriod(TestBiConnection);
				AssertNotNull("Should get a valid period number", result);
				AssertEquals("GetLastMaxLsnProcessed should use the UTC time from LsnTimeMapping", expectedPeriod, result.Value);
			}
		}

		public void TestGetLastMaxLsnProcessedPeriodNull()
		{
			using (BiTemporaryMasterState.SetParameterTemporaryValue(TestBiConnection, BiConstants.LastMaxLsnProcessed, null))
			{
				var result = BiMasterState.GetLastMaxLsnProcessedPeriod(TestBiConnection);
				AssertNull("If LAST_MAX_LSN_TIME_PROCESSED is not set, should return null", result);
			}
		}
	}

	class EdwBiMasterStateTest : BiMasterStateTest
	{
		protected override string DbName => Db.EdwDatabaseName;
	}
}
