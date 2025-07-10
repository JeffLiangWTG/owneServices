using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseEmptyJobLineWrapper : WarehouseGenericLineWrapper
	{
		public WarehouseEmptyJobLineWrapper(BusinessObject whsLineBO, BusinessObjectFactory factory)
			: base(whsLineBO, factory)
		{
		}
	}
}
