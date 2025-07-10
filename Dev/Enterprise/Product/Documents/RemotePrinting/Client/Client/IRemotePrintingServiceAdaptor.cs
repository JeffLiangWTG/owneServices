using System;
using System.Collections.Generic;
using Enterprise.RemotePrinting.Client.RemotePrintServer;

namespace Enterprise.RemotePrinting.Client
{
	public interface IRemotePrintingServiceAdaptor
	{
		bool IsSupportUser { get; }
		void SetWebServiceUrlAndCredentials(WebClientConfiguration config, Action<string> onShowInformation = null);
		ClientUpdate CheckClientUpdate();
		ClientUpdate CheckClientUpdate2();
		ServerPrintQueue[] GetChangedQueues(string serverName, string[] changedQueueNames);
		ServerPrintJobEx[] GetJobsCompressed(string serverName);
		ServerPrintJobEx[] GetJobsCompressed2(string serverName);
		ServerWatermark GetWatermarkInfo();
		void SetJobFailure(PrintJobFailed[] processedPrintJobs);
		void SetJobSuccess(Guid[] processedPrintJobPks);
		void SetQueues(string serverName, string[] printQueueNames);
		void SetQueuesEx(string serverName, PrintQueueInfo[] printQueueNames);
		void SendNotificationEmail(string subject, string body);
		CNSWClientSetting GetCNSWClientApplicationSetting(string machineName);
		TWNCATKClientSetting GetTWNCATKClientSetting(string machineName);
		CLSMSClientSetting GetCLSMSClientSetting(string machineName);
		JPNACCSClientSetting GetJPNACCSClientSetting(string machineName);
		void UpdateClientLogs(string recipientEmail, string fileName, byte[] fileData, string comments);
		HashSet<string> ExecutedSuccessfullyOperations { get; }
		IErrorResponseWebRequestProcessor ResponseProcessor { get; }
	}
}
