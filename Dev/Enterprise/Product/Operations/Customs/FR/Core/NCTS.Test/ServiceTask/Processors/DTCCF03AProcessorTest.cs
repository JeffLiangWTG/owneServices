using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF03A;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCCF03AProcessorTest : DTBaseProcessorTest<Ccf03AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF03A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.TransitNotifiedAtDestination,
					ExpectedNewMessageInterpretation = @"
<p>New detailed departure status: Transit Notified at Destination</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};
			}
		}
	}
}
