using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IdentityTenant.Business
{
	[ModuleID("EdiIdentityTenant")]
	public class EdiIdentityTenantCollection : ActiveBusinessObjectCollection<EdiIdentityTenant>
	{
		public EdiIdentityTenantCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiIdentityTenantCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
