using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC140A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC140AProcessorTest : DTBaseProcessorTest<Cc140AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT140A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.ResearchProcedureNotification,
					ExpectedNewMessageInterpretation = @"
<p>New detailed departure status: Research Procedure Notification</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Answer limit date: 06/05/2020</p>
<p>Customs office of departure: FR00200100</p>
<p>Authority of recovery: FR00200200</p>",
					HeaderAssertion = header => AssertEquals(expected: true, actual: header.IsQueried)
				};
			}
		}
	}
}
