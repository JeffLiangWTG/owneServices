using System;
using System.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;

[TestedType(typeof(RenameCE_EntryStatusFromNOTToNCNForICS2AsycudaManifestHeader))]
sealed class RenameCE_EntryStatusFromNOTToNCNForICS2AsycudaManifestHeaderTest : DataTransformationTestCase
{
	public override string[] expectedIndex => new string[]
	{
		"NONCLUSTERED INDEX [IX_RenameCE_EntryStatusFromNOTToNCNForICS2AsycudaManifestHeader] ON [dbo].[CusEntryNum] ([CE_ParentTable], [CE_EntryType], [CE_EntryStatus]) INCLUDE ([CE_SystemLastEditTimeUtc], [CE_SystemLastEditUser]) WHERE ([CE_ParentTable]='AsycudaManifestHeader' AND [CE_EntryType]='ASY' AND [CE_EntryStatus]='NOT') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
	};

	protected override DataTransformation GetNewTestTransformationInstance() => new RenameCE_EntryStatusFromNOTToNCNForICS2AsycudaManifestHeader();

	protected override void PrepareTestData()
	{
		var sql = $@"
DECLARE @CompanyPK			UNIQUEIDENTIFIER = NEWID(),
		@BranchPK			UNIQUEIDENTIFIER = NEWID(),
		@amaPK1				UNIQUEIDENTIFIER = '{amaPK1}',
		@amaPK2NotENS		UNIQUEIDENTIFIER = '{amaPK2NotENS}',
		@amaPK3				UNIQUEIDENTIFIER = '{amaPK3}',
		@amaPK4				UNIQUEIDENTIFIER = '{amaPK4}',
		@ablPK1				UNIQUEIDENTIFIER = '{ablPK1}',
		@cenPK1				UNIQUEIDENTIFIER = '{cenPK1}',
		@cenPK2				UNIQUEIDENTIFIER = '{cenPK2}',
		@cenPK3NotASY		UNIQUEIDENTIFIER = '{cenPK3NotASY}',
		@cenPK4NotNOT		UNIQUEIDENTIFIER = '{cenPK4NotNOT}',
		@cenPK5NotManifest	UNIQUEIDENTIFIER = '{cenPK5NotManifest}'

INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemCreateUser, GC_SystemLastEditUser)
VALUES(@CompanyPK, 'DE', 'EUR', 'DDE', 'DE company', GETUTCDATE(), GETUTCDATE(), 'STD', 'STD')

INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemCreateUser, GB_SystemLastEditUser)
VALUES(@BranchPK, @CompanyPK, 'BDE', GETUTCDATE(), GETUTCDATE(), 'STD', 'STD')

INSERT INTO dbo.AsycudaManifestHeader(AMA_PK, AMA_GB, AMA_JobReference, AMA_IsActive, AMA_RN_NKCountry, AMA_ManifestType, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey)
VALUES(@amaPK1, @BranchPK, 'MAN0000123', 1, 'DE', 'ENS', GETUTCDATE(), GETUTCDATE(), 'STD', 'STD', '1'),
	(@amaPK2NotENS, @BranchPK, 'MAN0000456', 1, 'DE', 'ABC', GETUTCDATE(), GETUTCDATE(), 'STD', 'STD', '2'),
	(@amaPK3, @BranchPK, 'MAN0000789', 1, 'DE', 'ENS', GETUTCDATE(), GETUTCDATE(), 'STD', 'STD', '3'),
	(@amaPK4, @BranchPK, 'MAN0000368', 1, 'DE', 'ENS', GETUTCDATE(), GETUTCDATE(), 'STD', 'STD', '4')

INSERT INTO dbo.AsycudaBill(ABL_PK, ABL_AMA, ABL_ClusterKey, ABL_SystemCreateTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditTimeUtc, ABL_SystemLastEditUser)
VALUES(@ablPK1, @amaPK1, 1, GETUTCDATE(), 'STD', GETUTCDATE(), 'STD')

INSERT INTO dbo.CusEntryNum(CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_EntryStatus, CE_EntryType, CE_Category, CE_RN_NKCountryCode, CE_IssueDate, CE_ExpiryDate, CE_EntryIsSystemGenerated, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) 
VALUES(@cenPK1, @amaPK1, 'AsycudaManifestHeader', '1111', 'NOT', 'ASY', 'CUS', 'DE', '2025-05-06', GETUTCDATE(), 0, '2025-05-06', 'STD', '2025-05-06', 'STD'),
	(@cenPK2, @amaPK2NotENS, 'AsycudaManifestHeader', '2222', 'NOT', 'ASY', 'CUS', 'DE', '2025-05-06', GETUTCDATE(), 0, '2025-05-06', 'STD', '2025-05-06', 'STD'),
	(@cenPK3NotASY, @amaPK3, 'AsycudaManifestHeader', '3333', 'NOT', 'ABC', 'CUS', 'DE', '2025-05-06', GETUTCDATE(), 0, '2025-05-06', 'STD', '2025-05-06', 'STD'),
	(@cenPK4NotNOT, @amaPK4, 'AsycudaManifestHeader', '4444', 'REG', 'ASY', 'CUS', 'DE', '2025-05-06', GETUTCDATE(), 0, '2025-05-06', 'STD', '2025-05-06', 'STD'),
	(@cenPK5NotManifest, @ablPK1, 'AsycudaBill', '5555', 'NOT', 'ASY', 'CUS', 'DE', '2025-05-06', GETUTCDATE(), 0, '2025-05-06', 'STD', '2025-05-06', 'STD')";

		using var cmd = TestConnection.Command(sql);
		cmd.ExecuteNonQuery();
	}

	protected override void AssertTransformationResults()
	{
		AssertEntryStatusAndModified(cenPK1, "NCN", true, "Updated");
		AssertEntryStatusAndModified(cenPK2, "NOT", false, "Not updated - not ENS");
		AssertEntryStatusAndModified(cenPK3NotASY, "NOT", false, "Not updated - not ASY");
		AssertEntryStatusAndModified(cenPK4NotNOT, "REG", false, "Not updated - not NOT");
		AssertEntryStatusAndModified(cenPK5NotManifest, "NOT", false, "Not updated - parent not Manifest");
	}

	void AssertEntryStatusAndModified(Guid cenPK, string expectedEntryStatus, bool expectedModified, string message)
	{
		const string sqlQuery = "SELECT CE_EntryStatus, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser FROM dbo.CusEntryNum WHERE CE_PK = @PK";

		using var command = TestConnection.Command(sqlQuery);
		command.AddParameter("@PK", SqlDbType.UniqueIdentifier, cenPK);

		using var reader = command.ExecuteReader();
		if (reader.Read())
		{
			var entryStatus = reader["CE_EntryStatus"].ToString();
			var lastEditedTime = reader["CE_SystemLastEditTimeUtc"];
			var lastEditedUser = reader["CE_SystemLastEditUser"].ToString();

			var unmodifiedTime = new DateTime(2025, 05, 06);

			CombineAssertions(message ,() =>
			{
				AssertEquals("CE_EntryStatus", expectedEntryStatus, entryStatus);
				if (expectedModified)
				{
					AssertNotEquals("CE_SystemLastEditTimeUtc", unmodifiedTime, lastEditedTime);
					AssertEquals("CE_SystemLastEditUser", "E", lastEditedUser);
				}
				else
				{
					AssertEquals("CE_SystemLastEditTimeUtc", unmodifiedTime, lastEditedTime);
					AssertEquals("CE_SystemLastEditUser", "STD", lastEditedUser);
				}
			});
		}
	}

	Guid amaPK1 = Guid.NewGuid();
	Guid amaPK2NotENS = Guid.NewGuid();
	Guid amaPK3 = Guid.NewGuid();
	Guid amaPK4 = Guid.NewGuid();
	Guid ablPK1 = Guid.NewGuid();
	Guid cenPK1 = Guid.NewGuid();
	Guid cenPK2 = Guid.NewGuid();
	Guid cenPK3NotASY = Guid.NewGuid();
	Guid cenPK4NotNOT = Guid.NewGuid();
	Guid cenPK5NotManifest = Guid.NewGuid();
}
