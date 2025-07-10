using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class StandardXmlProcessorTest : ExtendedTestCaseWithFactory
	{
		delegate bool ReturnsDelegate(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants);

		public void TestContinuousProcessing()
		{
			var message1 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOA");
			var message2 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOB");
			message1.EM_MessageNum = "00000000000000000001";
			message2.EM_MessageNum = "00000000000000000002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("processed"));
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOA")).Returns(messageAction.Object);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOB")).Returns(messageAction.Object);
			string[] companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.Setup(m => m.NewMessageAvailable(out companyCodes)).Returns(true);
			processor.SetupSequence(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { message1.PK }).Returns(new List<ZGuid> { message2.PK });

			processor.Object.Process(notifications);
			Factory.ReloadAll<XmlEDIMessage>();
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(message1.PK).EM_Status);
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(message2.PK).EM_Status);

			AssertEquals(1, message1.Notes.FindByDescription("Processing Log").Length);
			AssertEquals(1, message2.Notes.FindByDescription("Processing Log").Length);

			AssertLogs(@"
Processing message batch. Number of messages: '1'
Processing message '00000000000000000001'
Processing finished. Saving changes...
Finished message processing.
Processing message batch. Number of messages: '1'
Processing message '00000000000000000002'
Processing finished. Saving changes...
Finished message processing.", notifications.AsString);

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.Exactly(2));
			messageAction.Verify(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()), Times.Exactly(2));
			processor.Verify(m => m.NewMessageAvailable(out companyCodes), Times.AtLeast(2));
		}

		public void TestCancellation()
		{
			var message1 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOA");
			var message2 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOB");
			message1.EM_MessageNum = "00000000000000000001";
			message2.EM_MessageNum = "00000000000000000002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));
			var cts = new CancellationTokenSource();
			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("processed"));
					cts.Cancel();
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOA")).Returns(messageAction.Object);
			var companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.Setup(m => m.NewMessageAvailable(out companyCodes)).Returns(true);
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { message1.PK, message2.PK });

			processor.Object.Process(notifications, cts.Token);
			Factory.ReloadAll<XmlEDIMessage>();
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(message1.PK).EM_Status);
			AssertEquals(EDIMessage.Status.Queued, Factory.Load<EDIMessage>(message2.PK).EM_Status);

			AssertEquals(1, message1.Notes.FindByDescription("Processing Log").Length);
			AssertEquals(0, message2.Notes.FindByDescription("Processing Log").Length);

			AssertLogs(@"
Processing message batch. Number of messages: '2'
Processing message '00000000000000000001'
Processing finished. Saving changes...
Finished message processing.
Warning: Task canceled.", notifications.AsString);

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.Exactly(1));
			messageAction.Verify(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()), Times.Exactly(1));
		}

		public void TestProcessBatchSucceed()
		{
			var goodMessage1 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOA");
			var goodMessage2 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOB");
			goodMessage1.EM_MessageNum = "00000000000000000001";
			goodMessage2.EM_MessageNum = "00000000000000000002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));
			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("processed"));
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOA")).Returns(messageAction.Object);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOB")).Returns(messageAction.Object);
			var companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage1.PK, goodMessage2.PK });

			processor.Object.Process(notifications);
			Factory.ReloadAll<XmlEDIMessage>();
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(goodMessage1.PK).EM_Status);
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(goodMessage2.PK).EM_Status);

			AssertEquals(1, goodMessage1.Notes.FindByDescription("Processing Log").Length);
			AssertEquals(1, goodMessage2.Notes.FindByDescription("Processing Log").Length);

			AssertLogs(@"
Processing message batch. Number of messages: '2'
Processing message '00000000000000000001'
Processing message '00000000000000000002'
Processing finished. Saving changes...
Finished message processing.", notifications.AsString);

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.Exactly(2));
			messageAction.Verify(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()), Times.Exactly(2));
		}

		public void TestProcessBatchFailDueToMessageTypeNotSupportThenProcessOneByOne()
		{
			const string MessageTypeNotSupportString = "Message Type 'XMS' with Subtype 'BAD' not supported.";

			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			var badMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "BAD");
			goodMessage.EM_MessageNum = "00000000000000000001";
			badMessage.EM_MessageNum = "00000000000000000002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));
			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("processed"));
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "BAD")).Returns((IMessageAction)null);
			var companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage.PK, badMessage.PK });

			processor.Object.Process(notifications);
			var newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Received, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);
			AssertEquals(EDIMessage.Status.Error, newFactory.Load<EDIMessage>(badMessage.PK).EM_Status);

			AssertEquals(1, goodMessage.Notes.FindByDescription("Processing Log").Length);
			AssertEquals(1, badMessage.Notes.FindByDescription("Processing Log").Length);
			Assert(badMessage.Notes.FindByDescription("Processing Log")[0].ST_NoteDataAsText.Contains(MessageTypeNotSupportString));

			AssertLogs(@"
Processing message batch. Number of messages: '2'
Processing message '00000000000000000001'
Processing message '00000000000000000002'
Warning: Message Type 'XMS' with Subtype 'BAD' not supported.
Processing finished. Saving changes...
Finished message processing.", notifications.AsString);

			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessBatchFailDueToMessageActionExecuteErrorThenProcessOneByOne()
		{
			const string MessageActionExecuteErrorString = "The 'http://www.edi.com.au/EnterpriseService/:Weight' element is invalid - The value '' is invalid according to its datatype 'http://www.edi.com.au/EnterpriseService/:DimensionValue' - The string '' is not a valid Decimal value.";

			var badMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "BAD");
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			badMessage.EM_MessageNum = "00000000000000000001";
			goodMessage.EM_MessageNum = "00000000000000000002";

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			var calls = 0;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m1, INotifications buffer, out List<ITransactionParticipant> participants) =>
				{
					calls++;
					participants = new List<ITransactionParticipant>();
					if (calls < 3)
					{
						buffer.Notify(new ErrorNotification(ErrorType.Error, MessageActionExecuteErrorString));
					}
					return calls > 2;
				}));

			var sendNotificationEmailCallCount = 0;
			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					sendNotificationEmailCallCount++;
					Assert(subject.StartsWith("EDI Message #"));
					if (sendNotificationEmailCallCount > 1)
					{
						Assert(subject.EndsWith("processed"));
					}
					else
					{
						Assert(subject.EndsWith("rejected"));
						Assert(body.Contains(MessageActionExecuteErrorString));
					}
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "BAD")).Returns(messageAction.Object);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			string[] companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { badMessage.PK, goodMessage.PK });

			processor.Object.Process(notifications);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Error, newFactory.Load<EDIMessage>(badMessage.PK).EM_Status);
			AssertEquals(EDIMessage.Status.Received, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);

			AssertEquals(1, badMessage.Notes.FindByDescription("Processing Log").Length);
			Assert(badMessage.Notes.FindByDescription("Processing Log")[0].ST_NoteDataAsText.Contains(MessageActionExecuteErrorString));
			AssertEquals(1, goodMessage.Notes.FindByDescription("Processing Log").Length);

			AssertLogs(@"
Processing message batch. Number of messages: '2'
Processing message '00000000000000000001'
Error: The 'http://www.edi.com.au/EnterpriseService/:Weight' element is invalid - The value '' is invalid according to its datatype 'http://www.edi.com.au/EnterpriseService/:DimensionValue' - The string '' is not a valid Decimal value.
Warning: Error(e.g. XML validation error) that prevent save happened. Message process failed.
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '00000000000000000001'
Error: The 'http://www.edi.com.au/EnterpriseService/:Weight' element is invalid - The value '' is invalid according to its datatype 'http://www.edi.com.au/EnterpriseService/:DimensionValue' - The string '' is not a valid Decimal value.
Warning: Error(e.g. XML validation error) that prevent save happened. Message process failed.
Processing message '00000000000000000002'
Finished message processing.", notifications.AsString);

			processor.Verify(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "BAD"), Times.Exactly(2));
			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessBatchFailThenSendEmailNotification()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GRP";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "~TT";
			staff.GS_EmailAddress = "blah@blah.com";

			var badMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, EDIMessageSubTypeList.Codes.Shipments);
			badMessage.EM_MessageNum = "00000000000000000001";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var processor = new Mock<StandardXmlProcessor>() { CallBase = true };
			processor.SetupAllProperties();
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { badMessage.PK });

			processor.Object.Process(notifications);

			var newFactory = new BusinessObjectFactory();
			MailItem[] emails = newFactory.Load<MailItem>(new ZQuery());

			AssertEquals("A email should be created.", 1, emails.Length);
			AssertEquals("email.MI_Subject", "EDI Message #00000000000000000001 Type SHP rejected", emails[0].MI_Subject);

			processor.VerifyAll();
		}

		public void TestProcessBatchFailDueToSaveAndClearParticipantsThrowAnExceptionThenProcessOneByOne()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			goodMessage.EM_MessageNum = "00000000000000000001";

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor>() { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage.PK });
			processor.Setup(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), null))
				.Throws(new Exception("Save current failed"));

			processor.Object.Process(notifications);
			var newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Received, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);

			AssertEquals(1, goodMessage.Notes.FindByDescription("Processing Log").Length);

			AssertLogs(@"
Processing message batch. Number of messages: '1'
Processing message '00000000000000000001'
Processing finished. Saving changes...
Warning: Save current failed
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '00000000000000000001'
Finished message processing.
			", notifications.AsString);

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.Exactly(2));
			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessBatchFailThenProcessOneByOneFailBothDueToSaveCurrentException()
		{
			const string ErrorToSaveMessageString = "Error to save message";

			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			goodMessage.EM_MessageNum = "00000000000000000001";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor>() { CallBase = true };
			processor.SetupAllProperties();
			var messageActionCalls = 0;
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO"))
				.Returns((ZString subject, ZString body) =>
				{
					messageActionCalls++;
					return messageActionCalls > 2 ? null : messageAction.Object;
				});
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage.PK });
			processor.Setup(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsAny<EDIMessage>())).Throws(new Exception("Save current failed"));
			processor.Setup(m => m.ReportError(It.IsAny<Exception>(), It.IsAny<string>()));

			processor.Object.Process(notifications);
			var newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Error, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);

			AssertEquals(1, goodMessage.Notes.FindByDescription("Processing Log").Length);
			Assert(goodMessage.Notes.FindByDescription("Processing Log")[0].ST_NoteDataAsText.Contains(ErrorToSaveMessageString));

			AssertLogs(@"
Processing message batch. Number of messages: '1'
Processing message '00000000000000000001'
Processing finished. Saving changes...
Warning: Save current failed
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '00000000000000000001'
Error: Error to save message: ''
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->XML Failure Fallback Notification Group'
Finished message processing.
			", notifications.AsString);

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.Exactly(2));
			processor.Verify(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO"), Times.AtLeast(2));
			processor.Verify(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsAny<EDIMessage>()), Times.Exactly(2));
			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessBatchFail_ProcessInBatch_ThrowCriticalException()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var processor = new Mock<StandardXmlProcessor>() { CallBase = true };
			processor.SetupAllProperties();
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.Setup(m => m.NewMessageAvailable(out companyCodes)).Returns(true);
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid>() { goodMessage.PK });

			processor.Setup(m => m.ProcessInBatch(It.IsAny<MessageBatch>(), CancellationToken.None))
				.Throws(new OutOfMemoryException());

			AssertExceptionThrown(typeof(OutOfMemoryException), () => processor.Object.Process(notifications));

			processor.VerifyAll();
		}

		public void TestProcessInBatch_UpdateStatusAfterProcessing()
		{
			var message1 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			var message2 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));
			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("processed"));
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.SetupSequence(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { message1.PK, message2.PK }).Returns(new List<ZGuid>());

			processor.Object.Process(notifications);

			Factory.ReloadAll<XmlEDIMessage>();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("EM_Status", EDIMessage.Status.Received, message2.EM_Status);

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.Exactly(2));
			messageAction.Verify(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()), Times.Exactly(2));
			processor.Verify(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO"), Times.Exactly(2));
			processor.Verify(m => m.NewMessageAvailable(out companyCodes), Times.Exactly(2));
			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessOneByOneFailOnDeadlockAndRetrySucceed()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new StandardXmlProcessor_SealedDummyTestClass();
			processor.SetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO", messageAction.Object);
			var companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetNewMessageAvailable(companyCodes, true);
			processor.GetEDIMessagePKsReturns = new List<ZGuid> { goodMessage.PK };

			var zSaveEx = CreateTestZSaveExceptionIncludingDeadlock();
			var saveAndClearCallCount = 0;
			processor.SaveAndClearParticipantsAction = new Action(() =>
			{
				saveAndClearCallCount++;
				if (saveAndClearCallCount == 1)
				{
					throw new Exception("Save current failed");
				}
				else if (saveAndClearCallCount > 1 && saveAndClearCallCount < 5)
				{
					throw zSaveEx;
				}
				else
				{
					processor.ShouldActivateBaseSaveAndClear = true;
				}
			});

			processor.Process(notifications);
			var newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Received, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);

			var messageNotes = goodMessage.Notes.FindByDescription("Processing Log");
			AssertEquals(1, messageNotes.Length);
			var messageNoteText = messageNotes[0].ST_NoteDataAsText;
			AssertEquals(string.Empty, messageNoteText);

			var notificationText = notifications.AsString;
			var expectedLog =
@"Processing message batch. Number of messages: '1'
Processing message ''
Processing finished. Saving changes...
Warning: Save current failed
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message ''
Warning: Failed, retry... Error Message : Server cancelled the operation due to deadlock with another operation. Please try again.
Processing message ''
Warning: Failed, retry... Error Message : Server cancelled the operation due to deadlock with another operation. Please try again.
Processing message ''
Warning: Failed, retry... Error Message : Server cancelled the operation due to deadlock with another operation. Please try again.
Processing message ''
Finished message processing.
";
			AssertEquals(expectedLog, Regex.Replace(notificationText, "[0-9][0-9]+", ""));
		}

		public void TestProcessOneByOneFailOnDeadlockAndRetryFail()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			goodMessage.EM_MessageNum = "00000000000000000001";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage.PK });

			processor.Setup(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), null))
				.Throws(new Exception("Save current failed"));
			var zSaveEx = CreateTestZSaveExceptionIncludingDeadlock();
			processor.SetupSequence(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsNotNull<EDIMessage>()))
				.Throws(zSaveEx)
				.Throws(zSaveEx)
				.Throws(zSaveEx)
				.Throws(zSaveEx)
				.Throws(zSaveEx);

			processor.Setup(m => m.ReportError(It.IsAny<Exception>(), It.IsAny<string>()));

			processor.Object.Process(notifications);
			var newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Error, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);

			var messageNotes = goodMessage.Notes.FindByDescription("Processing Log");
			AssertEquals(1, messageNotes.Length);
			var messageNoteText = messageNotes[0].ST_NoteDataAsText;
			var saveExceptionFriendlyMessage = @"was deadlocked on lock resources with another process and has been chosen as the deadlock victim.";
			Assert(messageNoteText.Contains(saveExceptionFriendlyMessage));

			var notificationText = notifications.AsString;
			var partialExpectedLog =
@"Processing message batch. Number of messages: '1'
Processing message '00000000000000000001'
Processing finished. Saving changes...
Warning: Save current failed
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '00000000000000000001'
Warning: Failed, retry... Error Message : Server cancelled the operation due to deadlock with another operation. Please try again.
Processing message '00000000000000000001'
Warning: Failed, retry... Error Message : Server cancelled the operation due to deadlock with another operation. Please try again.
Processing message '00000000000000000001'
Warning: Failed, retry... Error Message : Server cancelled the operation due to deadlock with another operation. Please try again.
Processing message '00000000000000000001'
Warning: Cannot Save... Server cancelled the operation due to deadlock with another operation. Please try again.
Warning: Handled save exception, retry...
Processing message '00000000000000000001'
Error: Error to save message:";
			Assert(notificationText.Contains(partialExpectedLog));
			Assert(notificationText.Contains(saveExceptionFriendlyMessage));

			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		#region Help TestProcessOneByOneFailOnDeadlockAndRetry

		ZSaveException CreateTestZSaveExceptionIncludingDeadlock()
		{
			var error = SqlExceptionBuilder.CreateSqlError(1205, 1, 1, Db.ServerName, "Transaction (Process ID 102) was deadlocked on lock resources with another process and has been chosen as the deadlock victim.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var sqlEx = SqlExceptionBuilder.CreateSqlException(errors);
			var dataEx = new ZDataException(sqlEx, null, null);
			return new ZSaveException(dataEx, Factory);
		}

		#endregion

		public void TestProcessOneByOneFailOnZSaveException_HandledZSaveExceptionAndRetry_Succeed()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new StandardXmlProcessor_SealedDummyTestClass();
			processor.SetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO", messageAction.Object);
			string[] companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetNewMessageAvailable(companyCodes, true);
			processor.GetEDIMessagePKsReturns = new List<ZGuid> { goodMessage.PK };

			var exception = SqlExceptionBuilder.CreateSqlException(2627, 0, 1, Db.ServerName, "Violation of PRIMARY KEY constraint 'PK_ID'. Cannot insert duplicate key in object 'dbo.StmALog'", "", 0);
			var zSaveEx = new ZSaveException(new ZDataException(exception, null, null), Factory);

			var saveAndClearCallCount = 0;
			processor.SaveAndClearParticipantsAction = new Action(() =>
			{
				saveAndClearCallCount++;
				if (saveAndClearCallCount == 1)
				{
					throw new Exception("Save current failed");
				}
				else if (saveAndClearCallCount == 2)
				{
					throw zSaveEx;
				}
				else
				{
					processor.ShouldActivateBaseSaveAndClear = true;
				}
			});

			processor.ZSaveExceptionShouldEqual = zSaveEx;

			processor.Process(notifications);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Received, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);

			var messageNotes = goodMessage.Notes.FindByDescription("Processing Log");
			AssertEquals(1, messageNotes.Length);
			var messageNoteText = messageNotes[0].ST_NoteDataAsText;
			AssertEquals(string.Empty, messageNoteText);

			var notificationText = notifications.AsString;
			var expectedLog = string.Format(
@"Processing message batch. Number of messages: '1'
Processing message '{0}'
Processing finished. Saving changes...
Warning: Save current failed
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '{0}'
Warning: Handled save exception, retry...
Processing message '{0}'
Finished message processing.
", goodMessage.EM_MessageNum);
			AssertEquals(expectedLog, notificationText);

			messageAction.VerifyAll();
		}

		public void TestProcessOneByOneFailOnZSaveException_HandledZSaveExceptionAndRetry_Fail()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");
			goodMessage.EM_MessageNum = "00000000000000000001";
			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new StandardXmlProcessor_SealedDummyTestClass();
			processor.SetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO", messageAction.Object);
			string[] companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetNewMessageAvailable(companyCodes, true);
			processor.GetEDIMessagePKsReturns = new List<ZGuid> { goodMessage.PK };

			var exception = SqlExceptionBuilder.CreateSqlException(2627, 0, 1, Db.ServerName, "Violation of PRIMARY KEY constraint 'PK_ID'. Cannot insert duplicate key in object 'dbo.StmALog'", "", 0);
			var zSaveEx = new ZSaveException(new ZDataException(exception, null, null), Factory);

			var saveAndClearCallCount = 0;
			processor.SaveAndClearParticipantsAction = new Action(() =>
			{
				saveAndClearCallCount++;
				if (saveAndClearCallCount == 1)
				{
					throw new Exception("Save current failed");
				}
				else if (saveAndClearCallCount > 1 && saveAndClearCallCount < 4)
				{
					throw zSaveEx;
				}
				else
				{
					processor.ShouldActivateBaseSaveAndClear = true;
				}
			});

			processor.ZSaveExceptionShouldEqual = zSaveEx;
			processor.HandleReportErrorNumberOfTimes = 1;

			processor.Process(notifications);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals(EDIMessage.Status.Error, newFactory.Load<EDIMessage>(goodMessage.PK).EM_Status);

			var messageNotes = goodMessage.Notes.FindByDescription("Processing Log");
			AssertEquals(1, messageNotes.Length);
			var messageNoteText = messageNotes[0].ST_NoteDataAsText;
			Assert(messageNoteText.Contains("Error to save message: 'CargoWise.EntityFramework.ZSaveException:"));

			var notificationText = notifications.AsString;
			var expectedLogStart =
@"Processing message batch. Number of messages: '1'
Processing message '00000000000000000001'
Processing finished. Saving changes...
Warning: Save current failed
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '00000000000000000001'
Warning: Handled save exception, retry...
Processing message '00000000000000000001'
Error: Error to save message: 'CargoWise.EntityFramework.ZSaveException:";
			var expectedLogEnd =
@"Finished message processing.
";
			Assert(notificationText.StartsWith(expectedLogStart));
			Assert(notificationText.EndsWith(expectedLogEnd));

			messageAction.VerifyAll();
		}

		public void TestProcessOneByOneFail_ProcessOneAndRetry_ThrowCriticalException()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage.PK });

			processor.SetupSequence(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsAny<EDIMessage>()))
				.Throws(new Exception("Save current failed"))
				.Pass();
			var criticalEx = new OutOfMemoryException();
			processor.Setup(m => m.ProcessOneAndRetry(It.IsAny<ZGuid>(), It.IsAny<int>())).Throws(criticalEx); //This is for ProcessOne

			AssertExceptionThrown(typeof(OutOfMemoryException), () => processor.Object.Process(notifications));

			messageAction.VerifyAll();
		}

		public void TestProcessOneByOneFail_ProcessOneAndRetry_ThrowException_Then_GetMessageAction_ThrowCriticalException()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupSequence(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object).CallBase();
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.Setup(m => m.NewMessageAvailable(out companyCodes)).Returns(true);
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage.PK });

			var criticalEx = new OutOfMemoryException();
			processor.SetupSequence(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Throws(criticalEx).CallBase();

			AssertExceptionThrown(typeof(OutOfMemoryException), () => processor.Object.Process(notifications));

			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessOneByOneFail_ProcessOneAndRetry_ThrowException_Then_SendFailNotificationEmail_ThrowCriticalException()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.Setup(m => m.NewMessageAvailable(out companyCodes)).Returns(true);
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid>() { goodMessage.PK });

			processor.Setup(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsAny<EDIMessage>())).Throws(new Exception("Save current failed")); //This is for ProcessInBatch.
			processor.SetupSequence(m => m.ProcessOneAndRetry(It.IsAny<ZGuid>(), It.IsAny<int>())).Throws(new Exception("Exception that ProcessOneAndRetry Thrown")).Pass(); //This is for ProcessOne
			processor.Setup(m => m.ReportError(It.IsAny<Exception>(), It.IsAny<string>()));
			var criticalEx = new OutOfMemoryException();
			processor.SetupSequence(m => m.SendFailNotificationEmail(It.IsAny<EDIMessage>(), It.IsAny<IMessageAction>(), It.IsAny<ZString>())).Throws(criticalEx).Pass();

			AssertExceptionThrown(typeof(OutOfMemoryException), () => processor.Object.Process(notifications));

			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessOneByOneFail_ThrowExceptionNotReportedToEDI()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid>() { goodMessage.PK });

			processor.Setup(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsAny<EDIMessage>())).Throws(new ZException("do not report to EDI", "NR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddress")); //This is for ProcessInBatch.

			processor.Object.Process(notifications);

			messageAction.VerifyAll();
			processor.VerifyAll();

			AssertEquals("no error reported", 0, ErrorReporter.TotalErrorCount);
			AssertNullOrEmpty("exception report key", ErrorReporter.LastKeyReported);
		}

		public void TestProcessOneByOneFail_ProcessOneAndRetry_ThrowException_Then_ReportOnce()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid>() { goodMessage.PK });
			processor.Setup(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsAny<EDIMessage>())).Throws(new InvalidOperationException()); //This is for ProcessInBatch.

			processor.Object.Process(notifications);

			messageAction.VerifyAll();
			processor.VerifyAll();

			AssertEquals("get one error report", 1, ErrorReporter.TotalErrorCount);
			var lastKey = ErrorReporter.LastKeyReported;
			AssertEquals("exception report key", $"StandardXmlProcessor.ProcessOneByOne: System.InvalidOperationException. MessageAction: {messageAction.Object}", lastKey);
			ErrorReporter.Clear();
		}

		public void TestProcessOneByOneFail_DatabaseIsUpgrading()
		{
			var goodMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOO");

			Factory.Save();

			var notifications = new NotificationBuffer();

			var messageAction = new Mock<IMessageAction>();
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOO")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage.PK });

			processor.Setup(m => m.SaveAndClearParticipants(It.IsAny<List<ITransactionParticipant>>(), It.IsAny<EDIMessage>()))
				.Throws(new Exception("Save current failed"));
			processor.SetupSequence(m => m.ProcessOneAndRetry(It.IsAny<ZGuid>(), It.IsAny<int>())).Throws(new DatabaseUpgradeInProgressException()).Pass();

			AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => processor.Object.Process(notifications));

			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestProcessOneByOneFailWithSqlLockLostExceptionThrown()
		{
			var pks = new List<ZGuid> { ZGuid.NewZGuid() };
			var notifications = new NotificationBuffer();

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			var companyCodes = new[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.Setup(m => m.NewMessageAvailable(out companyCodes)).Returns(true);
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(pks);
			processor.Setup(m => m.AlwaysProcessOneByOne).Returns(true);
			processor.SetupSequence(m => m.ProcessOneAndRetry(It.IsAny<ZGuid>(), It.IsAny<int>())).Throws(new SqlLockLostException()).Pass();
			AssertExceptionThrown(typeof(SqlLockLostException), () => processor.Object.Process(notifications));

			processor.VerifyAll();
		}

		public void TestMessageProcessingVerboseLogingForMemoryProblem()
		{
			var goodMessage1 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOA");
			var goodMessage2 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOB");
			goodMessage1.EM_MessageNum = "00000000000000000001";
			goodMessage2.EM_MessageNum = "00000000000000000002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("processed"));
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Object.SetIsVerboseLoggingForMemoryProblemForTesting(true);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOA")).Returns(messageAction.Object);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOB")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage1.PK, goodMessage2.PK });

			processor.Object.Process(notifications);
			Factory.ReloadAll<XmlEDIMessage>();
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(goodMessage1.PK).EM_Status);
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(goodMessage2.PK).EM_Status);

			AssertEquals(1, goodMessage1.Notes.FindByDescription("Processing Log").Length);
			AssertEquals(1, goodMessage2.Notes.FindByDescription("Processing Log").Length);

			var regEx = new Regex(@"Current memory usage: [0-9]+MB.
Processing message batch. Number of messages: '2'
Processing message '00000000000000000001'
Processing message '00000000000000000002'
Processing finished. Saving changes...
Current memory usage: [0-9]+MB.
Current memory usage: [0-9]+MB.
Finished message processing.
Current memory usage: [0-9]+MB.".Replace("'", "\\'"));

			AssertLogs(regEx, notifications.AsString.Trim());

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.AtLeast(2));
			messageAction.Verify(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()), Times.AtLeast(2));
			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestMessageProcessingVerboseLogingForBatchReprocessingProblem()
		{
			var goodMessage1 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOA");
			var goodMessage2 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOB");
			goodMessage1.EM_MessageNum = "00000000000000000001";
			goodMessage2.EM_MessageNum = "00000000000000000002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			List<ITransactionParticipant> forSave;
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					return true;
				}));

			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("processed"));
				});

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			processor.SetupAllProperties();
			processor.Object.SetIsVerboseLoggingForBatchReprocessingProblemForTesting(true);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOA")).Returns(messageAction.Object);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, "GOB")).Returns(messageAction.Object);
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { goodMessage1.PK, goodMessage2.PK });

			processor.Object.Process(notifications);
			Factory.ReloadAll<XmlEDIMessage>();
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(goodMessage1.PK).EM_Status);
			AssertEquals(EDIMessage.Status.Received, Factory.Load<EDIMessage>(goodMessage2.PK).EM_Status);

			AssertEquals(1, goodMessage1.Notes.FindByDescription("Processing Log").Length);
			AssertEquals(1, goodMessage2.Notes.FindByDescription("Processing Log").Length);

			var regEx = new Regex(@"Processing message batch. Number of messages: '2'
Processing message '00000000000000000001'
Processing message '00000000000000000001' in status 'QUE' and factory id '[0-9]+' in batch mode.
Message '00000000000000000001' in status 'RCV' and factory id '[0-9]+' in batch mode was processed.
Processing message '00000000000000000002'
Processing message '00000000000000000002' in status 'QUE' and factory id '[0-9]+' in batch mode.
Message '00000000000000000002' in status 'RCV' and factory id '[0-9]+' in batch mode was processed.
Processing finished. Saving changes...
Saving participants '1'. Current factory id '[0-9]+'.
Finished message processing.".Replace("'", "\\'"));

			AssertLogs(regEx, notifications.AsString.Trim());

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.AtLeast(2));
			messageAction.Verify(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()), Times.AtLeast(2));
			messageAction.VerifyAll();
			processor.VerifyAll();
		}

		public void TestAlwaysProcessOneByOne()
		{
			var processor = new Mock<StandardXmlProcessor>();
			AssertEquals(false, processor.Object.AlwaysProcessOneByOne);
		}

		public void TestProcess_AlwaysProcessOneByOne()
		{
			var pks = new List<ZGuid>();
			pks.Add(ZGuid.NewZGuid());
			var notifications = new NotificationBuffer();

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(pks);
			processor.Setup(m => m.AlwaysProcessOneByOne).Returns(true);
			processor.Setup(m => m.ProcessOneByOne(It.IsAny<MessageBatch>(), It.IsAny<CancellationToken>()))
				.Callback((MessageBatch messageBatch, CancellationToken token) =>
				{
					AssertEquals(1, messageBatch.MessagePKs.Count);
					AssertEquals(pks[0], messageBatch.MessagePKs[0]);
				});

			processor.Object.Process(notifications);
			processor.VerifyAll();
		}

		public void TestOtherException_EmailNotification()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GRP";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "~TT";
			staff.GS_EmailAddress = "blah@blah.com";
			NotificationDataRegistry.Instance.XMSFailureFallBackNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var badMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, EDIMessageSubTypeList.Codes.Shipments);
			badMessage.EM_MessageNum = "00000000000000000001";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var provider = new BusinessObjectFactoryProvider();
			provider.CreateNewWithoutSave();
			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { badMessage.PK });
			processor.SetupSequence(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, It.IsAny<ZString>()))
				.Throws(new Exception("Error When Getting messageAction"))
				.Throws(new Exception("Error When Getting messageAction"))
				.Throws(new Exception("Error When Getting messageAction"))
				.CallBase();
			processor.Setup(m => m.ReportError(It.IsAny<Exception>(), It.IsAny<string>()));

			processor.Object.Process(notifications);
			var newFactory = new BusinessObjectFactory();
			var emails = newFactory.Load<MailItem>(new ZQuery());

			AssertEquals("A email should be created.", 1, emails.Length);
			AssertEquals("email.MI_Subject", "EDI Message #00000000000000000001 Type SHP rejected", emails[0].MI_Subject);
			var partOfExpectedNotification =
@"Processing message batch. Number of messages: '1'
Processing message '00000000000000000001'
Warning: Error When Getting messageAction
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '00000000000000000001'
Error: Error to save message: 'System.Exception: Error When Getting messageAction";
			Assert(notifications.AsString.Contains(partOfExpectedNotification));
			Assert(notifications.AsString.Contains("Finished message processing."));
			processor.VerifyAll();
		}

		public void TestDeadlock_EmailNotification()
		{
			var badMessage = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, EDIMessageSubTypeList.Codes.Shipments);
			badMessage.EM_MessageNum = "00000000000000000001";
			Factory.Save();

			var notifications = new NotificationBuffer();

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(x => x.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { badMessage.PK });
			List<ITransactionParticipant> forSave;
			var messageAction = new Mock<IMessageAction>(MockBehavior.Strict);
			processor.Setup(m => m.GetMessageAction(EDIMessageTypeList.Codes.XMS, It.IsAny<ZString>())).Returns(messageAction.Object);
			messageAction.Setup(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave))
				.Returns(new ReturnsDelegate((EDIMessage m, INotifications n, out List<ITransactionParticipant> p) =>
				{
					p = new List<ITransactionParticipant>();
					throw new ZDataException(new Exception(), null, null);
				}));

			messageAction.Setup(m => m.SendNotificationEmail(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<INotifications>(), It.IsAny<bool>()))
				.Callback((ZString subject, ZString body, INotifications n, bool onSuccess) =>
				{
					Assert(subject.StartsWith("EDI Message #"));
					Assert(subject.EndsWith("rejected"));
				});

			processor.Setup(m => m.ReportError(It.IsAny<Exception>(), It.IsAny<string>()));

			processor.Object.Process(notifications);
			var partOfExpectedNotification =
@"Processing message batch. Number of messages: '1'
Processing message '00000000000000000001'
Warning: <ROW IS NULL>
InnerException Message = Exception of type 'System.Exception' was thrown.
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '00000000000000000001'
Error: Error to save message: 'CargoWise.EntityFramework.ZDataException: <ROW IS NULL>";
			Assert(notifications.AsString.Contains(partOfExpectedNotification));
			Assert(notifications.AsString.Contains("Finished message processing."));

			messageAction.Verify(m => m.ExecuteAction(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out forSave), Times.AtLeast(2));
			processor.VerifyAll();
			messageAction.VerifyAll();
		}

		public void TestProcessValidateDateRange()
		{
			var message = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "ORD");
			message.EM_MessageNum = "00000000000000000001";

			#region EM_MessageText

			message.EM_MessageText = @"<Orders xmlns=""http://www.edi.com.au/EnterpriseService/"">
    <Order>
        <OrderIdentifier>
            <OrderNumber>HC-PO084450</OrderNumber>
            <OrderNumberSplit>0</OrderNumberSplit>
        </OrderIdentifier>
        <OrderDetail>
            <Buyer>
                <OrganisationDetails>
                    <Name>HomeChoice</Name>
                </OrganisationDetails>
                <Addresses>
                    <Address AddressType=""Delivery"" AddressLine1=""7 School Road"" AddressLine2=""Blackheath Industrial Park"" CityOrSuburb=""BLACKHEATH"" PostCode=""7580"">
                        <AddressCode>HCDC Blackheath</AddressCode>
                    </Address>
                </Addresses>
            </Buyer>
            <Supplier>
                <OrganisationDetails>
                    <Name>VALUE SOURCE GIFTWARE (SHANGHAI) CO LTD</Name>
                </OrganisationDetails>
            </Supplier>
            <OrderStatus>INC</OrderStatus>
            <DeliveryRequiredBy>2021-05-12T06:31:30+02:00</DeliveryRequiredBy>
            <Description>Bedroom Textiles</Description>
            <OrderDateTime>2021-05-12T06:31:30+02:00</OrderDateTime>
            <OrderTotal CurrencyCode=""USD"">0</OrderTotal>
            <Incoterm>FOB</Incoterm>
            <TransportMode>SEA</TransportMode>
            <Custom>
                <Date1>0001-01-01T11:00:00</Date1>
            </Custom>
        </OrderDetail>
        <OrderLines>
            <OrderLine>
                <OrderLineNo>10</OrderLineNo>
                <OrderLineDetail>
                    <Product>36391</Product>
                    <Description>Ronda 5pc Frilled Sheet Set</Description>
                    <QtyOrdered DimensionType=""UNT"">5</QtyOrdered>
                    <QtyInvoiced DimensionType=""UNT"">0</QtyInvoiced>
                    <QtyReceived DimensionType=""UNT"">0</QtyReceived>
                    <QtyReceivedToDate DimensionType=""UNT"">0</QtyReceivedToDate>
                    <InnerPacks>0</InnerPacks>
                    <OuterPacks>0</OuterPacks>
                    <ItemPrice>0</ItemPrice>
                    <LinePrice>0</LinePrice>
                    <LineStatus>PLC</LineStatus>
                    <Custom>
                        <Text1>227042</Text1>
                        <CustomText1>Ronda 5pc Three Quarter Blush Frilled Sheet Set</CustomText1>
                        <Text5>10</Text5>
                    </Custom>
                    <Weight>0</Weight>
                    <Volume>0</Volume>
                </OrderLineDetail>
            </OrderLine>
        </OrderLines>
    </Order>
</Orders>";

			#endregion

			Factory.Save();

			var notifications = new NotificationBuffer();

			var processor = new Mock<StandardXmlProcessor> { CallBase = true };

			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(x => x.NewMessageAvailable(out companyCodes)).Returns(true).CallBase();
			processor.Setup(m => m.GetEDIMessagePKs()).Returns(new List<ZGuid> { message.PK });

			processor.Object.Process(notifications);
			Factory.ReloadAll<XmlEDIMessage>();
			AssertEquals(EDIMessage.Status.Error, Factory.Load<EDIMessage>(message.PK).EM_Status);

			AssertContains("Processing message batch. Number of messages: '1'", notifications.AsString);
			AssertContains("Processing message '00000000000000000001'", notifications.AsString);
			AssertContains("Error: Custom Date1 field is invalid. 1/01/0001 11:00:00 AM is not in range [01-Jan-1900 - 06-Jun-2079].", notifications.AsString);
			AssertContains("Warning: Error(e.g. XML validation error) that prevent save happened. Message process failed.", notifications.AsString);
			AssertContains("Finished message processing.", notifications.AsString);
		}

		// Testing class for use with Test: TestProcessOneByOneFailOnDeadlockAndRetrySucceed. Functionality unable to be achieved with Moq.
		sealed class StandardXmlProcessor_SealedDummyTestClass : StandardXmlProcessor
		{
			public List<ZGuid> GetEDIMessagePKsReturns { get; set; }
			IMessageAction getMessageActionReturns { get; set; }
			ZString messageActionTypeMustEqual { get; set; } = null;
			ZString messageActionSubTypeMustEqual { get; set; } = null;
			bool[] newMessageAvailableReturns { get; set; } = new bool[] { false };
			int newMessageAvailableCallNumber { get; set; }
			string[] companyCodes { get; set; }
			public Action SaveAndClearParticipantsAction { get; set; }
			public bool ShouldActivateBaseSaveAndClear { get; set; }
			public int NumberOfTimesHandleZSaveException { get; set; } = 1;
			public int HandleZSaveExceptionCallCount { get; set; }
			public ZSaveException ZSaveExceptionShouldEqual { get; set; }
			public int HandleReportErrorNumberOfTimes { get; set; }
			public int ReportErrorCalledTimes { get; set; }

			internal override List<ZGuid> GetEDIMessagePKs()
			{
				return GetEDIMessagePKsReturns;
			}

			internal override IMessageAction GetMessageAction(ZString messageType, ZString messageSubType)
			{
				if (messageActionTypeMustEqual == messageType && messageActionSubTypeMustEqual == messageSubType)
				{
					return getMessageActionReturns;
				}
				else
				{
					return base.GetMessageAction(messageType, messageSubType);
				}
			}

			internal void SetMessageAction(ZString messageType, ZString messageSubType, IMessageAction messageAction)
			{
				getMessageActionReturns = messageAction;
				messageActionTypeMustEqual = messageType;
				messageActionSubTypeMustEqual = messageSubType;
			}

			internal override bool NewMessageAvailable(out string[] companyCodes)
			{
				newMessageAvailableCallNumber++;
				companyCodes = this.companyCodes;

				if (newMessageAvailableCallNumber > newMessageAvailableReturns.Length)
				{
					return base.NewMessageAvailable(out companyCodes);
				}

				return newMessageAvailableReturns[newMessageAvailableCallNumber - 1];
			}

			internal void SetNewMessageAvailable(string[] companyCodes, params bool[] shouldReturn)
			{
				this.companyCodes = companyCodes;
				newMessageAvailableReturns = shouldReturn;
			}

			internal override void SaveAndClearParticipants(List<ITransactionParticipant> participants, EDIMessage message = null)
			{
				SaveAndClearParticipantsAction?.Invoke();

				if (ShouldActivateBaseSaveAndClear)
				{
					base.SaveAndClearParticipants(participants, message);
				}
			}

			internal override void HandleZSaveException(ZSaveException saveEx)
			{
				if (ZSaveExceptionShouldEqual == saveEx && NumberOfTimesHandleZSaveException > HandleZSaveExceptionCallCount)
				{
					HandleZSaveExceptionCallCount++;
				}
				else
				{
					base.HandleZSaveException(saveEx);
				}
			}

			internal override void ReportError(Exception ex, string key = "")
			{
				if (HandleReportErrorNumberOfTimes > ReportErrorCalledTimes)
				{
					ReportErrorCalledTimes++;
				}
				else
				{
					base.ReportError(ex, key);
				}
			}
		}
	}
}
