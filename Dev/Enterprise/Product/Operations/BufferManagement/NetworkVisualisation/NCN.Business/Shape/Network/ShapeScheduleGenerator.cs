using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using QuikGraph;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	static class ShapeScheduleGenerator
	{
		internal static ScheduleGraph CreateScheduleNetwork(IJobNetwork jobNetwork)
		{
			var entities = jobNetwork.Entities.ShapeEntities;
			var attachments = entities.SelectMany(s => s.Links).Distinct();
			var root = jobNetwork.DiagramEntity;

			var graph = new ScheduleGraph();
			var entityHash = new HashSet<ILinkEntity>(entities);
			entityHash.Add(root);
			var descendantsStrategy = new ShapeNetworkEntityDescendantsStrategy();

			var scale = root.Shape.Scale;

			foreach (var shape in entities.Where(IsSchedulable))
			{
				MakeNode(graph, descendantsStrategy, shape);
			}

			var rootSchedule = MakeNode(graph, descendantsStrategy, root);
			rootSchedule.EstimatedDurationHoursIncludingChildren = 0m;
			rootSchedule.IsScheduleApplicableToCriticalChainDuration = false;

			var scheduleScope = graph.SchedulesByEntity;

			SetParents(scheduleScope);
			AddLinksToScheduleScope(descendantsStrategy, jobNetwork.DiagramEntity, graph, attachments);

			foreach (var node in scheduleScope.Values)
			{
				node.Pin = GetPin(graph, descendantsStrategy, (ShapeNetworkEntity)node.Entity, scale);
			}

			return graph;
		}

		static ScheduleNode MakeNode(ScheduleGraph graph, ILinkDescendantsStrategy descendantsStrategy, ShapeNetworkEntity shape)
		{
			var node = graph.AddSchedule(shape, shape.Shape.ExplicitDurationHours);
			node.Depth = shape.Ancestors(descendantsStrategy).Count();
			node.IsScheduleApplicableToCriticalChainDuration = !shape.IsBufferShape;

			return node;
		}

		static void SetParents(Dictionary<ZGuid, ScheduleNode> scheduleScope)
		{
			foreach (var node in scheduleScope.Values)
			{
				var shape = (ShapeNetworkEntity)node.Entity;
				var owner = shape.Owner;
				ScheduleNode parent;

				if (owner != null && scheduleScope.TryGetValue(owner.PK, out parent))
				{
					node.Parent = parent;
				}
			}
		}

		internal static bool IsSchedulable(ShapeNetworkEntity shape)
		{
			return !shape.Shape.IsAnnotation;
		}

		static void AddLinksToScheduleScope(ILinkDescendantsStrategy descendantsStrategy, ILinkEntity source, ScheduleGraph graph, IEnumerable<ILink> links)
		{
			descendantsStrategy.DoToApplicableLinks(source, links, (link, prereq, postreq) => graph.TryAddLink(prereq.PK, postreq.PK));
		}

		internal static BidirectionalGraph<ILinkEntity, SEdge<ILinkEntity>> CreateRelationshipGraph(this ILinkDescendantsStrategy descendantsStrategy, ILinkEntity source, HashSet<ILinkEntity> entities, IEnumerable<ILink> links)
		{
			var graph = new BidirectionalGraph<ILinkEntity, SEdge<ILinkEntity>>();
			graph.AddVertexRange(entities);
			descendantsStrategy.DoToApplicableLinks(source, links, (link, prereq, postreq) => AddEdgeAndMissingVerticies(graph, prereq, postreq));
			return graph;
		}

		static void AddEdgeAndMissingVerticies(BidirectionalGraph<ILinkEntity, SEdge<ILinkEntity>> graph, ILinkEntity prereq, ILinkEntity postreq)
		{
			if (!graph.ContainsVertex(prereq))
			{
				graph.AddVertex(prereq);
			}

			if (!graph.ContainsVertex(postreq))
			{
				graph.AddVertex(postreq);
			}

			graph.AddEdge(new SEdge<ILinkEntity>(prereq, postreq));
		}

		#region Make pins

		static ScheduleNodePin GetPin(ScheduleGraph graph, ILinkDescendantsStrategy descendantsStrategy, ShapeNetworkEntity shape, ZDateTime scale)
		{
			var shapePin = shape.PinProvider.Pin;

			if (shapePin != null && shape.IsPinned) // Because shapes can have a virtual pin for unknown purposes when they are approved, as opposed to pinned.
			{
				var found = false;
				var pinAncestor = shapePin.Ancestor;
				var depth = 0;

				foreach (var ancestor in shape.Ancestors(descendantsStrategy))
				{
					depth++;
					if (ancestor == pinAncestor)
					{
						found = true;
						break;
					}
				}

				if (found)
				{
					return new ScheduleNodePin(graph, pinAncestor, shape, (decimal)ShapeOffsetToDateConverter.GetMinutesForScaleSize(scale, shapePin.XOffset) / 60m, depth);
				}
			}

			return null;
		}

		#endregion
	}
}
