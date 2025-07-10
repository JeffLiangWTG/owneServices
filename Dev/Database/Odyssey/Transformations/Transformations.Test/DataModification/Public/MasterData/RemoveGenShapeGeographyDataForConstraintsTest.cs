using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterData;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.MasterData.Testing
{
	[TestedType(typeof(RemoveGenShapeGeographyDataForConstraints))]
	public class RemoveGenShapeGeographyDataForConstraintsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GenShapeGeography WHERE SHG_ParentTableCode IN ('RN', 'R9', 'FZ', '') and SHG_Name = 'TestInConstraints' "));
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GenShapeGeography WHERE SHG_ParentTableCode NOT IN ('RN', 'R9', 'FZ', '') and SHG_Name = 'TestOutConstraints'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveGenShapeGeographyDataForConstraints();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GenShapeGeographySchema.Constants.TableName, "Constraint_SHG_ParentTableCode_NoCheck");
			var sql = $@"
insert into dbo.GenShapeGeography (SHG_PK, SHG_Name, SHG_Type, SHG_ParentTableCode, SHG_SystemCreateTimeUtc, SHG_SystemCreateUser, SHG_SystemLastEditTimeUtc, SHG_SystemLastEditUser)
values ('{Guid.NewGuid()}', 'TestInConstraints', 'UKN', 'RN', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
insert into dbo.GenShapeGeography (SHG_PK, SHG_Name, SHG_Type, SHG_ParentTableCode, SHG_SystemCreateTimeUtc, SHG_SystemCreateUser, SHG_SystemLastEditTimeUtc, SHG_SystemLastEditUser)
values ('{Guid.NewGuid()}', 'TestOutConstraints', 'UKN', 'AA', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
"
;
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
