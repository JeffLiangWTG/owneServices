using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(AdditionalInformationProvider))]
	sealed class AdditionalInformationProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInformationProvider>
	{
		public void TestSequenceNumber()
		{
			cusSupportingInfo.CSI_LineNo = 1;
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestCode()
		{
			AssertEquals("C13", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("AddInfoDescription", Provider.Text);
		}

		protected override AdditionalInformationProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			cusSupportingInfo = Factory.CreateCusSupportingInfo("OTH", "INF", "AddInfoRefNum", null, "C13", header);
			cusSupportingInfo.CSI_Description = "AddInfoDescription";
			provider = new AdditionalInformationProvider(cusSupportingInfo);
		}

		AdditionalInformationProvider provider;
		CusSupportingInfo cusSupportingInfo;
	}
}
