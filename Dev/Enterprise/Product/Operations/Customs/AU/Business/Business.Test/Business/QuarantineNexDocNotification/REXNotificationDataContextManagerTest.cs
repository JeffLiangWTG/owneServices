using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(REXNotificationDataContextManager))]
	sealed class REXNotificationDataContextManagerTest : DataContextManagerTestCase<REXNotificationDataContextManager, QuarantineNexDocNotification>
	{
		public void TestRexNotification_ServiceError()
		{
			var eventTime = ZDateTime.UtcNow.AddDays(-1);
			string incomingEvent = $@"
			<UniversalEvent>
				<Event>
					<DataContext>
						<DataProvider>NEXDOCS</DataProvider>
						<DataTargetCollection>
							<DataTarget>
								<Key>REX0000028829</Key>
								<Type>REXNotification</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<EventTime>{eventTime.ToISO8601String()}</EventTime>
					<EventType>MRR</EventType>
					<ContextCollection>
						<Context>
							<Type>MessageStatus</Type>
							<Value>ERO</Value>
						</Context>
						<Context>
							<Type>Message</Type>
							<Value>Calling REX Ownership Service Failed</Value>
						</Context>
						<Context>
							<Type>Message</Type>
							<Value>Outstanding change of ownership record does not exist.</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>
			";

			// Finds notification using REX number
			// Sets Acknowledge Status to ERR

			var notification = new QuarantineNexDocNotificationCreator(Factory.BOFactory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingAccept;

			AssertEquals("QN_RexNumber", "REX0000028829", notification.QN_RexNumber);
			AssertEquals("QN_NotificationType", NEXDOCNotificationType.Codes.ForwardAcceptanceRequired, notification.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.PendingAccept, notification.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, notification.QN_MessageStatus);

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var message = ProcessUniversalEventMessage(incomingEvent, serviceTaskLog);
			AssertEquals("Message EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", "Linked Event to NEXDOC Notification - REX0000028829 - BKG190607REF2.", serviceTaskLog.ToString().Trim());

			var reloadedNotification = new BusinessObjectFactory().Load<QuarantineNexDocNotification>(notification.PK);
			AssertEquals("QN_AcknowledgeStatus", NEXDOCAcknowledgeStatus.Codes.Error, reloadedNotification.QN_AcknowledgeStatus);
			AssertEquals("QN_MessageStatus", ZString.Empty, reloadedNotification.QN_MessageStatus);

			var logs = reloadedNotification.Logs;
			var log = logs.Find(c => c.SL_SE_NKEvent == "MRR" && !c.SL_IsCancelled).First();
			AssertEquals("Should set the event time from message.", eventTime.ToString(), log.SL_EventTime.ToString());

			var emailSent = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertNotNull("Should have sent an email.", emailSent);
			AssertEquals("Email title", "NEXDOCS Notification ", emailSent.Subject);
			AssertContains("REX Number.", "REX Number : REX0000028829", emailSent.Body);
		}

		public void TestRexNotification_IgnoresCustomsDeclarationServiceError()
		{
			var eventTime = ZDateTime.UtcNow.AddDays(-1);
			string incomingEvent = $@"
			<UniversalEvent>
				<Event>
					<DataContext>
						<DataProvider>NEXDOCS</DataProvider>
						<DataTargetCollection>
							<DataTarget>
								<Key>REX0000028829</Key>
								<Type>REXNotification</Type>
							</DataTarget>
							<DataTarget>
								<Key>B60002421</Key>
								<Type>CustomsDeclaration</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<EventTime>{eventTime.ToISO8601String()}</EventTime>
					<EventType>MRR</EventType>
					<ContextCollection>
						<Context>
							<Type>MessageStatus</Type>
							<Value>ERO</Value>
						</Context>
						<Context>
							<Type>Message</Type>
							<Value>Calling REX Ownership Service Failed</Value>
						</Context>
						<Context>
							<Type>Message</Type>
							<Value>Outstanding change of ownership record does not exist.</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>
			";

			var notification = new QuarantineNexDocNotificationCreator(Factory.BOFactory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingAccept;
			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			ProcessUniversalEventMessage(incomingEvent, serviceTaskLog);

			AssertMultilineASCIIEquals("Service Task Log", "Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString().Trim());
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("QuarantineNexDocNotification doesn't need any unique jobnumber", true);
		}

		protected override QuarantineNexDocNotification GetNewBusinessObjectForTesting()
		{
			var notification = new QuarantineNexDocNotificationCreator(Factory.BOFactory).Create(
				NEXDOCNotificationType.Codes.ForwardAcceptanceRequired,
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: BKG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.PendingAccept;

			return notification;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var factory = new BusinessObjectFactory();
			var group = factory.New<GlbGroup>();
			var currentStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			currentStaffMember.GS_EmailAddress = "TEST@EDI.COM.AU";
			group.Staff.Add(currentStaffMember);
			factory.Save();

			AUCustomsDataRegistry.Instance.SendAQISAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AUCustomsDataRegistry.Instance.SendAQISErrors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			AUCustomsDataRegistry.Instance.SendAQISErrorsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AUCustomsDataRegistry.Instance.SendAQISImpediments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			AUCustomsDataRegistry.Instance.SendAQISImpedimentsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		IEDIMessage ProcessUniversalEventMessage(string messageText, ISimpleLogger logger = null)
		{
			var message = GetQueuedUniversalEventMessage(Factory, messageText);
			var interchange = Factory.New<EDIInterchange>();
			message.EM_EI = interchange.PK;
			interchange.EI_From = "NEXDOCS";
			interchange.EI_InterchangeNum = "4061";
			Factory.SaveForTesting();

			var messageFactory = new BusinessObjectFactory();
			var messageToProcess = messageFactory.Load<IEDIMessage>(message.PK);
			var manager = new UniversalMessageProcessingManager(logger ?? new ServiceTaskLogForTesting());
			manager.Process(messageToProcess);
			messageFactory.Save();

			return messageToProcess;
		}
	}
}
