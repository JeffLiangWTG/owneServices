using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintPJ_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column PJ_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
DELETE FROM dbo.StmEntityScreeningLog
WHERE PJ_ParentTableCode NOT IN (
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
					.Where("[PJ_ParentTableCode]<>'' AND [PJ_ParentTableCode]<>'E2' AND [PJ_ParentTableCode]<>'JE' AND [PJ_ParentTableCode]<>'JK' AND [PJ_ParentTableCode]<>'JS' AND [PJ_ParentTableCode]<>'JW' AND [PJ_ParentTableCode]<>'OH' AND [PJ_ParentTableCode]<>'RN' AND [PJ_ParentTableCode]<>'RV' AND [PJ_ParentTableCode]<>'TH' AND [PJ_ParentTableCode]<>'WD'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
