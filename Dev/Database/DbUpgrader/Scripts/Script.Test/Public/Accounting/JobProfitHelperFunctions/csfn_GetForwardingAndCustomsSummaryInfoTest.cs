using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.JobProfitHelperFunctions;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.JobProfitHelperFunctions.Testing
{
	[TestedType(typeof(csfn_GetForwardingAndCustomsSummaryInfo))]
	class csfn_GetForwardingAndCustomsSummaryInfoTest : DbCreateScriptTest
	{
		public void TestQueriesWithDatesFilter()
		{
			var helper = new TestDbHelper(TestConnection);
			var shipment1PK = helper.InsertShipment("S001001", "SEA", "LCL", "AUBNE", "DEHAM", new DateTime(2017, 12, 1), new DateTime(2017, 12, 10), 1);
			var shipment2PK = helper.InsertShipment("S001002", "SEA", "LCL", "DEHAM", "AUBNE", new DateTime(2017, 12, 15), new DateTime(2018, 1, 10), 10);
			var shipment3PK = helper.InsertShipment("S001003", "SEA", "LCL", "AUBNE", "CNSHA", new DateTime(2018, 1, 15), new DateTime(2018, 1, 20), 100);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_GetForwardingAndCustomsSummaryInfo(NULL, NULL, '1900-01-01 00:00:00', '2079-06-06 23:59:29', '1900-01-01 00:00:00', '2079-06-06 23:59:29', '', '', NULL, NULL, NULL, NULL, '1900-01-01 00:00:00', '2079-06-06 23:59:29', '1900-01-01 00:00:00', '2079-06-06 23:59:29', 'AU')");
			AssertEquals("Export with Origin ETD [unrestricted], Destination ETA [unrestricted] - expect 3 rows", 3, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_GetForwardingAndCustomsSummaryInfo(NULL, NULL, '2018-01-01 00:00:00', '2019-06-06 23:59:29', '2018-01-01 00:00:00', '2019-06-06 23:59:29', '', '', NULL, NULL, NULL, NULL, '1900-01-01 00:00:00', '2079-06-06 23:59:29', '1900-01-01 00:00:00', '2079-06-06 23:59:29', 'AU')");
			AssertEquals("Export Origin ETD [Restricted], Destination ETA [Restricted] - expect 1 rows", 1, result.Rows.Count);
			AssertEquals(shipment3PK, result.Rows[0]["JH_ParentID"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_GetForwardingAndCustomsSummaryInfo(NULL, NULL, '2018-01-01 00:00:00', '2019-06-06 23:59:29', '1900-01-01 00:00:00', '2079-06-06 23:59:29', '', '', NULL, NULL, NULL, NULL, '1900-01-01 00:00:00', '2079-06-06 23:59:29', '1900-01-01 00:00:00', '2079-06-06 23:59:29', 'AU')");
			AssertEquals("Export Origin ETD [Restricted], Destination ETA [unrestricted] - expect 1 rows", 1, result.Rows.Count);
			AssertEquals(shipment3PK, result.Rows[0]["JH_ParentID"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_GetForwardingAndCustomsSummaryInfo(NULL, NULL, '1900-01-01 00:00:00', '2079-06-06 23:59:29', '2018-01-01 00:00:00', '2019-06-06 23:59:29', '', '', NULL, NULL, NULL, NULL, '1900-01-01 00:00:00', '2079-06-06 23:59:29', '1900-01-01 00:00:00', '2079-06-06 23:59:29', 'AU')");
			AssertEquals("Export Origin ETD [unrestricted], Destination ETA [Restricted] - expect 2 rows", 2, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2PK, shipment3PK }, result.Rows.Cast<DataRow>().Select(x => x["JH_ParentID"]));
		}
	}
}
