using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseJobEmptyWrapper : WarehouseJobGenericWrapper
	{
		public WarehouseJobEmptyWrapper(BusinessObject businessObject, BusinessObjectFactory factory)
			: base(businessObject, factory)
		{
		}

		#region GetJobLines

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			return new WarehouseEmptyWrapperCollection(Factory);
		}

		#endregion
	}
}
