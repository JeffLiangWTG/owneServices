using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820.MN_NG_8240_CargoQuery_Message;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class ILMAN820RequestMessagePrettierTest : ILEDIMessagePrettierTest<MnNg8240CargoQueryMessage, ILMAN820RequestMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ILManQueryMessageRequest_820.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ILManQueryMessageRequest_820.xml"));

		protected override ILEDIMessage GetNewMessage() => Factory.New<ILMAN820RequestMessage>();
	}
}
