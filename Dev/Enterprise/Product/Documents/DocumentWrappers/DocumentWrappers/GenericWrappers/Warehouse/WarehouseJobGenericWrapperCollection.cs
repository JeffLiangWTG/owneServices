using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class WarehouseJobGenericWrapperCollection : GenericWrapperCollection<WarehouseJobGenericWrapper>
	{
		public WarehouseJobGenericWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseJobGenericWrapperCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory)
			: base(collectionToWrap, factory)
		{
		}
	}
}
