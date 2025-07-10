using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Customs.CA;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(RequeFailedCADMessages))]
	public class RequeFailedCADMessagesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RequeFailedCADMessages();
		}

		protected override void AssertPreConditions()
		{
			var sql = @"SELECT COUNT(*) FROM EDIMessage WHERE EM_Status = 'QUE'";
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					existingQueuedMessagesCount = reader.GetInt32(0);
				}
			}
		}

		protected override void PrepareTestData()
		{
			var sql = @"DECLARE @companyPK UNIQUEIDENTIFIER = NEWID()
DECLARE @branchPK UNIQUEIDENTIFIER = NEWID()
DECLARE @depPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES (@companyPK, 'CA', 'BLO', 'AAA', 'CA company', SYSDATETIME(), 'JYN', SYSDATETIME(), 'JYN');
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES (@branchPK, @companyPK, 'XXX', 'CA', SYSDATETIME(), 'JYN', SYSDATETIME(), 'JYN');
INSERT INTO GlbDepartment(GE_PK, GE_Code, GE_SystemCreateTimeUtc, GE_SystemCreateUser, GE_SystemLastEditTimeUtc, GE_SystemLastEditUser)
VALUES(@depPK, 'DP1', SYSDATETIME(), 'JYN', SYSDATETIME(), 'JYN');

DECLARE @declarationPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_MessageType, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@declarationPK, @branchPK, @companyPK, 1, 'Ref1', 'CA', 'IMP', SYSDATETIME(), 'JYN', SYSDATETIME(), 'JYN');

DECLARE @entryPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusEntryHeader(CH_PK, CH_MessageType, CH_DataModel, CH_ClusterKey, CH_JE, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@entryPK, 'CAD', 'CA', 1, @declarationPK, SYSDATETIME(), 'JYN', SYSDATETIME(), 'JYN');

INSERT INTO EDIMessage(EM_PK, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_ApplicationCode, EM_MessageType, EM_ReceiveTransmit, EM_Status, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
VALUES (NEWID(), @branchPK, @depPK, @entryPK, 'EDIMessage', 'CAI', 'CAD', 'RCV', 'RCV', '2024-11-22', 'JYN', '2024-11-22', 'JYN')
, (NEWID(), @branchPK, @depPK, @entryPK, 'EDIMessage', 'CAI', 'CAD', 'RCV', 'FAL', '2024-11-24', 'JYN', '2024-11-24', 'JYN')
, (NEWID(), @branchPK, @depPK, @entryPK, 'EDIMessage', 'CAI', 'CAD', 'RCV', 'QUE', '2024-11-23', 'JYN', '2024-11-23', 'JYN');
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			var count = 0;
			var sql = @"SELECT COUNT(*) FROM EDIMessage WHERE EM_Status = 'QUE'";
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					count = reader.GetInt32(0);
				}
			}
			AssertEquals(existingQueuedMessagesCount + 1, count);
		}

		int existingQueuedMessagesCount;
	}
}
