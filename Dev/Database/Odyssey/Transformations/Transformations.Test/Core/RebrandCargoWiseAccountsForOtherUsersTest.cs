using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core;

[TestedType(typeof(RebrandCargoWiseAccountsForOtherUsers))]
class RebrandCargoWiseAccountsForOtherUsersTest : DataTransformationTestCase
{
	protected override void PrepareTestData()
	{
		// The transform has already been run by the time this test is executed, so we need to put the usernames back to the old names
		TestConnection.ExecuteNonQuery("""
			UPDATE dbo.GlbStaff
			SET
				GS_LoginName = 'CW1Web',
				GS_FullName = 'CargoWise One Web',
				GS_SystemLastEditUser = '~BP',
				GS_SystemLastEditTimeUtc = GETUTCDATE()
			WHERE
				GS_Code = 'ZZ';

			UPDATE dbo.GlbStaff
			SET
				GS_LoginName = 'CW1Service',
				GS_FullName = 'CargoWise One Service',
				GS_SystemLastEditUser = '~BP',
				GS_SystemLastEditTimeUtc = GETUTCDATE()
			WHERE
				GS_Code = '~BP';

			UPDATE dbo.GlbStaff
			SET
				GS_LoginName = 'CW1AutoDataImport',
				GS_SystemLastEditUser = '~BP',
				GS_SystemLastEditTimeUtc = GETUTCDATE()
			WHERE
				GS_Code = '~AD';
			""");
	}

	protected override DataTransformation GetNewTestTransformationInstance()
		=> new RebrandCargoWiseAccountsForOtherUsers();

	protected override void AssertPreConditions()
	{
		var cw1WebFullName = TestConnection.ExecuteScalar<string>("SELECT GS_FullName FROM dbo.GlbStaff WHERE GS_Code = 'ZZ' AND GS_LoginName = 'CW1Web' AND GS_IsSystemAccount = 1");
		AssertEquals("CargoWise One Web should be existed", "CargoWise One Web", cw1WebFullName);

		var cw1ServiceFullName = TestConnection.ExecuteScalar<string>("SELECT GS_FullName FROM dbo.GlbStaff WHERE GS_Code = '~BP' AND GS_LoginName = 'CW1Service' AND GS_IsSystemAccount = 1");
		AssertEquals("CargoWise One Service should be existed", "CargoWise One Service", cw1ServiceFullName);

		var cw1ADFullName = TestConnection.ExecuteScalar<string>("SELECT GS_FullName FROM dbo.GlbStaff WHERE GS_Code = '~AD' AND GS_LoginName = 'CW1AutoDataImport' AND GS_IsSystemAccount = 1");
		AssertEquals("Automated Data Import should be existed", "Automated Data Import", cw1ADFullName);
	}

	override protected void AssertTransformationResults()
	{
		AssertQueryResult(0, "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1Web'");
		AssertQueryResult(0, "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1Service'");
		AssertQueryResult(0, "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1AutoDataImport'");

		AssertQueryResult(1, "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = 'ZZ'  AND GS_LoginName = 'CWWeb' AND GS_FullName = 'CargoWise Web'");
		AssertQueryResult(1, "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = '~BP' AND GS_LoginName = 'CWService' AND GS_FullName = 'CargoWise Service'");
		AssertQueryResult(1, "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = '~AD' AND GS_LoginName = 'CWAutoDataImport' AND GS_FullName = 'Automated Data Import'");

		void AssertQueryResult(int expectedCount, string query)
		{
			var resultCount = TestConnection.ExecuteScalar<int>(query);
			AssertEquals($"Expected {expectedCount} result(s), get {resultCount} instead; Query: {query}", expectedCount, resultCount);
		}
	}
}
