using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
#if DEBUG
	public
#endif
 static class ProcessHeaderGraphProvider
	{
		public static ScheduleGraph CreateGraph(ProcessJobHeader jobHeader, IEnumerable<ProcessHeader> processHeaders, bool includeClosedTaskTimes)
		{
			var graph = new ScheduleGraph();
			var rootSchedule = graph.AddSchedule(jobHeader, 0m);
			rootSchedule.IsScheduleApplicableToCriticalChainDuration = false;

			AddProcessHeadersAsSchedules(rootSchedule, graph, processHeaders, includeClosedTaskTimes);

			return graph;
		}

		static Dictionary<ZGuid, ScheduleNode> AddProcessHeadersAsSchedules(ScheduleNode rootSchedule, ScheduleGraph graph, IEnumerable<ProcessHeader> processHeaders, bool includeClosedTaskTimes)
		{
			var bizos = processHeaders.Append((ProcessHeader)rootSchedule.Entity).ToDictionary(p => p.PK, p => p);

			foreach (var header in processHeaders)
			{
				var schedule = graph.AddSchedule(header, 0);
				schedule.Parent = rootSchedule;
				schedule.IsScheduleApplicableToCriticalChainDuration = true;
			}

			var scheduleScope = graph.SchedulesByEntity;

			foreach (var node in scheduleScope.Values)
			{
				node.AddPostrequisites(bizos[node.Entity.PK].PostrequisiteLinks
					.Where(l => bizos.ContainsKey(l.FP_FH_HeaderTo))
					.Select(l => scheduleScope[l.FP_FH_HeaderTo]).ToArray());

				node.AddPrerequisites(bizos[node.Entity.PK].PrerequisiteLinks
					.Where(l => bizos.ContainsKey(l.FP_FH_HeaderFrom))
					.Select(l => scheduleScope[l.FP_FH_HeaderFrom]).ToArray());
			}

			foreach (var header in processHeaders)
			{
				var node = scheduleScope[header.PK];
				node.EstimatedDurationHoursIncludingChildren = GetTaskEstimatesIncludingChildren(header, graph.SchedulesByEntity, includeClosedTaskTimes);
			}

			return scheduleScope;
		}

		static decimal GetTaskEstimatesIncludingChildren(ProcessHeader header, Dictionary<ZGuid, ScheduleNode> scheduleScope, bool includeClosedTaskTimes)
		{
			var result = GetTaskEstimates(header, includeClosedTaskTimes);

			foreach (var child in header.GetChildWorkflowsDownTheHierarchy())
			{
				if (scheduleScope.ContainsKey(child.PK))
				{
					result += GetTaskEstimates(child, includeClosedTaskTimes);
				}
			}

			return result;
		}

		static decimal GetTaskEstimates(ProcessHeader header, bool includeClosedTaskTimes)
		{
			return includeClosedTaskTimes ? header.TotalRelevantEstimatedHours : header.RemainingEstimateHours;
		}
	}
}
