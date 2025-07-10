using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class NeoUpgradeLicenceLookups : ZLookups
	{
		public NeoUpgradeLicenceLookups(NeoUpgradeLicence parent)
			: base(parent) { }

		#region Implementation

		protected new NeoUpgradeLicence Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (NeoUpgradeLicence)base.Parent; }
		}

		public LicenceEnterpriseCollection LicenceEnterpriseList => licenceEnterpriseList ?? (licenceEnterpriseList = new LicenceEnterpriseCollection(Factory));
		LicenceEnterpriseCollection licenceEnterpriseList;

		public new BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		#endregion
	}
}

