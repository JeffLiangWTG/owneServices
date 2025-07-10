using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class UpdateCusDispositionBlankStatusKey : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update CusDisposition StatusKey column";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE 
	dbo.CusDisposition
SET
	CDI_StatusKey = 'Space'
	, CDI_SystemLastEditTimeUtc = GETUTCDATE()
	, CDI_SystemLastEditUser = '~BP'
WHERE
	CDI_ParentTableCode = 'CH'
	AND CDI_Type = 'AES'
	AND CDI_StatusKey = 'Blank'
";

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CusDisposition_SystemLastEditAuditInfoMustBeUpdated_Update", CusDispositionSchema.Constants.TableName))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CusDisposition_AuditDetailsAreNotMissing_Update", CusDispositionSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusDispositionSchema.Instance)
					.Key(CusDispositionSchema.Constants.CDI_ParentTableCode, CusDispositionSchema.Constants.CDI_Type, CusDispositionSchema.Constants.CDI_StatusKey)
					.Include(CusDispositionSchema.Constants.CDI_SystemLastEditTimeUtc, CusDispositionSchema.Constants.CDI_SystemLastEditUser)
					.Where("[CDI_ParentTableCode]='CH' AND [CDI_Type]='AES' AND [CDI_StatusKey]='Blank'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
