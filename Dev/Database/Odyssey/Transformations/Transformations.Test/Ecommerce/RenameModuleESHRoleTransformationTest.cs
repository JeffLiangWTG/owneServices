using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(RenameModuleESHRoleTransformation))]
	public class RenameModuleESHRoleTransformationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RenameModuleESHRoleTransformation();

		protected override void AssertTransformationResults()
		{
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'eCommerceShipperAndNEOPortalsViewer'"));
		}

		protected override void PrepareTestData()
		{
			var groupRole_PK1 = Guid.NewGuid();
			var group_FK = "0c7bb530-621f-426c-b90b-33ccd12de20e";

			Db.Connection.ExecuteNonQuery($@"
				INSERT INTO [dbo].[GlbGroupRole]
					([GGR_PK]
					,[GGR_RoleName]
					,[GGR_GG_Group]
					,[GGR_AutoVersion]
					,[GGR_SystemCreateTimeUtc]
					,[GGR_SystemCreateUser]
					,[GGR_SystemLastEditTimeUtc]
					,[GGR_SystemLastEditUser])
				VALUES
					('{groupRole_PK1.ToString()}'
					,'module-ESH'
					,'{group_FK}'
					,0
					,'2023-10-04 04:41:00'
					,'E'
					,'2023-10-04 04:41:00'
					,'E');
			");
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'module-ESH'"));
		}
	}
}
