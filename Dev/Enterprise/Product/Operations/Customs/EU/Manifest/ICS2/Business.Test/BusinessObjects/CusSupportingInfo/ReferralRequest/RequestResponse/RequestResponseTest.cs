using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RequestResponse))]
	sealed class RequestResponseTest : CusSupportingInfoTest<RequestResponse>
	{
		public void TestSetCodeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2AI", "IC2AI");
			helper.CreateCusCodeList("EUN", "IC2AI", "0586", "TestCodeDescription", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			Factory.Save();

			var requestResponse = Factory.NewWithValidTestData<RequestResponse>();
			requestResponse.CSI_Code = "0586";
			AssertEquals("Set code description after set code", "TestCodeDescription", requestResponse.CodeDescription);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var requestResponseInNewFactory = newFactory.Load<RequestResponse>(requestResponse.PK);
			AssertEquals("Set code description when load data from factory", "TestCodeDescription", requestResponseInNewFactory.CodeDescription);
		}

		public void TestSetTypeDescription()
		{
			var requestResponse = Factory.NewWithValidTestData<RequestResponse>();
			requestResponse.CSI_SubType = EUICS2HRCMAdditionalInfoTypes.Codes.CL703_C1;
			AssertEquals("Set type description after set code", EUICS2HRCMAdditionalInfoTypes.Descriptions.CL703_C1, requestResponse.SubTypeDescription);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var requestResponseInNewFactory = newFactory.Load<RequestResponse>(requestResponse.PK);
			AssertEquals("Set type description after set code", EUICS2HRCMAdditionalInfoTypes.Descriptions.CL703_C1, requestResponseInNewFactory.SubTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			var requestHeader = header.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "XXX";
			requestHeader.EUS_Type = "YYY";
			return requestHeader.RequestResponses.AddNew();
		}
	}
}
