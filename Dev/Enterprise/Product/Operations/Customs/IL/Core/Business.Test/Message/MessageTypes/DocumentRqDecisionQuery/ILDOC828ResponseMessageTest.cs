using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDOC828ResponseMessage))]
	sealed class ILDOC828ResponseMessageTest : ILEDIResponseMessageTestBase<ILDOC828ResponseMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => null;

		protected override string GetExpectedMessageSubType() => "828";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Supporting Documents Request Decision Response";

		protected override string GetExpectedMessageType() => "DOC";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILDOC828ResponseMessageDataObject);

		protected override ZString GetMessageText() => string.Empty;
	}
}
