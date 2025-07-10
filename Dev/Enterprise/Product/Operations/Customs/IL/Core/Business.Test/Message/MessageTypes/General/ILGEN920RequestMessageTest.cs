using System;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILGEN920RequestMessage))]
	sealed class ILGEN920RequestMessageTest : ILEDIRequestMessageTestBase<ILGEN920RequestMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => ZString.Empty;

		protected override string GetExpectedMessageSubType() => "920";

		protected override ZString GetMessageText() => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("OutgoingMessageRequest_9200.xml"));

		protected override string GetExpectedMessageType() => ILMessageTypeList.Codes.GEN;

		protected override Type GetExpectedTypeOfMessageDataObject() => null;

		protected override ZString GetExpectedMessageSubTypeDescription() => "Synchronization Acknowledgement Message";
	}
}
