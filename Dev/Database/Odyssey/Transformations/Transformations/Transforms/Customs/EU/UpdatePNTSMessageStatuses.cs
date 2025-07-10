using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	sealed class UpdatePNTSMessageStatuses : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update existing PNTS message statuses to 3 chars. Empty PNTS message status where message status is NST.";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(AsycudaManifestHeaderSchema.Instance).Key(AsycudaManifestHeaderSchema.Constants.AMA_ApplicationCode, AsycudaManifestHeaderSchema.Constants.AMA_MessageStatus).Where(@"[AMA_ApplicationCode]='STO' AND ([AMA_MessageStatus] IN ('FA', 'ST', 'FR', 'NST'))").GetInfo();
				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
			BEGIN TRY
				UPDATE dbo.AsycudaManifestHeader
				SET AMA_MessageStatus = CASE WHEN AMA_MessageStatus='FA' THEN 'FAL'
											 WHEN AMA_MessageStatus='ST' THEN 'SNT'
											 WHEN AMA_MessageStatus='FR' THEN 'REJ'
											 ELSE '' END, 
                    AMA_SystemLastEditUser = '~BP', AMA_SystemLastEditTimeUtc = GetUtcDate()
				WHERE AMA_MessageStatus IN ('FA', 'ST', 'FR', 'NST')
				AND AMA_ApplicationCode = 'STO';
			END TRY
			BEGIN CATCH
				THROW;
			END CATCH";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
