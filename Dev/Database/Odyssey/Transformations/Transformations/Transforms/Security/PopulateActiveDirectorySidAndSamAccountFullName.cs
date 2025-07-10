using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class PopulateActiveDirectorySidAndSamAccountFullName : DataTransformation
	{
		public override string UserDescription => "Populate Active Directory SID and SAM Account Full Name";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			// I need to revert this transform, but was told to retain OnlinePostUpgradeTransform otherwise ODT Service task could fail.
			// So I make it as a no-op transform as suggested, when it is needed in furture, it should be re-added as version 2
		}
	}
}
