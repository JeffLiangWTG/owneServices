using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Implementation;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.Schema;
using QuikGraph;
using QuikGraph.Algorithms;

namespace Enterprise.BufferManagement.Business
{
	public static class WorkflowRelationshipGraphBuilder
	{
		#region Public API

		/// <summary>
		/// Get all headers that are part of a circular Parent-Child relationship related to the header.
		/// If there is only one header, no circular Parent-Child relationship exists.
		/// </summary>
		public static ProcessHeader[] GetHierarchicCycles(this ProcessHeader header, WorkflowCycleCache cacheOverride = null, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			return GetCycle(header, cacheOverride, PopulateHierarchyCycles, tempLinks);
		}

		/// <summary>
		/// Get all headers that are part of a circular dependency with the input header.
		/// If there is only one header, no circular dependency exists.
		/// </summary>
		public static ProcessHeader[] GetDependencyCycles(this ProcessHeader header, WorkflowCycleCache cacheOverride = null, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			return GetCycle(header, cacheOverride, PopulateRelationCycles, tempLinks);
		}

		/// <summary>
		/// Get a graph of all Parent Child relationships of a header.
		/// </summary>
		public static WorkflowGraph GetHierarchyGraph(this ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			var hierarchyGraph = new WorkflowGraph();
			hierarchyGraph.AddVertex(header);

			var queue = new Queue<ProcessHeader>();
			queue.Enqueue(header);

			while (queue.Count > 0)
			{
				var entity = queue.Dequeue();

				foreach (var child in GetChildHeaders(entity, tempLinks))
				{
					AddNotSeenEntitiesToGraph(hierarchyGraph, queue, entity, child, (e, c) => new SEdge<ProcessHeader>(c, e));
				}

				foreach (var parent in GetParentHeaders(entity, tempLinks))
				{
					AddNotSeenEntitiesToGraph(hierarchyGraph, queue, entity, parent, (e, p) => new SEdge<ProcessHeader>(e, p));
				}
			}

			return hierarchyGraph;
		}

		static IEnumerable<ProcessHeader> GetChildHeaders(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			return tempLinks == null ? header.ChildHeaders : TemporaryTemplateLink.GetChildHeadersFromTemporaryLinks(header, tempLinks);
		}

		static IEnumerable<ProcessHeader> GetParentHeaders(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			return tempLinks == null ? header.ParentHeaders : TemporaryTemplateLink.GetParentHeadersFromTemporaryLinks(header, tempLinks);
		}

		static void AddNotSeenEntitiesToGraph(WorkflowGraph hierarchyGraph, Queue<ProcessHeader> queue, ProcessHeader entity, ProcessHeader addedEntity, Func<ProcessHeader, ProcessHeader, SEdge<ProcessHeader>> createEdge)
		{
			if (!hierarchyGraph.ContainsVertex(addedEntity))
			{
				queue.Enqueue(addedEntity);
				hierarchyGraph.AddVertex(addedEntity);
			}

			var edge = createEdge(entity, addedEntity);

			if (!hierarchyGraph.ContainsEdge(edge))
			{
				hierarchyGraph.AddEdge(edge);
			}
		}

		#endregion

		#region Identify Header Cycles

		static ProcessHeader[] GetCycle(ProcessHeader header, WorkflowCycleCache cacheOverride, Action<ProcessHeader, WorkflowCycleCache, IEnumerable<TemporaryTemplateLink>> populate, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			var cache = cacheOverride ?? new WorkflowCycleCache();

			ProcessHeader[] cycle;
			if (cache.TryGetCycle(header, out cycle))
			{
				return cycle;
			}
			else
			{
				populate(header, cache, tempLinks);

				if (cache.TryGetCycle(header, out cycle))
				{
					return cycle;
				}
				else
				{
					throw new InvalidOperationException("Logic error: PopulateCache failed to populate the cache...");
				}
			}
		}

		static void PopulateHierarchyCycles(ProcessHeader header, WorkflowCycleCache cache, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			var hierarchyGraph = GetHierarchyGraph(header, tempLinks);
			PopulateStronglyConnectedCache(cache, hierarchyGraph);
		}

		/// <summary>
		/// Populate a graph with Dependency links and Parent-Child links to check
		/// if it will create a circular dependency involving the related header.
		/// </summary>
		static void PopulateRelationCycles(ProcessHeader processHeader, WorkflowCycleCache foundCyclesCache, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			var graph = PopulateDependencyAndHierarchyGraph(processHeader, tempLinks);
			PopulateStronglyConnectedCache(foundCyclesCache, graph);
		}

		/// <summary>
		/// Creates a graph based on the passed-in process header. Directionality of the graph indicates the finishability of the workflow (whether the workflow is able to be finished).
		/// Eg: workflowA -> workflowB -> workflowC, workflowB is not finishable while workflowA is unfinished.
		/// </summary>
		static WorkflowGraph PopulateDependencyAndHierarchyGraph(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			var related = FetchAllRelated(header, tempLinks);

			var graph = new WorkflowGraph();
			graph.AddVertexRange(related.Entities.Cast<ProcessHeader>());
			new ProcessHeaderDescendantsStrategy().DoToApplicableLinks(header, related.DependencyLinks, (_, from, to) => graph.AddVerticesAndEdge(new SEdge<ProcessHeader>((ProcessHeader)from, (ProcessHeader)to)));

			foreach (var link in related.FamilyLinks)
			{
				var linkTo = (ProcessHeader)link.To;
				var linkFrom = (ProcessHeader)link.From;

				if (!graph.ContainsVertex(linkTo))
				{
					graph.AddVertex(linkTo);
				}

				if (!graph.ContainsVertex(linkFrom))
				{
					graph.AddVertex(linkFrom);
				}

				var edge = new SEdge<ProcessHeader>(linkFrom, linkTo);
				if (!graph.ContainsEdge(edge))
				{
					graph.AddEdge(edge);
				}
			}

			return graph;
		}

		/// <summary>
		/// Flattens the passed-in graph by applying parent relationships to children, and caches the output in the passed-in cache.
		/// </summary>
		static void PopulateStronglyConnectedCache(WorkflowCycleCache cycleCache, WorkflowGraph graph)
		{
			foreach (var innerCycle in graph.CondensateStronglyConnected<ProcessHeader, SEdge<ProcessHeader>, WorkflowGraph>().Vertices)
			{
				cycleCache.AddCycle(innerCycle);
			}
		}

		#region Load Entities in batches recursively

		/// <summary>
		/// Creates a POCO of workflows and the relationships between them.
		/// Directionality is as follows:
		/// Prereq -> Postreq (stored in the relatedLinks array),
		/// Child -> Parent (stored in the familyLinks array).
		/// These workflows and relationships can be used to make a graph modelling the related workflow network for the given process header.
		/// </summary>
		/// <param name="header"> The header to start making a graph from</param>
		static LinkSet FetchAllRelated(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			ILinkDescendantsStrategy<ProcessHeader> strategy;
			var relatedEntities = new HashSet<ILinkEntity>();
			var dependencyLinks = new HashSet<ILink>();
			var familyLinks = new HashSet<ILink>();
			var entityQueue = new Queue<ILinkEntity>();
			var factory = header.Factory;
			var processTemporaryLinks = tempLinks != null;

			EnsureEntityInQueueAndLoadLinksAndFetchHints(relatedEntities, entityQueue, factory, new[] { header }, processTemporaryLinks);

			if (tempLinks == null)
			{
				strategy = new ProcessHeaderDescendantsStrategy();
			}
			else
			{
				strategy = new ProcessHeaderDescendantsStrategyForTemporaryTemplateLinks(tempLinks);
			}

			while (entityQueue.Any())
			{
				var entity = entityQueue.Dequeue();
				var headerToFetch = (ProcessHeader)entity;
				var parents = entity.Parents(strategy);
				var parentsAndChildrenOfEntity = parents.Union(entity.Children(strategy)).Distinct().Cast<ProcessHeader>().ToArray();

				EnsureEntityInQueueAndLoadLinksAndFetchHints(relatedEntities, entityQueue, factory, parentsAndChildrenOfEntity, processTemporaryLinks);

				foreach (var parent in parents)
				{
					familyLinks.Add(new VirtualLink(headerToFetch, (ProcessHeader)parent));
				}

				var prerequisiteHeaders = new List<ProcessHeader>();

				foreach (var link in GetPrerequisiteLinks(headerToFetch, tempLinks))
				{
					var relatedEntity = link.HeaderFrom;
					if (relatedEntity != null && dependencyLinks.Add(link as ILink))
					{
						prerequisiteHeaders.Add(relatedEntity);
					}
				}

				if (prerequisiteHeaders.Any())
				{
					EnsureEntityInQueueAndLoadLinksAndFetchHints(relatedEntities, entityQueue, factory, prerequisiteHeaders.ToArray(), processTemporaryLinks);
				}
			}

			return new LinkSet(relatedEntities, dependencyLinks, familyLinks);
		}

		static IEnumerable<ILoopDetectable> GetPrerequisiteLinks(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			return tempLinks == null ? header.PrerequisiteLinks : TemporaryTemplateLink.GetPrerequisiteLinksFromTemporaryLinks(header, tempLinks);
		}

		static void EnsureEntityInQueueAndLoadLinksAndFetchHints(ISet<ILinkEntity> relatedEntities, Queue<ILinkEntity> entityQueue, BusinessObjectFactory factory, ICollection<ProcessHeader> relatedEntitiesToAdd, bool processTemporaryLinks)
		{
			if (processTemporaryLinks)
			{
				foreach (var relatedEntity in relatedEntitiesToAdd)
				{
					if (relatedEntities.Add(relatedEntity))
					{
						entityQueue.Enqueue(relatedEntity);
					}
				}
			}
			else
			{
				ProcessHeaderLink.LoadLinksIntoFactoryOptimisingForUnsavedProcessHeaders(factory, relatedEntitiesToAdd);

				foreach (var relatedEntity in relatedEntitiesToAdd)
				{
					if (relatedEntities.Add(relatedEntity))
					{
						entityQueue.Enqueue(relatedEntity);

						if (!relatedEntity.FH_FH_ParentHeader.IsEmpty)
						{
							factory.AddFetchHint(ProcessHeaderSchema.PK, relatedEntity.FH_FH_ParentHeader);
						}
					}
				}
			}
		}

		#endregion

		#endregion
	}

	#region Graph Implementations

	[Serializable]
	public class WorkflowGraph : BidirectionalGraph<ProcessHeader, SEdge<ProcessHeader>>
	{
		public WorkflowGraph()
			: base(allowParallelEdges: false)
		{
		}
	}

	public class WorkflowCycleCache
	{
		readonly Dictionary<ProcessHeader, ProcessHeader[]> cycleMap = new Dictionary<ProcessHeader, ProcessHeader[]>();

		public void AddCycle(WorkflowGraph graph)
		{
			var workflows = graph.Vertices.ToArray();

			foreach (var workflow in workflows)
			{
				cycleMap[workflow] = workflows;
			}
		}

		public bool TryGetCycle(ProcessHeader header, out ProcessHeader[] cycle)
		{
			return cycleMap.TryGetValue(header, out cycle);
		}
	}

	public class LinkSet
	{
		public LinkSet(HashSet<ILinkEntity> entities, HashSet<ILink> depLinks, HashSet<ILink> famLinks)
		{
			Entities = entities;
			DependencyLinks = depLinks;
			FamilyLinks = famLinks;
		}

		public HashSet<ILinkEntity> Entities { get; }
		public HashSet<ILink> DependencyLinks { get; }
		public HashSet<ILink> FamilyLinks { get; }
	}

	#endregion
}
