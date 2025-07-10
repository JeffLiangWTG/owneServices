
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutCollection : BusinessObjectCollection<Callout>
	{
		public CalloutCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CalloutCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
