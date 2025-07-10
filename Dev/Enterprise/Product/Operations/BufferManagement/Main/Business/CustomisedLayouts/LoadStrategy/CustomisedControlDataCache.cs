using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class CustomisedControlDataCache
	{
		public CustomisedControlDataCache()
		{
			LineCaches = Enumerable.Empty<ControlCustomisationLinePropertyCache>();
			staticCaches = ImmutableDictionary<StaticControlProperty, StaticCustomisationLineCache>.Empty;
		}

		public CustomisedControlDataCache(BMControlCustomisation controlCustomisation, PropertyCache cache, IEnumerable<ProcessTask> tasks, IEnumerable<ProcessHeader> headers, bool showJobWorkflow, bool showWorkflow)
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(controlCustomisation.FM_JobType);
			var providerType = descriptor?.WorkflowProviderType;
			if (providerType != null)
			{
				var jobColumns = controlCustomisation.CustomisationLines.Cast<BMControlCustomisationLine>().
					Where(line => line.PropertySource == PropertySourceList.Codes.Job).
					Select(line => new TableColumn(providerType.Name, line.PropertyName));

				if (jobColumns.Any())
				{
					var parentBizos = tasks.Select(task => task.ParentBusinessObject).WhereNotNull().Distinct();
					foreach (var bizo in parentBizos)
					{
						bizo.FetchStrategy.FetchForView(jobColumns.ToArray());
					}
				}
			}

			LineCaches = controlCustomisation.CustomisationLines.Cast<BMControlCustomisationLine>()
				.Select(line => new ControlCustomisationLinePropertyCache(line, tasks, headers)).ToArray();

			staticCaches = controlCustomisation.CustomisedControls.Cast<StaticControlCustomisation>()
				.SelectMany(StaticCustomisationLineCache.GetRequiredProperties)
				.Distinct()
				.Select(line => new StaticCustomisationLineCache(line, cache, tasks, headers, showJobWorkflow, showWorkflow))
				.ToImmutableDictionary(s => s.Key);
		}

		public IEnumerable<ControlCustomisationLinePropertyCache> LineCaches { get; private set; }
		readonly ImmutableDictionary<StaticControlProperty, StaticCustomisationLineCache> staticCaches;

		internal TValue GetValue<TValue>(StaticControlProperty key, ICardContent content)
		{
			return staticCaches[key].GetCachedValue<TValue>(key, content);
		}
	}
}
