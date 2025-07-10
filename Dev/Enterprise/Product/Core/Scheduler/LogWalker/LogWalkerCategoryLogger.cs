using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.LogWalker
{
	public enum LogWalkerCategories
	{
		StmJobQueueReport,
		EnvironmentLogging,
		ConcurrencyErrorReport,
		Default
	}

	public class LogWalkerCategoryLogger : ICategoryLogger<LogWalkerCategories>
	{
		readonly ILogger logger;
		readonly CodeDescriptionBoolCollection registry = SystemDataRegistry.Instance.LogWalkerLogging.Value;

		public LogWalkerCategoryLogger(ILogger logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}

		public string GetCategoryName(LogWalkerCategories category)
		{
			switch (category)
			{
				case LogWalkerCategories.StmJobQueueReport:
					return nameof(LogWalkerCategories.StmJobQueueReport);
				case LogWalkerCategories.EnvironmentLogging:
					return nameof(LogWalkerCategories.EnvironmentLogging);
				case LogWalkerCategories.ConcurrencyErrorReport:
					return nameof(LogWalkerCategories.ConcurrencyErrorReport);
				case LogWalkerCategories.Default:
					return nameof(LogWalkerCategories.Default);
				default:
					return "unknown";
			}
		}

		public string GetContext()
		{
			return "LogWalker";
		}

		public LogWalkerCategories GetDefaultCategory()
		{
			return LogWalkerCategories.Default;
		}

		public ILogger GetLogger()
		{
			return logger;
		}

		public bool ShouldLog(LogWalkerCategories category)
		{
			switch (category)
			{
				case LogWalkerCategories.StmJobQueueReport:
					return registry.GetBoolFromCode(SystemDataRegistry.LogWalkerLoggingKeys.StmJobQueueReport);
				case LogWalkerCategories.EnvironmentLogging:
					return registry.GetBoolFromCode(SystemDataRegistry.LogWalkerLoggingKeys.EnvironmentLogging);
				case LogWalkerCategories.ConcurrencyErrorReport:
					return registry.GetBoolFromCode(SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);
				case LogWalkerCategories.Default:
				default:
					return true;
			}
		}

		void ILogger.Log(LogType type, string message)
		{
			this.Log(type, message);
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			this.Log(type, message, ex);
		}
	}
}
