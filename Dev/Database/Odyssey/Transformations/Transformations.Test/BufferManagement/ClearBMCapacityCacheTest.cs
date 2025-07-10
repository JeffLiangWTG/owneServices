using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BufferManagement
{
	[TestedType(typeof(ClearBMCapacityCache))]
	sealed class ClearBMCapacityCacheTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new ClearBMCapacityCache();

		protected override void PrepareTestData()
		{
			var systemPK = Guid.NewGuid();
			var componentPK = Guid.NewGuid();

			if (DbObjectCreator.ColumnExists(Db.Connection, "BMCapacityCache", "BMC_Type"))
			{
				TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.BMSystem
(FS_PK, FS_Name, FS_Description, FS_SystemCreateTimeUtc, FS_SystemCreateUser, FS_SystemLastEditTimeUtc, FS_SystemLastEditUser)
values ('{systemPK}', 'System', 'A BMS System', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.BMComponent
(FC_PK, FC_FS_System, FC_Name, FC_Type, FC_SystemCreateTimeUtc, FC_SystemCreateUser, FC_SystemLastEditTimeUtc, FC_SystemLastEditUser)
values ('{componentPK}', '{systemPK}', 'Component1', 'BUC', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

				TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.BMCapacityCache
	(BMC_PK, BMC_FC_Component, BMC_Zone, BMC_CalculatedTimeUtc, BMC_SystemCreateTimeUtc, BMC_SystemCreateUser, BMC_SystemLastEditTimeUtc, BMC_SystemLastEditUser, BMC_GS_NKStaff, BMC_Type, BMC_Value)
VALUES
	(NEWID(), '{componentPK}', 0, GETUTCDATE(), GETUTCDATE(), 'E', GETUTCDATE(),'E','E', 0, 0),
	(NEWID(), '{componentPK}', 1, GETUTCDATE(), GETUTCDATE(), 'E', GETUTCDATE(),'E','E', 0, 0),
	(NEWID(), '{componentPK}', 2, GETUTCDATE(), GETUTCDATE(), 'E', GETUTCDATE(),'E','E', 0, 0)");
			}
		}

		protected override void AssertTransformationResults() => AssertEquals(0, TestConnection.ExecuteScalar($"SELECT Count(*) FROM dbo.BMCapacityCache"));
	}
}

