using System;
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
	[TestedType(typeof(PopulateUnitLineItemForPickup))]
	public class PopulateUnitLineItemForPickupTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			if (DbObjectCreator.ColumnExists(TestConnection, CYDPickupSchema.Constants.TableName, CYDPickupSchema.YPL_YLI_UnitLineItem.Name))
			{
				DBTransformationTestHelper.DropFunctionIfExists("Report_ContainerInventoryDetails");
				DBTransformationTestHelper.DropIndexIfExists(CYDPickupSchema.Constants.TableName, "FK_RX__YPL_YLI_UnitLineItem");
				DBTransformationTestHelper.DropConstraintIfExists(CYDPickupSchema.Constants.TableName, "CYDPickup_YPL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N", TestConnection);
				DBTransformationTestHelper.DropColumnIfExists(CYDPickupSchema.Constants.TableName, CYDPickupSchema.Constants.YPL_YLI_UnitLineItem, TestConnection);
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDPickupSchema.Constants.TableName, "YPL_Quantity", "SMALLINT", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDPickupSchema.Constants.TableName, "YPL_Type", "CHAR(3)", defaultValue: "''");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDPickupSchema.Constants.TableName, "YPL_RC_ContainerType", "UNIQUEIDENTIFIER");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDPickupSchema.Constants.TableName, "YPL_PickupID", "VARCHAR(20)", defaultValue: "''");
			}

			var sql = new StringBuilder();

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);

			var pickupHeader = new CYDPickupHeader(warehouse, "JOB004").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.CYDPickup (YPL_PK, YPL_YPH_PickupHeader, YPL_PickupID, YPL_Quantity, YPL_Type, YPL_RC_ContainerType, YPL_SystemCreateTimeUtc, YPL_SystemCreateUser, YPL_SystemLastEditTimeUtc, YPL_SystemLastEditUser) VALUES
                                ('292C8CC9-856E-4566-B291-8D531BBCD512','{pickupHeader.PK}', 'YPL000000000001', 1, 'CNT','{refContainer.PK}', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
INSERT INTO dbo.CYDPickup (YPL_PK, YPL_YPH_PickupHeader, YPL_PickupID, YPL_Quantity, YPL_Type, YPL_RC_ContainerType, YPL_SystemCreateTimeUtc, YPL_SystemCreateUser, YPL_SystemLastEditTimeUtc, YPL_SystemLastEditUser) VALUES
                                ('84DB2772-4A6D-42EC-B7AE-6D0DE5CA4B0E','{pickupHeader.PK}', 'YPL000000000002', 5, 'GEN','{refContainer.PK}', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, CYDPickupSchema.Constants.TableName, CYDPickupSchema.YPL_YLI_UnitLineItem.Name));
			AssertEquals(true, DbObjectCreator.TableExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName));
			var pickups = CYDPickup.ShallowLoadFromDB(TestConnection);
			AssertEquals("There should be two receive lines", 2, pickups.Length);

			foreach (var pickup in pickups)
			{
				var unitLine = CYDUnitLineItem.ShallowLoadFromDB(TestConnection).Single(l => l.PK == pickup.YPL_YLI_UnitLineItem);
				AssertNotNull(unitLine);
				AssertNotNull(unitLine.YLI_RC_ContainerType);

				if (string.Equals(pickup.PK.ToString(), "292C8CC9-856E-4566-B291-8D531BBCD512", StringComparison.OrdinalIgnoreCase))
				{
					AssertEquals((short)1, unitLine.YLI_Quantity);
					AssertEquals("CNT", unitLine.YLI_Type);
				}
				else
				{
					AssertEquals((short)5, unitLine.YLI_Quantity);
					AssertEquals("GEN", unitLine.YLI_Type);
				}
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateUnitLineItemForPickup();
	}
}
