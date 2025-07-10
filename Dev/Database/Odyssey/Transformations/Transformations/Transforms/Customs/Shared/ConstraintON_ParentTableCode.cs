using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintON_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column ON_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
DELETE FROM dbo.CusEntryCPDec
WHERE ON_ParentTableCode NOT IN (
	'',
	'CC', -- CusClassification
	'CH', -- CusEntryHeader
	'CI', -- CusClassPartPivot
	'CL', -- CusEntryLine
	'CRD', -- CusReconDeclaration
	'OH' -- OrgHeader
)";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusEntryCPDecSchema.Instance)
					.Key(CusEntryCPDecSchema.Constants.ON_ParentID)
					.Include(CusEntryCPDecSchema.Constants.ON_SystemCreateTimeUtc)
					.Include(CusEntryCPDecSchema.Constants.ON_SystemLastEditTimeUtc)
					.Where("[ON_ParentTableCode]<>'' AND [ON_ParentTableCode]<>'CC' AND [ON_ParentTableCode]<>'CH' AND [ON_ParentTableCode]<>'CI' AND [ON_ParentTableCode]<>'CL' AND [ON_ParentTableCode]<>'CRD' AND [ON_ParentTableCode]<>'OH'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
