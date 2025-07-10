using System;
using System.Diagnostics.CodeAnalysis;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Enterprise.DbBackupAndRestore.log4net.config")]

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class Logger
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Read-only and thread-safe")]
		static readonly Lazy<Logger> instance = new Lazy<Logger>();

		public static Logger Instance => instance.Value;

		public ILog logger;
		ILog GetLogger()
		{
			return logger ?? (logger = LogManager.GetLogger(typeof(Logger)));
		}

		public void LogMessage(string message)
		{
			GetLogger()?.Info(message);
			Console.WriteLine(message);
		}

		public void LogErrorMessage(string message)
		{
			GetLogger()?.Error(message);
			Console.Error.WriteLine(message);
		}
	}
}