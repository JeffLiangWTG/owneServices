using System;
using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDLO122ResponseMessageDataObjectTest : ILEDIMessageDataObjectTest<ILDLO122ResponseMessageDataObject, MnNg1220Msg22DeliveryOrderFeedBackMessage, ILDLO122ResponseMessage>
	{
		protected override Type ExpectedPrettierType => typeof(ILDLO122ResponseMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderResponse_1220.xml"));
	}
}
