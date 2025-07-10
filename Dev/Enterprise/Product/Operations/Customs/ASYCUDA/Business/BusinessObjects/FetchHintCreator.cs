using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business;

public class FetchHintCreator
{
	FetchHintCreator(BusinessObject bizObj, IEnumerable<TableRelationship> relationships)
	{
		topBizObj = bizObj;
		childToParentMap = new Dictionary<Type, Dictionary<Type, SchemaGuidColumn>>();
		DataLevel.Add(bizObj.GetType(), 1);
		BuildRelationshipMapping(bizObj.GetType(), relationships, 2);
	}

	public static FetchHintCreator New(BusinessObject bizObj)
	{
		FetchHintCreator result = null;
		if (bizObj != null && bizObj is AsycudaManifestHeader)
		{
			result = new FetchHintCreator(bizObj, GetAsycudaManifestHeaderChildrenRelationships());
		}
		return result;
	}

	static IEnumerable<TableRelationship> GetAsycudaManifestHeaderChildrenRelationships()
	{
		yield return new TableRelationship(typeof(AsycudaContainer), AsycudaContainerSchema.ACN_AMA_Manifest);
		yield return new TableRelationship(typeof(CusPerson), CusPersonSchema.CPN_ParentID);
		yield return new TableRelationship(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
		yield return new TableRelationship(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
		yield return CreateAsycudaBillRelationship();
	}

	static TableRelationship CreateAsycudaBillRelationship()
	{
		var result = new TableRelationship(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
		result.AddChildren(GetAsycudaBillChildrenRelationships());
		return result;
	}

	static IEnumerable<TableRelationship> GetAsycudaBillChildrenRelationships()
	{
		yield return CreateAsycudaPackRelationship();
		yield return CreateAsycudaPackedItemRelationship();
		yield return new TableRelationship(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
		yield return CreateABLEntryNumRelationship();
		yield return new TableRelationship(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
	}

	static TableRelationship CreateABLEntryNumRelationship()
	{
		var result = new TableRelationship(typeof(ABLEntryNum), CusEntryNumSchema.CE_ParentID);
		result.AddChildren(GetABCEntryNumChildrenRelationships());
		return result;
	}

	static IEnumerable<TableRelationship> GetABCEntryNumChildrenRelationships()
	{
		yield return new TableRelationship(typeof(ABLEntryNumRelatedPacksGenPivot), GenPivotSchema.XX_Relation1ID);
	}

	static TableRelationship CreateAsycudaPackRelationship()
	{
		var result = new TableRelationship(typeof(AsycudaPack), AsycudaPackSchema.APA_ABL_Bill);
		result.AddChildren(GetAsycudaPackChildrenRelationships());
		return result;
	}

	static IEnumerable<TableRelationship> GetAsycudaPackChildrenRelationships()
	{
		yield return new TableRelationship(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
	}

	static TableRelationship CreateAsycudaPackedItemRelationship()
	{
		var result = new TableRelationship(typeof(AsycudaPackedItem), AsycudaPackedItemSchema.API_ABL_Bill);
		result.AddChildren(GetAsycudaPackedItemChildrenRelationships());
		return result;
	}

	static IEnumerable<TableRelationship> GetAsycudaPackedItemChildrenRelationships()
	{
		yield return new TableRelationship(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
		yield return new TableRelationship(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
	}

	public void Create(IDictionary<Type, FetchHintData> fetchDataToCreate)
	{
		if (fetchDataToCreate != null)
		{
			var hasNaturalKeys = fetchDataToCreate.Any(x => x.Value.NaturalKeys.Any());
			if (topBizObj.IsInDatabase || hasNaturalKeys)
			{
				var bizObjsData = new Dictionary<Type, BusinessObject[]>();
				foreach (var fetchData in fetchDataToCreate.OrderBy(x => GetLevel(x.Key)))
				{
					var data = fetchData.Value;
					var bizObjs = AddForeignKeyFetchHints(bizObjsData, fetchData, data);
					AddNaturalKeyFetchHints(bizObjsData, fetchData, data, bizObjs);
				}
			}
		}
	}

	void AddNaturalKeyFetchHints(Dictionary<Type, BusinessObject[]> bizObjsData, KeyValuePair<Type, FetchHintData> fetchData, FetchHintData data, BusinessObject[] bizObjs)
	{
		foreach (var naturalKey in data.NaturalKeys)
		{
			if (ShouldAddFetchHint(fetchData.Key, naturalKey.Column, naturalKey.ChildType))
			{
				if (bizObjs == null)
				{
					bizObjs = GetBizObjsToAddFetchHint(fetchData.Key, bizObjsData);
				}
				if (bizObjs != null)
				{
					Action<ZString> addFetchHint;
					if (naturalKey.ChildType == null)
					{
						addFetchHint = (code) => Factory.AddFetchHint(naturalKey.Column, code);
					}
					else
					{
						addFetchHint = (pk) => Factory.AddFetchHint(naturalKey.ChildType, naturalKey.Column, pk);
					}
					foreach (var bizObj in bizObjs)
					{
						addFetchHint(bizObj[naturalKey.ParentColumn].ToString());
					}
				}
			}
		}
	}

	BusinessObject[] AddForeignKeyFetchHints(Dictionary<Type, BusinessObject[]> bizObjsData, KeyValuePair<Type, FetchHintData> fetchData, FetchHintData data)
	{
		BusinessObject[] bizObjs = null;
		foreach (var foreignKey in data.ForeignKeys)
		{
			if (ShouldAddFetchHint(fetchData.Key, foreignKey.Column, foreignKey.ChildType))
			{
				if (bizObjs == null)
				{
					bizObjs = GetBizObjsToAddFetchHint(fetchData.Key, bizObjsData);
				}
				if (bizObjs != null)
				{
					Action<ZGuid> addFetchHint;
					if (foreignKey.ChildType == null)
					{
						addFetchHint = (pk) => Factory.AddFetchHint(foreignKey.Column, pk);
					}
					else
					{
						addFetchHint = (pk) => Factory.AddFetchHint(foreignKey.ChildType, foreignKey.Column, pk);
					}
					foreach (var bizObj in bizObjs)
					{
						addFetchHint((ZGuid)bizObj[foreignKey.ParentColumn]);
					}
				}
			}
		}

		return bizObjs;
	}

	void BuildRelationshipMapping(Type parentType, IEnumerable<TableRelationship> relationships, int level)
	{
		foreach (var relationship in relationships)
		{
			if (DataLevel.TryGetValue(relationship.ChildType, out var currentLevel))
			{
				DataLevel[relationship.ChildType] = Math.Max(currentLevel, level);
			}
			else
			{
				DataLevel.Add(relationship.ChildType, level);
			}
			AddChildToParentMapping(relationship.ChildType, parentType, relationship.ForeignKey);
			BuildRelationshipMapping(relationship.ChildType, relationship.Children, level + 1);
		}
	}

	void AddChildToParentMapping(Type childType, Type parentType, SchemaGuidColumn column)
	{
		Dictionary<Type, SchemaGuidColumn> parentMaps;
		if (!childToParentMap.TryGetValue(childType, out parentMaps))
		{
			parentMaps = new Dictionary<Type, SchemaGuidColumn>();
			childToParentMap.Add(childType, parentMaps);
		}
		if (!parentMaps.ContainsKey(parentType))
		{
			parentMaps.Add(parentType, column);
		}
	}

	readonly Dictionary<Type, Dictionary<Type, SchemaGuidColumn>> childToParentMap;
	readonly BusinessObject topBizObj;

	BusinessObjectFactory Factory => topBizObj.Factory;

	Dictionary<Type, int> DataLevel => dataLevel ?? (dataLevel = new Dictionary<Type, int>());
	Dictionary<Type, int> dataLevel;

	int GetLevel(Type type)
	{
		DataLevel.TryGetValue(type, out var result);
		return result;
	}

	BusinessObject[] GetBizObjsToAddFetchHintFromParent(Dictionary<Type, BusinessObject[]> bizObjsData, Type parentType, SchemaGuidColumn foreignKeyColumn, Type childType)
	{
		return GetBizObjsToAddFetchHint(parentType, foreignKeyColumn, childType, GetBizObjsToAddFetchHint(parentType, bizObjsData));
	}

	BusinessObject[] GetBizObjsToAddFetchHint(Type parentType, SchemaGuidColumn foreignKeyColumn, Type childType, params BusinessObject[] parents)
	{
		BusinessObject[] result = null;
		if (parents != null)
		{
			if (parents.Length == 0)
			{
				result = Array.Empty<BusinessObject>();
			}
			else
			{
				ShouldAddFetchHint(parentType, foreignKeyColumn, childType);
				var query = new ZQuery(foreignKeyColumn, parents.Select(x => x.PK));
				query.FetchOnlyFromLocalCache = parents.All(x => !x.IsInDatabase);
				result = Factory.Load(childType, query);
			}
		}
		return result;
	}

	BusinessObject[] GetBizObjsToAddFetchHint(Type type, Dictionary<Type, BusinessObject[]> bizObjsData)
	{
		if (!bizObjsData.TryGetValue(type, out var result))
		{
			if (type.IsInstanceOfType(topBizObj))
			{
				result = new[] { topBizObj };
			}
			else
			{
				if (childToParentMap.TryGetValue(type, out var parentMaps))
				{
					List<BusinessObject> list = null;
					foreach (var mapData in parentMaps)
					{
						var bizObjs = GetBizObjsToAddFetchHintFromParent(bizObjsData, mapData.Key, mapData.Value, type);
						if (bizObjs != null)
						{
							list = list ?? new List<BusinessObject>();
							list.AddRange(bizObjs);
						}
					}
					if (list != null)
					{
						result = list.ToArray();
					}
				}
			}
			if (result == null)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Type '{0}' is not supported in FetchHintCreator.GetBizObjsToAddFetchHint()", type.FullName));
			}
			bizObjsData.Add(type, result);
		}
		return result;
	}

	bool ShouldAddFetchHint(Type parentType, SchemaColumn column, Type childType = null)
	{
		var result = false;
		if (!FetchHintAdded.TryGetValue(parentType, out var dictionary))
		{
			dictionary = new Dictionary<SchemaColumn, List<Type>>();
			FetchHintAdded.Add(parentType, dictionary);
		}

		if (!dictionary.TryGetValue(column, out var list))
		{
			list = new List<Type>();
			dictionary.Add(column, list);
		}
		if (!list.Contains(childType))
		{
			list.Add(childType);
			result = true;
		}
		return result;
	}
	Dictionary<Type, Dictionary<SchemaColumn, List<Type>>> FetchHintAdded => fetchHintAdded ?? (fetchHintAdded = new Dictionary<Type, Dictionary<SchemaColumn, List<Type>>>());
	Dictionary<Type, Dictionary<SchemaColumn, List<Type>>> fetchHintAdded;

	class TableRelationship
	{
		public TableRelationship(Type childType, SchemaGuidColumn foreignKey)
		{
			this.childType = childType;
			this.foreignKey = foreignKey;
		}

		public SchemaGuidColumn ForeignKey => foreignKey;
		readonly SchemaGuidColumn foreignKey;

		public Type ChildType => childType;
		readonly Type childType;

		public void AddChildren(IEnumerable<TableRelationship> extraChildren)
		{
			if (extraChildren != null)
			{
				children = children ?? new List<TableRelationship>();
				foreach (var child in extraChildren)
				{
					children.Add(child);
				}
			}
		}

		public IEnumerable<TableRelationship> Children => children ?? Enumerable.Empty<TableRelationship>();
		List<TableRelationship> children;
	}
}
