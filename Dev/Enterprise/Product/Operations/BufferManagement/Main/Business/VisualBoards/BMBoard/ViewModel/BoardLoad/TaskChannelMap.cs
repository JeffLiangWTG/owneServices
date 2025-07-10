using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TaskChannelMap : IEnumerable<ProcessTask>
	{
		#region Construction

		public static TaskChannelMap Create(BMBoardSection section, IEnumerable<IVisualBoardChannel> channels, BoardSectionEntities entities, Func<ProcessTask, bool> includeTaskFunc = null)
		{
			return new TaskChannelMap(section, channels, entities, includeTaskFunc ?? (t => t.IsOpen));
		}

		public static TaskChannelMap Empty => Create(null, Array.Empty<IVisualBoardChannel>(), BoardSectionEntities.Empty);

#if DEBUG
		public static TaskChannelMap ForTest(BMBoardSection section, BMBoardSectionViewModel viewModel, params ProcessHeader[] workflows)
		{
			var channels = viewModel.AllChannels.Any() ? viewModel.AllChannels : new[] { new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled) };
			var entities = BoardSectionEntities.ForTest(workflows);

			return Create(section, channels, entities);
		}

		protected
#endif
		TaskChannelMap(BMBoardSection section, IEnumerable<IVisualBoardChannel> channels, BoardSectionEntities entities, Func<ProcessTask, bool> allowTaskFunc)
		{
			var tasksByChannel = channels.ToDictionary(c => c, c => new List<ProcessTask>());
			var distinctTasks = new HashSet<ProcessTask>();
			var channelsWithTasks = new HashSet<IVisualBoardChannel>();
			AllWorkflows = entities.WorkflowsShownOnSection;

			var tasks = entities.TasksShownOnSection;

			if (section != null && AllWorkflows != null)
			{
				if (BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.PlanningManagement)
				{
					section.Factory.ServiceContainer.AddService(new ApprovedShapeBufferPenetrationService(entities.WorkflowsShownOnSectionAndTheirAncestors));
				}

				var showJobLevelWorkflowTickets = section.SectionConfiguration.ShowJobWorkflowCards;

				if (tasks.Any())
				{
					var capabilitiesPKs = tasks.Select(c => c.P9_G4_RequiredCapability).Distinct().ToArray();
					var tasksFactory = tasks.First().Factory;

					tasksFactory.AddFetchHint(GlbCapabilitySchema.Instance, new ZQuery(GlbCapabilitySchema.PK, capabilitiesPKs));
					tasksFactory.AddFetchHint(GlbResourceCapabilityPivotSchema.Instance, new ZQuery(GlbResourceCapabilityPivotSchema.G5_G4_Capability, capabilitiesPKs)); // for determining tasks' constraint status
				}

				foreach (var task in tasks.Where(allowTaskFunc))
				{
					foreach (var channel in tasksByChannel.Keys.Where(x => x.IsInChannel(task, showJobLevelWorkflowTickets)))
					{
						channelsWithTasks.Add(channel);
						distinctTasks.Add(task);
						tasksByChannel[channel].Add(task);
					}
				}
			}

			TasksByChannel = new TasksByChannel(tasksByChannel);
			AllTasks = distinctTasks.ToArray();
			AllChannels = channelsWithTasks.ToArray();

			Factory = section?.Factory;
		}

		#endregion

		#region Properties

		public TasksByChannel TasksByChannel { get; }

		public ICollection<ProcessTask> AllTasks { get; }

		public ICollection<ProcessHeader> AllWorkflows { get; }

		public ICollection<IVisualBoardChannel> AllChannels { get; }

		public BusinessObjectFactory Factory { get; }

		public int Length => AllTasks.Count;

		#endregion

		#region IEnumerable Implementation

		public IEnumerator<ProcessTask> GetEnumerator()
		{
			return AllTasks.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
