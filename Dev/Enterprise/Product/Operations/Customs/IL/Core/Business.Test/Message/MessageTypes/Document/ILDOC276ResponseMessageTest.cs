using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILDOC276ResponseMessage))]
	sealed class ILDOC276ResponseMessageTest : ILEDIResponseMessageTestBase<ILDOC276ResponseMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("AddAttachmentResponse_276_WithErrors.html"));

		protected override string GetExpectedMessageSubType() => "276";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Supporting Documents Response";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("AddAttachmentResponse_276_WithErrors.xml"));

		protected override string GetExpectedMessageType() => "DOC";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILDOC276ResponseMessageDataObject);
	}
}
