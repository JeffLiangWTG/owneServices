using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE;

public class UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Update BM_CustomsStatus and BM_Phase to EU system for DE NCTS Phase 5 Arrival";

	public TransformationIndexProvider IndexProvider
	{
		get
		{
			var cusInBondHeaderIndexInfo = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "CusInbondHeader", "IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BH_ApplicationCode_BH_HeaderType")
				.Key(CusInBondHeaderSchema.BH_ApplicationCode.Name, CusInBondHeaderSchema.BH_HeaderType.Name)
				.Include(CusInBondHeaderSchema.PK.Name, CusInBondHeaderSchema.BH_GB.Name)
				.Where("[BH_ApplicationCode]='NC5' AND [BH_HeaderType]='A'")
				.GetInfo();

			var cusInbondMoveHeaderBM_PhaseIndexInfo = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "CusInbondMoveHeader", "IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BM_Phase")
				.Key(CusInBondMoveHeaderSchema.BM_Phase.Name)
				.Where("[BM_Phase] IN ('STU', 'FRC', '007', '044')")
				.GetInfo();

			var cusInbondMoveHeaderBM_CustomsStatusIndexInfo = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "CusInbondMoveHeader", "IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BM_CustomsStatus")
				.Key(CusInBondMoveHeaderSchema.BM_CustomsStatus.Name)
				.Where("[BM_CustomsStatus] IN ('AUP', 'ART', 'UAP', 'CL1')")
				.GetInfo();

			return new TransformationIndexProvider(this) { new[] { cusInBondHeaderIndexInfo, cusInbondMoveHeaderBM_PhaseIndexInfo, cusInbondMoveHeaderBM_CustomsStatusIndexInfo } };
		}
	}

	protected override void OfflinePostUpgradeTransform()
	{
		const string sql = @"
UPDATE dbo.CusInbondMoveHeader
SET
	BM_CustomsStatus =
		CASE BM_CustomsStatus
			WHEN 'AUP' THEN 'UAP'
			WHEN 'ART' THEN 'CL1'
			ELSE BM_CustomsStatus
		END,
	BM_Phase =
		CASE
			WHEN BM_Phase = 'STU' THEN '007'
			WHEN BM_Phase = 'FRC' THEN '044'
			ELSE BM_Phase
		END,
	BM_SystemLastEditTimeUtc = GetUtcDate(),
	BM_SystemLastEditUser = 'E'
FROM dbo.CusInbondMoveHeader
INNER JOIN dbo.CusInbondHeader ON BM_BH = BH_PK
INNER JOIN dbo.GlbBranch ON GB_PK = BH_GB
INNER JOIN dbo.GlbCompany ON GC_PK = GB_GC AND GC_RN_NKCountryCode = 'DE'
WHERE BH_ApplicationCode = 'NC5'
	AND BH_HeaderType = 'A'
	AND (BM_Phase IN ('STU', 'FRC') OR BM_CustomsStatus IN ('AUP', 'ART'));
";
		Db.Connection.ExecuteNonQuery(sql);
	}
}
