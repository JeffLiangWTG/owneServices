using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.OrderManager
{
	class UpdateOrderIsReleasedFlagForInProgressOrders : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set JD_IsReleased to true if order is in progress (not incomplete, or cancelled)";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobOrderHeaderSchema.Instance)
					.Key(JobOrderHeaderSchema.Constants.JD_OrderStatus)
					.Where($"[{JobOrderHeaderSchema.Constants.JD_OrderStatus}]<>'INC' AND [{JobOrderHeaderSchema.Constants.JD_OrderStatus}]<>'CAN'")
					.Include(JobOrderHeaderSchema.Constants.JD_SystemLastEditUser)
					.Include(JobOrderHeaderSchema.Constants.JD_SystemLastEditTimeUtc)
					.GetInfo();

				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();
			Db.Connection.ExecuteNonQuery(@"
UPDATE
	dbo.JobOrderHeader
SET
	JD_IsReleased = 1,
	JD_SystemLastEditUser = '~BP',
	JD_SystemLastEditTimeUtc = GETUTCDATE()
WHERE
	JD_OrderStatus <> 'INC' AND JD_OrderStatus <> 'CAN'");
		}
	}
}
