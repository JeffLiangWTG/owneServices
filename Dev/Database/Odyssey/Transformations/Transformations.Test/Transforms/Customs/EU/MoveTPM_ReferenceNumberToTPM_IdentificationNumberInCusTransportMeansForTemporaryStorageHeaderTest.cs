using System;
using System.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;

[TestedType(typeof(MoveTPM_ReferenceNumberToTPM_IdentificationNumberInCusTransportMeansForTemporaryStorageHeader))]
sealed class MoveTPM_ReferenceNumberToTPM_IdentificationNumberInCusTransportMeansForTemporaryStorageHeaderTest : DataTransformationTestCase
{
	public override string[] expectedIndex => new string[]
	{
			"NONCLUSTERED INDEX [IX_MoveTPM_ReferenceNumberToTPM_IdentificationNumber_CusTransportMeansInTemporaryStorageHeader] ON [dbo].[CusTransportMeans] ([TPM_ParentTableCode], [TPM_ReferenceNumber]) INCLUDE ([TPM_ParentID]) WHERE ([TPM_ParentTableCode]='AMA' AND [TPM_ReferenceNumber]<>'') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
	};

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new MoveTPM_ReferenceNumberToTPM_IdentificationNumberInCusTransportMeansForTemporaryStorageHeader();
	}

	protected override void PrepareTestData()
	{
		var sql = $@"
DECLARE @CompanyPK	UNIQUEIDENTIFIER = NEWID(),
		@BranchPK	UNIQUEIDENTIFIER = NEWID(),
		@amaPK1		UNIQUEIDENTIFIER = '{amaPK1}',
		@amaPK2		UNIQUEIDENTIFIER = '{amaPK2}',
		@amaPK3		UNIQUEIDENTIFIER = '{amaPK3}',
		@amaPK4 	UNIQUEIDENTIFIER = '{amaPK4}',
		@tpmPK1		UNIQUEIDENTIFIER = '{tpmPK1}',
		@tpmPK2		UNIQUEIDENTIFIER = '{tpmPK2}',
		@tpmPK3		UNIQUEIDENTIFIER = '{tpmPK3}',
		@tpmPK4		UNIQUEIDENTIFIER = '{tpmPK4}'

INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemCreateUser, GC_SystemLastEditUser)
VALUES(@CompanyPK, 'IT', 'EUR', 'ITC', 'AU company', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')

INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemCreateUser, GB_SystemLastEditUser)
VALUES(@BranchPK, @CompanyPK, 'MIB', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')

INSERT INTO dbo.AsycudaManifestHeader(AMA_PK, AMA_GB, AMA_JobReference, AMA_IsActive, AMA_RN_NKCountry, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey)
VALUES(@amaPK1, @BranchPK, 'TSD0000123', 1, 'IT', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP', '1'),
	(@amaPK2, @BranchPK, 'TSD0000456', 1, 'IT', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP', '2'),
	(@amaPK3, @BranchPK, 'TSD0000789', 1, 'IT', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP', '3'),
	(@amaPK4, @BranchPK, 'TSD0000368', 1, 'IT', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP', '4')

INSERT INTO dbo.CusTransportMeans(TPM_PK, TPM_ParentID, TPM_ParentTableCode, TPM_ReferenceNumber, TPM_IdentificationNumber, TPM_SystemCreateTimeUtc, TPM_SystemLastEditTimeUtc, TPM_SystemCreateUser, TPM_SystemLastEditUser) 
VALUES(@tpmPK1, @amaPK1, 'AMA', '20','', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP'),
	(@tpmPK2, @amaPK2, 'AMA', '40','80', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP'),
	(@tpmPK3, @amaPK3, 'AMA', '','', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP'),
	(@tpmPK4, @amaPK4, 'AMA', '','60', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')";

		using var cmd = TestConnection.Command(sql);
		cmd.ExecuteNonQuery();
	}

	protected override void AssertTransformationResults()
	{
		CombineAssertions(() =>
		{
			AssertTPM_ReferenceNumberAndTPM_IdentificationNumber(tpmPK1, "20");
			AssertTPM_ReferenceNumberAndTPM_IdentificationNumber(tpmPK2, "40");
			AssertTPM_ReferenceNumberAndTPM_IdentificationNumber(tpmPK3, "");
			AssertTPM_ReferenceNumberAndTPM_IdentificationNumber(tpmPK4, "60");
		});
	}

	void AssertTPM_ReferenceNumberAndTPM_IdentificationNumber(Guid tpmPK, string expectedIdentificationNumber)
	{
		const string sqlQuery = "SELECT TPM_ReferenceNumber, TPM_IdentificationNumber FROM dbo.CusTransportMeans WHERE TPM_PK = @PK";

		using var command = TestConnection.Command(sqlQuery);
		command.AddParameter("@PK", SqlDbType.UniqueIdentifier, tpmPK);

		using var reader = command.ExecuteReader();
		if (reader.Read())
		{
			var referenceNumber = reader["TPM_ReferenceNumber"].ToString();
			var identificationNumber = reader["TPM_IdentificationNumber"].ToString();

			AssertEquals($"{tpmPK} - TPM_ReferenceNumber", "", referenceNumber);
			AssertEquals($"{tpmPK} - TPM_IdentificationNumber", expectedIdentificationNumber, identificationNumber);
		}
	}

	Guid amaPK1 = Guid.NewGuid();
	Guid amaPK2 = Guid.NewGuid();
	Guid amaPK3 = Guid.NewGuid();
	Guid amaPK4 = Guid.NewGuid();
	Guid tpmPK1 = Guid.NewGuid();
	Guid tpmPK2 = Guid.NewGuid();
	Guid tpmPK3 = Guid.NewGuid();
	Guid tpmPK4 = Guid.NewGuid();
}
