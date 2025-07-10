using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Model;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Model = CargoWise.PAVE.Common.Model;
using TaskDTO = CargoWise.PAVE.Common.DTO.TaskDTO;

namespace Enterprise.BufferManagement.Service
{
	internal static class TaskHelper
	{
		#region Model To DTO

		internal static IEnumerable<TaskDTO> ToTaskDTOs(this IEnumerable<Task> tasks,
			Dictionary<Guid, IEnumerable<Tag>> tagsByTaskPK,
			IEnumerable<Model.Workflow> workflows)
		{
			var orderedTasks = tasks
				.OrderBy(task => new ModelToTaskOrderable(task, workflows.SingleOrDefault(w => w.PK == task.WorkflowPK)), new VisualBoardTaskComparer()).ToArray();

			return orderedTasks.Select((task, order) => ToTaskDTO(task, tagsByTaskPK, order));
		}

		static TaskDTO ToTaskDTO(Task task, Dictionary<Guid, IEnumerable<Tag>> tagsByTaskPK, int displayOrder)
		{
			return new TaskDTO
			{
				PK = task.PK,
				WorkflowPK = task.WorkflowPK,
				CapabilityPK = task.CapabilityPK,
				TagPKs = tagsByTaskPK.GetOrderedTagPKs(task.PK),
				DisplayOrder = displayOrder,
				#region SuppressResourceStringsCheckRegion 
				Properties = new Dictionary<string, object>
				{
					//TODO: Remove cast to decimal and empty if null next WIs needs changes in client, 
					{ "note", task.Note ?? string.Empty },
					{ "type", task.Type },
					{ "status", task.Status },
					{ "resourceCode", task.StaffCode ?? string.Empty },
					{ "description", task.Description ?? string.Empty },
					{ "isStartable", task.IsStartable },
					{ "estimateVariationFactor", task.EstimateVariationFactor },
					{ "lowEstimatedMinutes", (decimal)task.LowEstimatedMinutes },
					{ "highEstimatedMinutes", (decimal)task.HighEstimatedMinutes },
					{ "standardEstimatedMinutes", (decimal)task.StandardEstimatedMinutes },
					{ "estimatedTimeToCompleteMinutes", (decimal)task.EstimatedMinutesToComplete },
				}
				#endregion
			};
		}

		#endregion

		#region BusinessObject to DTO

		internal static IEnumerable<TaskDTO> ToTasksDTO(
			this IEnumerable<ProcessTask> tasks,
			Dictionary<Guid, IEnumerable<Tag>> tagsPerTaskPks,
			PropertyCache cache,
			Dictionary<string, BMControlCustomisation> layouts)
		{
			var orderedTasks = tasks
				.OrderBy(task => new NaiveTaskOrderable(task, false, cache), new VisualBoardTaskPropertiesComparer());

			return orderedTasks.Select((task, order) => new TaskDTO()
			{
				PK = task.PK.ToGuid(),
				WorkflowPK = task.P9_FH_ProcessHeader.ToGuid(),
				CapabilityPK = task.P9_G4_RequiredCapability.IsValid ? task.P9_G4_RequiredCapability.ToGuid() : null,
				TagPKs = tagsPerTaskPks.GetOrderedTagPKs(task.PK.ToGuid()),
				DisplayOrder = order,
				Properties = task.LoadProperties(cache, CustomisedLayoutHelper.GetLayoutForRelatedJobType(layouts, task.GetProcessHeader()))
			}).ToArray();
		}

		static Dictionary<string, object> LoadProperties(this ProcessTask task, PropertyCache cache, BMControlCustomisation layout)
		{
			if (layout != null)
			{
				var properties = PropertyLoaderHelper.LoadPropertiesForLayout(task, layout, PropertySourceList.Codes.ProcessTask, cache);
				PropertyLoaderHelper.AddCompulsoryTaskProperties(task, properties);
				return properties;
			}
			#region SuppressResourceStringsCheckRegion 
			return new Dictionary<string, object>
				{
					{ "note", task.P9_CardNote },
					{ "type", task.P9_Type },
					{ "status", task.P9_Status  },
					{ "resourceCode", task.AssignedStaffMember?.GS_Code },
					{ "description", task.LongDescription },
					{ "isStartable", task.IsStartable(cache) },
					{ "estimateVariationFactor", task.P9_EstimateVariationFactor },
					{ "lowEstimatedMinutes", Utilities.Round(task.LowEstimatedDurationHours * 60, 0) },
					{ "standardEstimatedMinutes", Utilities.Round(task.StandardEstimateHours * 60, 0) },
					{ "highEstimatedMinutes", Utilities.Round(task.HighEstimatedDurationHours * 60, 0) },
					{ "estimatedTimeToCompleteMinutes", Utilities.Round(task.EstimatedTimeToCompleteHours * 60, 0) },
				};
			#endregion
		}

		#endregion
	}
}
