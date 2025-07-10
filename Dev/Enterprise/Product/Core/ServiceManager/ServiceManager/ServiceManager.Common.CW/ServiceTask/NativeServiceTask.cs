using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW;

public class NativeServiceTask : IServiceTask
{
	internal NativeServiceTask(
		NativeServiceTaskDTO dto,
		IClientHostedServiceAttributeProvider attributeProvider,
		IHostedServiceBusinessObjectBindingsProvider bindingsProvider,
		IServiceTaskScheduleStatusProvider statusProvider,
		IDateTimeProvider dateTimeProvider,
		ScheduleConfig? config = null,
		IServiceTaskGovernor? assignedGovernor = null)
	{
		taskDto = dto;
		scheduleConfig = config ?? (dto.Pk == Guid.Empty
			? new ScheduleConfig()
			: Deserialize(dto.SettingsXml));

		this.assignedGovernor = assignedGovernor;
		this.attributeProvider = attributeProvider ?? throw new ArgumentNullException(nameof(attributeProvider));
		this.bindingsProvider = bindingsProvider ?? throw new ArgumentNullException(nameof(bindingsProvider));
		this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
		this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

		lazyStaticServiceSettings = new Lazy<IHostedServiceAttribute>(() =>
			attributeProvider.GetClientHostedServiceAttribute(dto.ServiceTaskCode));
		lazyServiceTaskBindings = new Lazy<ICollection<string>>(
			() => bindingsProvider
				.BusinessObjectBindings
				.Where(binding => binding.ServiceTaskCode.Equals(dto.ServiceTaskCode))
				.Select(binding => binding.Table)
				.Distinct()
				.ToArray());
	}

	public Guid Pk => taskDto.Pk;
	public bool IsActive => taskDto.IsActive;
	public DateTimeOffset NextRunTime => taskDto.NextRunTime;

	public TimeSpan DailyStartTime
	{
		get
		{
			return scheduleConfig.Calculator switch
			{
				NextRunTimeCalculatorSeconds calculatorSeconds => calculatorSeconds.StartTime ?? TimeSpan.Zero,
				NextRunTimeCalculatorMinutes calculatorMinutes => calculatorMinutes.StartTime ?? TimeSpan.Zero,
				NextRunTimeCalculatorHours calculatorHours => calculatorHours.StartTime ?? TimeSpan.Zero,
				_ => TimeSpan.Zero
			};
		}
	}

	public TimeSpan DailyEndTime
	{
		get
		{
			return scheduleConfig.Calculator switch
			{
				NextRunTimeCalculatorSeconds calculatorSeconds => calculatorSeconds.EndTime ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59)),
				NextRunTimeCalculatorMinutes calculatorMinutes => calculatorMinutes.EndTime ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59)),
				NextRunTimeCalculatorHours calculatorHours => calculatorHours.EndTime ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59)),
				_ => TimeSpan.Zero
			};
		}
	}

	public string Code => taskDto.ServiceTaskCode;
	public string Description => lazyStaticServiceSettings.Value.Description;
	public string MultilingualDescription => Description;
	public Guid BranchPk => taskDto.BranchPk ?? Guid.Empty;
	public string BranchName => string.IsNullOrEmpty(taskDto.BranchName) ? string.Empty : taskDto.BranchName;
	public string BranchErrorMessage => taskDto.BranchErrorMessage;
	public string ConfigString => scheduleConfig.ConfigString ?? string.Empty;
	public string SettingsXml => taskDto.SettingsXml;
	public int SecondaryProcessesMaxCount => scheduleConfig.SecondaryProcessesMaxCount;

	public IReadOnlyList<DayOfWeek> ScheduleDaysOfWeek
	{
		get
		{
			if (scheduleConfig.Calculator is NextRunTimeCalculatorWeeks calculatorWeeks)
			{
				return calculatorWeeks.DaysOfOccurrence;
			}

			return Array.Empty<DayOfWeek>();
		}
	}

	public int ScheduleDayOfMonth
	{
		get
		{
			if (scheduleConfig.Calculator is NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate)
			{
				return calculatorMonthsByDate.DayOfOccurrence;
			}

			return 1;
		}
	}

	public int ScheduleDayNumber
	{
		get
		{
			return scheduleConfig.Calculator switch
			{
				NextRunTimeCalculatorMonthsByDayOfWeek calculatorMonthsByDayOfWeek => (int)calculatorMonthsByDayOfWeek.DayOfTheWeek + 1,
				NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth => (int)calculatorYearsByDayOfMonth.DayOfTheWeek + 1,
				_ => 0
			};
		}
	}

	public int ScheduleMonth
	{
		get
		{
			return scheduleConfig.Calculator switch
			{
				NextRunTimeCalculatorYearsByDate calculatorYearsByDate => calculatorYearsByDate.Month,
				NextRunTimeCalculatorYearsByDayOfMonth calculatorYearsByDayOfMonth => calculatorYearsByDayOfMonth.MonthOfTheYear,
				_ => 1
			};
		}
	}

	public DateTimeOffset ScheduleStartDate => NextRunTime;

	public string ScheduleOccurrence
	{
		get
		{
			return scheduleConfig.Calculator switch
			{
				NextRunTimeCalculatorSeconds _ => "S",
				NextRunTimeCalculatorMinutes _ => "T",
				NextRunTimeCalculatorHours _ => "H",
				NextRunTimeCalculatorDays _ => "D",
				NextRunTimeCalculatorWorkingDays _ => "D",
				NextRunTimeCalculatorWeeks _ => "W",
				NextRunTimeCalculatorMonthsByDate _ => "M",
				NextRunTimeCalculatorMonthsByDayOfWeek _ => "M",
				NextRunTimeCalculatorMonthsByLastDay _ => "M",
				NextRunTimeCalculatorYearsByDate _ => "Y",
				NextRunTimeCalculatorYearsByDayOfMonth _ => "Y",
				_ => string.Empty
			};
		}
	}

	public int ScheduleFrequency
	{
		get
		{
			return scheduleConfig.Calculator switch
			{
				NextRunTimeCalculatorSeconds calculator => calculator.Period,
				NextRunTimeCalculatorMinutes calculator => calculator.Period,
				NextRunTimeCalculatorHours calculator => calculator.Period,
				NextRunTimeCalculatorDays calculator => calculator.Period,
				NextRunTimeCalculatorWorkingDays _ => 1,
				NextRunTimeCalculatorWeeks calculator => calculator.Period,
				NextRunTimeCalculatorMonthsByDate calculator => calculator.Period,
				NextRunTimeCalculatorMonthsByDayOfWeek _ => 1,
				NextRunTimeCalculatorMonthsByLastDay calculator => calculator.Period,
				NextRunTimeCalculatorYearsByDate _ => 1,
				NextRunTimeCalculatorYearsByDayOfMonth _ => 1,
				_ => 1
			};
		}
	}

	public bool WeekDaysOnly
	{
		get
		{
			return scheduleConfig.Calculator is NextRunTimeCalculatorWorkingDays;
		}
	}

	public string Category => lazyStaticServiceSettings.Value.Category;

	public IServiceTaskGovernor AssignedGovernor => assignedGovernor ??= new NativeServiceTaskGovernor(taskDto, attributeProvider, statusProvider, bindingsProvider, dateTimeProvider);

	public void PreRunValidation() { }

	public void EnsureThreadSafety() { }

	public DateTimeOffset CalculateNextRunTime(ILogger logger)
	{
		var calculator = (INextRunTimeCalculator)scheduleConfig.Calculator!;
		var calculatedNextRunTime = calculator.CalculateNextRunTime(NextRunTime);
		var now = new DateTimeOffset(dateTimeProvider.CurrentDateTimeUtc, TimeSpan.Zero);

		if (NextRunTimeExpired(calculatedNextRunTime, now) || NextRunTime > now)
		{
			logger.Log(LogLevel.Debug, "Next runtime was out of the expected range, so it was recalculated based off 'now'");
			return calculator.CalculateNextRunTime(now);
		}

		return calculatedNextRunTime;

		bool NextRunTimeExpired(DateTimeOffset newNextRunTime, DateTimeOffset nowTime)
		{
			var allowableDeviationInMinutes = Math.Min((int)Math.Ceiling((newNextRunTime - NextRunTime).TotalMinutes * 0.25), 60);
			return nowTime.AddMinutes(-allowableDeviationInMinutes) > NextRunTime;
		}
	}

	public static string Serialize(INextRunTimeCalculator value, int secondaryProcessesMaxCount, string configString)
	{
		return Serialize(value, secondaryProcessesMaxCount, configString, out _);
	}

	public static string Serialize(INextRunTimeCalculator value, int secondaryProcessesMaxCount, string configString, out ScheduleConfig scheduleConfig)
	{
		scheduleConfig = new ScheduleConfig()
		{
			Calculator = value,
			SecondaryProcessesMaxCount = secondaryProcessesMaxCount,
			ConfigString = configString
		};

		var settings = new XmlWriterSettings
		{
			OmitXmlDeclaration = true,
			CloseOutput = true,
			Encoding = Encoding.UTF8,
		};

		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(stringWriter, settings);

		var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
		var serializer = new XmlSerializer(typeof(ScheduleConfig));
		serializer.Serialize(xmlWriter, (object?)scheduleConfig, ns);

		xmlWriter.Close();
		return stringWriter.ToString();
	}

	public static ScheduleConfig Deserialize(string value)
	{
		var settings = new XmlReaderSettings
		{
			CloseInput = false,
			IgnoreComments = true,
		};

		using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
		using var xmlReader = XmlReader.Create(memoryStream, settings);

		var serializer = new XmlSerializer(typeof(ScheduleConfig));
		return serializer.Deserialize(xmlReader) as ScheduleConfig ?? new ScheduleConfig();
	}

	public TimeSpan SchedulePeriodDuration
	{
		get
		{
			var nudgeableDuration = (DefaultScheduleDuration > minimumNudgeableScheduleDuration)
				? DefaultScheduleDuration
				: minimumNudgeableScheduleDuration;

			return IsNudgeable ? nudgeableDuration : ConfiguredScheduleDuration;
		}
	}

	public TimeSpan OverdueDuration => ServiceTaskScheduleHelper.GetPeriodDuration(
		lazyStaticServiceSettings.Value.DefaultSchedule.DoNotRunTillNextDueTimeIfOverdue, TimeSpan.Zero, false);

	TimeSpan DefaultScheduleDuration => defaultScheduleDuration ??= ServiceTaskScheduleHelper.GetPeriodDuration(lazyStaticServiceSettings.Value.DefaultSchedule.RunEvery, TimeSpan.FromMinutes(15), isRandomPeriod: false);
	TimeSpan? defaultScheduleDuration;

	readonly TimeSpan minimumNudgeableScheduleDuration = TimeSpan.FromMinutes(15);

	bool IsNudgeable => (ServiceTaskBindingsCount > 0 || (ServiceConfig?.IsConfiguredForNudging ?? false)) && SharedRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled;

	int ServiceTaskBindingsCount => lazyServiceTaskBindings.Value.Count;

	TimeSpan ConfiguredScheduleDuration => scheduleConfig.Calculator switch
	{
		NextRunTimeCalculatorSeconds calculatorSeconds => TimeSpan.FromSeconds(calculatorSeconds.Period),
		NextRunTimeCalculatorMinutes calculatorMinutes => TimeSpan.FromMinutes(calculatorMinutes.Period),
		NextRunTimeCalculatorHours calculatorHours => TimeSpan.FromHours(calculatorHours.Period),
		NextRunTimeCalculatorDays calculatorDays => TimeSpan.FromDays(calculatorDays.Period),
		NextRunTimeCalculatorWorkingDays => TimeSpan.FromDays(1),
		NextRunTimeCalculatorWeeks calculatorWeeks => TimeSpan.FromDays(calculatorWeeks.Period * 7),
		NextRunTimeCalculatorMonthsByDate calculatorMonthsByDate => TimeSpan.FromDays(calculatorMonthsByDate.Period * 28),
		NextRunTimeCalculatorMonthsByLastDay calculatorMonthsByLastDay => TimeSpan.FromDays(calculatorMonthsByLastDay.Period * 28),
		NextRunTimeCalculatorMonthsByDayOfWeek => TimeSpan.FromDays(28),
		NextRunTimeCalculatorYearsByDate or NextRunTimeCalculatorYearsByDayOfMonth => TimeSpan.FromDays(365),
		_ => TimeSpan.Zero
	};

	HostedServiceConfiguration? ServiceConfig
	{
		get
		{
			if (hostedServiceConfig == null)
			{
				var serviceAttributes = lazyStaticServiceSettings.Value;
				if (serviceAttributes != null)
				{
					hostedServiceConfig = new HostedServiceConfiguration(serviceAttributes);
				}
			}
			return hostedServiceConfig;
		}
	}

	HostedServiceConfiguration? hostedServiceConfig;
	IServiceTaskGovernor? assignedGovernor;

	readonly NativeServiceTaskDTO taskDto;
	readonly Lazy<IHostedServiceAttribute> lazyStaticServiceSettings;
	readonly ScheduleConfig scheduleConfig;
	readonly Lazy<ICollection<string>> lazyServiceTaskBindings;
	readonly IServiceTaskScheduleStatusProvider statusProvider;
	readonly IClientHostedServiceAttributeProvider attributeProvider;
	readonly IHostedServiceBusinessObjectBindingsProvider bindingsProvider;
	readonly IDateTimeProvider dateTimeProvider;
}
