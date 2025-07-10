using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseJobEmptyWrapperCollection : WarehouseJobGenericWrapperCollection
	{
		#region Constructors

		public WarehouseJobEmptyWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseJobEmptyWrapperCollection(BusinessObjectCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		#endregion
	}
}
