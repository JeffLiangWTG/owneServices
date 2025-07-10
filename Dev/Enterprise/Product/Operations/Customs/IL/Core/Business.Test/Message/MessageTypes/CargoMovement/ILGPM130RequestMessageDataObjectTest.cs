using System;
using CargoWise.Customs.IL.MessageDefinitions.GPM.REQ_130.GP_NG_1030_MSG1_GatepassRequestMessage;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGPM130RequestMessageDataObjectTest : ILEDIMessageDataObjectTest<ILGPM130RequestMessageDataObject, GpNg1030Msg1GatepassRequestMessage, ILGPM130RequestMessage>
	{
		protected override Type ExpectedPrettierType => typeof(ILGPM130RequestMessagePrettier);

		protected override ZString GetMessageText() => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassRequestMessage1030.xml"));
	}
}
