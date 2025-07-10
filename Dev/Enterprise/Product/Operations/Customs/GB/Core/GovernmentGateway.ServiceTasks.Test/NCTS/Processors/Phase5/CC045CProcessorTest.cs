using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc045c;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC045CProcessorTest : NctsBaseProcessorTest<Cc045CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupNctsHeader,
					SetupGuaranteesAndTransactions = SetupGuaranteesAndTransactions,
					MRN = "23XI000081RN3DBHJ0",
					CorrelationIdentifier = "2TQJ37C0ELHE2X",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC045C_Message.xml"),
					MessageSubType = "45C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Write-Off notification for NCTS departure received on 02/08/2023",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					GuaranteesAndTransactionsAssertion = AssertTransactions1,
				};
				yield return new MessageProcessorTestCase
				{
					MRN = "23XI000081RN3DBHJ0",
					CorrelationIdentifier = "2TQJ37C0ELHE2X",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC045C_Message_WithGuarantor.xml"),
					MessageSubType = "45C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Write-Off notification for NCTS departure received on 03/08/2023</br>The guarantor for this declaration is G1 </br>MK11</br>GB",
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

		void SetupNctsHeader(NctsHeader nctsHeader)
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		}

		void AssertTransactions1(NctsHeader nctsHeader)
		{
			AssertTransactionsCore(nctsHeader, 3, 25000, PermitTransactionStatusList.Codes.Confirmed, $"NCTS write-off {nctsHeader.MovementHeader.BM_PaperlessInbondNum} [{nctsHeader.MovementReferenceNumber}]");
		}
	}
}
