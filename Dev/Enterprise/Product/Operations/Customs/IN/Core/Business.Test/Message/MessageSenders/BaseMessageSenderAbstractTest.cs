using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IN.Business.Testing;

public abstract class BaseMessageSenderAbstractTest<T, U> : TestCaseWithFactory
	where T : BaseMessageSender<U>
	where U : BaseMessageSendingObject, IMessageSendingObject
{
	public void TestSendInEmailContext()
	{
		foreach (var messageType in MessageTyeList)
		{
			CombineAssertions($"Message Type = {messageType}", () =>
			{
				SetupAndAssertMessageSendingInContext(MessageSendingContext.EMAIL, messageType, message =>
				{
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);

					if (message.EM_LinkedObject is IMessageAttachee messageAttachee)
					{
						AssertEquals("Message Status", MessageStatusList.Codes.MessageQueued, messageAttachee.MessageStatus);
						AssertEquals("Customs Status", GetExpectedCustomsStatus(messageType), messageAttachee.CustomsStatus);
					}
				});
			});
		}
	}

	public void TestSendInDownloadContext()
	{
		SetupAndAssertMessageSendingInContext(MessageSendingContext.DOWNLOAD, MessageTyeList.FirstOrDefault(), message =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("EM_ApplicationReference", Constants.Messaging.MessageDownloaded, message.EM_ApplicationReference);

			if (message.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				AssertEquals("Message Status", ZString.Empty, messageAttachee.MessageStatus);
				AssertEquals("Customs Status", ZString.Empty, messageAttachee.CustomsStatus);
			}
		});
	}

	public void TestSend_WithoutCerificatePassword()
	{
		var (_, sender) = CreateMessageSender(MessageTyeList.FirstOrDefault());

		using (Globals.TemporaryOverrideForIsTest(false))
		{
			GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
			AssertExceptionThrown<DeveloperNotificationException>("CertificatePassword missing for current user", () => SendAndSaveMessage(sender, MessageSendingContext.EMAIL));
		}
	}

	public void TestSend_WithoutLoginPassword()
	{
		var (_, sender) = CreateMessageSender(MessageTyeList.FirstOrDefault());

		using (Globals.TemporaryOverrideForIsTest(false))
		{
			GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
			AssertExceptionThrown<DeveloperNotificationException>("LoginPassword missing for current user", () => sender.Send(MessageSendingContext.EMAIL));
		}
	}

	public void TestSendAndSaveMessages_ExceptionWhileSavingMessage()
	{
		var staffWrapper = GlbStaffWrapper.GetWrapperForCurrentUser();
		var certificate = staffWrapper.CertificatePassword;
		var loginPassword = staffWrapper.LoginPassword;

		var (sendingObject, sender) = CreateMessageSender(MessageTyeList.FirstOrDefault());
		var messageAttachee = sendingObject.MessageAttachee;
		Factory.Save();
		var message = sender.Send(MessageSendingContext.EMAIL);
		message.Saving += _ => throw new Exception("Saving failed");
		CombineAssertions(() =>
		{
			AssertExceptionThrown<Exception>("Saving failed", () => Factory.Save());
			AssertEquals("Message Status", ZString.Empty, messageAttachee.MessageStatus);
			AssertEquals("Customs Status", ZString.Empty, messageAttachee.CustomsStatus);
			AssertEquals("No EDIMessage sent", 0, messageAttachee.Messages.Count);
			Assert("message deleted", message.IsDeleted);
		});
	}

	void SetupAndAssertMessageSendingInContext(MessageSendingContext context, string messageType, Action<EDIMessage> additionalAsserts)
	{
		var staffWrapper = GlbStaffWrapper.GetWrapperForCurrentUser();
		var certificate = staffWrapper.CertificatePassword;
		var loginPassword = staffWrapper.LoginPassword;
		GlbStaff.CurrentUser.Factory.Save();

		var (sendingObject, sender) = CreateMessageSender(messageType);
		var messageAttachee = sendingObject.MessageAttachee;

		SendAndSaveMessage(sender, context);
		AssertEquals("One EDIMessage sent", 1, messageAttachee.Messages.Count);

		var message = (EDIMessage)messageAttachee.Messages[0];
		AssertEquals("EM_LinkedObject", messageAttachee, message.EM_LinkedObject);
		AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.INCustoms, message.EM_ApplicationCode);
		AssertEquals("EM_MessageType", ExpectedMessageType, message.EM_MessageType);
		AssertEquals("EM_MessageSubType", GetExpectedMessageSubType(messageType), message.EM_MessageSubType);
		AssertEquals("EM_MessageOwner", ExpectedMessageOwner, message.EM_MessageOwner);
		AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
		AssertEquals("EM_GP", loginPassword.PK, message.EM_GP);
		additionalAsserts?.Invoke(message);

		var messageText = message.EM_MessageText;
		if (!messageText.IsEmpty)
		{
			Assert("Message Signed", messageText.StartsWith("--Signed--"));
		}

		foreach (var tag in ExpectedMessageTags)
		{
			AssertEquals($"EM_MessageText contains {tag}", expected: true, messageText.Contains(tag));
		}
	}

	void SendAndSaveMessage(T messageSender, MessageSendingContext context)
	{
		messageSender.Send(context);
		Factory.Save();
	}

	protected abstract (U sendingObject, T messageSender) CreateMessageSender(string messageType);

	protected abstract string ExpectedMessageType { get; }

	protected abstract string GetExpectedMessageSubType(string messageType);

	protected abstract string ExpectedMessageOwner { get; }

	protected abstract IReadOnlyList<string> ExpectedMessageTags { get; }

	protected abstract IReadOnlyList<string> MessageTyeList { get; }

	protected abstract string GetExpectedCustomsStatus(string messageType);
}
