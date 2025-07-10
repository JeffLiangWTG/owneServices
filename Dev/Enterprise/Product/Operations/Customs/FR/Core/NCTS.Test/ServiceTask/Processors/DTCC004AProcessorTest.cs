using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC004A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC004AProcessorTest : DTBaseProcessorTest<Cc004AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT004A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = FR.Business.NctsTransitStatusList.Codes.DeclarationRejected,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.AmendmentAccepted,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration Rejected</p>
<p>New detailed departure status: Amendment Accepted</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};
			}
		}

		protected override ZString initialDeclarationStatus => FR.Business.NctsTransitStatusList.Codes.DeclarationRejected;
	}
}
