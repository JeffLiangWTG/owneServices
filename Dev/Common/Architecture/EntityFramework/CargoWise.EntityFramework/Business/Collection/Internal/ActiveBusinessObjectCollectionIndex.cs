using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	internal interface IActiveBusinessObjectCollectionIndex : IDisposable
	{
		internal Guid ID { get; }
		BusinessObjectFactory Factory { get; }
		Type ElementType { get; }
		ICollectionRelationship Relationship { get; }
		bool MatchesFilter(BusinessObject businessObject, bool fetchOnlyFromLocalCache);
		void Refresh();
		void DeactivateAllOwners();
		IEnumerable<BusinessObject> GetMatchingBusinessObjects(IEnumerable<BusinessObject> bizos);
		internal void OnFactorySaved(bool savedSuccessfully);
	}

	[Serializable]
	public class CannotAddNewRowToTableException : Exception
	{
		public CannotAddNewRowToTableException(string message, BusinessObject newBusinessObject, Exception innerException)
			: base(message, innerException)
		{
			NewBusinessObject = newBusinessObject;
		}

		public BusinessObject NewBusinessObject { get; private set; }

#if NETFRAMEWORK
		public CannotAddNewRowToTableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}

	/// <summary>
	/// An index of BusinessObjects. It encapsulates one particular filter, sort, and relationship configuration.
	/// An index wraps a .net DataView.
	/// </summary>
	internal abstract class ActiveBusinessObjectCollectionIndex<T> : IActiveBusinessObjectCollectionIndex,
		IBindingTracked
		where T : BusinessObject
	{
		readonly Guid id;

		public ActiveBusinessObjectCollectionIndex(BusinessObjectFactory factory, Type collectionType, Type elementType, ICollectionRelationship relationship, ZQuery additionalFilter, IComparer sortComparer, object[] collectionState,
			ActiveBusinessObjectCollectionIndexCache<T>.CacheKey cacheKey)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(collectionType, "collectionType");
			Argument.NotNull(elementType, "elementType");
			Argument.NotNull(relationship, "relationship");
			Argument.NotNull(additionalFilter, "additionalFilter");

			this.Factory = factory;
			this.CollectionType = collectionType;
			this.ElementType = elementType;
			this.relationship = relationship;
			this.AdditionalFilter = additionalFilter;
			this.SortComparer = sortComparer;
			this.CollectionState = collectionState;
			this.cacheKey = cacheKey;

			this.id = Guid.NewGuid();

			ActiveBusinessObjectCollectionIndexNotifier.For(Factory).Add(this);

			Relationship.RelationshipFilterChanged += new EventHandler(Relationship_RelationshipFilterChanged);

			ActiveBusinessObjectCollectionDataRefreshAdder.NotifyCollectionIndexCreated(this);
		}

		public BusinessObjectFactory Factory { get; private set; }
		public readonly Type CollectionType;
		public Type ElementType { get; private set; }
		public readonly ZQuery AdditionalFilter;
		public readonly IComparer SortComparer;
		public readonly object[] CollectionState;
		protected List<T> UncommittedObjects { get; private set; } = new List<T>();
		protected int inPopulateCacheCounter;

		protected bool IsInPopulateCache => inPopulateCacheCounter > 0;

		internal ActiveBusinessObjectCollectionIndexCache<T>.CacheKey cacheKey;

		[ThreadStatic]
		static bool hookToDisposedObjectErrorHasOccurred;
		StackTrace disposeStackTrace;

		public ICollectionRelationship Relationship
		{
			get { return relationship; }
		}
		readonly ICollectionRelationship relationship;

		public T AddNew()
		{
			T result = null;
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				result = GetBusinessObjectFromRow(Table.NewRow());
				var activeOwner = ActiveOwner;
				if (activeOwner != null)
				{
					activeOwner.SetDefaultsAndRelationshipForNewElement(result);
					activeOwner.OnAddIntoRelationship(result);
				}
				AddRowToTable(result);
			}

			if (!IsLoaded)
			{
				OnLoadedIntoCollection(result);
			}

			if (list != null && Factory.AreCollectionListChangedEventsDelayed && !list.Contains(result))
			{
				list.Add(result);
			}

			return result;
		}

		void AddRowToTable(T element)
		{
			if (element == null || element.Row == null || element.Row.Table == null)
			{
				return;
			}

			try
			{
				element.Row.Table.Rows.Add(element.Row);
				element.IsDataRowInDataTable = true;
			}
			catch (InvalidOperationException ex)
			{
				HandleAddToTableException(element, ex);
			}
			catch (ArgumentNullException ex)
			{
				HandleAddToTableException(element, ex);
			}
		}

		void HandleAddToTableException(T element, Exception ex)
		{
			var messageBuilder = new StringBuilder();
			messageBuilder.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"{0} [{1}] during adding DataRow to a DataTable in collection {2} of elements of type {3}.", ex.GetType().Name, ex.Message, CollectionType.FullName, ElementType.FullName);
			if (Relationship != null)
			{
				messageBuilder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Relationship type = {0}, relationship filter = {1}", Relationship.GetType().FullName, Relationship.RelationshipFilter.LiteralTextADO);
			}
			if (AdditionalFilter != null)
			{
				messageBuilder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Additional filter = {0}", AdditionalFilter.LiteralTextADO);
			}
			messageBuilder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, (NoResString)"DataRow state = {0}", element.Row.RowState);
			messageBuilder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Number of indexes of same type = {0}", ActiveBusinessObjectCollectionIndexCache<T>.GetInstance(Factory).All.Count());

			if (element.Row.Table.Rows.Contains(element.Row) && element.Row.RowState != DataRowState.Detached)
			{
				ErrorReporter.ReportOnce("ArgumentNullExceptionAfterAddingRowToTable", messageBuilder.ToString(), ex);
				InvalidateCacheHard(true);
			}
			else
			{
				throw new CannotAddNewRowToTableException(messageBuilder.ToString(), element, ex);
			}
		}

		public T AddNewUncommitted()
		{
			T result = null;
			if (!inCancelEdit) // In .net 2.0 RelatedCurrencyManager.ParentManager_CurrentItemChanged annoyingly calls AddNew().CancelEdit() which causes re-entrency problems, so IBindingList.AddNew will return null to overcome this performance issue
			{
				InAddNewUncommitted = true;
				try
				{
					result = GetBusinessObjectFromRow(Table.NewRow());
					lastNewRowFromAddNew = result.Row;

					ActiveOwner.SetDefaultsAndRelationshipForNewElement(result); // TODO: Fix when ActiveOwner is null. Mykola Kovalchuk

					if (!List.Contains(result))
					{
						List.Add(result);
						UncommittedObjects.Add(result);
						//Have to do this because HasChangesChanged isn't hit from here
						hasChanges = (hasChanges == true || Relationship.HasChangesIncludingRelationship(result)) ? true : null;
					}

					FireChangeEvents(new ListChangedEventArgs(ListChangedType.ItemAdded, Count - 1));
				}
				finally
				{
					InAddNewUncommitted = false;
				}
			}
			return result;
		}

		protected bool InAddNewUncommitted { get; private set; }

		public bool IsNonCommittedElement(BusinessObject businessObject)
		{
			if ((object)businessObject == null)
			{
				ErrorReporter.ReportOnce("NullBizoInIsNonCommittedElement", "BusinessObject passed to IsNonCommittedElement() is null. Type of elements: " + ElementType.FullName + ", Type of collection " + GetType().Name);
			}
			if (lastNewRowFromAddNew == null && (object)businessObject != null && ((INeedRow)businessObject).Row == lastNewRowFromAddNew)
			{
				ErrorReporter.ReportOnce("NullBizoInIsNonCommittedElement", "BusinessObject passed to IsNonCommittedElement() has null Row. Type of bizo: " + businessObject.GetType().FullName + ", Type of collection " + GetType().Name);
			}

			return (object)businessObject != null && lastNewRowFromAddNew != null &&
				((INeedRow)businessObject).Row == lastNewRowFromAddNew && lastNewRowFromAddNew.RowState == DataRowState.Detached;
		}

		public T[] ToArray()
		{
			T[] result = List.ToArray();
			if (ElementType != typeof(T))//where ActiveBusinessObjectCollection<T> is inherited and there is a new indexer
			{
				result = (T[])new ArrayList(result).ToArray(ElementType);
			}
			return result;
		}

		internal ZQuery CompleteFilter
		{
			get
			{
				if (completeFilter == null)
				{
					ZQuery result = new ZQuery();

					ZQuery relationshipFilter = Relationship.RelationshipFilter;

					result.AddToFilter(relationshipFilter, JoinCondition.And);
					result.AddToFilter(AdditionalFilter, JoinCondition.And);

					result.MaximumRows = AdditionalFilter.MaximumRows;
					if (result.MaximumRows == null && relationshipFilter != null)
					{
						result.MaximumRows = relationshipFilter.MaximumRows;
					}

					if (!result.IgnoreActiveFilter)
					{
						ZQuery activeFilter = BusinessObject.GetActiveFilter(ElementType);
						result.AddToFilter(activeFilter, JoinCondition.And);
						result.IgnoreActiveFilter = true;
					}

					completeFilter = result;
				}
				return completeFilter;
			}
		}
		ZQuery completeFilter;

		protected string CompleteAdoReductionFilter
		{
			get
			{
				string result = "";
				if (!CompleteFilter.IsDBOnlyQuery)
				{
					result = CompleteFilter.IsNoResultQuery ? "0 = 1" : CompleteFilter.LiteralTextADO;
				}
				else if (Relationship != null && !Relationship.RelationshipFilter.IsDBOnlyQuery)
				{
					result = Relationship.RelationshipFilter.IsNoResultQuery ? "0 = 1" : Relationship.RelationshipFilter.LiteralTextADO;
				}
				return result;
			}
		}

		public bool IsLoaded
		{
			get { return isLoaded; }
		}
		bool isLoaded;

		public T FindByPK(ZGuid pk)
		{
			var result = LoadWithExtraExceptionHandling(pk);
			return Contains(result) ? result : null;
		}

		public ListSortDescriptionCollection SortDescriptions
		{
			get
			{
				if (!sortDescriptionsPopulated)
				{
					sortDescriptions = new ListSortDescriptionCollection();

					IComparer comparer = SortComparer;
					IFreezeSortOnElementModifyComparer freezableComparer = comparer as IFreezeSortOnElementModifyComparer;
					if (freezableComparer != null)
					{
						comparer = freezableComparer.Comparer;
					}
					MultiPropertyComparer multiComparer = comparer as MultiPropertyComparer;
					if (multiComparer != null)
					{
						sortDescriptions = multiComparer.SortDescriptions;
					}
					PropertyComparer singleComparer = comparer as PropertyComparer;
					if (singleComparer != null)
					{
						sortDescriptions = new ListSortDescriptionCollection(new ListSortDescription[]
						{
							new ListSortDescription(singleComparer.PropertyDescriptor, singleComparer.Direction)
						});
					}
					sortDescriptionsPopulated = true;
				}
				return sortDescriptions;
			}
		}
		bool sortDescriptionsPopulated;
		ListSortDescriptionCollection sortDescriptions;

		public void Refresh()
		{
			if (CompleteFilter.IsDBOnlyQuery)
			{
				isDataLoaded = false;
			}
			InvalidateCache();
			FireChangeEvents(ListChangedType.Reset, -1, -1);
		}

		bool hasDeactivated;

		public void DeactivateAllOwners()
		{
			hasDeactivated = true;
			var ownersCopy = activeOwners.ToArray();
			foreach (var ownerRef in ownersCopy)
			{
				if (ownerRef.TryGetTarget(out var owner))
				{
					owner.Deactivate();
				}
			}
		}

		public void DeleteAll(IActiveBusinessObjectCollection collection)
		{
			if (Count > 0)
			{
				using (SuspendListChanged())
				{
					for (int i = Count - 1; i >= 0; i--)
					{
						var item = this[i];
						if (item != null && !item.IsDeleted)
						{
							this[i].FetchStrategy.FetchForDelete();
						}
					}
					for (int i = Count - 1; i >= 0; i--)
					{
						if (i < Count)
						{
							collection.Delete(this[i]);
						}
					}
				}
				FireChangeEvents(ListChangedType.Reset, -1, -1);
			}
		}

		#region AddOwner / Disconnect / ActiveOwner

		internal void AddOwner(ActiveBusinessObjectCollection<T> owner)
		{
			activeOwners.Add(new WeakReference<ActiveBusinessObjectCollection<T>>(owner));
		}

		bool hasDisconnected;

		internal void Disconnect(ActiveBusinessObjectCollection<T> owner)
		{
			for (int i = 0; i < activeOwners.Count; i++)
			{
				if (activeOwners[i].TryGetTarget(out var current) && current == owner)
				{
					activeOwners.RemoveAt(i);
					hasDisconnected = true;
					break;
				}
			}
		}

		bool hasLostActiveOwner;

		protected ActiveBusinessObjectCollection<T> ActiveOwner
		{
			get
			{
				var result = temporarilyStrongActiveOwner;
				while (activeOwners.Count > 0 && result == null)
				{
					if (!activeOwners[0].TryGetTarget(out result))
					{
						activeOwners.RemoveAt(0);
						hasLostActiveOwner = true;
					}
				}
				return result;
			}
		}
		readonly List<WeakReference<ActiveBusinessObjectCollection<T>>> activeOwners = new List<WeakReference<ActiveBusinessObjectCollection<T>>>();
		ActiveBusinessObjectCollection<T> temporarilyStrongActiveOwner;

		protected DisposableAction MakeActiveOwnerStrongTemporarilyForPerformance()
		{
			temporarilyStrongActiveOwner = ActiveOwner;
			return new DisposableAction(delegate
			{
				temporarilyStrongActiveOwner = null;
			});
		}

		#region Test stuff
#if DEBUG
		public ActiveBusinessObjectCollection<T> ActiveOwnerExposed => ActiveOwner;
#endif
		#endregion

		#endregion

		#region HasChanges

		public bool HasChanges
		{
			get
			{
				if (hasChanges == null)
				{
					hasChanges = HasChangesFromDelete;

					if (!(bool)hasChanges)
					{
						if (IsInPopulateCache)
						{ return false; }

						using (SuppressDataLoad())
						{
							bool hasChangesIncludingRelationship = false;
							foreach (T item in this)
							{
								if (Relationship.HasChangesIncludingRelationship(item))
								{
									hasChangesIncludingRelationship = true;
									break;
								}
							}
							hasChanges = hasChangesIncludingRelationship;
							return hasChangesIncludingRelationship; // hasChanges can be reset by disposing of SuppressDataLoad()
						}
					}
				}
				return (bool)hasChanges;
			}
		}
		bool? hasChanges;

		#region Test stuff
#if DEBUG
		internal bool? HasChangesFieldExposedForTest { get { return hasChanges; } }
#endif
		#endregion

		public bool HasChangesFromDelete
		{
			get { return hasChangesFromDelete; }
			set
			{
				hasChangesFromDelete = value;

				var newHasChangesValue = value ? true : (bool?)null;
				if (hasChanges != newHasChangesValue)
				{
					hasChanges = newHasChangesValue;
					OnHasChangesChanged(HasChangesChangedEventArgs.Create(newHasChangesValue.HasValue && newHasChangesValue.Value, this));
				}
			}
		}
		bool hasChangesFromDelete;

		#endregion

		#region UniqueBusinessObjectComparer

		protected ComparerWithRowOrderFallback UniqueBusinessObjectComparer
		{
			get
			{
				if (SortComparer != null && uniqueBusinessObjectComparer == null)
				{
					uniqueBusinessObjectComparer = new ComparerWithRowOrderFallback(this);
				}
				return uniqueBusinessObjectComparer;
			}
		}
		ComparerWithRowOrderFallback uniqueBusinessObjectComparer;

		protected class ComparerWithRowOrderFallback : IComparer<T>
		{
			public ComparerWithRowOrderFallback(ActiveBusinessObjectCollectionIndex<T> owner)
			{
				this.owner = owner;
			}

			public int Compare(T x, T y)
			{
				int result = CompareByNull(x, y);
				if (result == 0)
				{
					result = CompareByIsDeleted(x, y);
				}
				if (result == 0)
				{
					result = CompareByIsNonCommitted(x, y);
				}
				if (result == 0)
				{
					result = owner.SortComparer.Compare(x, y);
				}
				if (result == 0)
				{
					result = CompareByObjectAddedTime(x, y);
				}
				return result;
			}

			int CompareByNull(T x, T y)
			{
				int result = 0;
				if ((object)x == null)
				{
					result = -1;
				}
				else if ((object)y == null)
				{
					result = 1;
				}
				return result;
			}

			int CompareByIsDeleted(T x, T y)
			{
				if (x.IsDeleted)
				{
					return 1;
				}
				if (y.IsDeleted)
				{
					return -1;
				}

				return 0;
			}

			int CompareByIsNonCommitted(T x, T y)
			{
				int result = 0;
				if (!owner.IsNonCommittedElement(x) && owner.IsNonCommittedElement(y))
				{
					result = -1;
				}
				else if (owner.IsNonCommittedElement(x) && !owner.IsNonCommittedElement(y))
				{
					result = 1;
				}
				return result;
			}

			int CompareByObjectAddedTime(T x, T y)
			{
				return x.Table.Rows.IndexOf(x.Row) - y.Table.Rows.IndexOf(y.Row);
			}

			readonly ActiveBusinessObjectCollectionIndex<T> owner;
		}

		#endregion

		#region ConsolidateListResetEvents / SuspendListChanged

		public IDisposable ConsolidateListResetEvents()
		{
			ListChangedEventHandler listChangedHandler = null;
			IDisposable listChangedSuspender = null;
			if (listChanged != null)
			{
				listChangedHandler = (object sender, ListChangedEventArgs e) =>
				{
					if (e.ListChangedType == ListChangedType.Reset)
					{
						ListChanged -= listChangedHandler;
						listChangedHandler = null;
						listChangedSuspender = SuspendListChanged();
					}
				};
				ListChanged += listChangedHandler;
			}
			return new DisposableAction(delegate
			{
				if (listChangedHandler != null)
				{
					ListChanged -= listChangedHandler;
				}
				if (listChangedSuspender != null)
				{
					listChangedSuspender.Dispose();
					FireChangeEvents(ListChangedType.Reset, -1, -1);
				}
			});
		}

		public IDisposable SuspendListChanged()
		{
			listChangedSuspendedIndex++;
			return new DisposableAction(delegate
			{ listChangedSuspendedIndex--; });
		}

		bool IsListChangedSuspended
		{
			get { return listChangedSuspendedIndex > 0; }
		}
		int listChangedSuspendedIndex;

		#endregion

		#region ListChanged / HasChangesChanged events

		public event ListChangedEventHandler ListChanged
		{
			add
			{
				EnsurePopulated();
				listChanged += value;
			}
			remove { listChanged -= value; }
		}
		event ListChangedEventHandler listChanged;

		bool IBindingTracked.IsBound => listChanged.IsBound(doRecursiveCheck: true);

		protected virtual void OnHasChangesChangedHandlerAdded()
		{
		}

		public event EventHandler<HasChangesChangedEventArgs> HasChangesChanged
		{
			add
			{
				OnHasChangesChangedHandlerAdded();
				hasChangesChanged += value;
			}
			remove { hasChangesChanged -= value; }
		}
		event EventHandler<HasChangesChangedEventArgs> hasChangesChanged;

		void OnHasChangesChanged(HasChangesChangedEventArgs e)
		{
			if (hasChangesChanged != null)
			{
				hasChangesChanged(this, e);
			}
		}

		protected void FireChangeEvents(ListChangedType listChangedType, int newEntityIndex, int oldEntityIndex)
		{
			ListChangedEventArgs eventArgs;
			if (listChangedType == ListChangedType.Reset)
			{
				eventArgs = new ListChangedEventArgs(ListChangedType.Reset, -1);
			}
			else
			{
				eventArgs = new ListChangedEventArgs(listChangedType, newEntityIndex, oldEntityIndex);
			}
			FireChangeEvents(eventArgs);
		}

		protected void FireChangeEvents(ListChangedEventArgs e)
		{
			CollectionListChangedSuspender.GetInstance(Factory).RunDelayableListChanged(FireChangeEvents_Delayed, ActiveOwner, e);
		}

		void FireChangeEvents_Delayed(object sender, ListChangedEventArgs e)
		{
			if (!IsListChangedSuspended)
			{
				if (listChanged != null)
				{
					try
					{
						listChanged(this, e);
					}
					catch (IndexOutOfRangeException)
					{
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce(ex.Message, ex);
					}
				}
				UpdateHasChanges(e);
				if (e.ListChangedType != ListChangedType.ItemChanged && !IsValidationSuspendedOnCollection)
				{
					OnNotificationsChanged(new NotificationsChangedEventArgs(null));
				}
			}
		}

		void UpdateHasChanges(ListChangedEventArgs e)
		{
			if (!(e.ListChangedType == ListChangedType.ItemAdded &&
					(e.NewIndex >= Count ||
					e.NewIndex == Count - 1 && IsNonCommittedElement(this[e.NewIndex]))))
			{
				UpdateHasChangesCore(e);
			}
		}

		void UpdateHasChangesCore(ListChangedEventArgs e)
		{
			bool? newHasChangesValue = null;

			if (e.ListChangedType == ListChangedType.ItemChanged || e.ListChangedType == ListChangedType.ItemAdded)
			{
				if (e.NewIndex < 0 || e.NewIndex >= List.Count)
				{
					throw new IndexOutOfRangeException(string.Format(
						"ActiveBusinessObjectCollectionIndex<T>.UpdateHasChangesCore: Wrong e.NewIndex value: {0}, current elements count: {1}",
						e.NewIndex,
						List.Count));
				}

				newHasChangesValue = Relationship.HasChangesIncludingRelationship(this[e.NewIndex]) ? true : null;
			}
			else if (e.ListChangedType == ListChangedType.ItemDeleted)
			{
				var currentValue = hasChanges.HasValue && hasChanges.Value;
				newHasChangesValue = currentValue ? null : currentValue;
			}

			if ((!newHasChangesValue.HasValue || !newHasChangesValue.Value) && hasChangesFromDelete)
			{
				newHasChangesValue = true;
			}

			if (hasChanges != newHasChangesValue || (!newHasChangesValue.HasValue && e.ListChangedType == ListChangedType.Reset))
			{
				hasChanges = newHasChangesValue;
				OnHasChangesChanged(HasChangesChangedEventArgs.Create(newHasChangesValue.HasValue && newHasChangesValue.Value, this));
			}
		}

		protected bool ShouldFireChangeEvents()
		{
			return listChanged != null || hasChangesChanged != null || notificationsChanged != null;
		}

		#endregion

		#region List / DataViewBusinessObjectMapping

		internal class BusinessObjectList
		{
			public BusinessObjectList(ActiveBusinessObjectCollectionIndex<T> owner)
			{
				this.owner = owner;
			}

			public void Add(T value)
			{
				OnChanging(ChangeSource.Add);
				AddCore(value);
			}

			void AddCore(T value)
			{
				IndexOfCache[value.PK] = Count;
				List.Add(value);
				owner.HookBusinessObject(value);
			}

			public void AddRange(IEnumerable<BusinessObject> businessObjects)
			{
				OnChanging(ChangeSource.Add);
				foreach (BusinessObject businessObject in businessObjects.ToArray())
				{
					AddCore((T)businessObject);
				}
			}

			public void Insert(int index, T value)
			{
				OnChanging(ChangeSource.Insert);
				if (index == Count)
				{
					IndexOfCache[value.PK] = index;
				}
				else
				{
					InvalidateIndexOfCache();
				}
				List.Insert(index, value);
				owner.HookBusinessObject(value);
			}

			public void Remove(T value)
			{
				OnChanging(ChangeSource.Remove);
				InvalidateIndexOfCache();
				owner.UnhookBusinessObject(value);
				List.Remove(value);
			}

			public void RemoveAt(int index)
			{
				OnChanging(ChangeSource.RemoveAt);
				var value = this[index];
				if (index == Count - 1)
				{
					IndexOfCache.Remove(value.PK);
				}
				else
				{
					InvalidateIndexOfCache();
				}
				owner.UnhookBusinessObject(value);
				List.RemoveAt(index);
			}

			public void Clear()
			{
				OnChanging(ChangeSource.Clear);
				InvalidateIndexOfCache();
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(owner.Factory))
				{
					foreach (T businessObject in List)
					{
						owner.UnhookBusinessObject(businessObject);
					}
				}

				List.Clear();
			}

			public int Count
			{
				get { return List.Count; }
			}

			public IEnumerator<T> GetEnumerator()
			{
				enumeratingCounter++;
				try
				{
					int startingVersion = version;
					foreach (T item in List.Where(item => !item.IsDeleted).ToArray())
					{
						yield return item;
						if (startingVersion != version)
						{
							var parentTypeName = owner.ActiveOwner != null ? owner.ActiveOwner.GetType().Name : owner.GetType().Name;
							ErrorReporter.ReportOnce(parentTypeName + "_" + owner.ElementType.Name + "_EnumeratorChangedVersion", string.Format("Collection {0} with elements of type {1} was modified : enumeration will continue and the system will continue to function. Source = {2}", parentTypeName, owner.ElementType, lastChangeSource));
						}
					}
				}
				finally
				{
					enumeratingCounter--;
				}
			}

			internal bool IsEnumerating => enumeratingCounter > 0;

			int enumeratingCounter;

			public bool Contains(T element)
			{
				return List.Contains(element);
			}

			public int IndexOf(T item)
			{
				int result = -1;
				if (indexOfCache != null)
				{
					if (!indexOfCache.TryGetValue(item.PK, out result))
					{
						result = -1;
					}
				}
				if (result == -1)
				{
					result = List.IndexOf(item);
					IndexOfCache[item.PK] = result;
				}
				return result;
			}

			public T[] ToArray()
			{
				return List.Where(item => item != null && !item.IsDeleted).ToArray();
			}

			public void CopyTo(Array array, int index)
			{
				((IList)List).CopyTo(array, index);
			}

			public void CopyTo(T[] array, int index)
			{
				List.CopyTo(array, index);
			}

			public int BinarySearch(T item, IComparer<T> comparer)
			{
				return List.BinarySearch(item, comparer);
			}

			public int BinarySearchAssumingItsNotInTheRightOrder(int index, IComparer<T> comparer)
			{
				int result;
				if ((index < Count - 1 && comparer.Compare(this[index], this[index + 1]) > 0) &&
						(index > 0 && comparer.Compare(this[index], this[index - 1]) < 0))
				{
					result = index;
				}
				else
				{
					result = -List.BinarySearch(0, index, this[index], comparer) - 1;
					if (result >= index && (index != Count - 1))
					{
						result = -List.BinarySearch(index + 1, Count - index - 1, this[index], comparer) - 1;
					}
				}
				return result;
			}

			public T this[int i]
			{
				get { return List[i]; }
			}

			public IDisposable SuspendEnumerationCheck()
			{
				enumerationCheckSuspended++;
				return new DisposableAction(delegate
				{
					enumerationCheckSuspended--;
				});
			}

			public void Sort(IComparer comparer)
			{
				List.Sort(comparer.Compare);
				InvalidateIndexOfCache();
			}

			#region Implementation

			readonly ActiveBusinessObjectCollectionIndex<T> owner;
			Dictionary<ZGuid, int> indexOfCache;
			readonly List<T> List = new List<T>();
			ChangeSource lastChangeSource;
			int version;
			int enumerationCheckSuspended;

			enum ChangeSource
			{
				Add,
				Remove,
				RemoveAt,
				Insert,
				Clear,
			}

			void OnChanging(ChangeSource changeSource)
			{
				lastChangeSource = changeSource;
				if (enumerationCheckSuspended == 0)
				{
					version++;
					if (IsEnumerating)
					{
						var parentTypeName = owner.ActiveOwner != null ? owner.ActiveOwner.GetType().Name : owner.GetType().Name;
						ErrorReporter.ReportOnce(parentTypeName + "_" + owner.ElementType.Name + "_ChangingDuringEnumeration", string.Format("Modification of collection {0} with elements of type {1} during enumeration : system will continue to function. Source = {1}", parentTypeName, owner.ElementType, changeSource));
					}
				}
			}

			void InvalidateIndexOfCache()
			{
				indexOfCache = null;
			}

			Dictionary<ZGuid, int> IndexOfCache
			{
				get
				{
					if (indexOfCache == null)
					{
						indexOfCache = new Dictionary<ZGuid, int>();
					}
					return indexOfCache;
				}
			}

			#endregion
		}

		internal BusinessObjectList List
		{
			get
			{
				DecrementReadOnlyIncludingChildrenAndResumeValidationIfRequired();
				if (list == null)
				{
					PopulateCache();
				}
				return list;
			}
			private set
			{
				if (value != list && list != null)
				{
					using (list.SuspendEnumerationCheck())
					{
						list.Clear();
					}
				}
				list = value;

				if (list != null)
				{
					foreach (T businessObject in list)
					{
						HookEvents(businessObject);
					}
				}
			}
		}
		BusinessObjectList list;

		bool IsAdhocRelationship
		{
			get
			{
				bool isAdhoc = Relationship is AdhocCollectionRelationship;
				if (isAdhoc && IsMatchesFilterOverridden)
				{
					string collectionType = ActiveOwner.GetType().FullName;
					string relationshipType = Relationship.GetType().FullName;

					ErrorReporter.ReportOnce(
						collectionType + "|" + relationshipType,
						string.Format("Collection {0} uses an Adhoc Collection Relationship {1} and also overrides MatchesFilterCore. When using an Adhoc Relationship, you should not override MatchesFilterCore as the collection is entirely managed manually.",
							collectionType, relationshipType)
					);
				}
				return isAdhoc && !IsMatchesFilterOverridden;
			}
		}

		void PopulateCache()
		{
			if (ActiveOwner == null)
			{
				ErrorReporter.ReportOnce("ABOCI_PopulateCache", string.Format(CultureInfo.InvariantCulture, "ABOCI<{0}>.PopulateCache() was called with ActiveOwner == null. IsDisposed = {1}. hasDisconncted = {2}. hasDeactivated = {3}. hasLostActiveOwner = {4}.", typeof(T).Name, IsDisposed, hasDisconnected, hasDeactivated, hasLostActiveOwner));
			}

			isLoaded = true;
			inPopulateCacheCounter++;

			try
			{
				ResetListWithCache();
			}
			finally
			{
				inPopulateCacheCounter--;
			}

			OnRebuilt(EventArgs.Empty);
		}

		internal void ResetListWithCache()
		{
			List = null;
			List = PopulateCacheCore();
		}

		protected abstract BusinessObjectList PopulateCacheCore();

		#endregion

		#region OnLoadedIntoCollection

		protected void OnLoadingIntoCollection(T businessObject)
		{
			var activeOwner = ActiveOwner;
			if (activeOwner != null)
			{
				activeOwner.OnLoadingIntoCollection(businessObject);
			}
		}

		void OnLoadedIntoCollection(T businessObject)
		{
			var activeOwner = ActiveOwner;
			if (activeOwner != null && IsOnLoadedIntoCollectionOverridden)
			{
				activeOwner.OnLoadedIntoCollection(businessObject);
			}
		}

		bool IsOnLoadedIntoCollectionOverridden
		{
			get { return isOnLoadedIntoCollectionOverridden ?? (bool)(isOnLoadedIntoCollectionOverridden = ActiveBusinessObjectCollection<T>.IsOnLoadedIntoCollectionOverridden(CollectionType)); }
		}
		bool? isOnLoadedIntoCollectionOverridden;

		#endregion

		#region BusinessObject events

		void HookBusinessObject(T businessObject)
		{
			if (IsValidationSuspendedOnCollection)
			{
				businessObject.SuspendValidation();
			}
			if (readOnlyIndex > 0)
			{
				((IBusinessObjectState)businessObject).IncrementReadOnlyIncludingChildren();
			}

			HookEvents(businessObject); //Have to do this AFTER setting read only, because changing it calls BusinessObject_ListChanged which can look in the list - generating it re-entrantly while we're already generating it.
										//Also, we can't delay this until initial List construction is over - some unit tests will fail.

			OnLoadedIntoCollection(businessObject);
		}

		void UnhookBusinessObject(T businessObject)
		{
			UnhookEvents(businessObject);

			if (!businessObject.IsDeleted)
			{
				if (IsValidationSuspendedOnCollection)
				{
					businessObject.ResumeValidation();
				}
				if (readOnlyIndex > 0)
				{
					((IBusinessObjectState)businessObject).DecrementReadOnlyIncludingChildren();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void HookEvents(T businessObject)
		{
			UnhookEvents(businessObject);

			if (disposing)
			{
				// Prevent adding element to collecting via feedback from clearing/validation/etc during disposing.
				return;
			}

			if (IsDisposed)
			{
				if (disposeStackTrace != null)
				{
					// Assign issues with this error message to M.K
					ErrorReporter.ReportOnce(string.Format("Hooking to {0} with PK '{1}' (IsInDatabase={2}) in already disposed ABOCI<{3}>.\r\nActive owner = {4}\r\nDispose stack trace:\r\n{5}",
						businessObject.GetType().Name, businessObject.PK, businessObject.IsInDatabase, typeof(T).Name,
						ActiveOwner?.GetType().FullName ?? "null",
						disposeStackTrace.ToString()));
				}
				else
				{
					hookToDisposedObjectErrorHasOccurred = true;
				}
				return;
			}

			businessObject.HasChangesChanged += BusinessObject_HasChangesChanged;
			businessObject.NotificationsChanged += BusinessObject_NotificationsChanged;
			((IBindingList)businessObject).ListChanged += BusinessObject_ListChanged;
		}

		void UnhookEvents(T businessObject)
		{
			businessObject.HasChangesChanged -= BusinessObject_HasChangesChanged;
			businessObject.NotificationsChanged -= BusinessObject_NotificationsChanged;
			((IBindingList)businessObject).ListChanged -= BusinessObject_ListChanged;
		}

		void BusinessObject_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			hasChanges = null;
			OnHasChangesChanged(e);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void BusinessObject_ListChanged(object sender, ListChangedEventArgs e)
		{
			using (MakeActiveOwnerStrongTemporarilyForPerformance())
			{
				if (ActiveOwner == null && !IsInPopulateCache && !isInIndexSet)
				{
					if (disposing)
					{
						return;
					}

					if (!IsDisposed)
					{
						Dispose();
					}
					else
					{
						var businessObject = (T)sender;
						UnhookEvents(businessObject);
					}

					return;
				}

				if (inPopulateCacheCounter == 0) // Do not fire new events while we are rebuilding
				{
					ListChangedType listChangedType = ListChangedType.Reset;
					int oldIndex = -1;
					int newIndex = -1;
					if (inReadOnlyChangeIndex == 0)
					{
						bool dontBubbleEvent = false;
						if (!inListChangeProcess)
						{
							inListChangeProcess = true;
							HandleBusinessObjectElementChanged((T)sender, ref listChangedType, ref oldIndex, ref newIndex, ref dontBubbleEvent);
							if (!dontBubbleEvent)
							{
								FireChangeEvents(new ListChangedEventArgs(listChangedType, oldIndex, newIndex));
							}
							inListChangeProcess = false;
						}
					}
				}
			}
		}

		internal bool isInIndexSet;
		bool inListChangeProcess;

		void BusinessObject_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			if (IsCachePopulated())
			{
				BusinessObject_NotificationsChangedCore(e);
				OnNotificationsChanged(e);
			}
		}

		protected virtual void BusinessObject_NotificationsChangedCore(NotificationsChangedEventArgs e)
		{
		}

		#endregion

		protected bool isDataLoaded;

		protected virtual IDisposable SuppressDataLoad()
		{
			if (isDataLoaded)
			{
				return null;
			}

			isDataLoaded = true;
			return new DisposableAction(() =>
			{
				InvalidateCache();
				hasChanges = null;
				isDataLoaded = false;
			});
		}

		void EnsurePopulated()
		{
			object populated = List;
		}

		#region DataView.ListChanged

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected void HandleBusinessObjectElementChanged(T businessObject, ref ListChangedType listChangedType, ref int oldIndex, ref int newIndex, ref bool dontBubbleEvent)
		{
			if (MatchesFilter(businessObject))
			{
				var listWasNull = list == null;
				oldIndex = List.IndexOf(businessObject);
				if (oldIndex < 0)
				{
					dontBubbleEvent = true;

					ErrorReporter.ReportOnce("ABOCI_ChangedHookedBizoNotInList",
						string.Format(
							"A change event {0} in {1} with PK '{2}' (IsInDatabase={3}) has invoked method ABOCI<{4}>.HandleBusinessObjectElementChanged() while the business object does not exists in the inner list of the index." +
								"\r\nInner list count = {5}, list was null = {6}, SortComparer = {7}, IsDisposed = {8}." +
								"\r\nActive owner = {9}",
							listChangedType, businessObject.GetType().Name, businessObject.PK, businessObject.IsInDatabase, typeof(T).Name,
							List.Count, listWasNull, SortComparer != null ? SortComparer.GetType().FullName : "null", IsDisposed,
							ActiveOwner != null ? ActiveOwner.GetType().FullName : "null"
						));
				}
				else
				{
					listChangedType = ListChangedType.ItemChanged;

					newIndex =
						UniqueBusinessObjectComparer == null || SortComparer is IFreezeSortOnElementModifyComparer
							? oldIndex
							: ReshuffleBusinessObjectOrder(oldIndex);
					if (newIndex != oldIndex)
					{
						listChangedType = ListChangedType.ItemMoved;
					}
				}
			}
			else
			{
				// delete is handled by the DataView ItemDeleted
				dontBubbleEvent = true;
				//listChangedType = ListChangedType.ItemDeleted;
				//newIndex = -1;
			}
		}

		int ReshuffleBusinessObjectOrder(int index)
		{
			var result = index;

			var previousObjectShouldntBeBeforeIt = index >= 1 && UniqueBusinessObjectComparer.Compare(List[index - 1], List[index]) > 0;
			var nextObjectShouldntBeAfterIt = index >= 0 && index <= List.Count - 2 && UniqueBusinessObjectComparer.Compare(List[index], List[index + 1]) > 0;

			if ((previousObjectShouldntBeBeforeIt || nextObjectShouldntBeAfterIt) && !List.IsEnumerating)
			{
				var businessObject = List[index];
				List.RemoveAt(index);

				result = SortedIndexForObject(businessObject);

				List.Insert(result, businessObject);

				FixMappingsCore(index, result);
			}

			return result;
		}

		protected virtual void FixMappingsCore(int oldDestination, int newDestination)
		{
		}

		int SortedIndexForObject(T businessObject)
		{
			var result = List.BinarySearch(businessObject, UniqueBusinessObjectComparer);
			if (result < 0)
			{
				result = -result - 1;
			}

			return result;
		}

		#endregion

		#region Dispose

		public event EventHandler Disposed;

		internal void DisposeIfUnused()
		{
			if (ActiveOwner == null)
			{
				Dispose();
			}
		}

		public void Dispose()
		{
			disposing = true;
			try
			{
				DecrementReadOnlyIncludingChildrenAndResumeValidationIfRequired();

				if (list != null)
				{
					List.Clear(); // unhook all business objects
				}

				//make some best-faith efforts to detach events from MulticastDelegates, if they OoM then oh well
				try
				{
					ActiveBusinessObjectCollectionIndexNotifier.For(Factory).Remove(this);
				}
				catch (OutOfMemoryException) { }
				Relationship.RelationshipFilterChanged -= new EventHandler(Relationship_RelationshipFilterChanged);
				ActiveBusinessObjectCollectionDataRefreshAdder.NotifyCollectionIndexDisposed(this);

				Dispose(disposing);

				if (table != null)
				{
					try
					{
						table.RowDeleting -= new DataRowChangeEventHandler(Table_RowDeleting);
					}
					catch (OutOfMemoryException) { }
				}

				IsDisposed = true;
				Disposed?.Invoke(this, EventArgs.Empty);

				if (hookToDisposedObjectErrorHasOccurred)
				{
					disposeStackTrace = new StackTrace();
				}
			}
			finally
			{
				disposing = false;
			}
		}

		protected virtual void Dispose(bool isDisposing)
		{
		}

		internal bool IsDisposed { get; private set; }

		#endregion

		#region CheckInvariant

		protected void CheckInvariantIfEnabled()
		{
			if (ActiveBusinessObjectCollection.IsInvariantEnabled)
			{
				CheckInvariant();
			}
		}

		[Conditional("DEBUG")]
		internal void CheckInvariant()
		{
			if (IsCachePopulated())
			{
				if (!CompleteFilter.IsDBOnlyQuery)
				{
					CheckCorrectItemsInList();
				}
				CheckSortOrder();
				CheckInvariantCore();
			}
		}

		protected virtual void CheckInvariantCore()
		{
		}

		void CheckCorrectItemsInList()
		{
			int count = 0;
			foreach (DataRowView rowView in new DataView(Table, CompleteAdoReductionFilter, "", DataViewRowState.CurrentRows))
			{
				DataRow row = rowView.Row;
				var businessObjectSafe = GetBusinessObjectFromRowSafe(row);
				if (MatchesFilter(businessObjectSafe))
				{
					if (CompleteFilter.MaximumRows != null && ++count == CompleteFilter.MaximumRows)
					{
						break;
					}
					if (businessObjectSafe is T businessObject && !List.Contains(businessObject))
					{
						ErrorReporter.ReportOnce("Business object " + businessObject.HumanReadableName + " matches the collection criteria but is not in the collection");
					}
				}
				else if (businessObjectSafe is T businessObject && List.Contains(businessObject))
				{
					ErrorReporter.ReportOnce("Business object " + businessObject.HumanReadableName + " is in the collection but does not match the collection criteria");
				}
				if (businessObjectSafe.IsDeleted)
				{
					ErrorReporter.ReportOnce("Business object has been deleted but still exists in the list");
				}
			}
		}

		void CheckSortOrder()
		{
			T lastItem = null;
			if (list != null && !(SortComparer is IFreezeSortOnElementModifyComparer))
			{
				for (int i = 0; i < list.Count; i++)
				{
					T item = list[i];
					if (lastItem != null &&
						UniqueBusinessObjectComparer != null &&
						UniqueBusinessObjectComparer.Compare(lastItem, item) > 0)
					{
						ErrorReporter.ReportOnce("Item at position " + i + " not in the correct order");
					}
					lastItem = item;
				}
			}
		}

		#endregion

		#region IList

		public int Count
		{
			get { return List.Count; }
		}

		public void Add(T businessObject)
		{
			using (businessObject.SuspendListChanged())
			{
				var activeOwner = ActiveOwner;
				if (activeOwner != null)
				{
					activeOwner.SetRelationshipDefaultsForElement(businessObject);
					activeOwner.OnAddIntoRelationship(businessObject);
				}
			}
		}

		public void AddRange(IEnumerable businessObjects)
		{
			using (ConsolidateListResetEvents())
			{
				foreach (T item in businessObjects)
				{
					Add(item);
				}
			}
		}

		public void Remove(T businessObject)
		{
			Relationship.RemoveFromRelationship(businessObject);
		}

		public void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		public int IndexOf(T businessObject)
		{
			return List.IndexOf(businessObject);
		}

		public bool Contains(T businessObject)
		{
			bool result;
			if (IsCachePopulated())
			{
				result = businessObject != null && IndexOf(businessObject) != -1;
			}
			else
			{
				result = businessObject != null && !businessObject.IsDeleted && MatchesFilter(businessObject);
				if (result && CompleteFilter.MaximumRows != null && IndexOf(businessObject) == -1)
				{
					result = false;
				}
			}
			return result;
		}

		public void CopyTo(Array array, int index)
		{
			List.CopyTo(array, index);
		}

		public void CopyTo(T[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public T this[int index]
		{
			get { return List[index]; }
		}

		#endregion

		#region IEnumerable

		public IEnumerator<T> GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion

		#region ICancelAddNew

		public void Delete(T element)
		{
			if (IsNonCommittedElement(element))
			{
				CancelNew(element);
			}
			else
			{
				DeleteCore(element);
			}
		}

		protected virtual void DeleteCore(T element)
		{
		}

		public void CancelNew(T element)
		{
			if (IsNonCommittedElement(element))
			{
				inCancelEdit = true;
				try
				{
					lastNewRowFromAddNew = null;
					List.Remove(element);
					UncommittedObjects.Remove(element);
					element.Delete();
					FireChangeEvents(new ListChangedEventArgs(ListChangedType.ItemDeleted, Count - 1));
				}
				finally
				{
					inCancelEdit = false;
				}
			}
		}

		public void EndNew(T element)
		{
			if (IsNonCommittedElement(element))
			{
				UncommittedObjects.Remove(element);

				bool hasChanges = element.HasChanges;
				if (hasChanges)
				{
					using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
					{
						AddRowToTable(element);
					}
				}
				lastNewRowFromAddNew = null;
				if (hasChanges && MatchesFilter(element))
				{
					if (List[List.Count - 1] == element)
					{
						CommitNewElement();
					}
				}
				else
				{
					if (List[List.Count - 1] == element)
					{
						List.RemoveAt(List.Count - 1);
						FireChangeEvents(new ListChangedEventArgs(ListChangedType.ItemDeleted, Count - 1));
					}
				}
				if (!hasChanges)
				{
					element.Delete();
				}
			}
		}

		void CommitNewElement()
		{
			T item = List[Count - 1];
			List.RemoveAt(List.Count - 1);
			int index = InsertSorted(List, item);

			if (index == List.Count - 1)
			{
				FireChangeEvents(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
			}
			else
			{
				FireChangeEvents(new ListChangedEventArgs(ListChangedType.ItemMoved, index));
				FixMappingsCore(List.Count - 1, index);
			}
		}

		#endregion

		#region IBusiness

		int suspendValidationIndex;

		public bool IsValidationSuspendedOnCollection
		{
			get { return suspendValidationIndex > 0; }
		}

		public void SuspendValidation()
		{
			if (suspendValidationIndex++ == 0)
			{
				if (IsCachePopulated())
				{
					foreach (IBusiness element in this)
					{
						element.SuspendValidation();
					}
				}
			}
		}

		public void ResumeValidation()
		{
			if (suspendValidationIndex > 0)
			{
				suspendValidationIndex--;
				if (suspendValidationIndex == 0 && IsCachePopulated())
				{
					foreach (IBusiness element in this)
					{
						element.ResumeValidation();
					}
				}
			}
		}

		#endregion

		#region IBusinessObjectState Members

		bool resumeValidationPending;
		bool decrementReadOnlyIncludingChildrenPending;

		public event EventHandler<NotificationsChangedEventArgs> NotificationsChanged
		{
			add { notificationsChanged += value; }
			remove { notificationsChanged -= value; }
		}
		event EventHandler<NotificationsChangedEventArgs> notificationsChanged;

		protected void OnNotificationsChanged(NotificationsChangedEventArgs e)
		{
			if (notificationsChanged != null)
			{
				notificationsChanged(this, e);
			}
		}

		public void IncrementReadOnlyIncludingChildren()
		{
			inReadOnlyChangeIndex++;
			try
			{
				if (readOnlyIndex == 0)
				{
					if (IsCachePopulated())
					{
						for (int i = 0; i < Count; i++)
						{
							IBusinessObjectState child = this[i];
							child.IncrementReadOnlyIncludingChildren();
							if (disposing)
							{
								break;
							}
						}
						FireChangeEvents(ListChangedType.Reset, -1, -1);
					}
				}
				readOnlyIndex++;
			}
			finally
			{
				inReadOnlyChangeIndex--;
			}
		}

		public void ResumeValidation_NextPossibleOpportunity()
		{
			resumeValidationPending = true;
		}

		public void DecrementReadOnlyIncludingChildren_NextPossibleOpportunity()
		{
			decrementReadOnlyIncludingChildrenPending = true;
		}

		public void DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
			inReadOnlyChangeIndex++;
			try
			{
				readOnlyIndex--;
				if (readOnlyIndex == 0)
				{
					if (IsCachePopulated())
					{
						for (int i = 0; i < Count; i++)
						{
							IBusinessObjectState child = this[i];
							child.DecrementReadOnlyIncludingChildren(decrementToZero);
						}
						FireChangeEvents(ListChangedType.Reset, -1, -1);
					}
				}
				if (decrementToZero && readOnlyIndex > 0)
				{
					DecrementReadOnlyIncludingChildren(true);
				}
			}
			finally
			{
				inReadOnlyChangeIndex--;
			}
		}

		#endregion

		#region Implementation

		DataRow lastNewRowFromAddNew;
		int readOnlyIndex;
		int inReadOnlyChangeIndex;
		bool inCancelEdit;
		bool disposing;

		protected DataTable Table
		{
			get
			{
				if (table == null)
				{
					string tableName = BusinessObjectFactory.GetTableNameFromType(ElementType);
					table = Factory.RowFactory.GetTable(tableName);
					table.RowDeleting += new DataRowChangeEventHandler(Table_RowDeleting);
				}
				return table;
			}
		}
		DataTable table;

		protected virtual void Table_RowDeleting(object sender, DataRowChangeEventArgs e)
		{
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				bool shouldFire = false;
				if (!hasChanges.HasValue || (bool)hasChanges || HasChangesFromDelete)
				{
					shouldFire = true;
				}
				hasChanges = false;
				hasChangesFromDelete = false;
				if (shouldFire)
				{
					OnHasChangesChanged(HasChangesChangedEventArgs.Create(false, this));
				}
			}
		}

		protected virtual void Relationship_RelationshipFilterChanged(object sender, EventArgs e)
		{
			if (!InAddNewUncommitted || !IsAdhocRelationship)
			{
				InvalidateCacheHard();
			}
		}

		protected virtual void InvalidateCacheHard(bool forceNewDataView = false, bool fireResetEvent = true)
		{
			completeFilter = null;

			InvalidateCache();

			if (fireResetEvent)
			{
				FireChangeEvents(ListChangedType.Reset, -1, -1);
			}
		}

		internal bool IsCachePopulated()
		{
			return list != null;
		}

		internal event EventHandler Rebuilt;

		protected void OnRebuilt(EventArgs e)
		{
			if (Rebuilt != null)
			{
				Rebuilt(this, e);
			}
		}

		ZQuery AdditionalFilterIgnoreActiveFilter
		{
			get
			{
				if (additionalFilterIgnoreActiveFilter == null)
				{
					additionalFilterIgnoreActiveFilter = AdditionalFilter.DeepClone();
					additionalFilterIgnoreActiveFilter.IgnoreActiveFilter = true;
					additionalFilterIgnoreActiveFilter.ModificationsEnabled = false;
				}
				return additionalFilterIgnoreActiveFilter;
			}
		}
		ZQuery additionalFilterIgnoreActiveFilter;

		bool IActiveBusinessObjectCollectionIndex.MatchesFilter(BusinessObject businessObject, bool fetchOnlyFromLocalCache)
		{
			return MatchesFilter(businessObject as T, fetchOnlyFromLocalCache);
		}

		protected bool MatchesFilter(BusinessObject businessObject)
		{
			return MatchesFilter(businessObject, false);
		}

		protected bool MatchesFilter(BusinessObject businessObject, bool fetchOnlyFromLocalCache)
		{
			bool result = false;
			if (businessObject != null && !businessObject.IsDeleted)
			{
				var activeOwner = ActiveOwner;
				if (activeOwner != null && IsMatchesFilterOverridden)
				{
					if (businessObject is T element)
					{
						result = activeOwner.MatchesFilter(element, fetchOnlyFromLocalCache);
					}
					else
					{
						result = activeOwner.MatchesFilterIndex(businessObject, fetchOnlyFromLocalCache);
						if (result)
						{
							var correctedTypeBizo = GetBusinessObjectFromRow(businessObject.Row); // We get the type now, so if there is a type-decider conflict we still crash.
							result = activeOwner.MatchesFilterExcludingBaseBehaviour(correctedTypeBizo);
						}
					}
				}
				else
				{
					result = MatchesFilterBaseBehaviour(businessObject, fetchOnlyFromLocalCache);
				}
			}
			return result;
		}

		protected bool IsMatchesFilterOverridden
		{
			get { return isMatchesFilterOverridden ?? (bool)(isMatchesFilterOverridden = ActiveBusinessObjectCollection<T>.IsMatchesFilterOverridden(CollectionType)); }
		}

		Guid IActiveBusinessObjectCollectionIndex.ID => id;

		bool? isMatchesFilterOverridden;

		internal bool MatchesFilterBaseBehaviour(BusinessObject businessObject, bool fetchOnlyFromLocalCache)
		{
			bool result = true;
			if (!IsNonCommittedElement(businessObject))
			{
				ZQuery query = new ZQuery(Relationship.RelationshipFilter.IgnoreActiveFilter ? AdditionalFilterIgnoreActiveFilter : AdditionalFilter);
				if (query.IsDBOnlyQuery)
				{
					var queryWithRelationshipFilter = new ZQuery(query, Relationship.RelationshipFilter);
					if (Factory.RowFactory.IsDbOnlyQueryCached(businessObject.TableName, queryWithRelationshipFilter))
					{
						query = queryWithRelationshipFilter;
					}
				}
				query.FetchOnlyFromLocalCache |= fetchOnlyFromLocalCache;

				result = (businessObject.Row.RowState != DataRowState.Detached &&
					businessObject.MatchesFilter(query) &&
					Relationship.MatchesRelationshipFilter(businessObject, AdditionalFilter.IgnoreActiveFilter, fetchOnlyFromLocalCache));
			}
			return result;
		}

		protected void DecrementReadOnlyIncludingChildrenAndResumeValidationIfRequired()
		{
			if (decrementReadOnlyIncludingChildrenPending)
			{
				decrementReadOnlyIncludingChildrenPending = false;
				DecrementReadOnlyIncludingChildren(false);
			}
			if (resumeValidationPending)
			{
				resumeValidationPending = false;
				ResumeValidation();
			}
		}

		public void InvalidateCache()
		{
			List = null;
			InvalidateCacheCore();
		}

		protected virtual void InvalidateCacheCore()
		{
		}

		protected T LoadWithExtraExceptionHandling(ZGuid pk) => AddExtraExceptionDetailsIfNeeded(() => (T)Factory.Load(ElementType, pk));

		protected T AddExtraExceptionDetailsIfNeeded(Func<T> create)
		{
			try
			{
				return create();
			}
			catch (ApplicationException ex) when (ex.Message.StartsWith((NoResString)"Attempted to return a "))
			{
				var message = string.Format(CultureInfo.InvariantCulture,
					(NoResString)"Error has occur while creating Business object in {0} (ElementType:{1}, Complete Filter:{2})",
					GetType().FullName,
					ElementType.FullName,
					CompleteFilter.LiteralTextADO);

				throw new ApplicationException(message, ex);
			}
		}

		protected T GetBusinessObjectFromRow(DataRow row) => AddExtraExceptionDetailsIfNeeded(() => (T)Factory.CreateBusinessObject(row, ElementType, typeDeciderContext: Relationship?.Master as ITypeDeciderContext));

		protected BusinessObject GetBusinessObjectFromRowSafe(DataRow row)
		{
			return Factory.GetBizOsForDataRow(row)?.MaxBySafe(b => b is T) ?? GetBusinessObjectFromRow(row);
		}

		protected int InsertSorted(BusinessObjectList list, T businessObject, bool throwOnAlreadyExists = true)
		{
			int index;
			OnLoadingIntoCollection(businessObject);
			if (UniqueBusinessObjectComparer == null)
			{
				list.Add(businessObject);
				index = list.Count - 1;
			}
			else
			{
				index = list.BinarySearch(businessObject, UniqueBusinessObjectComparer);
				if (index >= 0)
				{
					if (index < list.Count)
					{
						BusinessObject existingBusinessObject = list[index];
						if (businessObject.PK == existingBusinessObject.PK)
						{
							if (throwOnAlreadyExists)
							{
								throw new InvalidOperationException(String.Format("BusinessObject already exists in list. BusinessObject Type: {0}, DataView Filter: {1}, DataView Sort Expression: {2}", businessObject.GetType().FullName, CompleteAdoReductionFilter, GetDataViewSortExpression()));
							}
							index = -1;
						}
						else
						{
							if (throwOnAlreadyExists)
							{
								StringBuilder errorMessageBuilder = new StringBuilder(
									String.Format((NoResString)"The binary search could not differentiate the following two business objects using the SortComparer {0}:",
									SortComparer != null ? SortComparer.GetType().ToString() : "N/A"));
								errorMessageBuilder.AppendLine();
								foreach (BusinessObject duplicateBusinessObject in new BusinessObject[] { businessObject, existingBusinessObject })
								{
									errorMessageBuilder.AppendFormat((NoResString)"Type: {0}, PK: {1}, ", duplicateBusinessObject.GetType(), duplicateBusinessObject.PK);
									DataRow row = duplicateBusinessObject.Row;
									if (row.HasVersion(DataRowVersion.Current))
									{
										foreach (DataColumn column in row.Table.Columns)
										{
											errorMessageBuilder.AppendFormat("{0}: {1}, ", column.ColumnName, row[column, DataRowVersion.Current]);
										}
									}
									else
									{
										errorMessageBuilder.AppendFormat((NoResString)"Current values not available");
									}
									errorMessageBuilder.AppendLine();
								}
								ErrorReporter.ReportOnce(errorMessageBuilder.ToString());
							}
							list.Insert(index, businessObject);
						}
					}
					else
					{
						throw new InvalidOperationException("Unexpected index out of range.");
					}
				}
				else
				{
					index = -index - 1;
					list.Insert(index, businessObject);
				}
			}
			return index;
		}

		protected virtual string GetDataViewSortExpression()
		{
			return string.Empty;
		}

		public IEnumerable<BusinessObject> GetMatchingBusinessObjects(IEnumerable<BusinessObject> bizos)
		{
			if (bizos != null && bizos.Any())
			{
				if (!CompleteFilter.IsDBOnlyQuery)
				{
					return bizos.Where(b => MatchesFilter(b as T, false));
				}
				else
				{
					try
					{
						var filter = CompleteFilter.DeepClone();
						filter.AllowTableValuedParameters = true;
						filter.AddToFilter(bizos.First().PKSchemaColumn, bizos.Select(b => b.PK));
						return Factory.Load<T>(filter);
					}
					catch (SqlException ex) when (ex.Number == 8623)
					{
						var results = Factory.Load<T>(CompleteFilter);
						var validPKs = bizos.Select(b => b.PK).ToHashSet();
						return results.Where(x => validPKs.Contains(x.PK));
					}
				}
			}
			return Enumerable.Empty<BusinessObject>();
		}

		void IActiveBusinessObjectCollectionIndex.OnFactorySaved(bool savedSuccessfully)
		{
			Factory_Saved(Factory, savedSuccessfully);
		}

		#endregion
	}
}
