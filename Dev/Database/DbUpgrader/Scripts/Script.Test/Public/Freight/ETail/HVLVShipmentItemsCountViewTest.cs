using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using Enterprise.Build.Database.Script.Public.Freight.ETail.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Test
{
	[TestedType(typeof(HVLVShipmentItemsCountView))]
	class HVLVShipmentItemsCountViewTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => true;

		public void TestViewColumns()
		{
			var connection = Db.Connection;
			TestDbViewHelper.AssertViewColumns(
				connection,
				"HVLVShipmentItemsCountView", "",
				new[]
				{
					new TestDbViewHelper.DbColumn("HSC_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("HSC_ItemCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ImportClearedCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ImportHeldCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ImportNoneReportedCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ExportClearedCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ExportHeldCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ExportNoneReportedCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_SurplusCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ScannedCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ScannedClearedCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ScannedHeldCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ScannedNoneReportedCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_ShortCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("HSC_DeliveredCount", Int, -1, 10, 0),
				}
			);
		}

		public void TestLoad()
		{
			var shipmentPK = TestDataCreator.CreateShipment("SHP000001");

			helper.CreateHVLVItem("HVI0001", "HVC001", "HVH001", 111, DBNull.Value, shipmentPK, "SHP", "C", "C", false);
			helper.CreateHVLVItem("HVI0002", "HVC002", "HVH002", 222, DBNull.Value, shipmentPK, "SHP", "H", "H", false);
			helper.CreateHVLVItem("HVI0003", "HVC003", "HVH003", 333, DBNull.Value, shipmentPK, "SHP", "N", "N", false);
			helper.CreateHVLVItem("HVI0004", "HVC004", "HVH004", 444, DBNull.Value, shipmentPK, "SUD", "C", "C", true);
			helper.CreateHVLVItem("HVI0005", "HVC005", "HVH005", 555, DBNull.Value, shipmentPK, "SHP", "C", "C", true);
			helper.CreateHVLVItem("HVI0006", "HVC006", "HVH006", 666, DBNull.Value, shipmentPK, "SHP", "C", "C", true);
			helper.CreateHVLVItem("HVI0007", "HVC007", "HVH007", 777, DBNull.Value, shipmentPK, "SHP", "H", "H", true);
			helper.CreateHVLVItem("HVI0008", "HVC008", "HVH008", 888, DBNull.Value, shipmentPK, "SHP", "N", "N", true);
			helper.CreateHVLVItem("HVI0009", "HVC009", "HVH009", 999, DBNull.Value, shipmentPK, "SSD", "N", "N", true);
			helper.CreateHVLVItem("HVI0010", "HVC010", "HVH010", 010, DBNull.Value, shipmentPK, "DLV", "N", "N", true);

			var connection = Db.Connection;
			var expected = new List<(Guid pk, int itemCount, int importClearedCount, int importHeldCount, int importNoneReportedCount, int exportClearedCount, int exportHeldCount, int exportNoneReportedCount, int surplusCount, int shortCount, int deliveredCount, int scannedCount, int scannedClearedCount, int scannedHeldCount, int scannedNoneReportedCount)>
			{
				(shipmentPK, 10, 4, 2, 4, 4, 2, 4, 1, 1, 1, 7, 3, 1, 3)
			};
			connection.ExecuteReader("SELECT * FROM dbo.HVLVShipmentItemsCountView", reader =>
			AssertCollectionContains((
				(Guid)reader["HSC_PK"],
				(int)reader["HSC_ItemCount"],
				(int)reader["HSC_ImportClearedCount"],
				(int)reader["HSC_ImportHeldCount"],
				(int)reader["HSC_ImportNoneReportedCount"],
				(int)reader["HSC_ExportClearedCount"],
				(int)reader["HSC_ExportHeldCount"],
				(int)reader["HSC_ExportNoneReportedCount"],
				(int)reader["HSC_SurplusCount"],
				(int)reader["HSC_ShortCount"],
				(int)reader["HSC_DeliveredCount"],
				(int)reader["HSC_ScannedCount"],
				(int)reader["HSC_ScannedClearedCount"],
				(int)reader["HSC_ScannedHeldCount"],
				(int)reader["HSC_ScannedNoneReportedCount"]
				), expected));
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new ETailTestHelper(TestConnection);
		}
		protected ETailTestHelper helper;
	}
}
