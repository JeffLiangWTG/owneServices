using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_EuNctsMovementSummaryPhase4))]
	class Report_EuNctsMovementSummaryPhase4Test : CustomsReportDbCreateScriptTest
	{
		#region Columns

		public void TestWeekKey()
		{
			var expectedWeekKey = "202411";

			AssertFunctionReturnExpectedValue("WeekKey", expectedWeekKey);

			var cusEntryNumCreateTime = new DateTime(2021, 12, 31);
			expectedWeekKey = "202153";

			UpdateCusEntryNumColumn("CE_SystemCreateTimeUTC", cusEntryNumCreateTime, SqlDbType.DateTime, cusEntryNumPK);
			AssertFunctionReturnExpectedValue("WeekKey", expectedWeekKey);
		}

		public void TestConsolKey()
		{
			var expectedConsoleKey = "202411";

			AssertFunctionReturnExpectedValue("ConsolKey", expectedConsoleKey);

			var cusEntryNumCreateTime = new DateTime(2021, 12, 31);
			expectedConsoleKey = "202153";

			UpdateCusEntryNumColumn("CE_SystemCreateTimeUTC", cusEntryNumCreateTime, SqlDbType.DateTime, cusEntryNumPK);
			AssertFunctionReturnExpectedValue("ConsolKey", expectedConsoleKey);

			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateCusInBondHeaderColumn("BH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, inBondHeaderPK);

			var consolPK = TestDataCreator.CreateJobConsol();
			TestDataCreator.CreateJobConShipLink(shipmentPK, consolPK);
			UpdateJobConsolColumn("JK_MasterBillNum", "1234", SqlDbType.VarChar, consolPK);
			expectedConsoleKey = "2021531234";
			AssertFunctionReturnExpectedValue("ConsolKey", expectedConsoleKey);
		}

		public void TestNctsReference()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "123", SqlDbType.VarChar, inBondHeaderPK);
			AssertFunctionReturnExpectedValue("NctsReference", "123");

			TestDataCreator.CreateGenAddOnColumn(inBondHeaderPK, "BH", "LocalReferenceNumber", "", "456");
			AssertFunctionReturnExpectedValue("NctsReference", "456");
		}

		public void TestReleaseDate()
		{
			var departmentPK = TestDataCreator.CreateDepartment("ABC");
			AssertFunctionReturnExpectedValue("ReleaseDate", createTime);

			var cusEntryNumCreateTime = new DateTime(2021, 12, 31);

			UpdateCusEntryNumColumn("CE_SystemCreateTimeUTC", cusEntryNumCreateTime, SqlDbType.DateTime, cusEntryNumPK);
			AssertFunctionReturnExpectedValue("ReleaseDate", cusEntryNumCreateTime);

			UpdateCusInBondMoveHeaderColumn("BM_EntryDate", new DateTime(2021, 01, 01), SqlDbType.DateTime, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("ReleaseDate", cusEntryNumCreateTime);

			TestDataCreator.CreateEDIMessage(branchPK, departmentPK, interchangePK: Guid.Empty, inBondHeaderPK, createTimeUTC: new DateTime(2021, 01, 02), status: "SNT", linkTable: "EDIMessage", messageSubType: "015");
			AssertFunctionReturnExpectedValue("ReleaseDate", cusEntryNumCreateTime);
		}

		public void TestDepartureMovementHeaderAggregatedFields()
		{
			UpdateCusInBondMoveHeaderColumn("BM_EntryDate", new DateTime(2021, 12, 31), SqlDbType.DateTime, inBondMoveHeaderPK);
			UpdateCusInBondMoveHeaderColumn("BM_CustomsStatus", "DRL", SqlDbType.VarChar, inBondMoveHeaderPK);

			var inBondMoveHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(inBondHeaderPK, "D");
			UpdateCusInBondMoveHeaderColumn("BM_EntryDate", new DateTime(2022, 03, 25), SqlDbType.DateTime, inBondMoveHeader2PK);
			UpdateCusInBondMoveHeaderColumn("BM_CustomsStatus", "AWO", SqlDbType.VarChar, inBondMoveHeader2PK);

			CombineAssertions("Assert CusInBondMoveHeader fields are consistent", () =>
			{
				AssertFunctionReturnExpectedValue("ReleaseDate", createTime);
				AssertFunctionReturnExpectedValue("NctsStatus", "AWO");
			});
		}

		public void TestMovementReferenceNumber()
		{
			AssertFunctionReturnExpectedValue("MovementReferenceNumber", "MRN001");

			UpdateCusEntryNumColumn("CE_EntryNum", "MRN999", SqlDbType.VarChar, cusEntryNumPK);

			AssertFunctionReturnExpectedValue("MovementReferenceNumber", "MRN999");
		}

		public void TestShipmentID()
		{
			AssertFunctionReturnExpectedValue("ShipmentID", DBNull.Value);

			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateJobShipmentColumn("JS_UniqueConsignRef", "1234", SqlDbType.VarChar, shipmentPK);
			UpdateCusInBondHeaderColumn("BH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, inBondHeaderPK);
			AssertFunctionReturnExpectedValue("ShipmentID", "1234");
		}

		public void TestContainerOrVehicleNoWithFallback()
		{
			AssertFunctionReturnExpectedValue("ContainerOrVehicleNo", DBNull.Value);

			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateCusInBondHeaderColumn("BH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, inBondHeaderPK);
			var consolPK = TestDataCreator.CreateJobConsol();
			TestDataCreator.CreateJobConShipLink(shipmentPK, consolPK);
			var jobConsolTransportPK = TestDataCreator.CreateJobConsolTransport(consolPK);
			UpdateJobConsolTransportColumn("JW_Vessel", "VES", SqlDbType.VarChar, jobConsolTransportPK);
			AssertFunctionReturnExpectedValue("ContainerOrVehicleNo", "VES");

			UpdateJobConsolTransportColumn("JW_VoyageFlight", "FLI", SqlDbType.VarChar, jobConsolTransportPK);
			AssertFunctionReturnExpectedValue("ContainerOrVehicleNo", "FLI");

			var jobPackLines = TestDataCreator.CreateJobPackLines(shipmentPK);
			var jobContainer1 = TestDataCreator.CreateJobContainer("CNT1", null);
			TestDataCreator.CreateJobContainerPackPivot(jobContainer1, jobPackLines);
			var jobContainer2 = TestDataCreator.CreateJobContainer("CNT2", null);
			TestDataCreator.CreateJobContainerPackPivot(jobContainer2, jobPackLines);
			AssertFunctionReturnExpectedValue("ContainerOrVehicleNo", "CNT1,CNT2");
		}

		public void TestJobConsolAggregatedFields()
		{
			CombineAssertions("Assertions when has no forwarding data", () =>
			{
				AssertFunctionReturnExpectedValue("ConsolID", DBNull.Value);
				AssertFunctionReturnExpectedValue("MasterBill", DBNull.Value);
				AssertFunctionReturnExpectedValue("ContainerOrVehicleNo", DBNull.Value);
			});

			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateJobShipmentColumn("JS_UniqueConsignRef", "1234", SqlDbType.VarChar, shipmentPK);
			UpdateCusInBondHeaderColumn("BH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, inBondHeaderPK);

			var consolPK = TestDataCreator.CreateJobConsol();
			var jobConShipLinkPK = TestDataCreator.CreateJobConShipLink(shipmentPK, consolPK);
			UpdateJobConsolColumn("JK_UniqueConsignRef", "AAA", SqlDbType.VarChar, consolPK);
			UpdateJobConsolColumn("JK_MasterBillNum", "BBB", SqlDbType.VarChar, consolPK);
			var jobConsolTransportPK = TestDataCreator.CreateJobConsolTransport(consolPK);
			UpdateJobConsolTransportColumn("JW_VoyageFlight", "CCC", SqlDbType.VarChar, jobConsolTransportPK);
			CombineAssertions("Assertions when has only one JobConsol and one JobConsolTransport", () =>
			{
				AssertFunctionReturnExpectedValue("ConsolID", "AAA");
				AssertFunctionReturnExpectedValue("MasterBill", "BBB");
				AssertFunctionReturnExpectedValue("ContainerOrVehicleNo", "CCC");
			});

			var consol2PK = TestDataCreator.CreateJobConsol();
			UpdateJobConsolColumn("JK_UniqueConsignRef", "DDD", SqlDbType.VarChar, consol2PK);
			UpdateJobConsolColumn("JK_MasterBillNum", "EEE", SqlDbType.VarChar, consol2PK);
			var jobConShipLink2PK = TestDataCreator.CreateJobConShipLink(shipmentPK, consol2PK);
			var jobConsolTransport2PK = TestDataCreator.CreateJobConsolTransport(consol2PK);
			UpdateJobConsolTransportColumn("JW_Vessel", "FFF", SqlDbType.VarChar, jobConsolTransport2PK);
			var jobConsolTransport3PK = TestDataCreator.CreateJobConsolTransport(consol2PK);
			UpdateJobConsolTransportColumn("JW_Vessel", "GGG", SqlDbType.VarChar, jobConsolTransport3PK);
			CombineAssertions("Assertions when has more than one JobConsol and more than one JobConsolTransport", () =>
			{
				AssertFunctionReturnExpectedValue("ConsolID", "DDD");
				AssertFunctionReturnExpectedValue("MasterBill", "EEE");
				AssertFunctionReturnExpectedValue("ContainerOrVehicleNo", "GGG");
			});
		}

		public void TestConsignorAggregatedFields()
		{
			var consignorPK = TestDataCreator.CreateOrganisation("OR_B", new string('1', 21), orgHeaderPK: new Guid("11111111-1111-1111-1111-111111111111"));
			var consignorAddressPK = TestDataCreator.CreateAddress(consignorPK, "AD", "AD");
			TestDataCreator.CreateDocAddress(consignorAddressPK, "ITA", inBondHeaderPK, "BH", "CRD", 0);

			var consignor1PK = TestDataCreator.CreateOrganisation("OR_A", new string('0', 21), orgHeaderPK: new Guid("22222222-2222-2222-2222-222222222222"));
			var consignorAddress1PK = TestDataCreator.CreateAddress(consignor1PK, "AD", "AD");
			TestDataCreator.CreateDocAddress(consignorAddress1PK, "ITA", inBondHeaderPK, "BH", "CRD", 1);

			CombineAssertions("Assert Consignor JobDocAddress fields are consistent", () =>
			{
				AssertFunctionReturnExpectedValue("ConsignorCode", "OR_A");
				AssertFunctionReturnExpectedValue("ConsignorShortDescription", new string('0', 20));
			});
		}

		public void TestConsigneeAggregatedFields()
		{
			var consigneePK = TestDataCreator.CreateOrganisation("OR_B", new string('1', 21), orgHeaderPK: new Guid("11111111-1111-1111-1111-111111111111"));
			var consigneeAddressPK = TestDataCreator.CreateAddress(consigneePK, "AD", "AD");
			TestDataCreator.CreateDocAddress(consigneeAddressPK, "ITA", inBondHeaderPK, "BH", "CEA", 0);

			var consignee1PK = TestDataCreator.CreateOrganisation("OR_A", new string('0', 21), orgHeaderPK: new Guid("22222222-2222-2222-2222-222222222222"));
			var consigneeAddress1PK = TestDataCreator.CreateAddress(consignee1PK, "AD", "AD");
			TestDataCreator.CreateDocAddress(consigneeAddress1PK, "ITA", inBondHeaderPK, "BH", "CEA", 1);

			CombineAssertions("Assert Consignee JobDocAddress fields are consistent", () =>
			{
				AssertFunctionReturnExpectedValue("ConsigneeCode", "OR_A");
				AssertFunctionReturnExpectedValue("ConsigneeShortDescription", new string('0', 20));
			});
		}

		public void TestLiabilityAmount()
		{
			TestDataCreator.CreateCusBondDetail(inBondHeaderPK, "BH", "1234", 123m);
			TestDataCreator.CreateCusBondDetail(inBondHeaderPK, "BH", "1234", 456m);
			AssertFunctionReturnExpectedValue("LiabilityAmount", 579m);
		}

		public void TestGoodsDetail()
		{
			var goods1PK = TestDataCreator.CreateCusInBondCargoDesc(inBondMoveHeaderPK, "BM", "GOODS", "NEW", 150, 200, "8007001000");
			UpdateCusInBondCargoDescColumn("BY_MonetaryValue", 3500.00m, SqlDbType.Money, goods1PK);
			AssertFunctionReturnExpectedValue("GoodsDetail", "8007001000, GOODS, 3500.00");

			var goods2PK = TestDataCreator.CreateCusInBondCargoDesc(inBondMoveHeaderPK, "BM", "FOOD", "NEW", 150, 200);
			UpdateCusInBondCargoDescColumn("BY_MonetaryValue", 45678.44m, SqlDbType.Money, goods2PK);
			AssertFunctionReturnExpectedValue("GoodsDetail", "8007001000, GOODS, 3500.00; FOOD, 45678.44");

			var goods3PK = TestDataCreator.CreateCusInBondCargoDesc(inBondMoveHeaderPK, "BM", "", "NEW", 150, 200, "59021010");
			UpdateCusInBondCargoDescColumn("BY_MonetaryValue", 0.00m, SqlDbType.Money, goods3PK);
			AssertFunctionReturnExpectedValue("GoodsDetail", "8007001000, GOODS, 3500.00; FOOD, 45678.44; 59021010, 0.00");

			var goods4PK = TestDataCreator.CreateCusInBondCargoDesc(inBondMoveHeaderPK, "BM", "", "NEW", 150, 200, "5902101000");
			UpdateCusInBondCargoDescColumn("BY_MonetaryValue", 0.00m, SqlDbType.Money, goods4PK);
			AssertFunctionReturnExpectedValue("GoodsDetail", "8007001000, GOODS, 3500.00; FOOD, 45678.44; 59021010, 0.00; 5902101000, 0.00");

			var goods5PK = TestDataCreator.CreateCusInBondCargoDesc(inBondMoveHeaderPK, "BM", "", "NEW", 150, 200);
			UpdateCusInBondCargoDescColumn("BY_MonetaryValue", 0.00m, SqlDbType.Money, goods5PK);
			AssertFunctionReturnExpectedValue("GoodsDetail", "8007001000, GOODS, 3500.00; FOOD, 45678.44; 59021010, 0.00; 5902101000, 0.00; 0.00");

			var goods6PK = TestDataCreator.CreateCusInBondCargoDesc(inBondMoveHeaderPK, "BM", "HOUSEHOLD ARTICLES - PERSONAL BELONGINGSPM=Marks PN=149 PC=PK", "NEW", 150, 200, "4821909000");
			UpdateCusInBondCargoDescColumn("BY_MonetaryValue", 1200.00m, SqlDbType.Money, goods6PK);
			AssertFunctionReturnExpectedValue("GoodsDetail", "8007001000, GOODS, 3500.00; FOOD, 45678.44; 59021010, 0.00; 5902101000, 0.00; 0.00; 4821909000, HOUSEHOLD ARTICLES - PERSONAL BELONGINGSPM=Marks PN=149 PC=PK, 1200.00");

			var goods7PK = TestDataCreator.CreateCusInBondCargoDesc(inBondMoveHeaderPK, "BM", new string('D', 512), "NEW", 150, 200, "4821909001");
			UpdateCusInBondCargoDescColumn("BY_MonetaryValue", 1300.00m, SqlDbType.Money, goods7PK);
			var filteredRows = GetFilteredRows(selectedColumn: "GoodsDetail");
			var row = filteredRows.Single();
			AssertEquals("Description of Length 512 NOT FOUND", row.ToString().IndexOf(new string('D', 512)), -1);
			AssertNotEquals("Description of Length 100 FOUND", row.ToString().IndexOf(new string('D', 100)), -1);
		}

		public void TestDuty()
		{
			var by_PK = CreateCusInBondCargoDesc(inBondMoveHeaderPK);
			_ = TestDataCreator.CreateCusInbondFee(by_PK, isValid: true, 2000m, "%", 0.16m, 320m, "", "", "DTY");
			AssertFunctionReturnExpectedValue("Duty", 320m);
		}

		public void TestAntiDumpingDuty()
		{
			var by_PK = CreateCusInBondCargoDesc(inBondMoveHeaderPK);
			_ = TestDataCreator.CreateCusInbondFee(by_PK, isValid: false, 2320m, "", 0.20m, 465m, "", "", "ADD");
			AssertFunctionReturnExpectedValue("AntiDumpingDuty", 465m);
		}

		public void TestCountervailingDuty()
		{
			var by_PK = CreateCusInBondCargoDesc(inBondMoveHeaderPK);
			_ = TestDataCreator.CreateCusInbondFee(by_PK, isValid: true, 2600m, "", 0.25m, 650m, "", "", "CVD");
			AssertFunctionReturnExpectedValue("CountervailingDuty", 650m);
		}

		public void TestValueAddedTax()
		{
			var by_PK = CreateCusInBondCargoDesc(inBondMoveHeaderPK);
			_ = TestDataCreator.CreateCusInbondFee(by_PK, isValid: false, 5000m, "%", 0.20m, 1000m, "", "", "VAT");
			AssertFunctionReturnExpectedValue("ValueAddedTax", 1000m);
		}

		public void TestExcise()
		{
			var by_PK = CreateCusInBondCargoDesc(inBondMoveHeaderPK);
			_ = TestDataCreator.CreateCusInbondFee(by_PK, isValid: true, 200m, "%", 0.5m, 100m, "", "", "EXC");
			AssertFunctionReturnExpectedValue("Excise", 100m);
		}

		#endregion

		#region Filters

		public void TestFilterByReleaseDateRange()
		{
			UpdateCusEntryNumColumn("CE_SystemCreateTimeUTC", new DateTime(2020, 01, 01), SqlDbType.DateTime, cusEntryNumPK);

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			var cusEntryNum2PL = TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			_ = TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");

			UpdateCusEntryNumColumn("CE_SystemCreateTimeUTC", new DateTime(2020, 01, 10), SqlDbType.DateTime, cusEntryNum2PL);

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DateFrom, new DateTime(2020, 01, 01)), (Report_EuNctsMovementSummaryParameters.DateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DateFrom, new DateTime(2020, 01, 05)), (Report_EuNctsMovementSummaryParameters.DateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);
		}

		public void TestFilterByPrincipalPK()
		{
			var principalPK = TestDataCreator.CreateOrganisation("OR1", "OR1");
			var principalAddressPK = TestDataCreator.CreateAddress(principalPK, "AD1", "AD1");
			TestDataCreator.CreateDocAddress(principalAddressPK, "ITA", inBondHeaderPK, "BH", "PRC");

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			var principal2PK = TestDataCreator.CreateOrganisation("OR2", "OR2");
			var principalAddress2PK = TestDataCreator.CreateAddress(principal2PK, "AD2", "AD2");
			TestDataCreator.CreateDocAddress(principalAddress2PK, "ITA", inBondHeader2PK, "BH", "PRC");

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.PrincipalPK, principalPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.PrincipalPK, principal2PK));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);
		}

		public void TestFilterByGuaranteeNumber()
		{
			TestDataCreator.CreateCusBondDetail(inBondHeaderPK, "BH", "1234");
			TestDataCreator.CreateCusBondDetail(inBondHeaderPK, "BH", "1234");
			TestDataCreator.CreateCusBondDetail(inBondHeaderPK, "BH", "0000");

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			TestDataCreator.CreateCusBondDetail(inBondHeader2PK, "BH", "5678");

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.GuaranteeNumber, "1234"));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.GuaranteeNumber, "0000"));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.GuaranteeNumber, "5678"));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);
		}

		public void TestFilterByDepartureOfficeCode()
		{
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT000000", inBondHeaderPK, "BH");
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT000000", inBondHeaderPK, "BH");
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT111111", inBondHeaderPK, "BH");

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT222222", inBondHeader2PK, "BH");

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DepartureOfficeCode, "IT000000"));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DepartureOfficeCode, "IT111111"));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DepartureOfficeCode, "IT222222"));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);
		}

		public void TestFilterByDestinationOfficeCode()
		{
			TestDataCreator.CreateCusCodeData("EUO", "DES", "IT000000", inBondHeaderPK, "BH");
			TestDataCreator.CreateCusCodeData("EUO", "DES", "IT000000", inBondHeaderPK, "BH");
			TestDataCreator.CreateCusCodeData("EUO", "DES", "IT111111", inBondHeaderPK, "BH");

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			TestDataCreator.CreateCusCodeData("EUO", "DES", "IT222222", inBondHeader2PK, "BH");

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DestinationOfficeCode, "IT000000"));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DestinationOfficeCode, "IT111111"));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DestinationOfficeCode, "IT222222"));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);
		}

		public void TestFilterByConsignorPK()
		{
			var consignorPK = TestDataCreator.CreateOrganisation("OR1", "OR1");
			var consignorAddressPK = TestDataCreator.CreateAddress(consignorPK, "AD1", "AD1");
			TestDataCreator.CreateDocAddress(consignorAddressPK, "ITA", inBondHeaderPK, "BH", "CRD");

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			var consignor2PK = TestDataCreator.CreateOrganisation("OR2", "OR2");
			var consignorAddress2PK = TestDataCreator.CreateAddress(consignor2PK, "AD2", "AD2");
			TestDataCreator.CreateDocAddress(consignorAddress2PK, "ITA", inBondHeader2PK, "BH", "CRD");

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.ConsignorPK, consignorPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.ConsignorPK, consignor2PK));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);
		}

		public void TestFilterByConsigneePK()
		{
			var consigneePK = TestDataCreator.CreateOrganisation("OR1", "OR1");
			var consigneeAddressPK = TestDataCreator.CreateAddress(consigneePK, "AD1", "AD1");
			TestDataCreator.CreateDocAddress(consigneeAddressPK, "ITA", inBondHeaderPK, "BH", "CEA");

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			var consignee2PK = TestDataCreator.CreateOrganisation("OR2", "OR2");
			var consigneeAddress2PK = TestDataCreator.CreateAddress(consignee2PK, "AD2", "AD2");
			TestDataCreator.CreateDocAddress(consigneeAddress2PK, "ITA", inBondHeader2PK, "BH", "CEA");

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.ConsigneePK, consigneePK));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.ConsigneePK, consignee2PK));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);
		}

		public void TestFilterByDeclarationType()
		{
			UpdateCusInBondMoveHeaderColumn("BM_InBondEntryType", "T1", SqlDbType.VarChar, inBondMoveHeaderPK);

			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			var inBondMoveHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			UpdateCusInBondMoveHeaderColumn("BM_EntryDate", new DateTime(2020, 01, 10), SqlDbType.DateTime, inBondMoveHeader2PK);
			UpdateCusInBondMoveHeaderColumn("BM_InBondEntryType", "T2", SqlDbType.VarChar, inBondMoveHeader2PK);
			var inBondMoveHeader3PK = TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");
			UpdateCusInBondMoveHeaderColumn("BM_EntryDate", new DateTime(2020, 01, 09), SqlDbType.DateTime, inBondMoveHeader3PK);
			UpdateCusInBondMoveHeaderColumn("BM_InBondEntryType", "T3", SqlDbType.VarChar, inBondMoveHeader3PK);

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DeclarationType, "T1"));
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "NctsReference", (Report_EuNctsMovementSummaryParameters.DeclarationType, "T2"));
			AssertContainsExactElementsInAnyOrder(new string[] { "002" }, filteredRows);
		}

		public void TestFilterByMrn()
		{
			var inBondHeader2PK = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NCT");
			TestDataCreator.CreateCusInBondMoveHeader(inBondHeader2PK, "D", "AWO");

			var filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001" }, filteredRows);

			TestDataCreator.CreateCusEntryNum(inBondHeader2PK, "CusInBondHeader", "MRN002", "MRN", "CUS", "");
			filteredRows = GetFilteredRows(selectedColumn: "NctsReference");
			AssertContainsExactElementsInAnyOrder(new string[] { "001", "002" }, filteredRows);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "ITA", countryCode: "IT", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "ITA", homePort: "ITALY");
			inBondHeaderPK = TestDataCreator.CreateCusInbondHeader("001", branchPK, "NCT");
			createTime = new DateTime(2024, 03, 14).ToSmallDateTimeFloor();
			cusEntryNumPK = TestDataCreator.CreateCusEntryNum(inBondHeaderPK, "CusInBondHeader", "MRN001", "MRN", "CUS", "", createTime: createTime);
			inBondMoveHeaderPK = TestDataCreator.CreateCusInBondMoveHeader(inBondHeaderPK, "D", "AWO");
		}

		Guid companyPK;
		Guid branchPK;
		Guid inBondHeaderPK;
		Guid inBondMoveHeaderPK;
		Guid cusEntryNumPK;
		DateTime createTime;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_EuNctsMovementSummaryParameters.CurrentCompany);
			yield return (SqlDbType.SmallDateTime, Report_EuNctsMovementSummaryParameters.DateFrom);
			yield return (SqlDbType.SmallDateTime, Report_EuNctsMovementSummaryParameters.DateTo);
			yield return (SqlDbType.UniqueIdentifier, Report_EuNctsMovementSummaryParameters.PrincipalPK);
			yield return (SqlDbType.VarChar, Report_EuNctsMovementSummaryParameters.GuaranteeNumber);
			yield return (SqlDbType.VarChar, Report_EuNctsMovementSummaryParameters.DepartureOfficeCode);
			yield return (SqlDbType.VarChar, Report_EuNctsMovementSummaryParameters.DestinationOfficeCode);
			yield return (SqlDbType.UniqueIdentifier, Report_EuNctsMovementSummaryParameters.ConsignorPK);
			yield return (SqlDbType.UniqueIdentifier, Report_EuNctsMovementSummaryParameters.ConsigneePK);
			yield return (SqlDbType.VarChar, Report_EuNctsMovementSummaryParameters.DeclarationType);
		}

		class Report_EuNctsMovementSummaryParameters
		{
			public const string CurrentCompany = "@CurrentCompany";
			public const string DateFrom = "@DateFrom";
			public const string DateTo = "@DateTo";
			public const string PrincipalPK = "@PrincipalPK";
			public const string GuaranteeNumber = "@GuaranteeNumber";
			public const string DepartureOfficeCode = "@DepartureOfficeCode";
			public const string DestinationOfficeCode = "@DestinationOfficeCode";
			public const string ConsignorPK = "@ConsignorPK";
			public const string ConsigneePK = "@ConsigneePK";
			public const string DeclarationType = "@DeclarationType";
		}

		void UpdateCusInBondMoveHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondMoveHeader", columnName, columnValue, columnType, "BM_PK", primaryKeyValue);
		}

		void UpdateCusInBondHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondHeader", columnName, columnValue, columnType, "BH_PK", primaryKeyValue);
		}

		void UpdateJobShipmentColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobShipment", columnName, columnValue, columnType, "JS_PK", primaryKeyValue);
		}

		void UpdateJobConsolColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobConsol", columnName, columnValue, columnType, "JK_PK", primaryKeyValue);
		}

		void UpdateJobConsolTransportColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobConsolTransport", columnName, columnValue, columnType, "JW_PK", primaryKeyValue);
		}

		void UpdateCusEntryNumColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryNum", columnName, columnValue, columnType, "CE_PK", primaryKeyValue);
		}

		void UpdateCusInBondCargoDescColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondCargoDesc", columnName, columnValue, columnType, "BY_PK", primaryKeyValue);
		}

		Guid CreateCusInBondCargoDesc(Guid parentID)
		{
			return TestDataCreator.CreateCusInBondCargoDesc(parentID, "B0", "desc123", "NEW", 150, 200);
		}

		#endregion
	}
}
