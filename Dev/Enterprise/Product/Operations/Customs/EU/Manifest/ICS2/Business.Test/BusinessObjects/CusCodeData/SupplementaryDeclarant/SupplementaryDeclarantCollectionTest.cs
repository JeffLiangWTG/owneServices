using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(SupplementaryDeclarantCollection))]
	sealed class SupplementaryDeclarantCollectionTest : CusCodeDataCollectionTest<SupplementaryDeclarant>
	{
		public void TestCollectionMaxCount()
		{
			var collection = GetCusCodeDataCollection();
			AssertEquals("Max count is 99", 99, collection.MaxCount);
		}

		protected override CusCodeDataCollection<SupplementaryDeclarant> GetCusCodeDataCollection()
		{
			var bill = Factory.New<AsycudaBill>();
			return new SupplementaryDeclarantCollection(bill);
		}
	}
}
