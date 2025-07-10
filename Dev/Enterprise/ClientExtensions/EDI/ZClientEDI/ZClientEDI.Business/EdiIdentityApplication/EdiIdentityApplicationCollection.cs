using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IdentityApplication.Business
{
	[ModuleID("EdiIdentityApplication")]
	public class EdiIdentityApplicationCollection : ActiveBusinessObjectCollection<EdiIdentityApplication>
	{
		public EdiIdentityApplicationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiIdentityApplicationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
