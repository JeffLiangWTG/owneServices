using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF97A;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCCF97AProcessorTest : DTBaseProcessorTest<Ccf97AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF97A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New message status: Message Syntax or Business Rule Errors</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Error: Max characters exceeded(Header, line 7) - Original value : <br>Error: Other formatting error(, line 8) - Original value : 254%¨@*]<br></p>"
				};
			}
		}
	}
}
