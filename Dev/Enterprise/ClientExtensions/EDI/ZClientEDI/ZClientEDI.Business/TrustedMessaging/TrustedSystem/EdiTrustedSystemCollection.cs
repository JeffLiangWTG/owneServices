using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	[ModuleID("EdiTrustedSystem")]
	public class EdiTrustedSystemCollection : ActiveBusinessObjectCollection<EdiTrustedSystem>
	{
		public EdiTrustedSystemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public EdiTrustedSystemCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
