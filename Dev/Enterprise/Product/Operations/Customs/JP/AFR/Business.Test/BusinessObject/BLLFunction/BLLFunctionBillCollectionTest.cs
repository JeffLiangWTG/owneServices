using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BLLFunctionBillCollection))]
	class BLLFunctionBillCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BLLFunctionBillCollection>
	{
		protected override BLLFunctionBillCollection GetCollectionToTest()
		{
			return new BLLFunctionBillCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bill = Header.Bills.AddNew();
			return new BLLFunctionBill(bill);
		}

		JPAFRHeader Header => header ?? (header = Factory.New<JPAFRHeader>());
		JPAFRHeader header;
	}
}
