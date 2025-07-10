using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseEmptyWrapperCollection : WarehouseGenericWrapperCollection<WarehouseEmptyJobLineWrapper>
	{
		#region Constructors

		public WarehouseEmptyWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseEmptyWrapperCollection(BusinessObjectCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		#endregion
	}
}
