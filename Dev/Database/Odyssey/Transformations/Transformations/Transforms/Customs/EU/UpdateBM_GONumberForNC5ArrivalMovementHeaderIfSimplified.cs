using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

public class UpdateBM_GONumberForNC5ArrivalMovementHeaderIfSimplified : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Update BM_GONumber to A3 for NCTS5 arrival movement header if who is previously Y";

	TransformationIndexProvider ITransformationIndexProvider.IndexProvider => BuildIndexProvider();

	TransformationIndexProvider BuildIndexProvider()
	{
		var indexProvider = new TransformationIndexProvider(this);

		indexProvider.New(CusInBondMoveHeaderSchema.Instance)
			.Key(CusInBondMoveHeaderSchema.BM_SubApplicationCode.Name, CusInBondMoveHeaderSchema.BM_GONumber.Name)
			.Where("[BM_SubApplicationCode]='A' AND [BM_GONumber]='Y'")
			.Include(CusInBondMoveHeaderSchema.BM_SystemLastEditTimeUtc.Name, CusInBondMoveHeaderSchema.BM_SystemLastEditUser.Name, CusInBondMoveHeaderSchema.BM_BH.Name)
			.GetInfo();

		return indexProvider;
	}

	const string sql = @"
UPDATE
	MovementHeader
SET
	BM_GONumber = 'A3',
	BM_SystemLastEditTimeUtc = GETUTCDATE(),
	BM_SystemLastEditUser = '~BP'
FROM dbo.CusInBondMoveHeader MovementHeader
JOIN dbo.CusInBondHeader ON BM_BH = BH_PK
WHERE 1 = 1
	AND BM_SubApplicationCode = 'A'
	AND BM_GONumber = 'Y'
	AND BH_ApplicationCode = 'NC5'
	AND BH_HeaderType = 'A'
";

	protected override void OfflinePostUpgradeTransform()
	{
		Db.Connection.ExecuteNonQuery(sql);
	}
}
