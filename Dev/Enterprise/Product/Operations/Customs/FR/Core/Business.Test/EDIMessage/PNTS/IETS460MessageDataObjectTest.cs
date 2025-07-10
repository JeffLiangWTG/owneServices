using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS460;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS460MessageDataObjectTest : PNTSMessageDataObjectTest<IETS460MessageDataObject, Iets460>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS460ResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS460MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl;
	}
}
