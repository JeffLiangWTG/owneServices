using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	abstract class CusEntryHeaderAmendmentMessageSenderTest<T> : TestCaseWithFactory
		where T : MessageSender
	{
		public virtual void TestSendMessage()
		{
			foreach (JobDeclarationAmendmentMessageSendingObject parent in Parents)
			{
				AssertEquals("Pre-Condition: No message exists for the sending message type", false, parent.Header.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == MessageType));
			}

			MessageSender.Send();

			foreach (JobDeclarationAmendmentMessageSendingObject parent in Parents)
			{
				var entry = parent.Header;
				var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
				AssertEquals(1, messages.Count());
				var message = messages.FirstOrDefault();
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.KRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				var entryType = ZString.Empty;
				if (OriginalMessageType == "830")
				{
					entryType = "EXP";
				}
				else
				{
					entryType = "IMP";
				}
				AssertEquals((entry.CH_VersionID + 1).ToString(), message.EM_ApplicationReference);
				Assert("Message content has been set to EM_MessageData", !message.EM_MessageData.IsEmpty);
				AssertEquals(entry.PK, message.EM_LinkedObject.PK);
			}
		}
		public void TestSendException()
		{
			IsExceptionTest = ZBool.True;
			var messages = Parents.Single().Header.Messages;
			AssertEquals("One message exits before sending a message.", 1, messages.Count);
			AssertExceptionThrown<Exception>(() => MessageSender.Send());
			AssertEquals("Exception occurred when sending a message, and no new message has been created.", 1, messages.Count);
		}
		public ZBool IsExceptionTest;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationAmendmentMessageSendingObject> parents;

		protected MessageSender MessageSender => messageSender ?? (messageSender = GetMessageSender());
		MessageSender messageSender;

		protected abstract MessageSender GetMessageSender();
		protected abstract IEnumerable<JobDeclarationAmendmentMessageSendingObject> GetMessageParents();
		protected abstract ZString MessageType { get; }

		ZString OriginalMessageType => ElectronicDocumentTypeList.GetOriginalType(MessageType);

		protected override void TearDown()
		{
			((IDisposable)MessageSender).Dispose();
		}

		public virtual void TestSnapshotIsCreated()
		{
			foreach (var parent in Parents)
			{
				var snapshot = parent.Header.Snapshots.GetLatestSnapshotIn(OriginalMessageType, Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current);
				AssertNull("Pre-Condition: no current snapshot exists", snapshot);
			}

			MessageSender.Send();
			foreach (var parent in Parents)
			{
				var snapshot = parent.Header.Snapshots.GetLatestSnapshotIn(OriginalMessageType, Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current);
				AssertNotNull("If this test fails, please see the individual message sender. OnSent, system should create a snapshot with a data provider used to send a message", snapshot);
			}
		}

		public virtual void TestStatusIsUpdated()
		{
			MessageSender.Send();
			foreach (JobDeclarationAmendmentMessageSendingObject parent in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentSent, GetStatusField(parent.Header));
			}
		}
		protected virtual ZString GetStatusField(CusEntryHeader entry) => entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == MessageSender.MessageType).CE_EntryStatus;
	}
}
