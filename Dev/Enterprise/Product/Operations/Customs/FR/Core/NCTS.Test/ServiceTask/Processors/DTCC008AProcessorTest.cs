using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC008A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC008AProcessorTest : DTBaseProcessorTest<Cc008AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT008A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationRejected,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewArrivalStatus = Enterprise.Customs.FR.Business.NctsTransitStatusList.Codes.ArrivalRejected,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.ArrivalNotificationRejected,
					ExpectedNewMessageInterpretation = @"
<p>New arrival status: Arrival Rejected</p>
<p>New detailed arrival status: Arrival Notification Rejected</p>
<p>New message status: Arrival Notification Rejected</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Reason: Test Arrival rejection reason</p>
<p>Action required: Test Action to be taken</p>
<p>CODE of PLACE OF UNLOADING in HEADER is in error : Infrigement to rule NAT004 - Original value : Bergerac<br></p>"
				};
			}
		}
	}
}
