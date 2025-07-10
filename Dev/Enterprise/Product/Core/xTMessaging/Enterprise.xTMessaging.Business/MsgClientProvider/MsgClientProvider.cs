using System;
using System.Threading;
using CargoWise.Common;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Grpc.Core;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.Business
{
	public class MsgClientProvider : IMsgClientProvider, IXtMessageEventsReaderClientProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string NetworkErrorReportKey = "Direct xT Client - Network Error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string NetworkErrorMessage = "Failed to establish connection, please check with the data in Registry.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string InvalidConfigMessage = "Connection configuration is invalid, please check with the data in Registry and reference database.";

		public MsgClientProvider(Configuration config, CancellationToken token)
		{
			Argument.NotNull(config, nameof(config));
			ConfigurationUtils.StandardizeConfig(config);
			this.config = config;
			this.token = token;
		}

		readonly Configuration config;
		internal GRPCInstance session;
		Msg.MsgClient client;
		CancellationToken token;
		TimeSpan messageTimeout;

		void InitializeIfNeeded()
		{
			try
			{
				if (config.IsValid())
				{
					messageTimeout = TimeSpan.FromSeconds(DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value);

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
				var exception = Shared.Utils.GetMsgServerConnectionException(nameof(config.LogOn), ex, messageTimeout);
				ErrorMessage = exception.ErrorDetail;
				throw exception;
			}
		}

#if DEBUG
		protected virtual
#endif
		CallOptions GetCallOptions() => new CallOptions(deadline: Utils.GetDeadline(messageTimeout), cancellationToken: token);

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
						msgClient = new MsgClientWithDeadline(client, token, messageTimeout);
					}
				}

				return client == null ? null : msgClient;
			}
		}
		MsgClientWithDeadline msgClient;

		public IXtMessageEventsReaderClient XtMessageEventsReaderClient => MsgClient is IXtMessageEventsReaderClient readerClient ? readerClient : null;

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
					var exception = Shared.Utils.GetMsgServerConnectionException(nameof(config.LogOff), ex, messageTimeout);
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
