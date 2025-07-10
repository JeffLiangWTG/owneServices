using CargoWise.Customs.IL.MessageDefinitions.DLO.REQ_120.MN_NG_1200_MSG2_DeliveryOrder_Message;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDLO120RequestMessagePrettierTest : ILEDIMessagePrettierTest<MnNg1200Msg2DeliveryOrderMessage, ILDLO120RequestMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderRequest_1200_WithData.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderRequest_1200_WithData.xml"));

		protected override ILEDIMessage GetNewMessage() => Factory.New<ILDLO122ResponseMessage>();
	}
}
