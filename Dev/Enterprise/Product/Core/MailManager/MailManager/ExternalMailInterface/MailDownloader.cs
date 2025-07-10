using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Common;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailDownloader : IMailDownloader
	{
		internal const int Megabyte = 1024 * 1024;

		public MailDownloader() : this(MailServerConfiguration.Default)
		{
		}

		public MailDownloader(IMailServerConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public event EmailDownloadedHandler EmailDownloaded;
		public event LogMessageHandler LogMessage;
		public event DownloaderClosingHandler DownloaderClosing;

		protected bool shouldContinue;

		IMailProtocol mailProtocol;
		protected IMailProtocol MailProtocol
		{
			get
			{
				if (mailProtocol == null)
				{
					mailProtocol = GetMailProtocol();
					mailProtocol.Open();
				}

				return mailProtocol;
			}
		}

		readonly IMailServerConfiguration configuration;

		protected virtual IMailProtocol GetMailProtocol()
		{
			return configuration.GetMailProtocol(LogMessage, ReportIncomingCommandException);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System log message")]
		public void DownloadFromServer()
		{
			if (string.IsNullOrEmpty(configuration.Server))
			{
				OnLogMessage(TraceEventType.Error, "Mail server should not be empty.\r\nThis can be entered in the Registry: System->Registry->Physical Server->Mail->Incoming->Mail Server");
				return;
			}

			if (configuration.Port == 0)
			{
				OnLogMessage(TraceEventType.Error, "Mail server port should not be empty.\r\nThis can be entered in the Registry: System->Registry->Physical Server->Mail->Incoming->Mail Server Port");
				return;
			}

			if (string.IsNullOrEmpty(configuration.UserName))
			{
				OnLogMessage(TraceEventType.Error, "Mailbox user name should not be empty.\r\nThis can be entered in the Registry: System->Registry->Physical Server->Mail->Incoming->Mailbox User Name");
				return;
			}

			try
			{
				var protocol = MailProtocol;
				var messageCount = protocol.MessageCount;

				try
				{
					if (messageCount > 0)
					{
						OnLogMessage(TraceEventType.Information,
							MailDownloaderLogBuilder.BuildDownloadingLog(messageCount, configuration.Server));
						shouldContinue = true;

						var ids = protocol.GetAllMessageIds();
						var theAmountOfMailItemsShouldBeLoggedAfterDownload = MailDownloaderLogBuilder
							.CalculateTheAmountOfMailItemsShouldBeLoggedAfterDownload(ids.Count);
						var idx = 0;
						foreach (var id in ids)
						{
							if (!shouldContinue)
							{
								break;
							}

							MemoryFailPoint memoryFailPoint = null;
							try
							{
								long messageSize;
								try
								{
									messageSize = protocol.GetMessageSizeById(id);
								}
								catch (ArgumentException ex)
								{
									OnLogMessage(TraceEventType.Error,
										"Error getting details of Email with unique id: {0}. Skipping this email.\r\n\r\n{1}",
										id, ex.ToString());
									continue; // Skip and go to next email item
								}

								memoryFailPoint = GetMemoryFailPoint(messageSize);

								var messageById = protocol.GetMessageById(id);
								if (++idx == theAmountOfMailItemsShouldBeLoggedAfterDownload)
								{
									OnLogMessage(TraceEventType.Information,
										MailDownloaderLogBuilder.BuildDownloadedLog(idx));
									idx = 0;
								}

								if (messageById != null)
								{
									var message = Encoding.UTF8.GetString(messageById);

									try
									{
										OnEmailDownloaded(id, ref message);
									}
									catch (Exception e) when (!e.IsCriticalException() &&
															!(e is ServiceNotConnectedException))
									{
										if (NeedToReport(e))
										{
											ErrorReporter.ReportOnce(
												$"An exception (other than FormatException) occured while downloading an email from the {protocol} server at port {configuration.Port}. Was downloading {messageCount} emails from the server. Registry Mail Retrieval Protocol: {configuration.Protocol}.",
												e);
										}

										OnLogMessage(TraceEventType.Error,
											MailDownloaderLogBuilder.BuildDownloadingErrorLogWithUniqueId(id, e));
									}
								}
							}
							catch (ServiceNotConnectedException)
							{
								protocol.ReOpenIfNeeded();
							}
							catch (InsufficientMemoryException)
							{
								OnLogMessage(TraceEventType.Error,
									MailDownloaderLogBuilder.BuildInsufficientMemoryLogWithUniqueId(id));
							}
							catch (OutOfMemoryException)
							{
								OnLogMessage(TraceEventType.Error,
									MailDownloaderLogBuilder.BuildOutOfMemoryLogWithUniqueId(id));
							}
							finally
							{
								memoryFailPoint?.Dispose();
							}
						}

						if (idx != 0)
						{
							OnLogMessage(TraceEventType.Information, MailDownloaderLogBuilder.BuildDownloadedLog(idx));
						}
					}
				}
				finally
				{
					OnDownloaderClosing(messageCount);
				}
			}
			catch (RegistryJsonException ex)
			{
				OnLogMessage(TraceEventType.Error,
					$"{ex.Message} Please check the registry and renew the corresponding {RawDataRegistry.oAuth} settings.");
			}
			catch (ProtocolException ex)
			{
				OnLogMessage(TraceEventType.Error,
					@"Protocol error while trying to download email from server '{0}': {1}

This error usually means there's a connection problem between {2} and the server. If you continually see this error, please check the mail incoming registry settings.",
					configuration.Server, ex.Message, BrandingFactory.Instance.ProductName);
			}
			catch (Exception ex) when (ErrorDownloadingEmailExceptionFilter(ex))
			{
				ReportErrorDownloadingEmail(ex);
			}
		}

		static bool ErrorDownloadingEmailExceptionFilter(Exception ex)
		{
			return ex
				is System.Security.Authentication.AuthenticationException
				or System.IO.IOException
				or FailedToAuthenticateException
				or SocketException
				or ServiceNotConnectedException
				or TimeoutException
				or MailKit.Security.AuthenticationException;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		protected void ReportErrorDownloadingEmail(Exception ex)
		{
			OnLogMessage(TraceEventType.Error, "Error downloading email from mail server at {0}:{1} - {2}", configuration.Server, configuration.Port, ex.GetFullMessage());
		}

		#region SuppressResourceStringsCheckRegion
		internal void ReportIncomingCommandException(string uniqueId, Exception exception, string emailAction)
		{
			var detailedExceptionMessage = string.Empty;
			if (exception is ImapCommandException imapCommandException)
			{
				detailedExceptionMessage = FormattableString.Invariant($@"Response Type: {imapCommandException.Response}
Response Text: {imapCommandException.ResponseText}");
			}
			else if (exception is Pop3CommandException pop3CommandException)
			{
				detailedExceptionMessage = FormattableString.Invariant($"Status Text: {pop3CommandException.StatusText}");
			}

			var uniqueIdMessage = string.IsNullOrEmpty(uniqueId) ? "" : $" with UniqueId '{uniqueId}'";
			var message = FormattableString.Invariant($@"Error {emailAction} email{uniqueIdMessage} from server '{configuration.Server}', this error is returned from the server side, it's better to check with your server provider, see below for details.
{detailedExceptionMessage}
Exception: {exception}");

			OnLogMessage(TraceEventType.Warning, message);
		}
		#endregion

		internal static MemoryFailPoint GetMemoryFailPoint(long messageSize)
		{
			var requiredMemory = (int)((messageSize * MemoryFailPointFactor) / Megabyte);
			return requiredMemory > 0 ? GCWrapper.MemoryFailPoint(requiredMemory) : null;
		}

		static int MemoryFailPointFactor => OverridableMemoryFailPointFactor.Value;
		internal static readonly Overridable<int> OverridableMemoryFailPointFactor = new(7);

		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "internal exception message")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal static bool NeedToReport(Exception e)
		{
			if (e is EmailHasNoRecipientsException)
			{
				return false;
			}

			if (e is ProtocolException)
			{
				return false;
			}

			if (e is InvalidOperationException && e.Message.StartsWith("Email address is longer than max length.", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return true;
		}

		void IMailDownloader.DeleteMessage(string messageId)
		{
			MailProtocol?.DeleteMessageById(messageId);
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void OnEmailDownloaded(string uniqueId, ref string email)
		{
			EmailDownloaded?.Invoke(uniqueId, ref email, ref shouldContinue);
		}

		protected void OnDownloaderClosing(long messageCount)
		{
			DownloaderClosing?.Invoke(messageCount);
		}

		internal void OnLogMessage(TraceEventType eventType, string format, params object[] args)
		{
			LogMessage?.Invoke(eventType, string.Format(format, args));
		}

		public virtual void Dispose()
		{
			try
			{
				mailProtocol?.Dispose();
			}
			catch (Exception ex)
			{
				OnLogMessage(TraceEventType.Error, (NoResString)"Error while closing connection to server '{0}': {1}", configuration.Server, ex.Message);
			}
		}
	}
}
