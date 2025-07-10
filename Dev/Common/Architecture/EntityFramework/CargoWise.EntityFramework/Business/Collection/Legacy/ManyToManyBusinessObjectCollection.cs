using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Business;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	#region Generic M-N Collection

	public abstract class ManyToManyBusinessObjectCollection<T, AssociatedObjectT> : ManyToManyBusinessObjectCollection
		where T : BusinessObject
		where AssociatedObjectT : BusinessObject
	{
		protected ManyToManyBusinessObjectCollection(AssociatedObjectT associatedObject)
			: base(associatedObject)
		{
		}

		protected ManyToManyBusinessObjectCollection(AssociatedObjectT associatedObject, ZQuery additionalFilter)
			: base(associatedObject, additionalFilter)
		{
		}

		public T this[int i]
		{
			get { return (T)Elements[i]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public new T AddNew(Type bizOType)
		{
			return (T)base.AddNew(bizOType);
		}
	}

	#endregion

	/// <summary>
	/// This is a collection of objects that are related to AssociatedObject through a M:N pivot table.
	/// </summary>
	public abstract class ManyToManyBusinessObjectCollection : BusinessObjectCollection,
		IBusinessObjectCollectionInternals,
		IBusinessObjectFilterFactory
	{
		#region Pivot Collection

#if DEBUG
		public
#endif
		class PivotCollection : DependentBusinessObjectCollection<BusinessObject, BusinessObject>
		{
			public PivotCollection(BusinessObject master, ManyToManyBusinessObjectCollection manyToManyCollection)
				: base(master, manyToManyCollection.Factory)
			{
				ManyToManyCollection = manyToManyCollection;
				if (manyToManyCollection.Factory.RefreshEnabled)
				{
					IsManagedForDataRefresh = true;
				}
			}

			protected override ZQuery CreateRelationshipFilter()
			{
				return ManyToManyCollection.RelationshipBusinessObjectsFilter;
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pk)
			{
				return ManyToManyCollection.TypeOfRelationshipBusinessObject;
			}

			protected internal override void SetCollectionRelationships(BusinessObject child)
			{
				// do nothing, M:N will handle this stuff
			}

			protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
			{
				// do nothing, M:N will handle this stuff
			}

			protected internal override SchemaGuidColumn FKSchemaColumnInDependent
			{
				get { return ManyToManyCollection.PivotTableFKToAssociatedBusinessObject; }
			}

			protected internal void OnRemovalFromOtherCollection(ZGuid pivotFK)
			{
				ManyToManyCollection.RemoveFromThisCollectionWhenRelationshipBizORemovedByOtherCollection(pivotFK);
			}

			internal override IEnumerable<IGrouping<DataTable, ZGuid>> CopyIntoNewDataTable(IGrouping<DataTable, BusinessObject> group)
			{
				if (group.Any())
				{
					var factory = group.First().Factory;
					var otherTable = factory.RowFactory.GetTable(BusinessObjectFactory.GetTableNameFromType(ManyToManyCollection.TypeOfElements));
					var bizosGroup = new Group<DataTable, BusinessObject>(otherTable, group.Select(GetBizo).WhereNotNull().ToArray());
					return base.CopyIntoNewDataTable(group).Concat(base.CopyIntoNewDataTable(bizosGroup));
				}
				else
				{
					return Enumerable.Empty<IGrouping<DataTable, ZGuid>>();
				}
			}

			internal override int AddFromDataRefresh(IEnumerable<BusinessObject> pivots)
			{
				return base.AddFromDataRefresh(pivots)
				+ ManyToManyCollection.AddFromDataRefresh(pivots.Select(GetBizo).WhereNotNull());
			}

			BusinessObject GetBizo(BusinessObject pivot)
			{
				return pivot.Factory.Load(ManyToManyCollection.TypeOfElements, (ZGuid)pivot[ManyToManyCollection.PivotTableFKToCollectionBusinessObjects]);
			}

			readonly ManyToManyBusinessObjectCollection ManyToManyCollection;
		}

		#endregion

		#region Initialise

		/// <summary>
		/// Creates Collection of BusinessObjects (eg, Consols) for an AssociatedObject (eg, Shipment) through a
		/// M:N pivot table (eg, JobConShipLink).
		/// </summary>
		protected ManyToManyBusinessObjectCollection(BusinessObject associatedObject)
			: base(associatedObject.Factory)
		{
			Initialise(associatedObject);
		}

		/// <summary>
		/// Creates Collection of BusinessObjects (eg, Consols) for an AssociatedObject (eg, Shipment) through a
		/// M:N pivot table (eg, JobConShipLink).
		/// Only objects that meet AdditionalFilter will be included in the collection.
		/// </summary>
		protected ManyToManyBusinessObjectCollection(BusinessObject associatedObject, ZQuery additionalFilter)
			: base(associatedObject.Factory, additionalFilter)
		{
			Initialise(associatedObject);
		}

		void Initialise(BusinessObject associatedObject)
		{
			if (associatedObject is null)
			{
				ErrorReporter.ReportOnce("NullAssociatedObject" + GetType().FullName, "Passing null AssociatedObject into constructor of: " + GetType().FullName);
			}

			fAssociatedObject = associatedObject;

			// The following collection is used to handle add and remove due to a DataRefreshBus publish
			fRelationshipBODependentCollection = new PivotCollection(associatedObject, this);
			associatedObject.RegisterEditableChildObject(fRelationshipBODependentCollection);
			fRelationshipBODependentCollection.ElementAdded += new ElementChangedHandler(AddToThisCollectionWhenRelationshipBizOAddedByDataRefreshBus);
			fRelationshipBODependentCollection.ElementRemoving += new ElementChangedHandler(RemoveFromThisCollectionWhenRelationshipBizORemovedByDataRefreshBus);

			if (fAssociatedObject.IsInDatabase)
			{
				InitialiseFetchHints();
			}
		}

		protected virtual void InitialiseFetchHints()
		{
			Factory.AddFetchHint(TypeOfRelationshipBusinessObject, PivotTableFKToAssociatedBusinessObject, fAssociatedObject.PK);
		}

		protected BusinessObject fAssociatedObject;

		#endregion

		#region PK/FK/Table names

		protected string NameOfPivotTable
		{
			get
			{
				if (fNameOfPivotTable == null)
				{
					fNameOfPivotTable = BusinessObjectFactory.GetTableNameFromType(TypeOfRelationshipBusinessObject);
				}
				return fNameOfPivotTable;
			}
		}

		SchemaGuidColumn PKSchemaColumnInCollectionBusinessObjects
		{
			get { return ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(((IBusiness)this).TableName); }
		}

		string PKNameInAssociatedBusinessObject
		{
			get { return fAssociatedObject.PKSchemaColumn.Name; }
		}

		protected virtual string PivotTablePrefix
		{
			get { return ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(NameOfPivotTable); }
		}

		protected virtual SchemaGuidColumn PivotTableFKToAssociatedBusinessObject
		{
			get
			{
				string associatedTablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(PKNameInAssociatedBusinessObject);
				string columnName = PivotTablePrefix + "_" + associatedTablePrefix; // eg, JN_JS

				return (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(columnName, NameOfPivotTable);
			}
		}

		protected virtual SchemaGuidColumn PivotTableFKToCollectionBusinessObjects
		{
			get
			{
				string collectionTablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(PKSchemaColumnInCollectionBusinessObjects.Name);
				string columnName = PivotTablePrefix + "_" + collectionTablePrefix; // eg, JN_JS

				return (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(columnName, NameOfPivotTable);
			}
		}

		string fNameOfPivotTable;

		#endregion

		#region Relationship Business Object Management

		/// <summary>
		/// Gets the BusinessObject (eg, ConsolShipmentLink) that provides the relationship between the Collection's
		/// AssociatedObject (eg, Consol) and the passed in CollectionElement (eg, Shipment).
		/// </summary>
		/// <returns></returns>
		public BusinessObject GetRelationshipBusinessObject(BusinessObject collectionElement)
		{
			return (BusinessObject)RelationshipBOHashByCollectionElementPK[collectionElement.PK];
		}

		readonly Hashtable RelationshipBOHashByCollectionElementPK = new Hashtable();

		/// <summary>
		/// Return the type of BusinessObject for the pivot table.
		/// </summary>
		protected abstract Type TypeOfRelationshipBusinessObject
		{
			get;
		}

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			bool result = true;

			if (IsLoading || IsUpdatingByDataRefreshBus)
			{
				BusinessObject relationshipBusinessObject = (BusinessObject)RelationshipBOHashByCollectionElementPK[bizO.PK];
				if (relationshipBusinessObject == null)
				{
					result = (LoadRelationshipBusinessObjectFor(bizO) != null);
				}
			}

			return result;
		}

		void SetupRelationshipBusinessObjectFor(BusinessObject collectionElement)
		{
			BusinessObject relationshipBusinessObject = (BusinessObject)RelationshipBOHashByCollectionElementPK[collectionElement.PK];

			if (relationshipBusinessObject == null)
			{
				relationshipBusinessObject = LoadRelationshipBusinessObjectFor(collectionElement);

				if (relationshipBusinessObject == null && !IsLoading)
				{
					relationshipBusinessObject = CreateNewRelationshipBusinessObjectFor(collectionElement);
				}

				if (relationshipBusinessObject != null)
				{
					RelationshipBOHashByCollectionElementPK[collectionElement.PK] = relationshipBusinessObject;

					if (!((IBusinessObjectInternals)relationshipBusinessObject).IsUnCommittedRow)
					{
						fRelationshipBODependentCollection.Add(relationshipBusinessObject);
					}
				}
			}
		}

		PivotCollection fRelationshipBODependentCollection;

		void AddToThisCollectionWhenRelationshipBizOAddedByDataRefreshBus(BusinessObject elementChanged)
		{
			if (!fRelationshipBODependentCollection.IsLoading && fRelationshipBODependentCollection.IsRefreshingByDataRefreshBus)
			{
				ZGuid pKToLoad = (ZGuid)elementChanged[PivotTableFKToCollectionBusinessObjects.Name];
				AddFromDatabaseIfMatchLastLoadedFilter(pKToLoad);
			}
		}

		void RemoveFromThisCollectionWhenRelationshipBizORemovedByDataRefreshBus(BusinessObject elementChanged)
		{
			if (!fRelationshipBODependentCollection.IsLoading && elementChanged.IsRefreshingByDataRefreshBus)
			{
				bool oldHasChanges = HasChanges;

				bool isUpdatingByDataRefreshBusState = IsUpdatingByDataRefreshBus;
				try
				{
					IsUpdatingByDataRefreshBus = true;
					Remove((ZGuid)elementChanged[PivotTableFKToCollectionBusinessObjects.Name]);
				}
				finally
				{
					IsUpdatingByDataRefreshBus = isUpdatingByDataRefreshBusState;
				}

				if (HasChanges != oldHasChanges)
				{
					HasChanges = oldHasChanges;
				}
			}
		}

		void RemoveFromThisCollectionWhenRelationshipBizORemovedByOtherCollection(ZGuid pivotFK)
		{
			if (!fRelationshipBODependentCollection.IsLoading)
			{
				Remove(pivotFK);
			}
		}

		protected virtual ZQuery GetAssociatedBusinessObjectZQuery() => new ZQuery(PivotTableFKToAssociatedBusinessObject, fAssociatedObject.PK);

		BusinessObject LoadRelationshipBusinessObjectFor(BusinessObject collectionElement)
		{
			if (IsLoading || UseQuickLoadRelationshipBusinessObjectFor)
			{
				return QuickLoadRelationshipBusinessObjectFor(collectionElement);
			}

			ZQuery filter1 = GetAssociatedBusinessObjectZQuery();
			ZQuery filter2 = new ZQuery(PivotTableFKToCollectionBusinessObjects, collectionElement.PK);
			ZQuery fullFilter = new ZQuery();
			fullFilter.AddToFilter(filter1);
			fullFilter.AddToFilter(filter2);

			if (!collectionElement.IsInDatabase || !fAssociatedObject.IsInDatabase)
			{
				filter1.FetchOnlyFromLocalCache = true;
				if (Factory.LoadTop1(TypeOfRelationshipBusinessObject, filter1) == null)
				{
					return null;
				}
				filter2.FetchOnlyFromLocalCache = true;
				if (Factory.LoadTop1(TypeOfRelationshipBusinessObject, filter2) == null)
				{
					return null;
				}
				fullFilter.FetchOnlyFromLocalCache = true;
			}

			return Factory.LoadTop1(TypeOfRelationshipBusinessObject, fullFilter);
		}

		protected bool UseQuickLoadRelationshipBusinessObjectFor { get; set; }

		#region QuickLoadRelationshipBusinessObjectFor

		BusinessObject QuickLoadRelationshipBusinessObjectFor(BusinessObject collectionElement)
		{
			return AllRelationshipBusinessObjects.ContainsKey(collectionElement.PK)
				? AllRelationshipBusinessObjects[collectionElement.PK]
				: null;
		}

		Dictionary<ZGuid, BusinessObject> AllRelationshipBusinessObjects
		{
			get
			{
				if (allRelationshipBusinessObjects == null)
				{
					allRelationshipBusinessObjects = new Dictionary<ZGuid, BusinessObject>();
					foreach (BusinessObject bizo in Factory.Load(TypeOfRelationshipBusinessObject, RelationshipBusinessObjectsFilter))
					{
						allRelationshipBusinessObjects[(ZGuid)bizo[PivotTableFKToCollectionBusinessObjects]] = bizo;
					}
				}
				return allRelationshipBusinessObjects;
			}
		}
		Dictionary<ZGuid, BusinessObject> allRelationshipBusinessObjects;

		#endregion

		BusinessObject CreateNewRelationshipBusinessObjectFor(BusinessObject collectionElement)
		{
			BusinessObject result = null;
			if (!collectionElement.IsDataRowDeleted)
			{
				if (IsNonCommittedCollectionElement(collectionElement))
				{
					result = fRelationshipBODependentCollection.CreateNewBusinessObject();
				}
				else
				{
					result = Factory.New(TypeOfRelationshipBusinessObject);
				}

				using (collectionElement.IsSettingHasChangesSuspended ? result.SuspendSettingHasChanges() : null)
				{
					result[PivotTableFKToCollectionBusinessObjects.Name] = collectionElement.PK;
					result[PivotTableFKToAssociatedBusinessObject.Name] = fAssociatedObject.PK;
				}
				collectionElement.RegisterEditableChildObject(result);
			}
			return result;
		}

		void DestroyRelationshipBusinessObjectFor(BusinessObject collectionElement)
		{
			BusinessObject relationshipBizO = GetRelationshipBusinessObject(collectionElement);

			if (relationshipBizO != null)
			{
				RelationshipBOHashByCollectionElementPK.Remove(collectionElement.PK);
				collectionElement.UnRegisterEditableChildObject(relationshipBizO);
				if (fRelationshipBODependentCollection.Contains(relationshipBizO))
				{
					fRelationshipBODependentCollection.RemoveAndDelete(relationshipBizO);
				}
				else if (((IBusinessObjectInternals)relationshipBizO).IsUnCommittedRow && !relationshipBizO.IsDeleted && !relationshipBizO.IsDeleting)
				{
					relationshipBizO.Delete();
				}
				if (!IsUpdatingByDataRefreshBus && (((IBusiness)collectionElement).HasChangesNotIncludingChildren || collectionElement.IsInDatabase))
				{
					((IBusinessObjectCollectionInternals)this).HasChangesFromDelete = true;
					((IBusinessObjectCollectionInternals)fRelationshipBODependentCollection).HasChangesFromDelete = true;
				}
			}
		}

		#endregion

		#region BusinessObjectCollection Overrides

		protected override void AddRowToDataTableIfDetached(BusinessObject bizo)
		{
			base.AddRowToDataTableIfDetached(bizo);

			var pk = bizo.PK;
			if (pk.IsValid && RelationshipBOHashByCollectionElementPK.ContainsKey(pk))
			{
				var relationshipBizo = (BusinessObject)RelationshipBOHashByCollectionElementPK[pk];
				if (relationshipBizo != null && ((IBusinessObjectInternals)relationshipBizo).IsUnCommittedRow)
				{
					fRelationshipBODependentCollection.Add(relationshipBizo);
				}
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			if (fAssociatedObject == null)
			{
				throw new NotSupportedException("Cannot add new BusinessObjects to a ManyToManyBusinessObjectCollection when it's associated to a NullBusinessObject.");
			}
			base.OnAdded(bizOAdded);
		}

		protected override bool HasChangesCore
		{
			get
			{
				return base.HasChangesCore || fRelationshipBODependentCollection.HasChanges;
			}
			set
			{
				base.HasChangesCore = value;
				fRelationshipBODependentCollection.HasChanges = value;
			}
		}

		public override void Load(ZQuery filter)
		{
			if (RelationshipFilter.IsEmpty)
			{
				IsLoaded = true;
				RemoveAllButLeaveRelationshipsIntact();
			}
			else
			{
				allRelationshipBusinessObjects = null;
				try
				{
					base.Load(filter);
				}
				finally
				{
					allRelationshipBusinessObjects = null;
				}
			}
		}

		internal override bool AddFromDatabaseIfMatchLastLoadedFilter(IEnumerable<ZGuid> pKs)
		{
			bool result = false;

			if (fAssociatedObject != null && !fAssociatedObject.IsDeleted)
			{
				if (!RelationshipFilter.IsEmpty)
				{
					result = base.AddFromDatabaseIfMatchLastLoadedFilter(pKs);
				}
			}

			return result;
		}

		protected override DataRefreshMatcher MakeDataRefreshMatcher(ZQuery filter)
		{
			var relationshipFilter = NewBusinessObjectFilter();

			return new DataRefreshMatcher(new ZQuery(), obj =>
			{
				if (obj is BusinessObject bizo)
				{
					var type = bizo.GetType();
					if (TypeOfRelationshipBusinessObject.IsAssignableFrom(type))
					{
						return relationshipFilter.IsMatching(bizo);
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			});
		}

		#region Filters

		protected override ZQuery CreateRelationshipFilter()
		{
			BusinessObject[] allRelationshipBizOs = Factory.Load(TypeOfRelationshipBusinessObject, RelationshipBusinessObjectsFilter);
			fRelationshipBODependentCollection.Load();
			return BuildFilterForAllCollectionElements(allRelationshipBizOs);
		}

		protected internal virtual ZQuery RelationshipBusinessObjectsFilter
		{
			get
			{
				ZQuery result = GetAssociatedBusinessObjectZQuery();
				if (!fAssociatedObject.IsInDatabase)
				{
					result.FetchOnlyFromLocalCache = true;
				}
				return result;
			}
		}

		ZQuery BuildFilterForAllCollectionElements(BusinessObject[] pivotsToAssociatedObject)
		{
			ZQuery result = new ZQuery();

			ArrayList fKList = new ArrayList();
			foreach (BusinessObject pivot in pivotsToAssociatedObject)
			{
				fKList.Add(pivot[PivotTableFKToCollectionBusinessObjects.Name]);
			}

			result.AddToFilter(PKSchemaColumnInCollectionBusinessObjects, SQLComparisonOperator.Equal, fKList);

			if (pivotsToAssociatedObject.Length > 0)
			{
				result.MaximumRows = pivotsToAssociatedObject.Length;
			}

			return result;
		}

		#endregion

		protected override void NotifyRegisteredChildEditable()
		{
			base.NotifyRegisteredChildEditable();
			IsManagedForDataRefresh = true;
		}

		protected internal override void SetCollectionRelationships(BusinessObject child)
		{
			SetupRelationshipBusinessObjectFor(child);
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			DestroyRelationshipBusinessObjectFor(child);
		}

		/// <summary>
		/// This function is called from the button grid. If you want to show the relationship, on the
		/// popped up form, then use IRelationshipAdderForController on your findbox list.
		/// </summary>
		public override void SetupNewElementButDoNotAddIt(BusinessObject @new, bool setupCollectionRelationships)
		{
			SetDefaultsForNewChild(@new);
			@new.HasChanges = false;
		}

		public override void Remove(BusinessObject businessObject)
		{
			using (var deleter = new DeleteFromOtherPivotCollectionsDisposable(GetRelationshipBusinessObject(businessObject), PivotTableFKToCollectionBusinessObjects.Name))
			{
				RunDeleteCheckersForRelationshipBusinessObject(businessObject);
				base.Remove(businessObject);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			using (var deleter = new DeleteFromOtherPivotCollectionsDisposable(GetRelationshipBusinessObject(elementToDelete), PivotTableFKToCollectionBusinessObjects.Name))
			{
				RunDeleteCheckersForRelationshipBusinessObject(elementToDelete);
				base.RemoveAndDelete(elementToDelete);
			}
		}

		class DeleteFromOtherPivotCollectionsDisposable : IDisposable
		{
			public DeleteFromOtherPivotCollectionsDisposable(BusinessObject pivot, string fkName)
			{
				if (pivot != null && !pivot.IsDeleted)
				{
					this.pivot = pivot;
					pivotFK = (ZGuid)pivot[fkName];
					pivotCollections = new List<PivotCollection>();
					foreach (var collection in pivot.ParentCollections.OfType<PivotCollection>())
					{
						pivotCollections.Add(collection);
					}
				}
			}

			readonly BusinessObject pivot;
			readonly ZGuid pivotFK;
			readonly List<PivotCollection> pivotCollections;

			void IDisposable.Dispose()
			{
				if (pivot != null && pivot.IsDeleted)
				{
					foreach (var collection in pivotCollections)
					{
						collection.OnRemovalFromOtherCollection(pivotFK);
					}
				}
			}
		}

		void RunDeleteCheckersForRelationshipBusinessObject(BusinessObject elementToDelete)
		{
			BusinessObject relationshipBizO = GetRelationshipBusinessObject(elementToDelete);
			relationshipBizO?.RunDeleteCheckers();
		}

		#region Masters Are In Database

		bool IBusinessObjectCollectionInternals.MastersAreInDatabase
		{
			get { return fAssociatedObject.IsInDatabase; }
		}

		#endregion

		#region Masters Are Deleted

		bool IBusinessObjectCollectionInternals.MastersAreDeleted
		{
			get
			{
				return fAssociatedObject.IsDeleted;
			}
		}

		#endregion

		#endregion

		#region IBusinessObjectFilterFactory Members

		class ManyToManyBusinessObjectFilter : BusinessObjectFilter
		{
			readonly bool isLoaded;
			readonly bool relationshipFilterIsEmpty;
			readonly ZQuery matchingFilter;

			public ManyToManyBusinessObjectFilter(bool isLoaded, bool relationshipFilterIsEmpty, ZQuery matchingFilter)
			{
				this.isLoaded = isLoaded;
				this.relationshipFilterIsEmpty = relationshipFilterIsEmpty;
				this.matchingFilter = matchingFilter;
			}

			protected override bool IsMatching(BusinessObject bizObj)
			{
				bool result = false;
				if (isLoaded && !relationshipFilterIsEmpty)
				{
					BusinessObjectFactory factoryOfPublishedObject = bizObj.Factory;
					if (factoryOfPublishedObject != null)
					{
						ZQuery filter = new ZQuery(bizObj.PKSchemaColumn, bizObj.PK);
						filter.AddToFilter(matchingFilter, JoinCondition.And);
						filter.FetchOnlyFromLocalCache = true;
						result = factoryOfPublishedObject.LoadTop1(bizObj.GetType(), filter) != null;
					}
				}
				return result;
			}
		}

		public IBusinessObjectFilter NewBusinessObjectFilter()
		{
			return new ManyToManyBusinessObjectFilter(IsLoaded, RelationshipFilter.IsEmpty, MatchingFilter);
		}

		ZQuery MatchingFilter
		{
			get
			{
				var filter = new ZQuery();
				filter.AddToFilter(RelationshipFilter, JoinCondition.And);
				filter.AddToFilter(LastLoadedAdditionalFilter, JoinCondition.And);
				return filter;
			}
		}

		#endregion
	}
}
