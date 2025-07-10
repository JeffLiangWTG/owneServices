using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.MasterFiles.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsJobChargeCollection))]
	sealed class DocWhsJobChargeCollectionTest : DocJobChargeCollectionTest<DocWhsJobChargeCollection, DocWhsJobCharge>
	{
		protected override DocWhsJobChargeCollection GetCollectionToTest()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			return DocWhsJobChargeCollection.GetCollection(WarehouseJobGenericWrapper.New(whsOrder, Factory), "TEST");
		}
	}
}
