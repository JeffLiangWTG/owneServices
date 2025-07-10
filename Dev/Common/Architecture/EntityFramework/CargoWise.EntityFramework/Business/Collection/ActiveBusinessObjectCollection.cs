using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public interface IActiveBusinessObjectCollection :
		IBusinessObjectCollection,
		INeedTable,
		INeedDataSet,
		IFindBoxListProvider,
		IFindBoxListProviderEx,
		IFindBoxListProviderDescriptionEx,
		IFilterBusinessObjectDefaultsProvider,
		IBusinessObjectCollectionInternals,
		IBindingListView,
		ISupportMaxCountValidation
	{
		void SetFactory(BusinessObjectFactory factory);
		ICollectionRelationship Relationship { get; set; }
		void SetDefaultsForNewElement(BusinessObject newElement);
		void SetDefaultsAndRelationshipForNewElement(BusinessObject newElement);
		ZQuery AdditionalFilter { get; set; }
		new IEnumerable<BusinessObject> Find(ZQuery filter);
		IComparer SortComparer { get; }

		bool IsRebuildPending { get; }
		event EventHandler Rebuilt;
		event EventHandler CountChanged;

		void DeleteAll();
		void Deactivate();
		IActiveBusinessObjectCollection Clone();
		void Refresh();
		ZString GetAllNotificationsWhenAdditionalFilterNotMet(BusinessObject selectedBusinessObject);
		INotificationType GetNotificationTyoeWhenAdditionalFilterNotMet();
		object Index { get; } // typically only used for testing purposes
	}

	public interface IActiveBusinessObjectCollection<out T> : IActiveBusinessObjectCollection, IEnumerable<T>
	{
		new IEnumerable<T> Find(ZQuery filter);
		new T AddNew();
	}

	public static class ActiveBusinessObjectCollection
	{
		public static IDisposable DelayListChangedEvents(BusinessObjectFactory factory)
		{
			return CollectionListChangedSuspender.GetInstance(factory).DelayListChangedEvents();
		}

		public static IDisposable EnableInvariant()
		{
			invariantEnabledIndex++;
			return new DisposableAction(delegate
			{
				invariantEnabledIndex--;
			});
		}

		internal static bool IsInvariantEnabled
		{
			get { return invariantEnabledIndex != 0; }
		}
		[ThreadStatic]
		static int invariantEnabledIndex;

		public static void RefreshAll(BusinessObjectFactory factory)
		{
			foreach (IActiveBusinessObjectCollectionIndex index in new List<IActiveBusinessObjectCollectionIndex>(ActiveBusinessObjectCollectionIndexCache.GetInstance(factory).All))
			{
				index.Refresh();
			}
		}

		public static void RefreshAll(Type bizoType, BusinessObjectFactory factory)
		{
			foreach (IActiveBusinessObjectCollectionIndex index in new List<IActiveBusinessObjectCollectionIndex>(ActiveBusinessObjectCollectionIndexCache.GetInstance(factory).All))
			{
				if (bizoType.IsAssignableFrom(index.ElementType))
				{
					index.Refresh();
				}
			}
		}

		public static IEnumerable<ActiveCollectionRelationship> FindAllActiveRelationships(Type typeToFilterBy, BusinessObjectFactory factory)
		{
			var indexes = ActiveBusinessObjectCollectionIndexCache.GetInstance(factory).All.ToList();
			foreach (var index in indexes)
			{
				if (typeToFilterBy.IsAssignableFrom(index.ElementType))
				{
					yield return new ActiveCollectionRelationship(index.ElementType, index.Relationship);
				}
			}
		}

		public static bool CollectionsAreRefreshedOnSave { get; internal set; }

#if DEBUG
		public static IDisposable TrackIndexedCollectionCounts_ForTest()
		{
			IndexedCollections = new ConcurrentDictionary<Type, int>();

			return new DisposableAction(() => IndexedCollections = null);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<KeyValuePair<Type, int>> IndexedCollections_ForTest => IndexedCollections;

		internal static ConcurrentDictionary<Type, int> IndexedCollections { get; private set; }
#endif
	}

	/// <summary>
	/// A collection that actively manages it's elements based on a relationship and filter.
	/// The collection operates much like a .net DataView in that it is a live view of data and
	/// dynamically updates when changes are made to a BusinessObject.<br/>
	/// 
	/// An ActiveBusinessObjectCollection operates much like a flyweight and wraps a
	/// ActiveBusinessObjectCollectionIndex which is possibly shared across many collection instances.<br/>
	/// 
	/// You must not maintain object state on a ActiveBusinessObjectCollection otherwise virtual
	/// methods such as MatchesFilter or SetDefaultsOnNewElement may result in unexpected results
	/// when there are multiple collection objects around the same index object.
	/// </summary>
	[FreezeSortOnGridCollectionElementModify(true)]
	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection]
	[System.Diagnostics.DebuggerDisplay("Count = {CountForDebuggerDisplay}")]
	public class ActiveBusinessObjectCollection<T> :
		ZCustomTypeDescriptor,
		IActiveBusinessObjectCollection<T>,
		IBindingTracked,
		ICancelAddNew,
		IEnumerable<T>,
		IList<T>
		where T : BusinessObject
	{
		#region Constructors

		/// <summary>
		/// Creates a collection that returns all BusinessObjects for a particular table.
		/// </summary>
		public ActiveBusinessObjectCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		/// <summary>
		/// Creates a collection that returns all BusinessObjects for a particular filter.
		/// The filter that is passed in is combined with the AdditionalFilter property to produce
		/// the elements in the collection.
		/// </summary>
		public ActiveBusinessObjectCollection(BusinessObjectFactory factory, ZQuery filter)
		{
			Argument.NotNull(factory, "factory");
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.ctor1");
			this.factory = factory;
			CheckNoOrderBy(filter, (NoResString)"filter");
			this.relationshipCreator = delegate
			{ return new CollectionRelationship(ElementType, CreateRelationshipFilter(filter)); };
			UpdateFinalizer();
		}

		/// <summary>
		/// Creates a dependent collection with the given BusinessObject as the master.
		/// </summary>
		public ActiveBusinessObjectCollection(BusinessObject master)
			: this(master.Factory, master)
		{
		}

		/// <summary>
		/// Creates a dependent collection with the given BusinessObject as the master.
		/// The filter that is passed in is combined with the AdditionalFilter property to produce
		/// the elements in the collection.
		/// </summary>
		public ActiveBusinessObjectCollection(BusinessObject master, ZQuery filter)
			: this(master.Factory, master, filter)
		{
		}

		/// <summary>
		/// Creates a dependent collection with the given BusinessObject as the master.
		/// 
		/// If 'master' is null, a collection with no results is created.
		/// </summary>
		public ActiveBusinessObjectCollection(BusinessObjectFactory factory, BusinessObject master)
			: this(factory, master, new ZQuery())
		{
		}

		/// <summary>
		/// Creates a dependent collection with the given BusinessObject as the master.
		/// 
		/// If 'master' is null, a collection with no results is created.
		/// 
		/// The filter that is passed in is combined with the AdditionalFilter property to produce
		/// the elements in the collection.
		/// </summary>
		public ActiveBusinessObjectCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter)
			: this(factory, master, filter, null)
		{
		}

		/// <summary>
		/// Creates a dependent collection with the given BusinessObject as the master.
		/// 
		/// If 'master' is null, a collection with no results is created.
		/// 
		/// The filter that is passed in is combined with the AdditionalFilter property to produce
		/// the elements in the collection.
		/// </summary>
		protected internal ActiveBusinessObjectCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter, SchemaColumn relationshipColumn)
		{
			Argument.NotNull(factory, "factory");
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.ctor2");
			this.factory = factory;
			CheckNoOrderBy(filter, (NoResString)"filter");
			if (master == null)
			{
				relationshipCreator = delegate
				{ return new NoResultRelationship(ElementType); };
			}
			else if (relationshipColumn is SchemaStringColumn)
			{
				relationshipCreator = delegate
				{ return new NkDependentRelationship(master, ElementType, CreateRelationshipFilter(filter), (SchemaStringColumn)relationshipColumn); };
			}
			else
			{
				relationshipCreator = delegate
				{ return new DependentRelationship(master, ElementType, CreateRelationshipFilter(filter), (SchemaGuidColumn)relationshipColumn); };
			}
			UpdateFinalizer();
		}

		/// <summary>
		/// Creates a many to many collection with the given BusinessObject as the master and the given business object type as the pivot object type.
		/// 
		/// The filter that is passed in is combined with the AdditionalFilter property to produce
		/// the elements in the collection.
		/// </summary>
		protected internal ActiveBusinessObjectCollection(BusinessObject master, Type pivotObjectType, ZQuery filter)
			: this(master, pivotObjectType, filter, null, null)
		{
		}

		/// <summary>
		/// Creates a many to many collection with the given BusinessObject as the master and the given business object type as the pivot object type.
		/// </summary>
		protected internal ActiveBusinessObjectCollection(BusinessObject master, Type pivotObjectType)
			: this(master, pivotObjectType, null)
		{
		}

		/// <summary>
		/// Creates a many to many collection with the given BusinessObject as the master and the given business object type as the pivot object type.
		/// 
		/// The filter that is passed in is combined with the AdditionalFilter property to produce
		/// the elements in the collection.
		/// </summary>
		protected internal ActiveBusinessObjectCollection(BusinessObject master, Type pivotObjectType, ZQuery filter, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements)
		{
			Argument.NotNull(master, "master");
			Argument.NotNull(master.Factory, "master.Factory");
			master.Factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.ctor3");

			this.factory = master.Factory;
			CheckNoOrderBy(filter, (NoResString)"filter");
			relationshipCreator = delegate
			{ return new ManyToManyRelationship(master, ElementType, pivotObjectType, CreateRelationshipFilter(filter), pivotTableFKToMaster, pivotTableFKToElements); };
			UpdateFinalizer();
		}

		/// <summary>
		/// Creates a collection with the given relationship.
		/// </summary>
		public ActiveBusinessObjectCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
		{
			Argument.NotNull(factory, "factory");
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.ctor4");

			this.factory = factory;
			this.relationship = relationship;
			UpdateFinalizer();
		}

		bool finalizeSuppressed;
		bool isInDisposableManager;

		internal bool IsFinalizeSuppressed => finalizeSuppressed;
		internal bool IsInDisposableManager => isInDisposableManager;

		[SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		void UpdateFinalizer()
		{
			if (index != null && (readOnlyIndex > 0 || validationIndex > 0))
			{
				if (!isInDisposableManager && Factory.TryGetDisposableManager(out var manager))
				{
					manager.Subscribe(new DeactivateCollectionAction(this));
					GC.SuppressFinalize(this);
					finalizeSuppressed = true;
					isInDisposableManager = true;
				}

				if (finalizeSuppressed && !isInDisposableManager)
				{
					GC.ReRegisterForFinalize(this);
					finalizeSuppressed = false;
				}
			}
			else
			{
				if (!finalizeSuppressed)
				{
					GC.SuppressFinalize(this);
					finalizeSuppressed = true;
				}
			}
		}

		sealed class DeactivateCollectionAction : IDisposable
		{
			public DeactivateCollectionAction(ActiveBusinessObjectCollection<T> collection)
			{
				this.collection = collection;
			}
			readonly ActiveBusinessObjectCollection<T> collection;

			public void Dispose()
			{
				var indexToBeDispose = collection.index;
				if (indexToBeDispose != null)
				{
					indexToBeDispose.DecrementReadOnlyIncludingChildren_NextPossibleOpportunity();
					indexToBeDispose.ResumeValidation_NextPossibleOpportunity();
				}
				collection.Deactivate();
			}
		}

		#endregion

		#region Factory / Relationship

		public BusinessObjectFactory Factory
		{
			get { return factory; }
			set
			{
				if (factory != value)
				{
					value.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.Factory");

					factory = value;
					Index = null;
					if (indexCache != null)
					{
						foreach (var index in indexCache.All.ToArray())
						{
							index.DisposeIfUnused();
						}
					}
					indexCache = null;
					lastChangeNumber = 0;
					var comparerWithCache = comparer as PropertyComparer;
					if (comparerWithCache != null)
					{
						comparerWithCache.cachedValues?.Clear();
					}
					FireListResetEvent();
				}
			}
		}
		BusinessObjectFactory factory;

		public void Deactivate()
		{
			UpdateFinalizer();
			var indexToBeDiscarded = index;

			if (indexToBeDiscarded != null)
			{
				Index = null;
				indexToBeDiscarded.DisposeIfUnused();
			}
		}

		public bool IsDeactivated
		{
			get { return index == null; }
		}

		public ICollectionRelationship Relationship
		{
			get
			{
				if (relationship == null && relationshipCreator != null)
				{
					relationship = relationshipCreator();
				}
				return relationship;
			}
			set
			{
				Argument.NotNull(value, nameof(Relationship));
				relationship = value;
				ResetIndex();
			}
		}

		ZQuery CreateRelationshipFilter(ZQuery additionalRelationshipFilter)
		{
			CheckNoOrderBy(additionalRelationshipFilter, "additionalRelationshipFilter");

			ZQuery result;
			if (additionalRelationshipFilter != null && !additionalRelationshipFilter.Equals(ZQuery.EmptyQuery))
			{
				result = new ZQuery();
				result.AddToFilter(additionalRelationshipFilter);
				result.AddToFilter(CheckNoOrderBy(CreateRelationshipFilter(), "CreateRelationshipFilter"), JoinCondition.And);
			}
			else
			{
				result = CheckNoOrderBy(CreateRelationshipFilter(), "CreateRelationshipFilter");
			}
			return result;
		}

		protected virtual ZQuery CreateRelationshipFilter()
		{
			return new ZQuery();
		}

		#endregion

		#region AddNew / DeleteAll / RemoveFromRelationship

		public T AddNew()
		{
			T result = Index.AddNew();
			OnAdded(result);
			return result;
		}

		/// <summary>
		/// Calls Delete() on all BusinessObjects currently in the collection which causes them
		/// to be deleted and removed from the collection.
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
		public void DeleteAll()
		{
			Index.DeleteAll(this);
		}

		/// <summary>
		/// Removes a BusinessObject from the collection by changing it's dependent foreign key.
		/// An exception is thrown if the relationship is not dependent.
		/// </summary>
		public void RemoveFromRelationship(BusinessObject businessObject)
		{
			Relationship.RemoveFromRelationship(businessObject);

			OnRemoveFromRelationship(businessObject);
		}

		/// <summary>
		/// Calls RemoveFromRelationship() on all BusinessObjects currently in the collection which causes them
		/// to be removed from the collection.
		/// </summary>
		public void RemoveAllFromRelationship()
		{
			using (Index.ConsolidateListResetEvents())
			{
				foreach (T item in ToArray())
				{
					RemoveFromRelationship(item);
				}
			}
		}

		internal void OnAddIntoRelationship(BusinessObject businessObject)
		{
			OnCollectionCountChange(true, businessObject);
			OnAddIntoRelationshipCore(businessObject);
		}

		protected virtual void OnAddIntoRelationshipCore(BusinessObject businessObject)
		{
		}

		internal void OnRemoveFromRelationship(BusinessObject businessObject)
		{
			OnCollectionCountChange(false, businessObject);
		}

		protected void OnCollectionCountChange(bool isAdded, BusinessObject businessObject)
		{
			if (CollectionCountChange != null)
			{
				CollectionCountChange(this, new CollectionCountChangedEventArgs(isAdded, businessObject));
			}
		}

		public event CollectionCountChangedEventHandler CollectionCountChange;

		#endregion

		#region Find / ToArray / Indexer

#if DEBUG

		internal ActiveBusinessObjectCollection<T> GetSubCollectionForTestPurposesOnly(ZQuery filter)
		{
			CheckNoOrderBy(filter, "filter");
			ActiveBusinessObjectCollection<T> result = Clone();
			result.relationship = result.Relationship.AddFilter(filter);
			return result;
		}

#endif

		public IEnumerable<T> Find(ZQuery filter)
		{
			CheckNoOrderBy(filter, (NoResString)"filter");

			foreach (T element in this)
			{
				if (element.MatchesFilter(filter))
				{
					yield return element;
				}
			}
		}

		public List<ZGuid> GetPKs()
		{
			List<ZGuid> result = new List<ZGuid>();

			foreach (BusinessObject bizObj in this)
			{
				result.Add(bizObj.PK);
			}

			return result;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive", Justification = "The ABOC becomes eligible for GC even while a method on the object is still active.")]
		[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
		public T[] ToArray()
		{
			var result = Index.ToArray();
			GC.KeepAlive(this);
			return result;
		}

		public static explicit operator T[]
			(ActiveBusinessObjectCollection<T> collection)
		{
			return collection.ToArray();
		}

		public T this[int i]
		{
			[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
			get => Index[i];
		}

		public delegate bool FindPredicate(T element);

		public IEnumerable<T> Find(FindPredicate predicate)
		{
			foreach (T element in this)
			{
				if (predicate(element))
				{
					yield return element;
				}
			}
		}

		#endregion

		#region RefreshAll / CheckAllInvariants

		public static void RefreshAll(BusinessObjectFactory factory)
		{
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.RefreshAll");

			foreach (ActiveBusinessObjectCollectionIndex<T> index in ActiveBusinessObjectCollectionIndexCache<T>.GetInstance(factory).All)
			{
				index.Refresh();
			}
		}

		/// <summary>
		/// Will Invalidate all the Indexes with the matching collection Type and will NOT trigger ListChanged.
		/// Do NOT use if you need a guarantee for this Collection to be updated in the UI or you track ListChanged.
		/// </summary>
		protected static void InvalidateAll(Type collectionType, BusinessObjectFactory factory)
		{
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.InvalidateAll");

			foreach (ActiveBusinessObjectCollectionIndex<T> index in ActiveBusinessObjectCollectionIndexCache<T>.GetInstance(factory).All)
			{
				if (collectionType.IsAssignableFrom(index.CollectionType))
				{
					index.InvalidateCache();
				}
			}
		}

		public static void CheckAllCollectionInvariants(BusinessObjectFactory factory)
		{
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollection.CheckAllCollectionInvariants");

			foreach (ActiveBusinessObjectCollectionIndex<T> index in ActiveBusinessObjectCollectionIndexCache<T>.GetInstance(factory).All)
			{
				index.CheckInvariant();
			}
		}

		public void RefreshFromDb()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(Index.CompleteFilter);
			query.ReLoadExistingRows = true;
			Factory.Load<T>(query);
		}

		public void UpdateFromDb()
		{
			var indexFilter = Index.CompleteFilter;
			if (!indexFilter.FetchOnlyFromLocalCache)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(indexFilter);
				query.IgnoreDbQueryCache = true;
				query.IsDBOnlyQuery = true;
				Factory.Load<T>(query);
				Index.Refresh();
			}
		}

		public void RefreshBinding()
		{
			FireListResetEvent();
		}

		#endregion

		#region Clone

		public virtual ActiveBusinessObjectCollection<T> Clone()
		{
			ActiveBusinessObjectCollection<T> result = (ActiveBusinessObjectCollection<T>)MemberwiseClone();
			result.listChanged = null;
			result.hasChangesChanged = null;
			result.guidListMapper = null;
			result.index = null;
			result.readOnlyIndex = 0;
			return result;
		}

		#endregion

		#region SetDefaultsForNewElement / SetRelationshipDefaultsForElement

		internal void SetDefaultsForNewElement(T newElement)
		{
			SetDefaultsForNewElementCore(newElement);
		}

		internal void SetDefaultsAndRelationshipForNewElement(T newElement)
		{
			SetRelationshipDefaultsForElementCore(newElement, false);
			using (newElement.SuspendSettingHasChanges())
			{
				SetDefaultsForNewElementCore(newElement);
			}
		}

		protected virtual void SetDefaultsForNewElementCore(T newElement)
		{
		}

		internal void SetRelationshipDefaultsForElement(T newElement)
		{
			SetRelationshipDefaultsForElementCore(newElement, true);
		}

		protected virtual void SetRelationshipDefaultsForElementCore(T newElement, bool throwIfRelationshipNotSupported)
		{
			if (throwIfRelationshipNotSupported || Relationship.SupportsAddToRelationship())
			{
				Relationship.AddToRelationship(newElement);
			}
		}

		internal void OnLoadingIntoCollection(T loadingObject)
		{
			OnLoadingIntoCollectionCore(loadingObject);
		}

		protected virtual void OnLoadingIntoCollectionCore(T loadingObject)
		{
		}

		internal void OnLoadedIntoCollection(T loadedObject)
		{
			OnLoadedIntoCollectionCore(loadedObject);
		}

		protected virtual void OnLoadedIntoCollectionCore(T loadedObject)
		{
		}

		#endregion

		#region MatchesFilter

		/// <summary>
		/// Override MatchesFilterCore to perform custom filtering in C# code of the elements in the collection.
		/// </summary>
		protected internal bool MatchesFilter(T element, bool fetchOnlyFromLocalCache)
		{
			return MatchesFilterCore(element, fetchOnlyFromLocalCache) && MatchesFilterIndex(element, fetchOnlyFromLocalCache);
		}

		internal bool MatchesFilterIndex(BusinessObject element, bool fetchOnlyFromLocalCache)
		{
			return Index.MatchesFilterBaseBehaviour(element, fetchOnlyFromLocalCache);
		}

		internal bool MatchesFilterExcludingBaseBehaviour(T element)
		{
			return MatchesFilterCore(element, false);
		}

		/// <summary>
		/// Override MatchesFilterCore to perform custom filtering in C# code of the elements in the collection.
		/// </summary>
		protected virtual bool MatchesFilterCore(T element, bool fetchOnlyFromLocalCache)
		{
			return true;
		}

		#endregion

		#region IsMatchesFilterOverridden

		[ThreadStatic]
		static bool lastIsMatchesFilterOverridden;
		[ThreadStatic]
		static Type lastTypeForIsMatchesFilterOverridden;
		[Common.Testing.SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, bool?> isMatchesFilterOverriddenCache = new LRUCache<Type, bool?>();

		/// <summary>
		/// Call this method to see if a call to MatchesFilter is really necessary, or if a call
		/// to Index.MatchesFilterBaseBehaviour will surfice. This is used by the index to determine if
		/// a WeakReference really needs to be dereferenced, which can degrade performance.
		/// </summary>
		internal static bool IsMatchesFilterOverridden(Type collectionType)
		{
			bool? result;
			if (collectionType == lastTypeForIsMatchesFilterOverridden)
			{
				result = lastIsMatchesFilterOverridden;
			}
			else
			{
				result = isMatchesFilterOverriddenCache[collectionType];
				if (result == null)
				{
					result = IsMethodTakingATParameterAndABooleanOverridden(collectionType, "MatchesFilterCore");
					isMatchesFilterOverriddenCache.Add(collectionType, result);
				}
				lastTypeForIsMatchesFilterOverridden = collectionType;
				lastIsMatchesFilterOverridden = (bool)result;
			}
			return (bool)result;
		}

		[ThreadStatic]
		static bool lastIsOnLoadedIntoCollectionOverridden;
		[ThreadStatic]
		static Type lastTypeForIsOnLoadedIntoCollectionOverridden;
		[Common.Testing.SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, bool?> isOnLoadedIntoCollectionOverriddenCache = new LRUCache<Type, bool?>();

		/// <summary>
		/// Call this method to see if a call to OnLoadedIntoCollection is necessary. This is used by the index to determine if
		/// a WeakReference actually needs to be dereferenced which can degrade performance.
		/// </summary>
		internal static bool IsOnLoadedIntoCollectionOverridden(Type collectionType)
		{
			bool? result;
			if (collectionType == lastTypeForIsOnLoadedIntoCollectionOverridden)
			{
				result = lastIsOnLoadedIntoCollectionOverridden;
			}
			else
			{
				result = isOnLoadedIntoCollectionOverriddenCache[collectionType];
				if (result == null)
				{
					result = IsMethodTakingOneTParameterOverridden(collectionType, "OnLoadedIntoCollectionCore");
					isOnLoadedIntoCollectionOverriddenCache.Add(collectionType, result);
				}
				lastTypeForIsOnLoadedIntoCollectionOverridden = collectionType;
				lastIsOnLoadedIntoCollectionOverridden = (bool)result;
			}
			return (bool)result;
		}

		static bool IsMethodTakingATParameterAndABooleanOverridden(Type collectionType, string methodName)
		{
			Type baseCollectionType = FindBaseCollectionType(collectionType);
			if (baseCollectionType != null && collectionType != null)
			{
				Type elementType = baseCollectionType.GetGenericArguments()[0];
				MethodInfo method = collectionType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { elementType, typeof(bool) }, null);
				if (method != null && method.DeclaringType != baseCollectionType)
				{
					return true;
				}
			}
			return false;
		}

		static bool IsMethodTakingOneTParameterOverridden(Type collectionType, string methodName)
		{
			Type currentType = collectionType;
			Type baseCollectionType = FindBaseCollectionType(collectionType);
			while (baseCollectionType != null && currentType != null)
			{
				Type elementType = baseCollectionType.GetGenericArguments()[0];
				MethodInfo method = collectionType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { elementType }, null);
				if (method != null && method.DeclaringType != baseCollectionType)
				{
					return true;
				}
				currentType = currentType.BaseType;
			}
			return false;
		}

		static Type FindBaseCollectionType(Type collectionType)
		{
			Type currentType = collectionType;
			while (
				currentType != null &&
				(currentType.IsGenericType ? currentType.GetGenericTypeDefinition() : currentType) != typeof(ActiveBusinessObjectCollection<>))
			{
				currentType = currentType.BaseType;
			}
			return currentType;
		}

		#endregion

		#region MarkAsNeedingValidation / MarkAsNeedingValidationIncludingChildren

		public void MarkAsNeedingValidationIncludingChildren()
		{
			foreach (IBusiness child in this)
			{
				child.MarkAsNeedingValidationIncludingChildren();
			}
		}

		public void MarkAsNeedingValidation()
		{
			foreach (BusinessObject child in this)
			{
				child.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Sorting

		public void ApplySort(string propertyName, ListSortDirection direction)
		{
			PropertyDescriptor property = ZCustomTypeDescriptor.GetProperties(ElementType)[propertyName]
				?? throw new ArgumentException("Property '" + propertyName + "' doesn't exist on ElementType '" + ElementType.FullName + "'");
			ApplySort(GetSortComparerForProperty(property, direction));
		}

		public IComparer SortComparer
		{
			get { return index == null ? null : index.SortComparer; }
		}

		public void ApplySort(IComparer comparer)
		{
			this.comparer = comparer;
			Index = null;
			OnSortChanged(EventArgs.Empty);
			if (comparer is IFreezeSortOnElementModifyComparer)
			{
				Index.InvalidateCache();
			}
			else
			{
				FireListResetEvent();
			}
		}

		public void ApplySort(IComparer<T> comparer)
		{
			ApplySort(new GenericComparerWrapper<T>(comparer));
		}

		public void RemoveSort()
		{
			ApplySort((IComparer)null);
		}

		protected virtual IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			PropertyComparer result;
			string listProviderPropertyName;
			if (guidListMapper != null && guidListMapper.TryGetValue(property.Name, out listProviderPropertyName))
			{
				result = new ZGuidPropertyComparer(property, direction, listProviderPropertyName);
			}
			else if (isTimeSet != null && isTimeSet.Contains(property.Name))
			{
				result = new TimeComparer(property, direction);
			}
			else
			{
				result = new PropertyComparer(property, direction);
			}
			return result;
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			comparer = GetSortComparerForProperty(property, direction);

			RunFetchForSort(property);
			ApplySort(comparer);
		}

		void IBindingListView.ApplySort(ListSortDescriptionCollection sorts)
		{
			comparer = new MultiPropertyComparer(GetSortComparerForProperty, sorts);

			List<PropertyDescriptor> propertyDescriptors = new List<PropertyDescriptor>(sorts.Count);
			foreach (ListSortDescription sortDescription in sorts)
			{
				propertyDescriptors.Add(sortDescription.PropertyDescriptor);
			}
			RunFetchForSort(propertyDescriptors.ToArray());

			ApplySort(comparer);
		}

		bool IBindingList.SupportsSorting
		{
			get { return SupportsSorting; }
		}

		protected virtual bool SupportsSorting
		{
			get { return true; }
		}

		bool IBindingListView.SupportsAdvancedSorting
		{
			get { return true; }
		}

		bool IBindingList.IsSorted
		{
			get { return comparer != null; }
		}

		ListSortDescriptionCollection IBindingListView.SortDescriptions
		{
			get { return SortDescriptions; }
		}

		ListSortDescriptionCollection SortDescriptions
		{
			get { return Index.SortDescriptions; }
		}

		ListSortDirection IBindingList.SortDirection
		{
			get { return (SortDescriptions == null || SortDescriptions.Count == 0) ? ListSortDirection.Ascending : SortDescriptions[0].SortDirection; }
		}

		PropertyDescriptor IBindingList.SortProperty
		{
			get { return (SortDescriptions == null || SortDescriptions.Count == 0) ? null : SortDescriptions[0].PropertyDescriptor; }
		}

		void RunFetchForSort(params PropertyDescriptor[] propertyDescriptors)
		{
			if (Count > 0)
			{
				var tableColumnsArray = BizoPropertiesToTableColumnsCalculator.GetBusinessObjectTableColumns(propertyDescriptors);
				if (tableColumnsArray.Length > 0)
				{
					foreach (T bizo in this)
					{
						bizo.FetchStrategy.FetchForView(tableColumnsArray);
					}
				}
			}
		}

		#endregion

		#region ListChanged event

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		int listChangedSuspended;

		bool IBusinessObjectCollectionInternals.IsListChangedSuspended
		{
			get { return IsListChangedSuspended; }
		}

		bool IsListChangedSuspended
		{
			get { return listChangedSuspended != 0; }
		}

		IDisposable SuppresListChanged()
		{
			listChangedSuspended++;
			return new DisposableAction(delegate
			{ listChangedSuspended--; });
		}

		event ListChangedEventHandler IBindingList.ListChanged
		{
			add { ListChanged += value; }
			remove { ListChanged -= value; }
		}

		event ListChangedEventHandler ListChanged
		{
			add
			{
				if (listChanged == null)
				{
					Index.ListChanged += new ListChangedEventHandler(Index_ListChanged);
				}
				listChanged += value;
			}
			remove
			{
				listChanged -= value;
				if (listChanged == null)
				{
					Index.ListChanged -= new ListChangedEventHandler(Index_ListChanged);
				}
			}
		}
		event ListChangedEventHandler listChanged;

		bool IBindingTracked.IsBound => listChanged.IsBound(doRecursiveCheck: false);

		void Index_ListChanged(object sender, ListChangedEventArgs e)
		{
			OnListChanged(e);
		}

		void OnListChanged(ListChangedEventArgs e)
		{
			if (listChangedSuspended == 0 && listChanged != null)
			{
				listChanged(this, e);
			}
		}

		#endregion

		#region CountChanged event

		public event EventHandler CountChanged
		{
			add
			{
				if (countChanged == null)
				{
					ListChanged += new ListChangedEventHandler(ListChanged_ToMaintainCountChanged);
				}
				countChanged += value;
			}
			remove
			{
				countChanged -= value;
				if (countChanged == null)
				{
					ListChanged -= new ListChangedEventHandler(ListChanged_ToMaintainCountChanged);
				}
			}
		}
		event EventHandler countChanged;

		void ListChanged_ToMaintainCountChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType != ListChangedType.ItemChanged &&
				(Index.IsCachePopulated() || lastKnownCountForCountChanged == null || lastKnownCountForCountChanged != Count))
			{
				countChanged(this, EventArgs.Empty);
			}
			if (IsLoaded)
			{
				lastKnownCountForCountChanged = Count;
			}
		}

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		int? lastKnownCountForCountChanged; // for performance

		#endregion

		#region HasChangesChanged event

		event EventHandler<HasChangesChangedEventArgs> IBusinessObjectState.HasChangesChanged
		{
			add
			{
				if (hasChangesChanged == null)
				{
					Index.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(Index_HasChangesChanged);
				}
				hasChangesChanged += value;
			}
			remove
			{
				hasChangesChanged -= value;
				if (hasChangesChanged == null)
				{
					Index.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(Index_HasChangesChanged);
				}
			}
		}
		event EventHandler<HasChangesChangedEventArgs> hasChangesChanged;

		void Index_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (hasChangesChanged != null)
			{
				hasChangesChanged(this, e);
			}
		}

		#endregion

		#region GetCollectionState

		/// <summary>
		/// Collection of same type (with same sort order and other) with differect states will have different indexes.
		/// Collection with same states will share same index.
		/// </summary>
		protected virtual object[] GetCollectionState()
		{
			return null;
		}

		#endregion

		#region OnAdded

		/// <summary>
		/// OnAdded is called when Add() or AddNew() are explicitly called on this collection instance.
		/// This method override has different semantics to ListChangedType.ItemAdded in that it isn't invoked when an
		/// object is changed such that it meets the criteria of the filter. This is for performance reasons.
		/// Only in exceptional circumstances should you need to override this method.
		/// </summary>
		protected virtual void OnAdded(T businessObject)
		{
		}

		#endregion

		#region RunPreSaveValidation

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		int preSaveValidationIndex;

		void IBusiness.RunPreSaveValidation()
		{
			preSaveValidationIndex++;
			try
			{
				bool validatedSuccessfully = false;
				while (!validatedSuccessfully)
				{
					validatedSuccessfully = RunPreSaveValidationCore();
				}
				MaxCountValidator.Refresh();
			}
			finally
			{
				preSaveValidationIndex--;
			}
			OnNotificationsChanged(new NotificationsChangedEventArgs(this));
		}

		bool IsInRunPreSaveValidation
		{
			get { return preSaveValidationIndex > 0; }
		}

		protected virtual bool RunPreSaveValidationCore()
		{
			bool moveNext;
			IEnumerator<BusinessObject> enumerator = Cast().GetEnumerator();
			do
			{
				try
				{
					moveNext = enumerator.MoveNext();
				}
				catch (InvalidOperationException)
				{
					return false;
				}
				if (moveNext && enumerator.Current != null)
				{
					enumerator.Current.RunPreSaveValidation();
				}
			}
			while (moveNext);
			return true;
		}

		#endregion

		#region IActiveBusinessObjectCollection Members

		void IActiveBusinessObjectCollection.SetDefaultsForNewElement(BusinessObject newElement)
		{
			SetDefaultsForNewElement((T)newElement);
		}

		void IActiveBusinessObjectCollection.SetDefaultsAndRelationshipForNewElement(BusinessObject newElement)
		{
			SetDefaultsAndRelationshipForNewElement((T)newElement);
		}

		IEnumerable<BusinessObject> IActiveBusinessObjectCollection.Find(ZQuery filter)
		{
			foreach (T element in Find(filter))
			{
				yield return element;
			}
		}

		IActiveBusinessObjectCollection IActiveBusinessObjectCollection.Clone()
		{
			return Clone();
		}

		void IActiveBusinessObjectCollection.SetFactory(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		void IActiveBusinessObjectCollection.Refresh()
		{
			Index.Refresh();
		}

		bool IBusinessObjectCollectionInternals.HasChangesFromDatabase()
		{
			var query = new ZQuery();
			query.AddToFilter(Index.CompleteFilter);
			var bizos = new BusinessObjectFactory().Load<T>(query);
			var oldInDbPKs = this.Where(x => x.IsInDatabase).Select(x => x.PK).ToArray(); //forcefully enumerate so that if the collection isn't loaded it is now
			var newInDbPKs = bizos.Select(x => x.PK).ToHashSet();

			//Step 1) If any returned PK isn't in our local data table at all (= brand new in the database), then raise error.
			foreach (var pk in newInDbPKs)
			{
				if (Factory.RowFactory.GetTable(((IBusiness)this).TableName).GetRowIncludingDeleted(pk.ToGuid()) == null)
				{
					return true;
				}
			}

			//Step 2) If any old PK is in the collection but not returned (= deleted in the database), then raise error.
			foreach (var pk in oldInDbPKs)
			{
				if (!newInDbPKs.Contains(pk))
				{
					return true;
				}
			}

			return false;
		}

		bool IActiveBusinessObjectCollection.IsRebuildPending
		{
			get { return !Index.IsCachePopulated(); }
		}

		event EventHandler IActiveBusinessObjectCollection.Rebuilt
		{
			add
			{
				if (rebuilt == null)
				{
					Index.Rebuilt += new EventHandler(Index_Rebuilt);
				}
				rebuilt += value;
			}
			remove
			{
				rebuilt -= value;
				if (rebuilt == null)
				{
					Index.Rebuilt -= new EventHandler(Index_Rebuilt);
				}
			}
		}
		event EventHandler rebuilt;

		void Index_Rebuilt(object sender, EventArgs e)
		{
			OnRebuilt(e);
		}

		void OnRebuilt(EventArgs e)
		{
			if (rebuilt != null)
			{
				rebuilt(this, e);
			}
		}

		ZString IActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(BusinessObject selectedBusinessObject)
		{
			ZString result = "";

			StringCollectionX errors = new StringCollectionX();
			AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (errors.Count == 0)
			{
				errors.Add(GetErrorMessageForNonOptionalRecord(selectedBusinessObject));
			}

			result = string.Join("\r\n", errors.ToArray());

			return result;
		}

		protected virtual string GetErrorMessageForNonOptionalRecord(BusinessObject selectedBusinessObject) =>
			Res.GetString("{D8C44AA5-0C4A-4902-BF4A-BE51D8E57F12}", "This {0} cannot be chosen here. Please choose another {0}.", GetRecordDescription(selectedBusinessObject));
		
		protected string GetRecordDescription(BusinessObject businessObject)
		{
			return businessObject != null ? businessObject.HumanReadableName.ToString() : Res.GetString("record", "record");
		}

		/// <summary>
		/// Add any messages to show when the item is visible in the collection but cannot be chosen, 
		/// based on the AdditionalFilter specified on the collection
		/// </summary>
		/// <param name="errors">Collection of all existing errors</param>
		/// <param name="selectedBusinessObject">The business object that does not match the Additional Filter</param>
		protected virtual void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			if (AddNotificationWhenAdditionalFilterNotMetOverride != null)
			{
				AddNotificationWhenAdditionalFilterNotMetOverride(errors, selectedBusinessObject);
			}
		}

		public AddNotificationWhenAdditionalFilterNotMetOverrideDelegate AddNotificationWhenAdditionalFilterNotMetOverride;
		public delegate void AddNotificationWhenAdditionalFilterNotMetOverrideDelegate(StringCollectionX errors, BusinessObject selectedBusinessObject);

		INotificationType IActiveBusinessObjectCollection.GetNotificationTyoeWhenAdditionalFilterNotMet()
		{
			return GetNotificationTypeWhenAdditionalFilterNotMetOverride == null ? NotificationType.Error : GetNotificationTypeWhenAdditionalFilterNotMetOverride();
		}

		public GetNotificationTypeWhenAdditionalFilterNotMetOverrideDelegate GetNotificationTypeWhenAdditionalFilterNotMetOverride;
		public delegate INotificationType GetNotificationTypeWhenAdditionalFilterNotMetOverrideDelegate();

		object IActiveBusinessObjectCollection.Index
		{
			get { return Index; }
		}

		#endregion

		#region IBusiness Members

		BusinessObjectFactory IBusiness.Factory
		{
			get { return factory; }
		}

		/// <summary>
		/// NOTE: All BusinessObjects retrieved Validation will not be suspended when the list cache is not active.
		/// </summary>
		void IBusiness.SuspendValidation()
		{
			if (validationIndex++ == 0)
			{
				if (index != null)
				{
					index.SuspendValidation();
					UpdateFinalizer();
				}
			}
		}

		void IBusiness.ResumeValidation()
		{
			if (--validationIndex == 0)
			{
				if (index != null)
				{
					index.ResumeValidation();
					UpdateFinalizer();
				}
			}
		}

		bool IBusiness.IsValidationSuspended
		{
			get { return IsValidationSuspended; }
		}

		public bool IgnoreValidationSuspended
		{
			get { return ignoreValidationSuspended; }
			set
			{
				ignoreValidationSuspended = value;
				foreach (IBusiness child in this)
				{
					child.IgnoreValidationSuspended = value;
				}
			}
		}
		bool ignoreValidationSuspended;

		bool IsValidationSuspended
		{
			get { return !IgnoreValidationSuspended && (validationIndex > 0 || Factory.IsValidationSuspended); }
		}

		void IBusiness.Delete()
		{
			DeleteAll();
		}

		string IBusiness.TableName
		{
			get { return BusinessObjectFactory.GetTableNameFromType(Index.ElementType); }
		}

		ZString IBusiness.HumanReadableName
		{
			get { return HumanReadableNameCore; }
		}

		protected virtual ZString HumanReadableNameCore
		{
			get { return ((INeedTable)this).Table == null ? Res.GetString("RecordHumanReadableNameCore", "record") : DataBoundResourceStrings.GetStringForTable(this.ElementType); }
		}

		bool IBusiness.CanContinueWithSave
		{
			get { return true; }
		}

		void IBusiness.RunPreSaveValidationFetch(bool executeHints)
		{
			FetchStrategy.FetchForValidate();
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				foreach (IBusiness element in this)
				{
					element.RunPreSaveValidationFetch(false);
				}
			}
			if (executeHints && Factory != null)
			{
				Factory.ExecuteAllFetchHints();
			}
		}

		IBusinessObjectCollectionFetchStrategy IBusinessObjectCollection.FetchStrategy
		{
			get { return FetchStrategy; }
		}

		IBusinessObjectCollectionFetchStrategy FetchStrategy
		{
			get { return GetFetchStrategy(); }
		}

		protected virtual IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new BusinessObjectCollectionFetchStrategy(this);
		}

		void IBusiness.MarkAsNeedingValidationIncludingChildren()
		{
			foreach (IBusiness child in this)
			{
				child.MarkAsNeedingValidationIncludingChildren();
			}
		}

		void IBusiness.ValidateIfQuickAndImprovesPreSaveValidationPerformance()
		{
		}

		IBusiness[] IBusiness.Children
		{
			get { return (IBusiness[])new ArrayList(this).ToArray(ElementType); }
		}

		void IBusiness.NotifyRegisteredChildEditable()
		{
		}

		bool IBusiness.CanDeleteForDataRefresh => true;

		void IBusiness.DeleteForDataRefresh() => Index.ToArray().OfType<IBusiness>().Where(b => b.CanDeleteForDataRefresh)
			.ToList().ForEach(b => b.DeleteForDataRefresh());

		#endregion

		#region INotificationProvider Members

		IEnumerable<INotification> INotificationProvider.Notifications
		{
			get { return new ZCollectionNotificationProviderHelper(this).Notifications; }
		}

		public bool HasNotifications()
		{
			return new ZCollectionNotificationProviderHelper(this).HasNotifications();
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return new ZCollectionNotificationProviderHelper(this).HasNotifications(type);
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return new ZCollectionNotificationProviderHelper(this).GetHighestSeverityNotificationType();
		}

		#endregion

		#region IBusinessObjectCollection Members

		BusinessObject IBusinessObjectCollection.AddNew()
		{
			return AddNew();
		}

		bool IBusinessObjectCollection.Contains(BusinessObject businessObject)
		{
			return Contains(businessObject as T);
		}

		bool IBusinessObjectCollection.Contains(ZGuid pk)
		{
			return FindByPK(pk) != null;
		}

		void IBusinessObjectCollection.Remove(BusinessObject businessObject)
		{
			throw new NotSupportedException();
		}

		Type IBusinessObjectCollection.TypeOfElements
		{
			get { return ElementType; }
		}

		Type IBusinessObjectCollection.GetTypeOfElementsFromPK(ZGuid pk)
		{
			return ElementType;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive", Justification = "The ABOC becomes eligible for GC even while a method on the object is still active.")]
		[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
		BusinessObject[] IBusinessObjectCollection.ToArray()
		{
			BusinessObject[] result = Index.ToArray();
			GC.KeepAlive(this);
			return result;
		}

		BusinessObject[] IBusinessObjectCollection.Find(ZQuery filter)
		{
			return Find(filter).Cast<BusinessObject>().ToArray();
		}

		ISortable IBusinessObjectCollection.Elements
		{
			get { throw new NotSupportedException(); }
		}

		public bool ReadOnly
		{
			get { return readOnlyIndex > 0; }
		}

		void IBusinessObjectCollection.AddGuidListMapping(string propertyName, string listName)
		{
			if (guidListMapper == null)
			{
				guidListMapper = new Dictionary<string, string>();
			}
			guidListMapper[propertyName] = listName;
		}

		public virtual void AddIsTime(string propertyName)
		{
			if (isTimeSet == null)
			{
				isTimeSet = new HashSet<string>();
			}
			isTimeSet.Add(propertyName);
		}

		bool IBusinessObjectCollection.IsLoaded
		{
			get { return Index.IsLoaded; }
		}

		BusinessObject IBusinessObjectCollection.FindByPK(ZGuid pk)
		{
			return FindByPK(pk);
		}

		[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
		public BusinessObject FindByPK(ZGuid pk)
		{
			return Index.FindByPK(pk);
		}

		void IBusinessObjectCollection.ApplySort(SortInfo sortInfo)
		{
			ApplySort(sortInfo.PropertyName, sortInfo.Direction);
		}

		public ZQuery AdditionalFilter
		{
			get => additionalFilter;
			set
			{
				if (value != additionalFilter)
				{
					if (value == null)
					{
						Argument.NotNull(value, "value");
					}
					CheckNoOrderBy(value, (NoResString)"value");
					additionalFilter = value;
					ResetIndex();
				}
			}
		}

		void ResetIndex()
		{
			additionalFilter.ModificationsEnabled = false;
			Index = null;
			FireListResetEvent();
		}

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		ZQuery additionalFilter = new ZQuery { ModificationsEnabled = false };

		public ZQuery CompleteFilter
		{
			[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
			get { return Index.CompleteFilter; }
		}

		ZQuery IBusinessObjectCollection.RelationshipFilter
		{
			get { return Relationship.RelationshipFilter; }
		}

		IComparer IBusinessObjectCollection.GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException();
		}

		SortInfo IBusinessObjectCollection.SortInformation
		{
			get { return SortDescriptions.Count == 0 ? null : new SortInfo(SortDescriptions[0].PropertyDescriptor.Name, SortDescriptions[0].SortDirection); }
		}

		int IBusinessObjectCollection.IndexOf(IBusiness bizObj, int startIndex, int countToSearchFromStartIndex)
		{
			if (bizObj != null)
			{
				for (int i = startIndex; i < startIndex + countToSearchFromStartIndex; i++)
				{
					if (this[i].PK == bizObj.Identifier)
					{
						return i;
					}
				}
			}

			return -1;
		}

		void IBusinessObjectCollection.Delete(BusinessObject businessObject)
		{
			Delete((T)businessObject);
		}

		event EventHandler IBusinessObjectCollection.SortChanged
		{
			add { SortChanged += value; }
			remove { SortChanged -= value; }
		}
		event EventHandler SortChanged;

		void OnSortChanged(EventArgs e)
		{
			if (SortChanged != null)
			{
				SortChanged(this, e);
			}
		}

		IDisposable IBusinessObjectCollection.SuspendListChanged()
		{
			return ActiveBusinessObjectCollection.DelayListChangedEvents(Factory);
		}

		public virtual IDisposable SuspendAdditionallyForImport()
		{
			return DisposableAction.NoAction;
		}

		PropertyDescriptor IBusinessObjectCollection.ListPropertyDescriptor { get; set; }
		object IBusinessObjectCollection.Parent { get; set; }

		#endregion

		#region ICancelAddNew Members

		void ICancelAddNew.CancelNew(int index)
		{
			CancelNew(index);
		}

		protected virtual void CancelNew(int index)
		{
			if (index >= 0 && index < Index.Count)
			{
				Index.CancelNew(this[index]);
			}
		}

		void ICancelAddNew.EndNew(int index)
		{
			EndNew(index);
		}

		protected virtual void EndNew(int index)
		{
			if (index >= 0 && index < Index.Count)
			{
				Index.EndNew(this[index]);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
		internal protected bool IsNonCommittedElement(T businessObject)
		{
			return Index.IsNonCommittedElement(businessObject);
		}

		#endregion

		#region IBusinessObjectCollectionInternals Members

		bool IBusinessObjectCollectionInternals.HasChangesFromDelete
		{
			get { return Index.HasChangesFromDelete; }
			set
			{
				Index.HasChangesFromDelete = value;
				if (value && Factory != null)
				{
					lastChangeNumber = Factory.GetNextChangeNumber();
				}
			}
		}

		bool IBusinessObjectCollectionInternals.MastersAreInDatabase
		{
			get { return Relationship.Master == null || Relationship.Master.IsInDatabase; }
		}

		bool IBusinessObjectCollectionInternals.MastersAreDeleted
		{
			get { return Relationship.Master != null && Relationship.Master.IsDeleted; }
		}

		void IBusinessObjectCollectionInternals.FireListResetEvent()
		{
			FireListResetEvent();
		}

		void FireListResetEvent()
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		#endregion

		#region IFilterBusinessObjectDefaultsProvider

		public FilterBusinessObjectDefaults FilterBusinessObjectDefaults
		{
			get { return filterBusinessObjectDefaults ?? (filterBusinessObjectDefaults = new FilterBusinessObjectDefaults()); }
		}

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		FilterBusinessObjectDefaults filterBusinessObjectDefaults;

		#endregion

		#region INeedTable Members

		ZDataTable INeedTable.Table
		{
			get
			{
				string tableName = BusinessObjectFactory.GetTableNameFromType(ElementType);
				return (ZDataTable)((INeedDataSet)this).Data.Tables[tableName];
			}
		}

		#endregion

		#region INeedDataSet Members

		DataSet INeedDataSet.Data
		{
			get { return ((INeedDataSet)Factory).Data; }
		}

		#endregion

		#region IFindBoxListProvider Members

		(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int cursor)
		{
			return FindBoxListProvider.NearestMatch(code, explicitAutoComplete, cursor);
		}

		(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
		{
			return FindBoxListProvider.NearestMatchCore(code, explicitAutoComplete);
		}

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			return FindBoxListProvider.DescriptionFromCode(code);
		}

		string IFindBoxListProvider.DescriptionFromPrimaryKey(ZGuid pK)
		{
			return FindBoxListProvider.DescriptionFromPrimaryKey(pK);
		}

		ZGuid IFindBoxListProvider.PrimaryKeyFromCode(string code)
		{
			return FindBoxListProvider.PrimaryKeyFromCode(code);
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCode(string code)
		{
			return FindBoxListProvider.GetBusinessObjectFromCode(code);
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter(string code)
		{
			return FindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter(code);
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCode(string code)
		{
			return FindBoxListProvider.GetBusinessObjectsFromCode(code);
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCodeWithoutFilter(string code)
		{
			return FindBoxListProvider.GetBusinessObjectsFromCodeWithoutFilter(code);
		}

		string IFindBoxListProvider.CodeFromPrimaryKey(ZGuid pK)
		{
			return FindBoxListProvider.CodeFromPrimaryKey(pK);
		}

		IBusinessObjectCollection IFindBoxListProvider.List
		{
			get { return FindBoxListProvider.List; }
		}

		bool IFindBoxListProvider.AutoCompleteOnCommit
		{
			get { return FindBoxListProvider.AutoCompleteOnCommit; }
		}

		ICodeDescription IFindBoxListProvider.GetCustomCodeDescription(BusinessObject bizo)
			=> FindBoxListProvider.GetCustomCodeDescription(bizo);

		protected virtual IFindBoxListProvider FindBoxListProvider
		{
			get { return new FindBoxListProvider(this); }
		}

		#endregion

		#region IFindBoxListProviderEx Members

		IList<AlternateKey> IFindBoxListProviderEx.AlternateKeys
		{
			get
			{
				IFindBoxListProviderEx findBoxListProviderEx = FindBoxListProvider as IFindBoxListProviderEx;
				return findBoxListProviderEx != null ? findBoxListProviderEx.AlternateKeys : Array.Empty<AlternateKey>();
			}
		}

		ZGuid IFindBoxListProviderEx.PrimaryKeyFromAlternateKey(string columnName, IZType value)
		{
			IFindBoxListProviderEx findBoxListProviderEx = FindBoxListProvider as IFindBoxListProviderEx;
			return findBoxListProviderEx != null ? findBoxListProviderEx.PrimaryKeyFromAlternateKey(columnName, value) : ZGuid.Invalid;
		}

		IZType IFindBoxListProviderEx.AlternateKeyFromPrimaryKey(string columnName, ZGuid pk)
		{
			IFindBoxListProviderEx findBoxListProviderEx = FindBoxListProvider as IFindBoxListProviderEx;
			return findBoxListProviderEx != null ? findBoxListProviderEx.AlternateKeyFromPrimaryKey(columnName, pk) : null;
		}

		#endregion

		#region IFindBoxListProviderDescriptionEx

		string IFindBoxListProviderDescriptionEx.NearestDescriptionMatch(string description, bool explicitAutoComplete)
		{
			IFindBoxListProviderDescriptionEx findBoxListProviderDescriptionEx = FindBoxListProvider as IFindBoxListProviderDescriptionEx;
			return findBoxListProviderDescriptionEx != null ? findBoxListProviderDescriptionEx.NearestDescriptionMatch(description, explicitAutoComplete) : null;
		}

		string IFindBoxListProviderDescriptionEx.CodeFromDescription(string description)
		{
			IFindBoxListProviderDescriptionEx findBoxListProviderDescriptionEx = FindBoxListProvider as IFindBoxListProviderDescriptionEx;
			return findBoxListProviderDescriptionEx != null ? findBoxListProviderDescriptionEx.CodeFromDescription(description) : null;
		}

		#endregion

		#region IBusinessObjectState Members

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		int readOnlyIndex;
#if DEBUG
		internal event EventHandler destuctorCalled;
#endif

		~ActiveBusinessObjectCollection()
		{
			if (index != null)
			{
				index.DecrementReadOnlyIncludingChildren_NextPossibleOpportunity();
				index.ResumeValidation_NextPossibleOpportunity();
			}

#if DEBUG
			if (destuctorCalled != null)
			{
				destuctorCalled(null, EventArgs.Empty);
			}
#endif
		}

		/// <summary>
		/// Sets the business object collection and all its children's .ReadOnly properties to the specified value.
		/// Note that read-only means the GUI becomes read-only; property values can still be modified.
		/// </summary>
		public void SetReadOnlyIncludingChildren(bool readOnly)
		{
			if (readOnly)
			{
				IncrementReadOnlyIncludingChildren();
			}
			else
			{
				DecrementReadOnlyIncludingChildren(true);
			}

			SetReadOnlyIncludingChildrenCore(readOnly);
		}

		protected virtual void SetReadOnlyIncludingChildrenCore(bool readOnly) { }

		void IBusinessObjectState.IncrementReadOnlyIncludingChildren()
		{
			IncrementReadOnlyIncludingChildren();
		}

		void IncrementReadOnlyIncludingChildren()
		{
			readOnlyIndex++;
			if (index != null)
			{
				using (SuppresListChanged())
				{
					index.IncrementReadOnlyIncludingChildren();
					UpdateFinalizer();
				}
				if (readOnlyIndex == 1)
				{
					FireListResetEvent();
				}
			}
		}

		void IBusinessObjectState.DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
			DecrementReadOnlyIncludingChildren(decrementToZero);
		}

		void DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
			if (readOnlyIndex > 0)
			{
				readOnlyIndex--;
				using (SuppresListChanged())
				{
					if (index != null)
					{
						index.DecrementReadOnlyIncludingChildren(decrementToZero);
						if (readOnlyIndex == 0)
						{
							UpdateFinalizer();
						}
					}
				}
				if (readOnlyIndex == 0)
				{
					FireListResetEvent();
				}
				if (decrementToZero && readOnlyIndex > 0)
				{
					DecrementReadOnlyIncludingChildren(true);
				}
			}
		}

		bool IBusinessObjectState.HasChanges
		{
			get { return Index.HasChanges; }
			set { SetHasChanges(value); }
		}

		protected virtual void SetHasChanges(bool hasChanges)
		{
			throw new NotSupportedException();
		}

		bool IBusinessObjectState.HasChangesNotIncludingChildren
		{
			get { return Index.HasChangesFromDelete; }
		}

		uint IBusinessObjectState.LastChangeNumber
		{
			get { return lastChangeNumber; }
		}

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		uint lastChangeNumber;

		bool IBusinessObjectState.IsInDatabase => false;

		bool IBusinessObjectState.IsInDatabaseIncludingChildren
		{
			get
			{
				return this.All(element => !element.ShouldCheckIsInDatabase || element.IsInDatabaseIncludingChildren);
			}
		}

		void IBusinessObjectState.RefreshBindingIncludingChildren()
		{
			FireListResetEvent();

			foreach (IBusiness child in this.ToArray())
			{
				child.RefreshBindingIncludingChildren();
			}
		}

		void IBusinessObjectState.ClearHasChangesIncludingChildren()
		{
			foreach (BusinessObject child in this)
			{
				Relationship.ClearHasChangesIncludingRelationship(child);
			}
		}

		event EventHandler IBusinessObjectState.UpdatedByDataRefreshIncludingChildren
		{
			add { }
			remove { }
		}

		event EventHandler<NotificationsChangedEventArgs> IBusinessObjectState.NotificationsChanged
		{
			add
			{
				if (notificationsChanged == null)
				{
					Index.NotificationsChanged += Index_NotificationsChanged;
				}
				notificationsChanged += value;
			}
			remove
			{
				notificationsChanged -= value;
				if (notificationsChanged == null)
				{
					Index.NotificationsChanged -= Index_NotificationsChanged;
				}
			}
		}
		event EventHandler<NotificationsChangedEventArgs> notificationsChanged;

		void Index_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			OnNotificationsChanged(e);
		}

		void OnNotificationsChanged(NotificationsChangedEventArgs e)
		{
			if (notificationsChanged != null && notificationsChangedSuspendedIndex == 0 && !IsValidationSuspended && !IsInRunPreSaveValidation)
			{
				notificationsChanged(this, e);
			}
		}

		protected IDisposable SuspendNotificationsChanged()
		{
			notificationsChangedSuspendedIndex++;
			return new DisposableAction(() => notificationsChangedSuspendedIndex--);
		}

		int notificationsChangedSuspendedIndex;

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			Index.CopyTo(array, index);
		}

		public int Count
		{
			get { return Index.Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return null; }
		}

		#endregion

		#region ICollection<T> Members

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			(this as ICollection).CopyTo(array, arrayIndex);
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		void ICollection<T>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IList Members

		public void AddRange(IEnumerable businessObjects)
		{
			Index.AddRange(businessObjects);
		}

		int IList.Add(object value)
		{
			T bizo = (T)value;
			Add(bizo);
			return IndexOf(bizo);
		}

		void IList.Clear()
		{
			Relationship.Clear();
		}

		bool IList.Contains(object value)
		{
			return Contains((T)value);
		}

		int IList.IndexOf(object value)
		{
			return IndexOf((T)value);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		void IList.Remove(object value)
		{
			Delete((T)value);
		}

		void IList.RemoveAt(int index)
		{
			Delete(this[index]);
		}

		object IList.this[int index]
		{
			get { return Index[index]; }
			set { throw new NotSupportedException(); }
		}

		#endregion

		#region IList<T> Members

		public void Add(T businessObject)
		{
			Index.Add(businessObject);
			OnAdded(businessObject);
		}

		[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
		public bool Contains(T value)
		{
			return Index.Contains(value);
		}

		[MethodImpl(MethodImplOptions.NoInlining)] // If this method is inlined this collection may be unreferenced and collected by the GC. This causes havoc for the index, who holds a weakref to this collection
		public int IndexOf(T value)
		{
			return Index.IndexOf(value);
		}

		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		public virtual void Delete(T businessObject)
		{
			Index.Delete(businessObject);
		}

		bool ICollection<T>.Remove(T item)
		{
			Delete(item);
			return true;
		}

		void IList<T>.RemoveAt(int index)
		{
			Delete(this[index]);
		}

		T IList<T>.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator<T> GetEnumerator()
		{
			return new IndexEnumeratorWrapper(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (BusinessObject businessObject in this)
			{
				yield return businessObject;
			}
		}

		public IEnumerable<BusinessObject> Cast()
		{
			foreach (BusinessObject item in this)
			{
				yield return item;
			}
		}

		internal class IndexEnumeratorWrapper : IEnumerator<T>
		{
			public IndexEnumeratorWrapper(ActiveBusinessObjectCollection<T> collection)
			{
				Collection = collection;
				IndexEnumerator = Collection.Index.GetEnumerator();
			}

			/// <summary>
			/// Reference to collection to prevent it from being GCed.
			/// </summary>
			internal ActiveBusinessObjectCollection<T> Collection { get; private set; }

			IEnumerator<T> IndexEnumerator { get; }

			public void Dispose()
			{
				Collection = null;
				IndexEnumerator.Dispose();
			}

			public bool MoveNext()
			{
				return IndexEnumerator.MoveNext();
			}

			public void Reset()
			{
				IndexEnumerator.Reset();
			}

			public T Current => IndexEnumerator.Current;

			object IEnumerator.Current => Current;
		}

		#endregion

		#region IBindingList Members

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
		}

		object IBindingList.AddNew()
		{
			T result = Index.AddNewUncommitted();
			OnAdded(result);
			return result;
		}

		bool IBindingList.AllowEdit
		{
			get { return !ReadOnly; }
		}

		bool IBindingList.AllowNew
		{
			get { return !ReadOnly && AllowNew; }
		}

		protected virtual bool AllowNew
		{
			get { return true; }
		}

		bool IBindingList.AllowRemove
		{
			get { return !ReadOnly; }
		}

		bool IBindingList.SupportsChangeNotification
		{
			get { return true; }
		}

		bool IBindingList.SupportsSearching
		{
			get { return false; }
		}

		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IBindingListView Members

		bool IBindingListView.SupportsFiltering
		{
			get { return false; }
		}

		string IBindingListView.Filter
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		void IBindingListView.RemoveFilter()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return ZGuid.Empty; }
		}

		#endregion

		#region ISupportMaxCountValidation Members

		BusinessObjectCollectionMaxCountValidator MaxCountValidator
		{
			get { return maxCountValidator ?? (maxCountValidator = new BusinessObjectCollectionMaxCountValidator(this)); }
		}
		BusinessObjectCollectionMaxCountValidator maxCountValidator;

		BusinessObjectCollectionMaxCountValidator ISupportMaxCountValidation.MaxCountValidator
		{
			get { return MaxCountValidator; }
		}

		#endregion

		#region Implementation

		delegate ICollectionRelationship RelationshipCreator();
		ICollectionRelationship relationship;
		readonly RelationshipCreator relationshipCreator;
		IComparer comparer;

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		Dictionary<string, string> guidListMapper;

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		HashSet<string> isTimeSet;

#if DEBUG
		[Testing.SuppressCollectionStateTest]
#endif
		int validationIndex;

		ZQuery CheckNoOrderBy(ZQuery query, string argumentName)
		{
			if (query != null && !query.OrderBy.IsEmpty)
			{
				ErrorReporter.ReportOnce("ZQuery.OrderByNotSupportedOn" + GetType().FullName, "ZQuery.OrderBy is not supported on an " + nameof(ActiveBusinessObjectCollection) + ". Consider using collection.ApplySort() as an alternative.");
			}
			return query;
		}

		// Do not expose in RELEASE unless you're prepared to track down all references and ensure a strong ref to this collection is maintained.
		ActiveBusinessObjectCollectionIndex<T> Index
		{
			get
			{
				if (index == null)
				{
					lock (indexMutex)
					{
						if (index == null)
						{
							Index = IndexCache.GetIndex(GetType(), ElementType, Relationship, additionalFilter, comparer, GetCollectionState());

#if DEBUG
							var indexedCollections = ActiveBusinessObjectCollection.IndexedCollections;

							if (indexedCollections != null)
							{
								var key = GetType();
								indexedCollections.AddOrUpdate(key, 1, (k, v) => ++v);
							}
#endif
						}
					}
				}
				return index;
			}
			set
			{
				if (index != value)
				{
					lock (indexMutex)
					{
						if (index != value)
						{
							try
							{
								if (value != null)
								{ value.isInIndexSet = true; }

								if (index != null)
								{
									index.ListChanged -= Index_ListChanged;
									index.Rebuilt -= Index_Rebuilt;
									index.HasChangesChanged -= Index_HasChangesChanged;
									index.NotificationsChanged -= Index_NotificationsChanged;
									for (int i = 0; i < readOnlyIndex; i++)
									{
										index.DecrementReadOnlyIncludingChildren(false);
									}
									if (validationIndex > 0)
									{
										index.ResumeValidation();
									}
									index.Disconnect(this);
								}

								index = value;
								if (index != null)
								{
									index.AddOwner(this);
									if (listChanged != null)
									{
										index.ListChanged += Index_ListChanged;
									}
									if (rebuilt != null)
									{
										index.Rebuilt += Index_Rebuilt;
									}
									if (hasChangesChanged != null)
									{
										index.HasChangesChanged += Index_HasChangesChanged;
									}
									if (notificationsChanged != null)
									{
										index.NotificationsChanged += Index_NotificationsChanged;
									}
									if (readOnlyIndex > 0)
									{
										using (index.SuspendListChanged())
										{
											for (int i = 0; i < readOnlyIndex; i++)
											{
												index.IncrementReadOnlyIncludingChildren();
											}
										}
									}
									if (validationIndex > 0)
									{
										index.SuspendValidation();
									}
									if (index.IsCachePopulated())
									{
										OnRebuilt(EventArgs.Empty);
									}
								}

								UpdateFinalizer();
							}
							finally
							{
								if (value != null)
								{ value.isInIndexSet = false; }
							}
						}
					}
				}
			}
		}
		ActiveBusinessObjectCollectionIndex<T> index;

		readonly object indexMutex = new object();

		public IDisposable SuspendEnumerationCheck()
		{
			return Index.List.SuspendEnumerationCheck();
		}

#if DEBUG
		internal ActiveBusinessObjectCollectionIndex<T> IndexExposed
		{
			get { return Index; }
		}
#endif

		ActiveBusinessObjectCollectionIndexCache<T> IndexCache
		{
			get { return indexCache ?? (indexCache = ActiveBusinessObjectCollectionIndexCache<T>.GetInstance(Factory)); }
		}
		ActiveBusinessObjectCollectionIndexCache<T> indexCache;

		Type ElementType
		{
			get { return elementType ?? (elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(GetType()) ?? typeof(T)); }
		}
		Type elementType;

		object CountForDebuggerDisplay
		{
			get { return IsLoaded ? Count : (NoResString)"<not loaded>"; }
		}

		bool IsLoaded
		{
			get { return index != null && index.IsLoaded; }
		}

		#endregion
	}
}
