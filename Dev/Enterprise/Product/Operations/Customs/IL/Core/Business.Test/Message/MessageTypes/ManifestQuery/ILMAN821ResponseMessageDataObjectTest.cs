
using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.RES_821.MN_NG_8241_Cargo_Message;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMAN821ResponseMessageDataObjectTest : ILEDIMessageDataObjectTest<ILMAN821ResponseMessageDataObject, MnNg8241CargoMessage, ILMAN821ResponseMessage>
	{
		protected override Type ExpectedPrettierType => null;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ManifestQueryResponse_8241.xml"));
	}
}
