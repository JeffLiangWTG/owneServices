using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterData;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.MasterData.Testing
{
	[TestedType(typeof(RemovePatternMatchingDataForDummyContacts))]
	public class RemovePatternMatchingDataForDummyContactsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingName WHERE PMN_HashedValue = '1566845912'"));
			AssertEquals(2, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PatternMatchingName WHERE PMN_HashedValue = '1234567890'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RemovePatternMatchingDataForDummyContacts();

		protected override void PrepareTestData()
		{
			var orgPk = Guid.NewGuid();
			var perPk1 = Guid.NewGuid();
			var perPk2 = Guid.NewGuid();
			var contactPk1 = Guid.NewGuid();
			var contactPk2 = Guid.NewGuid();
			var sql = $@"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser, OH_SystemCreateTimeUtc, OH_SystemCreateUser) VALUES ('{orgPk}', 'ORG01', 'ORGHEADER 1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser, PER_SystemCreateTimeUtc, PER_SystemCreateUser)
	VALUES
	('{perPk1}', 'PER 1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{perPk2}', 'PER 2', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_PER, OC_ContactName, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser, OC_SystemCreateTimeUtc, OC_SystemCreateUser)
	VALUES
	('{contactPk1}', '{orgPk}', '{perPk1}', 'DUMMY CONTACT TO SUPPRESS DOCS', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{contactPk2}', '{orgPk}', '{perPk2}', 'CONTACT 2', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.PatternMatchingName (PMN_PK, PMN_ParentTableCode, PMN_ParentId, PMN_PER, PMN_HashedValue, PMN_SystemLastEditTimeUtc, PMN_SystemLastEditUser, PMN_SystemCreateTimeUtc, PMN_SystemCreateUser)
	VALUES
	('{Guid.NewGuid()}', 'OC', '{contactPk1}', '{perPk1}', '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PER', '{perPk1}', '{perPk1}', '1566845912', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'OC', '{contactPk2}', '{perPk2}', '1234567890', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', 'PER', '{perPk2}', '{perPk2}', '1234567890', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
