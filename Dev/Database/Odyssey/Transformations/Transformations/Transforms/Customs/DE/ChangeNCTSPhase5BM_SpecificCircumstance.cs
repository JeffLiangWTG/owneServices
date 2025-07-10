using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE
{
	public sealed class ChangeNCTSPhase5BM_SpecificCircumstance : DataTransformation
	{
		public override string UserDescription => "Change DE NCTS Phase5 BM_SpecificCircumstance Values : '1' to 'A20' and '2' to 'XXX'";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(transformationQuery);
		}

		const string transformationQuery = @"UPDATE dbo.CusInBondMoveHeader
	SET BM_SpecificCircumstance = CASE 
		WHEN cusInBondMoveHeader.BM_SpecificCircumstance = '1' THEN 'A20'
		WHEN cusInBondMoveHeader.BM_SpecificCircumstance = '2' THEN 'XXX'
		ELSE cusInBondMoveHeader.BM_SpecificCircumstance
	END
	, BM_SystemLastEditTimeUtc = GetUtcDate(), BM_SystemLastEditUser = 'E'
	FROM dbo.CusInBondMoveHeader cusInBondMoveHeader
	INNER JOIN dbo.CusInBondHeader ON BM_BH = BH_PK AND BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'D'
	INNER JOIN dbo.GlbBranch ON GB_PK = BH_GB AND GB_RN_NKCountryCode = 'DE'
	WHERE cusInBondMoveHeader.BM_SpecificCircumstance BETWEEN '1' AND '2'";
	}
}
