using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class PreviousDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentWrapper>
	{
		public void TestGoodsItemIdentifier()
		{
			AssertEquals("Wrapper GoodsItemIdentifier should equal supportingInfo CSI_LineNo.", "3", Provider.GoodsItemIdentifier);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Wrapper ReferenceNumber should equal supportingInfo CSI_CSI_ReferenceNumberItemNumber.", "ReferenceNumber", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Wrapper Type should equal supportingInfo CSI_Code.", "Code", Provider.Type);
		}

		protected override PreviousDocumentWrapper GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_LineNo = 3;
			supportingInfo.CSI_ReferenceNumber = "ReferenceNumber";
			supportingInfo.CSI_Code = "Code";
			return PreviousDocumentWrapper.New(supportingInfo);
		}
	}
}
