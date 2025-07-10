using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc035c;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC035CProcessorTest : NctsBaseProcessorTest<Cc035CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					MRN = "23XI000081RN3DBHJ0",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC035C_Message.xml"),
					MessageSubType = "35C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Recovery notification for NCTS departure received on 01/08/2023</br>
</br>
Amount recovered: 10 EUR",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
				yield return new MessageProcessorTestCase
				{
					MRN = "23XI000081RN3DBHJ0",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC035C_Message_WithGuarantor.xml"),
					MessageSubType = "35C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Recovery notification for NCTS departure received on 02/08/2023</br>
</br>
Amount recovered: 0 </br>
The guarantor for this declaration is G1 </br>
MILTON KEYNES</br>
GB",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
			}
		}

		protected override IEnumerable<MessageShouldBeDiscardedTestCase> MessageShouldBeDiscardedTestCases
		{
			get
			{
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=MRN",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=WRO",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was WRO",
				};
			}
		}
	}
}
