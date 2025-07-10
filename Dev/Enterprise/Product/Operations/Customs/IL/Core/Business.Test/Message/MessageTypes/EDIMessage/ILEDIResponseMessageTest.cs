using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILEDIResponseMessage))]
	sealed class ILEDIResponseMessageTest : ILEDIResponseMessageTestBase<ILEDIResponseMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => ZString.Empty;

		protected override string GetExpectedMessageSubType() => null;

		protected override ZString GetExpectedMessageSubTypeDescription() => ZString.Empty;

		protected override ZString GetMessageText() => "Dummy message text";

		protected override string GetExpectedMessageType() => null;

		protected override Type GetExpectedTypeOfMessageDataObject() => null;
	}
}
