using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	abstract class CusEntryHeaderOriginalMessageSenderTest<T> : TestCaseWithFactory
		where T : MessageSender
	{
		public void TestSendMessage()
		{
			MessageSender.Send();

			foreach (CusEntryHeader parent in Parents)
			{
				AssertEquals(1, parent.Messages.Count);
				var message = parent.Messages[0];
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.KRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				Assert("Message content has been set to EM_MessageData", !message.EM_MessageData.IsEmpty);
				AssertEquals(parent.PK, message.EM_LinkedObject.PK);
			}
		}

		public void TestSendException()
		{
			IsExceptionTest = ZBool.True;

			AssertExceptionThrown<Exception>(() => MessageSender.Send());
			foreach (CusEntryHeader parent in Parents)
			{
				AssertEquals(0, parent.Messages.Where(x => x.EM_MessageType == MessageSender.MessageType).Count());
			}
		}
		public ZBool IsExceptionTest;

		IEnumerable<CusEntryHeader> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<CusEntryHeader> parents;

		protected MessageSender MessageSender => messageSender ?? (messageSender = GetMessageSender());
		MessageSender messageSender;

		protected abstract MessageSender GetMessageSender();
		protected abstract IEnumerable<CusEntryHeader> GetMessageParents();

		protected override void TearDown()
		{
			((IDisposable)MessageSender).Dispose();
		}

		public virtual void TestSnapshotIsCreated()
		{
			foreach (var entry in Parents)
			{
				var snapshot = entry.Snapshots.GetLatestSnapshotIn(MessageSender.MessageType, Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current);
				AssertNull("Pre-Condition: no current snapshot exists", snapshot);
			}
			MessageSender.Send();
			foreach (var entry in Parents)
			{
				var snapshot = entry.Snapshots.GetLatestSnapshotIn(MessageSender.MessageType, Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current);
				if (ElectronicDocumentTypeList.SupportsAmendment(MessageSender.MessageType) || MessageSender.MessageType == ElectronicDocumentTypeList.Codes._5UL)
				{
					AssertNotNull("If this test fails, please see the individual message sender. OnSent, system should create a snapshot with a data provider used to send a message", snapshot);
				}
				else
				{
					AssertNull("Snapshot has not been created", snapshot);
				}
			}
		}

		public virtual void TestStatusIsUpdated()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, GetStatusField(entry));
			}
		}

		protected virtual ZString GetStatusField(CusEntryHeader entry) => entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == MessageSender.MessageType).CE_EntryStatus;
	}
}
