using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.LandTransport;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.LandTransport
{
	[TestedType(typeof(RemoveEquipmentItemsWithInvalidParentTableCode))]
	class RemoveEquipmentItemsWithInvalidParentTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveEquipmentItemsWithInvalidParentTableCode();
		}

		protected override void PrepareTestData()
		{
			var containerPK1 = Guid.NewGuid();
			var sqlText = $@"
INSERT INTO dbo.RefContainer(RC_PK, RC_Code, RC_ShippingMode, RC_Description, RC_Length, RC_Height, RC_Width, RC_ContainerType, RC_ISOType, RC_TareWeight, RC_GrossWeight, RC_CubicCapacity, RC_StorageClass, RC_HandlingRateClass, RC_FreightRateClass, RC_IATARateClass, RC_USContainerCode, RC_TEU, RC_IsActive, RC_IsHighCube, RC_HasTynes, RC_HasVents, RC_IsIso, RC_ISOEquipmentSizeTypeCode, RC_SystemCreateTimeUtc, RC_SystemCreateUser, RC_SystemLastEditTimeUtc, RC_SystemLastEditUser, RC_IsSystem, RC_Contour, RC_InsideHeight, RC_InsideLength, RC_InsideWidth, RC_NetWeight)
VALUES
('{containerPK1}', '10001', '', 'CHEP Wooden Pallet', 0.000, 0.000, 0.000, '', '', 0.000, 0.000, 0.000, '', '', '', '', '', 0.000, 0, 0, 0, 0, 1, '', '2018-05-30T05:36:00', 'MW ', '2023-08-18T00:48:00', 'TM', 1, '', 0.000, 0.000, 0.000, 0.000);

INSERT INTO dbo.DtbEquipmentItem (LTE_PK,LTE_ParentID,LTE_ParentTableCode,LTE_RQ_Equipment,LTE_EquipmentTypeQuantity,LTE_RC_EquipmentType,LTE_AutoVersion,LTE_SystemCreateTimeUtc,LTE_SystemCreateUser,LTE_SystemLastEditTimeUtc,LTE_SystemLastEditUser)
VALUES
(NEWID(), NEWID(), 'LTC', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
(NEWID(), NEWID(), 'LTC', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
(NEWID(), NEWID(), 'LTA', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
(NEWID(), NEWID(), 'LTA', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
(NEWID(), NEWID(), 'KG', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
(NEWID(), NEWID(), 'KG', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
(NEWID(), NEWID(), 'KG', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
(NEWID(), NEWID(), 'KG', NULL, 1, '{containerPK1}', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');";

			DBTransformationTestHelper.DropConstraintIfExists("DtbEquipmentItem", "Constraint_LTE_ParentTableCode", Db.Connection);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertTransformationResults()
		{
			var numberOfInvalidRow = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM DtbEquipmentItem where LTE_ParentTableCode<>'KG'");
			AssertEquals("All Equipment Item with invalid data should be deleted", 0, numberOfInvalidRow);

			var numberOfValidRow = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM DtbEquipmentItem where LTE_ParentTableCode='KG'");
			AssertEquals("All Equipment Item with valid data should not be deleted", 4, numberOfValidRow);
		}

		public void TestWhenDtbEquipmentItemDoesNotExist_ThenTransformationDoesNotRun()
		{
			TestConnection.ExecuteNonQuery("DROP TABLE DtbEquipmentItem");
			AssertEquals(false, DbObjectCreator.TableExists(TestConnection, "DtbEquipmentItem"));

			AssertNoExceptionThrown(RunTransformation);
		}

		public void TestTransformationWorksCorrectly()
		{
			PrepareTestData();
			RunTransformation();
			AssertTransformationResults();
		}
	}
}
