using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceTaskCollection : BusinessObjectCollection<StmServiceTask>
	{
		readonly IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider;

		public StmServiceTaskCollection(BusinessObjectFactory factory) : this(factory, ObjectFactory.Get<IServiceTaskScheduleStatusProvider>())
		{
		}

		internal StmServiceTaskCollection(BusinessObjectFactory factory, IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider) : base(factory)
		{
			this.serviceTaskScheduleStatusProvider = serviceTaskScheduleStatusProvider;
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			var tasks = Load(Factory, alternativeAdditionalFilter, serviceTaskScheduleStatusProvider);
			SetLoadResult(alternativeAdditionalFilter, tasks);
		}

		public static StmServiceTask[] Load(BusinessObjectFactory factory, ZQuery alternativeAdditionalFilter, IServiceTaskScheduleStatusProvider serviceProvider)
		{
			return LoadWithStatus(factory, alternativeAdditionalFilter, serviceProvider.GetServiceStatus());
		}

		static StmServiceTask[] LoadWithStatus(BusinessObjectFactory factory, ZQuery alternativeAdditionalFilter, IDictionary<string, TaskInstanceStatus> instanceStatusCollection)
		{
			var attributes = ObjectFactory.Get<IClientHostedServiceAttributeProvider>().GetClientHostedServiceAttributes();

			var compositeFilters = new StmServiceTaskFilterHelper(alternativeAdditionalFilter, attributes, instanceStatusCollection, factory);

			var filteredCodes = compositeFilters.GetFilteredServiceCodes();
			compositeFilters.TableSchemaFilters.AddToFilter(new ZQuery(StmServiceTaskSchema.SST_ServiceTaskCode, filteredCodes));

			var result = factory.Load<StmServiceTask>(compositeFilters.TableSchemaFilters);
			PopulateStatusData(instanceStatusCollection);

			void PopulateStatusData(IDictionary<string, TaskInstanceStatus> taskStatus)
			{
				if (!taskStatus.Any())
				{
					return;
				}

				foreach (StmServiceTask taskSchedule in result)
				{
					taskStatus.TryGetValue(taskSchedule.SST_ServiceTaskCode, out var thisTaskStatus);
					taskSchedule.OnStatusUpdateComplete(thisTaskStatus);
				}
			}
			return result;
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return property.Name switch
			{
				$"{CustomTextFilterNameConstants.PlaceInQueue}String" => new NumericalStringComparer<StmServiceTask>(s => s.PlaceInQueueString, direction),
				$"{CustomTextFilterNameConstants.ProcessId}sString" => new NumericalStringComparer<StmServiceTask>(s => s.ProcessIDsString, direction),
				$"{CustomTextFilterNameConstants.SecondsInQueue}String" => new NumericalStringComparer<StmServiceTask>(s => s.SecondsInQueueString, direction),
				$"{CustomTextFilterNameConstants.SecondsRunning}String" => new NumericalStringComparer<StmServiceTask>(s => s.SecondsRunningString, direction),
				_ => base.GetComparerForSort(property, direction)
			};
		}
	}
}
