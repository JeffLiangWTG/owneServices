using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS016;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS016MessageDataObjectTest : PNTSMessageDataObjectTest<IETS016MessageDataObject, Iets016>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS016ResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS016MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => ZString.Empty;
	}
}
