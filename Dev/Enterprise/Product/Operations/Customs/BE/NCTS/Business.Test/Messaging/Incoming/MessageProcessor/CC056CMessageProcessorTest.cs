using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC056CMessageProcessor))]
	sealed class CC056CMessageProcessorTest : NctsMessageProcessorTestCase<CC056CMessageProcessor, ICC056CDataProvider>
	{
		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => true;

		protected override bool SupportsSearchByMRN => false;

		protected override Mock<ICC056CDataProvider> MockProvider => mockProvider ?? (mockProvider = GetNewMockProvider());
		Mock<ICC056CDataProvider> mockProvider;

		protected override Mock<CC056CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC056CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC056CMessageProcessor> mockProcessor;

		protected override ZString[] PreProcessedOKPhase => new ZString[] { NctsMovementHeaderTransactionStatusList.Codes.Amendment };

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC056C;

		public void TestProcessCC506CMessageDeclaration_014_ACK_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_ACK_SNT_LocateHeaderByMRN()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation, lrnIsEmpty: true);
		}

		public void TestProcessCC506CMessageDeclaration_014_ACK_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_PRE_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_PRE_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_MRN_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_MRN_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_REL_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_REL_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC014C, NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC056CMessageDeclaration_015_Blank_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC015C, NctsTransitStatusList.Codes.Unknown, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestProcessCC056CMessageDeclaration_015_Blank_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC015C, NctsTransitStatusList.Codes.Unknown, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestProcessCC056CMessageDeclaration_013_ACK_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_ACK_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_PRE_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_PRE_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_MRN_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_MRN_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_AMR_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_AMR_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_GIV_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment, expectedMovementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Declaration);
		}

		public void TestProcessCC056CMessageDeclaration_013_GIV_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC013C, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment, expectedMovementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Declaration);
		}

		public void TestProcessCC056CMessageDeclaration_054_AMR_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_AMR_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO0_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO0_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO1_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO1_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO2_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO2_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC054C, NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_141_ENQ_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC141C, NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_141_ENQ_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC141C, NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_ACK_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC170C, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_ACK_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC170C, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_PRE_ACK()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC170C, NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_PRE_SNT()
		{
			TestProcessCC056CMessageDeclaration(BEOutgoingMessageTypes.Codes.CC170C, NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		void TestProcessCC056CMessageDeclaration(string businessRejectionType, string customsStatus, string messageStatus, string expectedMessageStatus, string expectedTransactionStatus = PermitTransactionStatusList.Codes.Pending, string movementHeaderPhase = NctsMovementHeaderTransactionStatusList.Codes.Declaration, string expectedMovementHeaderPhase = "", bool lrnIsEmpty = false)
		{
			mockProvider.Setup(x => x.BusinessRejectionType).Returns(businessRejectionType);
			mockProvider.Setup(x => x.RejectionDateAndTimeUtc).Returns(DateTime.UtcNow);
			mockProvider.Setup(x => x.RejectionCode).Returns("4");
			mockProvider.Setup(x => x.RejectionReason).Returns("invalid value transport type");
			mockProvider.Setup(x => x.FunctionalErrorList).Returns(new Collection<FunctionalErrorXmlProvider>());

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = messageStatus;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = customsStatus;
			movementHeader.BM_Phase = movementHeaderPhase;
			if (lrnIsEmpty)
			{
				mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
				Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			}
			else
			{
				mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			}
			var assertTransaction = businessRejectionType == BEOutgoingMessageTypes.Codes.CC013C || businessRejectionType == BEOutgoingMessageTypes.Codes.CC015C;
			if (assertTransaction)
			{
				SetupGuaranteeTransaction(nctsHeader);
			}

			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			CombineAssertions($"BusinessRejectionType: {businessRejectionType}, Customs Status: {customsStatus}, Message Status: {messageStatus}", () =>
			{
				AssertEquals($"EDIMEssage Status Should be {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals($"Ncts Header Message Status Should be {expectedMessageStatus}", expectedMessageStatus, nctsHeader.EffectiveMessageStatus);
				AssertContains("ST_NoteText", "Declaration received an error for type ", incomingMessage.EM_MessageInterpretation);
				if (!string.IsNullOrEmpty(expectedMovementHeaderPhase))
				{
					AssertEquals($"Phase status", expectedMovementHeaderPhase, movementHeader.BM_Phase);
				}
				if (assertTransaction)
				{
					AssertEquals($"Transaction status should be {expectedTransactionStatus}", expectedTransactionStatus, guaranteeHeader.GetTransactions().ToArray()[1].CPL_TransactionStatus);
				}
			});
		}

		void SetupGuaranteeTransaction(NctsHeader nctsHeader)
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var movementHeader = nctsHeader.MovementHeader;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			guaranteeHeader = Factory.New<BE.Business.CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_Balance = 1000m;

			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "OPENING";
			transaction.CPL_TranValue = 1000m;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145;
			guarantee.CusGuarantee.AddTransaction(movementHeader.BM_PaperlessInbondNum,
												"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
												"CreatedByMessageNumber",
												ZString.Empty,
												guarantee.PW_BondAmount * -1,
												0,
												status: PermitTransactionStatusList.Codes.Pending);
		}
		CusGuaranteeHeader guaranteeHeader;

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.CC056C.ToString();

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested };

		protected override ZString[] PreProcessOKMessageStatus => new ZString[] { LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent };

		protected override Type ExpectedMessageInterpreterType => typeof(CC056CMessageInterpreter);

		Mock<ICC056CDataProvider> GetNewMockProvider()
		{
			Mock<ICC056CDataProvider> mockProvider = new Mock<ICC056CDataProvider>();
			mockProvider.Setup(x => x.BusinessRejectionType).Returns("013");
			return mockProvider;
		}
	}
}
