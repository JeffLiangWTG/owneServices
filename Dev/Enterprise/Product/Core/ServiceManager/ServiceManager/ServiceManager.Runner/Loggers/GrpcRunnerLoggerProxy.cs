using System;
using Microsoft.Extensions.Logging;
using ServiceManager.Runner.Abstractions;
using ILogger = Grpc.Core.Logging.ILogger;

namespace Enterprise.ServiceManager.Runner
{
	class GrpcLoggerProxy : IGrpcLogger
	{
		public GrpcLoggerProxy(IRunnerLogger logger)
			: this(logger, null)
		{
		}

		GrpcLoggerProxy(IRunnerLogger logger, Type type)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			grpcLogType = type;
		}

		public ILogger ForType<T>()
		{
			return new GrpcLoggerProxy(logger, typeof(T));
		}

		public void Debug(string message)
		{
			logger.Log(LogLevel.Debug, $"{GrpcLogPrefix(LogLevel.Debug)}{message}");
		}

		public void Debug(string format, params object[] formatArgs)
		{
			logger.Log(LogLevel.Debug, $"{GrpcLogPrefix(LogLevel.Debug)}{string.Format(format, formatArgs)}");
		}

		public void Error(string message)
		{
			logger.Log(LogLevel.Error, $"{GrpcLogPrefix(LogLevel.Error)}{message}");
		}

		public void Error(string format, params object[] formatArgs)
		{
			logger.Log(LogLevel.Error, $"{GrpcLogPrefix(LogLevel.Error)}{string.Format(format, formatArgs)}");
		}

		public void Error(Exception exception, string message)
		{
			logger.Log(LogLevel.Error, $"{GrpcLogPrefix(LogLevel.Error)}{message}", exception);
		}

		public void Info(string message)
		{
			logger.Log(LogLevel.Debug, $"{GrpcLogPrefix(LogLevel.Information)}{message}");
		}

		public void Info(string format, params object[] formatArgs)
		{
			logger.Log(LogLevel.Debug, $"{GrpcLogPrefix(LogLevel.Information)}{string.Format(format, formatArgs)}");
		}

		public void Warning(string message)
		{
			logger.Log(LogLevel.Debug, $"{GrpcLogPrefix(LogLevel.Warning)}{message}");
		}

		public void Warning(string format, params object[] formatArgs)
		{
			logger.Log(LogLevel.Debug, $"{GrpcLogPrefix(LogLevel.Warning)}{string.Format(format, formatArgs)}");
		}

		public void Warning(Exception exception, string message)
		{
			logger.Log(LogLevel.Debug, $"{GrpcLogPrefix(LogLevel.Warning)}{message}", exception);
		}

		string GrpcLogPrefix(LogLevel logLevel)
		{
			return grpcLogType == null ? $"Grpc severity:[{Enum.GetName(typeof(LogLevel), logLevel)}]:" : $"Grpc severity:[{logLevel}]|type:[{grpcLogType}]:";
		}

		readonly IRunnerLogger logger;
		readonly Type grpcLogType;
	}
}
