using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS410;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS410MessageDataObjectTest : PNTSMessageDataObjectTest<IETS410MessageDataObject, Iets410>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS410ResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS410MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;
	}
}
