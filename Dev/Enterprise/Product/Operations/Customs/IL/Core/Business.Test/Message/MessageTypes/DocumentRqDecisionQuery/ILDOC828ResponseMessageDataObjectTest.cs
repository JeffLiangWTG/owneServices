using System;
using CargoWise.Customs.IL.MessageDefinitions.DOC.RES_828.VAL_NG_8228_MSG550_RequiredDocumentVerificationDecisionMessage;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	internal class ILDOC828ResponseMessageDataObjectTest : ILEDIMessageDataObjectTest<ILDOC828ResponseMessageDataObject, ValNg8228Msg550RequiredDocumentVerificationDecisionMessage, ILDOC828ResponseMessage>
	{
		protected override Type ExpectedPrettierType => null;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("RequiredDocumentVerificationDecisionMessageResponse_8228.xml"));
	}
}
