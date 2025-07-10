using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Yard.Testing
{
	[TestedType(typeof(PopulatePickupID))]
	public class PopulatePickupIDTest : DataTransformationTestCase
	{
		CYDPickup pickup1;
		CYDPickup pickup2;
		CYDPickup pickup3;
		CYDPickup pickup4;

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CYDPickupSchema.Constants.TableName, "Constraint_YPL_PickupID");
			DBTransformationTestHelper.DropIndexIfExists(CYDPickupSchema.Constants.TableName, CYDPickupSchema.Constants.Indexes.NR_UX__YPL_PickupID);

			var sql = new StringBuilder();

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var warehouseLocations = warehouse.CreateLocations(sql, 3);

			var pickupHeader = new CYDPickupHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem(refContainer, type: "GEN", isEmpty: false).AppendInsertAndReturnObject(sql);
			var unitLineItem2 = new CYDUnitLineItem(refContainer, type: "CNT", isEmpty: false).AppendInsertAndReturnObject(sql);
			var unitLineItem3 = new CYDUnitLineItem(refContainer, type: "BLK", isEmpty: false).AppendInsertAndReturnObject(sql);
			var unitLineItem4 = new CYDUnitLineItem(refContainer, type: "CHS", isEmpty: false).AppendInsertAndReturnObject(sql);

			pickup1 = new CYDPickup(pickupHeader)
			{
				YPL_YLI_UnitLineItem = unitLineItem1
			}.AppendInsertAndReturnObject(sql);

			pickup2 = new CYDPickup(pickupHeader)
			{
				YPL_YLI_UnitLineItem = unitLineItem2
			}.AppendInsertAndReturnObject(sql);

			pickup3 = new CYDPickup(pickupHeader)
			{
				YPL_YLI_UnitLineItem = unitLineItem3
			}.AppendInsertAndReturnObject(sql);

			pickup4 = new CYDPickup(pickupHeader)
			{
				YPL_YLI_UnitLineItem = unitLineItem4
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			var pickups = CYDPickup.ShallowLoadFromDB(TestConnection);
			var pickupsWithPickupIDCount = pickups.Count(y => y.YPL_PickupID != null);
			AssertEquals("Expected 4 pickups to have YPL_PickupID value", 4, pickupsWithPickupIDCount);
			AssertEquals("Expected pick up id to be YPL000000000001", expected: "YPL000000000001", pickups.Single(pickup => pickup.PK == pickup1.PK).YPL_PickupID);
			AssertEquals("Expected pick up id to be YPL000000000002", expected: "YPL000000000002", pickups.Single(pickup => pickup.PK == pickup2.PK).YPL_PickupID);
			AssertEquals("Expected pick up id to be YPL000000000003", expected: "YPL000000000003", pickups.Single(pickup => pickup.PK == pickup3.PK).YPL_PickupID);
			AssertEquals("Expected pick up id to be YPL000000000004", expected: "YPL000000000004", pickups.Single(pickup => pickup.PK == pickup4.PK).YPL_PickupID);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulatePickupID();
	}
}
