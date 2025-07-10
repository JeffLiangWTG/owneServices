using System;
using System.Globalization;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class TransferRuleRunnerLogger : ITransferRuleRunnerLogger
	{
		public TransferRuleRunnerLogger(ILogger logger, IPAVESystem system = null)
		{
			this.logger = logger;
			SystemName = system?.Name;
		}

		readonly ILogger logger;
		protected string SystemName { get; set; }

		public void Log(string message)
		{
			Log(LogType.Information, message);
		}

		public void Log(LogType type, string message)
		{
			logger.Log(type, GetFormattedMessage(message));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			logger.Log(type, GetFormattedMessage(message), ex);
		}

		protected virtual string GetFormattedMessage(string message)
		{
			var systemNamePrefix = SystemName != null ? $"{SystemName}: " : string.Empty;
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}", systemNamePrefix, message);
		}
	}
}
