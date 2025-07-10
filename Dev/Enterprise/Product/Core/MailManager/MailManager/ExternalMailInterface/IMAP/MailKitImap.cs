using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using MailKit;
using MailKit.Net.Imap;

namespace Enterprise.MailManager.ExternalMailInterface.IMAP
{
	public class MailKitImap : ImapClient, IMailProtocol
	{
		public MailKitImap(
			MailServerConnectionConfiguration mailServerConfiguration,
			UserPasswordAuthConfiguration userPasswordAuthConfiguration,
			Action<string, Exception, string> reportErrorAction = null,
			IProtocolLogger protocolLogger = null)
			: this(reportErrorAction, protocolLogger)
		{
			MailServiceHelper = new MailServiceHelper(this, mailServerConfiguration, userPasswordAuthConfiguration);
		}

		public MailKitImap(
			MailServerConnectionConfiguration mailServerConfiguration,
			IOAuth2Configuration oAuth2Configuration,
			Action<string, Exception, string> reportErrorAction = null,
			IProtocolLogger protocolLogger = null)
			: this(reportErrorAction, protocolLogger)
		{
			MailServiceHelper = new MailServiceHelper(this, mailServerConfiguration, oAuth2Configuration);
		}

		MailKitImap(Action<string, Exception, string> reportErrorAction = null, IProtocolLogger protocolLogger = null) : base(protocolLogger ?? new NullProtocolLogger())
		{
			this.reportErrorAction = reportErrorAction;
		}

		readonly Action<string, Exception, string> reportErrorAction;
		internal MailServiceHelper MailServiceHelper { get; }

		public void Open()
		{
			MailServiceHelper.Open();
		}

		protected internal Dictionary<string, IMessageSummary> messageIds = new Dictionary<string, IMessageSummary>();
		readonly HashSet<string> removedIds = new HashSet<string>();

		void RemoveMessageId(string messageId)
		{
			if (messageIds.ContainsKey(messageId) && !string.IsNullOrEmpty(messageId))
			{
				messageIds.Remove(messageId);
				removedIds.Add(messageId);
			}
		}

		public ICollection<string> GetAllMessageIds()
		{
			FillAndCheckMessageIdsIfNeeded("");
			return messageIds.Keys.ToArray();
		}

		internal virtual bool FillAndCheckMessageIdsIfNeeded(string id)
		{
			if (messageIds.IsNullOrEmpty() && base.IsConnected && !removedIds.Contains(id) && Inbox.Count > 0)
			{
				var messageSummaries = Inbox.Fetch(0, -1, MessageSummaryItems.Size | MessageSummaryItems.UniqueId);
				messageSummaries.ForEach(s => messageIds[s.UniqueId.ToString()] = s);
			}

			if (!id.IsNullOrEmpty())
			{
				return messageIds.Any() && messageIds.ContainsKey(id);
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a CodeSmell")]
		public byte[] GetMessageById(string id)
		{
			if (FillAndCheckMessageIdsIfNeeded(id))
			{
				try
				{
					return Inbox.GetStream(messageIds[id].UniqueId, "").ToByteArray();
				}
				catch (ImapCommandException e)
				{
					reportErrorAction?.Invoke(id, e, "downloading");
					RemoveMessageId(id);
					return null;
				}
				catch (MessageNotFoundException)
				{
					RemoveMessageId(id);
					return null;
				}
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a CodeSmell")]
		public void DeleteMessageById(string id)
		{
			if (FillAndCheckMessageIdsIfNeeded(id))
			{
				try
				{
					Inbox.AddFlags(messageIds[id].UniqueId, MessageFlags.Deleted, true);
				}
				catch (ImapCommandException e)
				{
					reportErrorAction?.Invoke(id, e, "deleting");
				}
				RemoveMessageId(id);
			}
		}

		public long GetMessageSizeById(string id)
		{
			if (FillAndCheckMessageIdsIfNeeded(id))
			{
				var size = messageIds[id].Size;
				return size ?? 0;
			}

			return 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a CodeSmell")]
		public void Close()
		{
			if (IsConnected && IsAuthenticated && Inbox != null && Inbox.IsOpen)
			{
				try
				{
					Inbox.Expunge();
				}
				catch (ImapCommandException e)
				{
					reportErrorAction?.Invoke(null, e, "expunge");
				}
			}
			MailServiceHelper.Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				Close();
			}
			base.Dispose(disposing);

			disposed = true;
		}
		bool disposed;

		public long MessageCount => Inbox.Count;
		public byte[] GetMessageByMessageNumber(long messageNumber)
		{
			var realIndex = (int)messageNumber - 1; // Index of MailKit is 0 based, while the old Limilabs dll is 1 based, we need -1 here.
			return Inbox.GetStream(realIndex, "").ToByteArray();
		}

		public void DeleteMessageByNumber(List<long> messageNumbers)
		{
			MailServiceHelper.DeleteMessageByNumbers(messageNumbers, BatchDeleteAction, DeleteAction, IsMessageNotExistFunc);
		}

		virtual protected void BatchDeleteAction(IList<int> numbers) => Inbox.AddFlags(numbers, MessageFlags.Deleted, true);
		virtual protected void DeleteAction(int number) => Inbox.AddFlags(number, MessageFlags.Deleted, true);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		bool IsMessageNotExistFunc(Exception ex) =>
			ex is ImapCommandException imapCommandException &&
			(imapCommandException.Message.Contains("Invalid messageset") ||
			imapCommandException.Message.Contains("Some of the requested messages no longer exist") ||
			imapCommandException.Message.Contains("The specified message set is invalid"));

		public void ReOpenIfNeeded()
		{
			MailServiceHelper.TryHardConnectAndAuthenticate();
		}
	}
}
