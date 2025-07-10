using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(UpdateNEOERequestRolesToNotDefault))]
	public class UpdateNEOERequestRolesToNotDefaultTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			//group
			AssertEquals(true, Db.Connection.Exists($@"FROM dbo.GlbGroup WHERE GG_Code = 'NEOROLES' AND GG_Type = 'ORG'"));

			//role
			AssertRoleDoesntExist("NEOROLES", "eRequestViewAll");
			AssertRoleDoesntExist("NEOROLES", "eRequestViewOwn");
			AssertRole("ERQVIEWALL", "eRequestViewAll");
			AssertRole("ERQVIEWOWN", "eRequestViewOwn");
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateNEOERequestRolesToNotDefault();
		}

		protected override void PrepareTestData()
		{
			var creator = new TransformationTestDataCreator();
			var neoGroupGuid = creator.GetGlbGroupGuidFromCode("NEOROLES");

			AssertNotNull(neoGroupGuid);
			AssertNotEquals("Empty Guid", Guid.Empty, neoGroupGuid);

			Db.Connection.ExecuteNonQuery($"DELETE dbo.GlbGroupRole WHERE GGR_RoleName IN ('eRequestViewAll','eRequestViewOwn') AND GGR_GG_Group = '{neoGroupGuid}';");

			creator.CreateGlbGroupRole("eRequestViewAll", neoGroupGuid);
			creator.CreateGlbGroupRole("eRequestViewOwn", neoGroupGuid);

			AssertRole("NEOROLES", "eRequestViewAll");
			AssertRole("NEOROLES", "eRequestViewOwn");
		}

		void AssertRoleDoesntExist(string groupCode, string roleName)
		{
			AssertRole(groupCode, roleName, false);
		}

		void AssertRole(string groupCode, string roleName, bool exists = true)
		{
			AssertEquals(exists,
			Db.Connection.Exists($@"
FROM dbo.GlbGroupRole
JOIN dbo.GlbGroup ON GGR_GG_Group = GG_PK
WHERE GGR_RoleName = '{roleName}' AND GG_Code = '{groupCode}'
")
			);
		}
	}
}
