
using CargoWise.EntityFramework;

namespace Enterprise.CustomerService.Business
{
	public class IncidentApprovalCollection : BusinessObjectCollection<IncidentApproval>
	{
		public IncidentApprovalCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IncidentApprovalCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
