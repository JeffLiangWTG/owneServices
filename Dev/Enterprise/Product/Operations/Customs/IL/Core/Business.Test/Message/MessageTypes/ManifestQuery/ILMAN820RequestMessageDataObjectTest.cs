using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820.MN_NG_8240_CargoQuery_Message;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMAN820RequestMessageDataObjectTest : ILEDIMessageDataObjectTest<ILMAN820RequestMessageDataObject, MnNg8240CargoQueryMessage, ILMAN820RequestMessage>
	{
		protected override Type ExpectedPrettierType => typeof(ILMAN820RequestMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ILManQueryMessageRequest_820.xml"));
	}
}
