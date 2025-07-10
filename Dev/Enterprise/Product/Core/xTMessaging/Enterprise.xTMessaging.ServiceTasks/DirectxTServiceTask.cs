using System;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.ServiceTasks
{
	public abstract class DirectxTServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message, Constant value - Log Message")]
		public override void RunTask(CancellationToken token)
		{
			try
			{
				ServiceLogger.Log(LogType.Information, "DirectxT Service Task start");
				RunTaskCore(token);
				if (OutageStartTime != DateTime.MinValue)
				{
					OutageStartTime = DateTime.MinValue;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var utcNow = ZDateTime.UtcNow.ToDateTime();
				if (OutageStartTime == DateTime.MinValue)
				{
					OutageStartTime = utcNow;
				}

				var connectionException = ex as MsgServerConnectionException;
				var exception = connectionException?.InnerException ?? ex;
				var errorDetail = connectionException?.ErrorDetail ?? string.Empty;
				var message = string.IsNullOrEmpty(errorDetail) ? exception.Message : errorDetail;
				if (!IsKnownException(ex) || utcNow > OutageStartTime.Add(TimeSpan.FromMinutes(DirectxTMessagingRegistry.Instance.XTServerOutageExpiryTimeInMinutes.Value)))
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>()?.Key;

					ServiceLogger.Log(LogType.Error, $@"DirectxT Service Task Error. Message: {message}");

					var fullDetails = new ZStringBuilder();
					fullDetails.AppendLine("Error Details:");
					fullDetails.AppendLine($"Error when connecting from {xTMessaging.Shared.Utils.ClientSystemHostedInfo} EnterpriseCode: {registrationKey?.EnterpriseCode} ServerCode: {registrationKey?.ServerCode} to ServerAddress: {Configuration?.Connect ?? ""} ApplicationUri {Configuration?.Application?.URI ?? ""}");
					fullDetails.AppendLine(message);
					fullDetails.AppendLine(exception.ToString());

					ServiceLogger.Log(LogType.Debug, fullDetails.ToString());

					if (IsProduction())
					{
						var xTConnectionType = DirectxTMessagingRegistry.Instance.ConnectionToXTServer.Value;
						var key = FormatExceptionKey(connectionException, exception, xTConnectionType);
						ErrorReporter.ReportOnce(key.ToString(), fullDetails.ToString(), exception);
					}
				}
				else
				{
					ServiceLogger.Log(LogType.Warning, $@"DirectxT Service Task Error. Message: {message}");
				}
			}
		}

		bool IsKnownException(Exception ex) => (ex is MsgServerConnectionException || ex is MsgSessionTimeoutException || ex is OperationCanceledException);

		ZStringBuilder FormatExceptionKey(MsgServerConnectionException connectionException, Exception exception, string xTConnectionType)
		{
			var key = new ZStringBuilder();
			key.AppendLine(connectionException?.Message ?? exception.Message);
			var match = Regex.Match(exception.ToString(), "Detail=\"([^\"]*)\", DebugException", RegexOptions.Singleline);
			if (match.Success)
			{
				key.AppendLine($"{match.Groups[1].Value} on {xTConnectionType} xT Server");
			}
			return key;
		}

		protected virtual bool IsProduction() => IsProductionDatabase();

		public static bool IsProductionDatabase()
		{
			return !Globals.IsTest
				&& !Globals.IsDebugMode
				&& ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production;
		}

		protected virtual void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			Configuration = new CW1RegistryConfigurationProvider().GetConfiguration();
			GetMessageProcessor().Process(Configuration, token);
		}

		protected abstract DateTime OutageStartTime { get; set; }

		protected abstract IInterchangeProcessor GetMessageProcessor();

		protected Configuration Configuration { get; set; }
	}
}
