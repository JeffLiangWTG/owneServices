using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS928;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS928MessageDataObjectTest : PNTSMessageDataObjectTest<IETS928MessageDataObject, Iets928>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS928ResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS928MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => ZString.Empty;
	}
}
