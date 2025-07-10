using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC035A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC035AProcessorTest : DTBaseProcessorTest<Cc035AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT035A_MESSAGE.xml"),
					SetUpHeader = header => header.IsQueried = true,
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.RecoveryProcedure,
					ExpectedNewMessageInterpretation = @"
<p>New detailed departure status: Recovery Procedure</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Recovery date: 06/06/2020</p>
<p>Notification date: 08/06/2020</p>
<p>Detail: Test Detail</p>
<p>Amount claimed: 408.66EUR</p>
<p>Guarantee ID: GUA0045</p>
<p>Office of departure: FR0040040</p>
<p>Authority of recovery: FR0040060</p>",
					HeaderAssertion = header => AssertEquals(false, header.IsQueried)
				};
			}
		}
	}
}
