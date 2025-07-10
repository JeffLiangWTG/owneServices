using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common.EntityRepositories;
using Enterprise.DataTransfer.Native.Common.Exceptions;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Registry.Business;
using static Enterprise.DataTransfer.Native.Utils.Models.GraphNodeExtension;

namespace Enterprise.DataTransfer.Native.Common.Operations
{
	public class UpdateOperation : IUpdateOperation
	{
		public IEntityRepository EntityRepository { get; set; }

		public void Update(IEntitySet entitySet)
		{
			EntityRepository.OpenSession();

			if (!(entitySet.Definition?.UseBatching ?? false))
			{
				// Noted: Update will highly depended on Traversing Sequence
				 entitySet.Root.DepthFirstTraversal(null, UpdateAction);
			}
			else
			{
				BatchUpdate(entitySet);
			}

			ProcessNotFoundEntities();
			UpdateParentAfterChildrenAreProcessed(entitySet.Root);

			EntityRepository.CloseSession();
		}

		void BatchUpdate(IEntitySet entitySet)
		{
			IEntity rootEntity = entitySet.Root;
			var visited = new VisitedNodeList<IEntity>();
			visited.AddValue(rootEntity);
			var rootBatch = new List<IEntity>() { rootEntity };
			ProcessParents(visited, rootBatch);
			UpdateAction(rootEntity, null);
			ProcessChildrenInBatches(visited, rootBatch);
		}

		void ProcessChildrenInBatches(VisitedNodeList<IEntity> visited, List<IEntity> parents)
		{
			const int batchSize = 20000;
			var batch = new List<IEntity>(batchSize);

			// Hash of the parent entity names in the current batch. Calculated once per batch for efficiency.
			var parentEntityNamesInBatch = new HashSet<string>();

			var allChildrenEntityNameGroups = parents
				.SelectMany(x => x.Children)
				.GroupBy(x => x.Definition.EntityName);

			foreach (var childEntityGroup in allChildrenEntityNameGroups)
			{
				batch.Clear();
				var lookupParentToChildren = childEntityGroup
					.Where(x => !visited.ContainsKey(x))
					.ToLookup(x => x.Parent.InternalPK);

				// Make a list of the children for each parent.
				// Children for the same parent are in a queue since we have to process them in order.
				// Different parents can be processed in any order.
				// During processing, if all the children for a parent are done, that list item is set to null for efficient skipping.
				// Could implement this as a LinkedList, but that would cause thousands more memory allocations - one for each node.
				var childQueues = new List<Queue<IEntity>>(lookupParentToChildren.Count);
				foreach (var parent in lookupParentToChildren)
				{
					childQueues.Add(new Queue<IEntity>(parent));
				}

				int firstParentIndex = 0;
				int currentParentIndex = 0;
				int parentsRemaining = lookupParentToChildren.Count;
				while (parentsRemaining > 0)
				{
					var childQueueForCurrentParent = childQueues[currentParentIndex];
					var child = childQueueForCurrentParent.Peek();
					bool moveToNextParent = false;

					if (batch.Count == 0)
					{
						BuildParentNameSetForNewBatch(parentEntityNamesInBatch, child);
					}

					if (batch.Count == 0 || HasSameActionAndElements(batch[0], parentEntityNamesInBatch, child))
					{
						childQueueForCurrentParent.Dequeue();
						batch.Add(child);
						visited.AddValue(child);
						if (childQueueForCurrentParent.Count == 0)
						{
							--parentsRemaining;
							if (parentsRemaining > 0)
							{
								childQueues[currentParentIndex] = null;
								moveToNextParent = true;
								if (firstParentIndex == currentParentIndex)
								{
									firstParentIndex = FindIndexWhereNotNull(firstParentIndex + 1, childQueues);
								}
							}
						}
					}
					else
					{
						moveToNextParent = true;
					}

					bool shouldProcessBatch = batch.Count >= batchSize || parentsRemaining == 0;
					if (!shouldProcessBatch && moveToNextParent)
					{
						var nextParentIndex = Math.Max(currentParentIndex + 1, firstParentIndex);
						nextParentIndex = nextParentIndex < childQueues.Count
							? FindIndexWhereNotNull(nextParentIndex, childQueues)
							: -1;
						if (nextParentIndex >= 0)
						{
							currentParentIndex = nextParentIndex;
						}
						else
						{
							// No more parents are available to fill the batch
							shouldProcessBatch = true;
						}
					}

					if (shouldProcessBatch)
					{
						ProcessBatch(visited, batch);
						batch.Clear();
						currentParentIndex = firstParentIndex;
					}
				}
			}
		}

		static int FindIndexWhereNotNull(int startIndex, List<Queue<IEntity>> list)
			=> list.FindIndex(startIndex, x => x != null);

		static void BuildParentNameSetForNewBatch(HashSet<string> parentEntityNames, IEntity entity)
		{
			parentEntityNames.Clear();
			foreach (var parent in entity.Parents)
			{
				parentEntityNames.Add(parent.EntityName);
			}
		}

		bool HasSameActionAndElements(IEntity entity1, HashSet<string> entity1ParentNames, IEntity entity2)
		{
			return entity1.Action == entity2.Action
				&& entity1.PropertyCount == entity2.PropertyCount
				&& entity1ParentNames.Count == entity2.Parents.Count()
				&& entity2.Properties.All(x => entity1.HasProperty(x.Name))
				&& entity2.Parents.All(x => entity1ParentNames.Contains(x.EntityName));
		}

		void ProcessParents(VisitedNodeList<IEntity> visited, List<IEntity> batch)
		{
			// Do all parents. Future optimization would be to do all the foreign key lookups in a batch.
			foreach (var entity in batch)
			{
				foreach (var parent in entity.Parents.Where(x => !visited.ContainsKey(x)))
				{
					DepthNestedTraversal(parent, entity, visited, null, UpdateAction, null);
				}
			}
		}

		void ProcessBatch(VisitedNodeList<IEntity> visited, List<IEntity> batch)
		{
			ProcessParents(visited, batch);

			if (batch[0].Action == EntityAction.MERGE)
			{
				BatchMerge(visited, batch);
			}
			else
			{
				// Other actions are not batched (yet)
				foreach (var entity in batch)
				{
					DepthNestedTraversal(entity, entity.Parent, visited, null, UpdateAction, null);
				}
			}
		}

		void BatchMerge(VisitedNodeList<IEntity> visited, List<IEntity> batch)
		{
			EntityRepository.MergeBatch(batch);

			foreach (var entity in batch.Where(x => x.Action == EntityAction.INSERT))
			{
				ChangeMergeToInsertForChildrenRecursively(entity);
			}

			ProcessChildrenInBatches(visited, batch);
		}

		void ChangeMergeToInsertForChildrenRecursively(IEntity parent)
		{
			foreach (var child in parent.Children.Where(x => x.Action == EntityAction.MERGE))
			{
				child.Action = EntityAction.INSERT;
				ChangeMergeToInsertForChildrenRecursively(child);
			}
		}

		internal void UpdateAction(IEntity entity, IEntity relative)
		{
			var action = entity.Action;
			switch (action)
			{
				case EntityAction.INSERT:
					EntityRepository.Insert(entity);
					break;

				case EntityAction.UPDATE:
					EntityRepository.Update(entity);
					break;

				case EntityAction.MERGE:
					try
					{
						EntityRepository.Update(entity);
					}
					catch (ParentReferenceMismatchException)
					{
						throw;
					}
					catch (InvalidOperationException)
					{
						EntityRepository.Insert(entity);
					}
					break;

				case EntityAction.DELETE:
					EntityRepository.Delete(entity);
					break;

				case EntityAction.EMPTY:
					try
					{
						PerformSearch(entity, relative);
					}
					catch (NativeXMLUserVisibleException)
					{
						if (relative == null)
						{
							throw;
						}
						else
						{
							// Hide exception for now, this entity may be added later
							notFoundEntities.Add(new KeyValuePair<IEntity, IEntity>(entity, relative));
						}
					}
					break;
				case EntityAction.IGNORE:
					break;
			}
		}

		void UpdateParentAfterChildrenAreProcessed(IEntity parent)
		{
			var entities = EntityRepository.Statistics?.EntityAffected;
			if (entities != null)
			{
				var parentIsUnchanged = entities.Count(e => e.PK == parent.InternalPK && e.Action == Stat.DBEntity.DbAction.Unchanged) == 1;
				var childrenHaveChanges = entities.Count() != EntityRepository.Statistics.GetCount(Stat.DBEntity.DbAction.Unchanged);
				if (parentIsUnchanged && childrenHaveChanges && ShouldUpdateParentAfterChildrenAreProcessed(parent))
				{
					EntityRepository.SetModified(parent);
					UpdateAction(parent, null);
				}
			}
		}

		bool ShouldUpdateParentAfterChildrenAreProcessed(IEntity parent)
		{
			if (!string.IsNullOrEmpty(SystemDataRegistry.Instance.DisableLastEditUpdateOfParentInNativeXml.Value))
			{
				var excludedBizObjs = SystemDataRegistry.Instance.DisableLastEditUpdateOfParentInNativeXml.Value.Split(',')
					.Select(x => x.Trim())
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.ToArray();

				return !excludedBizObjs.Contains(parent.TableName);
			}

			return true;
		}

		void PerformSearch(IEntity entity, IEntity relative)
		{
			try
			{
				var properties = entity.Properties;
				var hasValues = properties.Select(x => x.Value.ToString()).Any(x => !string.IsNullOrEmpty(x));
				if (hasValues)
				{
					EntityRepository.Find(entity);
				}
			}
			catch (NativeXMLUserVisibleException ex)
			{
				if (relative == null)
				{
					throw;
				}
				else
				{
					var refereeName = DataBoundResourceStrings.GetTableDescriptiveName(entity.EntityName);
					var refererName = DataBoundResourceStrings.GetTableDescriptiveName(relative.EntityName);
					throw new NativeXMLUserVisibleException(string.Format(CultureInfo.InvariantCulture,
						"Could not insert/update the {0} ({1}) as it had an invalid reference to a {2} ({3}). {4}",
						refererName, relative.EntityName, refereeName, entity.EntityName, ex.Message), ex);
				}
			}
		}

		void ProcessNotFoundEntities()
		{
			try
			{
				foreach (var entry in notFoundEntities)
				{
					PerformSearch(entry.Key, entry.Value);
				}
			}
			finally
			{
				notFoundEntities.Clear();
			}
		}

		readonly ICollection<KeyValuePair<IEntity, IEntity>> notFoundEntities = new Collection<KeyValuePair<IEntity, IEntity>>();
	}
}
