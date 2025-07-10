using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.IdentityApplicationPermission.Business
{
	public class EdiIdentityApplicationPermissionCollection : ActiveBusinessObjectCollection<EdiIdentityApplicationPermission>
	{
		public EdiIdentityApplicationPermissionCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public EdiIdentityApplicationPermissionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public EdiIdentityApplicationPermissionCollection(BusinessObjectFactory factory, EdiIdentityApplication parent) : base(factory, parent)
		{
		}
	}
}
