using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	/// <summary>
	/// Wrapped JobConversation so it is properly accessed during factory saving.
	/// The incident adds messages during saving, but these messages will not show in an ActiveBusinessObjectCollection
	/// until after the save, since list events are delayed (see ActiveBusinessObjectCollection.DelayListChangedEvents).
	/// </summary>
	public class SupportIncidentEConversation : IConversation
	{
		public SupportIncidentEConversation(SupportIncident incident)
		{
			this.Incident = incident;
		}

		readonly SupportIncident Incident;

		public JobConversation ExistingConversation => conversation ?? (conversation = LoadConversation());
		public JobConversation Conversation => conversation ?? (conversation = (LoadConversation() ?? CreateConversation(Incident)));
		JobConversation conversation;

		JobConversation LoadConversation()
		{
			JobConversation result = null;

			if (!isLoaded && !Incident.IM_INC_Request.IsEmpty && Incident.Request.IsInDatabase)
			{
				isLoaded = true;
				result = Incident.Factory.LoadTop1<JobConversation>(new ZQuery(JobConversationSchema.JCC_ParentID, Incident.IM_INC_Request));
				if ((result == null || !result.Messages.Any()) && Incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentLog.Description).Length > 0)
				{
					if (result == null)
					{
						result = CreateConversationUnsafe(Incident);
					}
					ConvertOldLogTextToEConversation(result, Incident.LogText, Incident.IM_SystemCreateTimeUtc);
				}
				if (result != null)
				{
					Incident.RegisterEditableChildObject(result);
				}
			}

			return result;
		}
		bool isLoaded;

		static JobConversation CreateConversation(SupportIncident incident)
		{
			if (incident.IM_INC_Request.IsEmpty)
			{
				throw new InvalidOperationException("Request must be created before the conversation");
			}

			JobConversation conversation = CreateConversationUnsafe(incident);
			incident.RegisterEditableChildObject(conversation);
			return conversation;
		}

		static JobConversation CreateConversationUnsafe(SupportIncident incident)
		{
			if (!incident.Request.IsInDatabase)
			{
				var conversation = incident.Factory.New<JobConversation>();
				using (conversation.SuspendSettingHasChanges())
				{
					conversation.JCC_ParentID = incident.Request.PK;
					conversation.JCC_ParentTableCode = IncidentRequestSchema.Constants.Prefix;
				}

				return conversation;
			}

			return JobConversation.GetOrCreate(incident.Request);
		}

		internal JobConversation JobConversationForTest => Conversation;

		#region IConversation

		public bool IsEmpty => ExistingConversation?.IsEmpty ?? false;
		public bool HasChanges => (ExistingConversation?.HasChanges ?? false) || newLocalMessages.Any();

		public IList<IConversationMessage> GetTimeOrderedMessages()
		{
			return ExistingConversation?.GetTimeOrderedMessages() ?? Array.Empty<IConversationMessage>();
		}

		public bool AnyLocalMessageContains(string text)
		{
			return (ExistingConversation?.AnyLocalMessageContains(text) ?? false)
				|| newLocalMessages.Any(msg => msg.Body.Contains(text, StringComparison.OrdinalIgnoreCase));
		}

		public void Reload()
		{
			if (ExistingConversation != null)
			{
				ExistingConversation.Reload();
			}
		}

		#endregion

		public event EventHandler MessageCountChanged;
		bool isPendingMessageCountChanged;

		void OnMessageCountChanged()
		{
			// Delay notifications if in a transaction.
			// The ActiveBusinessCollection will not be updated until after the save completes.
			if (!Incident.Factory.IsInTransaction)
			{
				isPendingMessageCountChanged = false;
				MessageCountChanged?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				isPendingMessageCountChanged = true;
			}
		}

		public enum LocalMessageKind
		{
			LogOrInternal,
			ForCustomer,
			Resolution
		}

		class LocalMessage
		{
			internal LocalMessage(JobConversationMessage message, LocalMessageKind kind)
			{
				Message = message;
				Kind = kind;
			}

			internal readonly JobConversationMessage Message;
			internal readonly LocalMessageKind Kind;
		}

		readonly List<JobConversationMessage> newLocalMessages = new List<JobConversationMessage>();
		readonly List<JobConversationMessage> newMessages = new List<JobConversationMessage>();
		readonly List<LocalMessage> unsentMessages = new List<LocalMessage>();

		public JobConversationMessage AddMessageFromCurrentUser(ZString comment, bool isInternal, bool isSystem, LocalMessageKind kind = LocalMessageKind.LogOrInternal, bool shouldAddMessageSentEvent = true)
		{
			return AddMessage(comment, isInternal, isSystem, false, kind, shouldAddMessageSentEvent);
		}

		public JobConversationMessage AddMessageFromSupport(ZString comment, bool isInternal, bool isSystem, LocalMessageKind kind = LocalMessageKind.LogOrInternal, bool shouldAddMessageSentEvent = true)
		{
			return AddMessage(comment, isInternal, isSystem, true, kind, shouldAddMessageSentEvent);
		}

		JobConversationMessage AddMessage(ZString comment, bool isInternal, bool isSystem, bool isSupportUser = false, LocalMessageKind kind = LocalMessageKind.LogOrInternal, bool shouldAddMessageSentEvent = true)
		{
			if (ExistingConversation != null)
			{
				var zQuery = new ZQuery();
				zQuery.AddToFilter(ExistingConversation.Participants.CompleteFilter);
				zQuery.ReLoadExistingRows = false;
				ExistingConversation.Participants.Factory.Load<JobConversationParticipant>(zQuery);
				ActiveBusinessObjectCollection.RefreshAll(typeof(JobConversationParticipant), ExistingConversation.Participants.Factory);
			}

			JobConversationMessage msg;

			if (isSupportUser)
			{
				var serviceUser = Incident.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, User.ServiceUserCode)).FirstOrDefault();

				var participant = Conversation.Participants.GetOrAdd(serviceUser);

				msg = Conversation.Messages.AddNew(participant, comment, isInternal, isSystem, false);
			}
			else
			{
				msg = Conversation.AddMessageFromCurrentUser(comment, isInternal, isSystem);
			}

			msg.JCM_IsSystem = isSystem;

			if (!msg.Sender.IsInDatabase)
			{
				msg.Sender.JCP_IsSubscribed = false;
			}

			newLocalMessages.Add(msg);
			newMessages.Add(msg);

			if (!isInternal)
			{
				unsentMessages.Add(new LocalMessage(msg, kind));
			}

			OnMessageCountChanged();

			LastAddedMessageForTest = msg;

			if (shouldAddMessageSentEvent && !isInternal && !isSystem && Incident.ManagementGroup != null)
			{
				Incident.ManagementGroup.AddIncidentMessageSentEvent(Incident);
			}

			return msg;
		}

		internal JobConversationMessage LastAddedMessageForTest;

		public void IncidentSaveSucceeded()
		{
			newLocalMessages.Clear();
			unsentMessages.Clear();
			if (isPendingMessageCountChanged)
			{
				OnMessageCountChanged();
			}
		}

		public static class LegacyMessageTypes
		{
			public const string LocalInternal = "LIN";
			public const string LocalPublished = "LOC";
			public const string Remote = "RMT";
		}

		public static class LegacyMessageSubTypes
		{
			public const string UserMessage = "USR";
			public const string SystemLog = "SYS";
		}

		public IEnumerable<JobConversationMessage> GetNewLocalPublishedMessages()
		{
			foreach (var localMessage in unsentMessages)
			{
				var msg = localMessage.Message;
				if (!msg.IsDeleted)
				{
					yield return msg;
				}
			}
		}

		public IEnumerable<JobConversationMessage> GetNewLocalMessages()
		{
			foreach (var msg in newLocalMessages)
			{
				if (!msg.IsDeleted)
				{
					yield return msg;
				}
			}
		}

		public JobConversationMessage[] GetNewMessagesAndClear()
		{
			var result = newMessages.ToArray();
			newMessages.Clear();
			return result;
		}

		public IEnumerable<JobConversationMessage> GetNewLocalPublishedCustomerMessages(bool includeResolutionMessage = true)
		{
			foreach (var localMessage in unsentMessages)
			{
				var msg = localMessage.Message;
				if (localMessage.Kind != LocalMessageKind.LogOrInternal && !msg.IsDeleted
					&& (includeResolutionMessage || localMessage.Kind != LocalMessageKind.Resolution))
				{
					yield return msg;
				}
			}
		}

		public void GetNewLocalPublishedMessages(Xsd.ConversationMessageCollection xmlMessages)
		{
			foreach (var message in GetNewLocalPublishedMessages())
			{
				xmlMessages.Add(CreateXsdConversationMessageForPublish(message));
			}
		}

		static bool IsLocalPublished(JobConversationMessage message)
		{
			return !message.JCM_IsInternal && message.JCM_IsLocal;
		}

		static Xsd.ConversationMessage CreateXsdConversationMessageForPublish(JobConversationMessage message)
		{
			var sender = message.Sender;
			var parent = sender?.Parent;

			return new Xsd.ConversationMessage()
			{
				Id = message.Id.ToString(),
				Body = message.Body,
				SentTimeInUtc = message.JCM_PostedTimeUtc.ToString("o", CultureInfo.InvariantCulture),
				UserCode = parent?.Code ?? ZString.Empty,
				UserName = parent?.Name ?? ZString.Empty,
				MessageSubType = !message.JCM_IsSystem ? LegacyMessageSubTypes.UserMessage : LegacyMessageSubTypes.SystemLog
			};
		}

		public void GetNewLocalPublishedMessagesSinceLastUpdate(Xsd.ConversationMessageCollection xmlMessages, ZDateTime lastUpdateTimeUtc)
		{
			foreach (var message in Conversation.Messages)
			{
				if (message.IsInDatabase && message.SystemCreateTimeInUtc > lastUpdateTimeUtc && IsLocalPublished(message))
				{
					xmlMessages.Add(CreateXsdConversationMessageForPublish(message));
				}
			}
		}

		public void AddMessagesFromRemoteLegacyUser(Xsd.ConversationMessageCollection messages)
		{
			var orderedMessages = messages.Cast<Xsd.ConversationMessage>().OrderBy(m => m.SentTimeInUtc);
			ZDateTime now = ZDateTime.UtcNow;

			ZDateTime lastReceivedTime = ZDateTime.MinSmallDateTimeValue;
			foreach (Xsd.ConversationMessage remoteMessage in orderedMessages)
			{
				var body = remoteMessage.Body.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"); // Fixup: XML deserialization of strings converts "\r\n" to "\n"
				body = JobConversationMessage.TrimBody(body);

				if (string.IsNullOrWhiteSpace(body))
				{
					continue;
				}

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
				if (!ZGuid.TryParse(remoteMessage.Id, out id) || id.IsEmpty)
				{
					id = ZGuid.NewZGuid();
				}

				var msg = Incident.Factory.NewWithPrimaryKey<JobConversationMessage>(id.ToGuid());
				msg.JCM_Body = body;
				msg.JCM_IsInternal = false;
				msg.JCM_PostedTimeUtc = receivedTime;
				msg.JCM_IsSystem = remoteMessage.MessageSubType == LegacyMessageSubTypes.SystemLog;
				msg.JCM_IsLocal = false;
				msg.JCM_Language = Core.SharedConstants.Languages.English;

				var legacy = Incident.Factory.New<EdiLegacyConversationMessage>();
				legacy.ELC_JCM_Message = msg.PK;

				if (!IsSystemAccountCode(remoteMessage.UserCode))
				{
					var contact = FindOrCreateContact(Incident, remoteMessage.UserCode, remoteMessage.UserName);
					if (contact != null)
					{
						var participant = Conversation.Participants.GetOrAdd(contact);
						if (participant != null)
						{
							participant.JCP_IsSubscribed = false;
							msg.JCM_JCP_Participant = participant.PK;
						}
					}
				}

				newMessages.Add(msg);
				Conversation.Messages.Add(msg);
				OnMessageCountChanged();

				lastReceivedTime = receivedTime;
			}
		}

		static OrgContact FindOrCreateContact(SupportIncident incident, string userCode, string userName)
		{
			if (string.IsNullOrEmpty(userName))
			{
				return null;
			}

			var nameTruncatedForContact = userName.Length <= OrgContact.Schema.OC_ContactNameMaxLength
				? userName
				: userName.Substring(0, OrgContact.Schema.OC_ContactNameMaxLength);

			var mainContact = incident.Contact;
			if (mainContact != null && mainContact.OC_ContactName.EqualsIgnoringCase(nameTruncatedForContact))
			{
				return mainContact;
			}

			var orgPk = incident.IM_OH_Client;
			if (orgPk.IsEmpty && mainContact != null)
			{
				orgPk = mainContact.OC_OH;
			}

			OrgContact result = null;
			if (!orgPk.IsEmpty)
			{
				var nameQuery = new ZQuery(OrgContactSchema.OC_OH, orgPk);
				nameQuery.AddToFilter(OrgContactSchema.OC_ContactName, nameTruncatedForContact);
				var contacts = incident.Factory.Load<OrgContact>(nameQuery);
				if (contacts.Length > 0)
				{
					if (contacts.Length == 1)
					{
						result = contacts[0];
						if (!result.OC_IsActive)
						{
							result.OC_IsActive = true;
						}
					}
					else
					{
						// pick an active one
						result = contacts.OrderBy(x => x.OC_IsActive ? 0 : 1).First();
					}
				}
				else
				{
					// create new
					result = incident.Factory.New<OrgContact>();
					result.OC_OH = orgPk;
					result.OC_ContactName = nameTruncatedForContact;
					result.OC_Email = GetDatabaseStaffEmail(incident.Factory, incident.IM_LD, userCode);
				}
			}

			return result;
		}

		static ZString GetDatabaseStaffEmail(BusinessObjectFactory factory, ZGuid databasePk, string userCode)
		{
			if (databasePk.IsEmpty || string.IsNullOrEmpty(userCode))
			{
				return ZString.Empty;
			}

			var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, databasePk);
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, userCode);
			return factory.LoadTop1<EdiCustomerUserAccount>(query)?.EUA_Email ?? ZString.Empty;
		}

		static bool IsSystemAccountCode(string code)
		{
			return code == User.ServiceUserCode
				|| code == User.WebUserCode
				|| code == "E" // CargoWise Support
				|| code == User.UnKnownUserCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static void ConvertOldLogTextToEConversation(JobConversation convers, ZString textToConvert, ZDateTime defaultMessageTime)
		{
			ZString logText = textToConvert.ToString();

			Regex regex = new Regex(".. .+:..(:.. AM|:.. PM)? - .+\r\n");
			MatchCollection matches = regex.Matches(logText);

			if (matches.Count > 0)
			{
				foreach (Match match in matches)
				{
					Regex codeRegex = new Regex(Regex.Escape(" - "));
					string log = codeRegex.Replace(match.ToString(), " ~BP - ", 1);
					log = log.Insert(log.Length, "----------------------------------------------------------------------------------------------------------------------\r\n");
					Regex lineRegex = new Regex(Regex.Escape(match.ToString()));
					logText = lineRegex.Replace(logText, log);
				}
			}

			string splitter = "\r\n" + ZString.Replicate('-', 118) + "\r\n";
			string[] logEntries = logText.ToString().Split(new string[] { splitter }, StringSplitOptions.None);
			for (int i = logEntries.Length - 1; i >= 0; i--)
			{
				string entry = logEntries[i];
				if (!string.IsNullOrWhiteSpace(entry))
				{
					try
					{
						// cope with formats:
						//		22/06/2006 8:56:10 AM ME - Michelle's original resolution 
						//		04-Nov-11 14:11 LS - Email Sent to 
						//		3/07/2006 5:05:32 PM HY – single digit day
						//		16/08/2006 22:51:48 ~BP – has seconds, but no AM/PM

						int endInitialsIndex = entry.IndexOf(" - ", StringComparison.Ordinal);
						string dateTimeAndInitials = entry.Substring(0, endInitialsIndex).Trim();
						string[] words = dateTimeAndInitials.Split(' ');

						ZDateTime parsedDateTime = ZDateTime.Empty;
						if (words.Length == 3)
						{
							ZDateTime.TryParseExact(words[0] + ' ' + words[1], out parsedDateTime, ZDateTime.LongTimeFormat);
							if (!parsedDateTime.IsValid)
							{
								ZDateTime.TryParseExact(words[0] + ' ' + words[1], out parsedDateTime, "dd/MM/yyyy HH:mm:ss");
							}
						}
						else if (words.Length == 4)
						{
							ZDateTime.TryParseExact(words[0] + ' ' + words[1] + ' ' + words[2], out parsedDateTime, "dd/MM/yyyy h:mm:ss tt");
							if (!parsedDateTime.IsValid)
							{
								ZDateTime.TryParseExact(words[0] + ' ' + words[1] + ' ' + words[2], out parsedDateTime, "d/MM/yyyy h:mm:ss tt");
							}
						}

						string userCode = words[words.Length - 1];
						GlbStaff user = null;

						if (!string.IsNullOrWhiteSpace(userCode))
						{
							user = convers.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, userCode);
						}

						string body = entry.Substring(endInitialsIndex + 3);
						if (parsedDateTime.IsEmpty || parsedDateTime == DateTime.MinValue)
						{
							parsedDateTime = defaultMessageTime;
						}
						else
						{
							parsedDateTime = parsedDateTime.AddHours(-10);  //Assume log time is non day light saving sydney time
						}

						JobConversationParticipant participant = null;
						if (user != null)
						{
							participant = convers.Participants.GetOrAdd(user);
							participant.JCP_IsSubscribed = false;
						}
						var msg = convers.Messages.AddNew(participant, body, false);
						msg.JCM_PostedTimeUtc = parsedDateTime;
					}
					catch (Exception ex) when (!ex.IsCriticalException()) { }
				}
			}
		}
	}
}
