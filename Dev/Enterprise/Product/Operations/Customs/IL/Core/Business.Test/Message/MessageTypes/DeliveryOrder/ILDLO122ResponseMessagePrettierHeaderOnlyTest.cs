using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDLO122ResponseMessagePrettierHeaderOnlyTest : ILEDIMessagePrettierTest<MnNg1220Msg22DeliveryOrderFeedBackMessage, ILDLO122ResponseMessageDataObject>
	{
		protected override ILEDIMessage GetNewMessage() => Factory.New<ILDLO122ResponseMessage>();

		public ZString GetExpectedMessageInterpretation_Exposed() => GetExpectedMessageInterpretation();

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderResponse_1220WithExceptionInHeader.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderResponse_1220WithExceptionInHeader.xml"));
	}
}
