using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusSupportingDocumentCollection))]
	class CusSupportingDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<CusSupportingDocument>
	{
		public void TestConstructor()
		{
			var collection = GetCusSupportingInfoCollection();
			var newItem = collection.AddNew();
			AssertEquals("SUP", newItem.CSI_Type);
		}

		protected override CusSupportingInfoCollection<CusSupportingDocument> GetCusSupportingInfoCollection()
		{
			var testInvoiceLine = Factory.New<JobComInvoiceLine>();
			return new CusSupportingDocumentCollection(testInvoiceLine);
		}
	}
}
