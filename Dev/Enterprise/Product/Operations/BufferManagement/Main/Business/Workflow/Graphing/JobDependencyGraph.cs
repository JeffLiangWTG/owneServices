using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;

namespace Enterprise.BufferManagement.Business
{
	public class JobDependencyGraph : ProcessHeaderDependencyGraph
	{
		public static JobDependencyGraph Create(ProcessJobHeader processJobHeader)
		{
			if (processJobHeader.Template != null)
			{
				return new TemplateDependencyGraph(processJobHeader);
			}
			else
			{
				return new JobDependencyGraph(processJobHeader);
			}
		}

		protected JobDependencyGraph(ProcessJobHeader processJobHeader)
			: base(processJobHeader)
		{
		}

		protected override IEnumerable<ProcessHeader> GetVertexes()
		{
			return base.GetVertexes()
				.Where(x => x.FH_FH_ParentHeader == Source.PK);
		}

		public override string GetSequence(ProcessHeader vertex)
		{
			if (!EntitySequence.ContainsKey(vertex.PK.ToGuid()) && vertex.IsWorkflow && vertex.IsInSameJob(Source))
			{
				return GetSequenceForChildOfWorkflowInJob(vertex);
			}
			else
			{
				return base.GetSequence(vertex);
			}
		}

		protected override IEnumerable<ProcessHeader> SequenceEntities(IEnumerable<ProcessHeader> entities)
		{
			return entities.OrderBy(w => w.FH_SystemCreateTimeUtc).ThenBy(w => w.FH_CompletionStatement);
		}

		string GetSequenceForChildOfWorkflowInJob(ProcessHeader vertex)
		{
			var result = new StringBuilder();
			var parents = vertex.GetWorkflowParents().Reverse().Append(vertex).ToArray();

			for (int i = 0; i < parents.Length; i++)
			{
				var parent = i > 0 ? parents[i - 1] : null;
				var entity = parents[i];

				string sequence = null;

				if (EntitySequence.ContainsKey(entity.PK.ToGuid()))
				{
					sequence = base.GetSequence(entity);
				}
				else if (parent != null)
				{
					var graph = childGraphs.ContainsKey(parent)
						? childGraphs[parent]
						: childGraphs[parent] = new ProcessHeaderDependencyGraph(parent);

					sequence = graph.GetSequence(entity);
				}

				if (result.Length > 0 && !string.IsNullOrEmpty(sequence))
				{
					result.Append(".");
				}
				result.Append(sequence);
			}

			return result.ToString();
		}

		readonly Dictionary<ProcessHeader, ProcessHeaderDependencyGraph> childGraphs = new Dictionary<ProcessHeader, ProcessHeaderDependencyGraph>();
	}
}
