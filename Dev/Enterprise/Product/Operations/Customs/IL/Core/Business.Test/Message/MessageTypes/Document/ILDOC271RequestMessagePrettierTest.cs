using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271.D_NG_2715_MSG22002_AddAGlobalScannedAttachmentToEntity;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDOC271RequestMessagePrettierTest : ILEDIMessagePrettierTest<DNg2715Msg22002AddAGlobalScannedAttachmentToEntity, ILDOC271RequestMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("SendDocumentToCustomsMessage_2715_Interpretation.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("SendDocumentToCustomsMessage_2715.xml"));

		protected override ILEDIMessage GetNewMessage() => Factory.New<ILDLO120RequestMessage>();
	}
}
