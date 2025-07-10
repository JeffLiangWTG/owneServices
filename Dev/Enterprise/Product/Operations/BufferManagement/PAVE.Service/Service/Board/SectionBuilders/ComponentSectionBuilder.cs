using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Service
{
	internal class ComponentSectionBuilder : ISectionBuilder
	{
		public ComponentSectionBuilder(BMBoardSection section)
		{
			Section = section;
		}

		BMBoardSection Section { get; }
		BusinessObjectFactory Factory => Section.Factory;
		BMComponentSectionConfiguration SectionConfig => Section.SectionConfiguration;

		readonly PropertyCache Cache = new PropertyCache();

		public ISection Build()
		{
			TagProvider.GetAllTagDefinitions(Factory);
			var taskChannelMap = GetTaskChannelMap(GetChannels());
			var tagsPerTasks = taskChannelMap.AllTasks.ToTagsByTaskPK();
			var channels = taskChannelMap.AllChannels.ToChannelsDTO(channel => taskChannelMap.TasksByChannel[channel].Select(task => task.PK.ToGuid()));
			var layouts = BMSRegistry.Instance.PAVEOnTheWeb.Value ? Section.GetSummaryCardCustomisedLayouts() : new Dictionary<string, BMControlCustomisation>();
			var layoutsEnabledOnWeb = layouts.Where(pair => pair.Value.RenderOnTheWeb).ToDictionary(pair => pair.Key, pair => pair.Value);

			Factory.AddFetchHint(ProcessTaskIterationLinkSchema.Instance, new ZQuery(ProcessTaskIterationLinkSchema.P9I_FH_IterationWorkflow, taskChannelMap.AllWorkflows.Select(workflow => workflow.PK)));
			var workDetailsDTO = WorkDetailsDTOHelper.CreateWorkDetailsDTO(taskChannelMap.AllWorkflows, taskChannelMap.AllTasks, tagsPerTasks, () => GetIndexes(taskChannelMap.AllWorkflows), Cache, layouts: layoutsEnabledOnWeb);

			return new ComponentSectionDTO()
			{
				PK = Section.PK.ToGuid(),
				Jobs = workDetailsDTO.Jobs,
				Workflows = workDetailsDTO.Workflows,
				Capabilities = workDetailsDTO.Capabilities,
				Tags = workDetailsDTO.Tags,
				Tasks = workDetailsDTO.Tasks,
				Channels = channels
			};
		}

		#region TaskChannelMap

		TaskChannelMap GetTaskChannelMap((IEnumerable<IVisualBoardChannel> primary, IEnumerable<IVisualBoardChannel> secondary) channels)
		{
			var allChannels = channels.primary.Union(channels.secondary);
			var boardDataSourceParameters = new BoardSectionDataSourceParameters(
				Cache,
				Section.WorkflowSectionFilter,
				Section.TaskSectionFilter,
				SectionConfig.Channels,
				allChannels
			);

			if (Factory.ServiceContainer.GetService<ApprovedShapeBufferPenetrationService>() != null)
			{
				Factory.ServiceContainer.RemoveService<ApprovedShapeBufferPenetrationService>();
			}

			var dataSource = new BoardSectionDataSource(Section, boardDataSourceParameters);
			var taskChannelMap = channels.primary.Any() ? dataSource.GetIncompleteTasksForCurrentChannels() : dataSource.GetIncompleteTasksForUnchannelled();

			TaskJobWorkflowCacheHelper.PopulateCacheForTasks(taskChannelMap, Section, Cache);

			var approvedShapeBufferPenetrationService = Factory.ServiceContainer.GetService<ApprovedShapeBufferPenetrationService>();

			if (approvedShapeBufferPenetrationService != null)
			{
				approvedShapeBufferPenetrationService.WaitAllApprovedShapeDetails();
			}

			return taskChannelMap;
		}

		#endregion

		#region Indexes

		IDictionary<ZGuid, decimal> GetIndexes(IEnumerable<ProcessHeader> workflows)
		{
			var workingTimeContext = WorkingTimeContext.Create(Section);

			if (SectionConfig.IsBuffer)
			{
				return workflows
					.ToDictionary(workflow => workflow.PK,
						workflow => Utilities.Round(CardAllocationMap.GetBufferPenetration(Cache, workingTimeContext, workflow), 2));
			}
			else
			{
				var cachedWorkflowTimes = new Dictionary<ZGuid, TimeSpan>();
				return workflows
					.ToDictionary(workflow => workflow.PK,
							workflow => Convert.ToDecimal(CardAllocationMap.GetTimeIndex(SectionConfig, workflow, workingTimeContext, SectionConfig.TimeSpanPerCell, cachedWorkflowTimes)));
			}
		}

		#endregion

		#region Channels

		(IEnumerable<IVisualBoardChannel> primary, IEnumerable<IVisualBoardChannel> secondary) GetChannels()
		{
			var channelFactoryParameters = new ChannelFactoryParameters(
				Cache,
				false,
				SectionConfig.IsBuffer,
				Section.IsInConstrainedMode,
				SectionConfig.HideCapabilityTasksFromResourceChannels,
				SectionConfig.HideResourceTasksFromCapabilityChannels,
				Section.PK,
				new SharedBoardFactoryProvider(),
				Section?.CapabilityChannelEntityPKs);

			return (ChannelFactory.CreatePrimaryChannels(channelFactoryParameters, Section), ChannelFactory.CreateSecondaryChannels(channelFactoryParameters, Section));
		}

		#endregion
	}
}
