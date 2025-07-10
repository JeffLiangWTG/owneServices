using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Customs.BR.Testing
{
	[TestedType(typeof(RenameSystemUtilizationFeeOverrideToUtilizationFeeOverride))]
	class RenameSystemUtilizationFeeOverrideToUtilizationFeeOverrideTest : DataTransformationTestCase
	{
		Guid BRCeiPK = Guid.NewGuid();
		Guid BRCeiPK_NoBrazil = Guid.NewGuid();
		Guid BRCeiPK_NoSystem = Guid.NewGuid();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Rename SystemUtilizationFeeOverride To UtilizationFeeOverride_1] ON [dbo].[CusEntryInstruction] ([CEI_DataModel]) INCLUDE ([CEI_AddInfo], [CEI_SystemLastEditTimeUtc], [CEI_SystemLastEditUser]) WHERE ([CEI_DataModel]='BR') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance() => new RenameSystemUtilizationFeeOverrideToUtilizationFeeOverride();

		protected override void AssertTransformationResults()
		{
			var dt = new DataTable("CusEntryInstruction");
			dt.Load(Db.Connection.Command("SELECT CEI_PK, CEI_AddInfo FROM dbo.CusEntryInstruction").ExecuteReader());
			AssertEquals(1, dt.Select($"CEI_PK = '{BRCeiPK}' AND CEI_AddInfo = 'UtilizationFeeOverride=10*LegalDocument=10*TypeOfOperationExport=1001'").Length);
			AssertEquals(1, dt.Select($"CEI_PK = '{BRCeiPK_NoBrazil}' AND CEI_AddInfo = 'SystemUtilizationFeeOverride=10*LegalDocument=10*TypeOfOperationExport=1001'").Length);
			AssertEquals(1, dt.Select($"CEI_PK = '{BRCeiPK_NoSystem}' AND CEI_AddInfo = 'UtilizationFeeOverride=10*LegalDocument=10*TypeOfOperationExport=1001'").Length);
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
DECLARE @BRCompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode,GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@BRCompanyPK, 'DBR', 'BR company', 'BR', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

DECLARE @BRBranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@BRBranchPK, @BRCompanyPK, 'BBR', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

DECLARE @BRDecPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@BRDecPK, @BRBranchPK, @BRCompanyPK, 1, 'BR', 'B0001', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_Style, CEI_AddInfo, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
	VALUES (@BRCeiPK, 'BR', @BRDecPK, 1, 'S', 'SystemUtilizationFeeOverride=10*LegalDocument=10*TypeOfOperationExport=1001', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

DECLARE @XXJobPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@XXJobPK, @BRBranchPK, @BRCompanyPK, 2, 'XX', 'B0002', GetUtcDate(), '~XX', GetUtcDate(), '~XX');

INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_Style, CEI_AddInfo, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
	VALUES (@BRCeiPK_NoBrazil, 'XX', @XXJobPK, 1, 'S', 'SystemUtilizationFeeOverride=10*LegalDocument=10*TypeOfOperationExport=1001', GetUtcDate(), '~XX', GetUtcDate(), '~XX');


DECLARE @BRDec2PK UNIQUEIDENTIFIER = NEWID();
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@BRDec2PK, @BRBranchPK, @BRCompanyPK, 3, 'BR', 'B0003', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_Style, CEI_AddInfo, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
	VALUES (@BRCeiPK_NoSystem, 'BR', @BRDec2PK, 1, 'S', 'UtilizationFeeOverride=10*LegalDocument=10*TypeOfOperationExport=1001', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameter("@BRCeiPK", SqlDbType.UniqueIdentifier, BRCeiPK);
				command.AddParameter("@BRCeiPK_NoBrazil", SqlDbType.UniqueIdentifier, BRCeiPK_NoBrazil);
				command.AddParameter("@BRCeiPK_NoSystem", SqlDbType.UniqueIdentifier, BRCeiPK_NoSystem);
				command.ExecuteNonQuery();
			}
		}
	}
}
