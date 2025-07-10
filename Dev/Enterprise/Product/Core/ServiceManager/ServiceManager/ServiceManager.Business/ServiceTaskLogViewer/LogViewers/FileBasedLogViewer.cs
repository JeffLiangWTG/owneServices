using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ServiceManager.Business
{
	public class FileBasedLogViewer : NonPersistentBusinessObject, IServiceTaskLogViewer
	{
		[CodeAlive("Used in NonPersistentBusinessObject construction via Activator.CreateInstance")]
		public FileBasedLogViewer() : this(new LogViewerDataProviderFactory())
		{
		}

		public FileBasedLogViewer(ILogViewerDataProviderFactory dataProviderFactory) : base(new BusinessObjectFactory())
		{
			this.dataProviderFactory = dataProviderFactory;
		}

		public override bool HasChanges => false;

		#region Properties

		public IEnumerable<ILogViewerDataProvider> HostLogProviderCollection => dataProviderFactory.GetProviders(TaskType);

		string IServiceTaskLogViewer.TaskType
		{
			get => TaskType;
			set => TaskType = value;
		}

		[CargoWise.ComponentModel.MaxLength(4)]
		[BusinessObjectTestExclude]
		public ZString TaskType
		{
			get => taskType;
			set
			{
				var hasChanges = taskType != value;

				CheckMaximumLength(TaskTypeInfo, value);
				SetNonPersistentPropertyValue(TaskTypeInfo, ref taskType, value);

				if (hasChanges)
				{
					ReloadLogFileList();
				}
			}
		}

		ZString taskType;

		public ZPropertyInfo TaskTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TaskType)); }
		}

		#endregion

		#region Lists

		public CodeDescriptionPairList TaskTypes
		{
			get
			{
				if (taskTypes == null)
				{
					taskTypes = new CodeDescriptionPairList();
					taskTypes.AddPair(ServiceManagerHelper.HostLoggerCode, ResString.GetMultilingualString("AF8F626B-D8AA-41DD-8D87-46F638B8FD42", "Service Host"));

					if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
					{
						var taskCollection = new StmServiceTaskCollection(Factory);
						taskCollection.Load();

						foreach (var task in taskCollection.Cast<StmServiceTask>().OrderBy(x => x.SST_ServiceTaskCode))
						{
							taskTypes.AddPairIfNotExist(task.SST_ServiceTaskCode, task.DescriptionMultilingual);
						}
					}
					else
					{
						var taskScheduleCollection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithoutStatus);
						taskScheduleCollection.Load();
						foreach (var taskSchedule in taskScheduleCollection.Cast<ServiceTaskSchedule>().OrderBy(x => x[StmScheduleTaskSchema.S5_ScheduleType]))
						{
							taskTypes.AddPairIfNotExist(taskSchedule.S5_ScheduleType, taskSchedule.S5_ScheduleDescriptionMultilingual);
						}
					}
				}

				return taskTypes;
			}
		}

		CodeDescriptionPairList taskTypes;

		public ZBool TaskTypeReadOnly { get; set; } = false;

		#endregion

		#region Log File List

		public LogFileRecordCollection LogFileList
		{
			get
			{
				if (logFilesCollection == null)
				{
					logFilesCollection = new LogFileRecordCollection();
				}

				if (reload)
				{
					logFilesCollection.Load(HostLogProviderCollection);
					reload = false;
				}

				return logFilesCollection;
			}
		}

		LogFileRecordCollection logFilesCollection;

		public void ReloadLogFileList()
		{
			reload = true;
			if (logFilesCollection != null)
			{
				LogFileList.RefreshBinding();
			}
		}

		bool reload = true;

		#endregion

		readonly ILogViewerDataProviderFactory dataProviderFactory;
	}
}

