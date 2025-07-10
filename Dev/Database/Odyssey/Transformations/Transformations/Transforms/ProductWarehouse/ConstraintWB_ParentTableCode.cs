using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class ConstraintWB_ParentTableCode : DataTransformation
	{
		public override string UserDescription => "Try to repair WhsBondedWarehouseAttribute records with invalid WB_ParentTableCode, else delete them";
		const string LastProcessedChunkPKName = "ConstraintWB_ParentTableCode.LastProcessedChunkPK";
		readonly int BatchSize;

		public ConstraintWB_ParentTableCode() : this(1000)
		{
		}

		internal ConstraintWB_ParentTableCode(int batchSize)
		{
			BatchSize = batchSize;
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsBondedWarehouseAttributeSchema.Instance);
			var operation = new GuidChunkingOperation(manager, BatchSize, rowCount, ProcessChunk, LastProcessedChunkPKName, token);
			operation.DoChunking();

			Db.Connection.ExecuteNonQuery(DeleteNullParentID);
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
			dbo.WhsBondedWarehouseAttribute
		SET
			WB_ParentTableCode = 'WE',
			WB_SystemLastEditUser = '~BP',
			WB_SystemLastEditTimeUtc = GETUTCDATE()
		FROM
			dbo.WhsBondedWarehouseAttribute
			JOIN dbo.WhsDocketLine ON WE_PK = WB_ParentID
		WHERE 1=1
			AND WB_ParentID >= @FromPK
			AND WB_ParentID <= @ToPK
			AND WB_ParentTableCode <> 'WE'


		DELETE
			dbo.WhsBondedWarehouseAttribute
		WHERE 1=1
			AND WB_ParentID >= @FromPK
			AND WB_ParentID <= @ToPK
			AND WB_ParentTableCode <> 'WE'
END TRY
BEGIN CATCH
	THROW;
END CATCH
";

		const string DeleteNullParentID = @"
DELETE
	dbo.WhsBondedWarehouseAttribute
WHERE
	WB_ParentID IS NULL
";
	}
}
