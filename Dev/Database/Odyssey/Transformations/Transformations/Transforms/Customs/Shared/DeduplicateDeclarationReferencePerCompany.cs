namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

class DeduplicateDeclarationReferencePerCompany : DataTransformation
{
	public override string UserDescription => "Ensure JobDeclaration reference is unique per company.";

	public override bool IsRequired => base.IsRequired &&
		DbObjectCreator.TableExists(Db.Connection, StmALogSchema.Constants.TableName)
			&& DbObjectCreator.ColumnExists(Db.Connection, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.Constants.JE_ClusterKey)
			&& DbObjectCreator.ColumnExists(Db.Connection, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.Constants.JE_GC)
			&& DbObjectCreator.ColumnExists(Db.Connection, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.Constants.JE_DeclarationReference);

	protected override void OnlinePreUpgradeTransform()
	{
		DeduplicateDecReferencePerCompanyOnline();
	}

	protected override void OfflinePreUpgradeTransform()
	{
		DeduplicateDecReferencePerCompanyOffline();
	}

	void DeduplicateDecReferencePerCompanyOnline()
	{
		var rowCount = Db.Connection.ExecuteScalar<int>(OnlinePrepareTempTableSql);

		while (rowCount > 0)
		{
			using (var trxManager = Db.Connection.BeginTransactionWithManager())
			{
				rowCount = Db.Connection.ExecuteScalar<int>(OnlineDeduplicateBatchSql);
				trxManager.CommitTransaction();
			}

			if (rowCount > 0)
			{
				manager?.ShowInfoMessage(FormattableString.Invariant($"Deduplicated {rowCount} declarations."));
			}
		}
	}

	void DeduplicateDecReferencePerCompanyOffline()
	{
		Db.Connection.ExecuteNonQuery(OfflineDeduplicateSql);
	}

	string OnlinePrepareTempTableSql => @"
		DROP TABLE IF EXISTS #TempDupDec;
		CREATE TABLE #TempDupDec (ClusterKey INT NOT NULL);

		INSERT #TempDupDec
			SELECT JE_ClusterKey
			FROM (
				SELECT
					JE_ClusterKey,
					RefSequence = ROW_NUMBER() OVER (PARTITION BY JE_GC, JE_DeclarationReference ORDER BY JE_ClusterKey)
				FROM dbo.JobDeclaration) AS NumberedDecPerCompanyAndRef
			WHERE RefSequence > 1;

		SELECT @@ROWCOUNT;";

	string OnlineDeduplicateBatchSql => @"
		DECLARE @DedupedDec TABLE (
			DecPk UNIQUEIDENTIFIER NOT NULL,
			ClusterKey INT NOT NULL,
			NewDecRef VARCHAR(35) NOT NULL,
			OldDecRef VARCHAR(35) NOT NULL
		);

		WITH DedupRef AS (
			SELECT TOP (100)
				ClusterKey,
				Suffix = ':' + CONVERT(VARCHAR(10), ClusterKey)
			FROM #TempDupDec
		)

		UPDATE JobDec
			SET JobDec.JE_DeclarationReference = LEFT(JobDec.JE_DeclarationReference, 35 - LEN(DedupRef.Suffix)) + DedupRef.Suffix,
				JE_SystemLastEditTimeUtc = GETUTCDATE(),
				JE_SystemLastEditUser = 'E'
			OUTPUT
				INSERTED.JE_PK,
				INSERTED.JE_ClusterKey,
				INSERTED.JE_DeclarationReference,
				DELETED.JE_DeclarationReference
				INTO @DedupedDec
		FROM
			dbo.JobDeclaration JobDec
			INNER JOIN DedupRef ON DedupRef.ClusterKey = JobDec.JE_ClusterKey;

		INSERT dbo.StmALog (SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_GS_NKUser, SL_EventTime)
			SELECT
				'JobDeclaration',
				DecPk,
				'Reference Number Changed: [' + OldDecRef + '] > [' + NewDecRef + ']',
				'EDT',
				'E',
				GETDATE()
			FROM
				@DedupedDec;

		DELETE #TempDupDec WHERE ClusterKey IN (SELECT ClusterKey FROM @DedupedDec);

		SELECT @@ROWCOUNT;";

	string OfflineDeduplicateSql => @"
		DECLARE @DedupedDec TABLE (
			DecPk UNIQUEIDENTIFIER NOT NULL,
			NewDecRef VARCHAR(35) NOT NULL,
			OldDecRef VARCHAR(35) NOT NULL
		);

		WITH
			OrderedDecRef AS (
				SELECT
					JE_ClusterKey,
					RefSequence = ROW_NUMBER() OVER (PARTITION BY JE_GC, JE_DeclarationReference ORDER BY JE_ClusterKey)
				FROM dbo.JobDeclaration
			),
			DedupRef AS (
				SELECT
					JE_ClusterKey,
					Suffix = ':' + CONVERT(VARCHAR(10), JE_ClusterKey)
				FROM OrderedDecRef
				WHERE RefSequence > 1
			)

		UPDATE JobDec
			SET
				JobDec.JE_DeclarationReference = LEFT(JobDec.JE_DeclarationReference, 35 - LEN(DedupRef.Suffix)) + DedupRef.Suffix,
				JE_SystemLastEditTimeUtc = GETUTCDATE(),
				JE_SystemLastEditUser = 'E'
			OUTPUT
				INSERTED.JE_PK,
				INSERTED.JE_DeclarationReference,
				DELETED.JE_DeclarationReference
				INTO @DedupedDec
		FROM
			dbo.JobDeclaration JobDec
			INNER JOIN DedupRef ON DedupRef.JE_ClusterKey = JobDec.JE_ClusterKey;

		INSERT dbo.StmALog (SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_GS_NKUser, SL_EventTime)
			SELECT
				'JobDeclaration',
				DecPk,
				'Reference Number Changed: [' + OldDecRef + '] > [' + NewDecRef + ']',
				'EDT',
				'E',
				GETDATE()
			FROM
				@DedupedDec;";
}
