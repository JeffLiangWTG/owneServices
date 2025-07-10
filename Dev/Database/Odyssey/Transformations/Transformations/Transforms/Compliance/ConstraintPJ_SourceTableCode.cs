using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintPJ_SourceTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column PJ_SourceTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
DELETE FROM dbo.StmEntityScreeningLog
WHERE PJ_SourceTableCode NOT IN (
	'',
	'E2',
	'JE',
	'JK',
	'JS',
	'JW',
	'OH',
	'RN',
	'RV',
	'TH',
	'WD'
)";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(StmEntityScreeningLogSchema.Instance)
					.Key(StmEntityScreeningLogSchema.Constants.PJ_ParentID)
					.Include(StmEntityScreeningLogSchema.Constants.PJ_SystemCreateTimeUtc)
					.Include(StmEntityScreeningLogSchema.Constants.PJ_SystemLastEditTimeUtc)
					.Where("[PJ_SourceTableCode]<>'' AND [PJ_SourceTableCode]<>'E2' AND [PJ_SourceTableCode]<>'JE' AND [PJ_SourceTableCode]<>'JK' AND [PJ_SourceTableCode]<>'JS' AND [PJ_SourceTableCode]<>'JW' AND [PJ_SourceTableCode]<>'OH' AND [PJ_SourceTableCode]<>'RN' AND [PJ_SourceTableCode]<>'RV' AND [PJ_SourceTableCode]<>'TH' AND [PJ_SourceTableCode]<>'WD'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
