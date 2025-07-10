using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.FR;

[TestedType(typeof(RemoveLegacyEntrySnapshot))]
sealed class RemoveLegacyEntrySnapshotTest : DataTransformationTestCase
{
	public override string[] expectedIndex => new string[]
	{
		"NONCLUSTERED INDEX [_WTG__Remove Legacy Entry Snapshot_1] ON [dbo].[CusEntryHeader] ([CH_DataModel]) WHERE ([CH_DataModel]='FR') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		"NONCLUSTERED INDEX [_WTG__Remove Legacy Entry Snapshot_2] ON [dbo].[CusEntrySnapshot] ([CES_MessageType]) WHERE ([CES_MessageType] IN ('DG', 'DI')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
	};

	protected override void AssertTransformationResults()
	{
		var results = new List<Guid>();
		TestConnection.ExecuteReader("SELECT CES_PK FROM dbo.CusEntrySnapshot", reader => results.Add((Guid)reader["CES_PK"]));
		AssertContainsExactElementsInAnyOrder(new[] { frVAAEntrySnapshotPK, frVALEntrySnapshotPK, cnEntrySnapshotPK }, results);
	}

	protected override DataTransformation GetNewTestTransformationInstance() => new RemoveLegacyEntrySnapshot();

	protected override void PrepareTestData()
	{
		frVAAEntrySnapshotPK = Guid.NewGuid();
		frVALEntrySnapshotPK = Guid.NewGuid();
		frDGNewEntrySnapshotPK = Guid.NewGuid();
		frDGLegacyEntrySnapshotPK = Guid.NewGuid();
		frDILegacyEntrySnapshotPK = Guid.NewGuid();
		cnEntrySnapshotPK = Guid.NewGuid();

		var sqlText = @"
DECLARE @frCompanyPK UNIQUEIDENTIFIER = NEWID();
DECLARE @cnCompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany
    (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES
    (@frCompanyPK, 'FR', 'EUR', 'DFR', 'FR company', GETDATE(), '~BP', GETDATE(), '~BP'),
    (@cnCompanyPK, 'CN', 'CNY', 'DCN', 'CN company', GETDATE(), '~BP', GETDATE(), '~BP');

DECLARE @frBranchPK UNIQUEIDENTIFIER = NEWID();
DECLARE @cnBranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbBranch
    (GB_PK, GB_RN_NKCountryCode, GB_Code, GB_GC, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES
    (@frBranchPK, 'FR', 'PAR', @frCompanyPK, GETDATE(), '~BP', GETDATE(), '~BP'),
    (@cnBranchPK, 'CN', 'NJG', @cnCompanyPK, GETDATE(), '~BP', GETDATE(), '~BP');

DECLARE @frDecPK UNIQUEIDENTIFIER = NEWID();
DECLARE @cnDecPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobDeclaration
    (JE_PK, JE_GC, JE_GB, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES
    (@frDecPK, @frCompanyPK, @frBranchPK, 1, 'FR', GETDATE(), '~BP', GETDATE(), '~BP'),
    (@cnDecPK, @cnCompanyPK, @cnBranchPK, 2, 'CN', GETDATE(), '~BP', GETDATE(), '~BP');

DECLARE @frEntryPK UNIQUEIDENTIFIER = NEWID();
DECLARE @cnEntryPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusEntryHeader
	(CH_PK, CH_JE, CH_DataModel, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES
	(@frEntryPK, @frDecPK, 'FR', 1, GETDATE(), '~BP', GETDATE(), '~BP'),
	(@cnEntryPK, @cnDecPK, 'CN', 2, GETDATE(), '~BP', GETDATE(), '~BP');

INSERT INTO dbo.CusEntrySnapshot
	(CES_PK, CES_CH_EntryHeader, CES_MessageType, CES_SnapshotXml, CES_SystemCreateTimeUtc, CES_SystemCreateUser, CES_SystemLastEditTimeUtc, CES_SystemLastEditUser)
VALUES
	(@frDGLegacyEntrySnapshotPK, @frEntryPK, 'DG', '<Entry xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""></Entry>', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@frDGNewEntrySnapshotPK, @frEntryPK, 'DG', '<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""></UniversalShipment>', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@frDILegacyEntrySnapshotPK, @frEntryPK, 'DI', '<Entry xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""></Entry>>', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@frVAAEntrySnapshotPK, @frEntryPK, 'VAA', '<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""></UniversalShipment>', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@frVALEntrySnapshotPK, @frEntryPK, 'VAL', '<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""></UniversalShipment>', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@cnEntrySnapshotPK , @cnEntryPK, 'DG', '<Entry xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""></Entry>', GETDATE(), '~BP', GETDATE(), '~BP');
";
		var cmd = Db.Connection.Command(sqlText);
		cmd.AddParameter("@frVAAEntrySnapshotPK", SqlDbType.UniqueIdentifier, frVAAEntrySnapshotPK);
		cmd.AddParameter("@frVALEntrySnapshotPK", SqlDbType.UniqueIdentifier, frVALEntrySnapshotPK);
		cmd.AddParameter("@frDGNewEntrySnapshotPK", SqlDbType.UniqueIdentifier, frDGNewEntrySnapshotPK);
		cmd.AddParameter("@frDGLegacyEntrySnapshotPK", SqlDbType.UniqueIdentifier, frDGLegacyEntrySnapshotPK);
		cmd.AddParameter("@frDILegacyEntrySnapshotPK", SqlDbType.UniqueIdentifier, frDILegacyEntrySnapshotPK);
		cmd.AddParameter("@cnEntrySnapshotPK", SqlDbType.UniqueIdentifier, cnEntrySnapshotPK);

		cmd.ExecuteNonQuery();
	}

	Guid frVAAEntrySnapshotPK;
	Guid frVALEntrySnapshotPK;
	Guid frDGNewEntrySnapshotPK;
	Guid frDGLegacyEntrySnapshotPK;
	Guid frDILegacyEntrySnapshotPK;
	Guid cnEntrySnapshotPK;
}
