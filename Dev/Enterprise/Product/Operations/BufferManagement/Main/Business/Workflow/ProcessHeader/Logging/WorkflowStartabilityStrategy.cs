using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	static class WorkflowStartabilityStrategy
	{
		internal static string GetStartabilityLogReferenceString(bool isStartable)
		{
			return string.Format(CultureInfo.InvariantCulture, "|SRT={0}", isStartable ? "Y" : "N"); // Log Reference Values should be in English only
		}

		internal static void AddStartabilityEvent(ProcessHeader processHeader)
		{
			if (NeedStartabilityEvent(processHeader))
			{
				var allTasks = processHeader.GetTasksWithoutAccessingWorkflowParent();

				var isWorkflowStartable = ((IProposedNetworkEntity)processHeader).IsStartable;

				var startableTasks = isWorkflowStartable
					? allTasks.Where(task => task.IsStartable())
					: Enumerable.Empty<ProcessTask>();

				AddStartabilityAndNonStartabilityEvents(allTasks, startableTasks);
			}
		}

		internal static void AddFetchHints(ProcessHeader processHeader, BusinessObjectFactory factory)
		{
			if (NeedStartabilityEvent(processHeader))
			{
				var tasks = processHeader.GetTasksWithoutAccessingWorkflowParent();

				foreach (var task in tasks)
				{
					var query = task.Logs.MostRecentLogByEventTimeQuery(Events.StartabilityChanged, null);
					factory.AddFetchHint(typeof(StmALog), query);
				}
			}
		}

		#region Implementation

		static bool NeedStartabilityEvent(ProcessHeader processHeader)
		{
			return !processHeader.IsTemplate
				&& processHeader.IsWorkflow
				&& (!processHeader.IsInDatabase
					|| processHeader.FH_SystemLastEditTimeUtcInfo.HasChanges // tasks are changing in same workflow
					|| processHeader.FH_StatusInfo.HasChanges); // related workflow/s is/are updated and cause workflow status to change
		}

		static void AddStartabilityAndNonStartabilityEvents(IEnumerable<ProcessTask> allTasks, IEnumerable<ProcessTask> startableTasks)
		{
			startableTasks.ForEach(task => AddStartabilityEventCore(task, isStartable: true));

			var nonStartableTasks = allTasks
				.Except(startableTasks)
				.Where(task => task.IsOpen);

			foreach (var nonStartableTask in nonStartableTasks)
			{
				var lastStartabiliytLog = GetLastStartabilityLog(nonStartableTask);

				if (lastStartabiliytLog?.SL_Reference.Contains("|SRT=Y", StringComparison.OrdinalIgnoreCase) == true) // Log Reference Values should be in English only
				{
					AddStartabilityEventCore(nonStartableTask, isStartable: false, cachedLastStartabilityLog: lastStartabiliytLog);
				}
			}
		}

		static void AddStartabilityEventCore(ProcessTask processTask, bool isStartable, StmALog cachedLastStartabilityLog = null)
		{
			var lastStartabilityLog = cachedLastStartabilityLog ?? GetLastStartabilityLog(processTask);
			var startabilityLogReference = GetStartabilityLogReferenceString(isStartable);

			var needsLog = lastStartabilityLog == null
				|| !lastStartabilityLog.SL_Reference.Contains(startabilityLogReference, StringComparison.Ordinal)
				|| (isStartable && ProcessTask.IsClosedCodes.Contains((ZString)processTask.P9_StatusInfo.OriginalValue)); // on re-opening we should re-raise startability event

			if (needsLog)
			{
				processTask.Logs.CreateOrRecreateEventLog(
					Events.StartabilityChanged,
					EstimateActual.Actual,
					ZDateTimeOffset.Now,
					BMTaskStartabilityChangedEventParametersStrategy.GetLogReference(processTask, isStartable));
			}
		}

		static StmALog GetLastStartabilityLog(ProcessTask processTask)
		{
			var mostRecentLogByEventTimeQuery = processTask.Logs.MostRecentLogByEventTimeQuery(Events.StartabilityChanged, null);
			return new ReadOnlyBusinessObjectFactory().Load<StmALog>(mostRecentLogByEventTimeQuery).FirstOrDefault();
		}

		#endregion
	}
}
