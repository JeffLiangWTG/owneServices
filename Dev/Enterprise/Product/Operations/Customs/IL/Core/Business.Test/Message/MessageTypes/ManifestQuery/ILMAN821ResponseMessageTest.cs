using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILMAN821ResponseMessage))]
	sealed class ILMAN821ResponseMessageTest : ILEDIResponseMessageTestBase<ILMAN821ResponseMessage>
	{
		protected override string GetExpectedMessageType() => "MAN";

		protected override string GetExpectedMessageSubType() => "821";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Manifest Query Response";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILMAN821ResponseMessageDataObject);

		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ManifestQueryResponse_8241.xml"));

		protected override EDIMessage GetNewMessage()
		{
			var message = (ILMAN821ResponseMessage)base.GetNewMessage();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			return message;
		}
	}
}
