using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal class DependentActiveBusinessObjectCollectionTracker : IActiveBusinessObjectCollectionTracker, IService
	{
		public bool MatchesInAnyCollectionFilter(BusinessObject dependent)
		{
			foreach (SchemaGuidColumn fkColumn in GetKnownFKColumns(dependent))
			{
				ZGuid possibleMasterPK = (ZGuid)dependent[fkColumn];
				List<IActiveBusinessObjectCollectionIndex> collections;

				Masters.TryGetValue(possibleMasterPK, out collections);
				if (collections != null)
				{
					foreach (IActiveBusinessObjectCollectionIndex collection in collections)
					{
						if (collection.MatchesFilter(dependent, false))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public void NotifyCollectionIndexCreated(IActiveBusinessObjectCollectionIndex dependentCollectionIndex)
		{
			EnsureIsDependentRelationship(dependentCollectionIndex.Relationship);
			ZGuid masterPK = dependentCollectionIndex.Relationship.Master.PK;

			List<IActiveBusinessObjectCollectionIndex> collections;
			Masters.TryGetValue(masterPK, out collections);
			if (collections == null)
			{
				collections = new List<IActiveBusinessObjectCollectionIndex>();
				Masters[masterPK] = collections;
			}
			collections.Add(dependentCollectionIndex);

			AddKnownFKColumn(((DependentRelationship)dependentCollectionIndex.Relationship).FKSchemaColumnInDependent);
		}

		public void NotifyCollectionIndexDisposed(IActiveBusinessObjectCollectionIndex dependentCollectionIndex)
		{
			EnsureIsDependentRelationship(dependentCollectionIndex.Relationship);
			ZGuid masterPK = dependentCollectionIndex.Relationship.Master.PK;

			List<IActiveBusinessObjectCollectionIndex> collections;
			Masters.TryGetValue(masterPK, out collections);
			if (collections != null)
			{
				collections.Remove(dependentCollectionIndex);
				if (collections.Count == 0)
				{
					Masters.Remove(masterPK);
				}
			}
		}

		public IEnumerable<BusinessObject> GetBizosMatchingInAnyCollectionFilter(IEnumerable<BusinessObject> bizos)
		{
			if (bizos != null && bizos.Any())
			{
				var bizosList = bizos.ToList();
				foreach (var fkColumn in GetKnownFKColumns(bizos.First()))
				{
					foreach (var group in bizos.GroupBy(x => (ZGuid)x[fkColumn]))
					{
						var possibleMasterPK = group.Key;
						Masters.TryGetValue(possibleMasterPK, out var collections);
						if (collections != null)
						{
							foreach (var collection in collections)
							{
								var matchingBizos = collection.GetMatchingBusinessObjects(group.ToList());
								foreach (var bizo in matchingBizos)
								{
									yield return bizo;
								}
								bizosList = bizosList.Except(matchingBizos).ToList();
								if (bizosList.Count == 0)
								{
									yield break;
								}
							}
						}
					}
				}
			}
		}

		#region Implementation

		internal Dictionary<ZGuid, List<IActiveBusinessObjectCollectionIndex>> Masters { get; } = new Dictionary<ZGuid, List<IActiveBusinessObjectCollectionIndex>>();
		readonly Dictionary<string, List<SchemaGuidColumn>> knownFKColumns = new Dictionary<string, List<SchemaGuidColumn>>();

		void EnsureIsDependentRelationship(ICollectionRelationship relationship)
		{
			if (!(relationship is DependentRelationship))
			{
				throw new ArgumentException("Only collections with relationship of type " + typeof(DependentRelationship).FullName + " are supported");
			}
		}

		IEnumerable<SchemaGuidColumn> GetKnownFKColumns(BusinessObject dependent)
		{
			List<SchemaGuidColumn> result;
			knownFKColumns.TryGetValue(dependent.TableName, out result);
			return result ?? Enumerable.Empty<SchemaGuidColumn>();
		}

		void AddKnownFKColumn(SchemaGuidColumn fkColumn)
		{
			List<SchemaGuidColumn> knownFKColumnList;
			knownFKColumns.TryGetValue(fkColumn.TableName, out knownFKColumnList);
			if (knownFKColumnList == null)
			{
				knownFKColumnList = new List<SchemaGuidColumn>();
				knownFKColumns.Add(fkColumn.TableName, knownFKColumnList);
			}
			knownFKColumnList.Add(fkColumn);
		}

		#endregion
	}
}
