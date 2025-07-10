using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromCYDAdHocServiceOrder : FreightWrapper
	{
		public FreightWrapperFromCYDAdHocServiceOrder(CYDAdHocServiceOrder adHocServiceOrder, BusinessObjectFactory factory)
			: base(adHocServiceOrder, factory)
		{
			this.adHocServiceOrder = adHocServiceOrder;
		}

		readonly CYDAdHocServiceOrder adHocServiceOrder;

		protected override ZString GetJobNumber()
		{
			return adHocServiceOrder.YAO_JobNumber;
		}
	}
}
