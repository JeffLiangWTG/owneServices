using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc019c;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC019CProcessorTest : NctsBaseProcessorTest<Cc019CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				CreateZZRefTestValuesIfNeeded("XI000081", Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, ["DES"], "NI Auth Consignor/nees");
				yield return new MessageProcessorTestCase
				{
					MRN = "23XI000081RN3DBHJ0",
					CorrelationIdentifier = "CYC37UE12PZO0S",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC019C_Message.xml"),
					MessageSubType = "19C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"<h2>Discrepancies at Destination</h2><table><tr><td>MRN:</td><td>23XI000081RN3DBHJ0.</td></tr><tr><td>Customs Office of Departure:</td><td>XI000081 NI Auth Consignor/nees</td></tr></table><h3>Notification</h3><table><tr><td>Notification Text:</td><td>unsatisfactory</td></tr><tr><td>Notification Date:</td><td>01-08-2023</td></tr></table><h4>Guarantor</h4><table></table><h4>Holder Of The Transit Procedure</h4><table><tr><td>Holder Of The Transit Procedure Identification Number:</td><td>XI175521246821</td></tr></table>",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
				yield return new MessageProcessorTestCase
				{
					MRN = "23XI000081RN3DBHJ0",
					CorrelationIdentifier = "CYC37UE12PZO0S",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC019C_Message_NoNotificationText.xml"),
					MessageSubType = "19C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"<h2>Discrepancies at Destination</h2><table><tr><td>MRN:</td><td>23XI000081RN3DBHJ0.</td></tr><tr><td>Customs Office of Departure:</td><td>XI000081 NI Auth Consignor/nees</td></tr></table><h3>Notification</h3><table><tr><td>Notification Date:</td><td>02-08-2023</td></tr></table><h4>Guarantor</h4><table><tr><td>Guarantor Identification Number:</td><td>G1</td></tr><tr><td>Guarantor Address:</td><td>1 LOW ST  </td></tr></table><h4>Holder Of The Transit Procedure</h4><table><tr><td>Holder Of The Transit Procedure Identification Number:</td><td>XI175521246821</td></tr></table>",
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
