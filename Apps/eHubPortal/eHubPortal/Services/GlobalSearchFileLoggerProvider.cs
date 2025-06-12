using System.Collections.Concurrent;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Options;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace eServices.eHubPortal.Services;

[ProviderAlias("GlobalSearchFile")]
public sealed class GlobalSearchFileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
	private readonly ILog rollingFileLogger;
	private readonly Timer cleanUpTimer;
	private readonly GlobalSearchFileLoggerConfiguration config;
	private readonly ConcurrentDictionary<string, GlobalSearchFileLogger> loggers = new(StringComparer.OrdinalIgnoreCase);
	private IExternalScopeProvider? scopeProvider = null;

	public GlobalSearchFileLoggerProvider(IOptions<GlobalSearchFileLoggerConfiguration> config)
	{
		this.config = config.Value;
		rollingFileLogger = GetLogger(this.config);
		cleanUpTimer = new Timer(CleanUp, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
	}

	public ILogger CreateLogger(string categoryName)
	{
		return loggers.GetOrAdd(categoryName, name => new GlobalSearchFileLogger(categoryName, rollingFileLogger, scopeProvider));
	}

	private ILog GetLogger(GlobalSearchFileLoggerConfiguration config)
	{
		var hierarchy = (Hierarchy)LogManager.GetRepository();
		hierarchy.Configured = true;

		var appender = new RollingFileAppender
		{
			AppendToFile = true,
			File = config.File,
			Layout = new PatternLayout(),
			PreserveLogFileNameExtension = true,
			RollingStyle = RollingFileAppender.RollingMode.Date,
			StaticLogFileName = false,
			Threshold = Level.All
		};
		appender.ActivateOptions();

		var logger = LogManager.GetLogger(nameof(GlobalSearchFileLogger));
		((log4net.Repository.Hierarchy.Logger)logger.Logger).AddAppender(appender);
		return logger;
	}

	private void CleanUp(object? state)
	{
		var directory = Path.Combine(AppContext.BaseDirectory, Path.GetDirectoryName(config.File) ?? "");
		var filePrefix = Path.GetFileNameWithoutExtension(config.File);
		var fileExtn = Path.GetExtension(config.File);
		foreach (var file in Directory.EnumerateFiles(directory, $"{filePrefix}*{fileExtn}"))
		{
			if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(-config.MaxDays))
			{
				try
				{
					File.Delete(file);
				}
				catch (Exception) { }
			}
		}
	}

	public void Dispose()
	{
		cleanUpTimer.Dispose();
	}

	public void SetScopeProvider(IExternalScopeProvider scopeProvider)
	{
		this.scopeProvider = scopeProvider;

		foreach (var logger in loggers)
		{
			logger.Value.ScopeProvider = this.scopeProvider;
		}
	}
}
