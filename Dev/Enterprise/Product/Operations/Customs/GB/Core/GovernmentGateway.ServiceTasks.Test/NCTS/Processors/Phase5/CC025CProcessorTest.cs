using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc025c;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC025CProcessorTest : NctsBaseProcessorTest<Cc025CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					MRN = "23XI000081RN3DBHJ0",
					CorrelationIdentifier = "51840573406184",
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC025C_Message.xml"),
					MessageSubType = "25C",
					ExpectedNewArrivalStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedNewMessageInterpretation = "All Goods are released for transit upon arrival. The movement is closed.",
					ExpectedMovementType = NctsMovementType.Codes.Arrival,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Arrival,
				};
			}
		}
	}
}
