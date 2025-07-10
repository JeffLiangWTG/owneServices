using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillCollectionTest : TestCaseWithFactory
	{
		public void TestIndexer()
		{
			var oceanBillCollection = new CusSCAOceanBillCollection(Factory);
			var oceanBill = oceanBillCollection.AddNew();
			AssertEquals(oceanBill, oceanBillCollection[0]);
		}

		public void TestTypedAddNew()
		{
			var oceanBillCollection = new CusSCAOceanBillCollection(Factory);
			var oceanBill = oceanBillCollection.AddNew();
			AssertEquals(typeof(CusSCAOceanBill), oceanBill.GetType());
		}
	}
}
