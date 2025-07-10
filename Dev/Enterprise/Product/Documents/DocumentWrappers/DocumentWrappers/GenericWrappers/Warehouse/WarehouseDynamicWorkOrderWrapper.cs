using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse
{
	public class WarehouseDynamicWorkOrderWrapper : WarehousePickableDocketWrapper
	{
		public WarehouseDynamicWorkOrderWrapper(WhsDynamicWorkOrder whsWorkOrder, BusinessObjectFactory factory)
			: base(whsWorkOrder, factory)
		{
		}
	}
}
