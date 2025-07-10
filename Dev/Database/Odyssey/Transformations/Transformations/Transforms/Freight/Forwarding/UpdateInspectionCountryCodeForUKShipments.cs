using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public sealed class UpdateInspectionCountryCodeForUKShipments : DataTransformation
	{
		public override string UserDescription => "[Deleted Transformation] Copy inspection info (EU > UK) for UK shipments.";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			return;
		}
	}
}
