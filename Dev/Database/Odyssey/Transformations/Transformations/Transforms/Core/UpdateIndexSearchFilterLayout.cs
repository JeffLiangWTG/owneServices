using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	sealed class UpdateIndexSearchFilterLayout : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Index Search Filter Layout";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"UPDATE
dbo.StmModuleFilter
SET
	S9_IsIndexSearch = 1,
	S9_FilterType = '',
	S9_SystemLastEditTimeUtc = GetUtcDate(),
	S9_SystemLastEditUser = '~BP'
WHERE S9_FilterType = 'GLI'";

			Db.Connection.ExecuteNonQuery(sql);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(StmModuleFilterSchema.Instance)
				   .Key(StmModuleFilterSchema.Constants.S9_FilterType)
				   .Where($"[S9_FilterType]='GLI'")
				   .GetInfo();
				return indexProvider;
			}
		}
	}
}

