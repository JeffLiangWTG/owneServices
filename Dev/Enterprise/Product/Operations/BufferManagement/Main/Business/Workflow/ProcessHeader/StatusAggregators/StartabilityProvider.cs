using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	static class StartabilityProvider
	{
		internal static IsStartableMap CalculateStartability(TaskChannelMap tasks, BMBoardSection section, Action<TaskStartabilityService> replaceService = null)
		{
			var orderedTasks = tasks.AllTasks;

			var otherTasksInSameWorkflows = GetPreviousCompletedTasksInSameWorkflowsWhichMayHaveIncompleteQualityIterations(orderedTasks).ToArray();
			var openIterations = GetOpenQualityIterations(section.Factory, otherTasksInSameWorkflows);

			var startableTasks = new ConcurrentHashSet<ZGuid>();
			var tasksBlockedByMissingReplenishment = new HashSet<ZGuid>();
			var nonStartableTasks = new ConcurrentHashSet<ZGuid>();

			foreach (var task in orderedTasks)
			{
				var isStartable = task.IsStartable_ExcludingReplenishment(openIterations);

				if (isStartable)
				{
					startableTasks.TryAdd(task.PK);
				}
				else
				{
					nonStartableTasks.TryAdd(task.PK);
				}
			}

			var map = new IsStartableMap(startableTasks, tasksBlockedByMissingReplenishment.ToImmutableHashSet(), nonStartableTasks);

			if (replaceService != null)
			{
				var service = new TaskStartabilityService(map);
				replaceService(service);

				if (section.Factory.ServiceContainer.GetService<TaskStartabilityService>() == null)
				{
					section.Factory.ServiceContainer.AddService(service);
				}
			}

			return map;
		}

		#region Quality Iteration Considerations

		static IEnumerable<ZGuid> GetPreviousCompletedTasksInSameWorkflowsWhichMayHaveIncompleteQualityIterations(ICollection<ProcessTask> tasks)
		{
			return
				from task in tasks
				let workflow = task.GetProcessHeader()
				where workflow != null
				let tasksInWorkflow = workflow.GetTasksWithoutAccessingWorkflowParent()
				from taskInWorkflow in tasksInWorkflow
				where taskInWorkflow.P9_Sequence < task.P9_Sequence
				where taskInWorkflow.IsClosed
				select taskInWorkflow.PK;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static ICollection<IProcessTaskIterationLink> GetOpenQualityIterations(BusinessObjectFactory factory, ICollection<ZGuid> relevantTaskPKs)
		{
			if (relevantTaskPKs.Any())
			{
				var query = new ZQuery { AllowTableValuedParameters = true };
				query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_P9_ContainmentBarrierTask, relevantTaskPKs);
				query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_LinkType, IterationLinkTypeList.Codes.QualityIterationTask);
				query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_Outcome, IterationLinkOutcomeList.Codes.IterationRequired);

				var iterationLinks = factory.Load<IProcessTaskIterationLink>(query).ToLookup(link => link.P9I_FH_IterationWorkflow);
				var workflowPKs = iterationLinks.Select(g => g.Key).Where(g => g.IsValid).ToArray();

				if (workflowPKs.Any())
				{
					var openWorkflows = new HashSet<ZGuid>();
					var sql = @"
						SELECT FH_PK
						FROM dbo.ProcessHeader
						WHERE FH_Status <> 'CLS'
						AND FH_PK IN (SELECT Value FROM @WorkflowPKs)
						";

					using (var command = Db.Connection.Command(sql)) // Using BusinessObjectFactory.Exists would require one hit per workflow, and loading business objects for existence checks is overly heavy.
					{
						command.AddTableValuedParameter("@WorkflowPKs", ProcessHeaderSchema.PK, workflowPKs);

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								openWorkflows.Add(reader.GetGuid(0));
							}
						}
					}

					var openIterations = new List<IProcessTaskIterationLink>();
					foreach (var workflowPK in openWorkflows)
					{
						openIterations.AddRange(iterationLinks[workflowPK]);
					}

					return openIterations;
				}
			}

			return Array.Empty<IProcessTaskIterationLink>();
		}

		#endregion
	}
}
