using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC019A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC019AProcessorTest : DTBaseProcessorTest<Cc019AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT019A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.DiscrepanciesNotification,
					ExpectedNewMessageInterpretation = @"
<p>New detailed departure status: Discrepancies Notification</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};
			}
		}
	}
}
