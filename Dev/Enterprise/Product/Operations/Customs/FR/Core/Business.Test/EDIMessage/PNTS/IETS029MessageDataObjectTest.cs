using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS029;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS029MessageDataObjectTest : PNTSMessageDataObjectTest<IETS029MessageDataObject, Iets029>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS029_TEPResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS029MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.ProofOfUnionStatusPresented;
	}
}
