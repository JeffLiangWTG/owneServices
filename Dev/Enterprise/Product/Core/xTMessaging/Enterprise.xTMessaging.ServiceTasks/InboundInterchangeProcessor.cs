using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.ServiceTasks
{
	public class InboundInterchangeProcessor : IInterchangeProcessor
	{
		public InboundInterchangeProcessor(ILogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
		}

		protected readonly ILogger logger;

		void IInterchangeProcessor.Process(Configuration configuration, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			ProcessCore(configuration, token);
		}

		void ProcessCore(Configuration configuration, CancellationToken token)
		{
			if (!CheckIsEnabled())
			{
				return;
			}

			using (var connector = new DirectxTConnector(GetMsgClientProvider(configuration, token), new Cw1DirectxTMessagingConfig(), logger, null,
				receiveHandler: GetSaveToEDIInterchangeHandler()))
			{
				if (!connector.InitializeWithFullLogging())
				{
					return;
				}

				var sw = Stopwatch.StartNew();
				logger.Log(LogType.Debug, "Start Receive Message(s).");
				connector.Receive(token);
				sw.Stop();
				logger.Log(LogType.Debug, FormattableString.Invariant($"Finish Receive Message(s). Total Time: {sw.Elapsed.TotalSeconds} second(s)"));
			}
		}

		protected virtual IMsgClientProvider GetMsgClientProvider(Configuration config, CancellationToken token) => new MsgClientProvider(config, token);

		protected virtual IReceiveHandler GetSaveToEDIInterchangeHandler() => new SaveToEDIInterchangeHandler(logger);

		protected virtual bool CheckIsEnabled()
		{
			if (!DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.Value)
			{
				logger.Log(LogType.Debug, "Inbound Processing is currently disabled as this system has not sent any requests to xT in the past month.");
				return false;
			}

			return true;
		}
	}
}
