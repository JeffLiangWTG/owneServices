using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS030;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS030MessageDataObjectTest : PNTSMessageDataObjectTest<IETS030MessageDataObject, Iets030>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS030_PLNResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS030MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationLinked;
	}
}
