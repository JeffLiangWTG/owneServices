using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	[TestedType(typeof(TG_HVLVItem_PopulateUsageTimes_InsertUpdate))]
	class TTG_HVLVItem_PopulateUsageTimes_InsertTest : DbCreateScriptTest
	{
		public void TestHVI_ShipperFirstUsageTimeUtcInsert()
		{
			var shipmentPK = TestDataCreator.CreateShipment("SHP000001");
			var loadListPK = helper.CreateHVLVOriginLoadList("ELL000001");
			var itemPK = helper.CreateHVLVItem("IHAVEBOTH", "HVC001", "HVH001", 111, loadListPK, shipmentPK);
			Func<object> usageTimeGetter = () => helper.GetRow("HVLVItem", "HVI_PK", itemPK)["HVI_ShipperFirstUsageTimeUtc"];

			AssertEquals(DBNull.Value, usageTimeGetter());

			itemPK = helper.CreateHVLVItem("IHAVESHIPMENT", "HVC002", "HVH002", 222, DBNull.Value, shipmentPK);
			AssertEquals(DBNull.Value, usageTimeGetter());

			itemPK = helper.CreateHVLVItem("IHAVELOADLIST", "HVC003", "HVH003", 333, loadListPK, DBNull.Value);
			AssertEquals(DBNull.Value, usageTimeGetter());

			itemPK = helper.CreateHVLVItem("IAMALONE", "HVC004", "HVH004", 444, DBNull.Value, DBNull.Value);
			AssertNotEquals(DBNull.Value, usageTimeGetter());
		}

		public void TestHVI_OriginFirstUsageTimeUtcInsert()
		{
			var loadListPK = helper.CreateHVLVOriginLoadList("ELL000001");
			var itemPK = helper.CreateHVLVItem("IAMALONE", "HVC001", "HVH001", 111, DBNull.Value, DBNull.Value);
			Func<object> usageTimeGetter = () => helper.GetRow("HVLVItem", "HVI_PK", itemPK)["HVI_OriginFirstUsageTimeUtc"];

			AssertEquals(DBNull.Value, usageTimeGetter());

			itemPK = helper.CreateHVLVItem("IHAVELOADLIST", "HVC002", "HVH002", 222, loadListPK, DBNull.Value);
			AssertNotEquals(DBNull.Value, usageTimeGetter());
		}

		public void TestHVI_DestinationFirstUsageTimeUtcInsert()
		{
			var shipmentPK = TestDataCreator.CreateShipment("SHP000001");
			var itemPK = helper.CreateHVLVItem("IHAVEBOTH", "HVC001", "HVH001", 111, DBNull.Value, DBNull.Value);
			Func<object> usageTimeGetter = () => helper.GetRow("HVLVItem", "HVI_PK", itemPK)["HVI_DestinationFirstUsageTimeUtc"];

			AssertEquals(DBNull.Value, usageTimeGetter());

			itemPK = helper.CreateHVLVItem("IHAVESHIPMENT", "HVC002", "HVH002", 222, DBNull.Value, shipmentPK);
			AssertNotEquals(DBNull.Value, usageTimeGetter());
		}

		public void TestHVI_OriginFirstUsageTimeUtcUpdate()
		{
			var loadListPK = helper.CreateHVLVOriginLoadList("ELL000001");
			var itemPK = helper.CreateHVLVItem("IHAVEBOTH", "HVC001", "HVH001", 111, DBNull.Value, DBNull.Value);
			helper.ClearUsageTimeUtc(itemPK);

			Func<string, object> usageTimeGetter = (dataColumn) => helper.GetRow("HVLVItem", "HVI_PK", itemPK)[dataColumn];

			AssertUsageTimeUtcAreAllNull("precondition : Usage Time Utc are all null", usageTimeGetter);

			helper.SetLoadList(loadListPK, itemPK);
			CombineAssertions("only update HVI_OriginFirstUsageTimeUtc", () =>
			{
				AssertNotEquals("Origin First Usage Time Utc is not null", DBNull.Value, usageTimeGetter("HVI_OriginFirstUsageTimeUtc"));
				AssertEquals("Shipper First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_ShipperFirstUsageTimeUtc"));
				AssertEquals("Destination First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_DestinationFirstUsageTimeUtc"));
			});
		}

		public void TestHVI_DestinationFirstUsageTimeUtcUpdate()
		{
			var shipmentPK = TestDataCreator.CreateShipment("SHP000001");
			var itemPK = helper.CreateHVLVItem("IHAVEBOTH", "HVC001", "HVH001", 111, DBNull.Value, DBNull.Value);
			helper.ClearUsageTimeUtc(itemPK);

			Func<string, object> usageTimeGetter = (dataColumn) => helper.GetRow("HVLVItem", "HVI_PK", itemPK)[dataColumn];

			AssertUsageTimeUtcAreAllNull("precondition : Usage Time Utc are all null", usageTimeGetter);

			helper.SetLoadedOnShipment(shipmentPK, itemPK);
			CombineAssertions("only update HVI_DestinationFirstUsageTimeUtc", () =>
			{
				AssertNotEquals("Destination First Usage Time Utc is not null", DBNull.Value, usageTimeGetter("HVI_DestinationFirstUsageTimeUtc"));
				AssertEquals("Origin First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_OriginFirstUsageTimeUtc"));
				AssertEquals("Shipper First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_ShipperFirstUsageTimeUtc"));
			});
		}

		public void TestUsageTimeUtcWouldOnlyBeUpdatedAfterChangeLoadListAndLoadedOnShipment()
		{
			var itemPK = helper.CreateHVLVItem("IHAVEBOTH", "HVC001", "HVH001", 111, DBNull.Value, DBNull.Value);
			helper.ClearUsageTimeUtc(itemPK);

			Func<string, object> usageTimeGetter = (dataColumn) => helper.GetRow("HVLVItem", "HVI_PK", itemPK)[dataColumn];

			AssertUsageTimeUtcAreAllNull("precondition : Usage Time Utc are all null", usageTimeGetter);
			helper.UpdateHVLVItem(itemPK, 1, 1, 1, 1);
			CombineAssertions("Usage Time Utc would not be updated by regular column", () =>
			{
				AssertEquals("Shipper First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_ShipperFirstUsageTimeUtc"));
				AssertEquals("Origin First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_OriginFirstUsageTimeUtc"));
				AssertEquals("Destination First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_DestinationFirstUsageTimeUtc"));
			});
		}

		void AssertUsageTimeUtcAreAllNull(string message, Func<string, object> usageTimeGetter)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Shipper First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_ShipperFirstUsageTimeUtc"));
				AssertEquals("Origin First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_OriginFirstUsageTimeUtc"));
				AssertEquals("Destination First Usage Time Utc is null", DBNull.Value, usageTimeGetter("HVI_DestinationFirstUsageTimeUtc"));
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

