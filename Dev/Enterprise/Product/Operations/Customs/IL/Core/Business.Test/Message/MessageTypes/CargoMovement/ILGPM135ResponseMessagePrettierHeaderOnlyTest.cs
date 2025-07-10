using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGPM135ResponseMessagePrettierHeaderOnlyTest : ILEDIMessagePrettierTest<GpNg1035Msg2GatepassFeedbackMessage, ILGPM135ResponseMessageDataObject>
	{
		protected override ILEDIMessage GetNewMessage() => Factory.New<ILGPM135ResponseMessage>();

		public ZString GetExpectedMessageInterpretation_Exposed() => GetExpectedMessageInterpretation();

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassFeedbackMessage_WithExceptionInHeader_Interpretation.html")).Replace("\r\n", "");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassFeedbackMessage_WithExceptionInHeader.xml"));
	}
}
