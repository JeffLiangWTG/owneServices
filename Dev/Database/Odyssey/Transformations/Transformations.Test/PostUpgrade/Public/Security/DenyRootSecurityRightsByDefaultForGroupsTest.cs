using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;
namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.Security.Testing
{
	abstract class DenyRootSecurityRightsByDefaultForGroupsTest : DataTransformationTestCase
	{
		const string groupCode1 = "XDE";
		const string groupCode2 = "XAL";
		const string groupCode3 = "XNO";
		const string groupCode4 = "ORG";

		Guid groupPK1 = Guid.NewGuid(); //explicitly denied
		Guid groupPK2 = Guid.NewGuid(); //explicitly allowed
		Guid groupPK3 = Guid.NewGuid(); //nothing
		Guid groupPK4 = Guid.NewGuid(); //GG_Type = ORG

		protected override void PrepareTestData()
		{
			var transformation = (DenyRootSecurityRightsByDefaultForGroups)TransformationToTest;

			var sql = string.Format(
@"
INSERT INTO dbo.GlbGroup (GG_PK, GG_Code) VALUES ('{0}', '{3}');
INSERT INTO dbo.GlbGroup (GG_PK, GG_Code) VALUES ('{1}', '{4}');
INSERT INTO dbo.GlbGroup (GG_PK, GG_Code) VALUES ('{2}', '{5}');
INSERT INTO dbo.GlbGroup (GG_PK, GG_Code, GG_Type) VALUES ('{6}', '{7}', 'ORG');",
				groupPK1,
				groupPK2,
				groupPK3,
				groupCode1,
				groupCode2,
				groupCode3,
				groupPK4,
				groupCode4);

			var insertSecurityFormat = @"
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityRight, GU_GS, GU_GG, GU_IsValid, GU_SecurityItemIsAllowed) VALUES (NEWID(), '{0}', NULL, '{1}', 1, {2})";
			foreach (var securityRight in transformation.SecurityRights)
			{
				sql += string.Format(insertSecurityFormat, securityRight, groupPK1, 0);
				sql += string.Format(insertSecurityFormat, securityRight, groupPK2, 1);
			}

			using (var command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			var transformation = (DenyRootSecurityRightsByDefaultForGroups)TransformationToTest;
			foreach (string securityRight in transformation.SecurityRights)
			{
				ExecuteAndAssert(securityRight, groupPK1, groupCode1, false);
				ExecuteAndAssert(securityRight, groupPK2, groupCode2, true);
				ExecuteAndAssert(securityRight, groupPK3, groupCode3, false);
				AssertEquals(null, Execute(securityRight, groupPK4));
			}
		}

		void ExecuteAndAssert(string securityRight, Guid groupPK, string groupCode, bool expectedIsAllowed)
		{
			var message = string.Format("Expected right {0} for group {1} to {2}", securityRight, groupCode, expectedIsAllowed ? "be allowed, but was not." : "not be allowed, but was.");
			var isAllowed = (bool)Execute(securityRight, groupPK);
			AssertEquals(message, expectedIsAllowed, isAllowed);
		}

		object Execute(string securityRight, Guid groupPK)
		{
			var sql = string.Format("SELECT GU_SecurityItemIsAllowed FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{0}' AND GU_GS IS NULL AND GU_GG = '{1}'", securityRight, groupPK);
			using (var command = TestConnection.Command(sql))
			{
				return command.ExecuteScalar();
			}
		}
	}

	[TestedType(typeof(DenyRootSecurityRightsByDefaultForGroups))]
	class DenyRootSecurityRightsByDefaultForGroupsSubClassTest : DenyRootSecurityRightsByDefaultForGroupsTest
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new DenyRootSecurityRightsByDefaultForGroupsSubClass();

		class DenyRootSecurityRightsByDefaultForGroupsSubClass : DenyRootSecurityRightsByDefaultForGroups
		{
			public override IEnumerable<string> SecurityRights => new[] { "SecurityRight1", "SecurityRight2", "SecurityRight3" };
		}
	}
}
