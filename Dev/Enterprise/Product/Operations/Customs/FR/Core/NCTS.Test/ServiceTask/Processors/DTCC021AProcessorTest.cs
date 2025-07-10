using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC021A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC021AProcessorTest : DTBaseProcessorTest<Cc021AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT021A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewArrivalStatus = Enterprise.Customs.FR.Business.NctsTransitStatusList.Codes.ArrivalRejected,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.AlternateDestinationRejected,
					ExpectedNewMessageInterpretation = @"
<p>New arrival status: Arrival Rejected</p>
<p>New detailed arrival status: Alternate Destination Rejected</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Reason: Valar Morghulis</p>"
				};
			}
		}
	}
}
