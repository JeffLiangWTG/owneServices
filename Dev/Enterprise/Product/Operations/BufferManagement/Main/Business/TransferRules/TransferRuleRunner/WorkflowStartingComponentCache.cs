using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowStartingComponentCache
	{
		public WorkflowStartingComponentCache(ITransferRuleRunnerDataAccessor dataAccessor)
		{
			this.dataAccessor = dataAccessor;
		}

		readonly ITransferRuleRunnerDataAccessor dataAccessor;

		public void UpdateWorkflows(IEnumerable<ITransferrableProcessHeader> workflows)
		{
			foreach (var workflow in workflows)
			{
				UpdateWorkflow(workflow);
			}
		}

		public void UpdateWorkflow(ITransferrableProcessHeader workflow)
		{
			var key = workflow.PK;

			if (!cache.ContainsKey(key))
			{
				cache.Add(key, new List<(DateTime lastChange, Guid componentPK, string component)>());
			}

			var component = (workflow as ProcessHeader)?.CurrentComponent?.FC_Name.ToString();
			if (string.IsNullOrEmpty(component))
			{
				component = workflow.CurrentComponent.ToString();
			}
			cache[key].Add((workflow.SystemLastEditTimeUtc, workflow.CurrentComponent, component));

			if (workflow.IsDeleted)
			{
				cache.Remove(key);
			}
		}

		public void NotifySaveConcurrencyError(IEnumerable<ITransferrableProcessHeader> workflows)
		{
			foreach (var workflow in workflows)
			{
				cache.Remove(workflow.PK);
			}
		}

		public IEnumerable<(ITransferrableProcessHeader workflow, string path)> GetWorkflowsToDeactivate()
		{
			var pksAndPaths = GetWorkflowPKsToDeactivate();
			var workflows = dataAccessor.LoadWorkflowsToDeactivate(pksAndPaths.Select(pair => pair.workflowPK)).ToDictionary(w => w.PK);
			var workflowsAndPaths = pksAndPaths
				.Select(pair => (workflows[pair.workflowPK], pair.path))
				.Where(pair => pair.Item1 != null)
				.ToArray();

			return workflowsAndPaths;
		}

		IEnumerable<(Guid workflowPK, string path)> GetWorkflowPKsToDeactivate()
		{
			foreach (var pk in cache.Keys)
			{
				var timesAndPKs = cache[pk];
				var componentGroups = timesAndPKs.Distinct().GroupBy(x => x.componentPK).ToArray();

				var containsLoop = componentGroups.Length > 1 && componentGroups.Any(c => c.Count() > 1);
				if (containsLoop)
				{
					var times = timesAndPKs.Skip(1).Select(t => t.lastChange);
					var hasNoStateChanges = times.Batch(2).All(g => g.Distinct().Count() == 1);
					if (hasNoStateChanges)
					{
						var path = string.Join(",", timesAndPKs.Select(item => FormattableString.Invariant($"({item.component},{item.lastChange:O})")));
						yield return (pk, path);
					}
				}
			}
		}

		readonly Dictionary<Guid, List<(DateTime lastChange, Guid componentPK, string component)>> cache = new Dictionary<Guid, List<(DateTime lastChange, Guid componentPK, string component)>>();
	}
}
