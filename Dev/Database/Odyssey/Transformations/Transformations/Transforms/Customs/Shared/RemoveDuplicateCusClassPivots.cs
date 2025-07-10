using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Customs.Shared
{
	class RemoveDuplicateCusClassPivots : DataTransformation
	{
		public override string UserDescription => "Remove duplicates from CusClassPartPivot.";

		const string LastProcessedPKExtendedPropertyString = "RemoveDuplicateCusClassPivots.LastProcessedCusClassificationPK";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			ExtProperty.Database.Delete(Db.Connection, LastProcessedPKExtendedPropertyString);
		}
	}
}
