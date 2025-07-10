using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDLO120RequestMessage))]
	sealed class ILDLO120RequestMessageTest : ILEDIMessageWithPlaceHolderTestBase<ILDLO120RequestMessage>
	{
		protected override string GetExpectedMessageType() => "DLO";

		protected override string GetExpectedMessageSubType() => "120";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Delivery Order Request";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILDLO120RequestMessageDataObject);

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderRequest_1200_WithData.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderRequest_1200_WithData.xml"));

		protected override string GetExampleRequestMessage() => "DeliveryOrderRequest_1200.xml";

		protected override string GetReferenceTag() => "deliveryOrderNumber";

		protected override IILElectronicMessageProvider GetNewEmptyElectronicMessageProvider()
			=> Factory.New<ForwardingShipment>().DeliveryOrderProvider;

		protected override string GetRequestMessageSubType() => ILEDIMessageSubTypeList.Codes.DeliveryOrderResponse;
	}
}
