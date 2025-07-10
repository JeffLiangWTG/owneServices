using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.IdentityRedirectUrl.Business
{
	public class EdiIdentityRedirectUrlCollection : ActiveBusinessObjectCollection<EdiIdentityRedirectUrl>
	{
		public EdiIdentityRedirectUrlCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public EdiIdentityRedirectUrlCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public EdiIdentityRedirectUrlCollection(BusinessObjectFactory factory, EdiIdentityApplication parent) : base(factory, parent)
		{
		}
	}
}
