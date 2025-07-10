using System;
using System.IO;
using System.Linq;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ServiceManager.Business
{
	public class SearchBasedLogViewer : NonPersistentBusinessObject
	{
		[CodeAlive("Used in NonPersistentBusinessObject construction via Activator.CreateInstance")]
		public SearchBasedLogViewer() : this(new SearchBasedLogViewerDataProviderFactory())
		{
		}

		public SearchBasedLogViewer(ISearchBasedLogViewerDataProviderFactory providerFactory) : base(new BusinessObjectFactory())
		{
			searchBasedLogViewerDataProviderFactory = providerFactory;
			HostName = "All";
			ProcessId = "All";
			Severity = nameof(LogType.Information);
			FromDateTimeUtc = ZDateTime.UtcNow.AddHours(-24);
			ReloadEvents();
		}

		public override bool HasChanges
		{
			get => false;
		}

		#region ServiceTaskCode
		[CargoWise.ComponentModel.MaxLength(4)]
		[CargoWise.ComponentModel.Mandatory]
		public ZString ServiceTaskCode
		{
			get => serviceTaskCode;
			set
			{
				CheckMaximumLength(ServiceTaskCodeInfo, value);
				SetNonPersistentPropertyValue(ServiceTaskCodeInfo, ref serviceTaskCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateServiceTaskCode();
				}
			}
		}

		ZString serviceTaskCode;

		public ZPropertyInfo ServiceTaskCodeInfo => GetZPropertyInfo(nameof(ServiceTaskCode));

		public CodeDescriptionPairList ServiceTaskCodes
		{
			get
			{
				if (serviceTaskCodes == null)
				{
					serviceTaskCodes = new CodeDescriptionPairList();
					serviceTaskCodes.AddPair(ServiceManagerHelper.HostLoggerCode, ResString.GetMultilingualString("AF8F626B-D8AA-41DD-8D87-46F638B8FD42", "Service Host"));
					if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
					{
						var serviceTaskCollection = new StmServiceTaskCollection(Factory);
						serviceTaskCollection.Load();
						foreach (StmServiceTask serviceTask in serviceTaskCollection.OrderBy(x => x[StmServiceTaskSchema.SST_ServiceTaskCode]))
						{
							serviceTaskCodes.AddPairIfNotExist(serviceTask.SST_ServiceTaskCode, serviceTask.DescriptionMultilingual);
						}
					}
					else
					{
						var taskScheduleCollection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithoutStatus);
						taskScheduleCollection.Load();
						foreach (ServiceTaskSchedule taskSchedule in taskScheduleCollection.OrderBy(x => x[StmScheduleTaskSchema.S5_ScheduleType]))
						{
							serviceTaskCodes.AddPairIfNotExist(taskSchedule.S5_ScheduleType, taskSchedule.S5_ScheduleDescriptionMultilingual);
						}
					}
				}

				return serviceTaskCodes;
			}
		}

		CodeDescriptionPairList serviceTaskCodes;
		#endregion

		public ZString Severity
		{
			get => severity;
			set => severity = SeverityList.ContainsCode(value) ? value : nameof(LogType.Information);
		}
		ZString severity;

		public CodeDescriptionPairList SeverityList
		{
			get
			{
				if (severityCodes == null)
				{
					severityCodes = new CodeDescriptionPairList();
					severityCodes.AddPair(nameof(LogType.Error), ResString.GetMultilingualString("3c100729-e979-461c-9e1f-11350e303f44", "Error"));
					severityCodes.AddPair(nameof(LogType.Warning), ResString.GetMultilingualString("fbc013c1-65b6-4472-b584-4c46717a5571", "Warning"));
					severityCodes.AddPair(nameof(LogType.Information), ResString.GetMultilingualString("083436fd-c521-4c1c-82b7-560f01a90fa5", "Information"));
					severityCodes.AddPair(nameof(LogType.Debug), ResString.GetMultilingualString("4c7ccbb6-d791-4596-b837-0ec81b826bc5", "Debug"));
				}
				return severityCodes;
			}
		}

		CodeDescriptionPairList severityCodes;

		public ZString HostName
		{
			get => hostName;
			set => hostName = HostNameList.ContainsCode(value) ? value : "All";
		}
		ZString hostName;

		public CodeDescriptionPairList HostNameList
		{
			get
			{
				if (hostNameCodes == null)
				{
					hostNameCodes = new CodeDescriptionPairList();
					hostNameCodes.AddPair("All", Res.GetString("67415afa-205a-40e6-b0d0-ce48ff1522d5", "All"));
				}
				return hostNameCodes;
			}
		}

		CodeDescriptionPairList hostNameCodes;

		public ZString ProcessId
		{
			get => processId;
			set => processId = ProcessIdList.ContainsCode(value) ? value : "All";
		}
		ZString processId;

		public CodeDescriptionPairList ProcessIdList
		{
			get
			{
				if (processIdCodes == null)
				{
					processIdCodes = new CodeDescriptionPairList();
					processIdCodes.AddPair("All", Res.GetString("67415afa-205a-40e6-b0d0-ce48ff1522d5", "All"));
				}
				return processIdCodes;
			}
		}

		CodeDescriptionPairList processIdCodes;

		public ZDateTime FromDateTimeUtc
		{
			get => fromDateTimeUtc;
			set
			{
				if (value.IsValid)
				{
					fromDateTimeUtc = GetUtcDateTime(value);
					FromDateTimeLocalInfo.RefreshBinding();

					if (value > ToDateTimeUtc)
					{
						toDateTimeUtc = FromDateTimeUtc;
						ToDateTimeUtcInfo.RefreshBinding();
					}
				}
				else
				{
					fromDateTimeUtc = ZDateTime.Empty;
					FromDateTimeLocalInfo.RefreshBinding();
				}
			}
		}
		ZDateTime fromDateTimeUtc;

		public ZDateTime ToDateTimeUtc
		{
			get => toDateTimeUtc;
			set
			{
				if (value.IsValid)
				{
					toDateTimeUtc = GetUtcDateTime(value);
					ToDateTimeLocalInfo.RefreshBinding();

					if (value < FromDateTimeUtc)
					{
						fromDateTimeUtc = ToDateTimeUtc;
						FromDateTimeUtcInfo.RefreshBinding();
					}
				}
				else
				{
					toDateTimeUtc = ZDateTime.Empty;
					ToDateTimeLocalInfo.RefreshBinding();
				}
			}
		}
		ZDateTime toDateTimeUtc;

		public ZDateTime FromDateTimeLocal
		{
			get => FromDateTimeUtc.IsValid ? FromDateTimeUtc.UtcToDateTimeOffset().ToZDateTime() : ZDateTime.Empty;
			set
			{
				if (value.IsValid)
				{
					fromDateTimeUtc = Env.Time.GetUtcFromLocalTime(value.ToDateTime());
					FromDateTimeUtcInfo.RefreshBinding();

					if (FromDateTimeUtc > ToDateTimeUtc)
					{
						ToDateTimeUtc = FromDateTimeUtc;
						ToDateTimeUtcInfo.RefreshBinding();
					}
				}
				else
				{
					fromDateTimeUtc = ZDateTime.Empty;
					FromDateTimeLocalInfo.RefreshBinding();
				}
			}
		}

		public ZDateTime ToDateTimeLocal
		{
			get => ToDateTimeUtc.IsValid ? ToDateTimeUtc.UtcToDateTimeOffset().ToZDateTime() : ZDateTime.Empty;
			set
			{
				if (value.IsValid)
				{
					toDateTimeUtc = Env.Time.GetUtcFromLocalTime(value.ToDateTime());
					ToDateTimeUtcInfo.RefreshBinding();

					if (ToDateTimeUtc < FromDateTimeUtc)
					{
						FromDateTimeUtc = ToDateTimeUtc;
						FromDateTimeUtcInfo.RefreshBinding();
					}
				}
				else
				{
					toDateTimeUtc = ZDateTime.Empty;
					ToDateTimeLocalInfo.RefreshBinding();
				}
			}
		}

		ZDateTime GetUtcDateTime(ZDateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Local)
			{
				return new ZDateTime(Env.Time.GetUtcFromLocalTime(dateTime.ToDateTime()));
			}
			return dateTime;
		}

		ServiceTaskLogFilters Filters
		{
			get
			{
				return new ServiceTaskLogFilters
				{
					ServiceTaskCode = ServiceTaskCode,
					FromDateTimeUtc = FromDateTimeUtc.ToNullableDateTime(),
					ToDateTimeUtc = ToDateTimeUtc.ToNullableDateTime(),
					Severity = Enum.TryParse<LogType>(Severity, out var result) ? result : null,
					HostName = HostName.Equals("All") ? string.Empty : HostName,
					ProcessId = ProcessId.Equals("All") ? string.Empty : ProcessId
				};
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		SearchBasedLogViewerValidation Validation => new SearchBasedLogViewerValidation(this);

		public EventRecordCollection EventList
		{
			get
			{
				if (eventList == null)
				{
					eventList = new EventRecordCollection();
					RegisterEditableChildObject(eventList);
				}

				if (reload && LogViewerDataProvider != null)
				{
					UnRegisterEditableChildObject(eventList);
					var sorting = eventList.SortInformation;

					try
					{
						var buffer = LogViewerDataProvider.GetBytes(Filters);
						if (buffer != null)
						{
							using (var stream = new MemoryStream(buffer))
							{
								eventList.Load(stream);
							}
						}

						if (sorting != null)
						{
							eventList.Sort(sorting);
						}
					}
					catch (WebException e)
					{
						eventList.RemoveAndDeleteAll();
						eventList.Add(new EventRecord(ZDateTime.UtcNow, nameof(LogType.Error), "LogViewer Exception: " + e.Message, 0, null, 0));
					}

					if (hostNameCodes == null || hostName.ToString().Equals("All", StringComparison.InvariantCultureIgnoreCase))
					{
						hostNameCodes = new CodeDescriptionPairList();
						hostNameCodes.AddPair("All", Res.GetString("67415afa-205a-40e6-b0d0-ce48ff1522d5", "All"));
						var hostNames = eventList
							.Select(x => x.HostName)
							.Distinct()
							.ToList();
						hostNames.Sort();
						foreach (var hostName in hostNames)
						{
							hostNameCodes.AddPair(hostName, hostName);
						}
					}

					if (processIdCodes == null || processId.ToString().Equals("All", StringComparison.InvariantCultureIgnoreCase))
					{
						processIdCodes = new CodeDescriptionPairList();
						processIdCodes.AddPair("All", Res.GetString("67415afa-205a-40e6-b0d0-ce48ff1522d5", "All"));
						var ids = eventList
							.Select(x => x.ProcessId)
							.Distinct()
							.ToList();
						ids.Sort();
						foreach (var id in ids)
						{
							processIdCodes.AddPair(id.ToString(), id.ToString());
						}
					}

					reload = false;
				}

				return eventList;
			}
		}

		public void ReloadEvents()
		{
			if ((ElasticSearchReadLoggingEnabled && ElasticsearchHasFilledCredentials) || (KafkaReadLoggingEnabled && KafkaHasFilledCredentials))
			{
				reload = true;
				if (eventList != null)
				{
					EventList.RefreshBinding();
				}
			}
		}

		public static bool InvalidElasticSearchConfiguration => ElasticSearchReadLoggingEnabled && !ElasticsearchHasFilledCredentials;
		public static bool InvalidKafkaConfiguration => KafkaReadLoggingEnabled && !KafkaHasFilledCredentials;
		static bool ElasticSearchReadLoggingEnabled => SystemDataRegistry.Instance.LoggingMethods.Value.FindElementByCode(LoggingMethods.ELK).Bool;
		static bool KafkaReadLoggingEnabled => SystemDataRegistry.Instance.LoggingMethods.Value.FindElementByCode(LoggingMethods.KAF).Bool;
		static bool ElasticsearchHasFilledCredentials =>
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.ElasticsearchServerUserName.Value) &&
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.ElasticsearchServerPassword.Value) &&
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.ElasticsearchServiceUri.Value) &&
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.ElasticsearchIndex.Value);
		static bool KafkaHasFilledCredentials =>
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.KafkaElasticsearchServerUserName.Value) &&
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.KafkaElasticsearchServerPassword.Value) &&
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.KafkaElasticsearchServiceUri.Value) &&
			!string.IsNullOrWhiteSpace(SystemDataRegistry.Instance.KafkaElasticsearchIndex.Value);

		readonly ISearchBasedLogViewerDataProviderFactory searchBasedLogViewerDataProviderFactory;
		ISearchBasedLogViewerDataProvider LogViewerDataProvider => logViewerDataProvider ??= searchBasedLogViewerDataProviderFactory.GetProvider();
		ISearchBasedLogViewerDataProvider logViewerDataProvider;
		EventRecordCollection eventList;
		bool reload;
		ZPropertyInfo FromDateTimeUtcInfo => GetZPropertyInfo(nameof(FromDateTimeUtc));
		ZPropertyInfo ToDateTimeUtcInfo => GetZPropertyInfo(nameof(ToDateTimeUtc));
		ZPropertyInfo FromDateTimeLocalInfo => GetZPropertyInfo(nameof(FromDateTimeLocal));
		ZPropertyInfo ToDateTimeLocalInfo => GetZPropertyInfo(nameof(ToDateTimeLocal));
	}
}
