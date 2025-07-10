using System.Collections.Generic;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using QuikGraph;

namespace Enterprise.BufferManagement.Business
{
	public class ScheduleGraph
	{
		public ScheduleGraph()
		{
			CrossBoundaryDependencies = new HashSet<EdgeWithOffset>();
			Graph = new BidirectionalGraph<ScheduleNode, EdgeWithOffset>();
			SchedulesByEntity = new Dictionary<ZGuid, ScheduleNode>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal CriticalPathDurationHours { get; internal set; }

		public Dictionary<ZGuid, ScheduleNode> SchedulesByEntity { get; private set; }

		public BidirectionalGraph<ScheduleNode, EdgeWithOffset> Graph { get; private set; }

		public HashSet<EdgeWithOffset> CrossBoundaryDependencies { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public ScheduleNode AddSchedule(ILinkEntity entity, decimal estimatedHours)
		{
			var node = new ScheduleNode(this, entity);
			node.EstimatedDurationHoursIncludingChildren = estimatedHours;
			node.AgreedDeliveryDateInUtc = entity.AgreedDeliveryDateInUtc;

			SchedulesByEntity[entity.PK] = node;
			Graph.AddVertex(node);

			return node;
		}

		public void TryAddLink(ZGuid prereqPk, ZGuid postreqPk)
		{
			ScheduleNode prereq, postreq;

			if (SchedulesByEntity.TryGetValue(prereqPk, out prereq) && SchedulesByEntity.TryGetValue(postreqPk, out postreq))
			{
				EdgeWithOffset edge;
				if (!Graph.TryGetEdge(prereq, postreq, out edge))
				{
					edge = new EdgeWithOffset(prereq, postreq);
					Graph.AddEdge(edge);

					var candidateForCrossBoundary = prereq.Parent != postreq.Parent;
					if (candidateForCrossBoundary)
					{
						CrossBoundaryDependencies.Add(edge);
					}
				}
			}
		}
	}
}
