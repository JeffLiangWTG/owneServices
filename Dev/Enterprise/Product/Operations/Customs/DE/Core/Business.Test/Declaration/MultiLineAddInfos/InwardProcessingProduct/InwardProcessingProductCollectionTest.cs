using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InwardProcessingProductCollection))]
	public class InwardProcessingProductCollectionTest : CusSupportingInfoCollectionTest<InwardProcessingProduct>
	{
		public void TestMaxCount()
		{
			var collection = GetCusSupportingInfoCollection();
			AssertEquals("Max Count should be 999", 999, collection.MaxCount);
		}

		protected override CusSupportingInfoCollection<InwardProcessingProduct> GetCusSupportingInfoCollection()
		{
			return new InwardProcessingProductCollection(Factory.New<JobComInvoiceLine>());
		}
	}
}
