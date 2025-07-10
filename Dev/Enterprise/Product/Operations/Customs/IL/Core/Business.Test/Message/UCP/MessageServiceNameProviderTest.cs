using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageServiceNameProviderTest : TestCaseWithFactory
	{
		public void TestGetServiceName()
		{
			AssertEquals("SaveMN_MSG1200_1220_DeliveryOrder_Message", MessageServiceNameProvider.GetServiceName("120"));
			AssertEquals("GetGP_MSG1030_1035_GatepassFeedbackMessage", MessageServiceNameProvider.GetServiceName("130"));
			AssertEquals("SaveMN_MSG1170_1171_MANIFESTRequest", MessageServiceNameProvider.GetServiceName("170"));
			AssertEquals("GetDOC_MSG2715_2716_AddAttachmentResponse", MessageServiceNameProvider.GetServiceName("271"));
			AssertEquals("SaveDF_MSG2750_2754_ImportDeclarationRequest", MessageServiceNameProvider.GetServiceName("275"));
			AssertEquals("GetCD_8347_8348_Web01_02_CurrencyRateSearch", MessageServiceNameProvider.GetServiceName("347"));
			AssertEquals("GetMN_MSG_8240_8241_CargoQuery_Message", MessageServiceNameProvider.GetServiceName("820"));
			AssertEquals("GetSYSTBL_MSG9000_9001_SystemTableRequest", MessageServiceNameProvider.GetServiceName("901"));
			AssertEquals("Get_9100_OutgoingMessageRequest", MessageServiceNameProvider.GetServiceName("910"));
			AssertEquals("GET_9200_OutgoingMessageDeliveryApproval", MessageServiceNameProvider.GetServiceName("920"));
			AssertEquals("SaveDF_MSG2751_ExportDeclaration", MessageServiceNameProvider.GetServiceName("751"));
			AssertEquals("", MessageServiceNameProvider.GetServiceName("NotValid"));
		}
	}
}
