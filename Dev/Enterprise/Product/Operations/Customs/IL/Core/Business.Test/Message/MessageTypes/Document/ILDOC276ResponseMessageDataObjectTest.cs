using System;
using CargoWise.Customs.IL.MessageDefinitions.DOC.RES_276.D_NG_2716_MSG22001_AddAttachmentResponse;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDOC276ResponseMessageDataObjectTest : ILEDIMessageDataObjectTest<ILDOC276ResponseMessageDataObject, DNg2716Msg22001AddAttachmentResponse, ILDOC276ResponseMessage>
	{
		protected override Type ExpectedPrettierType => typeof(ILDOC276ResponseMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("AddAttachmentResponse_276.xml"));
	}
}
