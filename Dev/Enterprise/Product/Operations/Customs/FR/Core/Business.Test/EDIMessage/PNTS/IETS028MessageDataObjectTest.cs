using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS028;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS028MessageDataObjectTest : PNTSMessageDataObjectTest<IETS028MessageDataObject, Iets028>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS028ResponseMessageForIETS007.xml");

		protected override Type ExpectedPrettierType => typeof(IETS028MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.ProofOfUnionStatusPresented;
	}
}
