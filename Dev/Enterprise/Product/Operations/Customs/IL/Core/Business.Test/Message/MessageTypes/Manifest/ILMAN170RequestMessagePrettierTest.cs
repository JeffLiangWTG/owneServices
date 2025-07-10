using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMAN170RequestMessagePrettierTest : ILEDIMessagePrettierTest<MnMsg1Manifest, ILMAN170RequestMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1170.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1170.xml"));

		protected override ILEDIMessage GetNewMessage() => Factory.New<ILMAN171ResponseMessage>();
	}
}
