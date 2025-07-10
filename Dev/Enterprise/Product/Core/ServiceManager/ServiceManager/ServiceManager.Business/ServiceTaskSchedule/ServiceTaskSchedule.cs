using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business
{
	[CodeProperty(Schema.S5_ScheduleType)]
	[DescriptionProperty(Schema.S5_ScheduleDescription)]
	public class ServiceTaskSchedule : StmScheduleTask, IServiceTaskSchedule, IRecurrenceControlDataProvider
	{
		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ServiceTaskSchedule GetInstancesOfNameService(string codeToQuery)
			{
				var query = new ZQuery();
				query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix);
				query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, codeToQuery);

				var tasksFound = Factory.LoadTop1<ServiceTaskSchedule>(query);
				return tasksFound;
			}

			public IEnumerable<ServiceTaskSchedule> GetInstancesOfServiceTaskByCategory(string categoryToQuery)
			{
				var query = new ZQuery();
				query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix);
				query.AddToFilter(StmScheduleTaskSchema.S5_TypeOfDocument, categoryToQuery);

				var tasksFound = Factory.Load<ServiceTaskSchedule>(query);
				return tasksFound;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(ServiceTaskSchedule);
			}
		}

		public ServiceTaskSchedule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			lazyServiceTaskBindings = InitLazyServiceTaskBindings();
		}

		protected override StmScheduleTaskValidation GetNewValidation()
		{
			return new ServiceTaskScheduleValidation(this);
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && !InServiceHost)
			{
				if (NextRunTimeHasBeenSet)
				{
					DateTime? nextRunTime = S5_NextScheduledPrintRunTimeUtc.IsValid ? S5_NextScheduledPrintRunTimeUtc.ToDateTime() : null;
					StatusProvider.SetServiceTaskNextRuntime(S5_ScheduleType, nextRunTime);
					hostNextRuntime = ZDateTime.Empty;
					NextRunTimeHasBeenSet = false;
				}

				StatusProvider.RequestTaskConfigurationReload(S5_ScheduleType);
			}
		}

		protected override void ReloadCore()
		{
			hostedServiceSerializableSettings = null; // Clear the settings cache to force a reload of them next time we retrieve them.
			base.ReloadCore();
		}

		protected override bool MatchesFilterCore(ZQuery filter, DataRow row, DataTable table, string tableName, string identifier)
		{
			//LoadWithoutStatus
			var compositeFilters = new ServiceTaskScheduleFilterHelper(filter, new Dictionary<string, TaskInstanceStatus>(), Factory);
			return base.MatchesFilterCore(compositeFilters.TableSchemaFilters, row, table, tableName, identifier);
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
			S5_IsPrivate = true;

			var defaultBranch = GetDefaultBranch();
			if (defaultBranch != null)
			{
				S5_GB = defaultBranch.PK;
			}

			S5_WeekDaysOnly = false;
		}

		#endregion

		#region Logging
		protected override EnterpriseBusinessObject.AutologState AutoLoggingState =>
			Env.CurrentUser != null && !Env.CurrentUser.IsBatchProcessor
				? EnterpriseBusinessObject.AutologState.AutoLogged
				: EnterpriseBusinessObject.AutologState.NotLogged;

		#endregion

		#region Properties

		public bool S5_IsActive_ReadOnly
		{
			get
			{
				return IsScheduleReadOnly
						|| (
							StaticServiceAttributes != null
							&& StaticServiceAttributes.IsMandatory
						);
			}
		}

		public bool NextRunTime_ReadOnly => IsScheduleReadOnly;

		public bool IsScheduleReadOnly
		{
			get
			{
				return StaticServiceAttributes != null
						&& (
							StaticServiceAttributes.IsScheduleReadOnly
							|| (StaticServiceAttributes.IsReadOnlyForWiseCloudClient
								&& EnvProxy.IsHostedWithCargowise
								&& !EnvProxy.Instance.CurrentUser.IsSupportUser)
						);
			}
		}

		#region S5_ScheduleState

		[SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public override ZBlob S5_ScheduleState
		{
			get
			{
				return base.S5_ScheduleState;
			}
			set
			{
				var expectedXml = Encoding.ASCII.GetString(value);
				if (!string.IsNullOrEmpty(expectedXml) && !string.IsNullOrWhiteSpace(expectedXml))
				{
					try
					{
						var xmlDocument = new XmlDocument();
						xmlDocument.LoadXml(expectedXml);
					}
					catch (XmlException ex)
					{
						CargoWise.Common.ErrorReporter.ReportOnce("The string that is being saved to S5_Schedule State is not XML Parsable." + System.Environment.NewLine + "It might be compressed." + System.Environment.NewLine + "String: " + expectedXml, ex);
					}
				}
				hostedServiceSerializableSettings = null; // Clear the settings cache to force a reload of them next time we retrieve them.
				base.S5_ScheduleState = value;
			}
		}

		#endregion

		#region S5_TaskPeriod

		public override ZString S5_TaskPeriod
		{
			get => MinimumNudgeableScheduleShouldApply ? (ZString)minimumNudgeableTaskPeriod : base.S5_TaskPeriod;
			set
			{
				base.S5_TaskPeriod = value;
				if (!base.IsValidationSuspended)
				{
					Validation.ValidateS5_TaskPeriodCount();
				}
			}
		}

		#endregion

		#region TaskPeriodCount

		public override ZInt S5_TaskPeriodCount
		{
			get => MinimumNudgeableScheduleShouldApply ? (ZInt)minimumNudgeableTaskPeriodCount : base.S5_TaskPeriodCount;
			set => base.S5_TaskPeriodCount = value;
		}

		#endregion

		#endregion

		#region S5_GB

		protected override bool S5_GB_ReadOnly
		{
			get { return StaticServiceAttributes?.CanRunInAnyBranch ?? false; }
		}

		#endregion

		#region BranchName

		public ZString BranchName
		{
			get { return Branch != null ? Branch.GB_Code : ZString.Empty; }
		}

		#endregion

		#region Status

		public ZString StatusString { get; set; }
		public ZInt RunningCount { get; set; }
		public ZString PlaceInQueueString { get; set; }
		public ZString SecondsInQueueString { get; set; }
		public ZString SecondsRunningString { get; set; }
		public ZString ProcessIDsString { get; set; }
		public ZString RegisteredOnHosts { get; set; }

		[ResourceStringData("ServiceTaskSchedule.LastRunTime", Caption = "Last Run Time")]
		public ZDateTime LastRunTime { get; set; }

		[ResourceStringData("ServiceTaskSchedule.MutuallyExclusiveGroup", Caption = "Mutually Exclusive Group")]
		public ZString MutuallyExclusiveGroup
		{
			get
			{
				var groupString = new ZString(
					Enum.GetName(typeof(MutuallyExclusiveServiceTaskGroups),
					StaticServiceAttributes?.MutuallyExclusiveTaskGroup ?? MutuallyExclusiveServiceTaskGroups.NoGroup));
				return groupString;
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("ServiceTaskSchedule.CalcLastRunTimeLocal", Caption = "Last Run Time (local)")]
		public ZDateTime CalcLastRunTimeLocal => GetLocalDate(LastRunTime);

		[ResourceStringData("ServiceTaskSchedule.LastErrorTime", Caption = "Last Error Time")]
		public ZDateTime LastErrorTime
		{
			get
			{
				if (InServiceHost)
				{
					return serviceHostErrorTracker.Value.Latest ?? ZDateTime.Empty;
				}
				return lastErrorTimeFromServiceHosts;
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("ServiceTaskSchedule.CalcLastErrorTimeLocal", Caption = "Last Error Time (local)")]
		public ZDateTime CalcLastErrorTimeLocal => GetLocalDate(LastErrorTime);

		public ZInt ErrorCountLast24Hours
		{
			get
			{
				if (InServiceHost)
				{
					return serviceHostErrorTracker.Value.Count;
				}
				return errorCountLast24HoursFromServiceHosts;
			}
		}

		ZDateTime lastErrorTimeFromServiceHosts;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		int errorCountLast24HoursFromServiceHosts;
		readonly Lazy<OccurrenceTracker> serviceHostErrorTracker = new Lazy<OccurrenceTracker>(() => new OccurrenceTracker(TimeSpan.FromDays(1), () => ZDateTime.UtcNow.ToDateTime()));

		public void OnErrorReported()
		{
			serviceHostErrorTracker.Value.Add();
		}

		public ZInt ServiceTaskBindingsCount => lazyServiceTaskBindings.Value.Count;

		public ZString ServiceTaskBindingTypesString => new ZString(string.Join(",", lazyServiceTaskBindings.Value));

		Lazy<ICollection<string>> lazyServiceTaskBindings = new Lazy<ICollection<string>>(() => Array.Empty<string>());

		Lazy<ICollection<string>> InitLazyServiceTaskBindings()
		{
			return new Lazy<ICollection<string>>(
				() => HostedServiceBusinessObjectBindingsProvider
					.Instance
					.BusinessObjectBindings
					.Where(binding => binding.ServiceTaskCode == S5_ScheduleType)
					.Select(binding => binding.Table)
					.Distinct()
					.ToList());
		}

		#endregion

		#region S5_ScheduleType

		public override ZString S5_ScheduleType
		{
			get => base.S5_ScheduleType;
			set
			{
				if (base.S5_ScheduleType != value)
				{
					base.S5_ScheduleType = value;
					lazyServiceTaskBindings = InitLazyServiceTaskBindings();
				}
			}
		}

		#endregion

		#region ScheduleFrequency

		public TimeSpan SchedulePeriodDuration
		{
			get
			{
				var nudgeableDuration = (DefaultScheduleDuration > minimumNudgeableScheduleDuration.Value)
					? DefaultScheduleDuration
					: minimumNudgeableScheduleDuration.Value;

				return (IsNudgeable && IsInDatabase)
					? nudgeableDuration
					: ConfiguredScheduleDuration;
			}
		}

		TimeSpan ConfiguredScheduleDuration => CalculateScheduleDuration(base.S5_TaskPeriod, base.S5_TaskPeriodCount);

		TimeSpan? defaultScheduleCached;
		TimeSpan DefaultScheduleDuration => defaultScheduleCached ??= ServiceTaskScheduleValidation.GetPeriodDuration(StaticServiceAttributes.DefaultSchedule.RunEvery, TimeSpan.FromMinutes(15), isRandomPeriod: false);
		bool MinimumNudgeableScheduleShouldApply => IsNudgeable && IsInDatabase && DefaultScheduleDuration < minimumNudgeableScheduleDuration.Value;

		readonly Lazy<TimeSpan> minimumNudgeableScheduleDuration = new Lazy<TimeSpan>(() => CalculateScheduleDuration(minimumNudgeableTaskPeriod, minimumNudgeableTaskPeriodCount));

		static TimeSpan CalculateScheduleDuration(ZString taskPeriod, ZInt taskPeriodCount)
		{
			switch (taskPeriod)
			{
				case ScheduleRecurrenceType.Second:
					return TimeSpan.FromSeconds(taskPeriodCount);

				case ScheduleRecurrenceType.Minute:
					return TimeSpan.FromMinutes(taskPeriodCount);

				case ScheduleRecurrenceType.Hourly:
					return TimeSpan.FromHours(taskPeriodCount);

				case ScheduleRecurrenceType.Daily:
					return TimeSpan.FromDays(taskPeriodCount);

				case ScheduleRecurrenceType.Weekly:
					return TimeSpan.FromDays(taskPeriodCount * 7);

				case ScheduleRecurrenceType.Monthly:
					return TimeSpan.FromDays(taskPeriodCount * 28); // shortest possible month

				case ScheduleRecurrenceType.Yearly:
					return TimeSpan.FromDays(1 * 365); // always one year

				default:
					return TimeSpan.MaxValue;
			}
		}

		#endregion

		#region LogViewer

		public ServiceTaskLogViewer LogViewer => logViewer ??=
			new ServiceTaskLogViewer()
			{
				FileBasedLogViewer = { TaskType = S5_ScheduleType, TaskTypeReadOnly = true },
				SearchBasedLogViewer = { ServiceTaskCode = S5_ScheduleType },
			};
		ServiceTaskLogViewer logViewer;

		#endregion

		#region Settings

		HostedServiceConfiguration ServiceConfig
		{
			get
			{
				if (null == hostedServiceConfig)
				{
					var serviceAttributes = StaticServiceAttributes;
					if (serviceAttributes != null)
					{
						hostedServiceConfig = new HostedServiceConfiguration(serviceAttributes);
					}
				}
				return hostedServiceConfig;
			}
		}

		HostedServiceConfiguration hostedServiceConfig;

		public IHostedServiceAttribute StaticServiceAttributes =>
			actuallyStaticServiceSettings.Value
			?? (staticServiceSettings ??= ObjectFactory.Get<IClientHostedServiceAttributeProvider>().GetClientHostedServiceAttribute(S5_ScheduleType));

		static readonly Overridable<IHostedServiceAttribute> actuallyStaticServiceSettings = new Overridable<IHostedServiceAttribute>(null);
		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Safely handled, no need to be thread-static")]
		IHostedServiceAttribute staticServiceSettings;

#if DEBUG

		public void SetStaticServiceAttributesDebugOnly(IHostedServiceAttribute settings)
		{
			actuallyStaticServiceSettings.Value = settings;
			defaultScheduleCached = ServiceTaskScheduleValidation.GetPeriodDuration(settings.DefaultSchedule.RunEvery, TimeSpan.FromMinutes(15), isRandomPeriod: false);
		}

#endif

		internal HostedServiceSerializableSettings ServiceSettings
		{
			get
			{
				if (hostedServiceSerializableSettings == null)
				{
					hostedServiceSerializableSettings = HostedServiceSerializableSettings.FromXml<HostedServiceSerializableSettings>(S5_ScheduleState.ToAscii());
					if (hostedServiceSerializableSettings == null)
					{
						hostedServiceSerializableSettings = new HostedServiceSerializableSettings();

						if (StaticServiceAttributes?.AllowsMultipleInstances ?? false)
						{
							hostedServiceSerializableSettings.SecondaryProcessesMaxCount = MaxSecondaryProcessesMaxCount;
						}
					}
				}

				return hostedServiceSerializableSettings;
			}
			set
			{
				var xml = value.AsXml();
				if (S5_ScheduleState.ToAscii() != xml)
				{
					S5_ScheduleState = Encoding.ASCII.GetBytes(xml);
					hostedServiceSerializableSettings = value;
				}
			}
		}

		HostedServiceSerializableSettings hostedServiceSerializableSettings;

		public string ConfigString
		{
			get
			{
				return ServiceSettings?.ConfigString ?? string.Empty;
			}
			set
			{
				var settings = ServiceSettings ?? new HostedServiceSerializableSettings();
				settings.ConfigString = value;
				ServiceSettings = settings;
			}
		}

		#endregion

		#region SecondaryProcessesMaxCount

		public ZInt SecondaryProcessesMaxCount
		{
			get
			{
				var settings = ServiceSettings;
				return settings != null ? settings.SecondaryProcessesMaxCount : 0;
			}
			set
			{
				var settings = ServiceSettings ?? new HostedServiceSerializableSettings();
				settings.SecondaryProcessesMaxCount = value;
				ServiceSettings = settings;
				SetNonPersistentPropertyValue(SecondaryProcessesMaxCountInfo, ref secondaryProcessesMaxCount, value, false);
				if (!IsValidationSuspended)
				{
					new ServiceTaskScheduleValidation(this).ValidateSecondaryProcessesMaxCount();
				}
			}
		}
		ZInt secondaryProcessesMaxCount;
		ZString secondaryProcessesMaxCountDescription;
		const int MaxSecondaryProcessesMaxCount = 100;
		const int InvalidSecondaryProcessesMaxCount = MaxSecondaryProcessesMaxCount + 1;

		[List("SecondaryProcessesMaxCountsList")]
		public ZString SecondaryProcessesMaxCountDescription
		{
			get
			{
				if (secondaryProcessesMaxCountDescription.IsEmpty)
				{
					secondaryProcessesMaxCountDescription =
						SecondaryProcessesMaxCountsList.GetDescriptionFromCode(SecondaryProcessesMaxCount.ToString());
				}

				return secondaryProcessesMaxCountDescription;
			}
			set
			{
				var code = SecondaryProcessesMaxCountsList.GetCodeFromDescription(value);
				SecondaryProcessesMaxCount = Convert.ToInt32(code ?? InvalidSecondaryProcessesMaxCount.ToString());
				SetNonPersistentPropertyValue(SecondaryProcessesMaxCountDescriptionInfo, ref secondaryProcessesMaxCountDescription, value, false);
				if (!IsValidationSuspended)
				{
					new ServiceTaskScheduleValidation(this).ValidateSecondaryProcessesMaxCountDescription();
				}
			}
		}

		public ZPropertyInfo SecondaryProcessesMaxCountDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SecondaryProcessesMaxCountDescription), Res.GetString("8D4E054E-3FD4-4FF6-A6D7-A41A2AB7F45C", "Secondary Processes Max Count Description")); }
		}

		public ZPropertyInfo SecondaryProcessesMaxCountInfo
		{
			get { return GetZPropertyInfo(nameof(SecondaryProcessesMaxCount), Res.GetString("b6668770-165a-445e-976b-7baaba6b7ba8", "Secondary Processes Max Count")); }
		}

		public CodeDescriptionPairList SecondaryProcessesMaxCountsList
		{
			get
			{
				if (secondaryProcessesMaxCountsList == null)
				{
					secondaryProcessesMaxCountsList = new CodeDescriptionPairList();
					secondaryProcessesMaxCountsList.AddPair(MaxSecondaryProcessesMaxCount.ToString(), Res.GetString("0C64523E-C553-4970-B842-D8CB7D157DAD", "SYSTEM MANAGED"));
					Enumerable.Range(0, MaxSecondaryProcessesMaxCount).ForEach(i => secondaryProcessesMaxCountsList.AddPair(i.ToString(), i.ToString()));
				}

				return secondaryProcessesMaxCountsList;
			}
		}

		CodeDescriptionPairList secondaryProcessesMaxCountsList;

		public ZString ExtendedConfigProcessesMaxCountWarningMessage
		{
			get
			{
				return extendedConfigProcessesMaxCountWarningMessage;
			}
			set
			{
				extendedConfigProcessesMaxCountWarningMessage = value;
			}
		}
		ZString extendedConfigProcessesMaxCountWarningMessage;

		public ZString ExtendedConfigProcessesMaxCountWarningLink
		{
			get
			{
				return extendedConfigProcessesMaxCountWarningLink;
			}
			set
			{
				extendedConfigProcessesMaxCountWarningLink = value;
			}
		}
		ZString extendedConfigProcessesMaxCountWarningLink;

		#endregion

		#region EnvironmentProvider

		IServiceHostsCache serviceHostsCache;

		protected IServiceHostsCache ServiceHostsCache
		{
			get
			{
				return serviceHostsCache ?? (serviceHostsCache = ObjectFactory.Get<IServiceHostsCache>());
			}
			set
			{
				serviceHostsCache = value;
			}
		}

		#endregion

		#region Next Run Time

		public ZDateTime CalculateNextRunTime(ILogger logger)
		{
			ZDateTime nextRunTime;
			if (NextRunTimeExpired || NextRunTimeIsInFuture)
			{
				logger.Log(LogLevel.Debug, "Next runtime was out of the expected range, so it was recalculated based off 'now'");
				nextRunTime = Recurrence.CalculateNextScheduleDate(ZDateTime.UtcNow); // "current" next runtime is either expired or empty - calculate based on "now"
			}
			else
			{
				nextRunTime = Recurrence.CalculateNextScheduleDate(); // schedule next run based on usual recurrence rules
			}
			return nextRunTime;
		}

		public bool NextRunTimeIsInFuture => S5_NextScheduledPrintRunTimeUtc > ZDateTime.UtcNow;

		public bool NextRunTimeExpired
		{
			get
			{
				if (S5_NextScheduledPrintRunTimeUtc.IsEmpty || !S5_NextScheduledPrintRunTimeUtc.IsValid)
				{
					return true;
				}

				var allowableDeviationInMinutes = Math.Min((int)Math.Ceiling((Recurrence.CalculateNextScheduleDate() - S5_NextScheduledPrintRunTimeUtc).TotalMinutes * 0.25), 60);
				var now = ZDateTime.UtcNow;
				return now.AddMinutes(-allowableDeviationInMinutes) > S5_NextScheduledPrintRunTimeUtc;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Design", "CA1021")]
		public bool IsValidTimeOfDayToRun(out string message)
		{
			var now = ZDateTime.UtcNow;
			var timeOfDay = now.TimeOfDay;
			message = "";

			if (S5_NextScheduledPrintRunTimeUtc.IsValid &&
				(S5_TaskPeriod == ScheduleRecurrenceType.Daily
				|| S5_TaskPeriod == ScheduleRecurrenceType.Weekly
				|| S5_TaskPeriod == ScheduleRecurrenceType.Monthly
				|| S5_TaskPeriod == ScheduleRecurrenceType.Yearly))
			{
				var runTime = S5_NextScheduledPrintRunTimeUtc.TimeOfDay;
				var timeDiff = (timeOfDay - runTime).TotalHours;
				//normalize to within range [-12, 12] so that, for example, -23 becomes 1 is within bounds, and 23 becomes -1 is within bounds, but 22/-22 remain out of bounds
				if (timeDiff > 12) { timeDiff -= 24; }
				if (timeDiff < -12) { timeDiff += 24; }

				if (Math.Abs(timeDiff) > 1.5)
				{
					message = string.Format(CultureInfo.InvariantCulture,
					"Next Run Time UTC ({0}) time of day is more than 1.5 hours away from current UTC ({1}) time of day. To avoid this problem in the future, always set Next Run Time to within an hour of Scheduled Run Time, or use 'Schedule Now' to skip this check.",
					S5_NextScheduledPrintRunTimeUtc.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture), now.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture));
					return false;
				}

				if (S5_WeekDaysOnly && S5_TaskPeriod == ScheduleRecurrenceType.Daily)
				{
					if ((now.DayOfWeek == DayOfWeek.Sunday)
					|| (timeOfDay >= runTime && now.DayOfWeek == DayOfWeek.Saturday)
					|| (timeOfDay < runTime && now.DayOfWeek == DayOfWeek.Monday))
					{
						message = string.Format(CultureInfo.InvariantCulture,
						"This task is scheduled to run on weekdays only, and an attempt has been made to run it on the weekend (Next Run Time UTC = {0}, current UTC = {1}). To avoid this problem in the future, always set Next Run Time to an appropriate day of the week, or use 'Schedule Now' to skip this check.",
						S5_NextScheduledPrintRunTimeUtc.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture), now.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture));
						return false;
					}
				}
			}

			if (S5_DailyStartTime.IsValid && S5_DailyEndTime.IsValid &&
				(S5_TaskPeriod == ScheduleRecurrenceType.Hourly
				|| S5_TaskPeriod == ScheduleRecurrenceType.Minute
				|| S5_TaskPeriod == ScheduleRecurrenceType.Second))
			{
				var startTime = CalcDailyStartTimeUtc.TimeOfDay;
				var endTime = CalcDailyEndTimeUtc.TimeOfDay;

				if (
					((startTime < endTime) && (timeOfDay < startTime || timeOfDay > endTime))
					|| ((startTime > endTime) && (timeOfDay > endTime && timeOfDay < startTime))
					)
				{
					message = string.Format(CultureInfo.InvariantCulture,
					"The current UTC ({0}) is not between Start Time UTC ({1}) and End Time UTC ({2}). To avoid this problem in the future, always set Next Run Time within the bounds of Start Time and End Time, or use 'Schedule Now' to skip this check.",
					now.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture), CalcDailyStartTimeUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
					CalcDailyEndTimeUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
					return false;
				}
			}

			return true;
		}

		public override ZDateTime S5_NextScheduledPrintRunTimeUtc
		{
			get
			{
				if (!InServiceHost && !NextRunTimeHasBeenSet)
				{
					return !hostNextRuntime.IsEmpty ? hostNextRuntime : base.S5_NextScheduledPrintRunTimeUtc;
				}

				return base.S5_NextScheduledPrintRunTimeUtc;
			}
			set
			{
				SetNextRuntime(value, valueFromHost: false);
			}
		}

		public bool InServiceHost
		{
			get
			{
				return string.Equals(CargoWise.Data.DbConnection.ApplicationName, DbConnectionConstants.ApplicationNames.ServiceHost, StringComparison.OrdinalIgnoreCase);
			}
		}
		public bool NextRunTimeHasBeenSet { get; private set; }

		void SetNextRuntime(ZDateTime value, bool valueFromHost)
		{
			if (!InServiceHost && valueFromHost)
			{
				hostNextRuntime = value;
				return;
			}

			NextRunTimeHasBeenSet = true;
			base.S5_NextScheduledPrintRunTimeUtc = value;
		}

		public ZDateTime NextRunTimeOriginalValue
		{
			get
			{
				if (!InServiceHost && !hostNextRuntime.IsEmpty)
				{
					return hostNextRuntime;
				}

				var originalNextRunRaw = S5_NextScheduledPrintRunTimeUtcInfo.OriginalValue;
				return (originalNextRunRaw == null || !originalNextRunRaw.IsValid) ? DateTime.MinValue : ((ZDateTime)originalNextRunRaw).ToDateTime();
			}
		}

		ZDateTime hostNextRuntime;
		IServiceTaskScheduleStatusProvider statusProvider;
		bool statusUpdateInProgress;

		protected IServiceTaskScheduleStatusProvider StatusProvider
		{
			get
			{
				return statusProvider ?? (statusProvider = ObjectFactory.Get<IServiceTaskScheduleStatusProvider>());
			}
			set
			{
				statusProvider = value;
			}
		}

		#endregion

		#region IServiceTaskSchedule Members

		string IServiceTaskSchedule.GetBranchCountryCode()
		{
			return Branch != null ? (string)Branch.Country.Code : string.Empty;
		}

		public bool IsValidTaskPeriod
		{
			get { return !S5_TaskPeriod.IsEmpty; }
		}

		#endregion

		#region IsNudgeable

		public bool IsNudgeable => (ServiceTaskBindingsCount > 0 || (ServiceConfig?.IsConfiguredForNudging ?? false)) && SharedRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled;

		const string minimumNudgeableTaskPeriod = ScheduleRecurrenceType.Minute;
		const int minimumNudgeableTaskPeriodCount = 15;

		#endregion

		#region Copy

		public void CopyScheduleFrom(ServiceTaskSchedule scheduleToCopy)
		{
			this.S5_TaskPeriod = scheduleToCopy.S5_TaskPeriod;
			this.S5_WeekDaysOnly = scheduleToCopy.S5_WeekDaysOnly;
			this.S5_TaskPeriodCount = scheduleToCopy.S5_TaskPeriodCount;
			this.S5_DayNumber = scheduleToCopy.S5_DayNumber;
			this.S5_DayList = scheduleToCopy.S5_DayList;
			this.S5_MonthNumber = scheduleToCopy.S5_MonthNumber;
			this.S5_WeekDayOccurrenceNumber = scheduleToCopy.S5_WeekDayOccurrenceNumber;
			this.S5_StartDate = scheduleToCopy.S5_StartDate;
			this.S5_EndAfterCount = scheduleToCopy.S5_EndAfterCount;
			this.S5_EndDate = scheduleToCopy.S5_EndDate;
			this.S5_ScheduleActualRunCount = scheduleToCopy.S5_ScheduleActualRunCount;
			this.S5_AccountingPeriodScheduleFrstRun = scheduleToCopy.S5_AccountingPeriodScheduleFrstRun;
			this.S5_DateScheduleFirstRun = scheduleToCopy.S5_DateScheduleFirstRun;
			this.S5_ScheduleType = scheduleToCopy.S5_ScheduleType;
			this.S5_TypeOfDocument = scheduleToCopy.S5_TypeOfDocument;
			this.S5_NextScheduledPrintRunTimeUtc = scheduleToCopy.S5_NextScheduledPrintRunTimeUtc;
			this.S5_CurrentPrintRunTime = scheduleToCopy.S5_CurrentPrintRunTime;
			this.S5_IsPrivate = scheduleToCopy.S5_IsPrivate;
			this.S5_ScheduleState = scheduleToCopy.S5_ScheduleState;
			this.S5_GB = scheduleToCopy.S5_GB;
			this.S5_DailyStartTime = scheduleToCopy.S5_DailyStartTime;
			this.S5_DailyEndTime = scheduleToCopy.S5_DailyEndTime;
			this.S5_RunTimeInMinutes = scheduleToCopy.S5_RunTimeInMinutes;
		}

		public virtual TaskInstanceStatus UpdateStatus()
		{
			TaskInstanceStatus taskStatus = null;
			if (!statusUpdateInProgress && (!InServiceHost || ServiceHostsCache.ConfiguredServiceHosts.Count() > 1))
			{
				taskStatus = StatusProvider.GetServiceTaskStatus(S5_ScheduleType);
				OnStatusUpdateComplete(taskStatus);
			}

			return taskStatus;
		}

		public void OnStatusUpdateComplete(TaskInstanceStatus statusCopySource)
		{
			statusUpdateInProgress = true;
			try
			{
				if (statusCopySource == null)
				{
					return;
				}

				StatusString = statusCopySource.StatusString;
				PlaceInQueueString = statusCopySource.PlaceInQueueString;
				SecondsInQueueString = statusCopySource.SecondsInQueueString;
				RunningCount = statusCopySource.RunningCount;
				ProcessIDsString = statusCopySource.ProcessIDsString;
				SecondsRunningString = statusCopySource.SecondsRunningString;
				RegisteredOnHosts = statusCopySource.RegisteredOnHosts;
				errorCountLast24HoursFromServiceHosts = statusCopySource.ErrorCountLast24Hours;
				lastErrorTimeFromServiceHosts = statusCopySource.LastErrorTime ?? ZDateTime.Empty;

				if (statusCopySource.LastRunTime.HasValue)
				{
					var newLastRunTime = (ZDateTime)statusCopySource.LastRunTime;
					LastRunTime = (LastRunTime.IsEmpty || newLastRunTime > LastRunTime) ? newLastRunTime : LastRunTime;
				}

				if (statusCopySource.NextRunTime.HasValue)
				{
					var newNextRunTime = (ZDateTime)statusCopySource.NextRunTime;
					if (!InServiceHost || S5_NextScheduledPrintRunTimeUtc.IsEmpty || newNextRunTime > S5_NextScheduledPrintRunTimeUtc)
					{
						SetNextRuntime(newNextRunTime, valueFromHost: true);
					}
				}
			}
			finally
			{
				statusUpdateInProgress = false;
			}
		}

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
			HasChanges = false;
		}
#endif
		#endregion
	}
}

