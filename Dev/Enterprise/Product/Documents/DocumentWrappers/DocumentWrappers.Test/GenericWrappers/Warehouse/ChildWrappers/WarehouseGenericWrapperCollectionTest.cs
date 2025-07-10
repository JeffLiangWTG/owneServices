using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	internal abstract class WarehouseGenericWrapperCollectionTest<T> : GenericWrapperCollectionTest<T> where T : WarehouseGenericWrapperCollection
	{
		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;
	}
}
