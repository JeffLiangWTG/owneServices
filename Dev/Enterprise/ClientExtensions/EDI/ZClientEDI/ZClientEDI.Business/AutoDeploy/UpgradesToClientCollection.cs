using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.AutoDeploy.Business
{
	public class UpgradesToClientCollection : BusinessObjectCollection<UpgradesToClient>
	{
		public UpgradesToClientCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
