namespace CargoWise.Bi.GlobalMonitoring.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using CargoWise.Data;

	class AuditDatabasesDetailsTest : GlobalMonitoringTest
	{
		public void TestAuditDatabasesDetails()
		{
			var dataSetQuery = GetDataSetQuery(DataSetName);
			var metricDataDictionary = GetMetricDataDictionary();

			CombineAssertions(() =>
			{
				foreach (var metricData in metricDataDictionary)
				{
					RunAuditDatabasesDetailsTest(dataSetQuery, metricData);
				}
			});
		}

		void RunAuditDatabasesDetailsTest(string dataSetQuery, KeyValuePair<int, string> metricData)
		{
			var dataSetName = $"{DataSetName}_{metricData.Key}";

			var expectedTableQuery = $"SELECT * FROM dbo.[{dataSetName}]";
			var expectedDataTable = DataUtils.GetDataTableFromQuery(TestAdminConnection, expectedTableQuery);

			using (var cmd = TestAdminConnection.Command(dataSetQuery))
			{
				cmd.AddParameter("@CurrentDateUtc", SqlDbType.DateTime, TestDateUtc);
				cmd.AddParameter("@CurrentDateLocal", SqlDbType.DateTime, TestDateLocal);

				cmd.AddParameter("@MetricId", SqlDbType.Int, metricData.Key);
				cmd.AddParameter("@SupValue1Int", SqlDbType.Int, DBNull.Value);
				cmd.AddParameter("@SupValue2String", SqlDbType.VarChar, 128, DBNull.Value);

				var actualDataTable = DataUtils.GetDataTableFromCommand(cmd);
				var description = $"{DataSetName}, {metricData.Key} - {metricData.Value}";
				CompareDataTables(description, expectedDataTable, actualDataTable);
			}
		}

		Dictionary<int, string> GetMetricDataDictionary()
		{
			var result = new Dictionary<int, string>();

			var cmdText = GetDataSetQuery("MetricDataSet");
			using (var cmd = TestAdminConnection.Command(cmdText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var label = reader["Label"].ToString();
					var value = Convert.ToInt32(reader["Value"], CultureInfo.InvariantCulture);

					result[value] = label;
				}
			}

			return result;
		}

		const string DataSetName = "OverallAuditDataSet";

		protected override string ReportName => "Audit Databases Details";
	}
}
