using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF96A;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCCF96AProcessorTest : DTBaseProcessorTest<Ccf96AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF96A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New message status: Message Syntax or Business Rule Errors</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>DATE LIMIT of CONTROL RESULT is in error : Infrigement to rule NAT003 - Original value : 20180425<br>CODE of PLACE OF UNLOADING in HEADER is in error : Infrigement to rule NAT004 - Original value : Bergerac<br></p>"
				};
			}
		}
	}
}
