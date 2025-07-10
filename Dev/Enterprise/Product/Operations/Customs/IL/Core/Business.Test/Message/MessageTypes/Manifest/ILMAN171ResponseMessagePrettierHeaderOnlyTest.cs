using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMAN171ResponseMessagePrettierHeaderOnlyTest : ILEDIMessagePrettierTest<MnMsg4SendManifestFeedBackMessage, ILMAN171ResponseMessageDataObject>
	{
		protected override ILEDIMessage GetNewMessage() => Factory.New<ILMAN171ResponseMessage>();

		public ZString GetExpectedMessageInterpretation_Exposed() => GetExpectedMessageInterpretation();

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_HeaderOnly_Interpretation.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_HeaderOnly.xml"));
	}
}
