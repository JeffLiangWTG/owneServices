using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS095;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IETS095MessageDataObjectTest : PNTSMessageDataObjectTest<IETS095MessageDataObject, Iets095>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS095_TMRResponseMessage.xml");

		protected override Type ExpectedPrettierType => typeof(IETS095MessagePrettier);

		protected override ZString ExpectedNewCustomsStatus => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.MeasuresRequired;

		public void TestStatusReason()
		{
			AssertEquals($"Status Reason of IETS095", "Irregularity at timer expiration", messageDataObject.GetStatusReasonDescriptionFromMessage());
		}
	}
}
