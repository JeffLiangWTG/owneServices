using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromMNRWorkOrder : FreightWrapper
	{
		public FreightWrapperFromMNRWorkOrder(MNRWorkOrderHeader workOrderHeader, BusinessObjectFactory factory)
			: base(workOrderHeader, factory)
		{
			this.workOrderHeader = workOrderHeader;
		}

		readonly MNRWorkOrderHeader workOrderHeader;

		protected override ZString GetJobNumber()
		{
			return workOrderHeader.MWO_JobNumber;
		}
	}
}
