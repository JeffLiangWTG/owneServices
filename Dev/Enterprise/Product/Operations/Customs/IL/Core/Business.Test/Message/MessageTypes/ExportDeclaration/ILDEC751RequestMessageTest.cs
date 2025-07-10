using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDEC751RequestMessage))]
	sealed class ILDEC751RequestMessageTest : ILEDIRequestMessageTestBase<ILDEC751RequestMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override string GetExpectedMessageSubType() => "751";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Export Declaration Request";

		protected override ZString GetMessageText() => null;

		protected override string GetExpectedMessageType() => "DEC";

		protected override Type GetExpectedTypeOfMessageDataObject() => null;
	}
}
