using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Data;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using static System.FormattableString;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW;

[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "no use of Business Object and Business Object Factory in Process Controller")]
public class NativeServiceTaskGovernor : IServiceTaskGovernor
{
	internal NativeServiceTaskGovernor(NativeServiceTaskDTO dto, IClientHostedServiceAttributeProvider attributeProvider, IServiceTaskScheduleStatusProvider statusProvider, IHostedServiceBusinessObjectBindingsProvider bindingsProvider, IDateTimeProvider dateTimeProvider)
	{
		this.dto = dto ?? throw new ArgumentNullException(nameof(dto));
		this.attributeProvider = attributeProvider ?? throw new ArgumentNullException(nameof(attributeProvider));
		this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
		this.bindingsProvider = bindingsProvider ?? throw new ArgumentNullException(nameof(bindingsProvider));
		this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
		entityMap = new();
		lazyStaticServiceSettings = new Lazy<IHostedServiceAttribute>(() =>
			attributeProvider.GetClientHostedServiceAttribute(dto.ServiceTaskCode));
	}

	public void SetActive(bool isActive)
	{
		if (isActive && !IsGoverningNewServiceTask)
		{
			AddToEntityMap("SST_NextRunTime", ("@NextRunTime", SqlDbType.DateTimeOffset, GovernedTask.CalculateNextRunTime(NullLogger.Instance)));
		}
		AddToEntityMap("SST_Active", ("@IsActive", SqlDbType.Bit, isActive));
	}

	public void SetBranchPk(Guid pk)
	{
		AddToEntityMap("SST_GB_Branch", ("@BranchPk", SqlDbType.UniqueIdentifier, pk));
	}

	public void SetBranchFromCode(string branchCode)
	{
		var branchQuery = "SELECT TOP 1 [GB_PK] FROM [dbo].[GlbBranch] WHERE [GB_Code] = @BranchCode";
		var branchPk = Guid.Empty;

		using var cmd = Db.Connection.Command(branchQuery);
		cmd.AddParameter("@BranchCode", SqlDbType.VarChar, branchCode);

		using var reader = cmd.ExecuteReader();

		while (reader.Read())
		{
			branchPk = (Guid)reader["GB_PK"];
		}

		if (branchPk != Guid.Empty)
		{
			AddToEntityMap("SST_GB_Branch", ("@BranchPk", SqlDbType.UniqueIdentifier, branchPk));
		}
	}

	public void SetStartDate(DateTimeOffset startDate)
	{
		SetNextRunTime(startDate);
	}

	public void SetNextRunTime(DateTimeOffset nextRunTime)
	{
		AddToEntityMap("SST_NextRunTime", ("@NextRunTime", SqlDbType.DateTimeOffset, nextRunTime));
	}

	public void SetLastRunTime(DateTimeOffset lastRunTime)
	{
		AddToEntityMap("SST_LastRunTime", ("@LastRunTime", SqlDbType.DateTimeOffset, lastRunTime));
	}

	public void SetSchedule(int frequency, string recurrence)
	{
		if (IsGoverningNewServiceTask)
		{
			INextRunTimeCalculator? localCalculator = recurrence switch
			{
				"S" => new NextRunTimeCalculatorSeconds() { Period = frequency },
				"T" => new NextRunTimeCalculatorMinutes() { Period = frequency },
				"H" => new NextRunTimeCalculatorHours() { Period = frequency },
				"D" => new NextRunTimeCalculatorDays() { Period = frequency },
				"Y" => new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 1 },
				_ => null
			};
			if (localCalculator != null)
			{
				Calculator = localCalculator;
				var xml = NativeServiceTask.Serialize(Calculator, 0, "", out scheduleConfig);
				AddToEntityMap("SST_Configuration", ("@Configuration", SqlDbType.Xml, xml));
				SetGovernedTaskForNewSchedule();
			}
		}
	}

	public void SetWeeklySchedule(int frequency, DayOfWeek[] daysOfWeek)
	{
		if (IsGoverningNewServiceTask)
		{
			Calculator = new NextRunTimeCalculatorWeeks() { Period = frequency, DaysOfOccurrence = daysOfWeek };

			var xml = NativeServiceTask.Serialize(Calculator, 0, "", out scheduleConfig);
			AddToEntityMap("SST_Configuration", ("@Configuration", SqlDbType.Xml, xml));
			SetGovernedTaskForNewSchedule();
		}
	}

	public void SetMonthlySchedule(int frequency, int dayOfMonth)
	{
		if (IsGoverningNewServiceTask)
		{
			Calculator = new NextRunTimeCalculatorMonthsByDate() { Period = frequency, DayOfOccurrence = dayOfMonth };

			var xml = NativeServiceTask.Serialize(Calculator, 0, "", out scheduleConfig);
			AddToEntityMap("SST_Configuration", ("@Configuration", SqlDbType.Xml, xml));
			SetGovernedTaskForNewSchedule();
		}
	}

	public void SetDailyStartTimeLocal(TimeSpan startTime, TimeSpan offset)
	{
		var localToUtcOffset = GetUtcOffset();
		var utcTime = ApplyOffset(startTime, localToUtcOffset);

		SetDailyStartTimeUtc(utcTime, offset);
	}

	public void SetDailyStartTimeUtc(TimeSpan startTime, TimeSpan offset)
	{
		if (IsGoverningNewServiceTask)
		{
			switch (Calculator)
			{
				case NextRunTimeCalculatorSeconds calculatorSeconds:
					calculatorSeconds.StartTime = startTime + offset;
					break;
				case NextRunTimeCalculatorMinutes calculatorMinutes:
					calculatorMinutes.StartTime = startTime + offset;
					break;
				case NextRunTimeCalculatorHours calculatorHours:
					calculatorHours.StartTime = startTime + offset;
					break;
				case NextRunTimeCalculatorDays calculatorDays:
					calculatorDays.ScheduledRunTime = startTime + offset;
					break;
				case NextRunTimeCalculatorWeeks calculatorWeeks:
					calculatorWeeks.ScheduledRunTime = startTime + offset;
					break;
				case NextRunTimeCalculatorWorkingDays calculatorWorkingDays:
					calculatorWorkingDays.ScheduledRunTime = startTime + offset;
					break;
				case NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate:
					calculatorMonthsByDate.ScheduledRunTime = startTime + offset;
					break;
				case NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek:
					calculatorMonthsByDayOfWeek.ScheduledRunTime = startTime + offset;
					break;
				case NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay:
					calculatorMonthsByLastDay.ScheduledRunTime = startTime + offset;
					break;
				case NextRunTimeCalculatorYearsByDate calculatorYearsByDate:
					calculatorYearsByDate.ScheduledRunTime = startTime + offset;
					break;
				case NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth:
					calculatorYearsByDayOfMonth.ScheduledRunTime = startTime + offset;
					break;
			}

			var xml = NativeServiceTask.Serialize(Calculator, 0, "");
			AddToEntityMap("SST_Configuration", ("@Configuration", SqlDbType.Xml, xml));
		}
	}

	public void SetDailyEndTimeLocal(TimeSpan endTime)
	{
		var localToUtcOffset = GetUtcOffset();
		var utcTime = ApplyOffset(endTime, localToUtcOffset);

		SetDailyEndTimeUtc(utcTime);
	}

	public void SetDailyEndTimeUtc(TimeSpan endTime)
	{
		if (IsGoverningNewServiceTask)
		{
			switch (Calculator)
			{
				case NextRunTimeCalculatorSeconds calculatorSeconds:
					calculatorSeconds.EndTime = endTime;
					break;
				case NextRunTimeCalculatorMinutes calculatorMinutes:
					calculatorMinutes.EndTime = endTime;
					break;
				case NextRunTimeCalculatorHours calculatorHours:
					calculatorHours.EndTime = endTime;
					break;
			}

			var xml = NativeServiceTask.Serialize(Calculator, 0, "");
			AddToEntityMap("SST_Configuration", ("@Configuration", SqlDbType.Xml, xml));
		}
	}

	public void SetOverdueDuration(TimeSpan secondsOverdue)
	{
		// No action required.
	}

	public bool ResetScheduleToDefault(bool reEnableMandatory, ILogger logger)
	{
		var scheduleHasBeenUpdated = FixTaskIsActiveForMandatoryTasks();
		if (!GovernedTask.IsActive || !CheckAndFixSchedulePeriod())
		{
			return scheduleHasBeenUpdated;
		}

		ReCalculateNextRunTime();
		return true;

		void ReCalculateNextRunTime()
		{
			var originalNextRuntime = GovernedTask.NextRunTime;
			var newNextRunTime = GovernedTask.CalculateNextRunTime(logger);

			if (newNextRunTime != originalNextRuntime)
			{
				SetNextRunTime(newNextRunTime);
				logger.Log(
					LogLevel.Warning,
					Invariant($"Service task {GovernedTask.Description} - \"next runtime\" has been corrected from: {originalNextRuntime:o} to: {newNextRunTime:o} because the original scheduled value was outside the allowed interval."));
			}
		}

		bool FixTaskIsActiveForMandatoryTasks()
		{
			if (reEnableMandatory && !GovernedTask.IsActive && lazyStaticServiceSettings.Value.IsMandatory)
			{
				logger.Log(LogLevel.Warning, Invariant($"Task {GovernedTask.Description} is mandatory but inactive - reactivating"));
				SetActive(true);
				return true;
			}

			return false;
		}

		bool CheckAndFixSchedulePeriod()
		{
			var scheduledPeriod = GovernedTask.SchedulePeriodDuration;
			var scheduleUpdatedToMinimum = FixPeriodIfOutOfLimit(
				lazyStaticServiceSettings.Value.MinimumPeriod,
				scheduledPeriod,
				TimeSpan.MinValue,
				periodLimitedByTaskAttribute => scheduledPeriod < periodLimitedByTaskAttribute);
			var scheduleUpdatedToMaximum = FixPeriodIfOutOfLimit(
				lazyStaticServiceSettings.Value.MaximumPeriod,
				scheduledPeriod,
				TimeSpan.MaxValue,
				periodLimitedByTaskAttribute => scheduledPeriod > periodLimitedByTaskAttribute);

			return scheduleUpdatedToMinimum || scheduleUpdatedToMaximum;
		}

		bool FixPeriodIfOutOfLimit(string taskAttributePeriodLimit, TimeSpan scheduledPeriod, TimeSpan defaultToDuration, Func<TimeSpan, bool> isOutOfLimit)
		{
			taskAttributePeriodLimit = taskAttributePeriodLimit ?? string.Empty;
			var periodLimitByTaskAttribute = ServiceTaskScheduleHelper.GetPeriodDuration(taskAttributePeriodLimit, defaultToDuration, isRandomPeriod: false);
			if (isOutOfLimit(periodLimitByTaskAttribute)
				&& ServiceTaskScheduleHelper.ParseFrequency(taskAttributePeriodLimit, out int limitedPeriodCount, out string limitedPeriodType))
			{
				SetSchedule(limitedPeriodCount, limitedPeriodType);
				logger.Log(LogLevel.Warning, Invariant($"Service task period [{scheduledPeriod}] was updated to [{periodLimitByTaskAttribute}] to be in allowed interval for service task {GovernedTask.Description}."));
				var configString = ScheduleConfig.ConfigString ?? string.Empty;
				switch (limitedPeriodType)
				{
					case "W":
						if (Calculator is NextRunTimeCalculatorWeeks calculatorWeeks && calculatorWeeks.DaysOfOccurrence.Length == 0 )
						{
							calculatorWeeks.DaysOfOccurrence = new[] { DayOfWeek.Sunday };
							var xml = NativeServiceTask.Serialize(calculatorWeeks, ScheduleConfig.SecondaryProcessesMaxCount, configString);
							AddToEntityMap("SST_Configuration", ("@Configuration", SqlDbType.Xml, xml));
							logger.Log(LogLevel.Warning, Invariant($"Service task {GovernedTask.Description} has been set to run on Sunday(s) because there was no day selected on the weekly schedule."));
						}
						break;

					case "Y":
						if (Calculator is NextRunTimeCalculatorYearsByDate calculatorYears && calculatorYears.Month == 0)
						{
							calculatorYears.Month = 1;
							calculatorYears.Day = 1;
							var xml = NativeServiceTask.Serialize(calculatorYears, ScheduleConfig.SecondaryProcessesMaxCount, configString);
							AddToEntityMap("SST_Configuration", ("@Configuration", SqlDbType.Xml, xml));
							logger.Log(LogLevel.Warning, Invariant($"Service task {GovernedTask.Description} has been set to run on January because there was no month of the year selected on the yearly schedule."));
						}

						break;
				}

				return true;
			}

			return false;
		}
	}

	public TaskInstanceStatus UpdateStatus()
	{
		var status = statusProvider.GetServiceTaskStatus(ServiceTaskCode);
		return status;
	}

	public void Reload()
	{
		var query = @"
SELECT
	ServiceTask.SST_PK,
	ServiceTask.SST_ServiceTaskCode,
	ServiceTask.SST_Active,
	ServiceTask.SST_NextRunTime,
	ServiceTask.SST_LastRunTime,
	ServiceTask.SST_Configuration,
	Branch.GB_PK,
	Branch.GB_Code
FROM dbo.StmServiceTask ServiceTask
LEFT JOIN dbo.GlbBranch branch on ServiceTask.SST_GB_Branch = Branch.GB_PK
WHERE SST_PK = @ServiceTaskPk
";

		using var cmd = Db.Connection.Command(query);
		cmd.AddParameter("@ServiceTaskPk", SqlDbType.UniqueIdentifier, dto.Pk);

		using var reader = cmd.ExecuteReader();

		while (reader.Read())
		{
			var settingsXml = (string)reader["SST_Configuration"];

			dto = NativeServiceTaskDTO.CreateFromReader(reader);
			UpdateGovernedTask(dto);
			scheduleConfig = NativeServiceTask.Deserialize(settingsXml);
		}
	}

	public void OnErrorReported()
	{
		// no action required
	}

	public void Save()
	{
		if (entityMap.Count == 0)
		{
			return;
		}

		if (IsGoverningNewServiceTask)
		{
			Insert();
		}
		else
		{
			Update();
		}

		ResetEntityMap();
	}

	public IServiceTask GovernedTask => governedTask ??= new NativeServiceTask(dto, attributeProvider, bindingsProvider, statusProvider, dateTimeProvider, assignedGovernor: this);

	internal Dictionary<string, (string ParameterName, SqlDbType ParameterType, object ParameterValue)> EntityMap => entityMap;

	string ServiceTaskCode => dto.ServiceTaskCode;

	bool IsGoverningNewServiceTask => dto.Pk == Guid.Empty;

	ScheduleConfig ScheduleConfig
	{
		get
		{
			if (scheduleConfig != null)
			{
				return scheduleConfig;
			}

			scheduleConfig = IsGoverningNewServiceTask ? new ScheduleConfig() : NativeServiceTask.Deserialize(dto.SettingsXml);
			return scheduleConfig;
		}
	}

	public INextRunTimeCalculator Calculator
	{
		get
		{
			if (calculator != null)
			{
				return calculator;
			}

			calculator = (INextRunTimeCalculator)ScheduleConfig.Calculator!;
			return calculator;
		}
		private set
		{
			calculator = value;
		}
	}

	void AddToEntityMap(string colName, (string ParameterName, SqlDbType ParameterType, object ParameterValue) sqlParam)
	{
		entityMap[colName] = sqlParam;
	}

	void ResetEntityMap()
	{
		entityMap.Clear();
	}

	void Insert()
	{
		var fields = new List<string>();
		var sqlParamNames = new List<string>();
		foreach (var kvp in entityMap)
		{
			fields.Add(kvp.Key);
			sqlParamNames.Add(kvp.Value.ParameterName);
		}

		var sqlText = $@"
INSERT INTO dbo.StmServiceTask
(SST_PK, SST_ServiceTaskCode, SST_SystemCreateTimeUtc, SST_SystemCreateUser, SST_SystemLastEditTimeUtc, SST_SystemLastEditUser{(fields.Count > 0 ? ", " : "")}{string.Join(", ", fields)})
VALUES
(NewId(), @ServiceTaskCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP'{(sqlParamNames.Count > 0 ? ", " : "")}{string.Join(", ", sqlParamNames)})

SELECT TOP 1
	ServiceTask.SST_PK,
	ServiceTask.SST_ServiceTaskCode,
	ServiceTask.SST_Active,
	ServiceTask.SST_NextRunTime,
	ServiceTask.SST_LastRunTime,
	ServiceTask.SST_Configuration,
	Branch.GB_PK,
	Branch.GB_Code
FROM dbo.StmServiceTask ServiceTask
LEFT JOIN dbo.GlbBranch branch on ServiceTask.SST_GB_Branch = Branch.GB_PK
WHERE SST_ServiceTaskCode = @ServiceTaskCode
";
		using var cmd = Db.Connection.Command(sqlText);
		foreach (var keyToParameter in entityMap)
		{
			var parameter = keyToParameter.Value;
			cmd.AddParameter(parameter.ParameterName, parameter.ParameterType, parameter.ParameterValue);
		}
		cmd.AddParameter("@ServiceTaskCode", SqlDbType.VarChar, ServiceTaskCode);
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			dto = NativeServiceTaskDTO.CreateFromReader(reader);
			UpdateGovernedTask(dto);

			scheduleConfig = NativeServiceTask.Deserialize(dto.SettingsXml);
		}
	}

	void Update()
	{
		var sqlText = $@"
UPDATE dbo.StmServiceTask
SET
	SST_SystemLastEditTimeUtc = GetUtcDate(),
	SST_SystemLastEditUser = '~BP',
	{string.Join(", ", entityMap.Select((kvp) => $"{kvp.Key} = {kvp.Value.ParameterName}"))}
WHERE SST_PK = @ServiceTaskPk

SELECT TOP 1
	ServiceTask.SST_PK,
	ServiceTask.SST_ServiceTaskCode,
	ServiceTask.SST_Active,
	ServiceTask.SST_NextRunTime,
	ServiceTask.SST_LastRunTime,
	ServiceTask.SST_Configuration,
	Branch.GB_PK,
	Branch.GB_Code
FROM dbo.StmServiceTask ServiceTask
LEFT JOIN dbo.GlbBranch branch on ServiceTask.SST_GB_Branch = Branch.GB_PK
WHERE SST_PK = @ServiceTaskPk
";
		using var cmd = Db.Connection.Command(sqlText);
		foreach (var keyToParameter in entityMap)
		{
			var parameter = keyToParameter.Value;
			cmd.AddParameter(parameter.ParameterName, parameter.ParameterType, parameter.ParameterValue);
		}
		cmd.AddParameter("@ServiceTaskPk", SqlDbType.UniqueIdentifier, dto.Pk);
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			dto = NativeServiceTaskDTO.CreateFromReader(reader);
			UpdateGovernedTask(dto);

			scheduleConfig = NativeServiceTask.Deserialize(dto.SettingsXml);
		}
	}

	TimeSpan GetUtcOffset()
	{
		Guid branchPk = dto.BranchPk ?? Guid.Empty;
		string? unloco = null;

		if (entityMap.ContainsKey("SST_GB_Branch"))
		{
			branchPk = (Guid)entityMap["SST_GB_Branch"].ParameterValue;
		}

		if (branchPk != Guid.Empty)
		{
			var branchQuery = "SELECT TOP 1 [GB_RL_NKHomePort] FROM [dbo].[GlbBranch] WHERE [GB_PK] = @BranchPk";

			using var cmd = Db.Connection.Command(branchQuery);
			cmd.AddParameter("@BranchPk", SqlDbType.UniqueIdentifier, branchPk);

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				unloco = (string)reader["GB_RL_NKHomePort"];
			}
		}

		var now = dateTimeProvider.CurrentDateTimeUtc;
		if (unloco != null)
		{
			return ServiceTaskScheduleHelper.GetUtcOffsetBasedOnUtc(unloco, now);
		}

		return ServiceTaskScheduleHelper.GetUtcOffsetBasedOnUtc(now);
	}

	static TimeSpan ApplyOffset(TimeSpan time, TimeSpan offset)
	{
		TimeSpan result;
		if (offset < TimeSpan.Zero)
		{
			result = time - offset > TimeSpan.FromHours(24)
				? time - offset - TimeSpan.FromHours(24)
				: time - offset;
		}
		else
		{
			result = time - offset < TimeSpan.Zero
				? time - offset + TimeSpan.FromHours(24)
				: time - offset;
		}

		return result;
	}

	void SetGovernedTaskForNewSchedule()
	{
		governedTask ??= new NativeServiceTask(
			dto,
			attributeProvider,
			bindingsProvider,
			statusProvider,
			dateTimeProvider,
			scheduleConfig);
	}

	void UpdateGovernedTask(NativeServiceTaskDTO newDto)
	{
		governedTask = new NativeServiceTask(
			newDto,
			attributeProvider,
			bindingsProvider,
			statusProvider,
			dateTimeProvider,
			assignedGovernor: this);
	}

	INextRunTimeCalculator? calculator;
	NativeServiceTaskDTO dto;
	NativeServiceTask? governedTask;
	ScheduleConfig? scheduleConfig;

	readonly Dictionary<string, (string ParameterName, SqlDbType ParameterType, object ParameterValue)> entityMap;
	readonly Lazy<IHostedServiceAttribute> lazyStaticServiceSettings;
	readonly IClientHostedServiceAttributeProvider attributeProvider;
	readonly IServiceTaskScheduleStatusProvider statusProvider;
	readonly IHostedServiceBusinessObjectBindingsProvider bindingsProvider;
	readonly IDateTimeProvider dateTimeProvider;
}
