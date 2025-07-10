using System;
using System.IO;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.ELearningDocument
{
	public interface IMyAccountClient
	{
		byte[] DownloadFile(string path);
		string UrlToNetworkPath(string url);
	}
	public class MyAccountShareFolderClient : IMyAccountClient
	{
		readonly ILogger logger;
		public MyAccountShareFolderClient(ILogger logger)
		{
			this.logger = logger;
		}

		public byte[] DownloadFile(string path)
		{
			byte[] bytes = null;
			try
			{
				logger?.Log(LogType.Information, $"start {nameof(DownloadFile)}, path:{path}");
				if (string.IsNullOrWhiteSpace(path))
				{
					var message = $"unable to download a file, path is empty:{path}";
					logger?.Log(LogType.Warning, message);
					return null;
				}

				if (string.IsNullOrWhiteSpace(EDIDataRegistry.Instance.ELearningDocumentNetworkShareUser.Value))
				{
					var message =
						$"unable to download a file, EDIDataRegistry.Instance.ELearningDocumentNetworkShareUser is empty, please set value";
					logger?.Log(LogType.Warning, message);
					return null;
				}

				if (string.IsNullOrWhiteSpace(EDIDataRegistry.Instance.ELearningDocumentNetworkSharePassword.Value))
				{
					var message =
						$"unable to download a file, EDIDataRegistry.Instance.ELearningDocumentNetworkSharePassword is empty, please set value";
					logger?.Log(LogType.Warning, message);
					return null;
				}

				NetworkShare.DisconnectFromShare(path, true);
				NetworkShare.ConnectToShare(path,
					EDIDataRegistry.Instance.ELearningDocumentNetworkShareUser.Value,
					EDIDataRegistry.Instance.ELearningDocumentNetworkSharePassword.Value);
				if (!File.Exists(path))
				{
					logger?.Log(LogType.Information, $"file not found:{path}");
					NetworkShare.DisconnectFromShare(path, false);
					return null;
				}

				bytes = File.ReadAllBytes(path);
			}
			finally
			{
				NetworkShare.DisconnectFromShare(path, false);
			}
			return bytes;
		}

		public string UrlToNetworkPath(string url)
		{
			var prefix = EDIDataRegistry.Instance.ELearningDocumentNetworkSharePdfPathLocationPrefix.Value;
			if (string.IsNullOrWhiteSpace(prefix))
			{
				var message =
					$"unable to download a file, EDIDataRegistry.Instance.ELearningDocumentNetworkSharePdfPathLocationPrefix is empty, please set value";
				logger?.Log(LogType.Warning, message);
				ErrorReporter.ReportOnce(message);
				return null;
			}
			var myAccountUrl = EDIDataRegistry.Instance.ELearningDocumentMyAccountUrl.Value;
			if (string.IsNullOrWhiteSpace(myAccountUrl))
			{
				var message =
					$"unable to download a file, EDIDataRegistry.Instance.ELearningDocumentMyAccountUrl.Value is empty, please set value";
				logger?.Log(LogType.Warning, message);
				ErrorReporter.ReportOnce(message);
				return null;
			}
			return Uri.UnescapeDataString(url).ToLower().Replace(myAccountUrl, prefix).Replace("/", "\\");
		}
	}
}
