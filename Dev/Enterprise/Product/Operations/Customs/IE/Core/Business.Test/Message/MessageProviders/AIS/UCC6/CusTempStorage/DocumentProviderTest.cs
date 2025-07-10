using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	class DocumentProviderTest : DataProviderTestCase<DocumentProvider>
	{
		public void TestIDocument()
		{
			Assert("Should implement IDocument", Provider is IDocument);
		}

		public void TestType()
		{
			AssertEquals("1A01", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("RN1", Provider.Reference);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("1", Provider.SequenceNumber);
		}

		protected override DocumentProvider GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "1A01";
			supportingInfo.CSI_ReferenceNumber = "RN1";
			return DocumentProvider.New(supportingInfo, 1);
		}
	}
}
