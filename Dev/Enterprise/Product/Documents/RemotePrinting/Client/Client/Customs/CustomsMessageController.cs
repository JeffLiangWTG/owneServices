using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;

namespace Enterprise.RemotePrinting.Client
{
	public abstract class CustomsMessageController : Controller, ICustomsMessageController
	{
		CancellationToken cancellationToken;
		protected CustomsMessageController(string localMachineName, CancellationToken cancellationToken) : base(null)
		{
			LocalMachineName = localMachineName;
			LoggingEnable = false;
			this.cancellationToken = cancellationToken;
		}
		string LocalMachineName { get; }

		protected abstract CustomseHubClientSettingManager CreateNewSettingManager(string machineName);

		protected override bool IsMainController => false;
		const int pauseWhenSettingIsEmptyInMinutes = 20;
		const int pauseWhenSettingIsInvalidInMinutes = 5;

		CustomseHubClientSettingManager fSettingManager;
		protected CustomseHubClientSettingManager SettingManager
		{
			get
			{
				if (fSettingManager == null)
				{
					fSettingManager = CreateNewSettingManager(LocalMachineName);
					fSettingManager.LogInformation += (o, e) => OnShowInformation(e.Message);
					fSettingManager.OnSettingDownloaded += SettingManager_OnSettingDownloaded;
				}
				return fSettingManager;
			}
		}

		void SettingManager_OnSettingDownloaded(ICustomseHubClientSetting setting)
		{
			LoggingEnable = !string.IsNullOrEmpty(setting.MachineName);
			OnSettingDownloaded(setting);
		}

		void OnSettingDownloaded(ICustomseHubClientSetting clientSetting)
		{
			var decryptor = new CustomsClientDecryptor(this, clientSetting);
			decryptor.DecryptIfNeeded();
		}

		protected override void Process()
		{
			while (!ShouldStop && !cancellationToken.IsCancellationRequested)
			{
				ProcessLoop();
			}
		}

		void ProcessLoop()
		{
			var requestPauseInSeconds = 0;
			var numberOfMessages = 0;
			try
			{
				var setting = SettingManager.CurrentSetting;
				if (setting.IsValid)
				{
					numberOfMessages = ProcessCore(setting);
					requestPauseInSeconds = GetRunningIntervalCore(setting);
				}
				else
				{
					var requestPauseInMinutes = string.IsNullOrEmpty(setting.MachineName) ? pauseWhenSettingIsEmptyInMinutes : pauseWhenSettingIsInvalidInMinutes;
					requestPauseInSeconds = requestPauseInMinutes * 60;
					SettingManager.ClearCachedSetting();
				}
			}
			catch (Exception ex)
			{
				HandleExceptionWhenLooping(ex);
			}

			if (numberOfMessages == 0)
			{
				PauseWhenRequested(requestPauseInSeconds <= 0 ? 60 : requestPauseInSeconds);
			}
		}

		protected virtual int GetRunningIntervalCore(ICustomseHubClientSetting setting) => setting.RunningIntervalInSeconds;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logs and handles Web Service Client initialization failures.")]
		protected override void InitialiseWebServiceClient()
		{
			try
			{
				base.InitialiseWebServiceClient();
			}
			catch (Exception ex)
			{
				LoggingEnable = true;
				OnShowInformation("Initializing Web Service Client failed: " + ex.Message);
				throw;
			}
		}

		protected abstract int ProcessCore(ICustomseHubClientSetting setting);

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles requested pauses in processing.")]
		protected virtual void PauseWhenRequested(int requestPauseInSeconds)
		{
			if (!ShouldStop)
			{
				OnShowInformation(string.Format(CultureInfo.InvariantCulture, "Pause {0} seconds.", requestPauseInSeconds));
				System.Threading.Thread.Sleep(requestPauseInSeconds * 1000);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles exceptions during looping operations.")]
		protected virtual void HandleExceptionWhenLooping(Exception ex)
		{
			if (!HandleServerException(ex, shouldHandleTimeout: true, shouldSleepIfProtocolError: true))
			{
				OnShowInformation("* * * Failed * * *\r\n" + GetExceptionMessageAndStacktrace(ex) + "\r\n\r\n");
			}
			SettingManager.ClearCachedSetting();
		}

		public void RefreshCustomseHubClientSetting()
		{
			SettingManager.ClearCachedSetting();
		}

		protected string GetNewTimeStampedFilePathIfNecessary(string folder, string fileName)
		{
			var newFileName = Path.Combine(folder, fileName);

			if (File.Exists(newFileName))
			{
				newFileName = Path.Combine(folder, Path.GetFileNameWithoutExtension(fileName) + "_" + TimeStamp) + Path.GetExtension(fileName);
			}

			return newFileName;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305")]
		protected virtual string TimeStamp => DateTime.Now.ToString("TimeStampFormat");

		protected string WriteToLocalAsUTF8Text(Stream stream, string directory, string fileName, string ext = ".txt")
		{
			var outputFullPath = string.Empty;

			stream.SeekBegin();
			var streamText = stream.ReadToEnd();

			if (!streamText.Equals(SpecialTestString, StringComparison.OrdinalIgnoreCase))
			{
				var fileNameWithExt = string.IsNullOrWhiteSpace(ext) || fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ext;
				outputFullPath = GetNewTimeStampedFilePathIfNecessary(directory, fileNameWithExt);

				File.WriteAllText(outputFullPath, streamText, Encoding.UTF8);
			}

			return outputFullPath;
		}

		protected string WriteToLocalAsUTF8Xml(Stream stream, string directory, string fileName, string ext = ".xml")
		{
			var fileNameWithExt = string.IsNullOrWhiteSpace(ext) || fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ext;
			var outputFullPath = GetNewTimeStampedFilePathIfNecessary(directory, fileNameWithExt);
			stream.SeekBegin();
			var document = XDocument.Load(stream);
			using (var sw = new StreamWriter(outputFullPath, false, new UTF8Encoding(false)))
			{
				document.Save(sw);
			}
			return outputFullPath;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant used for testing purposes.")]
		const string SpecialTestString = "TEST STRING";

		#region ICustomsMessageController

		WebClientConfiguration ICustomsMessageController.ConfigSetting => ConfigSetting;

		void ICustomsMessageController.ResetConfigSetting() => ResetConfigSetting();

		void ICustomsMessageController.ShowInformation(string message) => OnShowInformation(message);

		#endregion
	}
}
