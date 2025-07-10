using System;
using System.Threading;
using Enterprise.Integration;
using Enterprise.xTMessaging.Shared;

namespace Enterprise.RemotePrinting.Client
{
	public static class CLSMSHelper
	{
		public static BasicMsgClientProvider GetMsgClientProvider(ICLSMSClientApplicationSetting clsmsSetting)
		{
			return new BasicMsgClientProvider(
				clsmsSetting.ServerAddress,
				clsmsSetting.CertificateForTheServer,
				clsmsSetting.ApplicationNodeName,
				clsmsSetting.ApplicationNodePassword,
				TimeSpan.FromSeconds(clsmsSetting.RunningIntervalInSeconds), CancellationToken.None);
		}
	}

	/// <summary>
	/// CL will create a new work item to handle this.
	/// We need to add three register items.
	/// </summary>
	public class CLDirectxTMessagingConfig : IDirectxTMessagingConfig
	{
		public double XTIdleConnectionKeepAliveInSecondsValue
		{
			get { return 60; }
		}

		public double XTIdleConnectionRetryPauseInSecondsValue
		{
			get { return 15; }
		}

		public int InterchangeCountPerBatchOnReceivingValue
		{
			get { return 100; }
		}

		public int XTServerMessageChunkSizeWhenSendingValue
		{
			get { return 32; }
		}
	}

	public class XTLogger : ILogger
	{
		public XTLogger(ShowMessage showMessage)
		{
			this.showNotificationMessage = showMessage;
		}
		readonly ShowMessage showNotificationMessage;
		public delegate void ShowMessage(string messageText);

		public void Log(LogType type, string message)
		{
			if (showNotificationMessage != null)
			{
				showNotificationMessage(message);
			}
		}

		public void Log(LogType type, string message, Exception ex)
		{
			if (showNotificationMessage != null)
			{
				showNotificationMessage(message + ex.ToString());
			}
		}
	}
}
