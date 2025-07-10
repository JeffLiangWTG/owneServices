using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck
{
	abstract public class FailedEDIInterchangeCheckJob : ServiceTaskHealthCheckJob
	{
		protected FailedEDIInterchangeCheckJob(string serviceTaskName, string serviceTaskCode, INotifications notifier) : base(serviceTaskName, serviceTaskCode, notifier)
		{ }

		/*
		 * If notification period is minimum (5mins) the check will be performed every 5mins.
		 *		Min(TimeInterval) = 5
		 * If the service task has not run for a period of time greater than 30hrs, the check should be performed no further back than (NOW – 24hrs).
		 *		If LastCheckTime >= 30 hours, LastCheckedTime = LastReportedTime = UTCNow - 24 hours.
		 * If notification period is greater than 24hrs the check will be performed every 24hours.
		 *		If TimeInterval >= 24 and LastCheckTime >= 24 hours and LastCheckTime < 30 hours, CheckRange >= LastCheckedTime and CheckRange < LastCheckedTime + 24 hours
		 * If notification period is greater than the minimum (5mins) and less than 24hrs, the check will be performed on the same interval.
		 *		If TimeInterval < 24 hours and LastCheckTime > 5 mins, CheckRange >= LastCheckedTime and CheckRange < LastCheckedTime + TimeInterval
		 * If notification is disabled, the period should be treated as same as the period over 24 hours. The check will be performed every 24hours.
		 *		Disabled Notification option = TimeInterval >= 24 and LastCheckTime >= 24 hours and LastCheckTime < 30 hours option.
		 * The check time is dynamic which equals LastCheckedTime + 24 hrs or LastCheckedTime + TimeInterval in upper conditions.
		 * The time is floored down from the last minute.
		 */
		public override void Notify(IHealthCheckResult result)
		{
			NotifyVerbose(Res.GetString("61d59f3d-2411-4cd8-a628-7aebbd676158", "Notification Frequency: {0}.", HealthCheckConstants.NotificationFrequencyConstants.Periodically));
			if (FrequencySetting == HealthCheckConstants.NotificationFrequencyConstants.Disable)
			{
				AddInfo(GetNotificationNotSentLog());
			}
			else
			{
				Notify(result, LastReportedTimeForFailedEDIInterchange);
			}
			LastReportedTimeForFailedEDIInterchange = LastCheckedTimeForFailedEDIInterchange;
			((IRegistryItemInternals)FailedEDIInterchangesCountInPeriod).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		string GetNotificationNotSentLog()
		{
			return Res.GetString("3b5973f8-ad3d-44fd-9018-2a45fa00d179", "Notification is configured to DISABLED. To change this setting go to {0}.", Utils.GetFullPath(NotificationFrequencyRegistryItem));
		}

		public override bool ReadyToPerformCheck()
		{
			var currentTime = GetSmallDateTimeUtcNowFloor();
			var lastCheckedTimeForFailedEDIInterchange = LastCheckedTimeForFailedEDIInterchange;
			if (!lastCheckedTimeForFailedEDIInterchange.IsValid || !LastReportedTimeForFailedEDIInterchange.IsValid)
			{
				AddInfo(Res.GetString("bd52d16f-4ecb-4ff8-9da7-c57affbb7853", "First time running Failed EDI Interchange Health Check. Record a starting time."));
				LastCheckedTimeForFailedEDIInterchange = currentTime;
				LastReportedTimeForFailedEDIInterchange = currentTime;
				((IRegistryItemInternals)FailedEDIInterchangesCountInPeriod).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
				return false;
			}
			// If the service task has not run for a period of time greater than 30hrs, the check should be performed no further back than (NOW – 24hrs).
			else if (lastCheckedTimeForFailedEDIInterchange.AddDays(1).AddHours(6) < currentTime)
			{
				var newStartingPoint = currentTime.AddDays(-1);
				AddInfo(Res.GetString("a838ab6f-d3f5-4a57-a6cd-fc486481a972", "Failed EDI Interchange Health Check hasn't run from {0} to {1}. Record a new starting time at {2}.", lastCheckedTimeForFailedEDIInterchange, currentTime, newStartingPoint));
				LastCheckedTimeForFailedEDIInterchange = newStartingPoint;
				LastReportedTimeForFailedEDIInterchange = newStartingPoint;
				((IRegistryItemInternals)FailedEDIInterchangesCountInPeriod).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
				return true;
			}

			return NextCheckTimeUtc <= currentTime;
		}

		ZDateTime NextCheckTimeUtc => LastCheckedTimeForFailedEDIInterchange.AddMinutes(ScheduledIntervalInMinute);

		int ScheduledIntervalInMinute
		{
			get
			{
				if (FrequencySetting == HealthCheckConstants.NotificationFrequencyConstants.Disable || IntervalInMinute == 0 || IntervalInMinute > ADayInMinute)
				{
					return ADayInMinute;
				}

				return IntervalInMinute;
			}
		}

		public override int ServiceTaskPeriodInMinute => Math.Min(ServiceTaskPeriodInMinuteMax, ScheduledIntervalInMinute);

		public override ZDateTime ServiceTaskNextRunTimeUtc
		{
			get
			{
				var nextCheckTimeUtc = NextCheckTimeUtc;
				var currentTimeUtc = GetSmallDateTimeUtcNowFloor();
				var minimumNextRuntimeUtc = currentTimeUtc.AddMinutes(ServiceTaskPeriodInMinuteMin);

				if (nextCheckTimeUtc < minimumNextRuntimeUtc)
				{
					return minimumNextRuntimeUtc;
				}

				var maximumNextRuntimeUtc = currentTimeUtc.AddMinutes(ServiceTaskPeriodInMinuteMax);
				if (nextCheckTimeUtc > maximumNextRuntimeUtc)
				{
					return maximumNextRuntimeUtc;
				}

				return nextCheckTimeUtc;
			}
		}

		public override IHealthCheckResult Check()
		{
			var lastCheckedTimeBeforePerformCheck = LastCheckedTimeForFailedEDIInterchange;
			var scheduledCheckTime = NextCheckTimeUtc;
			var result = IntervalIsLargerThanADay
				? new FailedEDIInterchangeHealthCheckResult(FailedEDIInterchangesCountInPeriod.GetValueWithoutFallback(Guid.Empty, Guid.Empty,
					Guid.Empty))
				: new FailedEDIInterchangeHealthCheckResult();
			var foundAnyRecord = false;
			foreach (DynamicBusinessObject dynamicObject in GetQueryResults(lastCheckedTimeBeforePerformCheck, scheduledCheckTime))
			{
				var companyPK = Guid.Parse(dynamicObject[GlbCompanySchema.Constants.PK].ToString());
				var companyCode = dynamicObject[GlbCompanySchema.Constants.GC_Code].ToString();
				var count = int.Parse(dynamicObject[CountEDIInterchangePkColumnName].ToString(), CultureInfo.InvariantCulture);

				result.Add(companyPK, count);
				AddWarning(
					LogMessages.FailedInterchangesReportMessageForCompany(companyCode, count, lastCheckedTimeBeforePerformCheck,
						scheduledCheckTime));
				foundAnyRecord = true;
			}

			LastCheckedTimeForFailedEDIInterchange = scheduledCheckTime;

			if (foundAnyRecord && IntervalIsLargerThanADay)
			{
				FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, result.Serialize());
				// If notification period is greater than 24hrs the check will be performed every 24hours. The check should return empty to not call notify.
				if (scheduledCheckTime < LastReportedTimeForFailedEDIInterchange.AddMinutes(IntervalInMinute))
				{
					result = FailedEDIInterchangeHealthCheckResult.Empty;
				}
			}
			else if (!foundAnyRecord)
			{
				NotifyVerbose(Res.GetString("303a5e21-f1c9-4308-a95d-ef3517d031bf",
					"0 failed Interchange found between {0} and {1}.", lastCheckedTimeBeforePerformCheck, scheduledCheckTime));
			}

			return result;
		}

		bool IntervalIsLargerThanADay => IntervalInMinute > ADayInMinute;

		DynamicBusinessObjectCollection GetQueryResults(ZDateTime start, ZDateTime end)
		{
			var dynamicCollection = new DynamicBusinessObjectCollection(Factory);
			var queryParams = new ZSqlParameterCollection
			{
				{ (NoResString)"@start", start.ToDateTime(), EDIInterchangeSchema.EI_SystemLastEditTimeUtc },
				{ (NoResString)"@end", end.ToDateTime(), EDIInterchangeSchema.EI_SystemLastEditTimeUtc },
				{ "@transportType", TransportType, EDIInterchangeSchema.EI_TransportType },
				{ "@receiveTransmit", EDIInterchange.Direction.Transmit, EDIInterchangeSchema.EI_ReceiveTransmit },
				{ "@statusCode", FailedEDIInterchangesStatusCode, EDIInterchangeSchema.EI_Status }
			};

			dynamicCollection.Load(SqlFilterFailedEDIInterchangesInPeriod, queryParams);

			return dynamicCollection;
		}

		internal virtual ZDateTime GetSmallDateTimeUtcNowFloor()
		{
			return ZDateTime.UtcNow.ToSmallDateTimeFloor(); // See TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Check_Periodically_FloorDownUtcNow_ProcessInterchangesWithFloorDownEI_SystemLastEditTimeUtc test case for sample scenario and explanation.
		}

		void Notify(IHealthCheckResult result, ZDateTime lastReportedPointOfTime)
		{
			foreach (var entry in ((FailedEDIInterchangeHealthCheckResult)result).FailedEDIInterchangesCountInPeriodPerCompany)
			{
				var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.Equal, entry.Key));
				if (!ReportFailuresInEmail)
				{
					ExceptionReporter.Instance.ReportDeveloperExceptionOrHandleSilently((NoResString)"Failed Interchange Health Check Report", new Exception("Developer generated exception - Failed Interchange Health Check Report"));
				}
				else
				{
					try
					{
						var currentCheckTime = LastCheckedTimeForFailedEDIInterchange;
						var emailBuilder = company == null ? new eServicesHealthCheckHtmlEmailBuilder(NotificationGroup, ServiceTaskName, HealthCheckName)
																				: new eServicesHealthCheckHtmlEmailBuilder(NotificationGroup, company.PK.ToGuid(), ServiceTaskName, HealthCheckName);
						var companyIdentifier = (company != null) ? company.GC_Name.ToString() : entry.Key.ToString();
						emailBuilder.SetSubject(Res.GetString("8fbfb41d-b980-4246-a006-c63da6965d18", "{0} Failed Interchange Health Check Report for company {1}", ServiceTaskCode, companyIdentifier));
						emailBuilder.SetBody(string.Format(CultureInfo.InvariantCulture, (NoResString)@"<p>{0}</p>
<p>{1}</p>", 
								LogMessages.FailedInterchangesReportMessage(entry.Value, lastReportedPointOfTime, currentCheckTime),
								LookupInformation), company);
						Env.OutgoingMailManager.CreateAndSave(emailBuilder.GetResult());
						AddInfo(Res.GetString("85bba488-332a-44eb-8f34-36d7657bf99f", "Notification(s) sent for period {0} to {1}.", lastReportedPointOfTime, currentCheckTime));
					}
					catch (EmailHasNoRecipientsException ex)
					{
						AddWarning(ex.Message);
					}
				}
			}
		}

		public virtual bool ReportFailuresInEmail { get; } = !ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalEDISystem();

		internal virtual GlbCompany[] GetCompaniesFromFactory()
		{
			return new eHubMessagingCompanySettingsManager().Companies;
		}

		static string LookupInformation => Res.GetString("0a6817a7-4262-4029-81ee-a0532fca6af0", "For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).");

		public abstract string TransportType { get; }

		public abstract NotificationFrequencyRegistryItem NotificationFrequencyRegistryItem { get; }

		public abstract GuidRegistryItem NotificationGroup { get; }

		public abstract int IntervalInMinute { get; }

		public abstract string FrequencySetting { get; }

		public abstract ZDateTime LastCheckedTimeForFailedEDIInterchange { get; set; }

		public abstract ZDateTime LastReportedTimeForFailedEDIInterchange { get; set; }

		public abstract StringRegistryItem FailedEDIInterchangesCountInPeriod { get; }

		public abstract string FailedEDIInterchangesStatusCode { get; }

		static string SqlFilterFailedEDIInterchangesInPeriod =>
			FormattableString.Invariant($@"
SELECT {GlbCompanySchema.Constants.PK}, {GlbCompanySchema.Constants.GC_Code}, {CountEDIInterchangePkColumnName}
FROM 
(
	SELECT {EDIInterchangeSchema.Constants.EI_GB}, Count(DISTINCT {EDIInterchangeSchema.Constants.PK}) AS {CountEDIInterchangePkColumnName}
	FROM {EDIInterchangeSchema.Constants.SqlSchemaName}.{EDIInterchangeSchema.Constants.TableName}
	WHERE {EDIInterchangeSchema.Constants.EI_Status} = @statusCode
	and {EDIInterchangeSchema.Constants.EI_SystemLastEditTimeUtc} >= @start
	and {EDIInterchangeSchema.Constants.EI_SystemLastEditTimeUtc} < @end
	and {EDIInterchangeSchema.Constants.EI_TransportType} = @transportType
	and {EDIInterchangeSchema.Constants.EI_ReceiveTransmit} = @receiveTransmit
	GROUP BY {EDIInterchangeSchema.Constants.EI_GB}
) FailedInterchange
INNER JOIN {GlbBranchSchema.Constants.SqlSchemaName}.{GlbBranchSchema.Constants.TableName} ON {EDIInterchangeSchema.Constants.EI_GB} = {GlbBranchSchema.Constants.PK}
INNER JOIN {GlbCompanySchema.Constants.SqlSchemaName}.{GlbCompanySchema.Constants.TableName} ON {GlbCompanySchema.Constants.PK} = {GlbBranchSchema.Constants.GB_GC}
");

		const string CountEDIInterchangePkColumnName = "Count_PK";
		const int ADayInMinute = 1440;
		BusinessObjectFactory factory;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory() { RefreshEnabled = false });
	}
}
