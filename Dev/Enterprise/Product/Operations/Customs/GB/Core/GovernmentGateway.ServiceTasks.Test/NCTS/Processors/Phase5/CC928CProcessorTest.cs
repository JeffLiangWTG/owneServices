using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc928c;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC928CProcessorTest : NctsBaseProcessorTest<Cc928CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					LRN = "TRATESTGB12308021209",
					CorrelationIdentifier = "28864312700322",
					SetUpHeader = header => header.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC928C_Message.xml"),
					MessageSubType = "928",
					OutgoingMessageSubType = "015",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"New declaration status: Declaration Accepted</br>
Correlation id: 28864312700322",
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
					Description = "EffectiveMessageStatus=SNT",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = ZString.Empty;
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "EffectiveMessageStatus=ACK",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = ZString.Empty;
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "EffectiveMessageStatus=MOK",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = ZString.Empty;
						header.EffectiveMessageStatus = NctsMessageStatusList.Codes.Ok;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "EffectiveMessageStatus=FAL",
					Setup = header =>
					{
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Failed;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the message status of the declaration was FAL",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "EffectiveMessageStatus=INV",
					Setup = header =>
					{
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the message status of the declaration was INV",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=MRN",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was MRN",
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
