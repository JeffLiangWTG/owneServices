using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintC4_ParentTableCode : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraint on column C4_ParentTableCode";

		public ConstraintC4_ParentTableCode() : this(1000)
		{
		}

		internal ConstraintC4_ParentTableCode(int batchSize) : base()
		{
			this.batchSize = batchSize;
		}

		readonly int batchSize;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, CusUnderbondSchema.Instance);
			var operation = new GuidChunkingOperation(manager, batchSize, rowCount, ProcessChunk, LastProcessedPropertyName, token);
			operation.DoChunking();
		}

		static void ProcessChunk(Guid lowerBound, Guid upperBound)
		{
			const string sql = """
				DROP TABLE IF EXISTS #ConstraintC4_ParentTableCode
				CREATE TABLE #ConstraintC4_ParentTableCode
				(
					C4_PK uniqueidentifier NOT NULL
				);
				
				INSERT INTO #ConstraintC4_ParentTableCode
				SELECT C4_PK
				FROM dbo.CusUnderbond
				WHERE C4_PK BETWEEN @LowerBound AND @UpperBound
				AND C4_ParentTableCode NOT IN (
					'',
					'BD', -- CusSeaManOBLDetail
					'CA', -- CusSCAHouse
					'CB', -- CusSCAOceanBill
					'CG', -- CusPartShip
					'CJ', -- CusSCADepotContainer
					'CM', -- CusMAWB
					'CN', -- CusSCAContainer
					'CS', -- CusHAWB
					'CV', -- CusSCAPivot
					'CX', -- CusSCADepotHouse
					'JC', -- JobContainer
					'JE', -- JobDeclaration
					'JS', -- JobShipment
					'PW' -- CusBondDetail
				)
				
				DELETE FROM dbo.CusOutturn
				WHERE C5_C4_Underbond IN (SELECT C4_PK FROM #ConstraintC4_ParentTableCode)
				
				DELETE FROM dbo.CusUnderbond
				WHERE C4_PK IN (SELECT C4_PK FROM #ConstraintC4_ParentTableCode)
				""";

			using var cmd = Db.Connection.Command(sql);
			cmd.AddParameterBasedOnDbColumn("@LowerBound", lowerBound, CusUnderbondSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@UpperBound", upperBound, CusUnderbondSchema.PK);

			cmd.ExecuteNonQuery();
		}

		const string LastProcessedPropertyName = "LastProcessedGuidForConstraintC4_ParentTableCode";
	}
}
