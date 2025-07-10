namespace CargoWise.Bi.GlobalMonitoring.Testing
{
	using System.Data;
	using CargoWise.Data;

	class BiDashboardTest : GlobalMonitoringTest
	{
		public void TestBiDashboard()
		{
			var dataSets = GetDataSets();

			CombineAssertions(() =>
			{
				foreach (var dataSetName in dataSets)
				{
					var dataSetQuery = GetDataSetQuery(dataSetName);
					RunBiDashboardTest(dataSetName, dataSetQuery);
				}
			});
		}

		void RunBiDashboardTest(string dataSetName, string dataSetQuery)
		{
			var expectedTableQuery = $"SELECT * FROM dbo.[{dataSetName}]";
			var expectedDataTable = DataUtils.GetDataTableFromQuery(TestAdminConnection, expectedTableQuery);

			using (var cmd = TestAdminConnection.Command(dataSetQuery))
			{
				cmd.AddParameter("@CurrentDateUtc", SqlDbType.DateTime, TestDateUtc);
				cmd.AddParameter("@CurrentDateLocal", SqlDbType.DateTime, TestDateLocal);

				var actualDataTable = DataUtils.GetDataTableFromCommand(cmd);
				CompareDataTables(dataSetName, expectedDataTable, actualDataTable);
			}
		}

		protected override string ReportName => "BI Dashboard";
	}
}
