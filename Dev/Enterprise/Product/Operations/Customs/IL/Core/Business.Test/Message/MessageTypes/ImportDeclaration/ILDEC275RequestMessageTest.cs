using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDEC275RequestMessage))]
	sealed class ILDEC275RequestMessageTest : ILEDIRequestMessageTestBase<ILDEC275RequestMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override string GetExpectedMessageSubType() => "275";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Import Declaration Request";

		protected override ZString GetMessageText() => null;

		protected override string GetExpectedMessageType() => "DEC";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILDEC275RequestMessageDataObject);
	}
}
