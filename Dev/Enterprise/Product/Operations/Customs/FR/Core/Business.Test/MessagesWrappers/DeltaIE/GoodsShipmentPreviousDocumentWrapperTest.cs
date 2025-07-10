using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GoodsShipmentPreviousDocumentWrapperTest : DataProviderTestCase<GoodsShipmentPreviousDocumentWrapper>
	{
		protected override GoodsShipmentPreviousDocumentWrapper GetProvider()
		{
			var document = Factory.New<PreviousDocument>();
			document.CSI_ReferenceNumber = "001";
			document.CSI_LineNo = 4;
			document.CSI_Code = "XX";

			return GoodsShipmentPreviousDocumentWrapper.New(document, string.Empty);
		}
		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be equal to FR as Customs Office is empty.", Core.Constants.CountryCodes.France, Provider.CcQualifier);

			var document = Factory.New<PreviousDocument>();
			document.CSI_ReferenceNumber = "001";
			document.CSI_LineNo = 4;
			document.CSI_Code = "XX";
			var wrapper = GoodsShipmentPreviousDocumentWrapper.New(document, Core.Constants.CountryCodes.France);
			AssertEquals("CcQualifier should be empty as customs office starts with FR.", string.Empty, wrapper.CcQualifier);

			wrapper = GoodsShipmentPreviousDocumentWrapper.New(document, Core.Constants.CountryCodes.Ukraine);
			AssertEquals("CcQualifier should be equal to FR as customs office doesn't start with FR.", Core.Constants.CountryCodes.France, wrapper.CcQualifier);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal document.CSI_ReferenceNumber.", "001", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type should equal document.CSI_Code.", "XX", Provider.Type);
		}
	}
}
