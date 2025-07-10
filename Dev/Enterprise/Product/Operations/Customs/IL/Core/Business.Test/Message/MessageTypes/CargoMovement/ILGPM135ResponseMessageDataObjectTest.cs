using System;
using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGPM135ResponseMessageDataObjectTest : ILEDIMessageDataObjectTest<ILGPM135ResponseMessageDataObject, GpNg1035Msg2GatepassFeedbackMessage, ILGPM135ResponseMessage>
	{
		protected override Type ExpectedPrettierType => typeof(ILGPM135ResponseMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassFeedbackMessage.xml"));
	}
}
