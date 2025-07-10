using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(fn_GbCcsuk_ShowOutTurnDetailsInline))]
	class fn_GbCcsuk_ShowOutTurnDetailsInlineTest : DbCreateScriptTest
	{
		public void TestShowOutTurnDetailsInline_Logs()
		{
			var parentId = Guid.NewGuid();
			var outturnId = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($"INSERT dbo.CusOutTurn (C5_PK, C5_ParentID, C5_ReceiptOnlyIndicator) VALUES ('{outturnId}', '{parentId}', 1)");
			TestConnection.ExecuteNonQuery($"INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_Reference, SL_EventTime, SL_SE_NKEvent, SL_IsCancelled) VALUES (newid(), 'CusOutTurn', '{outturnId}', 'something1', '2023-07-01 09:42', 'DLV', 'N')");
			TestConnection.ExecuteNonQuery($"INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_Reference, SL_EventTime, SL_SE_NKEvent, SL_IsCancelled) VALUES (newid(), 'CusOutTurn', '{outturnId}', 'something2', '2023-07-01 09:43', 'DLV', 'N')");

			RunReportAndAssertGoodsReceivedData(parentId, "something1 on 1/7 9:42\r\nsomething2 on 1/7 9:43");
		}

		public void TestShowOutTurnDetailsInline_Warehouse()
		{
			var parentId = Guid.NewGuid();
			var outturnId = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"INSERT dbo.CusOutTurn (C5_PK, C5_ParentID, C5_PackagesOutturned, C5_PackagesUnits, C5_MarksAndNumbers) VALUES ('{outturnId}', '{parentId}', 3, 'PKG', 'XYZ0')");

			var companyPK = TestDataCreator.CreateCompany("GB1", "GB", "GBP");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "GBFLX");
			var warehouseOrgPK = TestDataCreator.CreateOrganisation("GB2", "TESTGBWHS", "GBFLX");
			var warehouseAddressPK = TestDataCreator.CreateAddress(warehouseOrgPK, "Address", "1 ROAD");
			var warehouse = new WhsWarehouse("WHS", branchPK, warehouseAddressPK).WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA 51").InsertAndReturnObject(TestConnection);
			var whsRowPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.WhsRow(WR_PK, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_WW_Whs, WR_PickPathSequence, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditTimeUtc, WR_SystemLastEditUser)
VALUES ('{whsRowPK}', 'INCFTZ', 10, 2, 1, '{warehouse.PK}', 2, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')");
			var whsLocationTypePK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.WhsLocationType (WLT_PK, WLT_Code, WLT_Description, WLT_LocationClass, WLT_DefaultCycleCountGranularity, WLT_SystemCreateTimeUtc, WLT_SystemCreateUser, WLT_SystemLastEditTimeUtc, WLT_SystemLastEditUser)
VALUES ('{whsLocationTypePK}', 'BND', 'BOND PICKUP', 'DDL', '', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')");
			var whsLocationPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.WhsLocation(WL_PK, WL_WA_PickingArea, WL_WR, WL_WLT_LocationType, WL_LocationStatus, WL_WA_PutawayArea, WL_PutawayPathSequence, WL_SystemCreateTimeUtc, WL_SystemCreateUser, WL_SystemLastEditTimeUtc, WL_SystemLastEditUser, WL_Column, WL_Level)
VALUES ('{whsLocationPK}', '{area.PK}', '{whsRowPK}', '{whsLocationTypePK}', 'NOR', '{area.PK}', 7, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 4, 2)");
			TestConnection.ExecuteNonQuery($"INSERT dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation2ID) VALUES (newid(), '{outturnId}', '{whsLocationPK}')");

			RunReportAndAssertGoodsReceivedData(parentId, "3 PKG stored in [INCFTZ-4-2], [XYZ0]");
		}

		void RunReportAndAssertGoodsReceivedData(Guid parentId, string expectedData)
		{
			using (var command = Db.Connection.Command($@"SELECT * FROM fn_GbCcsuk_ShowOutTurnDetailsInline('{parentId}', null)"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Result not empty", expected: true, reader.Read());
						AssertEquals("GoodsReceivedData", expectedData, reader["GoodsReceivedData"]);
						AssertEquals("Result single row", expected: false, reader.Read());
					});
				}
			}
		}
	}
}
