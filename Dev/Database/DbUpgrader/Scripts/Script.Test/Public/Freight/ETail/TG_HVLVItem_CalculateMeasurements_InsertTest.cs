using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	[TestedType(typeof(TG_HVLVItem_CalculateMeasurements_Insert))]
	class TG_HVLVItem_CalculateMeasurements_InsertTest : DbCreateScriptTest
	{
		public void TestItemCountOnInsertSingleItem()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 0", (short)0, consignmentRow["HVC_ItemCount"]);
			});

			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new", 1, 1, 1, 1);

			var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
			AssertEquals(1, item.Rows.Count);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 1", (short)1, consignmentRow["HVC_ItemCount"]);
			});

			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 2, 3, 4);
			item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");

			AssertEquals(2, item.Rows.Count);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 2", (short)2, consignmentRow["HVC_ItemCount"]);
			});
		}

		public void TestItemCountOnInsertMultipleItems()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 0", (short)0, consignmentRow["HVC_ItemCount"]);
			});

			var numItems = 5;
			var itemPKs = helper.CreateMultipleHVLVItem(consignmentPK, 1, numItems, "item").ToArray();

			var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
			AssertEquals(numItems, item.Rows.Count);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be " + numItems, (short)numItems, consignmentRow["HVC_ItemCount"]);
			});
		}

		public void TestItemCountOnInsertInactiveItem()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");
			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new", 1, 1, 1, 1);

			var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");

			CombineAssertions(() =>
			{
				AssertEquals(1, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 1", (short)1, consignmentRow["HVC_ItemCount"]);
			});

			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 2, 3, 4, false);

			item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");

			CombineAssertions(() =>
			{
				AssertEquals(2, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 1", (short)1, consignmentRow["HVC_ItemCount"]);
			});
		}

		public void TestWeightAndVolumeOnInsertSingleItem()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 0", 0m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 0", 0m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 0", 0m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 0", 0m, consignmentRow["HVC_ActualVolume"]);
			});

			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new", 1, 1, 1, 1);

			var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
			AssertEquals(1, item.Rows.Count);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 1", 1m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 1", 1m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 1", 1m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 1", 1m, consignmentRow["HVC_ActualVolume"]);
			});

			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 2, 3, 4);
			item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");

			AssertEquals(2, item.Rows.Count);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 2", 2m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 3", 3m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 4", 4m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 5", 5m, consignmentRow["HVC_ActualVolume"]);
			});
		}

		public void TestWeightAndVolumeOnInsertMultipleItems()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 0", 0m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 0", 0m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 0", 0m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 0", 0m, consignmentRow["HVC_ActualVolume"]);
			});

			var numItems = 5;
			var itemPKs = helper.CreateMultipleHVLVItem(consignmentPK, 1, numItems, "item").ToArray();

			var itemSums = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT count(*) ItemCount, sum(HVI_ActualWeight) WeightSum, sum(HVI_ActualVolume) VolumeSum FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
			AssertEquals(numItems, itemSums.Rows[0]["ItemCount"]);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 15", itemSums.Rows[0]["WeightSum"], consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 15", itemSums.Rows[0]["WeightSum"], consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 15", itemSums.Rows[0]["VolumeSum"], consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 15", itemSums.Rows[0]["VolumeSum"], consignmentRow["HVC_ActualVolume"]);
			});
		}

		public void TestWeightAndVolumeOnInsertInactiveItem()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");
			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new", 1, 1, 1, 1);

			var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
			AssertEquals(1, item.Rows.Count);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 1", 1m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 1", 1m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 1", 1m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 1", 1m, consignmentRow["HVC_ActualVolume"]);
			});

			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 2, 3, 4, false);

			item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
			AssertEquals(2, item.Rows.Count);

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 1", 1m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 1", 1m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 1", 1m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 1", 1m, consignmentRow["HVC_ActualVolume"]);
			});
		}

		public void TestMixOfManifestAndActualWeightAndVolumeOnInsertMultipleItems()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			CombineAssertions(() =>
			{
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 0", 0m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 0", 0m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 0", 0m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 0", 0m, consignmentRow["HVC_ActualVolume"]);
			});
			//create items with a mix of manifest and actual weight/volume, some with both
			//assert when created (weight/volume = (actual>0)?actual:manifest) is used to calculate value 
			//manifestedWeight, actualWeight, manifestedVolume, actualVolume			//calculated total for header
			//
			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new1", 0, 0, 9, 9);  //weight=0 volume=9
			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 0, 2, 6);  //weight=1 volume=6
			var item3PK = helper.CreateHVLVItem(consignmentPK, 1, "new3", 0, 1, 7, 2);  //weight=1 volume=2
			var item4PK = helper.CreateHVLVItem(consignmentPK, 1, "new4", 2, 2, 0, 3);  //weight=2 volume=3
			var item5PK = helper.CreateHVLVItem(consignmentPK, 1, "new5", 4, 1, 3, 0);  //weight=1 volume=3
			var item6PK = helper.CreateHVLVItem(consignmentPK, 1, "new6", 1, 5, 0, 0);  //weight=5 volume=0
																						//sum for consignment total = 8, 9,21,20	//header=10      =23
			var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");

			CombineAssertions(() =>
			{
				AssertEquals(6, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 8", 8m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 9", 9m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 21", 21m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 20", 20m, consignmentRow["HVC_ActualVolume"]);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new ETailTestHelper(TestConnection);
		}
		protected ETailTestHelper helper;
	}
}

