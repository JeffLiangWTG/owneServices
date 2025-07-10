using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF15A;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCCF15AProcessorTest : DTBaseProcessorTest<Ccf15AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF15A_MESSAGE.xml"),
					ExpectedNewMessageStatus = ZString.Empty,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = ZString.Empty
				};
			}
		}

		protected override ZString ExpectedMessageType => "15F";
	}
}
