using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDOC271RequestMessage))]
	sealed class ILDOC271RequestMessageTest : ILEDIRequestMessageTestBase<ILDOC271RequestMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override string GetExpectedMessageSubType() => "271";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Supporting Documents Request";

		protected override ZString GetMessageText() => string.Empty;

		protected override string GetExpectedMessageType() => "DOC";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILDOC271RequestMessageDataObject);
	}
}
