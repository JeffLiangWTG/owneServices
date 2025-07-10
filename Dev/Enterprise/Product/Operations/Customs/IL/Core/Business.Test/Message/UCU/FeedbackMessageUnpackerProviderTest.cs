using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class FeedbackMessageUnpackerProviderTest : TestCaseWithFactory
	{
		public void TestGetFeedbackMessageUnpacker()
		{
			AssertType<FeedbackMessageUnpacker<ILDLO122ResponseMessage>>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message"));
			AssertType<FeedbackMessageUnpacker<ILGPM135ResponseMessage>>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("GP_NG_1035_MSG2_GatepassFeedbackMessage"));
			AssertType<FeedbackMessageUnpacker<ILDEC274ResponseMessage>>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("DF_NG_2754_MSG10004_ImportDeclarationResponse"));
			AssertType<FeedbackMessageUnpacker<ILMAN171ResponseMessage>>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("MN_MSG4_SendManifestFeedBack_Message"));
			AssertType<FeedbackMessageUnpacker<ILMAN821ResponseMessage>>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("MN_NG_8241_Cargo_Message"));
			AssertType<FeedbackMessageUnpacker<ILDOC276ResponseMessage>>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("D_NG_2716_MSG22001_AddAttachmentResponse"));
			AssertType<FeedbackMessageUnpacker<ILDOC828ResponseMessage>>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("VAL_NG_8228_MSG550_RequiredDocumentVerificationDecisionMessage"));
			AssertType<OutgoingMessageResponseUnpacker>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("NG_9101_MSG_OutgoingMessageResponse"));
			AssertType<InfMsgGenericUnpacker>(FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker("INF_MSG_Generic"));
		}
	}
}
