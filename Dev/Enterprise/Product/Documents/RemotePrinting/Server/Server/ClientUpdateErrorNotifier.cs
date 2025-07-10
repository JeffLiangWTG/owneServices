using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using Enterprise.RemotePrinting.Server.RPSCore;
using static System.FormattableString;

namespace Enterprise.RemotePrinting.Server
{
	public class ClientUpdateErrorNotifier : INotifications
	{
		public ClientUpdateErrorNotifier(ClientInfo clientInfo, IEmailSender emailSender, DbConnection dbConnection)
		{
			this.clientInfo = clientInfo;
			this.emailSender = emailSender;
			this.dbConnection = dbConnection;
		}

		readonly ClientInfo clientInfo;
		readonly IEmailSender emailSender;
		readonly DbConnection dbConnection;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void Add(INotification notification)
		{
			if (notification.Type == NotificationType.Error && ShouldNotify())
			{
				var emailSubject = "Failed to update Remote Printing Client";
				var emailBody = new StringBuilder(Invariant($"Remote Printing Client on machine '{clientInfo.MachineName}' cannot be updated to latest version due to unsatisfied requirements:"));
				emailBody.AppendLine();
				emailBody.Append(notification.Message);
				emailBody.AppendLine();
				emailBody.Append(Invariant($"Client version : '{clientInfo.ClientVersion}'"));
				emailBody.AppendLine();

				var postmastersGroupPk = RegistryData.WebPrintNotificationGroup(dbConnection);
				emailSender.SendEmailToGroup(postmastersGroupPk, emailSubject, emailBody.ToString());

				new NotificationDateTimeRegistryItem(clientInfo.MachineName).SetLastNotificationDateTimeUtc(dbConnection);
			}
		}

		bool ShouldNotify()
		{
			return !string.IsNullOrWhiteSpace(clientInfo.MachineName) && HasPrintableQueues() && TimeFromLastNotificationSent().TotalHours > 24.0;
		}

		bool HasPrintableQueues()
		{
			const string SqlServerNameParameter = "@ServerName";
			const string Sql = "select count(*) from dbo.StmPrintQueue left join dbo.StmPrintServer on SPS_PK = SQ_SPS_Server where SQ_AllowPrinting = 1 and SPS_ServerName = " + SqlServerNameParameter;

			var queuesCount = dbConnection.ExecuteScalar<int>(Sql, command =>
			{
				command.AddParameter(SqlServerNameParameter, SqlDbType.VarChar, 128, clientInfo.MachineName);
			});

			return queuesCount > 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		TimeSpan TimeFromLastNotificationSent()
		{
			var lastNotificationDate = new NotificationDateTimeRegistryItem(clientInfo.MachineName).GetLastNotificationDateTimeUtc(dbConnection);
			return DateTime.UtcNow - lastNotificationDate;
		}

		public class NotificationDateTimeRegistryItem : Enterprise.RemotePrinting.Server.RPSCore.StringDbRegistryItem
		{
			public NotificationDateTimeRegistryItem(string machineName)
			{
				MachineName = machineName;
			}

			protected override string DefaultValue => string.Empty;

			public override string ItemName => "WebPrintUpdateNotifierDate_" + MachineName;

			string MachineName { get; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			const string Pattern = "yyyy-MM-dd HH:mm:ss";

			public DateTime GetLastNotificationDateTimeUtc(DbConnection connection)
			{
				var stringValue = LoadValue(connection);
				if (DateTime.TryParseExact(stringValue, Pattern, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTimeValue))
				{
					return dateTimeValue;
				}
				return DateTime.MinValue;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
			public void SetLastNotificationDateTimeUtc(DbConnection connection)
			{
				try
				{
					var stringValue = DateTime.UtcNow.ToString(Pattern, CultureInfo.InvariantCulture);
					SaveValue(stringValue, connection);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// Cannot update last notification date in registry - ignore
				}
			}
		}
	}
}
