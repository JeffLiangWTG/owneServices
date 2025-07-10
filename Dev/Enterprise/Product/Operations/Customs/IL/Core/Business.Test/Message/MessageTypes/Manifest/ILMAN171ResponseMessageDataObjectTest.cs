using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMAN171ResponseMessageDataObjectTest : ILEDIMessageDataObjectTest<ILMAN171ResponseMessageDataObject, MnMsg4SendManifestFeedBackMessage, ILMAN171ResponseMessage>
	{
		protected override Type ExpectedPrettierType => typeof(ILMAN171ResponseMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171.xml"));
	}
}
