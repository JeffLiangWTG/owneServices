using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using CargoWise.Common;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.ZArchitecture.Core;
using Microsoft.Graph;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public sealed class GraphMailDownloader : IMailDownloader
	{
		public event EmailDownloadedHandler EmailDownloaded;
		public event LogMessageHandler LogMessage;
		public event DownloaderClosingHandler DownloaderClosing;

		readonly IMailServerConfiguration configuration;

		public GraphMailDownloader() : this(MailServerConfiguration.Default)
		{
		}

		public GraphMailDownloader(IMailServerConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public void DeleteMessage(string messageId)
		{
			try
			{
#if DEBUG
				SetUserRequestBuilderForTest();
#endif
				if (!string.IsNullOrEmpty(messageId) && userRequestBuilder != null)
				{
					userRequestBuilder.MailFolders.Inbox.Messages[messageId].Request().DeleteAsync().GetResultByAwaiter();
#if DEBUG
					LogMessage?.Invoke(TraceEventType.Information, $"Mail({messageId}) deleted");
#endif
				}
				else
				{
					LogMessage?.Invoke(TraceEventType.Error, $"'MailboxEmailAddress' doesn't exist in AAD, please check it in registry.");
				}
			}
			catch (Exception ex)
			{
				LogMessage?.Invoke(TraceEventType.Error, $"Error deleting email from mail server. \r\n message id : {messageId}, exception : {ex.GetFullMessage()}");
				throw;
			}
		}

		public void DownloadFromServer()
		{
			try
			{
				var oAuthConfig = configuration.GetOAuth2Configuration();
				var authResult = Ms365OAuth2AuthenticationHelper.GetOAuth2AuthenticationResult(oAuthConfig);
				var graphServiceClient = authResult.GetGraphServiceClient(m => LogMessage?.Invoke(TraceEventType.Verbose, m));
				userRequestBuilder = GraphUtils.GetUserRequestBuilder(graphServiceClient, (Ms365OAuth2Configuration)oAuthConfig);
#if DEBUG
				SetUserRequestBuilderForTest();
#endif

				long messageCount = 0;
				IMailFolderMessagesCollectionPage messageIds = null;

				do
				{
					if (messageIds == null)
					{
						messageIds = userRequestBuilder.MailFolders.Inbox.Messages.Request().Select(GetMessageSummaryFields).Expand(getMessageSizePropertyExpander).GetAsync().GetResultByAwaiter();
					}
					else
					{
						messageIds = messageIds.NextPageRequest.GetAsync().GetResultByAwaiter();
					}

					if (messageIds.Count > 0)
					{
						DownloadMails(messageIds);
						messageCount += messageIds.Count;
					}
				} while (messageIds.NextPageRequest != null);

				DownloaderClosing?.Invoke(messageCount);
			}
			catch (Exception ex)
			{
				LogMessage?.Invoke(TraceEventType.Error, $"Error downloading email from mail server - {ex.GetFullMessage()}");
			}
		}

		void DownloadMails(IMailFolderMessagesCollectionPage messagesIds)
		{
			if (messagesIds != null)
			{
				var messageCount = messagesIds.Count;
				LogMessage?.Invoke(TraceEventType.Information, MailDownloaderLogBuilder.BuildDownloadingLog(messageCount));

				var messageSummaries = messagesIds.Where(m => !string.IsNullOrEmpty(m.Id)).Select(m => new
				{
					m.Id,
					MessageSize = m.SingleValueExtendedProperties.GetValue(PidTagMessageSizeId),
				});

				var shouldContinue = true;

				var theAmountOfMailItemsShouldBeLoggedAfterDownload = MailDownloaderLogBuilder.CalculateTheAmountOfMailItemsShouldBeLoggedAfterDownload(messageCount);
				var idx = 0;
				foreach (var messageSummary in messageSummaries)
				{
					if (!shouldContinue)
					{
						break;
					}

					long.TryParse(messageSummary.MessageSize, out long messageSize);
					MemoryFailPoint memoryFailPoint = null;
					try
					{
						memoryFailPoint = MailDownloader.GetMemoryFailPoint(messageSize);

						var messageById = GetMessageById(messageSummary.Id);
						if (++idx == theAmountOfMailItemsShouldBeLoggedAfterDownload)
						{
							LogMessage?.Invoke(TraceEventType.Information, MailDownloaderLogBuilder.BuildDownloadedLog(idx));
							idx = 0;
						}

						if (!string.IsNullOrEmpty(messageById))
						{
							EmailDownloaded?.Invoke(messageSummary.Id, ref messageById, ref shouldContinue);
						}
					}
					catch (InsufficientMemoryException)
					{
						LogMessage?.Invoke(TraceEventType.Error, MailDownloaderLogBuilder.BuildInsufficientMemoryLogWithUniqueId(messageSummary.Id));
					}
					catch (OutOfMemoryException)
					{
						LogMessage?.Invoke(TraceEventType.Error, MailDownloaderLogBuilder.BuildOutOfMemoryLogWithUniqueId(messageSummary.Id));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						LogMessage?.Invoke(TraceEventType.Error, MailDownloaderLogBuilder.BuildDownloadingErrorLogWithUniqueId(messageSummary.Id, ex));
					}
					finally
					{
						memoryFailPoint?.Dispose();
					}
				}
				if (idx != 0)
				{
					LogMessage?.Invoke(TraceEventType.Information, MailDownloaderLogBuilder.BuildDownloadedLog(idx));
				}
			}
		}

		string GetMessageById(string id)
		{
			var mimeMessageStream = userRequestBuilder.Messages[id].Content.Request().GetAsync().GetResultByAwaiter();
			using (var reader = new StreamReader(mimeMessageStream))
			{
				return reader.ReadToEnd();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const String")]
		const string GetMessageSummaryFields = "id,singleValueExtendedProperties";

		//'LONG 0xE08' is the property id which refers to PidTagMessageSize.
		// See https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxprops/f6ab1613-aefe-447d-a49c-18217230b148 to find Exchange Server Protocols Master Property List in the documents.
		// Note: the property id in the document is LONG 0x0E08, but in the singleValueExtendedProperties it's LONG 0xE08. We need to use LONG 0xE08 to get the value.
#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const String")]
		internal
#endif
		const string PidTagMessageSizeId = "LONG 0xE08";

		readonly string getMessageSizePropertyExpander = $"singleValueExtendedProperties($filter=Id eq '{PidTagMessageSizeId}')";

		IUserRequestBuilder userRequestBuilder;

#if DEBUG
		#region ForTest

		public IUserRequestBuilder UserRequestBuilderForTest { get; set; }

		void SetUserRequestBuilderForTest()
		{
			if (ZArchitecture.Environment.Globals.IsTest && UserRequestBuilderForTest != null)
			{
				userRequestBuilder = UserRequestBuilderForTest;
			}
		}

		#endregion
#endif
		public void Dispose()
		{
		}
	}
}
