using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	public class PopulateBM_GrossWeightUnloadedAndB0_GrossWeightUnloadedForNctsP5Arrival : DataTransformation
	{
		public override string UserDescription => "Populate BM_GrossWeightUnloaded AND B0_GrossWeightUnloaded for Ncts Phase5 Arrival Declarations";

		public TransformationIndexProvider IndexProvider => BuildIndexProvider();

		TransformationIndexProvider BuildIndexProvider()
		{
			var indexInfo1 = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "CusInBondMoveDetail", "IX_PopulateBM_GrossWeightUnloadedAndB0_GrossWeightUnloadedForNctsP5Arrival1")
				.Key(CusInBondMoveDetailSchema.B9_UnloadedState.Name)
				.Include(CusInBondMoveDetailSchema.B9_B0.Name)
				.GetInfo();

			var indexInfo2 = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "CusInBondCargoDesc", "IX_PopulateBM_GrossWeightUnloadedAndB0_GrossWeightUnloadedForNctsP5Arrival2")
				.Key(CusInBondCargoDescSchema.BY_ParentTableCode.Name, CusInBondCargoDescSchema.BY_ParentID.Name, CusInBondCargoDescSchema.BY_UnloadedState.Name)
				.Include(CusInBondCargoDescSchema.PK.Name, CusInBondCargoDescSchema.BY_BY_Commodity.Name, CusInBondCargoDescSchema.BY_GrossWeight.Name)
				.GetInfo();

			return new TransformationIndexProvider(this) { indexInfo1.Yield(), indexInfo2.Yield() };
		}

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(transformationScript);
		}

		const string transformationScript = @"
		BEGIN TRY
			DECLARE @timeStamp DATETIME = GETUTCDATE();

			WITH GoodsItems AS (
				SELECT pa.BY_ParentID AS B0_PK, SUM(ISNULL(ch.BY_GrossWeight, pa.BY_GrossWeight)) AS GrossWeight
				FROM dbo.CusInBondCargoDesc pa
				LEFT JOIN dbo.CusInBondCargoDesc ch ON ch.BY_ParentTableCode = 'BY'
					AND ch.BY_ParentID = pa.BY_PK
					AND pa.BY_BY_Commodity = ch.BY_PK
					AND pa.BY_UnloadedState = 'DIF'
				WHERE pa.BY_ParentTableCode = 'B0'
					AND pa.BY_UnloadedState IN ('DIF', 'DEC', 'NEW')
				GROUP BY pa.BY_ParentID
			),
			MoveHeader AS (
			SELECT BM.*
			FROM dbo.CusInBondHeader BH
			JOIN dbo.CusInBondMoveHeader BM ON BM_BH = BH_PK
				AND BM_SubApplicationCode = 'A'
				AND BM_NoChangesToReport = 0
				AND BM_CustomsStatus > ''
			WHERE BH_ApplicationCode = 'NC5'
			)

			UPDATE B0
			SET B0_GrossWeightUnloaded = GrossWeight
				,B0_SystemLastEditTimeUtc = @timeStamp
				,B0_SystemLastEditUser = 'E'
			FROM dbo.CusInBondBill B0
			JOIN GoodsItems ON GoodsItems.B0_PK = B0.B0_PK
			WHERE B0_BH IN (SELECT BM_BH FROM MoveHeader)
				AND B0.B0_PK IN (SELECT B9_B0 FROM dbo.CusInBondMoveDetail WHERE B9_UnloadedState = 'DIF')
			;


			WITH GoodsItems AS (
				SELECT B0_BH, SUM(ISNULL(ch.BY_GrossWeight, pa.BY_GrossWeight)) AS GrossWeight
				FROM dbo.CusInBondCargoDesc pa
				LEFT JOIN dbo.CusInBondCargoDesc ch ON ch.BY_ParentTableCode = 'BY'
					AND ch.BY_ParentID = pa.BY_PK
					AND pa.BY_BY_Commodity = ch.BY_PK
					AND pa.BY_UnloadedState = 'DIF'
				JOIN dbo.CusInBondBill B0 ON B0.B0_PK = pa.BY_ParentID
				WHERE pa.BY_ParentTableCode = 'B0'
					AND pa.BY_UnloadedState IN ('DIF', 'DEC', 'NEW')
				GROUP BY B0_BH
			)
			UPDATE BM
			SET BM_GrossWeightUnloaded = GrossWeight
				,BM_SystemLastEditTimeUtc = @timeStamp
				,BM_SystemLastEditUser = 'E'
			FROM dbo.CusInBondMoveHeader BM
			JOIN dbo.CusInBondHeader BH ON BM_BH = BH_PK
				AND BH_ApplicationCode = 'NC5'
			JOIN GoodsItems ON GoodsItems.B0_BH = BH_PK
			WHERE BM_SubApplicationCode = 'A'
				AND BM_NoChangesToReport = 0
				AND BM_CustomsStatus > ''
			;
		END TRY
		BEGIN CATCH
			THROW;
		END CATCH;
		";
	}
}
