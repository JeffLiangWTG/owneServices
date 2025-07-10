using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC060C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC060CProcessor))]
	class CC060CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC060CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC060CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE060;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC060CText(notificationType: NotificationTypeForTest);

		protected override ZString MessageFriendlyName => "CC060C: CONTROL DECISION NOTIFICATION";

		protected virtual ZString NotificationTypeForTest => "0";

		protected virtual ZString ExpectedBM_CustomsStatus => NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

		protected override CC060CProcessor Processor => new CC060CProcessor(logger, typeof(Cc060CType));

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", ExpectedBM_CustomsStatus, movementHeader.BM_CustomsStatus);

			var requestedDocuments = movementHeader.Header.RequestedDocuments;
			AssertEquals("RequestedDocuments.Count", 2, requestedDocuments.Count);

			var doc1 = requestedDocuments[0];
			AssertEquals("CSI_Code", "Y022", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Consignor / exporter (AEO certificate number)", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 01, 31, 10, 22, 11), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", ZDateTime.Empty, doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc1.CSI_Status);

			var doc2 = requestedDocuments[1];
			AssertEquals("CSI_Code", "Y029", doc2.CSI_Code);
			AssertEquals("RequestInformation", "Other text", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 01, 31, 10, 22, 11), doc2.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", ZDateTime.Empty, doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc2.CSI_Status);

			AssertMessageInterpretation(incomingMessage, CC060CMessageInterpreterTest.GetExpectedInterpretationText(NotificationTypeForTest));
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Control Decision Notification (IE060) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			MessageTestHelper.SetupCL384Types(Factory);
			MessageTestHelper.SetupCL716Types(Factory);
			MessageTestHelper.SetupCL215Types(Factory);
			return base.CreateSetupData();
		}
	}

	[TestedType(typeof(CC060CProcessor))]
	class CC060CProcessor_NotificationType_Additional_Docs_Requested : CC060CProcessorTest
	{
		protected override ZString NotificationTypeForTest => "1";

		protected override ZString ExpectedBM_CustomsStatus => NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
	}

	[TestedType(typeof(CC060CProcessor))]
	class CC060CProcessor_NotificationType_Intention_To_Control : CC060CProcessorTest
	{
		protected override ZString NotificationTypeForTest => "2";

		protected override ZString ExpectedBM_CustomsStatus => NCTS5DepartureCustomsStatusList.Codes.IntentionToControl;
	}

	[TestedType(typeof(CC060CProcessor))]
	class CC060CProcessor_PhysicalControls_Test : NCTSDepartureMessageProcessorAbstractTest<CC060CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC060CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE060;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC060C_PhysicalControls_Text();

		protected override ZString MessageFriendlyName => "CC060C: CONTROL DECISION NOTIFICATION";

		protected override CC060CProcessor Processor => new CC060CProcessor(logger, typeof(Cc060CType));

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, movementHeader.BM_CustomsStatus);

			var requestedDocuments = movementHeader.Header.RequestedDocuments;
			AssertEquals("RequestedDocuments.Count", 1, requestedDocuments.Count);

			var doc1 = requestedDocuments[0];
			AssertEquals("CSI_Code", "Y057", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Information about the physical inspection", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 01, 31, 10, 22, 11), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", ZDateTime.Empty, doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, doc1.CSI_Status);
		}

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			MessageTestHelper.SetupCL384Types(Factory);
			return base.CreateSetupData();
		}
	}

	[TestedType(typeof(CC060CProcessor))]
	class CC060CProcessor_OtherControls_Test : NCTSDepartureMessageProcessorAbstractTest<CC060CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC060CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE060;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC060C_OtherControls_Text();

		protected override ZString MessageFriendlyName => "CC060C: CONTROL DECISION NOTIFICATION";

		protected override CC060CProcessor Processor => new CC060CProcessor(logger, typeof(Cc060CType));

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, movementHeader.BM_CustomsStatus);

			var requestedDocuments = movementHeader.Header.RequestedDocuments;
			AssertEquals("RequestedDocuments.Count", 0, requestedDocuments.Count);
		}

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			MessageTestHelper.SetupCL384Types(Factory);
			return base.CreateSetupData();
		}
	}
}
