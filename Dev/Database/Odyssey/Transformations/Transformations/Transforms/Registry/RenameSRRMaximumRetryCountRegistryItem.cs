
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	public class RenameSRRMaximumRetryCountRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from SRRRetriesInLast24Hours to SRRMaximumRetryCount";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("SRRRetriesInLast24Hours", "SRRMaximumRetryCount");
		}
	}
}
