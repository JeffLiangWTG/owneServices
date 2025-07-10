using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal class ManyToManyActiveBusinessObjectCollectionTracker : IActiveBusinessObjectCollectionTracker, IService
	{
		public bool MatchesInAnyCollectionFilter(BusinessObject pivotObject)
		{
			ZString tableName = pivotObject.TableName;
			List<IActiveBusinessObjectCollectionIndex> collections;

			Pivots.TryGetValue(tableName, out collections);
			if (collections != null)
			{
				ZGuid pk = GetFK(pivotObject);
				if (pk.IsValid)
				{
					foreach (IActiveBusinessObjectCollectionIndex collection in collections)
					{
						ManyToManyRelationship relationship = collection.Relationship as ManyToManyRelationship;
						if (relationship != null && relationship.PivotTableFKToMaster != null &&
							relationship.Master != null &&
							(ZGuid)pivotObject[relationship.PivotTableFKToMaster] == relationship.Master.PK)
						{
							return collection.Factory.Load(collection.ElementType, pk) != null;
						}
					}
				}
			}
			return false;
		}

		public void NotifyCollectionIndexCreated(IActiveBusinessObjectCollectionIndex manyToManyCollectionIndex)
		{
			ManyToManyRelationship relationship = manyToManyCollectionIndex.Relationship as ManyToManyRelationship;
			EnsureIsManyToManyRelationship(relationship);
			ZString pivotTableName = relationship.PivotTableName;

			List<IActiveBusinessObjectCollectionIndex> collections;
			Pivots.TryGetValue(pivotTableName, out collections);
			if (collections == null)
			{
				collections = new List<IActiveBusinessObjectCollectionIndex>();
				Pivots[pivotTableName] = collections;
				AddKnownFKColumn(relationship.PivotTableFKToElements);
			}
			collections.Add(manyToManyCollectionIndex);
		}

		public void NotifyCollectionIndexDisposed(IActiveBusinessObjectCollectionIndex manyToManyCollectionIndex)
		{
			ManyToManyRelationship relationship = manyToManyCollectionIndex.Relationship as ManyToManyRelationship;
			EnsureIsManyToManyRelationship(relationship);
			ZString pivotTableName = relationship.PivotTableName;

			List<IActiveBusinessObjectCollectionIndex> collections;
			Pivots.TryGetValue(pivotTableName, out collections);
			if (collections != null)
			{
				collections.Remove(manyToManyCollectionIndex);
				if (collections.Count == 0)
				{
					Pivots.Remove(pivotTableName);
					knownFKColumns.Remove(pivotTableName);
				}
			}
		}

		public IEnumerable<BusinessObject> GetBizosMatchingInAnyCollectionFilter(IEnumerable<BusinessObject> bizos)
		{
			if (bizos != null && bizos.Any())
			{
				var bizosList = bizos.ToList();
				var matchingBizos = new List<BusinessObject>();
				AddFetchHintsForMatchingInAnyCollectionFilter(bizosList);
				foreach (var pivotObject in bizosList)
				{
					var isMatch = MatchesInAnyCollectionFilter(pivotObject);
					if (isMatch)
					{
						matchingBizos.Add(pivotObject);
						yield return pivotObject;
					}
				}
				bizosList = bizosList.Except(matchingBizos).ToList();
				if (bizosList.Count == 0)
				{
					yield break;
				}
			}
		}

		void AddFetchHintsForMatchingInAnyCollectionFilter(IEnumerable<BusinessObject> bizos)
		{
			foreach (var pivotObject in bizos)
			{
				var tableName = pivotObject.TableName;
				Pivots.TryGetValue(tableName, out List<IActiveBusinessObjectCollectionIndex> collections);
				if (collections != null)
				{
					var pk = GetFK(pivotObject);
					if (pk.IsValid)
					{
						foreach (IActiveBusinessObjectCollectionIndex collection in collections)
						{
							var relationship = collection.Relationship as ManyToManyRelationship;
							if (relationship != null && relationship.PivotTableFKToMaster != null &&
								relationship.Master != null &&
								(ZGuid)pivotObject[relationship.PivotTableFKToMaster] == relationship.Master.PK)
							{
								var schema = BusinessObjectFactory.GetTableSchemaFromType(collection.ElementType);
								collection.Factory.AddFetchHint(schema.PK, pk);
							}
						}
					}
				}
			}
		}

		#region Implementation

		internal Dictionary<string, List<IActiveBusinessObjectCollectionIndex>> Pivots { get; } = new Dictionary<string, List<IActiveBusinessObjectCollectionIndex>>();
		readonly Dictionary<string, SchemaGuidColumn> knownFKColumns = new Dictionary<string, SchemaGuidColumn>();

		void EnsureIsManyToManyRelationship(ManyToManyRelationship relationship)
		{
			if (relationship == null)
			{
				throw new ArgumentException("Only collections with relationship of type " + typeof(ManyToManyRelationship).FullName + " are supported");
			}
		}

		ZGuid GetFK(BusinessObject pivotObject)
		{
			ZGuid result = ZGuid.Invalid;
			SchemaGuidColumn knownFK;
			if (knownFKColumns.TryGetValue(pivotObject.TableName, out knownFK))
			{
				result = (ZGuid)pivotObject[knownFK];
			}
			return result;
		}

		void AddKnownFKColumn(SchemaGuidColumn fkColumn)
		{
			if (!knownFKColumns.ContainsKey(fkColumn.TableName))
			{
				knownFKColumns.Add(fkColumn.TableName, fkColumn);
			}
		}

		#endregion
	}
}
