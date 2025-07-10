using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS906;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS906MessageDataObjectTest : PNTSMessageDataObjectTest<IETS906MessageDataObject, Iets906>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS906ResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS906MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => ZString.Empty;
	}
}
