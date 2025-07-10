using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("ZU8", "SMS Notifications", "CSP",
	typeof(SMSNotificationsServiceTask),
	MinimumPeriod = "1minute",
	MaximumPeriod = "240minutes",
	DefaultScheduleRunEvery = "5minutes"
	)]
namespace Enterprise.Client.UPE.ServiceTask
{
	public class SMSNotificationsServiceTask : UPEServiceTask
	{
		public SMSNotificationsServiceTask()
		{
		}

		public SMSNotificationsServiceTask(ILogger logger)
			: base(logger)
		{
		}

		#region Execute

		ZDateTime HighwaterMark = ZDateTime.Empty;

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var branch = UPETools.Instance.UPECustomisationBranches(false).FirstOrDefault();
			if (branch != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					if (!SMSSender.CheckEnvironmentValid())
					{
						Notify("Task environment is not valid. Task is stopped.");
						return;
					}

					Notifications.Notify(new InfoNotification("Start Checking for Notifications"));
					try
					{
						ZDateTime from = HighwaterMark.IsEmpty ? ZDateTime.Now.AddDays(-3) : HighwaterMark;
						ZDateTime to = ZDateTime.Now;

						NotifyIfShipmentNotLoadedShortlyBeforeFlightArrival(from, to);
						NotifyIfShipmentNotPrealerted3HoursBeforeFlightArrival(from, to);
						NotifyForShipmentsWithMessagesNotReceived1HourBeforeFlight(from, to);

						HighwaterMark = to;
					}
					finally
					{
						Notifications.Notify(new InfoNotification("Finish Checking for Notifications"));
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		[SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void NotifyIfShipmentNotLoadedShortlyBeforeFlightArrival(ZDateTime from, ZDateTime to)
		{
			string sQL = string.Format(CultureInfo.InvariantCulture, @"
SELECT
  JV_VoyageFlight AS FlightNo,
  JB_E_ARV AS ScheduledArrivalDate
FROM dbo.JobSailing
INNER JOIN dbo.JobVoyOrigin ON JX_JA=JA_PK
INNER JOIN dbo.JobVoyDestination ON JX_JB=JB_PK
INNER JOIN dbo.JobVoyage ON JB_JV=JV_PK
WHERE NOT EXISTS (SELECT CM_PK
				  FROM dbo.CusMAWB
				  WHERE CM_FlightNo=JV_VoyageFlight
					AND datepart(Year,  CM_ArrivalDate)=datepart(Year,  JB_E_ARV)
					AND datepart(Month, CM_ArrivalDate)=datepart(Month, JB_E_ARV)
					AND datepart(Day,   CM_ArrivalDate)=datepart(Day,   JB_E_ARV)
					AND CM_RL_NKLoadPort=JA_RL_NKPortOfLoading
					AND CM_RL_NKDischargePort=JB_RL_NKPortOfDischarge)
  AND dateadd(minute, -{0}, JB_E_ARV) >= @From AND dateadd(minute, -{0}, JB_E_ARV) <= @To
  AND @Now > dateadd(minute, -{0}, JB_E_ARV)
  AND JB_E_ARV > dateadd(day, -3, @Now)
", UPEDataRegistry.Instance.SMSShipmentLoadDeadlineBeforeArrivalInMinutes.Value);

			DbCommand command = Db.Connection.Command(sQL);
			command.AddParameter("@From", SqlDbType.DateTime, from.ToDateTime());
			command.AddParameter("@To", SqlDbType.DateTime, to.ToDateTime());
			command.AddParameter("@Now", SqlDbType.DateTime, ZDateTime.Now.ToDateTime());

			var adapter = command.NewDataAdapter();
			using (DataSet data = new DataSet())
			{
				adapter.Fill(data);

				foreach (DataRow row in data.Tables[0].Rows)
				{
					ZDateTime scheduledArrivalDate = (DateTime)row["ScheduledArrivalDate"];
					SendSMS(string.Format(CultureInfo.InvariantCulture,
						"No masters for flight {0} have not been loaded, and is scheduled to arrive {1}",
						row["FlightNo"], scheduledArrivalDate.ToLongTimeString()));
				}
			}
		}

		void NotifyIfShipmentNotPrealerted3HoursBeforeFlightArrival(ZDateTime from, ZDateTime to)
		{
			string cusHAWBStatusFilter = GetCusHAWBStatusFilter(CMRBaseStatuses.Codes.NotSent, CMRBaseStatuses.Codes.AwaitingResponseToOriginal);
			DataTable statuses = GetShipmentStatuses(cusHAWBStatusFilter, 3, from, to);

			foreach (DataRow row in statuses.Rows)
			{
				ZDateTime scheduledArrivalDate = (DateTime)row["ScheduledArrivalDate"];
				SendSMS(string.Format(CultureInfo.InvariantCulture,
					"Master {0} has {1} of {2} shipments not pre-alerted, scheduled to arrive {3}",
					row["MAWB"], row["HAWBCount"], row["TotalHAWBCount"], scheduledArrivalDate.ToLongTimeString()));
			}
		}

		void NotifyForShipmentsWithMessagesNotReceived1HourBeforeFlight(ZDateTime from, ZDateTime to)
		{
			string cusHAWBStatusFilter = CusHAWBSchema.CS_CustomsStatus.Name + "='" + CMRBaseStatuses.Codes.NotSent + "'";
			DataTable statuses = GetShipmentStatuses(cusHAWBStatusFilter, 1, from, to);

			foreach (DataRow row in statuses.Rows)
			{
				ZDateTime scheduledArrivalDate = (DateTime)row["ScheduledArrivalDate"];
				SendSMS(string.Format(CultureInfo.InvariantCulture,
					"Master {0} has {1} of {2} shipments with no response from customs, scheduled to arrive {3}",
					row["MAWB"], row["HAWBCount"], row["TotalHAWBCount"], scheduledArrivalDate.ToLongTimeString()));
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		[SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		DataTable GetShipmentStatuses(string cusHAWBStatusFilter, int hours, ZDateTime from, ZDateTime to)
		{
			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			var mawbQuery = new ZDBOnlyQuery(typeof(UPECusMAWB));
			mawbQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

			string sQL = string.Format(CultureInfo.InvariantCulture, @"
SELECT
  min(CM_MAWB) AS MAWB,
  COUNT(*) AS HAWBCount,
  (SELECT COUNT(*) FROM dbo.CusHAWB WHERE CS_CM=CM_PK) AS TotalHAWBCount,
  min(JB_E_ARV) AS ScheduledArrivalDate
FROM dbo.JobSailing 
INNER JOIN dbo.JobVoyOrigin ON JX_JA=JA_PK
INNER JOIN dbo.JobVoyDestination ON JX_JB=JB_PK
INNER JOIN dbo.JobVoyage ON JB_JV=JV_PK
INNER JOIN dbo.CusMAWB ON CM_FlightNo=JV_VoyageFlight
				  AND datepart(Year,  CM_ArrivalDate)=datepart(Year,  JB_E_ARV)
				  AND datepart(Month, CM_ArrivalDate)=datepart(Month, JB_E_ARV)
				  AND datepart(Day,   CM_ArrivalDate)=datepart(Day,   JB_E_ARV)
				  AND CM_RL_NKLoadPort=JA_RL_NKPortOfLoading
				  AND CM_RL_NKDischargePort=JB_RL_NKPortOfDischarge
INNER JOIN dbo.CusHAWB ON CS_CM=CM_PK
WHERE ({0})
  AND {2}
  AND dateadd(hour, -{1}, JB_E_ARV) >= @From AND dateadd(hour, -{1}, JB_E_ARV) <= @To
  AND @Now > dateadd(hour, -{1}, JB_E_ARV)
  AND CM_ArrivalDate > dateadd(day, -3, @Now)
GROUP BY CM_PK
", cusHAWBStatusFilter, hours, mawbQuery.LiteralTextSqlFormatted);

			DbCommand command = Db.Connection.Command(sQL);
			command.AddParameter("@From", SqlDbType.DateTime, from.ToDateTime());
			command.AddParameter("@To", SqlDbType.DateTime, to.ToDateTime());
			command.AddParameter("@Now", SqlDbType.DateTime, ZDateTime.Now.ToDateTime());

			var adapter = command.NewDataAdapter();
			DataSet data = new DataSet();
			adapter.Fill(data);
			return data.Tables[0];
		}

		string GetCusHAWBStatusFilter(params string[] statuses)
		{
			string result = "";
			foreach (string status in statuses)
			{
				if (!String.IsNullOrEmpty(result))
				{
					result += " OR ";
				}
				result += CusHAWBSchema.CS_MsgStatus.Name + "='" + status + "'";
			}
			return result;
		}

		#endregion

		#region Implementation

		void SendSMS(string message)
		{
			SMSSender.SendSMS(message);
			Notifications.Notify(new InfoNotification("Sent SMS: " + message));
		}

		SMSSender SMSSender
		{
			get
			{
				if (fSMSSender == null)
				{
					fSMSSender = new SMSSender(Notifications);
				}
				return fSMSSender;
			}
		}
		SMSSender fSMSSender;

		#endregion
	}
}
