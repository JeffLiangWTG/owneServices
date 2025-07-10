using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public static class WorkflowTransferOrderSorter
	{
		public static IEnumerable<T> Sort<T>(IEnumerable<T> workflows)
			where T : IWorkflowOrderable
		{
			return SortCore(workflows);
		}

		public static Queue<ReleaseGateRequest> Sort(IEnumerable<ReleaseGateRequest> releaseGateRequests)
		{
			return new Queue<ReleaseGateRequest>(SortCore(releaseGateRequests));
		}

		static IEnumerable<T> SortCore<T>(IEnumerable<T> workflows)
			where T : IWorkflowOrderable
		{
			var result = new LinkedList<T>();

			if (workflows == null || !workflows.Any())
			{
				return result;
			}

			var extraOrderByForADD = new Func<IWorkflowOrderable, ZDateTime>(w => ZDateTime.Empty);
			if (BMSRegistry.Instance.UseDateOrderingForReleaseGate.Value)
			{
				extraOrderByForADD = new Func<IWorkflowOrderable, ZDateTime>(w => !w.AgreedDeliveryDate.IsEmpty ? w.AgreedDeliveryDate : ZDateTime.MaxSmallDateTimeValue);
			}

			return from w in workflows
				   orderby extraOrderByForADD(w) ascending,
				   w.EffectiveNudge descending,
				   w.ReleaseSequenceSortDate,
				   w.CreateTime
				   select w;
		}

		public static void SortAndSetReleaseSequence(BusinessObjectFactory factory, IEnumerable<ProcessHeader> processHeaders)
		{
			NudgeCalculator.PopulateNudgeCache(factory, processHeaders);

			foreach (var processHeader in processHeaders)
			{
				processHeader.AddDeepFetchHintForParentType();
			}

			foreach (var workflowsByBMSystem in processHeaders.Where(wf => wf.BMSystem != null && wf.IsWorkflow).GroupBy(w => w.BMSystem))
			{
				int sequence = 0;
				var bmSystem = workflowsByBMSystem.First().BMSystem;

				foreach (var workflow in Sort(workflowsByBMSystem))
				{
					workflow.ReleaseSequence = string.Format(CultureInfo.InvariantCulture, "{0}:{1:D5}", bmSystem.FS_Name, ++sequence); // Format code is not language specific
				}
			}
		}
	}
}
