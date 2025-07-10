using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	class PurgeOrphanRecordsFromTables : DataTransformation
	{
		public override string UserDescription => "Purge orphan records from tables";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			return;
		}
	}
}
