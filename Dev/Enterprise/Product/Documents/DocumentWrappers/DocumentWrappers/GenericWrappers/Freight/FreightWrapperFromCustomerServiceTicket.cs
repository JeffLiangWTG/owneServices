using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromCustomerServiceTicket : FreightWrapper
	{
		public FreightWrapperFromCustomerServiceTicket(WorkRequest workRequest, BusinessObjectFactory factory)
			: base(workRequest, factory)
		{
			this.workRequest = workRequest;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return workRequest.PK;
		}

		protected override WorkRequest GetWorkRequest()
		{
			return workRequest;
		}

		protected override ZString GetJobNumber()
		{
			return workRequest.Number;
		}

		readonly WorkRequest workRequest;
	}
}
