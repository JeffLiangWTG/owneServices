using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDLO122ResponseMessage))]
	sealed class ILDLO122ResponseMessageTest : ILEDIResponseMessageTestBase<ILDLO122ResponseMessage>
	{
		protected override string GetExpectedMessageType() => "DLO";

		protected override string GetExpectedMessageSubType() => "122";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Delivery Order Response";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILDLO122ResponseMessageDataObject);

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderResponse_1220WithErrors.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderResponse_1220WithErrors.xml"));
	}
}
