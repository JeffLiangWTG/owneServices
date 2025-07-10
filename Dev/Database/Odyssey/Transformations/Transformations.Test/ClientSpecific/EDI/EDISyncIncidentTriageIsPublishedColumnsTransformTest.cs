using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(EDISyncIncidentTriageIsPublishedColumnsTransform))]
	public class EDISyncIncidentTriageIsPublishedColumnsTransformTest : DataTransformationTestCase
	{
		const string tableName = "IncidentTriage";

		public void TestTransformWithoutTable()
		{
			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() =>
			{
				transform.Run(TransformationSection.OfflinePostUpgrade, new System.Threading.CancellationToken());
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new EDISyncIncidentTriageIsPublishedColumnsTransform();
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();
			var sql = $"SELECT COUNT(*) FROM dbo.{tableName} WHERE IMT_IsActive != IMT_IsPublishedToAssist";
			var result = Db.Connection.ExecuteScalar<int>(sql);

			AssertEquals(0, result);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			var tableSQL = @$"CREATE TABLE dbo.{tableName}
(
	[IMT_PK] UNIQUEIDENTIFIER NOT NULL,
	[IMT_TriageNumber] VARCHAR(20) NOT NULL DEFAULT '',
	[IMT_IsActive] BIT NOT NULL DEFAULT 0,
	[IMT_IsPublishedToAssist] BIT NOT NULL DEFAULT 0,
	[IMT_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
	[IMT_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[IMT_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
	[IMT_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);";
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, tableName, tableSQL);

			var sql = @$"INSERT INTO dbo.{tableName} (IMT_PK, IMT_TriageNumber, IMT_IsActive, IMT_IsPublishedToAssist, IMT_SystemCreateTimeUtc, IMT_SystemCreateUser, IMT_SystemLastEditTimeUtc, IMT_SystemLastEditUser)
VALUES
(NEWID(), 'TRINTZ00000', 1, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), 'TRINTZ00001', 1, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), 'TRINTZ00002', 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), 'TRINTZ00003', 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
