using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.ZArchitecture.Environment;
using MailKit;
using MailKit.Net.Pop3;

namespace Enterprise.MailManager.ExternalMailInterface.POP3
{
	public class MailKitPop3 : Pop3Client, IMailProtocol
	{
		public MailKitPop3(
			MailServerConnectionConfiguration connectionConfiguration,
			UserPasswordAuthConfiguration authConfiguration,
			Action<string, Exception, string> reportErrorAction = null,
			IProtocolLogger protocolLogger = null)
			: this(connectionConfiguration.SecureConnectionType, reportErrorAction ,protocolLogger)
		{
			MailServiceHelper = new MailServiceHelper(this, connectionConfiguration, authConfiguration);
		}

		public MailKitPop3(
			MailServerConnectionConfiguration connectionConfiguration,
			IOAuth2Configuration authConfiguration,
			Action<string, Exception, string> reportErrorAction = null,
			IProtocolLogger protocolLogger = null)
			: this(connectionConfiguration.SecureConnectionType, reportErrorAction, protocolLogger)
		{
			MailServiceHelper = new MailServiceHelper(this, connectionConfiguration, authConfiguration);
		}

		MailKitPop3(SecureConnectionTypes secureConnectionType, Action<string, Exception, string> reportErrorAction = null, IProtocolLogger protocolLogger = null) : base(protocolLogger ?? new NullProtocolLogger())
		{
			this.reportErrorAction = reportErrorAction;
			this.secureConnectionType = secureConnectionType;
		}

		protected SecureConnectionTypes secureConnectionType;

		readonly Action<string, Exception, string> reportErrorAction;

		internal MailServiceHelper MailServiceHelper { get; }

#if DEBUG
		internal bool MockAuthenticateIsFalse_ForTest { get; set; }

		internal bool IsReTryAuthenticate_ForTest { get; set; }
#endif

		public void Open()
		{
			if (secureConnectionType == SecureConnectionTypes.TLS)
			{
				SslProtocols = SslProtocols.Tls12;
			}

			MailServiceHelper.Open();
		}

		internal Dictionary<string, int> messageIds = new Dictionary<string, int>();

		public ICollection<string> GetAllMessageIds()
		{
			FillAndCheckMessageIdsIfNeeded("");
			return messageIds.Keys.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a CodeSmell")]
		bool FillAndCheckMessageIdsIfNeeded(string id, bool forceRefresh = false)
		{
			if (IsConnected && ((!messageIdsFilled && messageIds.IsNullOrEmpty() && Count > 0) || forceRefresh))
			{
				if (forceRefresh)
				{
					messageIds.Clear();
				}

				for (var i = 0; i < Count; i++)
				{
					try
					{
						messageIds[GetMessageUid(i)] = i;
					}
					catch (Pop3CommandException e)
					{
						reportErrorAction?.Invoke(id, e, "downloading");
					}
					catch (NotSupportedException e)
					{
						reportErrorAction?.Invoke(id, e, "downloading");
					}
				}

				messageIdsFilled = true;
			}

			if (!id.IsNullOrEmpty())
			{
				return messageIds.Any() && messageIds.ContainsKey(id);
			}

			return false;
		}

		bool messageIdsFilled;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a CodeSmell")]
		public byte[] GetMessageById(string id)
		{
			if (FillAndCheckMessageIdsIfNeeded(id))
			{
				try
				{
					return GetStream(messageIds[id]).ToByteArray();
				}
				catch (Pop3CommandException e)
				{
					reportErrorAction?.Invoke(id, e, "downloading");
					messageIds.Remove(id);
					return null;
				}
			}

			return null;
		}

		public const int POP3MaximumTimeoutForEmailDeletion = 30;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a CodeSmell")]
		public void DeleteMessageById(string id)
		{
			ProcessMessagWithIdRefresh(id,
				messageId =>
				{
					var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(POP3MaximumTimeoutForEmailDeletion)).Token;
					try
					{
#if DEBUG
						if (Globals.IsTest && MockAuthenticateIsFalse_ForTest)
						{
							throw new ServiceNotAuthenticatedException("The Pop3Client has not been authenticated.");
						}
#endif
						DeleteMessage(messageIds[messageId], cancellationToken);
					}
					catch (ServiceNotAuthenticatedException)
					{
						MailServiceHelper.TryAuthenticate();
						if (IsAuthenticated)
						{
							DeleteMessage(messageIds[messageId], cancellationToken);
#if DEBUG
							if (Globals.IsTest && MockAuthenticateIsFalse_ForTest)
							{
								IsReTryAuthenticate_ForTest = true;
							}
#endif
						}
						else
						{
							throw;
						}
					}
					catch (Pop3CommandException e)
					{
						reportErrorAction?.Invoke(id, e, "deleting");
					}
					messageIds.Remove(messageId);
				}
			);
		}

		void ProcessMessagWithIdRefresh(string id, Action<string> messageAction)
		{
			if (FillAndCheckMessageIdsIfNeeded(id))
			{
				try
				{
					messageAction(id);
				}
				catch (ArgumentOutOfRangeException)
				{
					if (FillAndCheckMessageIdsIfNeeded(id, forceRefresh: true))
					{
						messageAction(id);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a CodeSmell")]
		public long GetMessageSizeById(string id)
		{
			if (FillAndCheckMessageIdsIfNeeded(id))
			{
				try
				{
					return GetMessageSize(messageIds[id]);
				}
				catch (Pop3CommandException e)
				{
					reportErrorAction?.Invoke(id, e, "downloading");
					messageIds.Remove(id);
				}
			}

			return 0;
		}

		public void Close()
		{
			MailServiceHelper.Close();
		}

		protected override void Dispose(bool disposing)
		{
			Close();
			base.Dispose(disposing);
		}

		public long MessageCount => Count;

		public byte[] GetMessageByMessageNumber(long messageNumber)
		{
			var realIndex = (int)messageNumber - 1; // Index of MailKit is 0 based, while the old Limilabs dll is 1 based, we need -1 here.
			return GetStream(realIndex).ToByteArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		public void DeleteMessageByNumber(List<long> messageNumbers)
		{
			void BatchDeleteAction(IList<int> numbers) => DeleteMessages(numbers);
			void DeleteAction(int number) => DeleteMessage(number);
			bool IsMessageNotExistFunc(Exception ex)
			{
				return ex.Message.Contains("One or more of the indexes are invalid") || ex is ArgumentOutOfRangeException;
			}

			MailServiceHelper.DeleteMessageByNumbers(messageNumbers, BatchDeleteAction, DeleteAction, IsMessageNotExistFunc);
		}

		public void ReOpenIfNeeded()
		{
			MailServiceHelper.TryHardConnectAndAuthenticate();
		}
	}
}
