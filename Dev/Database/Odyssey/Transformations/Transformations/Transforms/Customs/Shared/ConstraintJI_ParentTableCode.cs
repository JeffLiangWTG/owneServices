using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintJI_ParentTableCode : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraint on column JI_ParentTableCode";

		public ConstraintJI_ParentTableCode() : this(1000)
		{
		}

		internal ConstraintJI_ParentTableCode(int batchSize) : base()
		{
			this.batchSize = batchSize;
		}

		readonly int batchSize;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, JobComInvoiceLineSchema.Instance);
			var operation = new GuidChunkingOperation(manager, batchSize, rowCount, ProcessChunk, LastProcessedPropertyName, token);
			operation.DoChunking();
		}

		static void ProcessChunk(Guid lowerBound, Guid upperBound)
		{
			const string sql = """
				DROP TABLE IF EXISTS #ConstraintJI_ParentTableCode
				CREATE TABLE #ConstraintJI_ParentTableCode
				(
					JI_PK uniqueidentifier NOT NULL
				);
				
				INSERT INTO #ConstraintJI_ParentTableCode
				SELECT JI_PK
				FROM dbo.JobComInvoiceLine
				WHERE JI_PK BETWEEN @LowerBound AND @UpperBound
				AND JI_ParentTableCode NOT IN (
					'',
					'JI', -- JobComInvoiceLine
					'WOL' -- CusWHSOperatorTransactionLine
				)
				
				DELETE FROM dbo.CusContainerInvoiceLinePivot
				WHERE C2_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.CusHouseContPackInvoiceLinePivot
				WHERE CHC_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.CusPackableItem
				WHERE CUI_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.CusRulingConfig
				WHERE ZZY_JI_InvoiceLine IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.CusUnderbondDec
				WHERE BU_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.JobComInvLineRefs
				WHERE JG_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.JobComInvLineComponentInventory
				WHERE JIV_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.JobComInvoiceLineTax
				WHERE JLT_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.QuarantineExDocEstablishmentAndTime
				WHERE EE_QL IN
				(
					SELECT QL_PK FROM dbo.QuarantineExDocLine
					WHERE QL_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				)
				
				DELETE FROM dbo.QuarantineExDocLine
				WHERE QL_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.JobTWComInvoiceLine
				WHERE TWL_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.JobUSComInvoiceLine
				WHERE USI_JI IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				
				DELETE FROM dbo.JobComInvoiceLine
				WHERE JI_PK IN (SELECT JI_PK FROM #ConstraintJI_ParentTableCode)
				""";

			using var cmd = Db.Connection.Command(sql);
			cmd.AddParameterBasedOnDbColumn("@LowerBound", lowerBound, JobComInvoiceLineSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@UpperBound", upperBound, JobComInvoiceLineSchema.PK);

			cmd.ExecuteNonQuery();
		}

		const string LastProcessedPropertyName = "LastProcessedGuidForConstraintJI_ParentTableCode";
	}
}
