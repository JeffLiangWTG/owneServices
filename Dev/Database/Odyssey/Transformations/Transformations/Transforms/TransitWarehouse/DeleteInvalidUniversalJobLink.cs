using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class DeleteInvalidUniversalJobLink : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Delete Invalid Universal Job Link";

		protected override void OfflinePostUpgradeTransform()
			=> Db.Connection.ExecuteNonQuery(@"
			DELETE dbo.StmUniversalJobLink WHERE UCL_ParentTableCode NOT IN ('WDL', 'YDL', 'WRC', 'YPL', 'KM', 'WDC', 'WRP', 'WRH', 'WDH', 'YTU');");

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(StmUniversalJobLinkSchema.Instance)
				.Key(StmUniversalJobLinkSchema.Constants.UCL_ParentID)
				.Include(StmUniversalJobLinkSchema.Constants.UCL_SystemCreateTimeUtc)
				.Include(StmUniversalJobLinkSchema.Constants.UCL_SystemLastEditTimeUtc)
				.Include(StmUniversalJobLinkSchema.Constants.UCL_OH_Owner)
				.Where(@"[UCL_ParentTableCode]<>'WDL' AND [UCL_ParentTableCode]<>'YDL' AND [UCL_ParentTableCode]<>'WRC' AND [UCL_ParentTableCode]<>'YPL' AND [UCL_ParentTableCode]<>'KM' AND [UCL_ParentTableCode]<>'WDC' AND [UCL_ParentTableCode]<>'WRP' AND [UCL_ParentTableCode]<>'WRH' AND [UCL_ParentTableCode]<>'WDH' AND [UCL_ParentTableCode]<>'YTU'")
				.GetInfo();

				return indexProvider;
			}
		}
	}
}
