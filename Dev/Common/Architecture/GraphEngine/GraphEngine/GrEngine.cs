using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;

namespace CargoWise.GraphEngine
{
	/// <summary>
	/// The Queue<T> datastructure can be used to ensure that a set of entities are
	/// processed in the order in which they are added to the queue.
	/// 
	/// The GrEngine similarly is used for this purpose. 
	/// It has additional complexity that allows dequeued items to be processed in parallel without risk. 
	/// 
	/// This effect is achieved by giving each item a set of keys.
	/// The GrEngine will not dequeue two items with the same key.
	/// Once an entity has been marked as processed, 
	/// the GrEngine will no longer consider that item as blocking any other items.
	/// </summary>
	public abstract class GrEngine<TKey, TEntity>
		where TEntity : IEquatable<TEntity>
		where TKey : IEquatable<TKey>
	{
		#region State

		readonly StartTrackingGraph dependencies = new StartTrackingGraph();
		readonly Dictionary<TKey, NotableEntityTuple> keyToEntity = new Dictionary<TKey, NotableEntityTuple>();
		readonly Dictionary<TEntity, HashSet<TKey>> entityToKey = new Dictionary<TEntity, HashSet<TKey>>();

		IGrEngineTracker<TKey, TEntity> Tracker => tracker ?? (tracker = GetTracker());
		IGrEngineTracker<TKey, TEntity> tracker;

		#endregion

		#region Public API

		public LoadEntitiesResult LoadEntities()
		{
			var entities = LoadEntitiesCore().ToArray();
			Tracker.Loaded(entities);
			if (entities.Length > 0)
			{
				var oldEntities = FindProcessedEntities(entities);
				var oldEntitiesReadOnly = oldEntities.OrderBy(f => f, GetOrderComparer()).ToList().AsReadOnly();
				RemoveEntities(oldEntitiesReadOnly);
				Tracker.VetexRemoved(oldEntities, dependencies.ReassignEdgeCount);

				var newEntities = AddNewVertexes(entities.Where(e => !oldEntities.Contains(e)));
				Tracker.VetexAdded(newEntities);
				var entitiesWithKeys = GetKeyProvider().GetKeys(newEntities);
				AddPrerequisiteRelationships(entitiesWithKeys);

				return new LoadEntitiesResult(newEntities.AsReadOnly(), oldEntitiesReadOnly, Count, dependencies.ReassignEdgeCount);
			}
			else
			{
				var emptyReadOnlyList = new List<TEntity>().AsReadOnly();
				return new LoadEntitiesResult(emptyReadOnlyList, emptyReadOnlyList, Count, 0);
			}
		}

		public IEnumerable<TEntity> FetchUnblocked() => dependencies.UnblockedEntities;

		/// <summary>
		/// A Graph Engine Chain is a distinct startable queue of entities that do not share keys with any other startable queue.
		/// Any chain can be processed sequentially without risk of colliding with other chains.
		/// </summary>
		public Chain<TEntity>[] FindChains() => new ChainFinder(dependencies, GetKeyProvider(), GetOrderComparer()).FindChains();

		public int Count => dependencies.VertexCount;

		#endregion

		#region Abstract Members

		protected abstract IKeyProvider<TKey, TEntity> GetKeyProvider();
		protected abstract IProcessedProvider<TEntity> GetProcessedProvider();
		protected abstract IComparer<TEntity> GetOrderComparer();
		protected abstract IGrEngineTracker<TKey, TEntity> GetTracker();
		protected abstract IEnumerable<TEntity> LoadEntitiesCore();

		#endregion

		#region Implementation

		void RemoveEntities(IEnumerable<TEntity> oldEntities)
		{
			dependencies.ReassignEdgeCount = 0; // Hack, on account of my hook into QuickGraph.

			foreach (var entity in oldEntities)
			{
				dependencies.RemoveVertex(entity);
				HashSet<TKey> keys;
				if (entityToKey.TryGetValue(entity, out keys))
				{
					entityToKey.Remove(entity);

					foreach (var key in keys)
					{
						NotableEntityTuple tuple;
						if (keyToEntity.TryGetValue(key, out tuple) && tuple.LastAdded.Equals(entity))
						{
							keyToEntity.Remove(key);
						}
					}
				}
			}
		}

		HashSet<TEntity> FindProcessedEntities(IEnumerable<TEntity> entities)
		{
			return new HashSet<TEntity>(GetProcessedProvider().GetProcessed(entities));
		}

		List<TEntity> AddNewVertexes(IEnumerable<TEntity> entities)
		{
			var newEntities = new List<TEntity>(); // TODO: Add size hint.

			foreach (var entity in entities)
			{
				if (dependencies.AddVertex(entity))
				{
					newEntities.Add(entity);
				}
			}

			return newEntities;
		}

		void AddPrerequisiteRelationships(IEnumerable<IGrouping<TEntity, TKey>> entitiesWithKeys)
		{
			foreach (var group in entitiesWithKeys.OrderBy(e => e, GetNewElementOrderComparer()))
			{
				var prerequisites = FindLatestPrereqEntities(group);
				CreateLinks(prerequisites, group);
				AddToKeySets(group);
			}
		}

		IEnumerable<TEntity> FindLatestPrereqEntities(IEnumerable<TKey> keys)
		{
			var prereqEntities = new HashSet<TEntity>();

			foreach (var key in keys)
			{
				NotableEntityTuple tuple;
				if (keyToEntity.TryGetValue(key, out tuple))
				{
					prereqEntities.Add(tuple.LastAdded);
				}
			}

			return prereqEntities;
		}

		void CreateLinks(IEnumerable<TEntity> prerequisites, IGrouping<TEntity, TKey> newEntity)
		{
			foreach (var prereq in prerequisites)
			{
				CreateLink(prereq, newEntity);
			}
		}

		/// <summary>
		/// We are comparing keys between entities twice.
		/// The first time we find *any* duplicate keys.
		/// The second time we find *all* duplicate keys.
		/// </summary>
		void CreateLink(TEntity prereq, IGrouping<TEntity, TKey> newEntity)
		{
			var prereqKeys = entityToKey[prereq];
			var sharedKeys = new HashSet<TKey>(newEntity).Intersect(prereqKeys);
			dependencies.AddEdge(new KeyEdge(prereq, newEntity.Key, sharedKeys));
		}

		void AddToKeySets(IGrouping<TEntity, TKey> group)
		{
			var entity = group.Key;

			foreach (var key in group)
			{
				NotableEntityTuple tuple;
				if (keyToEntity.TryGetValue(key, out tuple))
				{
					tuple.LastAdded = entity;
				}
				else
				{
					keyToEntity[key] = new NotableEntityTuple(entity);
				}
			}

			entityToKey.Add(entity, new HashSet<TKey>(group));
		}

		IComparer<IGrouping<TEntity, TKey>> GetNewElementOrderComparer()
		{
			return new GroupingKeyComparerWrapper<TEntity, TKey>(GetOrderComparer());
		}

		#endregion

		#region Inner classes

		/// <summary>
		/// Tracking the first and last edges ensures that add an remove are O1.
		/// </summary>
		class NotableEntityTuple
		{
			public NotableEntityTuple(TEntity entity)
			{
				LastAdded = entity;
			}

			internal TEntity LastAdded { get; set; }
		}

		class ChainFinder
		{
			public ChainFinder(StartTrackingGraph graph, IKeyProvider<TKey, TEntity> keyProvider, IComparer<TEntity> comparer)
			{
				this.graph = graph;
				this.keyProvider = keyProvider;
				this.comparer = comparer;
			}

			readonly StartTrackingGraph graph;
			readonly IKeyProvider<TKey, TEntity> keyProvider;
			readonly IComparer<TEntity> comparer;

			public Chain<TEntity>[] FindChains()
			{
				var queue = new Queue<(TEntity, HashSet<TKey>, HashSet<TEntity>, List<TEntity>)>(keyProvider.GetKeys(graph.UnblockedEntities).Select(g => (g.Key, new HashSet<TKey>(g), new HashSet<TEntity>(), new List<TEntity>())));
				var result = new List<Chain<TEntity>>();

				while (queue.Any())
				{
					var (node, sharedKeys, visitedNodes, chain) = queue.Dequeue();
					sharedKeys.IntersectWith(keyProvider.GetKeys(new[] { node }).Single());

					var canAddNode = sharedKeys.Any() && (!graph.TryGetInEdges(node, out IEnumerable<KeyEdge> inEdges) || inEdges.All(i => visitedNodes.Contains(i.Source)));
					if (canAddNode)
					{
						chain.Add(node);
					}

					if (!canAddNode
						|| !graph.TryGetOutEdges(node, out IEnumerable<KeyEdge> edges)
						|| !(edges.Select(e => e.Target).Where(k => !visitedNodes.Contains(k)).OrderBy(e => e, comparer).FirstOrDefault() is TEntity nextInChain))
					{
						result.Add(new Chain<TEntity>(chain));
					}
					else
					{
						visitedNodes.Add(node);
						queue.Enqueue((nextInChain, sharedKeys, visitedNodes, chain));
					}
				}

				return result.ToArray();
			}
		}

		[Serializable]
		class StartTrackingGraph : BidirectionalGraph<TEntity, KeyEdge>
		{
			public StartTrackingGraph()
			 : base(allowParallelEdges: false)
			{
			}

			readonly HashSet<TEntity> unblockedEntities = new HashSet<TEntity>();
			public int ReassignEdgeCount { get; set; } // Track how many times we remove an item from the middle of the set.

			public ICollection<TEntity> UnblockedEntities => unblockedEntities;

			protected override void OnVertexAdded(TEntity item)
			{
				base.OnVertexAdded(item);
				unblockedEntities.Add(item);
			}

			public override bool RemoveVertex(TEntity item)
			{
				TryGetOutEdges(item, out IEnumerable<KeyEdge> outEdges);
				TryGetInEdges(item, out IEnumerable<KeyEdge> inEdges);

				if (base.RemoveVertex(item))
				{
					unblockedEntities.Remove(item);

					outEdges = outEdges ?? Enumerable.Empty<KeyEdge>();
					var prereqs = inEdges ?? Enumerable.Empty<KeyEdge>();
					foreach (var edge in outEdges ?? Enumerable.Empty<KeyEdge>())
					{
						foreach (var prereq in prereqs)
						{
							ReassignEdgeCount++;
							AddEdge(new KeyEdge(prereq.Source, edge.Target, edge.Keys.Union(prereq.Keys).ToArray()));
						}

						RemovePostreq(edge);
					}

					return true;
				}
				else
				{
					return false;
				}
			}

			protected override void OnEdgeAdded(KeyEdge edge)
			{
				base.OnEdgeAdded(edge);
				unblockedEntities.Remove(edge.Target);
			}

			protected override void OnEdgeRemoved(KeyEdge edge)
			{
				base.OnEdgeRemoved(edge);
				RemovePostreq(edge);
			}

			void RemovePostreq(KeyEdge edge)
			{
				if (IsInEdgesEmpty(edge.Target))
				{
					unblockedEntities.Add(edge.Target);
				}
			}
		}

		class KeyEdge : IEdge<TEntity>
		{
			public KeyEdge(TEntity source, TEntity target, IEnumerable<TKey> sharedKeys)
			{
				Source = source;
				Target = target;
				Keys = sharedKeys;
			}

			public TEntity Source { get; }
			public TEntity Target { get; }
			public IEnumerable<TKey> Keys { get; }
		}

		public class LoadEntitiesResult
		{
			internal LoadEntitiesResult(IList<TEntity> newEntities, IList<TEntity> oldEntities, int totalEntities, int entitiesWithPrerequisites)
			{
				NewEntities = newEntities;
				OldEntities = oldEntities;
				TotalEntities = totalEntities;
				OldEntitiesWithPrerequisitesMoved = entitiesWithPrerequisites;
			}

			public IList<TEntity> NewEntities { get; }
			public IList<TEntity> OldEntities { get; }
			public int TotalEntities { get; }
			public int OldEntitiesWithPrerequisitesMoved { get; }
		}

		class GroupingKeyComparerWrapper<TGroupKey, TGroupEntity> : IComparer<IGrouping<TGroupKey, TGroupEntity>>
		{
			public GroupingKeyComparerWrapper(IComparer<TGroupKey> innerComparer)
			{
				this.innerComparer = innerComparer;
			}
			readonly IComparer<TGroupKey> innerComparer;

			public int Compare(IGrouping<TGroupKey, TGroupEntity> x, IGrouping<TGroupKey, TGroupEntity> y)
			{
				return innerComparer.Compare(x.Key, y.Key);
			}
		}

		#endregion
	}
}
