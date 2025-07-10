using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ViewApprovedWorkflowSchedule : AutoViewApprovedWorkflowSchedule
	{
		public ViewApprovedWorkflowSchedule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
