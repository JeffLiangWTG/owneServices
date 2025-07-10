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
	[TestedType(typeof(PopulateUnitLineItemForCYDReleaseAdviceLine))]
	public class PopulateUnitLineItemForCYDReleaseAdviceLineTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			if (DbObjectCreator.ColumnExists(TestConnection, CYDReleaseAdviceLineSchema.Constants.TableName, CYDReleaseAdviceLineSchema.YEL_YLI_UnitLineItem.Name))
			{
				DBTransformationTestHelper.DropFunctionIfExists("Report_ContainerInventoryDetails");
				DBTransformationTestHelper.DropIndexIfExists(CYDReleaseAdviceLineSchema.Constants.TableName, "FK_RX__YEL_YLI_UnitLineItem");
				DBTransformationTestHelper.DropConstraintIfExists(CYDReleaseAdviceLineSchema.Constants.TableName, "CYDReleaseAdviceLine_YEL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N", TestConnection);
				DBTransformationTestHelper.DropColumnIfExists(CYDReleaseAdviceLineSchema.Constants.TableName, CYDReleaseAdviceLineSchema.Constants.YEL_YLI_UnitLineItem, TestConnection);
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReleaseAdviceLineSchema.Constants.TableName, "YEL_Quantity", "SMALLINT", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReleaseAdviceLineSchema.Constants.TableName, "YEL_Type", "CHAR(3)", defaultValue: "''");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDReleaseAdviceLineSchema.Constants.TableName, "YEL_RC_ContainerType", "UNIQUEIDENTIFIER");
			}

			var sql = new StringBuilder();

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);

			var releaseAdvice = new CYDReleaseAdvice(warehouse, "JOB003").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.CYDReleaseAdviceLine (YEL_PK, YEL_YRE_ReleaseAdvice, YEL_Quantity, YEL_Type, YEL_RC_ContainerType, YEL_PickupDate, YEL_ReadyDate, YEL_SystemCreateTimeUtc, YEL_SystemCreateUser, YEL_SystemLastEditTimeUtc, YEL_SystemLastEditUser) VALUES
                                ('492C8CC9-856E-4566-B291-8D531BBCD512','{releaseAdvice.PK}', 1, 'CNT','{refContainer.PK}', '2024-06-08','2024-06-18', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
INSERT INTO dbo.CYDReleaseAdviceLine (YEL_PK, YEL_YRE_ReleaseAdvice, YEL_Quantity, YEL_Type, YEL_RC_ContainerType, YEL_PickupDate, YEL_ReadyDate, YEL_SystemCreateTimeUtc, YEL_SystemCreateUser, YEL_SystemLastEditTimeUtc, YEL_SystemLastEditUser) VALUES
                                ('44DB2772-4A6D-42EC-B7AE-6D0DE5CA4B0E','{releaseAdvice.PK}', 5, 'GEN','{refContainer.PK}', '2024-06-08', '2024-06-18', '2023-07-01 07:21:00', '~BP', '2024-07-01 07:21:00', '~BP');
");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, CYDReleaseAdviceLineSchema.Constants.TableName, CYDReleaseAdviceLineSchema.YEL_YLI_UnitLineItem.Name));
			AssertEquals(true, DbObjectCreator.TableExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName));
			var releaseAdviceLines = CYDReleaseAdviceLine.ShallowLoadFromDB(TestConnection);
			AssertEquals("There should be two receive lines", 2, releaseAdviceLines.Length);

			foreach (var releaseLine in releaseAdviceLines)
			{
				var unitLine = CYDUnitLineItem.ShallowLoadFromDB(TestConnection).Single(l => l.PK == releaseLine.YEL_YLI_UnitLineItem);
				AssertNotNull(unitLine);
				AssertNotNull(unitLine.YLI_RC_ContainerType);

				if (string.Equals(releaseLine.PK.ToString(), "492C8CC9-856E-4566-B291-8D531BBCD512", StringComparison.OrdinalIgnoreCase))
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

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateUnitLineItemForCYDReleaseAdviceLine();
	}
}
