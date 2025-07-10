using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintCPN_ParentTableCode))]
	sealed class ConstraintCPN_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintCPN_ParentTableCode>
	{
		protected override string TableName => "CusPerson";

		protected override string TablePrefix => "CPN";

		protected override string[] SupportedParentPrefixes => new[] { "AMA", "JE" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[CPN_SystemCreateTimeUtc]", "[CPN_SystemLastEditTimeUtc]" };

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CusPersonSchema.Constants.TableName, "Constraint_CPN_ParentTableCode");

			var sqlText = @"
DECLARE @Person1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @Person2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @Person3PK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbPerson(PER_PK, PER_IsActive, PER_IsValid, PER_FullName, PER_PreferredLanguage, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser)
VALUES(@Person1PK, 1, 1, 'FN', 'EN', GetUTCDate(), '~BP', GetUTCDate(), '~BP'),
(@Person2PK, 1, 1, 'FN2', 'EN', GetUTCDate(), '~BP', GetUTCDate(), '~BP'),
(@Person3PK, 1, 1, 'FN3', 'EN', GetUTCDate(), '~BP', GetUTCDate(), '~BP');

DECLARE @CusPersonValid1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @CusPersonValid2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @CusPersonInvalidPK UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.CusPerson(CPN_PK, CPN_PER_Person, CPN_ParentTableCode, CPN_ParentID, CPN_IsPassenger, CPN_SystemCreateTimeUtc, CPN_SystemCreateUser, CPN_SystemLastEditTimeUtc, CPN_SystemLastEditUser)
VALUES (@CusPersonValid1PK, @Person1PK, 'JE', NEWID(), 0, GetUTCDate(), '~BP', GetUTCDate(), '~BP'),
(@CusPersonValid2PK, @Person2PK, 'AMA', NEWID(), 0, GetUTCDate(), '~BP', GetUTCDate(), '~BP'),
(@CusPersonInvalidPK, @Person3PK, 'NA', NEWID(), 0, GetUTCDate(), '~BP', GetUTCDate(), '~BP');

INSERT INTO dbo.CusPersonCountry(CPC_PK, CPC_CPN_Person, CPC_Type, CPC_Value, CPC_RN_NKCountry, CPC_SystemCreateTimeUtc, CPC_SystemCreateUser, CPC_SystemLastEditTimeUtc, CPC_SystemLastEditUser)
VALUES (NEWID(), @CusPersonValid1PK, 'OOC', 'H', 'ZA', GetUTCDate(), '~BP', GetUTCDate(), '~BP'),
(NEWID(), @CusPersonInvalidPK, 'OOC', 'H', 'ZA', GetUTCDate(), '~BP', GetUTCDate(), '~BP'),
(NEWID(), @CusPersonInvalidPK, 'OOC', 'H', 'AU', GetUTCDate(), '~BP', GetUTCDate(), '~BP'); 
";
			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
