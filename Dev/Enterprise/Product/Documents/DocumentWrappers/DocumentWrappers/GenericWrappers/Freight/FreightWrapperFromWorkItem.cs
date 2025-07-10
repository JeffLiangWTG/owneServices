using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromWorkItem : FreightWrapper
	{
		public FreightWrapperFromWorkItem(WorkItem workitem, BusinessObjectFactory factory)
			: base(workitem, factory)
		{
			this.workitem = workitem;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return workitem.PK;
		}

		protected override WorkItem GetWorkItem()
		{
			return workitem;
		}

		protected override ZString GetJobNumber()
		{
			return workitem.Number;
		}

		readonly WorkItem workitem;
	}
}
