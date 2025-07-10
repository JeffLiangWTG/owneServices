using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class ConstraintKI_ParentTableCode : DataTransformation
	{
		public override string UserDescription => "Try to repair PkgPackageItemDivot records with invalid KI_ParentTableCode, else delete them";

		const string LastProcessedChunkPKName = "ConstraintKI_ParentTableCode.LastProcessedChunkPK";
		readonly int BatchSize;

		public ConstraintKI_ParentTableCode() : this(1000)
		{
		}

		public ConstraintKI_ParentTableCode(int batchSize)
		{
			BatchSize = batchSize;
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, PkgPackageItemDivotSchema.Instance);
			var operation = new GuidChunkingOperation(manager, BatchSize, rowCount, ProcessChunk, LastProcessedChunkPKName, token);
			operation.DoChunking();
		}

		static void ProcessChunk(Guid lowerBound, Guid upperBound)
		{ 
			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			using (var command = Db.Connection.Command(RepairOrDeleteSQL))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, lowerBound);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, upperBound);

				command.ExecuteNonQuery();
				transactionManager.CommitTransaction();
			}
		}

		const string RepairOrDeleteSQL = @"
BEGIN TRY
		UPDATE
			dbo.PkgPackageItemDivot
		SET
			KI_ParentTableCode = TableCode,
			KI_SystemLastEditUser = '~BP',
			KI_SystemLastEditTimeUtc = GETUTCDATE()
		FROM
			dbo.PkgPackageItemDivot
			JOIN
			(
				SELECT
					CUI_PK as ParentPK,
					'CUI' as TableCode
				FROM
					dbo.CusPackableItem

				UNION ALL

				SELECT
					WZ_PK as ParentPK,
					'WZ' as TableCode
				FROM
					dbo.WhsPickLine

			) Parents ON ParentPK = KI_ParentID 
		WHERE 1=1
			AND KI_ParentID >= @FromPK
			AND KI_ParentID <= @ToPK
			AND KI_ParentTableCode NOT IN ('CUI', 'WZ')


		DELETE
			dbo.PkgPackageItemDivot
		WHERE 1=1
			AND KI_ParentID >= @FromPK
			AND KI_ParentID <= @ToPK
			AND KI_ParentTableCode NOT IN ('CUI', 'WZ')
END TRY
BEGIN CATCH
	THROW;
END CATCH
";
	}
}
