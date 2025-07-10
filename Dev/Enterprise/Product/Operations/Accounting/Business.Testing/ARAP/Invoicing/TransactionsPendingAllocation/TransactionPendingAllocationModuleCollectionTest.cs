using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationModuleCollection))]
	public class TransactionPendingAllocationModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransactionPendingAllocationModuleCollection(Factory, new ZQuery());
		}
	}
}
