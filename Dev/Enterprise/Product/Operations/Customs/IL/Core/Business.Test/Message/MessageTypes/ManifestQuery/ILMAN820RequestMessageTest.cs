using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILMAN820RequestMessage))]
	sealed class ILMAN820RequestMessageTest : ILEDIRequestMessageTestBase<ILMAN820RequestMessage>
	{
		protected override string GetExpectedMessageType() => "MAN";

		protected override string GetExpectedMessageSubType() => "820";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Manifest Query Request";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILMAN820RequestMessageDataObject);

		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override ZString GetMessageText() => null;
	}
}
