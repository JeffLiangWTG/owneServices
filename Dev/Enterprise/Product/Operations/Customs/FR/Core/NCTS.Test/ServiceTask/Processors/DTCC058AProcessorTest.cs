using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC058A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC058AProcessorTest : DTBaseProcessorTest<Cc058AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT058A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.UnloadingRemarksRejected,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.UnloadingRemarksRejected,
					ExpectedNewMessageInterpretation = @"
<p>New detailed arrival status: Unloading Remarks Rejected</p>
<p>New message status: Unloading Remarks Rejected</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>DATE LIMIT of CONTROL RESULT is in error : Infrigement to rule NAT003 - Original value : 20180425<br>CODE of PLACE OF UNLOADING in HEADER is in error : Infrigement to rule NAT004 - Original value : Bergerac<br></p>"
				};
			}
		}
	}
}
