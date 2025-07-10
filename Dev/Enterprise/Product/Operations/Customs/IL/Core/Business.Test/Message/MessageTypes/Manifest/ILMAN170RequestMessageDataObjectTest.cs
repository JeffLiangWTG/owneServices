
using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMAN170RequestMessageDataObjectTest : ILEDIMessageDataObjectTest<ILMAN170RequestMessageDataObject, MnMsg1Manifest, ILMAN171ResponseMessage>
	{
		protected override Type ExpectedPrettierType => typeof(ILMAN170RequestMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1170.xml"));
	}
}
