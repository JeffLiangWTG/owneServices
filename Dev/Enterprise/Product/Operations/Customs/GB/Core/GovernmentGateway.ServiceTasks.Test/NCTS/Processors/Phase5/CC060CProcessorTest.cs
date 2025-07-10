using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc060c;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.NCTS;
using Enterprise.Messaging.Integration;
using Moq;
using Moq.Protected;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC060CProcessorTest : NctsBaseProcessorTest<Cc060CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					LRN = "TRATESTGB142308011437",
					MRN = "23GB000246QK1MD0J2",
					CorrelationIdentifier = "13654275426362",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC060C_Message.xml"),
					MessageSubType = "60C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.IntentionToControl,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedNewMessageInterpretation = @"New Customs Status: Decision to Control Notification</br>
Status granted on 01/08/2023 13:39:29</br>
Type of Notification: 2 Intention to Control</br>",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
			}
		}

		public void TestProcessCC060CMessageDeclaration_NotificationType_0()
		{
			TestProcess("0", NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, NCTS5NotificationTypes.Descriptions.DecisionToControlAndRequestedDocumentsIfNeeded);
		}

		public void TestProcessCC060CMessageDeclaration_NotificationType_1()
		{
			TestProcess("1", NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, NCTS5NotificationTypes.Descriptions.AdditionalDocumentsRequest);
		}

		public void TestProcessCC060CMessageDeclaration_NotificationType_2()
		{
			TestProcess("2", NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, NCTS5NotificationTypes.Descriptions.IntentionToControl);
		}

		public void TestProcessCC060CMessageDeclaration_NotificationTypeInvalid()
		{
			TestProcess("9", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, string.Empty);
		}

		void TestProcess(string notificationType, string expectedCustomsStatus, string expectedNotificationInterpretation)
		{
			var currentDateTime = ZDateTime.Now.ToDateTime();

			var header = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.DepartureAndArrival, string.Empty, LogicalStatusList.Codes.Sent);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			mrn.CE_EntryNum = "22BE000000000012J1";

			var ediMessage = Factory.New<NCTSInboundEDIMessage>();
			ediMessage.EM_MessageSubType = "060";
			Factory.Save();

			var messageObject = new Cc060CType();
			messageObject.MessageType = MessageTypes.Cc060C;
			messageObject.TransitOperation = new TransitOperationType22();
			messageObject.TransitOperation.Lrn = "2204528148060XXXXXX";
			messageObject.TransitOperation.Mrn = "22BE000000000012J1";
			messageObject.TransitOperation.ControlNotificationDateAndTime = currentDateTime;
			messageObject.TransitOperation.NotificationType = notificationType;
			messageObject.TypeOfControls = new Collection<TypeOfControlsType>();
			messageObject.RequestedDocument = new Collection<RequestedDocumentType>();

			var mockProcessor = new Mock<CC060CProcessor>(new TestServiceLogger(), new BatchProcessor.LoggingInformation());
			mockProcessor.CallBase = true;
			mockProcessor.Protected().Setup<Cc060CType>("DeserializeMessage", ItExpr.IsAny<string>()).Returns(messageObject);

			var processor = mockProcessor.Object;
			processor.ProcessMessage(ediMessage);

			CombineAssertions("Notification Type: notificationType", () =>
			{
				AssertEquals("EDIMessage Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("MovementHeader CustomsStatus", expectedCustomsStatus, movementHeader.BM_CustomsStatus);
				AssertEquals("Header MessageStatus", LogicalStatusList.Codes.Accepted, header.EffectiveMessageStatus);
				AssertEquals("MRN", "22BE000000000012J1", header.MovementReferenceEntryNumber.CE_EntryNum);
				AssertContains("MessageInterpretation", "New Customs Status: Decision to Control Notification", ediMessage.EM_MessageInterpretation);
				AssertContains("MessageInterpretation", expectedNotificationInterpretation, ediMessage.EM_MessageInterpretation);
			});
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
					Description = "BM_CustomsStatus=CO1",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=CO2",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=CO3",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.IntentionToControl;
					},
					ExpectedResult = false,
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
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=AMR",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was AMR",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=ENQ",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was ENQ",
				};
			}
		}
	}
}
