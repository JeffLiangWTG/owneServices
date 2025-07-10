using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.TemporaryStorage.Business.G5MessageSender;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

public class G5MessageSenderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new G5MessageSender(null));
	}

	public void TestGetMessageBuildersData()
	{
		CombineAssertions(() =>
		{
			var messageSendingObject = new G5MessageSendingObject(header, staff);
			messageSendingObject.Factory.RefreshEnabled = false;
			var messageSendingObjectParent = new G5TemporaryStorageMessageSendingObjectParent(messageSendingObject);
			var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = nonExistentMessageType;
			var sender = new G5MessageSender(messageSendingObjectParent);
			var messageBuildersData = sender.GetMessageBuildersData();
			AssertEquals("The returned messagebuildersdata list has 1 element", 1, messageBuildersData.Count);
			AssertEquals("MessageBuilders failure is true when message type is not correct", true, messageBuildersData.FirstOrDefault().FailureFlag);
			AssertEquals("LastKeyReported has exception in the builder", "G5MessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			messageSendingObject = new G5MessageSendingObject(header, staff);
			messageSendingObject.Factory.RefreshEnabled = false;
			messageSendingObjectParent = new G5TemporaryStorageMessageSendingObjectParent(messageSendingObject);
			sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.G5v1Expedition;
			sender = new G5MessageSender(messageSendingObjectParent);
			messageBuildersData = sender.GetMessageBuildersData();
			AssertEquals("MessageBuilders count is 1", 1, messageBuildersData.Count);
			AssertEquals("MessaegBuilder Type", "G5X", messageBuildersData.FirstOrDefault().MessageBuilder.MessageType);
			AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		});
	}

	public void TestSendG5V1CancellationMessage()
	{
		CombineAssertions(() =>
		{
			SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

			SendMessageWithNullMessageBuilderAndAssertErrorInSend();

			var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation);
			SendMessageAndAssertCorrectResult(sender);
		});
	}

	public void TestSendG5V1ExpeditionMessage()
	{
		CombineAssertions(() =>
		{
			SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

			SendMessageWithNullMessageBuilderAndAssertErrorInSend();

			var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.G5v1Expedition);
			SendMessageAndAssertCorrectResult(sender);
		});
	}

	public void TestSendG5V1ExpAmendmentMessage()
	{
		CombineAssertions(() =>
		{
			SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

			SendMessageWithNullMessageBuilderAndAssertErrorInSend();

			var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment);
			SendMessageAndAssertCorrectResult(sender);
		});
	}

	public void TestSendG5V1ReceptionMessage()
	{
		CombineAssertions(() =>
		{
			SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

			SendMessageWithNullMessageBuilderAndAssertErrorInSend();

			var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.G5v1Reception);
			SendMessageAndAssertCorrectResult(sender);
		});
	}

	void SendMessageWithNonExistentMessageTypeAndAssertErrorInSender()
	{
		var emptyHeader = Factory.New<TemporaryStorageHeader>();
		var messageSendingObject = new G5MessageSendingObject(emptyHeader, staff);
		messageSendingObject.Factory.RefreshEnabled = false;
		var messageSendingObjectParent = new G5TemporaryStorageMessageSendingObjectParent(messageSendingObject);
		var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
		sendingObject.ShouldSend = true;
		sendingObject.MessageType = nonExistentMessageType;
		var sender = new G5MessageSender(messageSendingObjectParent);
		var messageBuilders = sender.GetMessageBuildersData();
		var result = G5MessageSender.Send(messageBuilders);
		AssertEquals("The message was created but could not be sent", 1, result.MessagesWithSendFailure);
		AssertEquals("LastKeyReported has exception in the builder", "G5MessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();
	}

	void SendMessageWithNullMessageBuilderAndAssertErrorInSend()
	{
		var messageBuilders = new List<MessageBuilderData> { new MessageBuilderData { MessageBuilder = null } };
		var result = G5MessageSender.Send(messageBuilders);
		AssertEquals("The message was created but could not be sent", 1, result.MessagesWithSendFailure);
		AssertEquals("Header doesn't have messages", false, header.Messages.Any());
		AssertEquals("LastKeyReported has exception in the sender", "G5MessageSender.Send", ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();
	}

	void SendMessageAndAssertCorrectResult(G5MessageSender sender, string messageStatus = PNTSMessageStatusList.Codes.Sent)
	{
		var messageBuilders = sender.GetMessageBuildersData();
		var result = G5MessageSender.Send(messageBuilders);
		Factory.Save();
		AssertEquals("The message has been created and sent", 1, result.MessagesSent);
		AssertEquals("Header message status has changed, BM_MessageStatus", messageStatus, header.AMA_MessageStatus);
		AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();
		header.Messages.Reload(true);
		var msg = header.Messages.LastOutgoingMessage;
		AssertNotNull("EDIMessage was created for the header", msg);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
		staff = staffWithCertificateHelperTest.Staff;
		certificate = staffWithCertificateHelperTest.Certificate;
		header = Factory.New<TemporaryStorageHeader>();
		header.AMA_GS_NKCustomsAgent = staff.GS_Code;
		header.AMA_CustomsProfile = certificate.CertificateName;

		Factory.Save();
	}
	TemporaryStorageHeader header;
	CertificateProviderTestClass certificate;
	GlbStaff staff;

	readonly ZString nonExistentMessageType = "AAA";

	G5MessageSender GetMessageSenderForSpecificMessageType(ZString messageType, TemporaryStorageHeader tempHeader = null)
	{
		var messageSendingObject = new G5MessageSendingObject(header ?? tempHeader, staff);
		messageSendingObject.Factory.RefreshEnabled = false;
		var messageSendingObjectParent = new G5TemporaryStorageMessageSendingObjectParent(messageSendingObject);
		var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
		sendingObject.ShouldSend = true;
		sendingObject.MessageType = messageType;
		return new G5MessageSender(messageSendingObjectParent);
	}
}
