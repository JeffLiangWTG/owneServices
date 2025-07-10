using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using CargoWise.Data;
using Enterprise.RemotePrinting.Server.JobPrinting;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server
{
	[WebService(Namespace = "http://www.cargowise.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ScriptService]
	public class RemotePrintingService : WebService
	{
		public RemotePrintingSoapHeader Header { get; set; }

		public RemotePrintingService()
		{
			Server.ScriptTimeout = JobWaiter.WaitingTimeoutInSeconds * 2;
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string HelloPrinter()
		{
			return "Hello Printer!";
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Outdated remote printing client error")]
		public ClientUpdate CheckClientUpdate()
		{
			const string AlphaClientVersion = "2.137.0"; // Starting with this version Client should use API method CheckClientUpdate2(ClientInfo clientInfo)
			const string OutdatedRemotePrintingClientError = "There is a Print Server with outdated RemotePrinting Client application that should be manually re-installed using current installation distributive.";

			var clientInfo = new ClientInfo
			{
				ClientVersion = Header?.VersionInfo,
				OSVersion = Header?.OSVersion,
				DotNetVersion = Header?.InstalledDotNetVersion,
				MachineName = Header?.LocalMachineName,
			};

			var clientVersion = string.IsNullOrEmpty(clientInfo?.ClientVersion) ? new Version(0, 0) : new Version(clientInfo.ClientVersion);
			var alphaVersion = new Version(AlphaClientVersion);
			if (clientVersion >= alphaVersion)
			{
				return new ClientUpdate
				{
					Version = UpdateChecker.UpdateErrorVersion,
					Link = OutdatedRemotePrintingClientError
				};
			}
			else
			{
				return GetClientUpdate(clientInfo);
			}
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public ClientUpdate CheckClientUpdate2(ClientInfo clientInfo)
		{
			return GetClientUpdate(clientInfo, true);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		ClientUpdate GetClientUpdate(ClientInfo clientInfo, bool reportMissingClientInfo = false)
		{
			const string hostParamPrefix = "host:";
			var installationFile = AppSettingsHelper.ClientInstallationFile;
			var installationFolder = AppSettingsHelper.ClientInstallationFolder;

			var localFolder = Server.MapPath("~/" + installationFolder);
			var webFolder = HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/" + installationFolder.TrimStart('/');

			var url = HttpContext.Current.Request.Url;
			var uriBuilder = new UriBuilder(url.Scheme, url.Host, url.Port, webFolder);

			var clientRequirements = new ClientRequirements
			{
				OSMinVersion = AppSettingsHelper.ClientRequirementOS,
				DotNetMinVersion = AppSettingsHelper.ClientRequirementDotnet,
				IntermediateInstallationFile = AppSettingsHelper.ClientIntermediateInstallationFile,
			};

			using (var connection = DbHelper.NewConnection())
			{
				var webServiceAddress = GetCurrentWebServiceAddress(url, connection);
				var serviceAddressParts = webServiceAddress.Split('|');
				var ipUrlString = serviceAddressParts.First().TrimEnd('/');
				if (Uri.IsWellFormedUriString(ipUrlString, UriKind.Absolute))
				{
					var ipUrl = new Uri(ipUrlString);
					var hostName = serviceAddressParts.FirstOrDefault(part => part.StartsWith(hostParamPrefix))?.Substring(hostParamPrefix.Length);
					uriBuilder = new UriBuilder(ipUrl.Scheme, hostName ?? url.Host, ipUrl.Port, webFolder);
				}

				var emailAddress = RegistryData.SMTPDefaultReturnEmailAddress(connection);
				var emailSender = new EmailSender(connection, emailAddress);
				var notifier = new ClientUpdateErrorNotifier(clientInfo, emailSender, connection);

				return GetClientUpdate(localFolder, installationFile, uriBuilder.ToString(), clientRequirements, notifier, clientInfo, reportMissingClientInfo);
			}
		}

		protected virtual string GetCurrentWebServiceAddress(Uri contextUrl, DbConnection connection) => ServerHelper.GetCurrentWebServiceAddress(contextUrl, connection);

		protected virtual ClientUpdate GetClientUpdate(string folder, string installation, string url, ClientRequirements clientRequirements, ClientUpdateErrorNotifier notifier, ClientInfo clientInfo, bool reportMissingClientInfo = false)
		{
			return new UpdateChecker(folder, installation, url, clientRequirements, notifier).CheckUpdate(clientInfo, HttpContext.Current, reportMissingClientInfo);
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public bool Nudge(string serverName, string printQueueName, Guid printJobPK)
		{
			using (var connection = DbHelper.NewConnection())
			{
				return RemoteHub.Controller.Nudge(serverName, printQueueName, printJobPK, connection);
			}
		}

		[WebMethod]
		[ScriptMethod]
		[ExceptionHandling]
		public bool Nudge2(string serverName, string printQueueName, string printJobPK)
		{
			if (Guid.TryParse(printJobPK, out var printJobPKGuid))
			{
				using (var connection = DbHelper.NewConnection())
				{
					return RemoteHub.Controller.Nudge(serverName, printQueueName, printJobPKGuid, connection);
				}
			}
			else
			{
				return false;
			}
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public void SetQueues(string serverName, List<string> printQueueNames)
		{
			if (IsCurrentUserSupportUser())
			{
				return;
			}
			NewPrintServer().SetPrintQueues(serverName, printQueueNames);
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public void SetQueuesEx(string serverName, List<PrintQueueInfo> printQueues)
		{
			if (IsCurrentUserSupportUser())
			{
				return;
			}
			NewPrintServer().SetPrintQueuesEx(serverName, printQueues);
		}

		protected virtual bool IsCurrentUserSupportUser()
		{
			return Authentication.IsSupportUser(HttpContext.Current?.User?.Identity?.Name ?? string.Empty);
		}
		protected virtual PrintServer NewPrintServer() => new PrintServer();

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public void SetJobSuccess(List<Guid> processedPrintJobPks)
		{
			if (IsCurrentUserSupportUser())
			{
				return;
			}
			NewPrintServer().SetPrintJobSuccess(processedPrintJobPks);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Legacy code, kept for backwards compatibility")]
		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		[Obsolete("Legacy code, kept for backwards compatibility")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void SetJobFailure(XmlNode processedPrintJobWithFailureReason = null, List<Guid> processedPrintJobPks = null)
		{
			if (IsCurrentUserSupportUser())
			{
				return;
			}

			var failures = CollectFailures(processedPrintJobWithFailureReason).ToList();
			if (processedPrintJobPks != null)
			{
				failures.AddRange(processedPrintJobPks.Select(pk => new PrintJobFailed(pk, "Unknown reason - please upgrade WebPrintClient for more details.")));
			}

			SetJobFailureV2(failures);
		}

		IEnumerable<PrintJobFailed> CollectFailures(XmlNode node)
		{
			if (node == null)
			{
				yield break;
			}
			else if (node.Name == "PrintJobFailed" || node.Name == "PrintJobFailedTupleOfGuidString")
			{
				yield return new PrintJobFailed(Guid.Parse(node["JobPk"].InnerText), node["FailureReason"].InnerText);
			}
			else if (node.HasChildNodes)
			{
				foreach (XmlNode child in node.ChildNodes)
				{
					foreach (var job in CollectFailures(child))
					{
						yield return job;
					}
				}
			}
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public void SetJobFailureV2(List<PrintJobFailed> processedPrintJobWithFailureReason)
		{
			if (IsCurrentUserSupportUser())
			{
				return;
			}
			NewPrintServer().SetPrintJobFailure(processedPrintJobWithFailureReason);
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public ServerWatermark GetWatermarkInfo()
		{
			return NewPrintServer().GetWatermark();
		}

		[WebMethod]
		public IAsyncResult BeginGetJobs(string serverName, AsyncCallback cb, object s)
		{
			if (IsCurrentUserSupportUser())
			{
				var result = PrintJobAsyncResult.Synchronous(new List<ServerPrintJob>());
				cb.Invoke(result);
				return result;
			}
			return NewPrintServer().BeginGetPrintJobs<ServerPrintJob>(serverName, cb);
		}

		[WebMethod]
		public List<ServerPrintJob> EndGetJobs(IAsyncResult call)
		{
			if (IsCurrentUserSupportUser())
			{
				return new List<ServerPrintJob>();
			}
			return NewPrintServer().EndGetPrintJobs<ServerPrintJob>(call);
		}

		[WebMethod]
		public IAsyncResult BeginGetJobsCompressed(string serverName, AsyncCallback cb, object s)
		{
			if (IsCurrentUserSupportUser())
			{
				var result = PrintJobAsyncResult.Synchronous(new List<ServerPrintJobEx>());
				cb.Invoke(result);
				return result;
			}
			return NewPrintServer().BeginGetPrintJobs<ServerPrintJobEx>(serverName, cb);
		}

		[WebMethod]
		public List<ServerPrintJobEx> EndGetJobsCompressed(IAsyncResult call)
		{
			if (IsCurrentUserSupportUser())
			{
				return new List<ServerPrintJobEx>();
			}
			return NewPrintServer().EndGetPrintJobs<ServerPrintJobEx>(call);
		}

		[WebMethod]
		public List<ServerPrintJobEx> GetJobsCompressed2(string serverName)
		{
			if (IsCurrentUserSupportUser())
			{
				return new List<ServerPrintJobEx>();
			}
			return NewPrintServer().GetPrintJobs<ServerPrintJobEx>(serverName);
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public List<ServerPrintQueue> GetChangedQueues(string serverName, List<string> changedQueueNames)
		{
			if (IsCurrentUserSupportUser())
			{
				return new List<ServerPrintQueue>();
			}
			return NewPrintServer().GetChangedPrintQueues(serverName, changedQueueNames);
		}

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public CLSMSClientSetting GetCLSMSClientSetting(string machineName)
		{
			if (IsCurrentUserSupportUser())
			{
				return CLSMSClientSetting.Empty;
			}

			return NewCLSMSServer().GetSettingByMachineName(machineName);
		}

		protected virtual CLSMSServer NewCLSMSServer() => new CLSMSServer();

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public CNSWClientSetting GetCNSWClientSetting(string machineName)
		{
			if (IsCurrentUserSupportUser())
			{
				return CNSWClientSetting.Empty;
			}
			return NewCNSWServer().GetSettingByMachineName(machineName);
		}

		protected virtual CNSWServer NewCNSWServer() => new CNSWServer();

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public TWNCATKClientSetting GetTWNCATKClientSetting(string machineName)
		{
			if (IsCurrentUserSupportUser())
			{
				return TWNCATKClientSetting.Empty;
			}
			return NewTWNCATKServer().GetSettingByMachineName(machineName);
		}
		protected virtual TWNCATKServer NewTWNCATKServer() => new TWNCATKServer();

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public JPNACCSClientSetting GetJPNACCSClientSetting(string machineName)
		{
			return IsCurrentUserSupportUser() ? JPNACCSClientSetting.Empty : NewJPNACCSServer().GetSettingByMachineName(machineName);
		}

		protected virtual JPNACCSServer NewJPNACCSServer() => new JPNACCSServer();

		[WebMethod]
		[ExceptionHandling]
		[SoapHeader(nameof(Header))]
		public void SendNotificationEmail(string subject, string body)
		{
			if (IsCurrentUserSupportUser())
			{
				return;
			}
			NewPrintServer().SendNotificationEmail(subject, body);
		}

		[WebMethod]
		public void UpdateClientLogs(string recipientEmail, string fileName, byte[] fileData, string comments)
		{
			NewPrintServer().SendLogsFilesEmail(recipientEmail, fileName, fileData, comments);
		}
	}

	public class RemotePrintingSoapHeader : SoapHeader
	{
		public string VersionInfo { get; set; }

		public string OSVersion { get; set; }

		public string InstalledDotNetVersion { get; set; }

		public string LocalMachineName { get; set; }
	}
}
