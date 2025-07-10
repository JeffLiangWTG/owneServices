using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC005A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC005AProcessorTest : DTBaseProcessorTest<Cc005AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT005A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = FR.Business.NctsTransitStatusList.Codes.DeclarationRejected,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.AmendmentRefused,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration Rejected</p>
<p>New detailed departure status: Amendment Refused</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Rejection reason: Not registered</p>
<p>Rejection comment: The broker is not registered yet.</p>
<p>CODE of PLACE OF UNLOADING in HEADER is in error : Infrigement to rule NAT004 - Original value : Bergerac<br></p>"
				};
			}
		}

		protected override ZString initialDeclarationStatus => FR.Business.NctsTransitStatusList.Codes.DeclarationRejected;
	}
}
