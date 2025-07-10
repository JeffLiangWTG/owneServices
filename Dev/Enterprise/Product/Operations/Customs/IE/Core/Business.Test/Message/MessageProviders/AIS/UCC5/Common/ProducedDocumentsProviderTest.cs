using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class ProducedDocumentsProviderTest : DataProviderTestCase<ProducedDocumentsProvider>
	{
		public void TestProviderInterface()
		{
			Assert(Provider is IIdType);
		}

		public void TestId()
		{
			AssertEquals("REFNO1", Provider.Id);
		}

		public void TestType()
		{
			AssertEquals("123", Provider.Type);
		}

		protected override ProducedDocumentsProvider GetProvider() => ProducedDocumentsProvider.New(supportingDocument);

		protected override void SetUp()
		{
			supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "123";
			supportingDocument.CSI_ReferenceNumber = "REFNO1";
		}
		SupportingDocument supportingDocument;
	}
}
