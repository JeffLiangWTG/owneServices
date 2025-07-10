using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDEC274ResponseMessage))]
	sealed class ILDEC274ResponseMessageTest : ILEDIResponseMessageTestBase<ILDEC274ResponseMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override string GetExpectedMessageSubType() => "274";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Import Declaration Response";

		protected override ZString GetMessageText() => null;

		protected override string GetExpectedMessageType() => "DEC";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILDEC274ResponseMessageDataObject);
	}
}
