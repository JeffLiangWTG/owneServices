using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DocumentEngine;
using Enterprise.RemotePrinting.Server.JobPrinting;
using Enterprise.RemotePrinting.Server.Model;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.RemotePrinting.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server
{
	[ThreadSafe]
	public class RemotePrintController
	{
		readonly IHubContext<IRemoteClient> hub;
		readonly ConcurrentDictionary<string, SignalRClientInfo> registered = new ConcurrentDictionary<string, SignalRClientInfo>();

		public RemotePrintController()
			: this(GlobalHost.ConnectionManager.GetHubContext<RemoteHub, IRemoteClient>())
		{
		}

		public RemotePrintController(IHubContext<IRemoteClient> hub)
		{
			this.hub = hub;
			SignalRExceptionHandler.RegisterHandler();
		}

		public SignalRClientInfo[] GetRegisteredClients()
		{
			return registered.Values.ToArray();
		}

		public bool HasRegisteredClient(string clientId)
		{
			return !string.IsNullOrEmpty(clientId) && hub.Clients.Client(clientId) != null;
		}

		public void RegisterClientForReconnecting(string clientId)
		{
			if (!string.IsNullOrEmpty(clientId) && hub.Clients.Client(clientId) != null && !registered.Any(x => x.Key == clientId))
			{
				hub.Clients.Client(clientId).RegisterClientForReconnecting();
			}
		}

#if DEBUG

		public void RegisterClientForTest(string client, string serverName, string[] printers, string versionNumber)
		{
			registered[client] = new SignalRClientInfo
			{
				ServerName = serverName,
				PrintersList = printers.ToList(),
				WebPrintClientVersionNumber = versionNumber,
				ClientId = client
			};
		}

#endif

		public bool RequestPrint(string server, SerialisablePrintJob job)
		{
			var clientId = FindClientForPrinter(server, job.QueueName);
			return RequestPrintCore(clientId, job);
		}

		protected virtual bool RequestPrintCore(string clientId, SerialisablePrintJob job)
		{
			if (clientId != null)
			{
				var client = hub.Clients.Client(clientId);

				if (client is null)
				{
					return false;
				}
				client.Print(job);

				return true;
			}
			return false;
		}

		public string RequestClientLogs(string clientId, ClientLogRequestDetails requestDetails)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				return (NoResString)"Client ID is not specified.";
			}

			var client = hub.Clients.Client(clientId);
			if (client == null)
			{
				return (NoResString)"Client is not registered on this hub.";
			}

			client.RetrieveLogsAndPostToServer(requestDetails.EmailAddress, requestDetails.FromDate, requestDetails.ToDate, requestDetails.LogTypes);
			return string.Empty;
		}

		public bool Nudge(string serverName, string printQueueName, Guid printJobPK, HubCallerContext context)
		{
			using (var connection = DbHelper.NewConnection())
			{
				return Nudge(serverName, printQueueName, printJobPK, connection, context);
			}
		}

		public bool Nudge(string serverName, string printQueueName, Guid printJobPK, DbConnection connection, HubCallerContext context = null, bool isForwarded = false)
		{
			SerialisablePrintJob job = null;
			string serviceAddress = null;
			if (printJobPK != Guid.Empty && RegistryData.WebPrintAllowDirectPrintPrintPushNotification(connection))
			{
				(job, serviceAddress) = GetSerialisablePrintJob(printJobPK, connection);
			}

			try
			{
				var printQueueToNudge = job != null ? job.QueueName : printQueueName;
				var clientId = !string.IsNullOrEmpty(printQueueToNudge) ? FindClientForPrinter(serverName, printQueueToNudge) : FindClient(serverName);

				if (clientId == null || hub.Clients.Client(clientId) == null)
				{
					if (string.IsNullOrEmpty(serviceAddress))
					{
						serviceAddress = GetWebPrintServiceAddress(serverName, printQueueToNudge, connection);
					}

					if (!string.IsNullOrEmpty(serviceAddress) && !isForwarded)
					{
						return ForwardNudgeToCorrectServer(serviceAddress, serverName, printQueueToNudge, printJobPK, context, connection);
					}

					return false;
				}

				if (job != null && job.Contents.Length < MaximumReceiveMessageSize)
				{
					if (MarkJobAsWorking(job.JobPk, connection))
					{
						if (!RequestPrint(serverName, job))
						{
							MarkJobAsQueue(job.JobPk, connection);
							return false;
						}
						return true;
					}
				}

				return NudgeCore(clientId);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (job != null)
				{
					MarkJobAsQueue(job.JobPk, connection);
				}

				ErrorReporter.ReportOnce("Nudge failed with the following exception: " + ex.Message, ex);
				return false;
			}
		}

		protected virtual bool ForwardNudgeToCorrectServer(string serviceAddress, string serverName, string queueName, Guid printJobPK, HubCallerContext context, DbConnection connection)
		{
			var contextUrl = (context?.Request?.Url) ?? (HttpContext.Current?.Request?.Url);

			var currentWebServiceAddress = GetCurrentWebServiceAddress(contextUrl, connection);
			if (!string.IsNullOrEmpty(serviceAddress) && !string.IsNullOrEmpty(currentWebServiceAddress))
			{
				var serviceUrlParts = serviceAddress.Split('|');
				var hostPart = serviceUrlParts.Length > 1 ? serviceUrlParts[1] : string.Empty;
				var serviceUrlStr = serviceUrlParts[0];
				var serviceUrl = new Uri(serviceUrlStr);
				var serviceIp = serviceUrl.Host;
				var servicePort = serviceUrl.Port;

				var currentWebServiceUrlStr = currentWebServiceAddress.Split('|')[0];
				var currentWebServiceUrl = new Uri(currentWebServiceUrlStr);
				var currentWebServiceIp = currentWebServiceUrl.Host;
				var currentWebServicePort = currentWebServiceUrl.Port;

				if (serviceIp != currentWebServiceIp || servicePort != currentWebServicePort)
				{
					var result = WebRequestHelper.NudgePrintServerAsync(serviceAddress, serverName, queueName, printJobPK, true).Result;
					if (WebRequestHelper.IsAuthenticationExceptionWithInvalidCertificate(result.Exception))
					{
						// Retry with the service HTTP IP address.
						if (Uri.IsWellFormedUriString(serviceUrlStr, UriKind.Absolute))
						{
							var originalServiceUri = new Uri(serviceUrlStr);
							var httpUriBuilder = new UriBuilder(Uri.UriSchemeHttp, serviceIp, 80, originalServiceUri.AbsolutePath).ToString();
							result = WebRequestHelper.NudgePrintServerAsync($"{httpUriBuilder}|{hostPart}", serverName, queueName, printJobPK, true).Result;
						}
					}
					return result.Success;
				}
			}

			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public bool MarkJobAsWorking(Guid jobPk, DbConnection connection)
		{
			Argument.NotNull(jobPk, nameof(jobPk)); // Suggested By ReviewBot
			const string commandText = @"
UPDATE dbo.StmPrintJob
SET SP_Status = 'WRK'
WHERE SP_PK = @JobPk AND SP_RetryAttempts = 0";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobPk);
				return command.ExecuteNonQuery() > 0;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public bool MarkJobAsQueue(Guid jobPk, DbConnection connection)
		{
			Argument.NotNull(jobPk, nameof(jobPk)); // Suggested By ReviewBot
			const string commandText = @"
UPDATE dbo.StmPrintJob
SET SP_Status = 'QUE'
WHERE SP_PK = @JobPk AND SP_Status = 'WRK'";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobPk);
				return command.ExecuteNonQuery() > 0;
			}
		}

		protected virtual bool NudgeCore(string clientId)
		{
			if (clientId != null)
			{
				hub.Clients.Client(clientId)?.Nudge();
				return true;
			}
			return false;
		}

		const int MaximumReceiveMessageSize = 32000;

		public void RegisterClient(HubCallerContext context, string serverName, string[] printers, string versionNumber)
		{
			try
			{
				if (IsSupportUser)
				{
					printers = Array.Empty<string>();
				}

				RemoveRegisteredSignalRClientInfoByServerName(serverName);

				using (var connection = DbHelper.NewConnection())
				{
					var printServer = new PrintServer();

					printServer.SetPrintQueues(serverName, printers, connection);
					var currentWebServiceAddress = GetCurrentWebServiceAddress(context?.Request?.Url, connection);

					printServer.UpdatePrintQueuesWebPrintServerAddress(serverName, printers, currentWebServiceAddress, string.Empty, connection);
				}

				registered[context.ConnectionId] = new SignalRClientInfo
				{
					ServerName = serverName,
					PrintersList = printers.ToList(),
					WebPrintClientVersionNumber = versionNumber,
					ClientId = context.ConnectionId
				};
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error registering SignalR Client: " + ex.Message, ex);
				throw;
			}
		}

		void RemoveRegisteredSignalRClientInfoByServerName(string serverName)
		{
			var registeredClient = FindClient(serverName);
			if (registeredClient != null)
			{
				registered.TryRemove(registeredClient, out _);
			}
		}

		protected virtual string GetCurrentWebServiceAddress(Uri contextUrl, DbConnection connection) => ServerHelper.GetCurrentWebServiceAddress(contextUrl, connection);

		bool IsSupportUser => Authentication.IsSupportUser(HttpContext.Current?.User?.Identity?.Name ?? string.Empty);

		public void SetPrintStatus(Guid jobPk, ProcessedStatus status, string failureInfo)
		{
			if (jobPk != Guid.Empty)
			{
				var printserver = new PrintServer();
				if (status == ProcessedStatus.Processed)
				{
					printserver.SetPrintJobSuccess(new List<Guid> { jobPk });
				}
				else
				{
					printserver.SetPrintJobFailure(new List<PrintJobFailed> { new PrintJobFailed(jobPk, failureInfo) });
				}
			}
		}

		public string UnregisterClientAndGetVersionNumber(HubCallerContext context)
		{
			string versionNum = string.Empty;
			if (registered.TryRemove(context.ConnectionId, out SignalRClientInfo serverWithPrinters))
			{
				versionNum = serverWithPrinters.WebPrintClientVersionNumber;
				var printServer = new PrintServer();

				using (var connection = DbHelper.NewConnection())
				{
					var currentWebServiceAddress = GetCurrentWebServiceAddress(context?.Request?.Url, connection);
					printServer.UpdatePrintQueuesWebPrintServerAddress(serverWithPrinters.ServerName, serverWithPrinters.PrintersList, string.Empty, currentWebServiceAddress, connection);
				}
			}
			return versionNum;
		}

#if DEBUG
		protected virtual
#endif
		string FindClientForPrinter(string server, string printer)
		{
			return registered.FirstOrDefault(kvp => MatchesServerNameAndHasPrinter(kvp.Value.ServerName, kvp.Value.PrintersList)).Key;

			bool MatchesServerNameAndHasPrinter(string serverName, List<string> printers)
				=> server.Equals(serverName, StringComparison.OrdinalIgnoreCase) && printers.Contains(printer, StringComparer.OrdinalIgnoreCase);
		}

#if DEBUG
		protected virtual
#endif
		string FindClient(string server)
		{
			return registered.FirstOrDefault(kvp => server.Equals(kvp.Value.ServerName, StringComparison.OrdinalIgnoreCase)).Key;
		}

		public void RequestRefreshCNSWClientSetting(string server)
		{
			var clientId = FindClient(server);
			if (clientId != null)
			{
				hub.Clients.Client(clientId).RefreshCNSWClientSetting();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public (SerialisablePrintJob job, string serviceAddress) GetSerialisablePrintJob(Guid printJobPK, DbConnection connection)
		{
			const string Sql = @"
	SELECT TOP 1
		SP_PK,
		SP_EmailAttachments,
		SP_EmailSubjectLine,
		SP_WatermarkText,
		SP_WatermarkImage,
		SP_Copies,
		SP_CustomProperties,
		SP_EscapeSequence,
		SQ_QueueName,
		SQ_PrintQueueStateChanged,
		SQ_WebPrintServiceAddress
	FROM
		dbo.StmPrintJob
		INNER JOIN dbo.StmPrintQueue ON SQ_PK = SP_SQ
	WHERE 
		SP_PK = @PrintJobPK
		AND SP_JobType = 'PRN'
";

			using (var command = connection.Command(Sql))
			{
				command.AddParameterBasedOnDbColumn("@PrintJobPK", printJobPK, StmPrintQueueSchema.PK);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						var job = new SerialisablePrintJob();

						job.JobPk = (Guid)reader[StmPrintJobSchema.Constants.PK];
						job.EmailSubjectLine = reader[StmPrintJobSchema.Constants.SP_EmailSubjectLine].ToString();

						var watermarkText = reader[StmPrintJobSchema.Constants.SP_WatermarkText].ToString();
						var watermarkImage = reader[StmPrintJobSchema.Constants.SP_WatermarkImage];
						job.HasWatermark = !string.IsNullOrEmpty(watermarkText) || watermarkImage != DBNull.Value;

						job.Copies = SafeCast<short>(reader[StmPrintJobSchema.Constants.SP_Copies]);

						var customPropertiesObj = SafeCast(reader[StmPrintJobSchema.Constants.SP_CustomProperties], Array.Empty<byte>());
						job.Contents = GetUncompressedByteArray(customPropertiesObj);

						var escapeSequence = SafeCast(reader[StmPrintJobSchema.Constants.SP_EscapeSequence], Array.Empty<byte>());
						job.EscapeSequence = GetUncompressedByteArray(escapeSequence);

						var emailAttachmentsObj = reader[StmPrintJobSchema.Constants.SP_EmailAttachments].ToString();
						var safeBlobFileName = PathValidation.GetSafeFilename(emailAttachmentsObj);
						job.BlobType = Path.GetExtension(safeBlobFileName).Trim('.');

						job.QueueName = reader[StmPrintQueueSchema.Constants.SQ_QueueName].ToString();
						job.QueueStateChangedStamp = SafeCast<Guid>(reader[StmPrintQueueSchema.Constants.SQ_PrintQueueStateChanged]);

						var serviceAddress = reader[StmPrintQueueSchema.Constants.SQ_WebPrintServiceAddress].ToString();

						return (job, serviceAddress);
					}
				}
			}

			return (null, null);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		string GetWebPrintServiceAddress(string serverName, string printQueueName, DbConnection connection)
		{
			const string sql = @"
SELECT TOP 1 SQ_WebPrintServiceAddress
  FROM dbo.StmPrintQueue
 INNER JOIN dbo.StmPrintServer on (SPS_PK = SQ_SPS_Server)
 WHERE SQ_QueueName = @QueueName
   AND SPS_ServerName = @ServerName"
			;

			using (var command = connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@ServerName", serverName, StmPrintServerSchema.SPS_ServerName);
				command.AddParameterBasedOnDbColumn("@QueueName", printQueueName, StmPrintQueueSchema.SQ_QueueName);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						return reader[StmPrintQueueSchema.Constants.SQ_WebPrintServiceAddress].ToString();
					}
				}
			}

			return null;
		}

		byte[] GetUncompressedByteArray(byte[] rawValue)
		{
			Argument.NotNull(rawValue, nameof(rawValue));
			return Compressor.Uncompress(rawValue);
		}

		static T SafeCast<T>(object value, T defaultValue = default)
		{
			return (value is null || value == DBNull.Value) ? defaultValue : (T)value;
		}
	}
}
