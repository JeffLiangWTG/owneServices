using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILXERResponseMessage))]
	sealed class ILXERResponseMessageTest : ILEDIResponseMessageTestBase<ILXERResponseMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override string GetExpectedMessageSubType() => null;

		protected override ZString GetExpectedMessageSubTypeDescription() => null;

		protected override ZString GetMessageText() => null;

		protected override string GetExpectedMessageType() => "XER";

		protected override Type GetExpectedTypeOfMessageDataObject() => null;
	}
}
