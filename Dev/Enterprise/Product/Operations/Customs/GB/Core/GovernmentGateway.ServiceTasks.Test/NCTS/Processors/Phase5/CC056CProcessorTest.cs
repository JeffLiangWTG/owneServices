using System;
using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc056c;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Moq;
using Moq.Protected;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC056CProcessorTest : NctsBaseProcessorTest<Cc056CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupNctsHeader,
					SetupGuaranteesAndTransactions = SetupGuaranteesAndTransactions,
					LRN = "TRATESTGB262307050928",
					CorrelationIdentifier = "28605030153222",
					InitialTransitStatus = string.Empty,
					InitialPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC056C_Message.xml"),
					MessageSubType = "56C",
					ExpectedNewDeclarationStatus = string.Empty,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Invalid,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Declaration received an error for type 015 on 05/07/2023 08:32:03</br>
Reason: 12 Message with functional error(s) - Violation of Rules & Conditions",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					GuaranteesAndTransactionsAssertion = AssertTransactions1,
				};

				yield return new MessageProcessorTestCase
				{
					LRN = "TRATESTXI12308031514",
					CorrelationIdentifier = "22627777941834",
					InitialTransitStatus = string.Empty,
					InitialPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC056C_Multiple_FunctionalError.xml"),
					MessageSubType = "56C",
					ExpectedNewDeclarationStatus = string.Empty,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Invalid,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Declaration received an error for type 015 on 03/08/2023 14:14:52</br>
Reason: 12 Message with functional error(s) - Violation of Rules & Conditions </br>
</br>
Functional error code: 12 Code list violation (incorrect enumeration)</br>
Reason: <a href=""https://developer.service.hmrc.gov.uk/guides/ctc-traders-phase5-tis/documentation/rules-r.html#r0850"" target=""_new"">R0850</a></br>
Attribute: /CC015C/HolderOfTheTransitProcedure/identificationNumber</br>
Element in declaration now contains the value: XI985524247819</br>
</br>
Functional error code: 12 Code list violation (incorrect enumeration)</br>
Reason: <a href=""https://developer.service.hmrc.gov.uk/guides/ctc-traders-phase5-tis/documentation/rules-r.html#r0850"" target=""_new"">R0850</a></br>
Attribute: /CC015C/Consignment/Consignor/identificationNumber</br>
Element in declaration now contains the value: XI985524247819</br>
</br>
Functional error code: 12 Code list violation (incorrect enumeration)</br>
Reason: BR20005</br>
Attribute: /CC015C/HolderOfTheTransitProcedure/identificationNumber</br>
Element in declaration now contains the value: XI985524247819",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};

				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupHeaderForAcknowledgedDeclaration_CustomsStatus_ACK,
					SetupRegistry = SetNctsIsManualDepartureCustomerReferenceEnabled(false),
					LRN = "TRATESTGB262307050928",
					InitialPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC056C_Message.xml"),
					MessageSubType = "56C",
					ExpectedNewDeclarationStatus = string.Empty,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Invalid,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Declaration received an error for type 015 on 05/07/2023 08:32:03</br>
Reason: 12 Message with functional error(s) - Violation of Rules & Conditions </br>
LRN has been reset to allow resubmission",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					HeaderAssertion = (nctsHeader) => {
						AssertNotEquals("LRN Updated", "TRATESTGB262307050928", nctsHeader.MovementHeader.BM_PaperlessInbondNum);
						AssertNotEquals("New LRN Not Empty", string.Empty, nctsHeader.MovementHeader.BM_PaperlessInbondNum);
					}
				};

				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupHeaderForAcknowledgedDeclaration_CustomsStatus_PRE,
					SetupRegistry = SetNctsIsManualDepartureCustomerReferenceEnabled(false),
					LRN = "TRATESTGB262307050928",
					InitialPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC056C_Message.xml"),
					MessageSubType = "56C",
					ExpectedNewDeclarationStatus = string.Empty,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Invalid,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Declaration received an error for type 015 on 05/07/2023 08:32:03</br>
Reason: 12 Message with functional error(s) - Violation of Rules & Conditions </br>
LRN has been reset to allow resubmission",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					HeaderAssertion = (nctsHeader) => {
						AssertNotEquals("LRN Updated", "TRATESTGB262307050928", nctsHeader.MovementHeader.BM_PaperlessInbondNum);
						AssertNotEquals("New LRN Not Empty", string.Empty, nctsHeader.MovementHeader.BM_PaperlessInbondNum);
					}
				};

				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupHeaderForAcknowledgedDeclaration_CustomsStatus_ACK,
					SetupRegistry = SetNctsIsManualDepartureCustomerReferenceEnabled(true),
					LRN = "TRATESTGB262307050928",
					InitialPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC056C_Message.xml"),
					MessageSubType = "56C",
					ExpectedNewDeclarationStatus = string.Empty,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Invalid,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"Declaration received an error for type 015 on 05/07/2023 08:32:03</br>
Reason: 12 Message with functional error(s) - Violation of Rules & Conditions",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					HeaderAssertion = (nctsHeader) => AssertEquals("LRN", "TRATESTGB262307050928", nctsHeader.MovementHeader.BM_PaperlessInbondNum)
				};
			}
		}

		protected override IEnumerable<MessageShouldBeDiscardedTestCase> MessageShouldBeDiscardedTestCases
		{
			get
			{
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BH_MessageStatus=ACC/BM_CustomsStatus=ACK/BM_Phase=015",
					Setup = header =>
					{
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
					},
					ExpectedResult = false,
				};

				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BH_MessageStatus=ACC/BM_CustomsStatus=PRE/BM_Phase=015",
					Setup = header =>
					{
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
					},
					ExpectedResult = false,
				};

				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BH_MessageStatus=REJ/BM_CustomsStatus=RJO/BM_Phase=015",
					Setup = header =>
					{
						header.EffectiveMessageStatus = NctsMessageStatusList.Codes.Rejected;
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.RejectedAtOrigin;
						header.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Message Status of REJ and the Customs Status of RJO for the declaration could not be mapped to correct value of the Business Rejection Type element for Phase Status of 015.",
				};
			}
		}

		Func<IDisposable> SetNctsIsManualDepartureCustomerReferenceEnabled(bool registryValue)
		{
			return () => EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
		}

		void SetupHeaderForAcknowledgedDeclaration(NctsHeader nctsHeader, string customsStatus)
		{
			nctsHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TEST1234", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			nctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
		}

		void SetupHeaderForAcknowledgedDeclaration_CustomsStatus_ACK(NctsHeader nctsHeader)
		{
			 SetupHeaderForAcknowledgedDeclaration(nctsHeader, NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
		}

		void SetupHeaderForAcknowledgedDeclaration_CustomsStatus_PRE(NctsHeader nctsHeader)
		{
			SetupHeaderForAcknowledgedDeclaration(nctsHeader, NCTS5DepartureCustomsStatusList.Codes.PreLodged);
		}

		void SetupNctsHeader(NctsHeader nctsHeader)
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_JobReference = "TRATESTGB262307050928";
		}

		void AssertTransactions1(NctsHeader nctsHeader)
		{
			AssertTransactionsCore(nctsHeader, 2, -25000m, PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestProcessCC506CMessageDeclaration_014_ACK_SNT()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_ACK_SNT_LocateHeaderByMRN()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation, lrnIsEmpty: true);
		}

		public void TestProcessCC506CMessageDeclaration_014_ACK_ACK()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_PRE_ACK()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_PRE_SNT()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_MRN_SNT()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_MRN_ACK()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_REL_ACK()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC506CMessageDeclaration_014_REL_SNT()
		{
			TestProcess("014", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Cancellation);
		}

		public void TestProcessCC056CMessageDeclaration_015_Blank_SNT()
		{
			TestProcess("015", NctsTransitStatusList.Codes.Unknown, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestProcessCC056CMessageDeclaration_015_Blank_ACK()
		{
			TestProcess("015", NctsTransitStatusList.Codes.Unknown, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestProcessCC056CMessageDeclaration_015_ACK_ACK()
		{
			TestProcess("015", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Accepted, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_015_PRE_ACK()
		{
			TestProcess("015", NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Accepted, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_013_ACK_ACK()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_ACK_SNT()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_PRE_ACK()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_PRE_SNT()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_MRN_ACK()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_MRN_SNT()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_AMR_ACK()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_013_AMR_SNT()
		{
			TestProcess("013", NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid, movementHeaderPhase: NctsMovementHeaderTransactionStatusList.Codes.Amendment);
		}

		public void TestProcessCC056CMessageDeclaration_054_AMR_ACK()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_AMR_SNT()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO0_ACK()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO0_SNT()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO1_ACK()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO1_SNT()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO2_ACK()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_054_CO2_SNT()
		{
			TestProcess("054", NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_141_ENQ_ACK()
		{
			TestProcess("141", NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_141_ENQ_SNT()
		{
			TestProcess("141", NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_ACK_ACK()
		{
			TestProcess("170", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_ACK_SNT()
		{
			TestProcess("170", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_PRE_ACK()
		{
			TestProcess("170", NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC056CMessageDeclaration_170_PRE_SNT()
		{
			TestProcess("170", NCTS5DepartureCustomsStatusList.Codes.PreLodged, LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Invalid);
		}

		void TestProcess(string businessRejectionType, string customsStatus, string messageStatus, string expectedMessageStatus, string expectedTransactionStatus = PermitTransactionStatusList.Codes.Pending, string movementHeaderPhase = NctsMovementHeaderTransactionStatusList.Codes.Declaration, bool lrnIsEmpty = false)
		{
			var header = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.DepartureAndArrival, string.Empty, LogicalStatusList.Codes.Sent);
			header.EffectiveMessageStatus = messageStatus;
			var movementHeader = header.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = customsStatus;
			movementHeader.BM_Phase = movementHeaderPhase;

			var messageObject = new Cc056CType();
			messageObject.TransitOperation = new TransitOperationType20();
			messageObject.MessageType = MessageTypes.Cc056C;

			messageObject.TransitOperation.BusinessRejectionType = businessRejectionType;
			messageObject.TransitOperation.RejectionCode = "4";
			messageObject.TransitOperation.RejectionReason = "invalid value transport type";

			if (lrnIsEmpty)
			{
				messageObject.TransitOperation.Mrn = "22BE000000000012J1";
				var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				mrn.CE_EntryNum = "22BE000000000012J1";
			}
			else
			{
				messageObject.TransitOperation.Lrn = "2204528148060XXXXXX";
			}

			var ediMessage = Factory.New<NCTSInboundEDIMessage>();
			ediMessage.EM_MessageSubType = "056";
			Factory.Save();

			var mockProcessor = new Mock<CC056CProcessor>(new TestServiceLogger(), new BatchProcessor.LoggingInformation());
			mockProcessor.CallBase = true;
			mockProcessor.Protected().Setup<Cc056CType>("DeserializeMessage", ItExpr.IsAny<string>()).Returns(messageObject);

			var processor = mockProcessor.Object;
			processor.ProcessMessage(ediMessage);

			CombineAssertions($"BusinessRejectionType: {businessRejectionType}, Customs Status: {customsStatus}, Message Status: {messageStatus}", () =>
			{
				AssertEquals("EDIMessage Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("Header MessageStatus", expectedMessageStatus, header.MovementHeader.BM_MessageStatus);
				AssertContains("EM_MessageInterpretation", "Declaration received an error for type ", ediMessage.EM_MessageInterpretation);
			});
		}
	}
}
