using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.ES.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ESMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ESMessageSender(null));
		}

		public void TestGetMessageBuildersData()
		{
			var staff = Factory.GetStaffAccount();

			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true);

			CombineAssertions(() =>
			{
				var messageSendingObject = new MessageSendingObject(emptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);

				var messageBuildersData = sender.GetMessageBuildersData();
				AssertEquals("The returned messagebuildersdata list has 1 element", 1, messageBuildersData.Count);
				AssertEquals("The returned messagebuildersdata element has FailureFlag true", true, messageBuildersData[0].FailureFlag);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				messageSendingObject = new MessageSendingObject(declaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.ShouldSend = true;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportUcc6;
				var entryHeader1 = sendingObject1.Header;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.ShouldSend = true;
				sendingObject2.MessageType = DeclarationMessageTypeList.Codes.ExportUcc6;
				var entryHeader2 = sendingObject2.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				AssertEquals("The returned messagebuildersdata list has 2 elements", 2, messageBuildersData.Count);
				AssertNotNull("The returned messagebuildersdata element 1 has MessageBuilder not null", messageBuildersData[0].MessageBuilder);
				AssertNotNull("The returned messagebuildersdata element 2 has MessageBuilder not null", messageBuildersData[1].MessageBuilder);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendMessage()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.ShouldSend = true;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportUcc6;
				var entryHeader1 = sendingObject1.Header;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.ShouldSend = true;
				sendingObject2.MessageType = DeclarationMessageTypeList.Codes.ExportUcc6;
				var entryHeader2 = sendingObject2.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The messages were created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				entryHeader1.Messages.Reload(true);
				var msg1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for first entry header", msg1);
				entryHeader2.Messages.Reload(true);
				var msg2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for second entry header", msg2);
			});
		}

		public void TestSendMessageWithMessageDecorator()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory1.Load<JobDeclaration>(declaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "EXP";
				var entryHeader = sendingObject.Header;

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData, messageDecorator: m => { m.EM_HeldUntilDate = new ZDateTime(2030, 1, 1); m.EM_ApplicationReference = "BBB"; });
				var message = factory1.Load<ESEDIMessage>(new ZQuery()).First();
				AssertEquals("EM_HeldUntilDate should be modified by the decorator.", new ZDateTime(2030, 1, 1), message.EM_HeldUntilDate);
				AssertEquals("EM_ApplicationReference should be modified by the decorator.", "BBB", message.EM_ApplicationReference);
			});
		}

		public void TestSendMultipleMessagesWithOneError()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true);

			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var newFactoryDeclaration = factory.Load<JobDeclaration>(declaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageType = "EXP";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.MessageType = "EXP";
				sendingObject2.ShouldSend = true;
				var entryHeader2 = sendingObject2.Header;
				entryHeader2.CH_CEI_Instruction = ZGuid.Empty;
				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory.Save();
				AssertEquals("Only 1 message has been created and sent, MessagesSent is 1", 1, result.MessagesSent);
				AssertEquals("Only 1 message has been created and sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.SendIndividualDeclaration", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				entryHeader1.Messages.Reload(true);
				var msg1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for first entry header", msg1);
				entryHeader2.Messages.Reload(true);
				var msg2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertNull("No EDIMessage was created for second entry header", msg2);

				var queryMessage = new ZQuery(EDIMessageSchema.EM_MessageType, "EXP");
				queryMessage.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "ESC");
				queryMessage.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.NotEqual, ZString.Empty);
				var messagesInDatabase = Factory.Load<EDIMessage>(queryMessage);
				AssertEquals("There should only be one edimessage saved in the database", 1, messagesInDatabase.Length);
			});
		}

		public void TestSendAllButTheLastT2LAnnexMessage()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LAnnexDeclaration(true);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("Only 1 annex message has been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msgs = entryHeader1.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX").ToList();
				AssertEquals("There are 2 TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type T2A", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.T2lAnnex));
				AssertContains("New EDIMessage has last annex flag to false because there is one annex left to send, first", "indicadorFinDeAnexado>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessage has last annex flag to false because there is one annex left to send, second", "indicadorFinDeAnexado>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to the sent annex", 1, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to the sent annex", 1, pivotsForSecondMessage.Count());
			});
		}

		public void TestSendLastT2LAnnexMessage()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LAnnexDeclaration(false);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("1 annex message has been created and sent, MessagesSent is 1", 1, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msg = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for first entry header", msg);
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to true", "indicadorFinDeAnexado>S", msg.EM_MessageText);

				var msgPivot = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault().Message;
				AssertEquals("New EDIMessage is associated to the annex", msgPivot, msg);
			});
		}

		public void TestTrySendLastT2LAnnexMessageWithError()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LAnnexDeclaration(false, "Invoice.xxx");

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception in the builder", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception in the sender", "ESMessageSender.SendIndividualDeclaration", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendAllButTheLastAESAnnexMessage_RequestDispatchN()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateAESAnnexDeclaration(true);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportAnnexes;
				sendingObject1.RequestDispatch = "N";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("2 annex messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader1.CH_EntryStatus);
				AssertEquals("ZG_RequestDispatch was set to N", "N", entryHeader1.ZG_RequestDispatch);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msgs = entryHeader1.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX").ToList();
				AssertEquals("There are 2 TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type EDA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.ExportAnnexes));
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, first", "finAnexos>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, second", "finAnexos>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 9, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to 4 of the sent annexes", 4, pivotsForSecondMessage.Count());
			});
		}

		public void TestSendAllButTheLastAESAnnexMessage_RequestDispatchY()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateAESAnnexDeclaration(true);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportAnnexes;
				sendingObject1.RequestDispatch = "Y";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("2 annex messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader1.CH_EntryStatus);
				AssertEquals("ZG_RequestDispatch was set to Y", "Y", entryHeader1.ZG_RequestDispatch);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msgs = entryHeader1.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX").ToList();
				AssertEquals("There are 2 TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type EDA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.ExportAnnexes));
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, first", "finAnexos>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, second", "finAnexos>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 9, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to 4 of the sent annexes", 4, pivotsForSecondMessage.Count());
			});
		}

		public void TestSendLastAESAnnexMessage_RequestDispatchN()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateAESAnnexDeclaration(false);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportAnnexes;
				sendingObject1.RequestDispatch = "N";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("No annex message has been created or sent since there is only one annex left and request dispatch is N, MessagesSent is 0", 0, result.MessagesSent);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("ZG_RequestDispatch was not changed", ZString.Empty, entryHeader1.ZG_RequestDispatch);
				AssertEquals("LastKeyReported has no exception", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendLastAESAnnexMessage_RequestDispatchY()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateAESAnnexDeclaration(false);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportAnnexes;
				sendingObject1.RequestDispatch = "Y";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("1 annex message has been created and sent, MessagesSent is 1", 1, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader1.CH_EntryStatus);
				AssertEquals("ZG_RequestDispatch was set to Y", "Y", entryHeader1.ZG_RequestDispatch);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msg = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for entry header", msg);
				AssertEquals("New EDIMessage is type EDA", DeclarationMessageTypeList.Codes.ExportAnnexes, msg.EM_MessageType);
				AssertContains("New EDIMessage has dispatch request flag to S since it is the last annex and the flag in the form is Y", "finAnexos>S", msg.EM_MessageText);

				var msgPivot = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault().Message;
				AssertEquals("New EDIMessage is associated to the annex", msgPivot, msg);
			});
		}

		public void TestTrySendLastAESAnnexMessageWithError()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateAESAnnexDeclaration(false, "Invoice.xxx");

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception in the builder", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportAnnexes;
				sendingObject1.RequestDispatch = "Y";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("ZG_RequestDispatch was not changed", ZString.Empty, entryHeader1.ZG_RequestDispatch);
				AssertEquals("LastKeyReported has exception in the sender", "ESMessageSender.SendIndividualDeclaration", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendAllButTheLastDocumentationT2LPOUSMessage_RequestDispatchN()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LPOUSDeclarationForAnnexes(true);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.T2lDocumentationPous;
				sendingObject1.RequestDispatch = "N";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("2 annex messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader1.CH_EntryStatus);
				AssertEquals("ZG_RequestDispatch was set to N", "N", entryHeader1.ZG_RequestDispatch);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msgs = entryHeader1.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX").ToList();
				AssertEquals("There are 2 TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type T2D", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.T2lDocumentationPous));
				AssertContains("New EDIMessage has dispatch request flag to N because there is one annex left to send, first", "Valor>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessage has dispatch request flag to N because there is one annex left to send, second", "Valor>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to the sent annex", 1, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to the sent annex", 1, pivotsForSecondMessage.Count());
			});
		}

		public void TestSendAllButTheLastDocumentationT2LPOUSMessage_RequestDispatchY()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LPOUSDeclarationForAnnexes(true);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.T2lDocumentationPous;
				sendingObject1.RequestDispatch = "Y";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("2 annex messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader1.CH_EntryStatus);
				AssertEquals("ZG_RequestDispatch was set to Y", "Y", entryHeader1.ZG_RequestDispatch);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msgs = entryHeader1.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX").ToList();
				AssertEquals("There are 2 TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type T2D", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.T2lDocumentationPous));
				AssertContains("New EDIMessage has dispatch request flag to N because there is one annex left to send, first", "Valor>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessage has dispatch request flag to N because there is one annex left to send, second", "Valor>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to the sent annex", 1, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to the sent annex", 1, pivotsForSecondMessage.Count());
			});
		}

		public void TestSendLastDocumentationT2LPOUSMessage_RequestDispatchN()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LPOUSDeclarationForAnnexes(false);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.T2lDocumentationPous;
				sendingObject1.RequestDispatch = "N";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("No annex message has been created or sent since there is only one annex left and request dispatch is N, MessagesSent is 0", 0, result.MessagesSent);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("ZG_RequestDispatch was not changed", ZString.Empty, entryHeader1.ZG_RequestDispatch);
				AssertEquals("LastKeyReported has no exception", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendLastDocumentationT2LPOUSMessage_RequestDispatchY()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LPOUSDeclarationForAnnexes(false);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.T2lDocumentationPous;
				sendingObject1.RequestDispatch = "Y";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("1 annex message has been created and sent, MessagesSent is 1", 1, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader1.CH_EntryStatus);
				AssertEquals("ZG_RequestDispatch was set to Y", "Y", entryHeader1.ZG_RequestDispatch);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msg = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for entry header", msg);
				AssertEquals("New EDIMessage is type T2D", DeclarationMessageTypeList.Codes.T2lDocumentationPous, msg.EM_MessageType);
				AssertContains("New EDIMessage has dispatch request flag to S since it is the last annex and the flag in the form is Y", "Valor>S", msg.EM_MessageText);

				var msgPivot = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault().Message;
				AssertEquals("New EDIMessage is associated to the annex", msgPivot, msg);
			});
		}

		public void TestTrySendLastDocumentationT2LPOUSMessageWithError()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LPOUSDeclarationForAnnexes(false, "Invoice.xxx");

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception in the builder", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.T2lDocumentationPous;
				sendingObject1.RequestDispatch = "Y";
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("ZG_RequestDispatch was not changed", ZString.Empty, entryHeader1.ZG_RequestDispatch);
				AssertEquals("LastKeyReported has exception in the sender", "ESMessageSender.SendIndividualDeclaration", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendT2LReceptionPOUSMessage()
		{
			var staff = Factory.GetStaffAccount();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = CreateT2LPOUSDeclarationForAnnexes(true, pousVersion: 2);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.T2lReceptionPous;
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("1 annex message has been created and sent, MessagesSent is 1", 1, result.MessagesSent);
				AssertEquals("EntryStatus has not changed ", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry header message status is awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msg = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for entry header", msg);
				AssertEquals("New EDIMessage is type T2I", DeclarationMessageTypeList.Codes.T2lReceptionPous, msg.EM_MessageType);
				AssertContains("New EDIMessage has first annex", "Invoice.pdf", msg.EM_MessageText);
				AssertContains("New EDIMessage has second annex", "Invoice2.txt", msg.EM_MessageText);
				AssertContains("New EDIMessage has third annex", "Invoice3.txt", msg.EM_MessageText);

				var pivotsForMessage = entryHeader1.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msg);
				AssertEquals("New EDIMessage is associated to all 3 annexes", entryHeader1.EDocPivotCollection.Count, pivotsForMessage.Count());
			});
		}

		public void TestSendPDCAndPDSMessages()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import, entryInstructionSubStyle2: EntrySubStyleList.Codes.C);

			var expectedTotalAmountEntry1 = 1000.200M;
			var expectedTotalAmountEntry2 = 2000.200M;
			AddFees(declaration, expectedTotalAmountEntry1, expectedTotalAmountEntry2);

			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var newFactoryDeclaration = factory.Load<JobDeclaration>(declaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.ShouldSend = true;
				var entryHeader1 = sendingObject1.Header;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.ShouldSend = true;
				var entryHeader2 = sendingObject2.Header;
				sendingObject2.MessageType = DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;

				AssertEquals("First Entry's TotalAmount is empty (0) before sending", ZDecimal.Zero, entryHeader1.TotalAmount);
				AssertEquals("Second Entry's TotalAmount is empty (0) before sending", ZDecimal.Zero, entryHeader2.TotalAmount);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory.Save();
				AssertEquals("The 2 messages were created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("First Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("First Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("First Entry's TotalAmount has changed", expectedTotalAmountEntry1, entryHeader1.TotalAmount);
				AssertEquals("Second Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Second EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("Second Entry's TotalAmount has changed", expectedTotalAmountEntry2, entryHeader2.TotalAmount);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				entryHeader1.Messages.Reload(true);
				var msg1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for first entry header", msg1);
				entryHeader2.Messages.Reload(true);
				var msg2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for second entry header", msg2);
			});
		}

		public void TestSendDVDQueryMessageNotCanaryIsland()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import, entryInstructionSubStyle2: EntrySubStyleList.Codes.A, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001278", ZDateTime.Today);
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.DvdH2Query;
				sendingObject1.ShouldSend = true;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.MessageType = DeclarationMessageTypeList.Codes.DvdH2Query;
				sendingObject2.ShouldSend = true;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The 2 messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var msg1 = entryHeader1.Messages;
				AssertEquals("1 EDIMessage was created for first entry header", 1, msg1.Count);
				AssertNotContains("Sent DQU message for first entry header is not for canary islands", "IndicadorDatosATC", msg1[0].EM_MessageText);
				var msg2 = entryHeader2.Messages;
				AssertEquals("1 EDIMessage was created for second entry header", 1, msg2.Count);
				AssertNotContains("Sent DQU message for second entry header is not for canary islands", "IndicadorDatosATC", msg2[0].EM_MessageText);
			});
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestSendDVDQueryMessageCanaryIsland()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import, entryInstructionSubStyle2: EntrySubStyleList.Codes.A, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_CustomsOffice = "ES003500";
				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.DvdH2Query;
				sendingObject1.ShouldSend = true;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				MockRandomGenerator(messageBuildersData);
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The 2 messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var msg1 = entryHeader1.Messages;
				AssertEquals("2 EDIMessages were created for first entry header", 2, msg1.Count);
				AssertContainsExactElementsInAnyOrder("Sent DQU messages are one for canary islands and one for mainland territory", ExpectedDVDQueryMessagesText, msg1.Cast<EDIMessage>().Select(x => x.EM_MessageText));
				var msg2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertNull("EDIMessage was not created for second entry header", msg2);
			});
		}

		public void TestSendImportQueryMessageNotCanaryIsland()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import, entryInstructionSubStyle2: EntrySubStyleList.Codes.C);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001278", ZDateTime.Today);
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ImportQuery;
				sendingObject1.ShouldSend = true;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.MessageType = DeclarationMessageTypeList.Codes.ImportQuery;
				sendingObject2.ShouldSend = true;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The 2 messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var msg1 = entryHeader1.Messages;
				AssertEquals("1 EDIMessage was created for first entry header", 1, msg1.Count);
				AssertNotContains("Sent IQU message for first entry header is not for canary islands", "DatosEnATC", msg1[0].EM_MessageText);
				var msg2 = entryHeader2.Messages;
				AssertEquals("1 EDIMessage was created for second entry header", 1, msg2.Count);
				AssertNotContains("Sent IQU message for second entry header is not for canary islands", "DatosEnATC", msg2[0].EM_MessageText);
			});
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestSendImportQueryMessageCanaryIsland()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import, entryInstructionSubStyle2: EntrySubStyleList.Codes.C);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.ZG_DestinationState = BuilderHelperTest.CanaryIslandCode;
				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ImportQuery;
				sendingObject1.ShouldSend = true;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				MockRandomGenerator(messageBuildersData);
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The 2 messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var msg1 = entryHeader1.Messages;
				AssertEquals("2 EDIMessages were created for first entry header", 2, msg1.Count);
				AssertContainsExactElementsInAnyOrder("Sent IQU messages are one for canary islands and one for mainland territory", ExpectedImportQueryMessagesText, msg1.Cast<EDIMessage>().Select(x => x.EM_MessageText));
				var msg2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertNull("EDIMessage was not created for second entry header", msg2);
			});
		}

		public void TestSendQueryImportH1MessageNotCanaryIsland()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import, entryInstructionSubStyle2: EntrySubStyleList.Codes.A, entryInstructionStyle2: IMPDeclarationTypeList.Codes.IM);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001278", ZDateTime.Today);
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ImportH1Query;
				sendingObject1.ShouldSend = true;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.MessageType = DeclarationMessageTypeList.Codes.ImportH1Query;
				sendingObject2.ShouldSend = true;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The 2 messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var msg1 = entryHeader1.Messages;
				AssertEquals("1 EDIMessage was created for first entry header", 1, msg1.Count);
				AssertNotContains("Sent H1Q message for first entry header is not for canary islands", "ATC", msg1[0].EM_MessageText);
				var msg2 = entryHeader2.Messages;
				AssertEquals("1 EDIMessage was created for second entry header", 1, msg2.Count);
				AssertNotContains("Sent H1Q message for second entry header is not for canary islands", "ATC", msg2[0].EM_MessageText);
			});
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestSendQueryImportH1MessageCanaryIsland()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import, entryInstructionSubStyle2: EntrySubStyleList.Codes.A, entryInstructionStyle2: IMPDeclarationTypeList.Codes.IM);

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_CustomsOffice = "ES003500";
				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ImportH1Query;
				sendingObject1.ShouldSend = true;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				MockRandomGenerator(messageBuildersData);
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The 2 messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var msg1 = entryHeader1.Messages;
				AssertEquals("2 EDIMessages were created for first entry header", 2, msg1.Count);
				AssertContainsExactElementsInAnyOrder("Sent H1Q messages are one for canary islands and one for mainland territory", ExpectedQueryImportH1MessagesText, msg1.Cast<EDIMessage>().Select(x => x.EM_MessageText));
				var msg2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertNull("EDIMessage was not created for second entry header", msg2);
			});
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestSendExportAmendmentMessage()
		{
			var staff = Factory.GetStaffAccount();
			Factory.Save();
			var emptyDeclaration = CreateEmptyDeclaration();
			var declaration = Factory.GetNewJobDeclaration(staff, Supplier, Importer, Declarant, true);
			declaration.JE_CustomsOffice = "ES009999";
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			var officeOfExit = declaration.CustomsOffices.AddNew();
			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;

			CombineAssertions(() =>
			{
				var factory1 = new BusinessObjectFactory();
				var newFactoryEmptyDeclaration = factory1.Load<JobDeclaration>(emptyDeclaration.PK);
				var messageSendingObject = new MessageSendingObject(newFactoryEmptyDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = "AAA";
				var entryHeader = sendingObject.Header;
				AssertNotEquals("Entry header message status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not awaiting response", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);

				var sender = new ESMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				var result = ESMessageSender.Send(messageBuildersData);
				factory1.Save();
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("Entry header doesn't have messages", false, entryHeader.Messages.Any());
				AssertNotEquals("Entry header message status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertNotEquals("Entry header status not AwaitingResponse", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
				AssertEquals("LastKeyReported has exception", "ESMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				Factory.Save();
				entryHeader1.CH_BGMReference = "ES000001";
				entryHeader2.CH_BGMReference = "ES000002";
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var newFactoryDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
				messageSendingObject = new MessageSendingObject(newFactoryDeclaration, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First(x => x.Header.CH_BGMReference == entryHeader1.CH_BGMReference);
				sendingObject1.ShouldSend = true;
				sendingObject1.MessageType = DeclarationMessageTypeList.Codes.ExportUcc6;
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First(x => x.Header.CH_BGMReference == entryHeader2.CH_BGMReference);
				sendingObject2.ShouldSend = true;
				sendingObject2.MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment;
				sendingObject2.MessageType = DeclarationMessageTypeList.Codes.ExportAmendmentUcc6;
				sender = new ESMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				MockRandomGenerator(messageBuildersData);
				result = ESMessageSender.Send(messageBuildersData);
				factory2.Save();
				AssertEquals("The messages were created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for first entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("EntryStatus has not changed to awaiting for second entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				entryHeader1.Messages.Reload(true);
				var msg1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for first entry header", msg1);
				AssertMultilineASCIIEquals("First entry has a normal AES export message with the correct text", ExpectedStringDUAExport, msg1.EM_MessageText);
				entryHeader2.Messages.Reload(true);
				var msg2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for second entry header", msg2);
				AssertMultilineASCIIEquals("First entry has an AES export amendment message with the correct text", ExpectedStringDUAExportAmendment, msg2.EM_MessageText);
			});
		}

		void MockRandomGenerator(List<ESMessageSender.MessageBuilderData> messageBuildersData)
		{
			messageBuildersData.ForEach(d =>
			{
				RandomGeneratorHelper.MockRandomGenerator(d.MessageBuilder, 4560);
			});
		}

		ZString[] ExpectedDVDQueryMessagesText => new ZString[]
{
			ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaDVDH2V1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/ConsultaDVDH2V1Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}Mensaje>
    <{XMLTestFileConstants.XmlElementNamespace}SegmentosDeServicio>
      <Id xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd"">ES2001091613234560</Id>
      <FechaPreparacion xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd"">20200109</FechaPreparacion>
      <HoraPreparacion xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd"">161323</HoraPreparacion>
    </{XMLTestFileConstants.XmlElementNamespace}SegmentosDeServicio>
    <{XMLTestFileConstants.XmlElementNamespace}MRN_Operacion>20ES00999830001277</{XMLTestFileConstants.XmlElementNamespace}MRN_Operacion>
  </{XMLTestFileConstants.XmlElementNamespace}Mensaje>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaDVDH2V1Ent>
  </soapenv:Body>
</soapenv:Envelope>"),
			ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaDVDH2V1Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/ConsultaDVDH2V1Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}Mensaje>
    <{XMLTestFileConstants.XmlElementNamespace}SegmentosDeServicio>
      <Id xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd"">ES2001091613234560</Id>
      <FechaPreparacion xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd"">20200109</FechaPreparacion>
      <HoraPreparacion xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd"">161323</HoraPreparacion>
    </{XMLTestFileConstants.XmlElementNamespace}SegmentosDeServicio>
    <{XMLTestFileConstants.XmlElementNamespace}MRN_Operacion>20ES00999830001277</{XMLTestFileConstants.XmlElementNamespace}MRN_Operacion>
    <{XMLTestFileConstants.XmlElementNamespace}IndicadorDatosATC>S</{XMLTestFileConstants.XmlElementNamespace}IndicadorDatosATC>
  </{XMLTestFileConstants.XmlElementNamespace}Mensaje>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaDVDH2V1Ent>
  </soapenv:Body>
</soapenv:Envelope>"),
};

		ZString[] ExpectedImportQueryMessagesText => new ZString[]
		{
			ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ConsultaImportacionV2Ent.xsd"">
  <SegmentosDeServicio Id=""ES2001091613234560"" fecha=""20200109"" hora=""161323"" />
  <NumeroDeReferencia>20ES00999830001277</NumeroDeReferencia>
  <DatosEnATC>S</DatosEnATC>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent>
  </soapenv:Body>
</soapenv:Envelope>"),
			ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ConsultaImportacionV2Ent.xsd"">
  <SegmentosDeServicio Id=""ES2001091613234560"" fecha=""20200109"" hora=""161323"" />
  <NumeroDeReferencia>20ES00999830001277</NumeroDeReferencia>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent>
  </soapenv:Body>
</soapenv:Envelope>"),
		};

		ZString[] ExpectedQueryImportH1MessagesText => new ZString[]
		{
			ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adip/jdit/ws/cci/ConsultaImportacionV2Ent.xsd"">
  <Message>
    <messageIdentification>ES2001091613234560</messageIdentification>
    <preparationDateAndTime>2020-01-09T16:13:23</preparationDateAndTime>
  </Message>
  <ConsultaCompleta>
    <MRN>20ES00999830001277</MRN>
  </ConsultaCompleta>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent>
  </soapenv:Body>
</soapenv:Envelope>"),
			ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adip/jdit/ws/cci/ConsultaImportacionV2Ent.xsd"">
  <Message>
    <messageIdentification>ES2001091613234560</messageIdentification>
    <preparationDateAndTime>2020-01-09T16:13:23</preparationDateAndTime>
  </Message>
  <ConsultaCompleta>
    <MRN>20ES00999830001277</MRN>
    <ATC>S</ATC>
  </ConsultaCompleta>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent>
  </soapenv:Body>
</soapenv:Envelope>"),
		};

		const string ExpectedStringDUAExport = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}CC515CV1Ent Id=""ES2001091613234560"" xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515CV1Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}CC515C>
    <{XMLTestFileConstants.XmlElementNamespace}messageSender>ESDEC33333333</{XMLTestFileConstants.XmlElementNamespace}messageSender>
    <{XMLTestFileConstants.XmlElementNamespace}messageRecipient>NECA.ES</{XMLTestFileConstants.XmlElementNamespace}messageRecipient>
    <{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>2020-01-09T16:13:23</{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>
    <{XMLTestFileConstants.XmlElementNamespace}messageIdentification>ES000001</{XMLTestFileConstants.XmlElementNamespace}messageIdentification>
    <{XMLTestFileConstants.XmlElementNamespace}messageType>CC515C</{XMLTestFileConstants.XmlElementNamespace}messageType>
    <{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
      <{XMLTestFileConstants.XmlElementNamespace}LRN>ES000001</{XMLTestFileConstants.XmlElementNamespace}LRN>
      <{XMLTestFileConstants.XmlElementNamespace}declarationType>EX</{XMLTestFileConstants.XmlElementNamespace}declarationType>
      <{XMLTestFileConstants.XmlElementNamespace}additionalDeclarationType>A</{XMLTestFileConstants.XmlElementNamespace}additionalDeclarationType>
      <{XMLTestFileConstants.XmlElementNamespace}security>0</{XMLTestFileConstants.XmlElementNamespace}security>
      <{XMLTestFileConstants.XmlElementNamespace}totalAmountInvoiced>0.00</{XMLTestFileConstants.XmlElementNamespace}totalAmountInvoiced>
    </{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
    <{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExport>
      <{XMLTestFileConstants.XmlElementNamespace}referenceNumber>ES009999</{XMLTestFileConstants.XmlElementNamespace}referenceNumber>
    </{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExport>
    <{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExitDeclared>
      <{XMLTestFileConstants.XmlElementNamespace}referenceNumber>ES009999</{XMLTestFileConstants.XmlElementNamespace}referenceNumber>
    </{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExitDeclared>
    <{XMLTestFileConstants.XmlElementNamespace}Exporter>
      <{XMLTestFileConstants.XmlElementNamespace}identificationNumber>SUP22222222</{XMLTestFileConstants.XmlElementNamespace}identificationNumber>
    </{XMLTestFileConstants.XmlElementNamespace}Exporter>
    <{XMLTestFileConstants.XmlElementNamespace}Declarant>
      <{XMLTestFileConstants.XmlElementNamespace}identificationNumber>ESDEC33333333</{XMLTestFileConstants.XmlElementNamespace}identificationNumber>
      <{XMLTestFileConstants.XmlElementNamespace}ContactPerson>
        <{XMLTestFileConstants.XmlElementNamespace}name>Declarant Test Org</{XMLTestFileConstants.XmlElementNamespace}name>
        <{XMLTestFileConstants.XmlElementNamespace}eMailAddress>Default@edi.com.au</{XMLTestFileConstants.XmlElementNamespace}eMailAddress>
      </{XMLTestFileConstants.XmlElementNamespace}ContactPerson>
    </{XMLTestFileConstants.XmlElementNamespace}Declarant>
    <{XMLTestFileConstants.XmlElementNamespace}GoodsShipment>
      <{XMLTestFileConstants.XmlElementNamespace}DeliveryTerms>
        <{XMLTestFileConstants.XmlElementNamespace}incotermCode>FOB</{XMLTestFileConstants.XmlElementNamespace}incotermCode>
      </{XMLTestFileConstants.XmlElementNamespace}DeliveryTerms>
      <{XMLTestFileConstants.XmlElementNamespace}Consignment>
        <{XMLTestFileConstants.XmlElementNamespace}containerIndicator>0</{XMLTestFileConstants.XmlElementNamespace}containerIndicator>
        <{XMLTestFileConstants.XmlElementNamespace}grossMass>0</{XMLTestFileConstants.XmlElementNamespace}grossMass>
        <{XMLTestFileConstants.XmlElementNamespace}Consignee>
          <{XMLTestFileConstants.XmlElementNamespace}identificationNumber>ESIMP11111111</{XMLTestFileConstants.XmlElementNamespace}identificationNumber>
        </{XMLTestFileConstants.XmlElementNamespace}Consignee>
      </{XMLTestFileConstants.XmlElementNamespace}Consignment>
      <{XMLTestFileConstants.XmlElementNamespace}GoodsItem>
        <{XMLTestFileConstants.XmlElementNamespace}declarationGoodsItemNumber>1</{XMLTestFileConstants.XmlElementNamespace}declarationGoodsItemNumber>
        <{XMLTestFileConstants.XmlElementNamespace}statisticalValue>0.00</{XMLTestFileConstants.XmlElementNamespace}statisticalValue>
        <{XMLTestFileConstants.XmlElementNamespace}Procedure>
          <{XMLTestFileConstants.XmlElementNamespace}requestedProcedure>12</{XMLTestFileConstants.XmlElementNamespace}requestedProcedure>
          <{XMLTestFileConstants.XmlElementNamespace}previousProcedure>34</{XMLTestFileConstants.XmlElementNamespace}previousProcedure>
          <{XMLTestFileConstants.XmlElementNamespace}AdditionalProcedure>
            <{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>1</{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>
            <{XMLTestFileConstants.XmlElementNamespace}additionalProcedure>001</{XMLTestFileConstants.XmlElementNamespace}additionalProcedure>
          </{XMLTestFileConstants.XmlElementNamespace}AdditionalProcedure>
        </{XMLTestFileConstants.XmlElementNamespace}Procedure>
        <{XMLTestFileConstants.XmlElementNamespace}Commodity>
          <{XMLTestFileConstants.XmlElementNamespace}descriptionOfGoods>Description1</{XMLTestFileConstants.XmlElementNamespace}descriptionOfGoods>
          <{XMLTestFileConstants.XmlElementNamespace}CommodityCode>
            <{XMLTestFileConstants.XmlElementNamespace}harmonizedSystemSubHeadingCode>220300</{XMLTestFileConstants.XmlElementNamespace}harmonizedSystemSubHeadingCode>
            <{XMLTestFileConstants.XmlElementNamespace}combinedNomenclatureCode>10</{XMLTestFileConstants.XmlElementNamespace}combinedNomenclatureCode>
            <{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
              <{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>1</{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>
              <{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>First</{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>
            </{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
            <{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
              <{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>2</{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>
              <{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>Second</{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>
            </{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
          </{XMLTestFileConstants.XmlElementNamespace}CommodityCode>
          <{XMLTestFileConstants.XmlElementNamespace}GoodsMeasure>
            <{XMLTestFileConstants.XmlElementNamespace}grossMass>0</{XMLTestFileConstants.XmlElementNamespace}grossMass>
            <{XMLTestFileConstants.XmlElementNamespace}netMass>0</{XMLTestFileConstants.XmlElementNamespace}netMass>
          </{XMLTestFileConstants.XmlElementNamespace}GoodsMeasure>
        </{XMLTestFileConstants.XmlElementNamespace}Commodity>
      </{XMLTestFileConstants.XmlElementNamespace}GoodsItem>
    </{XMLTestFileConstants.XmlElementNamespace}GoodsShipment>
  </{XMLTestFileConstants.XmlElementNamespace}CC515C>
</{XMLTestFileConstants.XmlElementNamespace}CC515CV1Ent>
  </soapenv:Body>
</soapenv:Envelope>";

		const string ExpectedStringDUAExportAmendment = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}CC513CV1Ent Id=""ES2001091613234560"" xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC513CV1Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}CC513C>
    <{XMLTestFileConstants.XmlElementNamespace}messageSender>ESDEC33333333</{XMLTestFileConstants.XmlElementNamespace}messageSender>
    <{XMLTestFileConstants.XmlElementNamespace}messageRecipient>NECA.ES</{XMLTestFileConstants.XmlElementNamespace}messageRecipient>
    <{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>2020-01-09T16:13:23</{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>
    <{XMLTestFileConstants.XmlElementNamespace}messageIdentification>ES000002</{XMLTestFileConstants.XmlElementNamespace}messageIdentification>
    <{XMLTestFileConstants.XmlElementNamespace}messageType>CC513C</{XMLTestFileConstants.XmlElementNamespace}messageType>
    <{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
      <{XMLTestFileConstants.XmlElementNamespace}LRN>ES000002</{XMLTestFileConstants.XmlElementNamespace}LRN>
      <{XMLTestFileConstants.XmlElementNamespace}declarationType>EX</{XMLTestFileConstants.XmlElementNamespace}declarationType>
      <{XMLTestFileConstants.XmlElementNamespace}additionalDeclarationType>B</{XMLTestFileConstants.XmlElementNamespace}additionalDeclarationType>
      <{XMLTestFileConstants.XmlElementNamespace}security>0</{XMLTestFileConstants.XmlElementNamespace}security>
      <{XMLTestFileConstants.XmlElementNamespace}totalAmountInvoiced>0.00</{XMLTestFileConstants.XmlElementNamespace}totalAmountInvoiced>
    </{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
    <{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExport>
      <{XMLTestFileConstants.XmlElementNamespace}referenceNumber>ES009999</{XMLTestFileConstants.XmlElementNamespace}referenceNumber>
    </{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExport>
    <{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExitDeclared>
      <{XMLTestFileConstants.XmlElementNamespace}referenceNumber>ES009999</{XMLTestFileConstants.XmlElementNamespace}referenceNumber>
    </{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExitDeclared>
    <{XMLTestFileConstants.XmlElementNamespace}Exporter>
      <{XMLTestFileConstants.XmlElementNamespace}identificationNumber>SUP22222222</{XMLTestFileConstants.XmlElementNamespace}identificationNumber>
    </{XMLTestFileConstants.XmlElementNamespace}Exporter>
    <{XMLTestFileConstants.XmlElementNamespace}Declarant>
      <{XMLTestFileConstants.XmlElementNamespace}identificationNumber>ESDEC33333333</{XMLTestFileConstants.XmlElementNamespace}identificationNumber>
      <{XMLTestFileConstants.XmlElementNamespace}ContactPerson>
        <{XMLTestFileConstants.XmlElementNamespace}name>Declarant Test Org</{XMLTestFileConstants.XmlElementNamespace}name>
        <{XMLTestFileConstants.XmlElementNamespace}eMailAddress>Default@edi.com.au</{XMLTestFileConstants.XmlElementNamespace}eMailAddress>
      </{XMLTestFileConstants.XmlElementNamespace}ContactPerson>
    </{XMLTestFileConstants.XmlElementNamespace}Declarant>
    <{XMLTestFileConstants.XmlElementNamespace}GoodsShipment>
      <{XMLTestFileConstants.XmlElementNamespace}Consignment>
        <{XMLTestFileConstants.XmlElementNamespace}containerIndicator>0</{XMLTestFileConstants.XmlElementNamespace}containerIndicator>
        <{XMLTestFileConstants.XmlElementNamespace}grossMass>0</{XMLTestFileConstants.XmlElementNamespace}grossMass>
        <{XMLTestFileConstants.XmlElementNamespace}Consignee>
          <{XMLTestFileConstants.XmlElementNamespace}identificationNumber>ESIMP11111111</{XMLTestFileConstants.XmlElementNamespace}identificationNumber>
        </{XMLTestFileConstants.XmlElementNamespace}Consignee>
      </{XMLTestFileConstants.XmlElementNamespace}Consignment>
      <{XMLTestFileConstants.XmlElementNamespace}GoodsItem>
        <{XMLTestFileConstants.XmlElementNamespace}declarationGoodsItemNumber>1</{XMLTestFileConstants.XmlElementNamespace}declarationGoodsItemNumber>
        <{XMLTestFileConstants.XmlElementNamespace}statisticalValue>0.00</{XMLTestFileConstants.XmlElementNamespace}statisticalValue>
        <{XMLTestFileConstants.XmlElementNamespace}Procedure>
          <{XMLTestFileConstants.XmlElementNamespace}requestedProcedure>12</{XMLTestFileConstants.XmlElementNamespace}requestedProcedure>
          <{XMLTestFileConstants.XmlElementNamespace}previousProcedure>34</{XMLTestFileConstants.XmlElementNamespace}previousProcedure>
          <{XMLTestFileConstants.XmlElementNamespace}AdditionalProcedure>
            <{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>1</{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>
            <{XMLTestFileConstants.XmlElementNamespace}additionalProcedure>001</{XMLTestFileConstants.XmlElementNamespace}additionalProcedure>
          </{XMLTestFileConstants.XmlElementNamespace}AdditionalProcedure>
        </{XMLTestFileConstants.XmlElementNamespace}Procedure>
        <{XMLTestFileConstants.XmlElementNamespace}Commodity>
          <{XMLTestFileConstants.XmlElementNamespace}descriptionOfGoods>Description1</{XMLTestFileConstants.XmlElementNamespace}descriptionOfGoods>
          <{XMLTestFileConstants.XmlElementNamespace}CommodityCode>
            <{XMLTestFileConstants.XmlElementNamespace}harmonizedSystemSubHeadingCode>220300</{XMLTestFileConstants.XmlElementNamespace}harmonizedSystemSubHeadingCode>
            <{XMLTestFileConstants.XmlElementNamespace}combinedNomenclatureCode>10</{XMLTestFileConstants.XmlElementNamespace}combinedNomenclatureCode>
            <{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
              <{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>1</{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>
              <{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>First</{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>
            </{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
            <{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
              <{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>2</{XMLTestFileConstants.XmlElementNamespace}sequenceNumber>
              <{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>Second</{XMLTestFileConstants.XmlElementNamespace}taricAdditionalCode>
            </{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
          </{XMLTestFileConstants.XmlElementNamespace}CommodityCode>
          <{XMLTestFileConstants.XmlElementNamespace}GoodsMeasure>
            <{XMLTestFileConstants.XmlElementNamespace}grossMass>0</{XMLTestFileConstants.XmlElementNamespace}grossMass>
            <{XMLTestFileConstants.XmlElementNamespace}netMass>0</{XMLTestFileConstants.XmlElementNamespace}netMass>
          </{XMLTestFileConstants.XmlElementNamespace}GoodsMeasure>
        </{XMLTestFileConstants.XmlElementNamespace}Commodity>
      </{XMLTestFileConstants.XmlElementNamespace}GoodsItem>
    </{XMLTestFileConstants.XmlElementNamespace}GoodsShipment>
  </{XMLTestFileConstants.XmlElementNamespace}CC513C>
</{XMLTestFileConstants.XmlElementNamespace}CC513CV1Ent>
  </soapenv:Body>
</soapenv:Envelope>";

		void AddFees(JobDeclaration declaration, ZDecimal expectedTotalAmountEntry1, ZDecimal expectedTotalAmountEntry2)
		{
			var entryHeader1 = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entryHeader1.MergedLines[0];
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeAmount = expectedTotalAmountEntry1 / 2;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeAmount = expectedTotalAmountEntry1 / 2;

			var entryHeader2 = declaration.CustomsEntryHeaders[1];
			var entryLine2 = entryHeader2.MergedLines[0];
			var fee3 = entryLine2.Fees.AddNew();
			fee3.CF_ChargeAmount = expectedTotalAmountEntry2 / 4;
			var fee4 = entryLine2.Fees.AddNew();
			fee4.CF_ChargeAmount = expectedTotalAmountEntry2 / 4;
			var fee5 = entryLine2.Fees.AddNew();
			fee5.CF_ChargeAmount = expectedTotalAmountEntry2 / 4;
			var fee6 = entryLine2.Fees.AddNew();
			fee6.CF_ChargeAmount = expectedTotalAmountEntry2 / 4;

			Factory.Save();
		}

		JobDeclaration CreateEmptyDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.ZG_IsTrainingDeclaration = true;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert("Merge done", mergeResult);
			Factory.Save();

			return declaration;
		}

		JobDeclaration CreateT2LAnnexDeclaration(ZBool addSecondAndThirdAnnexDocs, string filename = "Invoice.pdf")
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.JE_CustomsProfile = "TestCert1";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], filename, "CIV");
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			if (addSecondAndThirdAnnexDocs)
			{
				var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.txt", "MSC");
				var pivot2 = entryHeader.EDocPivotCollection.AddNew();
				pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;

				var eDoc3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice3.txt", "MSC");
				var pivot3 = entryHeader.EDocPivotCollection.AddNew();
				pivot3.CSD_StorageDocReference = eDoc3.UniqueKey;
			}

			Factory.Save();
			declaration.DocManagerInfo.Save();

			return declaration;
		}

		JobDeclaration CreateAESAnnexDeclaration(ZBool addMultipleAnnexDocs, string filename = "Invoice.pdf")
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.JE_CustomsProfile = "TestCert1";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			entryHeader.MovementReferenceNumber = "MRN-TEST";
			entryHeader.ZG_UCC6Version = 1;

			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], filename, "CIV");
			var pivot1 = entryHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			if (addMultipleAnnexDocs)
			{
				for (int i = 0; i <= 12; i++)
				{
					var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice" + i + ".txt", "MSC");
					var pivot = entryHeader.EDocPivotCollection.AddNew();
					pivot.CSD_StorageDocReference = eDoc.UniqueKey;
				}
			}

			Factory.Save();
			declaration.DocManagerInfo.Save();

			return declaration;
		}

		JobDeclaration CreateT2LPOUSDeclarationForAnnexes(ZBool addSecondAndThirdAnnexDocs, string filename = "Invoice.pdf", int pousVersion = 1)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.JE_CustomsProfile = "TestCert1";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			entryHeader.MovementReferenceNumber = "MRN-TEST";
			entryHeader.ZG_POUSVersion = pousVersion;

			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], filename, "CIV");
			var pivot1 = entryHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			if (addSecondAndThirdAnnexDocs)
			{
				var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.txt", "MSC");
				var pivot2 = entryHeader.EDocPivotCollection.AddNew();
				pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;

				var eDoc3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice3.txt", "MSC");
				var pivot3 = entryHeader.EDocPivotCollection.AddNew();
				pivot3.CSD_StorageDocReference = eDoc3.UniqueKey;
			}

			Factory.Save();
			declaration.DocManagerInfo.Save();

			return declaration;
		}

		public OrgHeader Declarant
		{
			get
			{
				if (declarant == null)
				{
					declarant = Factory.GetNewDeclarant();
				}
				return declarant;
			}
		}
		OrgHeader declarant;

		public OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.GetNewImporter();
				}
				return importer;
			}
		}
		OrgHeader importer;

		public OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = Factory.GetNewSupplier();
				}
				return supplier;
			}
		}
		OrgHeader supplier;
	}
}
