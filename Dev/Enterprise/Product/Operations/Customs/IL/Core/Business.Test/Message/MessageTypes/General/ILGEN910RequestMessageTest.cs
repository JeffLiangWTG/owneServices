using System;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILGEN910RequestMessage))]
	sealed class ILGEN910RequestMessageTest : ILEDIRequestMessageTestBase<ILGEN910RequestMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => ZString.Empty;

		protected override string GetExpectedMessageSubType() => "910";

		protected override ZString GetMessageText() => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("OutgoingMessageRequest_9100.xml"));

		protected override string GetExpectedMessageType() => ILMessageTypeList.Codes.GEN;

		protected override Type GetExpectedTypeOfMessageDataObject() => null;

		protected override ZString GetExpectedMessageSubTypeDescription() => "Synchronization Outgoing Message Request";
	}
}
