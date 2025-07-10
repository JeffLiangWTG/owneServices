using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.HRMS
{
	public class RemoveReviewProcessWithInvalidFilterType : DataTransformation
	{
		public override string UserDescription => "Remove specific Review Process record with an invalid DateTimeFilter type";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
			=> Db.Connection.ExecuteNonQuery("DELETE ReviewProcess WHERE RPR_PK = '7D90CD77-4183-4FFE-9AEC-576385E8ACE0';");
	}
}
