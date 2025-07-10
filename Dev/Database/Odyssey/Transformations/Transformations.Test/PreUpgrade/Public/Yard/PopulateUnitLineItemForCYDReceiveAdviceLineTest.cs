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
	[TestedType(typeof(PopulateUnitLineItemForCYDReceiveAdviceLine))]
	public class PopulateUnitLineItemForCYDReceiveAdviceLineTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			if (DbObjectCreator.ColumnExists(TestConnection, CYDReceiveAdviceLineSchema.Constants.TableName, CYDReceiveAdviceLineSchema.YRL_YLI_UnitLineItem.Name))
			{
				DBTransformationTestHelper.DropFunctionIfExists("Report_ContainerInventoryDetails");
				DBTransformationTestHelper.DropIndexIfExists(CYDReceiveAdviceLineSchema.Constants.TableName, "FK_RX__YRL_YLI_UnitLineItem");
				DBTransformationTestHelper.DropConstraintIfExists(CYDReceiveAdviceLineSchema.Constants.TableName, "CYDReceiveAdviceLine_YRL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N", TestConnection);
				DBTransformationTestHelper.DropColumnIfExists(CYDReceiveAdviceLineSchema.Constants.TableName, CYDReceiveAdviceLineSchema.Constants.YRL_YLI_UnitLineItem, TestConnection);
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_Quantity", "SMALLINT", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_Type", "CHAR(3)", defaultValue: "''");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_ManufactureDate", "DATE");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_RC_ContainerType", "UNIQUEIDENTIFIER");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_IsEmpty", "BIT", defaultValue: "1");
			}

			var sql = new StringBuilder();

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);

			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB002").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.CYDReceiveAdviceLine (YRL_PK, YRL_YRA_ReceiveAdvice, YRL_Quantity, YRL_Type, YRL_RC_ContainerType, YRL_IsEmpty, YRL_ManufactureDate, YRL_SystemCreateTimeUtc, YRL_SystemCreateUser, YRL_SystemLastEditTimeUtc, YRL_SystemLastEditUser) VALUES
                                ('992C8CC9-856E-4566-B291-8D531BBCD512','{receiveAdvice.PK}', 1, 'CNT','{refContainer.PK}', 1, '2024-06-18', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
INSERT INTO dbo.CYDReceiveAdviceLine (YRL_PK, YRL_YRA_ReceiveAdvice, YRL_Quantity, YRL_Type, YRL_RC_ContainerType, YRL_IsEmpty, YRL_ManufactureDate, YRL_SystemCreateTimeUtc, YRL_SystemCreateUser, YRL_SystemLastEditTimeUtc, YRL_SystemLastEditUser) VALUES
                                ('64DB2772-4A6D-42EC-B7AE-6D0DE5CA4B0E','{receiveAdvice.PK}', 5, 'GEN','{refContainer.PK}', 0, '2022-08-19', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, CYDReceiveAdviceLineSchema.Constants.TableName, CYDReceiveAdviceLineSchema.YRL_YLI_UnitLineItem.Name));
			AssertEquals(true, DbObjectCreator.TableExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName));
			var receiveAdviceLines = CYDReceiveAdviceLine.ShallowLoadFromDB(TestConnection);
			AssertEquals("There should be two receive lines", 2, receiveAdviceLines.Length);

			foreach (var receiveLine in receiveAdviceLines)
			{
				var unitLine = CYDUnitLineItem.ShallowLoadFromDB(TestConnection).Single(l => l.PK == receiveLine.YRL_YLI_UnitLineItem);
				AssertNotNull(unitLine);
				AssertNotNull(unitLine.YLI_RC_ContainerType);
				AssertNotNull(unitLine.YLI_ManufactureDate);

				if (string.Equals(receiveLine.PK.ToString(), "992C8CC9-856E-4566-B291-8D531BBCD512", StringComparison.OrdinalIgnoreCase))
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

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateUnitLineItemForCYDReceiveAdviceLine();
	}
}
