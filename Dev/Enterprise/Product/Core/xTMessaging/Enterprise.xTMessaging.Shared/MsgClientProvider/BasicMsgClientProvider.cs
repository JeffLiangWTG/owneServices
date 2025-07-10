using System;
using System.Threading;
using Grpc.Core;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.Shared
{
	public class BasicMsgClientProvider : IMsgClientProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string NetworkErrorReportKey = "Direct xT Client - Network Error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string NetworkErrorMessage = "Failed to establish connection, please check with the data in Registry.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string InvalidConfigMessage = "Connection configuration is invalid, please check with the data in Registry and reference database.";

		public BasicMsgClientProvider(string connect, string caBundle, string uri, string password, TimeSpan timeout, CancellationToken token)
		{
			var config = new Configuration { Application = new Application { URI = uri, Password = password }, CA = caBundle, Connect = connect };
			ConfigurationUtils.StandardizeConfig(config);
			this.config = config;
			this.token = token;
			messageTimeout = timeout;
		}

		readonly Configuration config;
		protected GRPCInstance session;
		protected Msg.MsgClient client;
		protected readonly CancellationToken token;
		protected readonly TimeSpan messageTimeout;

		protected void InitializeIfNeeded()
		{
			try
			{
				if (config.IsValid())
				{
					session = config.LogOn(GetCallOptions());
					client = session?.stub;
				}
				else
				{
					ErrorMessage = InvalidConfigMessage;
				}
			}
			catch (NullReferenceException ex)
			{
				ErrorMessage = NetworkErrorMessage + ex.Message;
				throw new MsgServerConnectionException(NetworkErrorReportKey, ex, ErrorMessage);
			}
			catch (RpcException ex)
			{
				var exception = Utils.GetMsgServerConnectionException(nameof(config.LogOn), ex, messageTimeout);
				ErrorMessage = exception.ErrorDetail;
				throw exception;
			}
		}

		protected virtual CallOptions GetCallOptions() => new (deadline: Utils.GetDeadline(messageTimeout), cancellationToken: token);

		#region IMsgClientProvider

		public IMsgClient MsgClient
		{
			get
			{
				if (session == null && client == null)
				{
					InitializeIfNeeded();
					if (client != null)
					{
						msgClient = new BasicMsgClient(client, messageTimeout, token);
					}
				}

				return client == null ? null : msgClient;
			}
		}
		BasicMsgClient msgClient;

		public void TearDown()
		{
			if (session != null)
			{
				try
				{
					config.LogOff(session, GetCallOptions());
				}
				catch (RpcException ex)
				{
					var exception = Utils.GetMsgServerConnectionException(nameof(config.LogOff), ex, messageTimeout);
					ErrorMessage = exception.ErrorDetail;
					throw exception;
				}
			}
		}

		public string ErrorMessage { get; private set; }

		public string XtToObjFilter
		{
			get
			{
				ConfigurationUtils.StandardizeConfig(config);
				return config.Application.URI;
			}
		}

		public TimeSpan XtServerMessageTimeout => messageTimeout;

		#endregion
	}
}
