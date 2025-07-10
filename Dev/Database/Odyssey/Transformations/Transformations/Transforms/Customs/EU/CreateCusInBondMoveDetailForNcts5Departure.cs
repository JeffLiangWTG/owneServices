using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	public class CreateCusInBondMoveDetailForNcts5Departure : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Create a new record if not exist in CusInBondMoveDetail for Save the Sequence Number in NCTS5 Departure declarations";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(nctsCusBondDetailTransformation);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var cusInBondHeaderIndexInfo = IndexInfo.Builder
					.New(Db.SqlDbOwnerSchema, "CusInBondHeader", "_IDX_Transformation_CusInBondHeader_1")
					.Key(CusInBondHeaderSchema.BH_ApplicationCode.Name, CusInBondHeaderSchema.BH_HeaderType.Name)
					.Include(CusInBondHeaderSchema.PK.Name)
					.Where("[BH_ApplicationCode]='NC5' AND [BH_HeaderType]='D'")
					.GetInfo();

				var cusInBondMoveDetailIndexInfo = IndexInfo.Builder
					.New(Db.SqlDbOwnerSchema, "CusInBondMoveDetail", "_IDX_Transformation_CusInBondMoveDetail_1")
					.Key(CusInBondMoveDetailSchema.B9_SeqNo.Name, CusInBondMoveDetailSchema.B9_PreviousITNumber.Name, CusInBondMoveDetailSchema.B9_SystemLastEditTimeUtc.Name, CusInBondMoveDetailSchema.B9_SystemLastEditUser.Name)
					.Include(CusInBondMoveDetailSchema.PK.Name)
					.Where("[B9_SeqNo]='0' AND [B9_PreviousITNumber]='PUPD'")
					.GetInfo();

				return new TransformationIndexProvider(this) { new[] { cusInBondHeaderIndexInfo, cusInBondMoveDetailIndexInfo } };
			}
		}

		const string nctsCusBondDetailTransformation = @"
		BEGIN TRY
			DECLARE @timeStamp DATETIME = GetUtcDate();

			INSERT INTO dbo.CusInBondMoveDetail(B9_PK, B9_BM, B9_InBoundQty, B9_B0,
												B9_IsValid, B9_SeqNo, B9_MonetaryValue, B9_ExportDate,
												B9_FirstAcceptedTime, B9_AutoVersion, B9_SystemCreateTimeUtc, B9_SystemCreateUser,
												B9_SystemLastEditTimeUtc, B9_SystemLastEditUser, B9_B9_InBondMoveDetail, B9_UnloadedState,
												B9_PreviousITNumber)
			(SELECT	NEWID(),BM_PK, '0', B0_PK,
					'1', '0', '0', NULL,
					NULL, '0', @timeStamp, 'E',
					@timeStamp, 'E', NULL, 'NEW',
					'PUPD'
				FROM dbo.CusInBondMoveHeader
				INNER JOIN dbo.CusInBondHeader ON BM_BH = BH_PK AND BH_ApplicationCode = 'NC5' and BH_HeaderType = 'D'
				INNER JOIN dbo.CusInBondBill ON B0_BH = BH_PK
				WHERE NOT EXISTS (SELECT 1 FROM dbo.CusInBondMoveDetail WHERE B9_BM = BM_PK and B9_B0 = B0_PK)
			);

			WITH movDetail AS(
				SELECT *, ROW_NUMBER() OVER( PARTITION BY B9_BM ORDER BY B9_PK) AS noLine
				FROM dbo.CusInBondMoveDetail
				WHERE B9_SeqNo = '0'
				AND B9_PreviousITNumber = 'PUPD'
			)
			UPDATE movDetail
			SET	B9_SeqNo = noLine
				,B9_PreviousITNumber = ''
				,B9_SystemLastEditTimeUtc = @timeStamp
				,B9_SystemLastEditUser = 'E';

		END TRY
		BEGIN CATCH
			THROW;
		END CATCH;
		";
	}
}
