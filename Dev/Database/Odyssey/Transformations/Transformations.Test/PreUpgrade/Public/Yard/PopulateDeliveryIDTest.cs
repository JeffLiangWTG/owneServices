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
	[TestedType(typeof(PopulateDeliveryID))]
	public class PopulateDeliveryIDTest : DataTransformationTestCase
	{
		CYDDelivery delivery1;
		CYDDelivery delivery2;
		CYDDelivery delivery3;
		CYDDelivery delivery4;

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CYDDeliverySchema.Constants.TableName, "Constraint_YDL_DeliveryID");
			DBTransformationTestHelper.DropIndexIfExists(CYDDeliverySchema.Constants.TableName, CYDDeliverySchema.Constants.Indexes.NR_UX__YDL_DeliveryID);

			var sql = new StringBuilder();

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var warehouseLocations = warehouse.CreateLocations(sql, 3);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem(refContainer, type: "GEN", isEmpty: false).AppendInsertAndReturnObject(sql);
			var unitLineItem2 = new CYDUnitLineItem(refContainer, type: "CNT", isEmpty: false).AppendInsertAndReturnObject(sql);
			var unitLineItem3 = new CYDUnitLineItem(refContainer, type: "BLK", isEmpty: false).AppendInsertAndReturnObject(sql);
			var unitLineItem4 = new CYDUnitLineItem(refContainer, type: "CHS", isEmpty: false).AppendInsertAndReturnObject(sql);

			delivery1 = new CYDDelivery(deliveryHeader)
			{
				YDL_YLI_UnitLineItem = unitLineItem1
			}.AppendInsertAndReturnObject(sql);

			delivery2 = new CYDDelivery(deliveryHeader)
			{
				YDL_YLI_UnitLineItem = unitLineItem2
			}.AppendInsertAndReturnObject(sql);

			delivery3 = new CYDDelivery(deliveryHeader)
			{
				YDL_YLI_UnitLineItem = unitLineItem3
			}.AppendInsertAndReturnObject(sql);

			delivery4 = new CYDDelivery(deliveryHeader)
			{
				YDL_YLI_UnitLineItem = unitLineItem4
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			var deliveries = CYDDelivery.ShallowLoadFromDB(TestConnection);
			var deliveriesWithDeliveryIDCount = deliveries.Count(y => y.YDL_DeliveryID != null);
			AssertEquals("Expected 4 deliveries to have YDL_DeliveryID value", 4, deliveriesWithDeliveryIDCount);
			AssertEquals("Expected delivery id to be YDL000000000001", expected: "YDL000000000001", deliveries.Single(delivery => delivery.PK == delivery1.PK).YDL_DeliveryID);
			AssertEquals("Expected delivery id to be YDL000000000002", expected: "YDL000000000002", deliveries.Single(delivery => delivery.PK == delivery2.PK).YDL_DeliveryID);
			AssertEquals("Expected delivery id to be YDL000000000003", expected: "YDL000000000003", deliveries.Single(delivery => delivery.PK == delivery3.PK).YDL_DeliveryID);
			AssertEquals("Expected delivery id to be YDL000000000004", expected: "YDL000000000004", deliveries.Single(delivery => delivery.PK == delivery4.PK).YDL_DeliveryID);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateDeliveryID();
	}
}
