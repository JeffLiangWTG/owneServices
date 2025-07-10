using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BulkAddDiscountCollection))]
	public class BulkAddDiscountCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BulkAddDiscountCollection>
	{
		protected override BulkAddDiscountCollection GetCollectionToTest()
		{
			return new BulkAddDiscountCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BulkAddDiscount();
		}
	}
}
