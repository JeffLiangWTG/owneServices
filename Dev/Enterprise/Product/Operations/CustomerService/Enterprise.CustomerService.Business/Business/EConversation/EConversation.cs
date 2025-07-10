using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business
{
	public interface IIncidentConversation : IConversation
	{
		event EventHandler NewMessageAdded;

		void AddMessageFromLocalUser(string message, string additionalNote = "");
		void AddInternalMessageFromLocalUser(string message);
		void AddSystemLogFromLocalUser(string message, string additionalNote = "");
		void AddInternalSystemLogFromLocalUser(string message);
		void AddMessagesFromRemoteUser(Xsd.ConversationMessageCollection messages);
		void AddMessage(ZGuid id, ZDateTime sentTimeUtc, string userCode, string userName, string message, string additionalNote = "");
		void GetNewLocalPublishedMessagesSinceLastUpdate(Xsd.ConversationMessageCollection messages, ZDateTime lastUpdateTimeUtc);
		// Get local, publishable messages added since the last factory save.
		// Call this before saving the new messages or there will never be anything returned.
		void GetNewLocalPublishedMessages(Xsd.ConversationMessageCollection messages);
	}

	public class EConversation : IIncidentConversation
	{
		public EConversation(IStmNoteParent parent, int maxMessages)
			: this(parent)
		{
			this.maxMessages = maxMessages;
		}

		public EConversation(IStmNoteParent parent)
		{
			notes = new ConversationNoteCollection(parent, parent.NotesFactory);
			notes.Load();
			messages = new ConversationMessageCollection(notes);

			var parentBizo = parent as BusinessObject;
			if (parentBizo != null)
			{
				RegisterThisAsChildOf(parentBizo);
			}
		}

		readonly int maxMessages = -1;
		readonly ConversationNoteCollection notes;
		readonly ConversationMessageCollection messages;

		public IEnumerable<ConversationMessage> LocalMessages
		{
			get { return messages.Cast<ConversationMessage>().ToArray(); }
		}

		public void Reload()
		{
			var newHasChangesValue = messages.HasChanges;

			notes.Load();

			var existingMessages = new HashSet<ZGuid>();
			foreach (ConversationMessage message in messages)
			{
				existingMessages.Add(message.NotePk);
			}

			foreach (ConversationNote note in notes)
			{
				if (!existingMessages.Contains(note.PK))
				{
					newHasChangesValue |= note.HasChanges;

					messages.Add(note);
				}
			}

			messages.HasChanges = newHasChangesValue;
		}

		#region Add Message

		public event EventHandler NewMessageAdded;

		void AddMessage(ZGuid id, ZDateTime sentTimeUtc, string userCode, string userName, string body, string additionalNote, string messageType, string messageSubType)
		{
			var message = messages.AddNew();
			message.Id = id;
			message.SentTimeInUtc = sentTimeUtc;
			message.UserCode = userCode;
			message.UserName = userName;
			message.Body = body.TrimEnd('\n', '\r', ' ');
			message.AdditionalNote = additionalNote;
			message.MessageType = messageType;
			message.MessageSubType = messageSubType;
			message.SetNoteText();

			LimitMessages();

			if (NewMessageAdded != null)
			{
				NewMessageAdded(message, EventArgs.Empty);
			}
		}

		void LimitMessages()
		{
			while (maxMessages > 0 && notes.Count > maxMessages)
			{
				messages.RemoveAndDelete(messages[0]);
			}
		}

		public void AddMessage(ZGuid id, ZDateTime sentTimeUtc, string userCode, string userName, string message, string additionalNote = "")
		{
			AddMessage(id, sentTimeUtc, userCode, userName, message, additionalNote, ConversationMessage.MessageTypes.LocalPublished, ConversationMessage.MessageSubTypes.UserMessage);
		}

		public void AddMessageFromLocalUser(string message, string additionalNote = "")
		{
			AddMessage(Guid.NewGuid(), ZDateTime.UtcNow, Env.CurrentUser.Initials, Env.CurrentUser.FullName, message, additionalNote, ConversationMessage.MessageTypes.LocalPublished, ConversationMessage.MessageSubTypes.UserMessage);
		}

		public void AddInternalMessageFromLocalUser(string message)
		{
			AddMessage(Guid.NewGuid(), ZDateTime.UtcNow, Env.CurrentUser.Initials, Env.CurrentUser.FullName, message, "", ConversationMessage.MessageTypes.LocalInternal, ConversationMessage.MessageSubTypes.UserMessage);
		}

		public void AddSystemLogFromLocalUser(string message, string additionalNote = "")
		{
			AddMessage(Guid.NewGuid(), ZDateTime.UtcNow, Env.CurrentUser.Initials, Env.CurrentUser.FullName, message, additionalNote, ConversationMessage.MessageTypes.LocalPublished, ConversationMessage.MessageSubTypes.SystemLog);
		}

		public void AddInternalSystemLogFromLocalUser(string message)
		{
			AddMessage(Guid.NewGuid(), ZDateTime.UtcNow, Env.CurrentUser.Initials, Env.CurrentUser.FullName, message, "", ConversationMessage.MessageTypes.LocalInternal, ConversationMessage.MessageSubTypes.SystemLog);
		}

		public void AddMessagesFromRemoteUser(Xsd.ConversationMessageCollection messages)
		{
			var orderedMessages = messages.Cast<Xsd.ConversationMessage>().OrderBy(m => m.SentTimeInUtc);
			ZDateTime now = ZDateTime.UtcNow;

			ZDateTime lastReceivedTime = ZDateTime.MinSmallDateTimeValue;
			foreach (Xsd.ConversationMessage remoteMessage in orderedMessages)
			{
				ZDateTime sentTime = ZDateTimeConversionHelper.ForceParseRoundTripFormatStringAsUtc(remoteMessage.SentTimeInUtc);
				if (sentTime.IsEmpty)
				{
					sentTime = now;
				}

				// Set the time to now only for recent messages
				const int MaxHoursOldToUseCurrentTime = 4;
				ZDateTime receivedTime = (now - sentTime).TotalHours > MaxHoursOldToUseCurrentTime
					? sentTime
					: now;

				// Ensure order is preserved
				if (receivedTime <= lastReceivedTime)
				{
					receivedTime = lastReceivedTime.AddMilliseconds(1);
				}

				ZGuid id;
				ZGuid.TryParse(remoteMessage.Id, out id);

				AddMessage(
					id,
					receivedTime,
					remoteMessage.UserCode,
					remoteMessage.UserName,
					remoteMessage.Body.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"), // Fixup: XML deserialization of strings converts "\r\n" to "\n",
					remoteMessage.AdditionalNote,
					ConversationMessage.MessageTypes.Remote,
					remoteMessage.MessageSubType);

				lastReceivedTime = receivedTime;
			}
		}

		#endregion

		#region Psuedo businessobjects

		public void RegisterThisAsChildOf(BusinessObject parent)
		{
			parent.RegisterEditableChildObject(messages);
		}

		#endregion

		#region IConversation Members

		public bool IsEmpty
		{
			get { return messages.Count == 0; }
		}

		public bool HasChanges
		{
			get { return messages.HasChanges; }
		}

		public void GetNewLocalPublishedMessages(Xsd.ConversationMessageCollection messages)
		{
			foreach (ConversationMessage message in this.messages)
			{
				if (!message.IsInDatabase && message.IsLocalPublishedMessage)
				{
					messages.Add(new Xsd.ConversationMessage()
					{
						AdditionalNote = message.AdditionalNote,
						Id = message.Id.ToString(),
						Body = message.Body,
						SentTimeInUtc = message.SentTimeInUtc.ToString("o"),
						UserCode = message.UserCode,
						UserName = message.UserName,
						MessageSubType = message.MessageSubType
					});
				}
			}
		}

		public void GetNewLocalPublishedMessagesSinceLastUpdate(Xsd.ConversationMessageCollection messages, ZDateTime lastUpdateTimeUtc)
		{
			foreach (ConversationMessage message in this.messages)
			{
				if (message.IsInDatabase && message.SystemCreateTimeInUtc > lastUpdateTimeUtc && message.IsLocalPublishedMessage)
				{
					messages.Add(new Xsd.ConversationMessage()
					{
						AdditionalNote = message.AdditionalNote,
						Id = message.Id.ToString(),
						Body = message.Body,
						SentTimeInUtc = message.SentTimeInUtc.ToString("o"),
						UserCode = message.UserCode,
						UserName = message.UserName,
						MessageSubType = message.MessageSubType
					});
				}
			}
		}

		public bool AnyLocalMessageContains(string text)
		{
			return messages.Cast<ConversationMessage>().Any(msg => msg.IsLocalMessage && msg.Body.Contains(text));
		}

		public bool AnyNewLocalMessageContains(string text)
		{
			return messages.Cast<ConversationMessage>().Any(msg => msg.IsLocalMessage && !msg.IsInDatabase && msg.Body.Contains(text));
		}

		public IList<IConversationMessage> GetTimeOrderedMessages()
		{
			var result = new List<IConversationMessage>(messages.Count);

			foreach (var message in messages.Cast<ConversationMessage>().OrderByDescending(x => x.SentTimeInUtc))
			{
				message.CanRate = false;
				result.Add(message);
			}

			return result;
		}

		#endregion
	}
}
