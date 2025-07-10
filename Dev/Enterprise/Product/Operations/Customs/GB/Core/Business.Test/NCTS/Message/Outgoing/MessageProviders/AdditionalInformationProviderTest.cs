using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class AdditionalInformationProviderTest : DataProviderTestCase<AdditionalInformationProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestCode()
		{
			AssertEquals("C13", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("AddInfoRefNum", Provider.Text);
		}

		protected override AdditionalInformationProvider GetProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var cusSupportingInfo = Factory.CreateCusSupportingInfo("OTH", "INF", "AddInfoRefNum", null, "C13", header);
			cusSupportingInfo.CSI_ItemNumber = 2;
			cusSupportingInfo.CSI_LineNo = 1;
			return new AdditionalInformationProvider(cusSupportingInfo);
		}
	}
}
