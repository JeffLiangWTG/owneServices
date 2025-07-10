namespace CargoWise.Bi.GlobalMonitoring.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using CargoWise.Data;

	class OltpDatabasesDetailsTest : GlobalMonitoringTest
	{
		public void TestOltpDatabasesDetails()
		{
			var dataSetQuery = GetDataSetQuery(DataSetName);
			var metricDataDictionary = GetValidParameterValues("MetricId");

			CombineAssertions(() =>
			{
				foreach (var metricData in metricDataDictionary)
				{
					RunOltpDatabasesDetailsTest(dataSetQuery, metricData);
				}
			});
		}

		void RunOltpDatabasesDetailsTest(string dataSetQuery, KeyValuePair<int, string> metricData)
		{
			var dataSetName = $"{DataSetName}_{metricData.Key}";

			var expectedTableQuery = $"SELECT * FROM dbo.[{dataSetName}]";
			var expectedDataTable = DataUtils.GetDataTableFromQuery(TestAdminConnection, expectedTableQuery);

			using (var cmd = TestAdminConnection.Command(dataSetQuery))
			{
				cmd.AddParameter("@CurrentDateUtc", SqlDbType.DateTime, TestDateUtc);
				cmd.AddParameter("@CurrentDateLocal", SqlDbType.DateTime, TestDateLocal);

				cmd.AddParameter("@MetricId", SqlDbType.Int, metricData.Key);
				cmd.AddParameter("@SupValue1String", SqlDbType.VarChar, 128, DBNull.Value);
				cmd.AddParameter("@SupValue2Int", SqlDbType.Int, DBNull.Value);

				var actualDataTable = DataUtils.GetDataTableFromCommand(cmd);
				var description = $"{DataSetName}, {metricData.Key} - {metricData.Value}";
				CompareDataTables(description, expectedDataTable, actualDataTable);
			}
		}

		const string DataSetName = "OLTPDetailsDataSet";

		protected override string ReportName => "OLTP Databases Details";
	}
}
