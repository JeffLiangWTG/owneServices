using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RequestResponseCollection))]
	sealed class RequestResponseCollectionTest : CusSupportingInfoCollectionTest<RequestResponse>
	{
		protected override CusSupportingInfoCollection<RequestResponse> GetCusSupportingInfoCollection()
		{
			var header = Factory.New<RequestHeader>();
			return new RequestResponseCollection(header);
		}

		public void TestDefaultReplyTypeFromHeader()
		{
			var collection = GetCusSupportingInfoCollection();

			var header = (RequestHeader)collection.Master;
			header.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var response = collection.AddNew();
			AssertEquals(string.Empty, response.CSI_SubType);

			header.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;

			response = collection.AddNew();
			AssertEquals(EUICS2HRCMAdditionalInfoTypes.Codes.CL703_R4, response.CSI_SubType);
		}
	}
}
