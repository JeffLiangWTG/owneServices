using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc009c;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC009CProcessorTest : NctsBaseProcessorTest<Cc009CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupNctsHeader,
					SetupGuaranteesAndTransactions = SetupGuaranteesAndTransactions,
					LRN = "TRATESTGB92308011429",
					MRN = "23GB000246ZPABRDJ3",
					CorrelationIdentifier = "27280645444926",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC009C_Message.xml"),
					MessageSubType = "09C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					InitialPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Cancellation,
					ExpectedNewMessageInterpretation = @"New declaration status: Cancellation accepted</br>
Status granted on: 01/08/2023 02:30:52</br>
Request date and time to invalidate/cancel: 01/08/2023 02:30:52</br>
Initiated by customs: no</br>
Justification: Container is broken</br>
Correlation id: 27280645444926",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					GuaranteesAndTransactionsAssertion = AssertTransactions1,
				};
				yield return new MessageProcessorTestCase
				{
					LRN = "TRATESTGB92308011429",
					MRN = "23GB000246ZPABRDJ3",
					CorrelationIdentifier = "27280645444926",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC009C_Denied.xml"),
					MessageSubType = "09C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Invalid,
					InitialPhase = NCTS5DeparturePhaseList.Codes.Cancellation,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"New declaration status: Cancellation refused</br>
Status granted on: 01/08/2023 23:59:59</br>
Request date and time to invalidate/cancel: 01/08/2023 02:30:52</br>
Initiated by customs: yes</br>
Justification: Nope: denied</br>
Correlation id: 27280645444926",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
				yield return new MessageProcessorTestCase
				{
					LRN = "TRATESTGB92308011429",
					MRN = "23GB000246ZPABRDJ3",
					CorrelationIdentifier = "27280645444926",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC009C_Denied.xml"),
					MessageSubType = "09C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Sent,
					InitialPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"New declaration status: Cancellation refused</br>
Status granted on: 01/08/2023 23:59:59</br>
Request date and time to invalidate/cancel: 01/08/2023 02:30:52</br>
Initiated by customs: yes</br>
Justification: Nope: denied</br>
Correlation id: 27280645444926",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
			}
		}

		void SetupNctsHeader(NctsHeader nctsHeader)
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_JobReference = "TRATESTGB92308011429";
		}

		void AssertTransactions1(NctsHeader nctsHeader)
		{
			AssertTransactionsCore(nctsHeader, 2, -25000m, PermitTransactionStatusList.Codes.Deleted);
		}
	}
}
