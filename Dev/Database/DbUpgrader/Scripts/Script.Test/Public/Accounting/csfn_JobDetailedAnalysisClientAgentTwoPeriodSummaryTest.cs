using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(csfn_JobDetailedAnalysisClientAgentTwoPeriodSummary))]
	class csfn_JobDetailedAnalysisClientAgentTwoPeriodSummaryTest : DbCreateScriptTest
	{
		public void TestShipmentIsNotDuplicatedWhenAttachedToMultipleConsols()
		{
			var helper = new TestDbHelper(TestConnection);

			var shipment1PK = helper.InsertShipment("S001001", "SEA", "LCL", "AUBNE", "DEHAM", new DateTime(2017, 12, 1), new DateTime(2012, 12, 10), 1);
			var shipment2PK = helper.InsertShipment("S001002", "SEA", "LCL", "DEHAM", "AUBNE", new DateTime(2017, 12, 15), new DateTime(2018, 1, 10), 10);
			var shipment3PK = helper.InsertShipment("S001003", "SEA", "LCL", "AUBNE", "CNSHA", new DateTime(2018, 1, 15), new DateTime(2018, 1, 20), 100);

			var consol1PK = helper.InsertConsol("C001001", "AUBNE", "HKHKG");
			var consol2PK = helper.InsertConsol("C001002", "HKHKG", "DEHAM");
			var consol3PK = helper.InsertConsol("C001003", "DEHAM", "HKHKG");
			var consol4PK = helper.InsertConsol("C001004", "HKHKG", "AUBNE");

			var link1 = helper.InsertJobConShipLink(consol1PK, shipment1PK);
			var link2 = helper.InsertJobConShipLink(consol2PK, shipment1PK);
			var link3 = helper.InsertJobConShipLink(consol3PK, shipment2PK);
			var link4 = helper.InsertJobConShipLink(consol4PK, shipment2PK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_JobDetailedAnalysisClientAgentTwoPeriodSummary (null, 'AU', '2017-01-01', '2018-01-01', '2018-01-01', '2019-01-01', '', 'F', null, null, null, null, null, null, null, null, null, null, '', '', null, null, '', 'EXP', null)");
			AssertEquals("Export - expect 1 rows", 1, result.Rows.Count);
			AssertEquals("Export Period 1 record count", 1, (int)result.Rows[0]["JH_RecordCountLCLPeriod1"]);
			AssertEquals("Export Period 1 chargeable", 1m, (decimal)result.Rows[0]["JS_ActualChargeableLCLOTHPeriod1"]);
			AssertEquals("Export Period 2 record count", 1, (int)result.Rows[0]["JH_RecordCountLCLPeriod2"]);
			AssertEquals("Export Period 2 chargeable", 100m, (decimal)result.Rows[0]["JS_ActualChargeableLCLOTHPeriod2"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_JobDetailedAnalysisClientAgentTwoPeriodSummary (null, 'AU', '2017-01-01', '2018-01-01', '2018-01-01', '2019-01-01', '', 'F', null, null, null, null, null, null, null, null, null, null, '', '', null, null, '', 'IMP', null)");
			AssertEquals("Import - expect 1 rows", 1, result.Rows.Count);
			AssertEquals("Import Period 1 record count", DBNull.Value, result.Rows[0]["JH_RecordCountLCLPeriod1"]);
			AssertEquals("Import Period 1 chargeable", 0m, (decimal)result.Rows[0]["JS_ActualChargeableLCLOTHPeriod1"]);
			AssertEquals("Import Period 2 record count", 1, (int)result.Rows[0]["JH_RecordCountLCLPeriod2"]);
			AssertEquals("Import Period 2 chargeable", 10m, (decimal)result.Rows[0]["JS_ActualChargeableLCLOTHPeriod2"]);
		}
	}
}

