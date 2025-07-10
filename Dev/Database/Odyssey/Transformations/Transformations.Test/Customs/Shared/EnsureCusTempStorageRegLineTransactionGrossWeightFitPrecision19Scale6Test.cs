using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared;

[TestedType(typeof(EnsureCusTempStorageRegLineTransactionGrossWeightFitPrecision19Scale6))]
sealed class EnsureCusTempStorageRegLineTransactionGrossWeightFitPrecision19Scale6Test : DataTransformationTestCase
{
	public override string[] expectedIndex => ["NONCLUSTERED INDEX [_WTG__Ensure CusTempStorageRegLineTransaction.SRT_GrossWeight fits type DECIMAL(19, 6)_1] ON [dbo].[CusTempStorageRegLineTransaction] ([SRT_GrossWeight]) WHERE ([SRT_GrossWeight]>(9999999999999.)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"];

	protected override void AssertTransformationResults()
	{
		CombineAssertions("CusTempStorageRegLineTransaction(s)", () =>
		{
			AssertEquals("Line [TRN001] > SRT_GrossWeight", 9999999999999m, GetGrossWeight("TRN001"));
			AssertEquals("Line [TRN002] > SRT_GrossWeight", 9999999999999m, GetGrossWeight("TRN002"));
			AssertEquals("Line [TRN003] > SRT_GrossWeight", 9999999999999m, GetGrossWeight("TRN003"));
			AssertEquals("Line [TRN004] > SRT_GrossWeight", 9999999999999m, GetGrossWeight("TRN004"));
			AssertEquals("Line [TRN005] > SRT_GrossWeight", 9999999999998.99999m, GetGrossWeight("TRN005"));
		});

		AssertNoExceptionThrown("Should be able to alter SRT_GrossWeight to DECIMAL(19,6) after transformation.", () =>
		{
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.CusTempStorageRegLineTransaction ALTER COLUMN SRT_GrossWeight DECIMAL(19,6)");
		});
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		new DbColumnDependencyRemover(CusTempStorageRegLineTransactionSchema.Constants.TableName, CusTempStorageRegLineTransactionSchema.Constants.SRT_GrossWeight)
			.DropRelateObjects(Db.Connection);

		const string sql = @"

ALTER TABLE dbo.CusTempStorageRegLineTransaction ALTER COLUMN SRT_GrossWeight DECIMAL(19,5);

DECLARE @SRH_PK UNIQUEIDENTIFIER = NEWID();
DECLARE @SRL_PK UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.CusTempStorageRegHeader
	(SRH_PK, SRH_Reference, SRH_AppCode, SRH_SystemCreateTimeUtc, SRH_SystemCreateUser, SRH_SystemLastEditTimeUtc, SRH_SystemLastEditUser)
VALUES
	(@SRH_PK, 'REGH001', 'XXX', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusTempStorageRegLine
	(SRL_PK, SRL_SRH, SRL_LineNumber, SRL_PackagesRemaining, SRL_SystemCreateTimeUtc, SRL_SystemCreateUser, SRL_SystemLastEditTimeUtc, SRL_SystemLastEditUser)
VALUES
	(@SRL_PK, @SRH_PK, 1, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusTempStorageRegLineTransaction
	(SRT_PK, SRT_SRL, SRT_TransactionType, SRT_InternalReferenceNumber, SRT_GrossWeight, SRT_SystemCreateTimeUtc, SRT_SystemCreateUser, SRT_SystemLastEditTimeUtc, SRT_SystemLastEditUser)
VALUES
	(NEWID(), @SRL_PK, 'STA', 'TRN001', 99999999999999, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), @SRL_PK, 'STA', 'TRN002', 99999999999999.99999, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), @SRL_PK, 'STA', 'TRN003', 99999999999999.999994, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), @SRL_PK, 'STA', 'TRN004', 9999999999999, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), @SRL_PK, 'STA', 'TRN005', 9999999999998.99999, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
		TestConnection.ExecuteNonQuery(sql);
	}

	decimal GetGrossWeight(string lineTransactionRefNo)
		=> TestConnection.ExecuteScalar<decimal>($"SELECT SRT_GrossWeight FROM dbo.CusTempStorageRegLineTransaction WHERE SRT_InternalReferenceNumber = '{lineTransactionRefNo}'");

	protected override DataTransformation GetNewTestTransformationInstance() => new EnsureCusTempStorageRegLineTransactionGrossWeightFitPrecision19Scale6();
}
