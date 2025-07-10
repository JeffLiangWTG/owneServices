using CargoWise.Customs.IL.MessageDefinitions.DOC.RES_276.D_NG_2716_MSG22001_AddAttachmentResponse;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDOC276ResponseMessagePrettierTest : ILEDIMessagePrettierTest<DNg2716Msg22001AddAttachmentResponse, ILDOC276ResponseMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("AddAttachmentResponse_276_WithErrors.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("AddAttachmentResponse_276_WithErrors.xml"));

		protected override ILEDIMessage GetNewMessage() => Factory.New<ILDOC276ResponseMessage>();
	}
}
