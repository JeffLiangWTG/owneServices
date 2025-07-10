using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	[TestedType(typeof(TG_HVLVItem_CalculateMeasurements_Update))]
	class TG_HVLVItem_CalculateMeasurements_UpdateTest : DbCreateScriptTest
	{
		public void TestWeightAndVolumeOnUpdateSingleItem()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new", 1, 1, 1, 1);
			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 2, 3, 4);

			CombineAssertions(() =>
			{
				var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(2, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 2", 2m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 3", 3m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 4", 4m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 5", 5m, consignmentRow["HVC_ActualVolume"]);
			});
			helper.UpdateHVLVItem(item1PK, 2, 4, 6, 8);

			CombineAssertions(() =>
			{
				var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(2, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 3", 3m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 6", 6m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 9", 9m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 12", 12m, consignmentRow["HVC_ActualVolume"]);
			});
		}

		public void TestWeightAndVolumeOnUpdateMultipleItems()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");
			var numItems = 5;
			var itemPKs = helper.CreateMultipleHVLVItem(consignmentPK, 1, numItems, "item").ToArray();

			CombineAssertions(() =>
			{
				var itemSums = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT count(*) ItemCount, sum(HVI_ActualWeight) WeightSum, sum(HVI_ActualVolume) VolumeSum FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(numItems, itemSums.Rows[0]["ItemCount"]);
				AssertEquals("WeightSum should be 1+2+3+4+5=15", 15m, itemSums.Rows[0]["WeightSum"]);
				AssertEquals("VolumeSum should be 1+2+3+4+5=15", 15m, itemSums.Rows[0]["VolumeSum"]);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 15", itemSums.Rows[0]["WeightSum"], consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 15", itemSums.Rows[0]["WeightSum"], consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 15", itemSums.Rows[0]["VolumeSum"], consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 15", itemSums.Rows[0]["VolumeSum"], consignmentRow["HVC_ActualVolume"]);
			});

			helper.UpdateHVLVItems(new Guid[] { itemPKs[1], itemPKs[2] }, "HVI_ActualWeight", 100);

			CombineAssertions(() =>
			{
				var itemSums = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT count(*) ItemCount, sum(HVI_ManifestedWeight) ManifestedWeightSum, sum(HVI_ActualWeight) ActualWeightSum, sum(HVI_ActualVolume) VolumeSum FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(numItems, itemSums.Rows[0]["ItemCount"]);
				AssertEquals("WeightSum should be 1+2+3+4+5=15", 15m, itemSums.Rows[0]["ManifestedWeightSum"]);
				AssertEquals("WeightSum should be 1+100+100+4+5=210", 210m, itemSums.Rows[0]["ActualWeightSum"]);
				AssertEquals("VolumeSum should be 1+2+3+4+5=15", 15m, itemSums.Rows[0]["VolumeSum"]);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 15", itemSums.Rows[0]["ManifestedWeightSum"], consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 210", itemSums.Rows[0]["ActualWeightSum"], consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 15", itemSums.Rows[0]["VolumeSum"], consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 15", itemSums.Rows[0]["VolumeSum"], consignmentRow["HVC_ActualVolume"]);
			});
		}

		public void TestWeightAndVolume_WithInactiveItem_ShouldExcludeFromCalculation()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new", 1, 1, 1, 1);
			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 2, 3, 4);

			CombineAssertions(() =>
			{
				var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(2, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 2", 2m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 3", 3m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 4", 4m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 5", 5m, consignmentRow["HVC_ActualVolume"]);
			});
			helper.UpdateHVLVItem(item1PK, 2, 4, 6, 8, false);

			CombineAssertions(() =>
			{
				var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(2, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ManifestedWeight should be 1", 1m, consignmentRow["HVC_ManifestedWeight"]);
				AssertEquals("HVC_ActualWeight should be 2", 2m, consignmentRow["HVC_ActualWeight"]);
				AssertEquals("HVC_ManifestedVolume should be 3", 3m, consignmentRow["HVC_ManifestedVolume"]);
				AssertEquals("HVC_ActualVolume should be 4", 4m, consignmentRow["HVC_ActualVolume"]);
			});
		}

		public void TestUpdateItemIsActive_ShouldUpdateItemCountOnConsignment()
		{
			var headerPK = helper.CreateHVLVBookingHeader(1, "M00001001");
			var consignmentPK = helper.CreateHVLVConsignment(headerPK, 1, "ConsignmentId");

			var item1PK = helper.CreateHVLVItem(consignmentPK, 1, "new", 1, 1, 1, 1);
			var item2PK = helper.CreateHVLVItem(consignmentPK, 1, "new2", 1, 2, 3, 4);

			CombineAssertions(() =>
			{
				var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(2, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 2", (short)2, consignmentRow["HVC_ItemCount"]);
			});
			helper.UpdateHVLVItem(item1PK, 2, 4, 6, 8, false);

			CombineAssertions(() =>
			{
				var item = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.HVLVItem WHERE HVI_HVC_Consignment = '" + consignmentPK + "'");
				AssertEquals(2, item.Rows.Count);
				var consignmentRow = helper.GetRow("HVLVConsignment", "HVC_PK", consignmentPK);
				AssertEquals("HVC_ItemCount should be 1", (short)1, consignmentRow["HVC_ItemCount"]);
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

