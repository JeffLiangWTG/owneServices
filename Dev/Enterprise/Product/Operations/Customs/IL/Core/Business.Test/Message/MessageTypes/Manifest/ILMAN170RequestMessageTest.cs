using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILMAN170RequestMessage))]
	sealed class ILMAN170RequestMessageTest : ILEDIRequestMessageTestBase<ILMAN170RequestMessage>
	{
		protected override string GetExpectedMessageType() => "MAN";

		protected override string GetExpectedMessageSubType() => "170";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Forwarder Manifest Request";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILMAN170RequestMessageDataObject);

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1170.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1170.xml"));
	}
}
