
using CargoWise.EntityFramework;

namespace Enterprise.CustomerService.Business
{
	public class IncidentRequestCollection : BusinessObjectCollection<IncidentRequest>
	{
		public IncidentRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IncidentRequestCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
