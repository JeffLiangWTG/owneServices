using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	class PreviousDocumentProviderTest : DataProviderTestCase<PreviousDocumentProvider>
	{
		public void TestIPreviousDocument()
		{
			Assert("Should implement IPreviousDocument", Provider is IPreviousDocument);
		}

		public void TestGoodsItemIdentifier()
		{
			AssertEquals("1", Provider.GoodsItemIdentifier);
		}

		public void TestType()
		{
			AssertEquals("235", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("RN1", Provider.Reference);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("2", Provider.SequenceNumber);
		}

		protected override PreviousDocumentProvider GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_LineNo = 1;
			supportingInfo.CSI_Code = "235";
			supportingInfo.CSI_ReferenceNumber = "RN1";
			return PreviousDocumentProvider.New(supportingInfo, 2);
		}
	}
}
