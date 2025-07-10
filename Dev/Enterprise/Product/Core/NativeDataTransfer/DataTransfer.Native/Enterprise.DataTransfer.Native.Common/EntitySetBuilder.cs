using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.EntityBuilders;
using Enterprise.DataTransfer.Native.Common.Sql;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common
{
	/// <summary>
	/// Build Entity Set base on Root Entity Row
	/// Use Divided-And-Conquer Algorithm
	/// </summary>
	public class EntitySetBuilder
	{
		public Entity BuildEntitySet(DataRow row, IEntityDefinition definition, AncillaryImportServices sessionServices)
		{
			var entity = BuildEntitySet(row, definition.ConvertToTree(), sessionServices);

			return entity;
		}

		public Entity BuildEntitySet(DataRow row, TreeNode<IEntityDefinition> self, AncillaryImportServices sessionServices)
		{
			if (row == null)
			{
				return null;
			}
			var definition = self.Data;
			var entity = BuildEntity(row, definition, sessionServices);
			var childNodeToRows = BuildChildRows(self, entity);

			var pkColumnName = definition.Table.Columns.PrimaryKey.Name;

			foreach (TreeNode<IEntityDefinition> childNode in self.Children)
			{
				var childNodeDefinition = childNode.Data;
				var optionalCondition = childNodeDefinition.OptionalEntityCondition;

				if (string.IsNullOrEmpty(optionalCondition) || OptionalEntityConditionMapping.IsEntityOptionalConditionSatisfied(optionalCondition))
				{
					var childNodeRows = childNodeToRows[childNode];

					if (childNodeDefinition.IsChildrenOf(definition))
					{
						var pk2ChildEntities = BuildEntitySets(childNodeRows, childNode, pkColumnName, sessionServices, ApplyMergeActionEnum.Child);
						LinkChildEntities(pk2ChildEntities, entity);
					}
					else if (childNodeDefinition.IsParentOf(definition))
					{
						if (!childNodeRows.Any())
						{
							var notInDBParentEntity = BuildReferencedEntityWhichDoesntExistInDB(row, definition, childNodeDefinition, sessionServices);  //ToDo. Create method LinkParentEntity(childEntity, entity)
							LinkParentEntity(notInDBParentEntity, entity);
						}
						else
						{
							var pk2ParentEntities = BuildEntitySets(childNodeRows, childNode, pkColumnName, sessionServices, ApplyMergeActionEnum.Parent);
							LinkParentEntities(pk2ParentEntities, entity);
						}
					}
				}
			}

			return entity;
		}

		enum ApplyMergeActionEnum
		{
			CannotMerge,
			Parent,
			Child
		}

		IDictionary<Guid, HashSet<Entity>> BuildEntitySets(IEnumerable<DataRow> rows, TreeNode<IEntityDefinition> self, string parentPKColumnName, AncillaryImportServices sessionServices, ApplyMergeActionEnum applyMergeAction)
		{
			var parentPK2Entities = new Dictionary<Guid, HashSet<Entity>>();
			if (!rows.Any())
			{
				return parentPK2Entities;
			}

			var definition = self.Data;

			var shouldApplyMergeAction = sessionServices.RequiresAdditionOfActionEqualsMerge && applyMergeAction != ApplyMergeActionEnum.CannotMerge;
			shouldApplyMergeAction &= (applyMergeAction == ApplyMergeActionEnum.Child || definition.RequiresAdditionOfActionEqualsMerge);
			foreach (var row in rows)
			{
				var uniqueParentPKColumnName = parentPKColumnName + "0";    //in SelfReference, the first "JZ_PK" is changed to "JZ_PK0" to resolve ambiguous column names(e.g. JZ_PK, JZ_PK)
				var parentPK = (Guid)row[uniqueParentPKColumnName];
				var entity = BuildEntity(row, definition, sessionServices);

				if (shouldApplyMergeAction)
				{
					entity.Action = EntityAction.MERGE;
				}

				Add(parentPK2Entities, parentPK, entity);
			}

			var entities = parentPK2Entities.SelectMany(x => x.Value).ToArray();
			var childNodeToRows = BuildChildRows(self, entities);

			var pk2ChildEntities = new Dictionary<Guid, HashSet<Entity>>();
			var pk2ParentEntities = new Dictionary<Guid, HashSet<Entity>>();

			var pkColumnName = definition.Table.Columns.PrimaryKey.Name;

			foreach (TreeNode<IEntityDefinition> childNode in self.Children)
			{
				var childNodeDefinition = childNode.Data;
				var childNodeRows = childNodeToRows[childNode];

				if (childNodeDefinition.IsChildrenOf(definition))
				{
					var childEntities = BuildEntitySets(childNodeRows, childNode, pkColumnName, sessionServices, shouldApplyMergeAction  ? ApplyMergeActionEnum.Child : ApplyMergeActionEnum.CannotMerge);
					Add(pk2ChildEntities, childEntities);
				}
				else if (childNodeDefinition.IsParentOf(definition))
				{
					var rowsWithNoParentRowInDB = GetRowsWithNoParentRowInDB(rows, childNodeRows);
					var referencedEntityWhichDoesntExistInDB = BuildReferencedEntityWhichDoesntExistInDB(rowsWithNoParentRowInDB, definition, childNodeDefinition, pkColumnName, sessionServices);
					Add(pk2ParentEntities, referencedEntityWhichDoesntExistInDB);

					var entitySets = BuildEntitySets(childNodeRows, childNode, pkColumnName, sessionServices, shouldApplyMergeAction ? ApplyMergeActionEnum.Parent : ApplyMergeActionEnum.CannotMerge);
					Add(pk2ParentEntities, entitySets);
				}
			}

			foreach (var entity in entities)
			{
				LinkChildEntities(pk2ChildEntities, entity);
				LinkParentEntities(pk2ParentEntities, entity);
			}

			return parentPK2Entities;
		}

		void Add(IDictionary<Guid, HashSet<Entity>> destination, Guid key, Entity value)
		{
			if (destination.TryGetValue(key, out HashSet<Entity> values))
			{
				values.Add(value);
			}
			else
			{
				values = new HashSet<Entity>();
				values.Add(value);

				destination[key] = values;
			}
		}

		void Add(IDictionary<Guid, HashSet<Entity>> destination, IDictionary<Guid, HashSet<Entity>> source)
		{
			foreach (var pair in source)
			{
				if (destination.TryGetValue(pair.Key, out HashSet<Entity> values))
				{
					values.UnionWith(pair.Value);
				}
				else
				{
					destination[pair.Key] = new HashSet<Entity>(pair.Value);
				}
			}
		}

		Dictionary<TreeNode<IEntityDefinition>, IEnumerable<DataRow>> BuildChildRows(TreeNode<IEntityDefinition> parentNode, params IEntity[] parentEntities)
		{
			var childNodes = parentNode.Children.Cast<TreeNode<IEntityDefinition>>().ToArray();
			var childNodeToRows = BuildRows(childNodes, parentEntities.ToArray());
			return childNodeToRows;
		}

		Dictionary<TreeNode<IEntityDefinition>, IEnumerable<DataRow>> BuildRows(TreeNode<IEntityDefinition>[] nodes, params IEntity[] parentEntities)
		{
			var result = new Dictionary<TreeNode<IEntityDefinition>, IEnumerable<DataRow>>();
			if (!parentEntities.Any() || !nodes.Any())
			{
				return result;
			}

			var self = parentEntities.First().Definition;
			var multipleValueCriteria = new MultipleValueCriteria
			{
				TableName = self.TableName,
				ColumnName = self.Table.Columns.PrimaryKey.Name,
				Values = parentEntities.Select(e => e.InternalPK).Cast<object>()
			};

			var childDefinitions = nodes.Select(e => e.Data);
			var dataSets = new SelectWithCriteria(childDefinitions, self, multipleValueCriteria).GetDataSets().ToArray();

			for (int i = 0; i < dataSets.Length; ++i)
			{
				result.Add(nodes[i], dataSets[i]);
			}

			return result;
		}

		// this function builds entity for a referenced object which doesn't actually exist in DB
		Entity BuildReferencedEntityWhichDoesntExistInDB(DataRow childRow, IEntityDefinition childDefinition, IEntityDefinition referencedDefinition, AncillaryImportServices sessionServices)
		{
			var path = referencedDefinition.FindPath(childDefinition);
			var relations = path.GetRelations();
			var entity = new Entity(referencedDefinition, sessionServices);
			var relation = relations.LastOrDefault();

			if (sessionServices.RequiresAdditionOfActionEqualsMerge && referencedDefinition.RequiresAdditionOfActionEqualsMerge)
			{
				entity.Action = EntityAction.MERGE;
			}
			if (relation != null)
			{
				foreach (var keyRelation in relation.Keys)
				{
					var toKey = keyRelation.ToKey;
					var fromKey = keyRelation.FromKey;
					// build entity from childRow[fromkey] by adding property named as toKey
					if (!string.IsNullOrEmpty(childRow[fromKey.Name]?.ToString()))
					{
						var propertyDef = referencedDefinition.PropertyDefinitions.FirstOrDefault(def => def.ColumnDef.Name == toKey.Name);
						if (propertyDef != null)
						{
							var property = new Property(propertyDef);
							property.Value = childRow[fromKey.Name];
							entity.AddProperty(property);
						}
					}
				}
			}
			return entity;
		}

		IDictionary<Guid, HashSet<Entity>> BuildReferencedEntityWhichDoesntExistInDB(IEnumerable<DataRow> childRows, IEntityDefinition childDefinition, IEntityDefinition referencedDefinition, string parentPKColumnName, AncillaryImportServices sessionServices)
		{
			var referencedEntitiesThatDontExistInTheDB = new Dictionary<Guid, HashSet<Entity>>();

			foreach (var childRow in childRows)
			{
				var entity = BuildReferencedEntityWhichDoesntExistInDB(childRow, childDefinition, referencedDefinition, sessionServices);
				var parentPK = (Guid)childRow[parentPKColumnName];

				Add(referencedEntitiesThatDontExistInTheDB, parentPK, entity);
			}

			return referencedEntitiesThatDontExistInTheDB;
		}

		List<DataRow> GetRowsWithNoParentRowInDB(IEnumerable<DataRow> rows, IEnumerable<DataRow> parentRows)
		{
			if (!parentRows.Any())
			{
				return rows.ToList(); //All rows have no parent rows.
			}

			var result = new List<DataRow>();
			var rowPKs = new HashSet<Guid>(parentRows.Select(p => p[0]).Cast<Guid>());
			foreach (var row in rows)
			{
				var rowPK = (Guid)row[1];
				if (!rowPKs.Contains(rowPK))
				{
					result.Add(row);
				}
			}

			return result;
		}

		Entity BuildEntity(DataRow row, IEntityDefinition definition, AncillaryImportServices sessionServices)
		{
			var builder = new DataRowEntityBuilder(row, definition, sessionServices);
			return EntityBuilder.Construct(builder);
		}

		void LinkChildEntities(IDictionary<Guid, HashSet<Entity>> pk2ChildEntities, Entity entity)
		{
			if (!pk2ChildEntities.TryGetValue(entity.InternalPK, out HashSet<Entity> entities))
			{
				return;
			}

			var entitySet = entity.ToString();
			var childrenPKs = GroupPKsByEntityDefinition(entity.ChildrenCollection);
			var childrenWithNoPK = entity.ChildrenCollection.Where(e => e.InternalPK == Guid.Empty).ToArray();

			foreach (var child in entities)
			{
				if (!child.Parents.Any(parent => parent.ToString() == entitySet))
				{
					child.Parent = entity;
				}

				var isMatchedByPK =
					child.InternalPK != Guid.Empty &&
					childrenPKs.TryGetValue(child.Definition, out HashSet<Guid> addedChildrenPKs) &&
					addedChildrenPKs.Contains(child.InternalPK);

				if (!isMatchedByPK)
				{
					// Match by content is very slow, so, we do it only if we were unable to match by PK
					var isMatchedByContent = childrenWithNoPK.Contains(child);
					if (!isMatchedByContent)
					{
						entity.ChildrenCollection.Add(child);
					}
				}
			}
		}

		static IDictionary<IEntityDefinition, HashSet<Guid>> GroupPKsByEntityDefinition(EntityCollection entities)
		{
			var res = new Dictionary<IEntityDefinition, HashSet<Guid>>();

			foreach (var entity in entities)
			{
				if (res.TryGetValue(entity.Definition, out HashSet<Guid> pks))
				{
					pks.Add(entity.InternalPK);
				}
				else
				{
					pks = new HashSet<Guid>();
					pks.Add(entity.InternalPK);

					res[entity.Definition] = pks;
				}
			}

			return res;
		}

		void LinkParentEntities(IDictionary<Guid, HashSet<Entity>> pk2ParentEntities, Entity entity)
		{
			if (pk2ParentEntities.TryGetValue(entity.InternalPK, out HashSet<Entity> entities))
			{
				foreach (var parent in entities)
				{
					LinkParentEntity(parent, entity);
				}
			}
		}

		void LinkParentEntity(Entity parentEntity, Entity entity)
		{
			entity.ParentCollection.Add(parentEntity);
		}
	}
}
