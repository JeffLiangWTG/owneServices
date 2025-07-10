using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	public class MoveGuaranteesToCusInBondMoveHeaderForNctsP5Departure : DataTransformation
	{
		public override string UserDescription => "Change PW_ParentID and PW_ParentTableCode From CusInBondHeader to CusInBondMovementHeader for NCTS P5 Departure declarations";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(nctsCusBondDetailTransformation);
		}

		const string nctsCusBondDetailTransformation = @"
			DECLARE @timeStamp DATETIME = GetUtcDate();
			INSERT INTO dbo.CusInBondMoveHeader (BM_PK, BM_BH, BM_SubApplicationCode, BM_TypeOfSecurity, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
			(
				SELECT
					NEWID(),
					BH_PK,
					'D',
					'NON',
					@timeStamp,
					'~UK',
					@timeStamp,
					'~UK'
				FROM dbo.CusInBondHeader
				LEFT JOIN dbo.CusInBondMoveHeader ON BM_BH = BH_PK
				WHERE BM_BH IS NULL AND BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'D'
				AND EXISTS(SELECT NULL FROM dbo.CusBondDetail WHERE PW_ParentID = BH_PK)
			);

			UPDATE cusBondDetail
			SET PW_ParentID = BM_PK
				, PW_ParentTableCode = 'BM'
				, PW_SystemLastEditTimeUtc = @timeStamp
				, PW_SystemLastEditUser = 'E'
			FROM dbo.CusBondDetail cusBondDetail
			INNER JOIN
			(
				SELECT
					BM_PK,
					BH_PK,
					ROW_NUMBER() OVER (PARTITION BY BH_PK ORDER BY BM_SystemCreateTimeUtc) AS RN
				FROM dbo.CusInBondMoveHeader
				INNER JOIN dbo.CusInBondHeader ON BM_BH = BH_PK AND BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'D' AND BM_SubApplicationCode = 'D'
			) AS header ON cusBondDetail.PW_ParentID = header.BH_PK AND header.RN = 1				
			WHERE cusBondDetail.PW_ParentTableCode = 'BH';";
	}
}
