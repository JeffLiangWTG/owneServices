using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc004c;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC004CProcessorTest : NctsBaseProcessorTest<Cc004CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					MRN = "23GB000246JHSAY3J5",
					CorrelationIdentifier = "37363989506266",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC004C_Message.xml"),
					MessageSubType = "04C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedNewMessageInterpretation = @"New detailed status: Amendment acceptance</br>
Status granted on: 01/08/2023 08:11:53</br>
Amendment submission date and time: 01/08/2023 08:12:27</br>
Correlation id: 37363989506266",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};

				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupNctsHeader,
					SetupGuaranteesAndTransactions = SetupGuaranteesAndTransactions,
					CorrelationIdentifier = "37363989506266",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC004C_Message.xml"),
					MessageSubType = "04C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedNewMessageInterpretation = @"New detailed status: Amendment acceptance</br>
Status granted on: 01/08/2023 08:11:53</br>
Amendment submission date and time: 01/08/2023 08:12:27</br>
Correlation id: 37363989506266",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					GuaranteesAndTransactionsAssertion = AssertTransactions,
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
					Description = "BM_CustomsStatus=ACK",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=PRE",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=AMR",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=GIV/BM_Phase=013/BH_MessageStatus=SNT",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=GIV/BM_Phase=013/BH_MessageStatus=ACK",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=GIV/BM_Phase=013/BH_MessageStatus=MOK",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
						header.EffectiveMessageStatus = NctsMessageStatusList.Codes.Ok;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=GIV/BM_Phase=013/BH_MessageStatus=ACC",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the message status of the declaration was ACC",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=GIV/BM_Phase=015",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the phase status of the declaration was 015",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=CAR",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.CancellationRequestedByCustoms;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was CAR",
				};
			}
		}

		void SetupNctsHeader(NctsHeader nctsHeader)
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_JobReference = "TRATESTGB12308021209";
			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			mrn.CE_EntryNum = "";
		}
	}
}
