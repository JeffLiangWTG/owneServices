using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskScheduleCollection : BusinessObjectCollection<ServiceTaskSchedule>, IServiceTaskScheduleCollection
	{
		public enum RemoteStatus
		{
			WithoutStatus,
			WithStatus,
		}

		readonly IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider;
		readonly RemoteStatus remoteStatus;

		public ServiceTaskScheduleCollection(BusinessObjectFactory factory, RemoteStatus remoteStatus) : this(factory, remoteStatus, ObjectFactory.Get<IServiceTaskScheduleStatusProvider>())
		{
		}

		internal ServiceTaskScheduleCollection(BusinessObjectFactory factory, RemoteStatus remoteStatus, IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider) : base(factory)
		{
			this.remoteStatus = remoteStatus;
			this.serviceTaskScheduleStatusProvider = serviceTaskScheduleStatusProvider;
		}

		public BusinessObjectCollection<ServiceTaskSchedule> Tasks => this;

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(GetParentTableCodeFilter());

			if (SharedRegistry.Instance.ProductivityWiseModeEnabled)
			{
				var categories = ServiceTaskCategoryDescriptors.Get(x => x.IsShownInProductivityWiseMode);
				result.AddToFilter(StmScheduleTaskSchema.S5_TypeOfDocument, categories.Select(x => x.Code));
			}

			return result;
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			var tasks = Load(Factory, alternativeAdditionalFilter, serviceTaskScheduleStatusProvider, remoteStatus);
			SetLoadResult(alternativeAdditionalFilter, tasks);
		}

		public static ServiceTaskSchedule[] Load(BusinessObjectFactory factory, ZQuery alternativeAdditionalFilter, IServiceTaskScheduleStatusProvider serviceProvider, RemoteStatus remoteStatus)
		{
			IDictionary<string, TaskInstanceStatus> instanceStatusCollection = new Dictionary<string, TaskInstanceStatus>();
			if (remoteStatus == RemoteStatus.WithStatus)
			{
				instanceStatusCollection = serviceProvider.GetServiceStatus();
				return LoadWithStatus(factory, alternativeAdditionalFilter, instanceStatusCollection);
			}
			if (remoteStatus == RemoteStatus.WithoutStatus)
			{
				return LoadWithoutStatus(factory, alternativeAdditionalFilter, instanceStatusCollection);
			}
			else
			{
				throw new InvalidOperationException($"Unknown status {remoteStatus}");
			}
		}

		public static ServiceTaskSchedule[] LoadWithoutStatus(BusinessObjectFactory factory, ZQuery alternativeAdditionalFilter, IDictionary<string, TaskInstanceStatus> instanceStatusCollection)
		{
			var compositeFilters = new ServiceTaskScheduleFilterHelper(alternativeAdditionalFilter, instanceStatusCollection, factory);
			return factory.Load<ServiceTaskSchedule>(compositeFilters.TableSchemaFilters);
		}

		public static ServiceTaskSchedule[] LoadWithStatus(BusinessObjectFactory factory, ZQuery alternativeAdditionalFilter, IDictionary<string, TaskInstanceStatus> instanceStatusCollection)
		{
			var compositeFilters = new ServiceTaskScheduleFilterHelper(alternativeAdditionalFilter, instanceStatusCollection, factory);
			if (!string.IsNullOrEmpty(compositeFilters.ServiceStatusFilters.LiteralTextADO))
			{
				var filteredCodes = compositeFilters.GetFilteredServiceCodes();
				compositeFilters.TableSchemaFilters.AddToFilter(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, filteredCodes));
			}

			var result = factory.Load<ServiceTaskSchedule>(compositeFilters.TableSchemaFilters);
			PopulateStatusData(instanceStatusCollection);

			void PopulateStatusData(IDictionary<string, TaskInstanceStatus> taskStatus)
			{
				if (!taskStatus.Any())
				{
					return;
				}

				foreach (ServiceTaskSchedule taskSchedule in result)
				{
					taskStatus.TryGetValue(taskSchedule.S5_ScheduleType, out var thisTaskStatus);
					taskSchedule.OnStatusUpdateComplete(thisTaskStatus);
				}
			}
			return result;
		}

		internal static ZQuery GetParentTableCodeFilter()
		{
			return new ZQuery(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix);
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return property.Name switch
			{
				$"{CustomTextFilterNameConstants.PlaceInQueue}String" => new NumericalStringComparer<ServiceTaskSchedule>(s => s.PlaceInQueueString, direction),
				$"{CustomTextFilterNameConstants.ProcessId}sString" => new NumericalStringComparer<ServiceTaskSchedule>(s => s.ProcessIDsString, direction),
				$"{CustomTextFilterNameConstants.SecondsInQueue}String" => new NumericalStringComparer<ServiceTaskSchedule>(s => s.SecondsInQueueString, direction),
				$"{CustomTextFilterNameConstants.SecondsRunning}String" => new NumericalStringComparer<ServiceTaskSchedule>(s => s.SecondsRunningString, direction),
				_ => base.GetComparerForSort(property, direction)
			};
		}
	}
}
