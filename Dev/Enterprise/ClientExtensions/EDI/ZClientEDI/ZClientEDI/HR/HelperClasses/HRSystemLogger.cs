using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.HR
{
	public class HRSystemLogger : ILogger
	{
		public HRSystemLogger(ILogger initialLogger, string hrSystemName)
		{
			InternalLogger = initialLogger;
			Logs = new ZStringBuilder();
			HRSystemName = hrSystemName;
		}

		public void Log(LogType type, string message)
		{
			AppendLog(type, message, null);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			AppendLog(type, message, ex);
		}

		public void SendNotificationEmail()
		{
			if (HasErrorsOrWarnings)
			{
				var regItem = EDIDataRegistry.Instance.HRNotificationGroup;
				var regFullName = string.Join(" > ", regItem.Categories) + " > " + regItem.Caption;
				var mail = new EmailDef()
				{
					Subject = $"ediProd {HRSystemName} sync failed",
					Body = Logs.ToString() + System.Environment.NewLine + System.Environment.NewLine
							+ "You are receiving this email because you are a member of the group in registry " + regFullName
				};
				try
				{
					Env.OutgoingMailManager.CreateAndSave(mail, regItem.Value, GroupSourceLocator.GetFromRegistryItem(regItem));
				}
				catch (EmailSendFailedException)
				{
				}
			}
		}

		public override string ToString() => Logs.ToString();

		void AppendLog(LogType type, string message, Exception ex)
		{
			if (type == LogType.Error || type == LogType.Warning)
			{
				HasErrorsOrWarnings = true;
			}
			if (ex != null)
			{
				InternalLogger?.Log(type, message, ex);
				Logs.AppendLine($"[{ZDateTime.UtcNow.ToLongTimeString()}] {type.ToString()} {message} {ex.ToString()}");
			}
			else
			{
				InternalLogger?.Log(type, message);
				Logs.AppendLine($"[{ZDateTime.UtcNow.ToLongTimeString()}] {type.ToString()} {message}");
			}
		}

		readonly ILogger InternalLogger;
		readonly ZStringBuilder Logs;
		readonly string HRSystemName;
		bool HasErrorsOrWarnings;
	}
}
