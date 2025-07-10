using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RexAcknowledgeOwnershipResponseProcessor))]
	sealed class RexAcknowledgeOwnershipResponseProcessorTest : NEXDOCMessageProcessorAbstractTest
	{
		public void TestProcessNEXDOCResponse_AcknowledgeOwnership_Accepted()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NEXDOC_UniversalInterchangeResponse_Acknowledgement_Accepted.xml")));

			var notification = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingAccept;

			AssertEquals("QN_RexNumber", "REX0000028829", notification.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification.QN_NotificationType);
			Factory.Save();

			var messageProcessor = new RexAcknowledgeOwnershipResponseProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			AssertEmailHasMessageStatus("CLOSED_ACCEPTED");

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Received, message.EM_Status);

			notification.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.Accepted, notification.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", NEXDOCMessageStatus.Codes.ClosedAccepted, notification.QN_MessageStatus);
			Assert("Notification contains message", notification.Messages.Any(m => m.PK == message.PK));
		}

		public void TestProcessNEXDOCResponse_AcknowledgeOwnership_Accepted_MultiplePending()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NEXDOC_UniversalInterchangeResponse_Acknowledgement_Accepted.xml")));

			var notification_NOT = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);

			AssertEquals("QN_RexNumber", "REX0000028829", notification_NOT.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification_NOT.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.NotActioned, notification_NOT.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_NOT.QN_MessageStatus);

			var notification_PAC = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification_PAC.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingAccept;

			AssertEquals("QN_RexNumber", "REX0000028829", notification_PAC.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification_PAC.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.PendingAccept, notification_PAC.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_PAC.QN_MessageStatus);

			var notification_PRJ = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification_PRJ.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingReject;

			AssertEquals("QN_RexNumber", "REX0000028829", notification_PRJ.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification_PRJ.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.PendingReject, notification_PRJ.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_PRJ.QN_MessageStatus);

			Factory.Save();

			var messageProcessor = new RexAcknowledgeOwnershipResponseProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			AssertEmailHasMessageStatus("CLOSED_ACCEPTED");

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Received, message.EM_Status);

			notification_NOT.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.NotActioned, notification_NOT.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_NOT.QN_MessageStatus);
			Assert("Notification does not contain message", !notification_NOT.Messages.Any(m => m.PK == message.PK));

			notification_PAC.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.Accepted, notification_PAC.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", NEXDOCMessageStatus.Codes.ClosedAccepted, notification_PAC.QN_MessageStatus);
			Assert("Notification contains message", notification_PAC.Messages.Any(m => m.PK == message.PK));

			notification_PRJ.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.PendingReject, notification_PRJ.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_PRJ.QN_MessageStatus);
			Assert("Notification does not contain message", !notification_PRJ.Messages.Any(m => m.PK == message.PK));
		}

		public void TestProcessNEXDOCResponse_AcknowledgeOwnership_Rejected()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NEXDOC_UniversalInterchangeResponse_Acknowledgement_Rejected.xml")));

			var notification = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingReject;

			AssertEquals("QN_RexNumber", "REX0000028829", notification.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification.QN_NotificationType);
			Factory.Save();

			var messageProcessor = new RexAcknowledgeOwnershipResponseProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			AssertEmailHasMessageStatus("CLOSED_REJECTED");

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Received, message.EM_Status);

			notification.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.Rejected, notification.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", NEXDOCMessageStatus.Codes.ClosedRejected, notification.QN_MessageStatus);
			Assert("Notification contains message", notification.Messages.Any(m => m.PK == message.PK));
		}

		public void TestProcessNEXDOCResponse_AcknowledgeOwnership_Rejected_MultiplePending()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NEXDOC_UniversalInterchangeResponse_Acknowledgement_Rejected.xml")));

			var notification_NOT = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);

			AssertEquals("QN_RexNumber", "REX0000028829", notification_NOT.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification_NOT.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.NotActioned, notification_NOT.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_NOT.QN_MessageStatus);

			var notification_PAC = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification_PAC.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingAccept;

			AssertEquals("QN_RexNumber", "REX0000028829", notification_PAC.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification_PAC.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.PendingAccept, notification_PAC.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_PAC.QN_MessageStatus);

			var notification_PRJ = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification_PRJ.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingReject;

			AssertEquals("QN_RexNumber", "REX0000028829", notification_PRJ.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification_PRJ.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.PendingReject, notification_PRJ.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_PRJ.QN_MessageStatus);

			Factory.Save();

			var messageProcessor = new RexAcknowledgeOwnershipResponseProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			AssertEmailHasMessageStatus("CLOSED_REJECTED");

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Received, message.EM_Status);

			notification_NOT.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.NotActioned, notification_NOT.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_NOT.QN_MessageStatus);
			Assert("Notification does not contain message", !notification_NOT.Messages.Any(m => m.PK == message.PK));

			notification_PAC.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.PendingAccept, notification_PAC.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification_PAC.QN_MessageStatus);
			Assert("Notification does not contain message", !notification_PAC.Messages.Any(m => m.PK == message.PK));

			notification_PRJ.Reload();
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.Rejected, notification_PRJ.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", NEXDOCMessageStatus.Codes.ClosedRejected, notification_PRJ.QN_MessageStatus);
			Assert("Notification contains message", notification_PRJ.Messages.Any(m => m.PK == message.PK));
		}

		public void TestProcessingError_UnexpectedRexNumber()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NEXDOC_UniversalInterchangeResponse_Acknowledgement_Accepted.xml")));
			Factory.Save();

			var messageProcessor = new RexAcknowledgeOwnershipResponseProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Error, message.EM_Status);

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Response Email Subject", "ERROR PROCESSING", responseEmail.Subject);
			AssertContains("Response Email Body contains Status", "FATAL PROCESSING ERROR: Unexpected or Invalid Rex Number 'REX0000028829'. Cannot Process.", responseEmail.Body);
		}

		public void TestProcessingError_UnexpectedResponseBody()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<n1:UniversalInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:n1=""http://www.cargowise.com/Schemas/Universal/2011/11"" 
xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal/2011/11 UniversalInterchange.xsd"">
	<n1:Header>
		<n1:SenderID>NEXDOCSTest</n1:SenderID>
		<n1:RecipientID>HYEDAUCM2</n1:RecipientID>
		<n1:DeliveryMetadata>
			<n1:ValueCollection>
				<n1:Value>
					<n1:Name>RexNumber</n1:Name>
					<n1:Type>String</n1:Type>
					<n1:Data>REX0000028829</n1:Data>
				</n1:Value>
			</n1:ValueCollection>
		</n1:DeliveryMetadata>
	</n1:Header>
	<n1:Body>
		<ns1:SomeOtherResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
			<ns1:outcome>CLOSED_ACCEPTED</ns1:outcome>
		</ns1:SomeOtherResponse>
	</n1:Body>
</n1:UniversalInterchange>");

			var notification = new QuarantineNexDocNotificationCreator(Factory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);

			AssertEquals("QN_RexNumber", "REX0000028829", notification.QN_RexNumber);
			Factory.Save();

			var messageProcessor = new RexAcknowledgeOwnershipResponseProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Error, message.EM_Status);

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Response Email Subject", "ERROR PROCESSING", responseEmail.Subject);
			AssertContains("Response Email Body contains Status", "FATAL PROCESSING ERROR: Invalid RexAcknowledgeOwnershipResponse. Cannot Process.", responseEmail.Body);
		}

		void AssertEmailHasMessageStatus(string expectedMessageStatus)
		{
			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();

			AssertContains("Response Email Body contains Status",
$@"A NEXDOC Notification has been received.<br />
<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
	Shown below is a summary of relevant information received in the message.
</p>
<br />
<strong>
	REX Number: REX0000028829<br />Exporter Reference: BKG190607REF2<br />Message Status: {expectedMessageStatus}<br />
</strong>
<br />
<hr />
<br />
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
", responseEmail.Body);

			AssertEquals("Response Email Subject", "NEXDOC Notification Advice", responseEmail.Subject);
		}
	}
}
