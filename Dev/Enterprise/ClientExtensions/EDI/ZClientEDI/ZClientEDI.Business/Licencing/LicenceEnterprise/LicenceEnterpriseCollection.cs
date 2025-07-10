
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[ModuleID("LicenceEnterprise")]
	public class LicenceEnterpriseCollection : BusinessObjectCollection<LicenceEnterprise>
	{
		public LicenceEnterpriseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LicenceEnterpriseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Implementation

		#endregion
	}
}

