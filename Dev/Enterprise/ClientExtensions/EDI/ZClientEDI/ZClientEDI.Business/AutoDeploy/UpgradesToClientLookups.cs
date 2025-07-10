//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUpgradesToClientLookups
//
//    This class should be used for overriding collections in AutoUpgradesToClientLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;

namespace Enterprise.Client.EDI.AutoDeploy.Business
{
	public class UpgradesToClientLookups : AutoUpgradesToClientLookups
	{
		public UpgradesToClientLookups(AutoUpgradesToClient parent)
			: base(parent)
		{
		}

		#region Builds

		public virtual ReleaseBuildCollection Builds
		{
			get
			{
				if (fBuilds == null)
				{
					fBuilds = new ReleaseBuildCollection(Factory);
				}
				return fBuilds;
			}
		}
		ReleaseBuildCollection fBuilds;

		#endregion

		#region Upgrade Methods

		public UpgradeMethods UpgradeMethodsList
		{
			get { return new UpgradeMethods(); }
		}

		#endregion

		#region CurrentStatus

		public UpgradesToClientStatus UpgradeStatusList
		{
			get { return new UpgradesToClientStatus(); }
		}

		#endregion
	}
}

