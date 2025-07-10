using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc055c;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC055CProcessorTest : NctsBaseProcessorTest<Cc055CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					MRN = "23GB000246F5YWI4J2",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC055C_Message.xml"),
					MessageSubType = "55C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Guarantee invalid</br>
Guarantee sequence: 1</br>
GRN 23XI00000100000D0</br>
Reason : code: G04 Holder of Guarantee is not equal to Holder of Transit procedure in declaration</br>
Reason : text: </br>",
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
					Description = "BM_CustomsStatus=MRN",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
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
					Description = "BM_CustomsStatus=GIV",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was GIV",
				};
			}
		}
	}
}
