using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business
{
	[CodeProperty(Schema.SST_ServiceTaskCode)]
	public class StmServiceTask : AutoStmServiceTask, IStmServiceTask, IStmServiceTaskRecurrenceControlDataProvider, IStmServiceTaskConfigControlDataProvider, IStmServiceTaskDefaultScheduleControlDataProvider
	{
		public StmServiceTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			lazyServiceTaskBindings = new Lazy<ICollection<string>>(
				() => HostedServiceBusinessObjectBindingsProvider
					.Instance
					.BusinessObjectBindings
					.Where(binding => binding.ServiceTaskCode.Equals(SST_ServiceTaskCode))
					.Select(binding => binding.Table)
					.Distinct()
					.ToArray());

			lazyStaticServiceSettings = new Lazy<IHostedServiceAttribute>(() =>
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>().GetClientHostedServiceAttribute(SST_ServiceTaskCode));
		}

		protected override bool MatchesFilterCore(ZQuery filter, DataRow row, DataTable table, string tableName, string identifier)
		{
			var compositeFilters = new StmServiceTaskFilterHelper(filter, Factory);
			return base.MatchesFilterCore(compositeFilters.TableSchemaFilters, row, table, tableName, identifier);
		}

		protected override ZString HumanReadableNameCore => SST_ServiceTaskCode;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ExtractBaseXmlConfig();
			nextRunTimeCalculator ??= new NextRunTimeCalculatorDays { Period = 1, ScheduledRunTime = TimeSpan.Zero };
		}

		public void OnStatusUpdateComplete(TaskInstanceStatus statusCopySource)
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
			ErrorCountLast24Hours = statusCopySource.ErrorCountLast24Hours;
			LastErrorTime = statusCopySource.LastErrorTime ?? ZDateTime.Empty;
			lastRunTime = statusCopySource.LastRunTime ?? ZDateTime.Empty;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			ExtractBaseXmlConfig();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (NextRunTimeHasBeenSet)
				{
					var nextRunTime = NextRunTime.ToNullableDateTime();
					StatusProvider.SetServiceTaskNextRuntime(SST_ServiceTaskCode, nextRunTime);
					NextRunTimeHasBeenSet = false;
				}
				StatusProvider.RequestTaskConfigurationReload(SST_ServiceTaskCode);
			}
		}

		public override ZBool SST_Active
		{
			get { return base.SST_Active; }
			set
			{
				if (value && !SST_Active)
				{
					SST_NextRunTime = Recurrence.HasErrors
						? ZDateTimeOffset.UtcNow.ToDateTimeOffset()
						: NextRunTimeCalculator.CalculateNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset());
				}
				base.SST_Active = value;
			}
		}

		public bool SST_Active_ReadOnly
		{
			get
			{
				return IsScheduleReadOnly || StaticServiceAttributes.IsMandatory;
			}
		}

		public bool IsScheduleReadOnly
		{
			get
			{
				return StaticServiceAttributes.IsScheduleReadOnly
							|| (StaticServiceAttributes.IsReadOnlyForWiseCloudClient
								&& EnvProxy.IsHostedWithCargowise
								&& !EnvProxy.Instance.CurrentUser.IsSupportUser);
			}
		}

		public bool IsNudgeable
		{
			get
			{
				return ServiceTaskBindingsCount > 0 || ServiceConfig.IsConfiguredForNudging && SharedRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled;
			}
		}

		HostedServiceConfiguration ServiceConfig
		{
			get
			{
				return hostedServiceConfig ??= new HostedServiceConfiguration(StaticServiceAttributes);
			}
		}

		INextRunTimeCalculator nextRunTimeCalculator;
		public INextRunTimeCalculator NextRunTimeCalculator
		{
			get
			{
				return nextRunTimeCalculator;
			}
			set
			{
				nextRunTimeCalculator = value;
				base.SST_Configuration = Serialize(value, secondaryProcessesMaxCount, configString);
				base.SST_ConfigurationInfo.RefreshBinding();

				SST_NextRunTimeInfo.RefreshBinding();
				Recurrence.Validation.ValidateAll();

				SST_NextRunTime = Recurrence.HasErrors
					? ZDateTimeOffset.UtcNow.ToDateTimeOffset()
					: new ZDateTimeOffset(nextRunTimeCalculator.CalculateNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset()));
			}
		}

		ZInt secondaryProcessesMaxCount;

		public ZInt SecondaryProcessesMaxCount
		{
			get
			{
				if (secondaryProcessesMaxCountDescription.IsEmpty)
				{
					SetDefaultSecondaryProcessesMaxCount();
				}

				return secondaryProcessesMaxCount;
			}
			set
			{
				secondaryProcessesMaxCount = value;
				SecondaryProcessesMaxCountInfo.RefreshBinding();
				base.SST_Configuration = Serialize(nextRunTimeCalculator, secondaryProcessesMaxCount, configString);
				base.SST_ConfigurationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecondaryProcessesMaxCount();
				}
			}
		}

		public ZPropertyInfo SecondaryProcessesMaxCountInfo
		{
			get { return GetZPropertyInfo(nameof(SecondaryProcessesMaxCount), Res.GetString("b6668770-165a-445e-976b-7baaba6b7ba8", "Secondary Processes Max Count")); }
		}

		string configString;

		public string ConfigString
		{
			get => configString;
			set
			{
				configString = value;
				base.SST_Configuration = Serialize(nextRunTimeCalculator, secondaryProcessesMaxCount, configString);
				base.SST_ConfigurationInfo.RefreshBinding();
			}
		}

		void ExtractBaseXmlConfig()
		{
			if (string.IsNullOrEmpty(base.SST_Configuration))
			{
				return;
			}

			try
			{
				var scheduleConfig = Deserialize(base.SST_Configuration);

				nextRunTimeCalculator = (INextRunTimeCalculator)scheduleConfig.Calculator;

				if (!base.SST_Configuration.Contains("SecondaryProcessesMaxCount"))
				{
					SetDefaultSecondaryProcessesMaxCount();
				}
				else
				{
					secondaryProcessesMaxCount = scheduleConfig.SecondaryProcessesMaxCount;
				}

				secondaryProcessesMaxCountDescription = StmServiceTaskSecondaryProcessesMaxCountsList.GetDescriptionFromCode(secondaryProcessesMaxCount.ToString());
				configString = scheduleConfig.ConfigString;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				nextRunTimeCalculator = null;
				configString = null;
				secondaryProcessesMaxCount = ZInt.Zero;
				secondaryProcessesMaxCountDescription = ZString.Empty;
			}
		}

		void SetDefaultSecondaryProcessesMaxCount()
		{
			if (secondaryProcessesMaxCount.IsEmpty)
			{
				secondaryProcessesMaxCount =
					StaticServiceAttributes.AllowsMultipleInstances
						? ScheduleConfig.MaxSecondaryProcessesMaxCount
						: 0;
			}
		}

		static ZString Serialize(INextRunTimeCalculator value, int secondaryProcessesMaxCount, string configString)
		{
			var scheduleConfig = new ScheduleConfig()
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
			serializer.Serialize(xmlWriter, scheduleConfig, ns);

			xmlWriter.Close();
			return new ZString(stringWriter.ToString());
		}

		static ScheduleConfig Deserialize(string value)
		{
			var settings = new XmlReaderSettings
			{
				CloseInput = false,
				IgnoreComments = true,
			};

			using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
			using var xmlReader = XmlReader.Create(memoryStream, settings);

			var serializer = new XmlSerializer(typeof(ScheduleConfig));
			return (ScheduleConfig)serializer.Deserialize(xmlReader);
		}

		HostedServiceConfiguration hostedServiceConfig;

		static ZDateTime GetLocalDate(ZDateTime utcDate) => (utcDate.IsEmpty || !utcDate.IsValid)
			? utcDate
			: Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime());

		#region LogViewer

		public ServiceTaskLogViewer LogViewer => logViewer ??=
			new ServiceTaskLogViewer()
			{
				FileBasedLogViewer = { TaskType = SST_ServiceTaskCode, TaskTypeReadOnly = true },
				SearchBasedLogViewer = { ServiceTaskCode = SST_ServiceTaskCode },
			};
		ServiceTaskLogViewer logViewer;

		#endregion

		public ZString Description => StaticServiceAttributes.Description;
		public MultilingualString DescriptionMultilingual
		{
			get { return CustomizableDataResourceStrings.GetMultilingualString(null, Description); }
		}
		public ZString Category => StaticServiceAttributes.Category;
		public ZString MutuallyExclusiveGroup => StaticServiceAttributes.MutuallyExclusiveTaskGroup.ToString();
		public ZString StatusString { get; set; }
		public ZInt RunningCount { get; set; }
		public ZString PlaceInQueueString { get; set; }
		public ZString SecondsInQueueString { get; set; }
		public ZString SecondsRunningString { get; set; }
		public ZString ProcessIDsString { get; set; }
		public ZString RegisteredOnHosts { get; set; }
		public ZInt ErrorCountLast24Hours { get; set; }
		public ZDateTime LastErrorTime { get; set; }
		public ZDateTime LastErrorTimeLocal => GetLocalDate(LastErrorTime);
		bool NextRunTimeHasBeenSet { get; set; }
		public ZDateTime NextRunTime
		{
			get => SST_NextRunTime.IsValid ? SST_NextRunTime.ToUtcZDateTime() : ZDateTime.Empty;
			set
			{
				if (value.IsValid)
				{
					NextRunTimeHasBeenSet = true;
					SST_NextRunTime = value.UtcToDateTimeOffset();
					SST_NextRunTimeInfo.RefreshBinding();
				}
			}
		}

		IServiceTaskScheduleStatusProvider statusProvider;
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

		public ZDateTime NextRunTimeLocal => GetLocalDate(NextRunTime);
		public ZDateTime LastRunTime => lastRunTime.IsEmpty
			? SST_LastRunTime.IsValid ? SST_LastRunTime.ToUtcZDateTime() : ZDateTime.Empty
			: lastRunTime;
		public ZDateTime LastRunTimeLocal => GetLocalDate(LastRunTime);

		ZDateTime lastRunTime;

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

		TimeSpan DefaultScheduleDuration => defaultScheduleDuration ??= ServiceTaskScheduleHelper.GetPeriodDuration(StaticServiceAttributes.DefaultSchedule.RunEvery, TimeSpan.FromMinutes(15), isRandomPeriod: false);
		TimeSpan? defaultScheduleDuration;

		readonly Lazy<TimeSpan> minimumNudgeableScheduleDuration = new (() => TimeSpan.FromMinutes(15));

		TimeSpan ConfiguredScheduleDuration => NextRunTimeCalculator switch
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

		public ZInt ServiceTaskBindingsCount => lazyServiceTaskBindings.Value.Count;
		public ZString ServiceTaskBindingTypesString => new ZString(string.Join(",", lazyServiceTaskBindings.Value));
		public IHostedServiceAttribute StaticServiceAttributes => lazyStaticServiceSettings.Value;

		readonly Lazy<ICollection<string>> lazyServiceTaskBindings;
		readonly Lazy<IHostedServiceAttribute> lazyStaticServiceSettings;

		CustomizableDataResourceStrings CustomizableDataResourceStrings =>
			customizableDataResourceStrings ??= new CustomizableDataResourceStrings(ObjectFactory.Get<ICustomizableDataCaptionSource>("StmScheduleTaskDescriptionCaptionSource"));

		CustomizableDataResourceStrings customizableDataResourceStrings;

		public StmServiceTaskAdapter Recurrence
		{
			get
			{
				if (recurrence == null)
				{
					recurrence = new StmServiceTaskAdapter(this);
					RegisterEditableChildObject(recurrence);
				}
				return recurrence;
			}
		}

		StmServiceTaskAdapter recurrence;

		public StmServiceTaskAdapter ConfigAdapter
		{
			get
			{
				if (configAdapter == null)
				{
					configAdapter = new StmServiceTaskAdapter(Factory, this);
					RegisterEditableChildObject(configAdapter);
				}
				return configAdapter;
			}
		}

		StmServiceTaskAdapter configAdapter;

		ZString secondaryProcessesMaxCountDescription;

		[List("StmServiceTaskSecondaryProcessesMaxCountsList")]
		public ZString SecondaryProcessesMaxCountDescription
		{
			get
			{
				if (secondaryProcessesMaxCountDescription.IsEmpty)
				{
					secondaryProcessesMaxCountDescription =
						StmServiceTaskSecondaryProcessesMaxCountsList.GetDescriptionFromCode(SecondaryProcessesMaxCount.ToString());
				}

				return secondaryProcessesMaxCountDescription;
			}
			set
			{
				var code = StmServiceTaskSecondaryProcessesMaxCountsList.GetCodeFromDescription(value);
				SecondaryProcessesMaxCount = Convert.ToInt32(code ?? ScheduleConfig.InvalidSecondaryProcessesMaxCount.ToString());
				SetNonPersistentPropertyValue(SecondaryProcessesMaxCountDescriptionInfo, ref secondaryProcessesMaxCountDescription, value, false);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecondaryProcessesMaxCountDescription();
				}
			}
		}

		public ZPropertyInfo SecondaryProcessesMaxCountDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SecondaryProcessesMaxCountDescription), Res.GetString("8D4E054E-3FD4-4FF6-A6D7-A41A2AB7F45C", "Secondary Processes Max Count Description")); }
		}

		public CodeDescriptionPairList StmServiceTaskSecondaryProcessesMaxCountsList
		{
			get
			{
				if (stmServiceTaskSecondaryProcessesMaxCountsList == null)
				{
					stmServiceTaskSecondaryProcessesMaxCountsList = new CodeDescriptionPairList();
					stmServiceTaskSecondaryProcessesMaxCountsList.AddPair(ScheduleConfig.MaxSecondaryProcessesMaxCount.ToString(), Res.GetString("0C64523E-C553-4970-B842-D8CB7D157DAD", "SYSTEM MANAGED"));
					Enumerable.Range(0, ScheduleConfig.MaxSecondaryProcessesMaxCount).ForEach(i => stmServiceTaskSecondaryProcessesMaxCountsList.AddPair(i.ToString(), i.ToString()));
				}

				return stmServiceTaskSecondaryProcessesMaxCountsList;
			}
		}

		CodeDescriptionPairList stmServiceTaskSecondaryProcessesMaxCountsList;

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

		public NextRunTimeEstimator NextRunTimeEstimator => nextRunTimeEstimator ??= new NextRunTimeEstimator(this);

		NextRunTimeEstimator nextRunTimeEstimator;

		public StmServiceTaskAdapter DefaultSchedule
		{
			get
			{
				if (defaultSchedule == null)
				{
					var taskGovernor = ObjectFactory
						.Get<IServiceTaskGovernorFactory>()
						.GetServiceTaskGovernor(StaticServiceAttributes);

					var configurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());
					configurer.SetDefaultScheduleForTaskForDisplay(taskGovernor, StaticServiceAttributes);

					var nativeServiceTask = new ReadOnlyBusinessObjectFactory().New<StmServiceTask>();
					nativeServiceTask.SST_ServiceTaskCode = SST_ServiceTaskCode;
					nativeServiceTask.NextRunTimeCalculator = taskGovernor.Calculator;

					defaultSchedule = new StmServiceTaskAdapter(nativeServiceTask);
				}

				return defaultSchedule;
			}
		}

		StmServiceTaskAdapter defaultSchedule;
	}
}
