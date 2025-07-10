using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

public class ChangeCFR_ParentTableCodeAndCFR_ParentIDFromHeaderToMovementHeaderPhase5 : DataTransformation
{
	public override string UserDescription => "Change CFR_ParentID and CFR_ParentTableCode From CusInBondHeader to CusInBondMovementHeader for NCTS P5 Departure declarations";

	protected override void OfflinePostUpgradeTransform()
	{
		Db.Connection.ExecuteNonQuery(nctsCusReferenceTransformation);
	}

	const string nctsCusReferenceTransformation = @"
BEGIN TRY
	INSERT INTO dbo.CusInBondMoveHeader (BM_PK, BM_BH, BM_SubApplicationCode, BM_TypeOfSecurity, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
	(
		SELECT
			NEWID(),
			BH_PK,
			'D',
			'NON',
			GetUtcDate(),
			'E',
			GetUtcDate(),
			'E'
		FROM dbo.CusInBondHeader
		WHERE BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'D'
			AND NOT EXISTS (SELECT NULL FROM dbo.CusInBondMoveHeader WHERE BM_BH = BH_PK)
			AND EXISTS(SELECT NULL FROM dbo.CusReference WHERE CFR_ParentID = BH_PK and CFR_Type = 'SCA')
	);

	UPDATE cusReference
	SET CFR_ParentID = BM_PK, CFR_ParentTableCode = 'BM', CFR_SystemLastEditTimeUtc = GetUtcDate(), CFR_SystemLastEditUser = 'E'
	FROM dbo.CusReference cusReference
	INNER JOIN
	(
		SELECT
			BM_PK,
			BH_PK,
			ROW_NUMBER() OVER (PARTITION BY BH_PK ORDER BY BM_SystemCreateTimeUtc) AS RN
		FROM dbo.CusInBondMoveHeader
		INNER JOIN dbo.CusInBondHeader ON BM_BH = BH_PK AND BM_SubApplicationCode = 'D' AND BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'D'
	) AS header ON cusReference.CFR_ParentID = header.BH_PK AND header.RN = 1
	WHERE cusReference.CFR_ParentTableCode = 'BH' and CFR_Type = 'SCA'

END TRY
BEGIN CATCH
	THROW;
END CATCH;
";
}
