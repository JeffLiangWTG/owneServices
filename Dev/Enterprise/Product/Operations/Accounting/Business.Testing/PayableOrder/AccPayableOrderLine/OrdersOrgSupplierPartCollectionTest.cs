using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	[TestedType(typeof(OrdersOrgSupplierPartCollection))]
	class OrdersOrgSupplierPartCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrdersOrgSupplierPartCollection(Factory);
		}
	}
}
