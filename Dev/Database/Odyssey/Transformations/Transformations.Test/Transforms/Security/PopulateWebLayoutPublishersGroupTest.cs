using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(PopulateWebLayoutPublishersGroup))]
	public class PopulateWebLayoutPublishersGroupTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertGroup("WEBLAYOUTPUB", "Web Layout Publishers", true);

			AssertRole("WEBLAYOUTPUB", "weblayoutpublisher");

			AssertGlbGroupOrgLink("WEBLAYOUTPUB", "TEST_ORG_1", true, "OrgLink should exist when both org and contact has security right");
			AssertGlbGroupOrgLink("WEBLAYOUTPUB", "TEST_ORG_2", false, "OrgLink shouldnt exist when no contacts have security right");
			AssertGlbGroupOrgLink("WEBLAYOUTPUB", "TEST_ORG_3", true, "OrgLink should exist when contact has overridden security right");

			AssertGlbGroupOrgContactLink("WEBLAYOUTPUB", "TEST_ORG_1.Contact1", true);
			AssertGlbGroupOrgContactLink("WEBLAYOUTPUB", "TEST_ORG_2.Contact2", false);
			AssertGlbGroupOrgContactLink("WEBLAYOUTPUB", "TEST_ORG_3.Contact3", true);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateWebLayoutPublishersGroup();
		}

		protected override void PrepareTestData()
		{
			CleanUpTestData();
			AddTestData();
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(false, DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"));
		}

		void AssertGroup(string code, string desc, bool isSystemDefined)
		{
			AssertEquals(true,
			Db.Connection.Exists($@"
FROM
dbo.GlbGroup 
WHERE GG_Code = '{code}'
AND GG_Desc = '{desc}'
AND GG_Type = 'ORG'
AND GG_IsSystemDefined = '{(isSystemDefined ? 1 : 0)}'
AND GG_IsSecurityEnabled = 0
AND GG_IsValid = 1
AND GG_IsActive = 1
")
			);
		}

		void AssertRole(string groupCode, string roleName)
		{
			AssertEquals(
				true,
				Db.Connection.Exists($@"
FROM dbo.GlbGroupRole
JOIN dbo.GlbGroup ON GGR_GG_Group = GG_PK
WHERE GGR_RoleName = '{roleName}' AND GG_Code = '{groupCode}'
")
			);
		}

		void AssertGlbGroupOrgLink(string groupCode, string orgCode, bool expected, string message)
		{
			AssertEquals(
				message,
				expected,
				Db.Connection.Exists($@"
FROM
dbo.GlbGroupOrgLink
JOIN dbo.OrgHeader ON OH_PK = GOK_OH_Org
JOIN dbo.GlbGroup ON GOK_GG_Group = GG_PK
WHERE GG_Code = '{groupCode}' AND OH_Code = '{orgCode}'
")
			);
		}

		void AssertGlbGroupOrgContactLink(string groupCode, string contactName, bool expected)
		{
			AssertEquals(
				expected,
				Db.Connection.Exists($@"
FROM
dbo.GlbGroupOrgContactLink
JOIN dbo.OrgContact ON OC_PK = GCK_OC_Contact
JOIN dbo.GlbGroup ON GCK_GG_Group = GG_PK
WHERE GG_Code = '{groupCode}' AND OC_ContactName = '{contactName}'
")
			);
		}

		void CleanUpTestData()
		{
			Db.Connection.ExecuteNonQuery(@"
DELETE dbo.GlbGroupRole
WHERE GGR_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE dbo.GlbSecurity
WHERE GU_GG IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE dbo.GlbGroupOrgLink
WHERE GOK_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE dbo.GlbGroupOrgContactLink
WHERE GCK_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN';");
		}

		void AddTestData()
		{
			var creator = new TransformationTestDataCreator();

			var org1 = creator.CreateOrgHeader("TEST_ORG_1", "TEST_ORG_1");
			var org2 = creator.CreateOrgHeader("TEST_ORG_2", "TEST_ORG_2");
			var org3 = creator.CreateOrgHeader("TEST_ORG_3", "TEST_ORG_3");

			var contact1 = creator.CreateContact("TEST_ORG_1.Contact1", org1);
			var contact2 = creator.CreateContact("TEST_ORG_2.Contact2", org2);
			var contact3 = creator.CreateContact("TEST_ORG_3.Contact3", org3);

			var setContactsActiveQuery = $@"
UPDATE dbo.OrgContact
SET OC_IsActive = 1,
	OC_WebAccessEnabled = 1,
	OC_SystemLastEditTimeUtc = GETUTCDATE(), -- test fails without these being added manually but it's not actually used
	OC_SystemLastEditUser = 'E'
WHERE OC_PK IN ('{contact1}','{contact2}','{contact3}')";

			Db.Connection.ExecuteNonQuery(setContactsActiveQuery);

			var orgSecurity1 = creator.CreateOrgSecurity("Web Publish Layouts", org1, true);

			var orgSecurity2 = creator.CreateOrgSecurity("Web Publish Layouts", org2, true);
			creator.CreateOrgSecurityContact(orgSecurity2, contact2, false);

			var orgSecurity3 = creator.CreateOrgSecurity("Web Publish Layouts", org3, false);
			creator.CreateOrgSecurityContact(orgSecurity3, contact3, true);
		}
	}

	[TestedType(typeof(PopulateWebLayoutPublishersGroup))]
	public class PopulateWebLayoutPublishersGroupEDITest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(false, Db.Connection.Exists(" FROM dbo.GlbGroup WHERE GG_Code = 'WEBLAYOUTPUB' "));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateWebLayoutPublishersGroup();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiBilledUsage", "CREATE TABLE dbo.EdiBilledUsage (ID INT)");

			CleanUpTestData();
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(true, DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"));
		}

		void CleanUpTestData()
		{
			Db.Connection.ExecuteNonQuery(@"
DELETE dbo.GlbGroupRole
WHERE GGR_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE dbo.GlbSecurity
WHERE GU_GG IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE dbo.GlbGroupOrgLink
WHERE GOK_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE dbo.GlbGroupOrgContactLink
WHERE GCK_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN');

DELETE FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code <> 'ORGSECROLEADMIN';");
		}
	}
}
