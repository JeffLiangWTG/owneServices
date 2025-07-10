using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageCollection : EDIMessageCollection<EDIMessage>
	{
		public EDIMessageCollection(BusinessObject master, BusinessObjectFactory factory) : base(master, factory) { }

		public EDIMessageCollection(BusinessObject master, ZQuery additionalFilter) : base(master, additionalFilter) { }

		public EDIMessageCollection(BusinessObject master) : base(master) { }

		public EDIMessageFlattenedCollection MessagesIncludingInterchangeRejections(ZString applicationCode)
		{
			return new EDIMessageFlattenedCollection(this, new ZQuery(EDIMessageSchema.EM_MessageSubType, nameof(Core.Constants.AUCTLMessageSubType.REJ)), applicationCode);
		}
	}

	public abstract class EDIMessageCollection<T> : DependentBusinessObjectCollection<T, BusinessObject> where T : EDIMessage
	{
		protected EDIMessageCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected EDIMessageCollection(BusinessObject master)
			: this(master, master.Factory)
		{
		}

		protected EDIMessageCollection(BusinessObject master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
		}

		public int NumberOfOutgoingMessages
		{
			get
			{
				int result = 0;
				foreach (T message in this)
				{
					if (message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						result++;
					}
				}
				return result;
			}
		}

		public T FirstOutgoingMessage
		{
			get { return GetFirstMessage(ZString.Empty, ZString.Empty, EDIMessage.Direction.Transmit, ZString.Empty); }
		}

		public T LastOutgoingMessage
		{
			get { return GetLastMessage(ZString.Empty, ZString.Empty, EDIMessage.Direction.Transmit); }
		}

		public T LastOutgoingNonSystemAndNonNullUserMessage
		{
			get { return GetLastMessage(ZString.Empty, ZString.Empty, EDIMessage.Direction.Transmit, Array.Empty<ZString>(), Array.Empty<ZString>(), Array.Empty<ZString>(), Array.Empty<ZString>(), true); }
		}

		public T LastIncomingMessage
		{
			get { return GetLastMessage(ZString.Empty, ZString.Empty, EDIMessage.Direction.Receive); }
		}

		public T LastSentOutgoingMessage
		{
			get { return GetLastMessage(ZString.Empty, ZString.Empty, EDIMessage.Direction.Transmit, new ZString[] { EDIMessage.Status.Sent }); }
		}

		public T LastMessage
		{
			get { return GetLastMessage(ZString.Empty); }
		}

		public bool IsWaitingForAResponse
		{
			get
			{
				T lastMessage = this.LastMessage;
				return lastMessage != null && (lastMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && lastMessage.EM_Status != EDIMessage.Status.Rejected && lastMessage.EM_Status != EDIMessage.Status.Cancelled && !(lastMessage.AssumeMessageClearIfAcknowledgedAndNoResponse && lastMessage.HasHadItsInterchangeAcknowledged));
			}
		}

		public T GetLastMessage(ZString applicationCode)
		{
			return GetLastMessage(applicationCode, ZString.Empty, ZString.Empty);
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType)
		{
			return GetLastMessage(applicationCode, messageType, ZString.Empty);
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV)
		{
			return GetLastMessage(applicationCode, messageType, tRXorRCV, ZString.Empty);
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV, ZString status)
		{
			return GetLastMessage(applicationCode, messageType, tRXorRCV, status, ZString.Empty);
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV, ZString status, ZString messageSubType)
		{
			ZString[] messageSubTypes = messageSubType.IsEmpty ? Array.Empty<ZString>() : new ZString[] { messageSubType };
			ZString[] statusList = status.IsEmpty ? Array.Empty<ZString>() : new ZString[] { status };

			return GetLastMessage(applicationCode, messageType, tRXorRCV, statusList, messageSubTypes, Array.Empty<ZString>(), Array.Empty<ZString>());
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV, ZString status, IList<ZString> messageSubTypes)
		{
			ZString[] statusList = status.IsEmpty ? Array.Empty<ZString>() : new ZString[] { status };
			return GetLastMessage(applicationCode, messageType, tRXorRCV, statusList, messageSubTypes, Array.Empty<ZString>(), Array.Empty<ZString>());
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV, IList<ZString> statusList)
		{
			return GetLastMessage(applicationCode, messageType, tRXorRCV, statusList, Array.Empty<ZString>(), Array.Empty<ZString>(), Array.Empty<ZString>());
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV, IList<ZString> statusList, IList<ZString> messageSubTypes, IList<ZString> excludeMessagesInThisStatus, IList<ZString> excludeMessagesWithThisSubType)
		{
			return GetLastMessage(applicationCode, messageType, tRXorRCV, statusList, messageSubTypes, excludeMessagesInThisStatus, excludeMessagesWithThisSubType, false);
		}

		public T GetLastMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV, IList<ZString> statusList, IList<ZString> messageSubTypes, IList<ZString> excludeMessagesInThisStatus, IList<ZString> excludeMessagesWithThisSubType, bool excludeMessagesGeneratedBySystemOrNullUsers)
		{
			T lastMessage = null;

			foreach (T message in this)
			{
				if ((applicationCode.IsEmpty || message.EM_ApplicationCode == applicationCode)
					&& (messageType.IsEmpty || message.EM_MessageType == messageType)
					&& (tRXorRCV.IsEmpty || message.EM_ReceiveTransmit == tRXorRCV)
					&& (statusList.Count == 0 || statusList.Contains(message.EM_Status))
					&& (excludeMessagesInThisStatus.Count == 0 || !excludeMessagesInThisStatus.Contains(message.EM_Status))
					&& (excludeMessagesWithThisSubType.Count == 0 || !excludeMessagesWithThisSubType.Contains(message.EM_MessageSubType))
					&& (messageSubTypes.Count == 0 || messageSubTypes.Contains(message.EM_MessageSubType))
					&& (!excludeMessagesGeneratedBySystemOrNullUsers || (!message.UserWhoQueuedThisRecord?.GS_IsSystemAccount ?? false)))
				{
					if (lastMessage == null)
					{
						lastMessage = message;
					}
					else
					{
						if (!message.IsInDatabase || IsMessageNewer(message, lastMessage))
						{
							lastMessage = message;
						}
					}
				}
			}

			return lastMessage;
		}

		protected virtual ZBool IsMessageNewer(T messageX, T messageY)
		{
			return messageX.EM_SystemCreateTimeUtc > messageY.EM_SystemCreateTimeUtc;
		}

		public T GetFirstMessage(ZString applicationCode, ZString messageType, ZString tRXorRCV, ZString status)
		{
			T firstMessage = null;
			foreach (T message in this)
			{
				if ((applicationCode.IsEmpty || message.EM_ApplicationCode == applicationCode)
					&& (messageType.IsEmpty || message.EM_MessageType == messageType)
					&& (tRXorRCV.IsEmpty || message.EM_ReceiveTransmit == tRXorRCV)
					&& (status.IsEmpty || message.EM_Status == status))
				{
					if (firstMessage == null || message.EM_SystemCreateTimeUtc < firstMessage.EM_SystemCreateTimeUtc)
					{
						firstMessage = message;
					}
				}
			}
			return firstMessage;
		}

		public T GetLastClearReceivedMessage(ZString messageType)
		{
			T result = null;
			foreach (T message in this)
			{
				if (!message.IsTransmitMessage && message.EM_Status != EDIMessage.Status.Discarded && message.EM_MessageSubType != EDIMessage.Status.Rejected && message.EM_MessageType == messageType && (result == null || message.EM_SystemCreateTimeUtc > result.EM_SystemCreateTimeUtc))
				{
					result = message;
				}
			}
			return result;
		}

		public bool HasNonDiscardedMessage()
		{
			return Find(new ZQuery(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded)).Length > 0;
		}

		public bool IsMatchingMessages(ZString applicationCode, ZString[] messageTypes, ZString direction, bool ignoreDiscardedMessages)
		{
			return Find(GetFilter(applicationCode, messageTypes, direction, ignoreDiscardedMessages)).Any();
		}

		public T[] GetMatchingMessages(ZString applicationCode, ZString[] messageTypes, ZString direction, bool ignoreDiscardedMessages, ListSortDirection sortDirection)
		{
			return EDIMessageComparer.GetSortedMessages<T>(Find(GetFilter(applicationCode, messageTypes, direction, ignoreDiscardedMessages)), sortDirection);
		}

		public T[] GetMatchingMessages(ZString applicationCode, ZString[] messageTypes, ZString direction)
		{
			return GetMatchingMessages(applicationCode, messageTypes, direction, false, ListSortDirection.Descending);
		}

		public T GetMatchingMessage(ZString applicationCode, ZString messageType, ZString messageNum, ZString direction)
		{
			var matchingMessages = GetMatchingMessages(applicationCode, new ZString[] { messageType }, direction);
			return matchingMessages.FirstOrDefault(message => message.EM_MessageNum == messageNum);
		}

		static Func<EDIMessage, bool> GetFilter(ZString applicationCode, ZString[] messageTypes, ZString direction, bool ignoreDiscardedMessages = false)
		{
			Func<EDIMessage, bool> funcResult = ediMessage =>
				{
					var result = true;

					if (ignoreDiscardedMessages)
					{
						result &= ediMessage.EM_Status != EDIMessage.Status.Discarded;
					}
					if (!applicationCode.IsEmpty)
					{
						result &= ediMessage.EM_ApplicationCode == applicationCode;
					}
					if (messageTypes.Length > 0)
					{
						result &= messageTypes.Contains(ediMessage.EM_MessageType);
					}
					if (!direction.IsEmpty)
					{
						result &= ediMessage.EM_ReceiveTransmit == direction;
					}
					return result;
				};
			return funcResult;
		}

		public void DeleteNonPersistedMessages()
		{
			foreach (T message in this)
			{
				if (!message.IsInDatabase)
				{
					message.Delete();
				}
			}
		}

		public bool ContainsByKeyFields(T potentialMessage)
		{
			return Contains(potentialMessage.PK) ||
				   this.Cast<T>().Any(message => message.EM_EI == potentialMessage.EM_EI &&
												 message.EM_MessageNum == potentialMessage.EM_MessageNum &&
												 message.EM_MessageType == potentialMessage.EM_MessageType &&
												 message.EM_MessageSubType == potentialMessage.EM_MessageSubType &&
												 message.EM_MessageText == potentialMessage.EM_MessageText);
		}

		public T[] UpdateStatusOfPendingMessagesTo(string newStatus)
		{
			return UpdateStatusOfMatchingMessagesTo(newStatus, delegate(T msg) { return msg.IsPending; });
		}

		public T[] UpdateStatusOfAllMessagesTo(string newStatus)
		{
			return UpdateStatusOfMatchingMessagesTo(newStatus, delegate(T msg) { return true; });
		}

		public T[] UpdateStatusOfMatchingMessagesTo(string newStatus, BooleanDelegateMessageSeeker booleanDelegate)
		{
			List<T> result = new List<T>();

			foreach (T message in this)
			{
				if (booleanDelegate(message))
				{
					result.Add(message);
					message.EM_Status = newStatus;
				}
			}

			return result.ToArray();
		}

		public delegate bool BooleanDelegateMessageSeeker(T message);

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			T newMessage = child as T;
			if (newMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
			{
				newMessage.EM_SendWithMessageErrors = Master.HasMessageErrors;
			}
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			T newMessage = child as T;

			if (newMessage != null)
			{
				newMessage.EM_LinkedObject = Master;
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return EDIMessageSchema.EM_LinkUniqueID; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Test Helpers
#if DEBUG

		public void RemoveAndDeleteAllFromTest()
		{
			try
			{
				foreach (EDIMessage message in this)
				{
					message.IsDeletingInTest = true;
				}

				RemoveAndDeleteAll();
			}
			finally
			{
				foreach (EDIMessage message in this)
				{
					message.IsDeletingInTest = false;
				}
			}
		}

#endif
		#endregion

	}
}
