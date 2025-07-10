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
	[TestedType(typeof(PopulateUnitLineItemForCYDDelivery))]
	public class PopulateUnitLineItemForCYDDeliveryTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			if (DbObjectCreator.ColumnExists(TestConnection, CYDDeliverySchema.Constants.TableName, CYDDeliverySchema.YDL_YLI_UnitLineItem.Name))
			{
				DBTransformationTestHelper.DropFunctionIfExists("Report_ContainerInventoryDetails");
				DBTransformationTestHelper.DropIndexIfExists(CYDDeliverySchema.Constants.TableName, "FK_RX__YDL_YLI_UnitLineItem");
				DBTransformationTestHelper.DropConstraintIfExists(CYDDeliverySchema.Constants.TableName, "CYDDelivery_YDL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N", TestConnection);
				DBTransformationTestHelper.DropColumnIfExists(CYDDeliverySchema.Constants.TableName, CYDDeliverySchema.Constants.YDL_YLI_UnitLineItem, TestConnection);
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDDeliverySchema.Constants.TableName, "YDL_Quantity", "SMALLINT", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDDeliverySchema.Constants.TableName, "YDL_Type", "CHAR(3)", defaultValue: "''");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDDeliverySchema.Constants.TableName, "YDL_ManufactureDate", "DATE");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDDeliverySchema.Constants.TableName, "YDL_RC_ContainerType", "UNIQUEIDENTIFIER");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDDeliverySchema.Constants.TableName, "YDL_IsEmpty", "BIT", defaultValue: "1");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDDeliverySchema.Constants.TableName, "YDL_DeliveryID", "VARCHAR(20)", defaultValue: "''");
			}

			var sql = new StringBuilder();

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.CYDDelivery (YDL_PK, YDL_YDH_DeliveryHeader, YDL_DeliveryID, YDL_Quantity, YDL_Type, YDL_RC_ContainerType, YDL_IsEmpty, YDL_ManufactureDate, YDL_SystemCreateTimeUtc, YDL_SystemCreateUser, YDL_SystemLastEditTimeUtc, YDL_SystemLastEditUser) VALUES
                                ('692C8CC9-856E-4566-B291-8D531BBCD512','{deliveryHeader.PK}', 'YDL000000000001', 1, 'CNT','{refContainer.PK}', 1, '2024-06-18', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
INSERT INTO dbo.CYDDelivery (YDL_PK, YDL_YDH_DeliveryHeader, YDL_DeliveryID, YDL_Quantity, YDL_Type, YDL_RC_ContainerType, YDL_IsEmpty, YDL_ManufactureDate, YDL_SystemCreateTimeUtc, YDL_SystemCreateUser, YDL_SystemLastEditTimeUtc, YDL_SystemLastEditUser) VALUES
                                ('74DB2772-4A6D-42EC-B7AE-6D0DE5CA4B0E','{deliveryHeader.PK}', 'YDL000000000002', 5, 'GEN','{refContainer.PK}', 0, '2022-08-19', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, CYDDeliverySchema.Constants.TableName, CYDDeliverySchema.YDL_YLI_UnitLineItem.Name));
			AssertEquals(true, DbObjectCreator.TableExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName));
			var deliveries = CYDDelivery.ShallowLoadFromDB(TestConnection);
			AssertEquals("There should be two receive lines", 2, deliveries.Length);

			foreach (var delivery in deliveries)
			{
				var unitLine = CYDUnitLineItem.ShallowLoadFromDB(TestConnection).Single(l => l.PK == delivery.YDL_YLI_UnitLineItem);
				AssertNotNull(unitLine);
				AssertNotNull(unitLine.YLI_RC_ContainerType);
				AssertNotNull(unitLine.YLI_ManufactureDate);

				if (string.Equals(delivery.PK.ToString(), "692C8CC9-856E-4566-B291-8D531BBCD512", StringComparison.OrdinalIgnoreCase))
				{
					AssertEquals((short)1, unitLine.YLI_Quantity);
					AssertEquals("CNT", unitLine.YLI_Type);
					AssertEquals(true, unitLine.YLI_IsEmpty);
					AssertEquals(2024, unitLine.YLI_ManufactureDate.Value.Year);
					AssertEquals(6, unitLine.YLI_ManufactureDate.Value.Month);
					AssertEquals(18, unitLine.YLI_ManufactureDate.Value.Day);
				}
				else
				{
					AssertEquals((short)5, unitLine.YLI_Quantity);
					AssertEquals("GEN", unitLine.YLI_Type);
					AssertEquals(false, unitLine.YLI_IsEmpty);
					AssertEquals(2022, unitLine.YLI_ManufactureDate.Value.Year);
					AssertEquals(8, unitLine.YLI_ManufactureDate.Value.Month);
					AssertEquals(19, unitLine.YLI_ManufactureDate.Value.Day);
				}
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateUnitLineItemForCYDDelivery();
	}
}
