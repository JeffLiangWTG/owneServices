using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

#if DEBUG
using NUnit.Framework;
#endif

namespace CargoWise.EntityFramework
{
	#region Count Changed

	public class CollectionCountChangedEventArgs : EventArgs
	{
		public CollectionCountChangedEventArgs(bool itemAdded, BusinessObject bizObject)
		{
			fItemAdded = itemAdded;
			fBizObject = bizObject;
		}

		public bool ItemAdded
		{
			get { return fItemAdded; }
		}

		public bool ItemRemoved
		{
			get { return !ItemAdded; }
		}

		public BusinessObject BizObject
		{
			get { return fBizObject; }
		}

		readonly bool fItemAdded;
		readonly BusinessObject fBizObject;
	}

	public delegate void CollectionCountChangedEventHandler(object sender, CollectionCountChangedEventArgs e);

	#endregion

	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection]
	public abstract class BusinessObjectCollection<T> : BusinessObjectCollection where T : BusinessObject
	{
		protected BusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected BusinessObjectCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
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

		public IOrderedEnumerable<T> OrderBy<TKey>(Func<T, TKey> keySelector)
		{
			return this.Cast<T>().OrderBy(keySelector);
		}

		public IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
		{
			foreach (T item in this)
			{
				yield return selector(item);
			}
		}

		public IEnumerable<T> Where(Func<T, bool> predicate)
		{
			foreach (T item in this)
			{
				if (predicate(item))
				{
					yield return item;
				}
			}
		}

		public T this[int i]
		{
			get { return (T)Elements[i]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public new T AddNew(Type bizObjType)
		{
			return (T)base.AddNew(bizObjType);
		}
	}

	/// <summary>
	/// Obsolete. Consider using ActiveBusinessObjectCollection as an alternative.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[PropertyDescriptorCollection(typeof(BusinessObjectPropertyDescriptorCollection))]
	[DebuggerDisplay("Count = {Count}")]
	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection]
	public abstract partial class BusinessObjectCollection :
		ZCustomTypeDescriptor,
		IBusiness,
		INeedTable,
		INeedDataSet,
		IFindBoxListProvider,
		IFindBoxListProviderEx,
		IFindBoxListProviderDescriptionEx,
		IFilterBusinessObjectDefaultsProvider,
		IBusinessObjectCollectionInternals,
		ILegacyBusinessObjectCollectionInternals,
		IBusinessObjectCollection,
		IEnumerable<BusinessObject>,
		IBusinessObjectCollectionTestingMembers,
		IBindingListView,
		IBindingTracked,
		ICancelAddNew,
		ISupportMaxCountValidation,
		ICanSuspendSettingHasChanges
	{
		#region Construction

		protected BusinessObjectCollection(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		protected BusinessObjectCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
		{
			SetupWithFactory(factory, additionalFilter == null ? null : new ZQuery(additionalFilter));
#if DEBUG
			EnsureIsNotNewUsageOfThisObsoleteCollection();
#endif
		}

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		void SetupWithFactory(BusinessObjectFactory factory, ZQuery additionalFilter)
		{
#if DEBUG
			_Instance = ZGuid.NewZGuid();
#endif
			lastChangeNumber = 0;
			this.factory = factory;
			if (additionalFilter != null)
			{
				this.additionalFilter = additionalFilter;
			}
		}

		public virtual void AddGuidListMapping(string propertyName, string listName)
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

		protected virtual ZDataTable Table
		{
			get { return Factory.RowFactory.GetTable(((IBusiness)this).TableName); }
		}

		#endregion

		#region Type of Elements

		/// <summary>
		/// Consider using the indexer to specify the type of elements instead of overriding this.
		/// Binding uses the indexer and doesn't consider the value of this property.
		/// </summary>
		public virtual Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			if (fTypeOfElements == null)
			{
				fTypeOfElements = GetElementTypeFromCollectionType(GetType());
			}
			return fTypeOfElements;
		}

		public Type TypeOfElements
		{
			get { return GetTypeOfElementsFromPK(ZGuid.Empty); }
		}

		Type fTypeOfElements;

		public static Type GetElementTypeFromCollectionType(Type typeOfBusinessObjectCollection)
		{
			return GetGenericBusinessObjectCollectionInterfaceType(typeOfBusinessObjectCollection) ??
				GetIndexerPropertyInfoFromCollectionType(typeOfBusinessObjectCollection)?.PropertyType;
		}

		static Type GetGenericBusinessObjectCollectionInterfaceType(Type typeOfBusinessObjectCollection)
		{
			if (typeOfBusinessObjectCollection.IsInterface)
			{
				var genericBusinessObjectCollectionInterfaceType = typeOfBusinessObjectCollection.GetInterfaces()
					.FirstOrDefault((Type type) =>
						type.IsGenericType &&
						type.GetGenericTypeDefinition() == typeof(IBusinessObjectCollection<>)
					);
				if (genericBusinessObjectCollectionInterfaceType != null)
				{
					return genericBusinessObjectCollectionInterfaceType.GetGenericArguments().SingleOrDefault();
				}
			}

			return null;
		}

		static PropertyInfo GetIndexerPropertyInfoFromCollectionType(Type typeOfBusinessObjectCollection)
		{
			PropertyInfo info = PropertyInfoFetcher.GetFromLowestSubclassCache(typeOfBusinessObjectCollection, (NoResString)"Item");
			if (info == null)
			{
				if (!typeof(IBusinessObjectCollection).IsAssignableFrom(typeOfBusinessObjectCollection))
				{
					throw new ArgumentException(String.Format("This method only works for subclasses of BusinessObjectCollection or DynamicBusinessObjectCollection. Type = {0}", typeOfBusinessObjectCollection.ToString()), nameof(typeOfBusinessObjectCollection));
				}
				else if (typeof(BusinessObjectCollection) == typeOfBusinessObjectCollection)
				{
					throw new ArgumentException("This method does not work for BusinessObjectCollection itself because the indexer is defined in its subclasses", nameof(typeOfBusinessObjectCollection));
				}
				info = PropertyInfoFetcher.GetFromLowestSubclass(typeOfBusinessObjectCollection, (NoResString)"Item");
			}
			return info;
		}

		#endregion

		#region Default Values

		/// <summary>
		/// Set defaults for new elements based on collection state (eg, highest line number in collection).
		/// </summary>
		/// <param name="child">Set the values in this new element that is about to be added to the collection.</param>
		protected virtual internal void SetDefaultsForNewChild(BusinessObject child)
		{
		}

		#endregion

		#region Filter

		/// <summary>
		/// The whole filter on the collection. Combination of the RelationshipFilter and 
		/// the AdditionalFilter.
		/// </summary>
		public ZQuery CompleteFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				if (RelationshipFilter != null && !RelationshipFilter.IsEmpty)
				{
					filter.AddToFilter(RelationshipFilter);
				}
				if (AdditionalFilter != null)
				{
					filter.AddToFilter(AdditionalFilter);
				}
				if (AdditionalRelationshipFilter != null)
				{
					filter.AddToFilter(AdditionalRelationshipFilter);
				}
				filter.ModificationsEnabled = false;
				return filter;
			}
		}

		ZQuery ILegacyBusinessObjectCollectionInternals.RelationshipFilter
		{
			get { return RelationshipFilter; }
		}

		ZQuery IBusinessObjectCollection.RelationshipFilter
		{
			get { return RelationshipFilter; }
		}

		/// <summary>
		/// The filter that defines the relationship that the collection 
		/// represents. Eg, In dependent collection, this is having a valid 
		/// FK to the master.
		/// </summary>
		protected internal ZQuery RelationshipFilter
		{
			get
			{
				ZQuery result = CreateRelationshipFilter();
				if (result != null)
				{
					result.ModificationsEnabled = false;
				}

				return result;
			}
		}

		ZQuery ILegacyBusinessObjectCollectionInternals.AdditionalFilter
		{
			get { return AdditionalFilter; }
		}

		/// <summary>
		/// A settable and overridable extra filter on the collection for loading.
		/// </summary>
		protected internal ZQuery AdditionalFilter
		{
			get
			{
				ZQuery result = CreateAdditionalFilter();
				if (result != null)
				{
					result.ModificationsEnabled = false;
				}
				return result;
			}
		}

		void ILegacyBusinessObjectCollectionInternals.SetOverriddenAdditionalFilter(ZQuery filter)
		{
			SetOverriddenAdditionalFilter(filter);
		}

		void SetOverriddenAdditionalFilter(ZQuery filter)
		{
			additionalFilter = filter;
			OnCompleteFilterChange(CompleteFilter);
		}

		protected virtual ZQuery CreateRelationshipFilter()
		{
			return new ZQuery();
		}

		protected virtual ZQuery CreateAdditionalFilter()
		{
			return additionalFilter != null ? additionalFilter.ShallowClone() : new ZQuery();
		}

		ZQuery additionalFilter;

		public ZString GetAllNotificationsWhenAdditionalFilterNotMet(BusinessObject selectedBusinessObject)
		{
			ZString result = "";

			if (!fOverriddenNotificationWhenAdditionalFilterNotMet.IsEmpty)
			{
				result = fOverriddenNotificationWhenAdditionalFilterNotMet;
			}
			else
			{
				StringCollectionX errors = new StringCollectionX();
				AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
				if (errors.Count == 0)
				{
					errors.Add(Res.GetString("b7e2e0cb-3059-469c-b588-c93294d1bf0c", "This {0} cannot be chosen here. Please choose another {0}.", GetRecordDescription(selectedBusinessObject)));
				}

				result = string.Join("\r\n", errors.ToArray());
			}

			return result;
		}

		string GetRecordDescription(BusinessObject businessObject)
		{
			return businessObject != null ? businessObject.HumanReadableName.ToString() : Res.GetString("959419b2-3cce-4068-b766-4552609c898c", "record");
		}

		/// <summary>
		/// Add any messages to show when the item is visible in the collection but cannot be chosen, 
		/// based on the AdditionalFilter specified on the collection
		/// </summary>
		/// <param name="notifications">Collection of all existing notifications</param>
		/// <param name="selectedBusinessObject">The business object that does not match the Additional Filter</param>
		protected virtual void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
		}

		public void SetOverrideNotificationWhenAdditionalFilterNotMet(ZString notificationMessage)
		{
			fOverriddenNotificationWhenAdditionalFilterNotMet = notificationMessage;
		}
		ZString fOverriddenNotificationWhenAdditionalFilterNotMet;

		public INotificationType GetNotificationTyoeWhenAdditionalFilterNotMet()
		{
			return fOverriddenNotificationTypeWhenAdditionalFilterNotMet ?? NotificationType.Error;
		}

		public void SetOverrideNotificationTypeWhenAdditionalFilterNotMet(INotificationType notificationType)
		{
			fOverriddenNotificationTypeWhenAdditionalFilterNotMet = notificationType;
		}
		INotificationType fOverriddenNotificationTypeWhenAdditionalFilterNotMet;

		ZQuery ILegacyBusinessObjectCollectionInternals.AdditionalRelationshipFilter
		{
			get { return AdditionalRelationshipFilter; }
		}

		internal ZQuery AdditionalRelationshipFilter
		{
			get { return additionalRelationshipFilter; }
			set
			{
				additionalRelationshipFilter = value;
				OnCompleteFilterChange(CompleteFilter);
			}
		}
		ZQuery additionalRelationshipFilter;

		public bool LoadedFilterMatches(ZQuery newFilterForLoad)
		{
			return LastLoadedAdditionalFilter != null && newFilterForLoad != null && LastLoadedAdditionalFilter.LiteralTextADO == newFilterForLoad.LiteralTextADO;
		}

		#endregion

		#region Running validation

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

		internal int ValidationIndex;

		public void SuspendValidation()
		{
			ValidationIndex++;
#if DEBUG
			ValidationHasBeenSuspended = true;
#endif
			foreach (IBusiness child in this)
			{
				child.SuspendValidation();
			}
		}

#if DEBUG
		public bool ValidationHasBeenSuspended;
#endif

		public void ResumeValidation()
		{
			ValidationIndex--;
			foreach (IBusiness child in this)
			{
				child.ResumeValidation();
			}
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

		public bool IsValidationSuspended
		{
			get { return !IgnoreValidationSuspended && (ValidationIndex > 0 || (Factory != null && Factory.IsValidationSuspended)); }
		}

		#region PreFetch Management

		public IBusinessObjectCollectionFetchStrategy FetchStrategy
		{
			get { return GetFetchStrategy(); }
		}

		protected virtual IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new BusinessObjectCollectionFetchStrategy(this);
		}

		void IBusiness.RunPreSaveValidationFetch(bool executeHints)
		{
			FetchStrategy.FetchForValidate();
			foreach (IBusiness element in this)
			{
				element.RunPreSaveValidationFetch(false);
			}
			if (executeHints && Factory != null)
			{
				Factory.ExecuteAllFetchHints();
			}
		}

		void RunFetchForSort(params PropertyDescriptor[] propertyDescriptors)
		{
			if (Count > 0)
			{
				var tableColumnsArray = BizoPropertiesToTableColumnsCalculator.GetBusinessObjectTableColumns(propertyDescriptors);
				if (tableColumnsArray.Length > 0)
				{
					FetchStrategy.FetchForView(this.ToArray<BusinessObject>(), tableColumnsArray);
					foreach (BusinessObject bizo in this)
					{
						bizo.FetchStrategy.FetchForView(tableColumnsArray);
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Runs PreSave validation for every element of the collection.
		/// </summary>
		public void RunPreSaveValidation()
		{
			int lastNotificationChangeCount = BusinessObject.NotificationChangeCount;
			try
			{
				preSaveValidationDepthCount++;
				bool validatedSuccessfully = false;
				while (!validatedSuccessfully)
				{
					validatedSuccessfully = RunPreSaveValidationCore();
				}
				MaxCountValidator.Refresh();
			}
			finally
			{
				preSaveValidationDepthCount--;
			}
			if (lastNotificationChangeCount != BusinessObject.NotificationChangeCount)
			{
				OnNotificationsChanged(new NotificationsChangedEventArgs(this));
			}
		}

		protected virtual bool RunPreSaveValidationCore()
		{
			bool moveNext;
			IEnumerator<BusinessObject> enumerator = ((IEnumerable<BusinessObject>)Elements).GetEnumerator();
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
				if (enumerator.Current != null)
				{
					enumerator.Current.RunPreSaveValidation();
				}
			}
			while (moveNext);
			return true;
		}

		#endregion

		#region Object State Members

		#region Count

		/// <summary>
		/// Number of elements in the collection.
		/// </summary>
		public int Count
		{
			get { return Elements.Count; }
		}

		/// <summary>
		/// 	 Need a new COUNT because there's a bug in ZArch.GUI. The GB mawbforms's houses grid is bound to CusMAWB.ChildBills.
		///		 If you open a form and select the delivery/receipt tab page immediately, the GUI fires an OnEnter event for the houses grid (the tab that's shown first by default), 
		///		 then fires OnEnter for the receipt/delivery grid (the OutTurns collection), and finally fires OnLeave for the houses grid. 
		///		 During the house grid's OnEnter a new uncommitted row is added, and then removed during OnLeave.  But since the OutTurns collection
		///		 member on the mawb is touched for binding on the delivery/receipt grid's OnEnter event, which fires BEFORE the house grid's OnLeave, 
		///		 it means that when CusMAWB.OutTurns looks at CusMAWB.ChildBills it sees the uncommitteed row.  Totally lame. 
		///		 You can avoid it by selecting another tab page (e.g. messages) before selecting Receipt/Delivery), or this will fix it too.  
		///		 We exclude the uncommitteed row. 
		/// </summary>
		public int CountExcludingUnCommitted
		{
			get
			{
				int counter = 0;
				foreach (IBusinessObjectInternals item in this)
				{
					if (!item.IsUnCommittedRow)
					{
						counter++;
					}
				}
				return counter;
			}
		}

		/// <summary>
		/// This is fired when the number of elements in the collection has changed.
		/// </summary>
		public event CollectionCountChangedEventHandler CountChanged;

#if DEBUG
		public bool CountChangedIsNull_DebugOnly
		{
			get { return CountChanged == null; }
		}
#endif

		#region Implementation

		void UpdateCountOnAdded(BusinessObject bizO)
		{
			bool itemAdded = true;
			UpdateCount(itemAdded, bizO);
		}

		void UpdateCountOnRemoved(BusinessObject bizO)
		{
			bool itemAdded = false;
			UpdateCount(itemAdded, bizO);
		}

		void UpdateCount(bool itemAdded, BusinessObject bizO)
		{
			CollectionCountChangedEventArgs args = new CollectionCountChangedEventArgs(itemAdded, bizO);

			var countChangedWhileSuspended = CountChangedWhileSuspended;
			if (countChangedWhileSuspended != null)
			{
				countChangedWhileSuspended(this, args);
			}
			else
			{
				OnCountChanged(args);
			}
		}

		protected virtual void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			if (CountChanged != null)
			{
				CountChanged(this, e);
			}
		}

		event CollectionCountChangedEventHandler CountChangedWhileSuspended;

		public IDisposable SuspendCountChanged(Action<IEnumerable<CollectionCountChangedEventArgs>> suspendedEventsProcessor)
		{
			return new CountChangedSuspender(this, suspendedEventsProcessor);
		}

		#region CountChangedSuspender

		class CountChangedSuspender : IDisposable
		{
			public CountChangedSuspender(BusinessObjectCollection collection, Action<IEnumerable<CollectionCountChangedEventArgs>> suspendedEventsProcessor)
			{
				this.collection = collection;
				this.suspendedEventsProcessor = suspendedEventsProcessor;
				collection.CountChangedWhileSuspended += CountChangedWhileSuspendedHandler;

				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			readonly BusinessObjectCollection collection;
			readonly Action<IEnumerable<CollectionCountChangedEventArgs>> suspendedEventsProcessor;
			readonly List<CollectionCountChangedEventArgs> suspendedCountChangedEventArgs = new List<CollectionCountChangedEventArgs>();
			bool isDisposed;

			void CountChangedWhileSuspendedHandler(object sender, CollectionCountChangedEventArgs e)
			{
				suspendedCountChangedEventArgs.Add(e);
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					try
					{
						collection.CountChangedWhileSuspended -= CountChangedWhileSuspendedHandler;
						if (suspendedEventsProcessor != null && suspendedCountChangedEventArgs.Count > 0)
						{
							suspendedEventsProcessor(suspendedCountChangedEventArgs);
						}
					}
					finally
					{
						DisposableLeakListener.Instance.UnRegisterDisposable(this);
						isDisposed = true;
					}
				}
			}
		}

		#endregion

		#endregion

		#endregion

		#region Has Changes

		/// <summary>
		/// Does the collection have any changes that should be written to the Database?
		/// </summary>
		public event EventHandler<HasChangesChangedEventArgs> HasChangesChanged;

		protected virtual bool HasChangesCore
		{
			get
			{
				bool result = ((IBusinessObjectCollectionInternals)this).HasChangesFromDelete;

				if (!result)
				{
					foreach (var bO in Elements)
					{
						if (bO.HasChanges)
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
			set
			{
				if (!IsSettingHasChangesSuspended)
				{
					foreach (var bO in Elements)
					{
						bO.HasChanges = value;
					}

					if (!value)
					{
						((IBusinessObjectCollectionInternals)this).HasChangesFromDelete = false;
					}
				}
			}
		}

		public bool IsSettingHasChangesSuspended
		{
			get { return settingHasChangesIndex > 0; }
		}

		int settingHasChangesIndex;

		public IDisposable SuspendSettingHasChanges()
		{
			settingHasChangesIndex++;
			return new DisposableAction(() => settingHasChangesIndex--);
		}

		public
#if DEBUG
		virtual
#endif
		bool HasChanges
		{
			get { return HasChangesCore; }
			set { HasChangesCore = value; }
		}

		#region Implementation

		bool fHasChangesFromDelete;

		bool IBusinessObjectCollectionInternals.HasChangesFromDelete
		{
			get { return HasChangedFromDeleteCore; }
			set { HasChangedFromDeleteCore = value; }
		}

		protected virtual bool HasChangedFromDeleteCore
		{
			get { return fHasChangesFromDelete; }
			set
			{
				if (!IsSettingHasChangesSuspended)
				{
					if (value && Factory != null)
					{
						lastChangeNumber = Factory.GetNextChangeNumber();
					}
					fHasChangesFromDelete = value;
					UpdateHasChanges(HasChangesChangedEventArgs.Create(value, this));
				}
			}
		}

		void UpdateHasChanges(HasChangesChangedEventArgs e)
		{
			if (HasChangesChanged != null)
			{
				HasChangesChanged(this, e);
			}
		}

		#endregion

		#endregion

		#region IsInDatabase

		bool IBusinessObjectState.IsInDatabase => false;

		/// <summary>
		/// Is a version of every element in the collection stored in the database?
		/// </summary>
		public virtual bool IsInDatabaseIncludingChildren
		{
			get
			{
				return this.All(element => !element.ShouldCheckIsInDatabase || element.IsInDatabaseIncludingChildren);
			}
		}

		#endregion

		void IBusinessObjectState.ClearHasChangesIncludingChildren()
		{
			foreach (IBusinessObjectState child in this)
			{
				child.ClearHasChangesIncludingChildren();
			}
		}

		#region RefreshBinding

		/// <summary>
		/// Calls RefreshBinding on this object and all its registered-editable children.
		/// </summary>
		public void RefreshBindingIncludingChildren()
		{
			RefreshBinding();

			foreach (IBusiness child in this.ToArray())
			{
				child.RefreshBindingIncludingChildren();
			}
		}

		public void RefreshBinding()
		{
			FireListResetEvent();
		}

		#endregion

		#region ReadOnly

		int readOnlyIndex;

		/// <summary>
		/// Gets a value indicating the ReadOnly status of the collection. To set the collection
		/// and its children's ReadOnly state, use SetReadOnlyIncludingChildren(bool ReadOnly).
		/// </summary>
		public virtual bool ReadOnly
		{
			get { return readOnlyIndex > 0; }
		}

		/// <summary>
		/// Sets the business object collection and all its children's .ReadOnly properties to the specified value.
		/// Note that read-only means the GUI becomes read-only; property values can still be modified.
		/// </summary>
		public void SetReadOnlyIncludingChildren(bool readOnly)
		{
			if (readOnly)
			{
				bool isCurrentlyReadOnly = readOnlyIndex > 0;
				if (readOnly != isCurrentlyReadOnly)
				{
					IncrementReadOnlyIncludingChildren();
				}
			}
			else
			{
				DecrementReadOnlyIncludingChildren(true);
			}
		}

		void IBusinessObjectState.IncrementReadOnlyIncludingChildren()
		{
			IncrementReadOnlyIncludingChildren();
		}

		void IncrementReadOnlyIncludingChildren()
		{
			using (SuspendListChanged())
			{
				foreach (IBusinessObjectState child in this)
				{
					child.IncrementReadOnlyIncludingChildren();
				}
			}
			readOnlyIndex++;
			if (readOnlyIndex == 1)
			{
				FireListResetEvent();
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
				using (SuspendListChanged())
				{
					foreach (IBusinessObjectState child in this)
					{
						child.DecrementReadOnlyIncludingChildren(decrementToZero);
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

		#endregion

		#region Child Notifications Change

		void HandleChildNotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			OnNotificationsChanged(e);
		}

		internal bool IsInPreSaveValidation
		{
			get { return preSaveValidationDepthCount > 0; }
		}

		event EventHandler<NotificationsChangedEventArgs> IBusinessObjectState.NotificationsChanged
		{
			add { notificationsChanged += value; }
			remove { notificationsChanged -= value; }
		}

		event EventHandler<NotificationsChangedEventArgs> notificationsChanged;

		bool CanRaiseNotificationsChanged()
		{
			return !IsValidationSuspended && !IsInPreSaveValidation && notificationsChanged != null;
		}

		void OnNotificationsChanged(NotificationsChangedEventArgs args)
		{
			if (CanRaiseNotificationsChanged())
			{
				notificationsChanged(this, args);
			}
		}

		EventHandler<NotificationsChangedEventArgs> ChildNotificationChangedHandler
		{
			get
			{
				if (childNotificationChangedHandler == null)
				{
					childNotificationChangedHandler = new EventHandler<NotificationsChangedEventArgs>(HandleChildNotificationsChanged);
				}
				return childNotificationChangedHandler;
			}
		}

		int preSaveValidationDepthCount;
		EventHandler<NotificationsChangedEventArgs> childNotificationChangedHandler;

		#endregion

		#region UpdatedByDataRefreshIncludingChildren

		void RaiseUpdatedByDataRefreshIncludingChildren()
		{
			if (fUpdatedByDataRefreshIncludingChildren != null)
			{
				fUpdatedByDataRefreshIncludingChildren(this, EventArgs.Empty);
			}
		}

		void HandleChildUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			RaiseUpdatedByDataRefreshIncludingChildren();
		}

		event EventHandler IBusinessObjectState.UpdatedByDataRefreshIncludingChildren
		{
			add
			{
				if (fUpdatedByDataRefreshIncludingChildren == null)
				{
					foreach (IBusiness element in this)
					{
#pragma warning disable
						element.UpdatedByDataRefreshIncludingChildren += HandleChildUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
					}
				}
				fUpdatedByDataRefreshIncludingChildren += value;
			}
			remove
			{
				fUpdatedByDataRefreshIncludingChildren -= value;
				if (fUpdatedByDataRefreshIncludingChildren == null)
				{
					foreach (IBusiness element in this)
					{
#pragma warning disable
						element.UpdatedByDataRefreshIncludingChildren -= HandleChildUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
					}
				}
			}
		}
		event EventHandler fUpdatedByDataRefreshIncludingChildren;

		#endregion

		#region IBusinessObjectCollectionInternals Members

		#region Masters Are In Database

		bool IBusinessObjectCollectionInternals.MastersAreInDatabase
		{
			get { return true; } // no masters
		}

		#endregion

		#region Masters Are Deleted

		bool IBusinessObjectCollectionInternals.MastersAreDeleted
		{
			get { return false; } // no masters 
		}

		#endregion

		#endregion

		void IBusinessObjectCollectionInternals.FireListResetEvent()
		{
			FireListResetEvent();
		}

		#endregion

		#region Load

		/// <summary>
		/// Loads collection using the collection's relationship filter plus collection's additional filter.
		/// Consider using Reload() instead if you are reloading data to avoid caching issues.
		/// </summary>
		public virtual void Load()
		{
			Load(AdditionalFilter);
		}

		/// <summary>
		/// Loads using the relationship filter plus the additional filter 
		/// plus the filter passed in to this function.
		/// </summary>
		public void LoadWithMoreFiltering(ZQuery subFilterToAddToAdditionalFilter) //TODO: Store this value to regurgitate for the data refresh bus
		{
			ZQuery query = new ZQuery();
			if (AdditionalFilter != null)
			{
				query.AddToFilter(AdditionalFilter);
			}

			query.AddToFilter(subFilterToAddToAdditionalFilter);
			query.MaximumRows = subFilterToAddToAdditionalFilter.MaximumRows;

			Load(query);
		}

		protected internal void OnAfterResort()
		{
			if (AfterResort != null)
			{
				AfterResort(this, new EventArgs());
			}
		}

		public event EventHandler AfterResort;

		/// <summary>
		/// Loads using the relationship filter plus the filter passed in to this function.
		/// All other filters are ignored.
		/// </summary>
		/// <param name="filter"></param>
		public virtual void Load(ZQuery filter)
		{
			WithLoad(filter, GetCompleteLoadFilter, f => AddItemsToCollectionForLoad(f));
		}

		/// <summary>
		/// So you loaded some stuff and now you want it to go into a bizo-collection?
		/// This method works like "Load" in that DataRefreshBus, Loaded, Fetch Hints and Sorting are supported properly.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="businessObjects"></param>
		public void SetLoadResult(ZQuery query, IEnumerable<BusinessObject> businessObjects)
		{
			WithLoad(query, f => f, f => AddRange(businessObjects));
		}

		void WithLoad(ZQuery filter, Func<ZQuery, ZQuery> getCompleteFilter, Action<ZQuery> addItemsToCollection)
		{
			loaded = true;
			loading = true;
			try
			{
				using (SuspendListChanged())
				{
					RemoveAllButLeaveRelationshipsIntact();

					var filter1 = getCompleteFilter(filter);
					LastLoadedAdditionalFilter = filter;

					OnCompleteFilterChange(filter1);
					addItemsToCollection(filter1);

					if (Sorter.IsSorted)
					{
						Sorter.Resort();
						OnAfterResort();
					}
					AddFetchHintsAfterLoad();
				}
				LastReloadTime = ZDateTime.Now;
			}
			finally
			{
				loading = false;
			}
			OnLoaded();
		}

		protected virtual void AddFetchHintsAfterLoad()
		{
		}

		protected virtual void AddItemsToCollectionForLoad(ZQuery filter)
		{
			if (filter.OrderBy == "" && filter.MaximumRows == 1)
			{
				BusinessObject top1 = Factory.LoadTop1(TypeOfElements, filter);
				if (top1 != null)
				{
					Add(top1);
				}
			}
			else
			{
				AddRange(Factory.Load(TypeOfElements, filter));
			}
		}

		protected ZQuery GetCompleteLoadFilter(ZQuery initialFilter)
		{
			ZQuery filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = FetchOnlyFromLocalCache;

			ZQuery relationshipFilter = RelationshipFilter;
			if (relationshipFilter != null && !relationshipFilter.IsEmpty)
			{
				filter.AddToFilter(relationshipFilter);
				filter.FetchOnlyFromLocalCache |= relationshipFilter.FetchOnlyFromLocalCache;
			}

			if (initialFilter != null)
			{
				if (!initialFilter.IsEmpty)
				{
					filter.AddToFilter(initialFilter);
					filter.MaximumRows = initialFilter.MaximumRows;
					filter.IsNoLock = initialFilter.IsNoLock;
					filter.ReLoadExistingRows = initialFilter.ReLoadExistingRows;
				}
				filter.IgnoreActiveFilter = initialFilter.IgnoreActiveFilter;
				filter.FetchOnlyFromLocalCache |= initialFilter.FetchOnlyFromLocalCache;
				filter.IncludeBlob(initialFilter.LoadWithBlobs);
				filter.IsUnionQuery = initialFilter.IsUnionQuery;
				filter.LoadSmallBlobs = initialFilter.LoadSmallBlobs;
			}
			OnCompleteFilterChange(filter);
			return filter;
		}

		/// <summary>
		/// Estimates the count that would be returned if Load were called with the given filter. Load may actually return a different count. 
		/// This method should be faster than doing a Load, so that callers can choose whether or not to proceed with a Load based on 
		/// the result of this method.
		/// </summary>
		public virtual int GetEstimatedLoadCount(ZQuery initialFilter)
		{
			ZQuery filter = GetCompleteLoadFilter(initialFilter);
			return Factory.GetDatabaseCount(TypeOfElements, filter);
		}

		/// <summary>
		/// Overridable method that is fired after loading the collection from data every time Load is called.
		/// </summary>
		protected virtual void OnLoaded()
		{
		}

		/// <summary>
		/// True when the Collection is being loaded.
		/// </summary>
		public bool IsLoading
		{
			get { return loading; }
		}

		public bool IsLoaded
		{
			get { return loaded; }
			protected set { loaded = value; }
		}

		protected virtual bool FetchOnlyFromLocalCache
		{
			get { return false; }
		}

		bool IBusinessObjectCollectionInternals.HasChangesFromDatabase()
		{
			var query = GetFilterForReload();
			var bizos = new BusinessObjectFactory().Load(TypeOfElements, query);
			var oldInDbPKs = this.Where(x => x.IsInDatabase).Select(x => x.PK);
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

		public void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false)
		{
			var query = GetFilterForReload();
			var bizos = LoadFromDb(reLoadExistingRows, query).Concat(LoadFromLocalCache(query));
			ReloadWith(bizos, assumeRowsMissingFromQueryResultsAreDeleted);

			HasReloadedFromDb = true;
			LastReloadTime = ZDateTime.Now;
		}

		public void ReloadIfTimeout(bool reLoadExistingRows, int timeoutSecond, bool assumeRowsMissingFromQueryResultsAreDeleted = false)
		{
			if (LastReloadTime.IsEmpty || (LastReloadTime.AddSeconds(timeoutSecond) < ZDateTime.Now))
			{
				Reload(reLoadExistingRows, assumeRowsMissingFromQueryResultsAreDeleted);
			}
		}

		public void ReloadFromLocalCache() => AddRange(LoadFromLocalCache(GetFilterForReload()));

		ZQuery GetFilterForReload()
		{
			ZQuery query = new ZQuery(AdditionalFilter);
			query.AddToFilter(GetCompleteLoadFilter(new ZQuery()));
			return query;
		}

		BusinessObject[] LoadFromDb(bool reLoadExistingRows, ZQuery query)
		{
			var dbQuery = new ZQuery();
			dbQuery.AddToFilter(query);
			dbQuery.ReLoadExistingRows = reLoadExistingRows;
			dbQuery.IsDBOnlyQuery = true;

			return Factory.Load(TypeOfElements, dbQuery);
		}

		IEnumerable<BusinessObject> LoadFromLocalCache(ZQuery query)
		{
			var cacheQuery = new ZQuery();
			cacheQuery.AddToFilter(query);
			cacheQuery.FetchOnlyFromLocalCache = true;
			return Factory.Load(TypeOfElements, cacheQuery).Where(c => !c.IsInDatabase);
		}

		void ReloadWith(IEnumerable<BusinessObject> items, bool assumeRowsMissingFromQueryResultsAreDeleted)
		{
			using (SuspendListChanged())
			{
				AddRange(items);
				if (assumeRowsMissingFromQueryResultsAreDeleted)
				{
					var itemsToRemove = this.Except(items, new BizoEqualityComparer()).ToArray();
					if (itemsToRemove.Any())
					{
						RemoveRange(itemsToRemove);
						foreach (IBusiness item in itemsToRemove.Where(i => i.IsInDatabase))
						{
							item.DeleteForDataRefresh();
						}
					}
				}
			}
		}

		class BizoEqualityComparer : IEqualityComparer<BusinessObject>
		{
			bool IEqualityComparer<BusinessObject>.Equals(BusinessObject x, BusinessObject y) => x.PK == y.PK;
			int IEqualityComparer<BusinessObject>.GetHashCode(BusinessObject obj) => obj.PK.GetHashCode();
		}

		public bool HasReloadedFromDb { get; set; }

		ZDateTime LastReloadTime { get; set; }

		#endregion

		#region DataRefresh

		protected ZQuery LastLoadedAdditionalFilter
		{
			get { return lastLoadedAdditionalFilter; }
			set { lastLoadedAdditionalFilter = value; }
		}

		ZQuery lastLoadedAdditionalFilter;

		/// <summary>
		/// False by default. Set to True if you want your collection to be updated when 
		/// a new business object is saved to the same table in the database.
		/// </summary>
		public bool IsManagedForDataRefresh
		{
			get { return fIsManagedForDataRefresh ?? false; }
			set
			{
				if (Factory == null)
				{
					ErrorReporter.ReportOnce("IsManagedForDataRefresh", "You cannot set IsManagedForDataRefresh when Factory is null");
				}
				else
				{
					fIsManagedForDataRefresh = value;
					if (value)
					{
						Factory.StartManagingCollectionForAddingBusinessObjects(this);
					}
					else
					{
						Factory.StopManagingCollectionForAddingBusinessObjects(this);
					}
				}
			}
		}
		bool? fIsManagedForDataRefresh;

		protected bool IsManagedForDataRefreshSet
		{
			get { return fIsManagedForDataRefresh != null; }
		}

		/// <summary>
		/// Is this collection being updated by the DataRefreshBus at the moment?
		/// </summary>
		public bool IsRefreshingByDataRefreshBus
		{
			get { return IsDeletingForDataRefresh || IsUpdatingByDataRefreshBus || Factory.RefreshManager.IsRefreshing(this); }
		}

		protected bool IsDeletingForDataRefresh { get; private set; }

		internal void RemoveForDataRefresh(BusinessObject businessObject)
		{
			try
			{
				IsDeletingForDataRefresh = true;
				Remove(businessObject);
			}
			finally
			{
				IsDeletingForDataRefresh = false;
			}
		}

		#endregion

		#region Adding Elements

		/// <summary>
		/// Called whenever a new element has been added to the collection, either
		/// via the Grid or by calling Add.
		/// </summary>
		protected virtual void OnAdded(BusinessObject bizOAdded)
		{
			OnElementAdded(bizOAdded);
			UpdateCountOnAdded(bizOAdded);
		}

		void OnElementAdded(BusinessObject bizOAdded)
		{
			if (ElementAdded != null)
			{
				ElementAdded(bizOAdded);
			}
		}

		/// <summary>
		/// Add an existing BusinessObject to the collection.
		/// </summary>
		public virtual void Add(BusinessObject businessObject)
		{
			if (businessObject == null)
			{
				throw new ArgumentNullException(nameof(businessObject));
			}

			if ((!businessObject.IsDeleted && ElementCanBeAdded(businessObject) && !Contains(businessObject.PK)))
			{
				NotifyEnumerators();
				AddRowToDataTableIfNotNullAndDetached(businessObject);
				int elementIndex = AddToElements(businessObject);
				SetCollectionRelationships(businessObject);
				HookupElementToCollection(businessObject);

				if (HasChangesChanged != null && businessObject.HasChanges)
				{
					var factoryInternals = Factory as IBusinessObjectFactoryInternals;
					var sureHasChanges = factoryInternals != null && !factoryInternals.IsProcessingOnAllTransactionsCommitted;
					UpdateHasChanges(HasChangesChangedEventArgs.Create(sureHasChanges, this));
				}

				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, elementIndex));

				if (CanRaiseNotificationsChanged() && businessObject.HasNotifications())
				{
					OnNotificationsChanged(new NotificationsChangedEventArgs(businessObject));
				}

				OnAdded(businessObject);
				if (Factory != null)
				{
					Factory.InvalidateCachedProperties();
				}
			}
		}

		protected virtual bool ElementCanBeAdded(BusinessObject bizO)
		{
			return true;
		}

		void AddRowToDataTableIfNotNullAndDetached(BusinessObject bizo)
		{
			if (bizo.Row != null)
			{
				AddRowToDataTableIfDetached(bizo);
			}
		}

		// Is overriden for PRA Container Collection.
		protected virtual void AddRowToDataTableIfDetached(BusinessObject bizo)
		{
			var row = bizo.Row;
			if (row.RowState == DataRowState.Detached)
			{
				Table.Rows.Add(row);
				bizo.IsDataRowInDataTable = true;
			}
		}

		/// <summary>
		/// Adds multiple BusinessObjects to the collection.
		/// </summary>
		public void AddRange(params BusinessObject[] businessObjects)
		{
			if (businessObjects.Length > 0)
			{
				NotifyEnumerators();
			}
			AddRange((IEnumerable)businessObjects);
		}

		/// <summary>
		/// Adds all business objects from one collection to this one.
		/// </summary>
		public void AddRange(IEnumerable businessObjects)
		{
			using (SuspendListChanged())
			{
				OnAddingRange();

				foreach (BusinessObject bizObject in businessObjects)
				{
					Add(bizObject);
				}

				OnAddedRange(businessObjects);
			}
		}

		protected virtual void OnAddingRange()
		{
		}

		protected virtual void OnAddedRange(IEnumerable businessObjects)
		{
		}

		internal virtual int AddFromDataRefresh(IEnumerable<BusinessObject> bizos)
		{
			IsUpdatingByDataRefreshBus = true;
			var added = 0;
			try
			{
				var filter = (this as IBusinessObjectFilterFactory)?.NewBusinessObjectFilter();
				var completeFilter = CompleteFilter;
				using (SuspendListChanged())
				{
					foreach (var bizo in bizos)
					{
						if (bizo.MatchesFilter(completeFilter) && (filter?.IsMatching(bizo) ?? true))
						{
							Add(bizo);
							added++;
						}
					}

					if (added > 0)
					{
						OnAddedFromDataRefresh?.Invoke(this, EventArgs.Empty);
					}
				}
				return added;
			}
			finally
			{
				IsUpdatingByDataRefreshBus = false;
			}
		}

		protected bool IsUpdatingByDataRefreshBus;

		public event EventHandler OnAddedFromDataRefresh;

		/// <summary>
		/// Loads an element from the database and adds it to the collection, given a PK.
		/// Returns True if element was found and added. The element will only be added if
		/// it matches the complete filter of this collection.
		/// </summary>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
		internal bool AddFromDatabaseIfMatchLastLoadedFilter(params ZGuid[] pKs)
		{
			return AddFromDatabaseIfMatchLastLoadedFilter((IEnumerable<ZGuid>)pKs);
		}

		/// <summary>
		/// Loads an element from the database and adds it to the collection, given a PK.
		/// Returns True if element was found and added. The element will only be added if
		/// it matches the complete filter of this collection.
		/// </summary>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
		internal virtual bool AddFromDatabaseIfMatchLastLoadedFilter(IEnumerable<ZGuid> pKs)
		{
			return Factory.RefreshManager.PerformImmediateOrDeferredUpdate(() =>
			{
				int numLoaded = 0;
				if (IsLoaded && pKs.Any() && TypeOfElements != null)
				{
					IsUpdatingByDataRefreshBus = true;
					DataRefreshManager.Bus.SetIsRefreshing(this, true);

					try
					{
						var type = TypeOfElements;
						var bizos = new List<BusinessObject>();

						foreach (var pk in pKs)
						{
							try
							{
								var bizo = Factory.Load(type, pk);
								if (bizo != null)
								{
									bizos.Add(bizo);
								}
							}
							catch (NoConcreteTypeException)
							{
								// If the row cannot be loaded, that's ok.
							}
						}

						if (bizos.Any())
						{
							numLoaded += AddFromDataRefresh(bizos);
						}
					}
					finally
					{
						IsUpdatingByDataRefreshBus = false;
						DataRefreshManager.Bus.SetIsRefreshing(this, false);
					}
				}

				return numLoaded > 0;
			});
		}

		/// <summary>
		/// Loads an element from the database and adds it to the collection, given a PK.
		/// Returns True if element was found and added.
		/// </summary>
		public bool AddFromDatabase(ZGuid pK)
		{
			BusinessObject loaded = Factory.Load(GetTypeOfElementsFromPK(pK), pK);
			if (loaded != null)
			{
				Add(loaded);
			}

			return loaded != null;
		}

		/// <summary>
		/// Adds a newly created BusinessObject to the collection.
		/// </summary>
		/// <returns></returns>
		public BusinessObject AddNew()
		{
			return AddNewCore();
		}

		protected virtual BusinessObject AddNewCore()
		{
			BusinessObject newBizObj = CreateNewBusinessObject();

			if (newBizObj != null)
			{ SetupNewlyAddedObject(newBizObj); }
			return newBizObj;
		}

		BusinessObject ILegacyBusinessObjectCollectionInternals.CreateNewBusinessObject()
		{
			return CreateNewBusinessObject();
		}

		internal virtual BusinessObject CreateNewBusinessObject()
		{
			return CreateInitialisedBusinessObjectFromRow(Table.NewRow());
		}

		public BusinessObject AddNew(Type bizoType)
		{
			return AddNewCore(bizoType);
		}

		protected virtual BusinessObject AddNewCore(Type bizoType)
		{
			var pk = Guid.Empty;
			return AddNewCore(bizoType, pk);
		}

		protected BusinessObject AddNewCore(Type bizoType, Guid pk)
		{
			BusinessObject newBizObj = Factory.New(bizoType, pk);
			HookupElementToCollection(newBizObj);
			SetupNewlyAddedObject(newBizObj);
			return newBizObj;
		}

		protected internal void SetupNewlyAddedObject(BusinessObject newBizObj)
		{
			//moved up to here so it runs before we set defaults (that may cause validation)
			for (int indexer = 0; indexer < ValidationIndex; indexer++)
			{
				newBizObj.SuspendValidation();
			}

			using (newBizObj.SuspendSettingHasChanges())
			{
				SetCollectionRelationships(newBizObj);
				SetDefaultsForNewChild(newBizObj);
			}

			int elementIndex = AddToElements(newBizObj, withValidationSuspended: false);
			AddRowToDataTableIfNotNullAndDetached(newBizObj);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, elementIndex));
			OnAdded(newBizObj);
		}

		public void AddFromDataRefreshIfMatching(IEnumerable<object> publishedObjects)
		{
			var matcher = GetDataRefreshMatcher();
			if (IsLoaded && matcher != null)
			{
				var objectsToExport = publishedObjects.OfType<BusinessObject>()
					.Where(matcher.IsMatch)
					.GroupBy(bizo => bizo.Row.Table)
					.SelectMany(CopyIntoNewDataTable)
					.ToArray();
				if (objectsToExport.Any())
				{
					Factory.ThreadSentry.Post(EnqueueAddFromDataRefreshIfMatching, objectsToExport, FormattableString.Invariant($"{nameof(BusinessObjectCollection)}.{nameof(AddFromDataRefreshIfMatching)}"));
				}
			}
		}

		internal virtual IEnumerable<IGrouping<DataTable, ZGuid>> CopyIntoNewDataTable(IGrouping<DataTable, BusinessObject> group)
		{
			if (group.Any())
			{
				var table = group.Key.Clone();
#if NETFRAMEWORK
				group.DistinctBy(g => g.PK).ForEach(g => table.ImportRow(g.Row));
#else
				IEnumerableExtensions.DistinctBy(group, g => g.PK).ForEach(g => table.ImportRow(g.Row));
#endif
				yield return new Group<DataTable, ZGuid>(table, group.Select(s => s.PK).ToArray());
			}
		}

		protected class Group<TKey, TValue> : IGrouping<TKey, TValue>
		{
			internal Group(TKey key, TValue[] values)
			{
				Key = key;
				Values = values;
			}

			public TKey Key { get; }
			TValue[] Values { get; }

			public IEnumerator<TValue> GetEnumerator() => ((IEnumerable<TValue>)Values).GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		DataRefreshMatcher GetDataRefreshMatcher() => dataRefreshMatcher;

		DataRefreshMatcher dataRefreshMatcher;
		int completeFilterSuspender;

		protected void OnCompleteFilterChange(ZQuery filter)
		{
			if (completeFilterSuspender == 0)
			{
				dataRefreshMatcher = MakeDataRefreshMatcher(filter);
			}
		}

		protected IDisposable DelayOnCompleteFilterChange()
		{
			++completeFilterSuspender;
			return new DisposableAction(() =>
			{
				--completeFilterSuspender;
				OnCompleteFilterChange(CompleteFilter);
			});
		}

		protected virtual DataRefreshMatcher MakeDataRefreshMatcher(ZQuery filter)
		{
			return new DataRefreshMatcher(filter, _ => true);
		}

		protected class DataRefreshMatcher
		{
			internal DataRefreshMatcher(ZQuery filter, Func<object, bool> match)
			{
				Filter = filter;
				Match = match;
			}
			ZQuery Filter { get; }
			Func<object, bool> Match { get; }

			public bool IsMatch(BusinessObject publishedObject)
			{
				return publishedObject != null && publishedObject.MatchesFilter(Filter) && Match(publishedObject);
			}
		}

		void EnqueueAddFromDataRefreshIfMatching(object state)
		{
			var groups = (IGrouping<DataTable, ZGuid>[])state;

			foreach (var group in groups)
			{
				var table = group.Key;
				if (!((IBusinessObjectCollectionInternals)this).MastersAreDeleted)
				{
					var localTable = Factory.RowFactory.GetTable(table.TableName);
					foreach (DataRow row in table.Rows)
					{
						var keys = table.PrimaryKey.Select(k => row[k]).ToArray();
						var localRow = localTable.Rows.Find(keys);

						if (localRow == null && !keys.OfType<Guid>().Any(k => localTable.HasDeletedRowWithPK(k)))
						{
							localTable.ImportRow(row);
						}
					}
				}
			}

			foreach (var group in groups)
			{
				AddFromDatabaseIfMatchLastLoadedFilter(group);
				UpdateSubscriptionStats(true);
			}
		}

		partial void UpdateSubscriptionStats(bool result);

		#region Implementation

		protected virtual BusinessObject CreateInitialisedBusinessObjectFromRow(DataRow row)
		{
			BusinessObject newElement = CreateBusinessObjectFromRow(row);
			HookupElementToCollection(newElement);
			return newElement;
		}

		protected internal void HookupElementToCollection(BusinessObject element)
		{
			if (!((IList)element.ParentCollections).Contains(this))
			{
				element.AddParentCollection(this);
				HookupElementChangedEvent(element);
				element.HasChangesChanged -= ElementHasChangedEventHandler;
				element.HasChangesChanged += ElementHasChangedEventHandler;
				var boState = (IBusinessObjectState)element;
				if (fUpdatedByDataRefreshIncludingChildren != null)
				{
#pragma warning disable
					boState.UpdatedByDataRefreshIncludingChildren += new EventHandler(HandleChildUpdatedByDataRefreshIncludingChildren);
#pragma warning restore
				}
				boState.NotificationsChanged -= ChildNotificationChangedHandler;
				boState.NotificationsChanged += ChildNotificationChangedHandler;
				if (readOnlyIndex > 0)
				{
					boState.IncrementReadOnlyIncludingChildren();
				}
			}
		}

		void ILegacyBusinessObjectCollectionInternals.SetCollectionRelationships(BusinessObject child)
		{
			SetCollectionRelationships(child);
		}

		protected internal virtual void SetCollectionRelationships(BusinessObject child)
		{
		}

		/// <summary>
		/// This function is called from the button grid and StmNoteCollectionView.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void SetupNewElementButDoNotAddIt(BusinessObject @new, bool setupCollectionRelationships)
		{
			if (setupCollectionRelationships)
			{
				SetCollectionRelationships(@new);
			}
			SetDefaultsForNewChild(@new);
			if (!setupCollectionRelationships)
			{
				RemoveCollectionRelationshipsCore(@new, false);
			}
			@new.HasChanges = false;
		}

		#endregion

		#endregion

		#region Removing / Deleting Elements

		/// <summary>
		/// Called whenever an element is removed from the collection / deleted.
		/// </summary>
		protected virtual void OnRemoving(BusinessObject bizO)
		{
			if (ElementRemoving != null)
			{
				ElementRemoving(bizO);
			}
		}

		protected virtual void OnRemoved(BusinessObject bizO)
		{
			if (CanRaiseNotificationsChanged() && bizO.HasNotifications())
			{
				OnNotificationsChanged(new NotificationsChangedEventArgs(bizO));
			}
			UpdateCountOnRemoved(bizO);
		}

		/// <summary>
		/// Removes all elements from the Collection, without deleting them.
		/// </summary>
		public void RemoveAll()
		{
			if (Elements.Count > 0)
			{
				RemoveRange(Elements.ToArray());
			}
		}

		/// <summary>
		/// Removes multiple BusinessObjects from the collection.
		/// Has O(n^2) remove cost, but at least it doesn't call event handlers too often.
		/// </summary>
		public void RemoveRange(IEnumerable businessObjects)
		{
			NotifyEnumerators();
			using (SuspendListChanged())
			{
				OnRemovingRange();

				foreach (BusinessObject bizObject in businessObjects)
				{
					Remove(bizObject);
				}

				OnRemovedRange(businessObjects);
			}
		}

		protected virtual void OnRemovingRange()
		{
		}

		protected virtual void OnRemovedRange(IEnumerable businessObjects)
		{
		}

		void IBusinessObjectCollection.RemoveFromRelationship(BusinessObject businessObject)
		{
			Remove(businessObject);
		}

		/// <summary>
		/// Removes an element from the collection. It is not deleted from the dataset.
		/// </summary>
		public virtual void Remove(BusinessObject businessObject)
		{
			Remove(businessObject, true);
		}

		internal void Remove(BusinessObject elementToRemove, bool removeCollectionRelationship)
		{
			int index = Elements.IndexOfOptimisedForHashByPK(elementToRemove);

			if (index != -1)
			{
				OnRemoving(elementToRemove);
				UnHookElementFromCollection(elementToRemove);
				if (removeCollectionRelationship)
				{
					RemoveCollectionRelationships(elementToRemove, false);
				}

				RemoveFromElements(elementToRemove);

				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
				OnRemoved(elementToRemove);
			}
		}

		public void Remove(ZGuid pK)
		{
			BusinessObject bO = FindByPK(pK);
			if (bO != null)
			{
				Remove(bO);
			}
		}

		/// <summary>
		/// Removes and deletes all elements in the collection.
		/// </summary>
		public virtual void RemoveAndDeleteAll()
		{
			using (SuspendListChanged())
			{
				BusinessObject[] elements = this.ToArray();
				foreach (BusinessObject bO in elements)
				{
					if (Contains(bO))
					{
						bO.FetchStrategy.FetchForDelete();
					}
				}
				foreach (BusinessObject bO in elements)
				{
					if (Contains(bO))
					{
						RemoveAndDelete(bO);
					}
				}
			}
		}

		void ILegacyBusinessObjectCollectionInternals.RemoveAllButLeaveRelationshipsIntact()
		{
			RemoveAllButLeaveRelationshipsIntact();
		}

		protected internal void RemoveAllButLeaveRelationshipsIntact()
		{
			if (Elements.Count > 0)
			{
				InRemoveAllButLeaveRelationshipsIntact = true;
				try
				{
					bool hasChecked = false;
					foreach (BusinessObject element in Elements.ToArray())
					{
						if (Elements.Contains(element))
						{
							if (!hasChecked)
							{
								hasChecked = true;
								NotifyEnumerators();
							}
							OnRemoving(element);
							UnHookElementFromCollection(element);
							ResumeValidationOnRemoved(element);
						}
					}
					Elements.Clear();
					FireListResetEvent();
				}
				finally
				{
					InRemoveAllButLeaveRelationshipsIntact = false;
				}
			}
		}

		internal bool InRemoveAllButLeaveRelationshipsIntact;

		void IBusinessObjectCollection.Delete(BusinessObject businessObject)
		{
			RemoveAndDelete(businessObject);
		}

		/// <summary>
		/// Removes element from collection, then deletes it from the dataset.
		/// </summary>
		/// <param name="elementToDelete"></param>
		public virtual void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete == null)
			{
				throw new ArgumentNullException(nameof(elementToDelete), "ElementToDelete should not be null for RemoveAndDelete.");
			}

			if (Elements == null)
			{
				throw new InvalidOperationException("Elements should not be null for RemoveAndDelete.");
			}

			InRemoveAndDelete = true;
			try
			{
				int index = Elements.IndexOfOptimisedForHashByPK(elementToDelete);
				if (index < 0 || index >= Elements.Count)
				{
					ErrorReporter.ReportOnce("CollectionRemoveAndDelete" + elementToDelete.GetType().FullName, "Element (" + elementToDelete.GetType().FullName + ", " + elementToDelete.PK + ")is not in collection " + GetType().FullName + " so cannot be removed & deleted. Index was " + index + " out of " + Elements.Count + " Elements.");
				}

				OnRemoving(elementToDelete);

				using (SuspendListChanged())
				{
					RemoveCollectionRelationships(elementToDelete, true);
					elementToDelete.Delete();
					UnHookElementFromCollection(elementToDelete);
					RemoveFromElements(elementToDelete);
				}

				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
				OnRemoved(elementToDelete);
			}
			finally
			{
				InRemoveAndDelete = false;
			}
		}

		protected bool InRemoveAndDelete;

		#region Implementation

		public void RemoveCollectionRelationships(BusinessObject child, bool forDelete)
		{
			if (child != null)
			{
				using (child.GetValidationSuspender())
				{
					child.IsRemovingFromRelationship = true;
					RemoveCollectionRelationshipsCore(child, forDelete);
					child.IsRemovingFromRelationship = false;
				}
			}
		}

		/// <summary>
		/// Disconnects any relationships to parents. Eg, removes pivot table entries etc
		/// for Child.
		/// </summary>
		/// <param name="child"></param>
		protected virtual void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
		}

		EventHandler<HasChangesChangedEventArgs> fElementHasChangedEventHandler;
		protected EventHandler<HasChangesChangedEventArgs> ElementHasChangedEventHandler
		{
			get
			{
				if (fElementHasChangedEventHandler == null)
				{
					fElementHasChangedEventHandler = new EventHandler<HasChangesChangedEventArgs>(ElementHasChangesChanged);
				}
				return fElementHasChangedEventHandler;
			}
		}

		ListChangedEventHandler fElementListChangedEventHandler;
		protected ListChangedEventHandler ElementListChangedEventHandler
		{
			get
			{
				if (fElementListChangedEventHandler == null)
				{
					fElementListChangedEventHandler = new ListChangedEventHandler(ElementListChanged);
				}
				return fElementListChangedEventHandler;
			}
		}

		protected void UnHookElementFromCollection(BusinessObject element)
		{
			if (Factory != null)
			{
				Factory.InvalidateCachedProperties();
			}
			((IBindingList)element).ListChanged -= ElementListChangedEventHandler;
			element.HasChangesChanged -= ElementHasChangedEventHandler;
			if (fUpdatedByDataRefreshIncludingChildren != null)
			{
#pragma warning disable
				((IBusinessObjectState)element).UpdatedByDataRefreshIncludingChildren -= new EventHandler(HandleChildUpdatedByDataRefreshIncludingChildren);
#pragma warning restore
			}
			((IBusinessObjectState)element).NotificationsChanged -= ChildNotificationChangedHandler;

			element.RemoveParentCollection(this);
		}

		#endregion

		#endregion

		#region Searching & Sorting Elements & ToArray

		/// <summary>
		/// Override this to provide your own IComparer for sorting the collection.
		/// </summary>
		protected internal virtual IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
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

		/// <summary>
		/// Does the Collection have this element?
		/// </summary>
		public bool Contains(BusinessObject businessObject)
		{
			return Elements.Contains(businessObject);
		}

		/// <summary>
		/// Does the Collection have an element with the PK provided?
		/// </summary>
		public bool Contains(ZGuid businessObject)
		{
			return FindByPK(businessObject) != null;
		}

		/// <summary>
		/// Copies the Elements of this Collection into an Array.
		/// </summary>
		/// <returns>Elements of the Collection as a BusinessObject[].</returns>
		public BusinessObject[] ToArray()
		{
			return Elements.ToArray();
		}

		/// <summary>
		/// Copies the elements of BusinessObjectCollection to an array of the specified type.
		/// </summary>
		public Array ToArray(Type type)
		{
			return Elements.ToArray(type);
		}

		/// <summary>
		/// Copies the collection's elements to the given list.
		/// </summary>
		public void CopyToList(IList list)
		{
			list.Clear();

			foreach (BusinessObject element in this)
			{
				list.Add(element);
			}
		}

		/// <summary>
		/// Copies the collection's elements to the given list starting a startIndex.
		/// </summary>
		public void CopyToList(IList list, int startIndex)
		{
			CopyToList(list, startIndex, Elements.Count - startIndex);
		}

		/// <summary>
		/// Copies the collection's elements to the given list starting a startIndex and copying numberOfElementsToCopy
		/// </summary>
		public void CopyToList(IList list, int startIndex, int numberOfElementsToCopy)
		{
			list.Clear();

			startIndex = Math.Max(0, startIndex);
			numberOfElementsToCopy = Math.Max(0, numberOfElementsToCopy);

			int endIndex = Math.Min(startIndex + numberOfElementsToCopy, Elements.Count);

			for (int i = startIndex; i < endIndex; i++)
			{
				list.Add(Elements[i]);
			}
		}

		/// <summary>
		/// Gets an element by the PK.
		/// </summary>
		public BusinessObject FindByPK(ZGuid pk)
		{
			return Elements.GetByPK(pk);
		}

		/// <summary>
		/// Creates an array of BusinessObjects that match the ZQuery provided.
		/// May return an array of 0 items.
		/// </summary>
		public BusinessObject[] Find(ZQuery filter)
		{
			// Hey, Brett, don't worry, this DOES work, FindByPK holds the magic. :) DJC
			ArrayList result = new ArrayList();
			DataRow[] rows = Factory.RowFactory.Select(Table.TableName, filter);
			foreach (DataRow row in rows)
			{
				BusinessObject bizObj = FindByPK((Guid)row[0]);
				if (bizObj != null)
				{
					if (TypeOfElements.IsAssignableFrom(bizObj.GetType()))
					{
						result.Add(bizObj);
					}
					else
					{
						ErrorReporter.ReportOnce("WrongTypeInElements", string.Format(CultureInfo.InvariantCulture,
							"Collection had an unexpected type. bizObj.GetType() == {0}, TypeOfElements = {1}, PK = {2}.", bizObj.GetType(), TypeOfElements, row[0]));
					}
				}
			}

			return (BusinessObject[])result.ToArray(TypeOfElements);
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

		/// <summary>
		/// Sort the Collection elements on a given property.
		/// </summary>
		public void Sort(string propertyName)
		{
			Sorter.Sort(propertyName, ListSortDirection.Ascending);
		}

		public void Sort(string propertyName, ListSortDirection direction)
		{
			Sorter.Sort(propertyName, direction);
		}

		public void Sort<T>(Comparison<T> comparison) where T : BusinessObject
		{
			Sort(new FunctorComparer<T>(comparison));
		}

		protected void ReSort()
		{
			if (Sorter.IsSorted)
			{
				Sorter.Resort();
			}
		}

		/// <summary>
		/// Sort the Collection elements using the specified IComparer.
		/// </summary>
		/// <param name="comparer">IComparer to sort with.</param>
		public void Sort<T>(IComparer<T> comparer) where T : BusinessObject
		{
			sortChangedSuspendCount++;
			var result = false;
			try
			{
				NotifyEnumerators();
				result = Elements.ApplySort(comparer);
				if (result)
				{
					((IBindingList)this).RemoveSort(); // IBindingList can't tell the grid anything useful about this type of sort
					FireListResetEvent();
				}
			}
			finally
			{
				sortChangedSuspendCount--;
			}

			if (result)
			{
				OnSortChanged();
			}
		}

		/// <summary>
		/// Sort the Collection elements using the specified IComparer.
		/// </summary>
		/// <param name="comparer">IComparer to sort with.</param>
		public void Sort(IComparer comparer)
		{
			sortChangedSuspendCount++;
			var result = false;
			try
			{
				NotifyEnumerators();
				result = Elements.ApplySort(comparer);
				if (result)
				{
					((IBindingList)this).RemoveSort(); // IBindingList can't tell the grid anything useful about this type of sort
					FireListResetEvent();
				}
			}
			finally
			{
				sortChangedSuspendCount--;
			}

			if (result)
			{
				OnSortChanged();
			}
		}

		/// <summary>
		/// Sort the Collection elements on a given property.
		/// </summary>
		public void Sort(SortInfo sortInfo)
		{
			Sort(sortInfo.PropertyName, sortInfo.Direction);
		}

		/// <summary>
		/// Provides information on collection sort order. Returns null if Collection
		/// does not have a sort order set or sorted by Sort(IComparer).
		/// </summary>
		public SortInfo SortInformation
		{
			get { return Sorter.SortInformation; }
		}

		#endregion

		#region IIdentified Members

		/// <summary>
		/// This is used by the FormCache to determine if the same form is 
		/// open already. Default behaviour is that there is only one identifer 
		/// for each collection, so form with a collection as the top level 
		/// business object can only be opened once. Override this to return 
		/// a relevant master record PK for the collection, if there is one and 
		/// the form will be able to be opened multiple times.
		/// </summary>
		public virtual ZGuid Identifier
		{
			get { return ZGuid.Empty; }
		}

		#endregion

		#region Non Persistent

		public bool IsNonPersistent
		{
			get { return typeof(NonPersistentBusinessObject).IsAssignableFrom(TypeOfElements); }
		}

		#endregion

		#region HumanReadableName

		public ZString HumanReadableName
		{
			get { return HumanReadableNameCore; }
		}

		protected virtual ZString HumanReadableNameCore
		{
			get { return Table == null ? Res.GetString("a298d732-a5eb-4b54-9592-8c1b57f8992d", "record") : DataBoundResourceStrings.GetStringForTable(TypeOfElements); }
		}

		#endregion

		#region Debugging Trace Data

#if DEBUG
		ZGuid _Instance;
#endif
		#endregion

		#region MaxCountValidation

		public void MaxCountValidationEnable(int maxCount)
		{
			MaxCountValidationEnable(maxCount, "");
		}

		public void MaxCountValidationEnable(int maxCount, string errorMessageOverride, bool warnAtHalfway = false)
		{
			this.EnableMaxCountValidation(maxCount, errorMessageOverride, warnAtHalfway);
		}

		public void MaxCountValidationWithMessageErrorEnable(int maxCount, string messageOverride, bool warnAtHalfway = false)
		{
			this.EnableMaxCountValidationWithMessageError(maxCount, warnAtHalfway, messageOverride);
		}

		public void MaxCountValidationDisable()
		{
			MaxCountValidator.MaxCount = -1;
		}

		public int MaxCount
		{
			get { return MaxCountValidator.MaxCount; }
		}

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

		int AddToElements(BusinessObject bizO, bool withValidationSuspended = true)
		{
			NotifyEnumerators();
			//note: subtly different (on purpose) from RemoveFromElements
			if (withValidationSuspended)
			{
				for (int indexer = 0; indexer < ValidationIndex; indexer++)
				{
					bizO.SuspendValidation();
				}
			}
			return Elements.Add(bizO);
		}

		protected void RemoveFromElements(BusinessObject bizO, bool withValidation = true)
		{
			if (withValidation)
			{
				NotifyEnumerators();
				ResumeValidationOnRemoved(bizO);
			}
			Elements.Remove(bizO);
		}

		void ResumeValidationOnRemoved(BusinessObject bizO)
		{
			if (!bizO.IsDeleted)
			{
				for (int indexer = 0; indexer < ValidationIndex; indexer++)
				{
					bizO.ResumeValidation();
				}
			}
		}

		/// <summary>
		/// Generate a typed Business Object that can be added to the collection. For an OrderCollection, this would have the code:
		/// return new Order(Factory, Row);
		/// </summary>
		/// <param name="row">Row to wrap in a Business Object.</param>
		/// <returns></returns>
		protected virtual BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			if (Factory == null)
			{
				throw new Exception(string.Format("Use the Factory constructor for {0} or override CreateBusinessObjectFromRow if you're using the deprecated constructor with no Factory arg.", GetType().Name));
			}
			return Factory.CreateBusinessObject(row, TypeOfElements, typeDeciderContext: GetTypeDeciderContext());
		}

		public ITypeDeciderContext GetTypeDeciderContext() => GetTypeDeciderContextCore();

		protected virtual ITypeDeciderContext GetTypeDeciderContextCore() => null;

		#region Attributes

		protected HashedBizOList Elements
		{
			get
			{
				if (elements == null)
				{
					elements = new HashedBizOList();
				}
				return elements;
			}
		}

		HashedBizOList elements;
		BusinessObjectFactory factory;
		bool loaded;
		bool loading;

		#endregion

		#region ListChanged hookup

		void ElementListChanged(object sender, ListChangedEventArgs e)
		{
			OnListChanged((BusinessObject)sender);
		}

		internal void HookupElementChangedEvent(BusinessObject element)
		{
			((IBindingList)element).ListChanged -= ElementListChangedEventHandler;
			((IBindingList)element).ListChanged += ElementListChangedEventHandler;
		}

		#endregion

		#region HasChanges Hookup

		void ElementHasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			UpdateHasChanges(e);
		}

		#endregion

		#region Grid BeginEdit and EndEdit support stuff

		public bool IsNonCommittedCollectionElement(BusinessObject bizObj)
		{
			return bizObj == (nonCommittedCollectionElementOverride ?? NonCommittedCollectionElement);
		}
		BusinessObject NonCommittedCollectionElement;

		internal IDisposable OverrideNonCommittedCollectionElementTemporarily(BusinessObject bizo)
		{
			nonCommittedCollectionElementOverride = bizo;
			return new DisposableAction(() => nonCommittedCollectionElementOverride = null);
		}
		BusinessObject nonCommittedCollectionElementOverride;

		//referenced by ZArch.GUI
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void DeleteOnCancelOfElement(BusinessObject bizObj)
		{
			bizObj.Delete();
			UpdateHasChanges(HasChangesChangedEventArgs.Create(false, this));
		}

		// used by the grid - add a business object that may be ditched if user doesn't enter anything
		// in the row and grid loses focus.
		BusinessObject AddNewUncommittedBusinessObject()
		{
			if (NonCommittedCollectionElement != null)
			{
				// ditch if it's still hanging around for some reason
				if (Elements.Contains(NonCommittedCollectionElement)
					&& NonCommittedCollectionElement.Row != null
					&& NonCommittedCollectionElement.Row.RowState == DataRowState.Detached)
				{
					CancelNew(NonCommittedCollectionElement);
				}
			}

			NonCommittedCollectionElement = CreateNewBusinessObject();
			using (NonCommittedCollectionElement.SuspendSettingHasChangesIncludingChildren())
			{
				SetCollectionRelationships(NonCommittedCollectionElement);
				SetDefaultsForNewChild(NonCommittedCollectionElement);
			}
			AddToElements(NonCommittedCollectionElement);
			return NonCommittedCollectionElement;
		}

		#endregion

		#region Modifications in Enumerator Checking

		bool hasCheckedFetchHints;

		protected virtual TableColumn[] GetColumnsToFetchForEnumerate()
		{
			return Array.Empty<TableColumn>();
		}

		protected void NotifyEnumerators()
		{
			for (var node = enumerators.First; node != null; node = node.Next)
			{
				if (node.Value.TryGetTarget(out var enumerator))
				{
					enumerator.NotifyCollectionModified();
				}
			}
		}

		void RegisterEnumerable(BusinessObjectCollectionEnumerator enumerable)
			=> enumerators.AddLast(new WeakReference<BusinessObjectCollectionEnumerator>(enumerable));

		void UnregisterEnumerable(BusinessObjectCollectionEnumerator toRemove)
		{
			for (var node = enumerators.First; node != null; node = node.Next)
			{
				// Someone has not disposed their Enumerator
				// Should not be possible because enumerator destructor will Unregister it, but no point in reporting without enough info to fix
				if (!node.Value.TryGetTarget(out var enumerator))
				{
					enumerators.Remove(node);
				}
				else if (enumerator == toRemove)
				{
					enumerators.Remove(node);
					return;
				}
			}
		}

		readonly LinkedList<WeakReference<BusinessObjectCollectionEnumerator>> enumerators = new LinkedList<WeakReference<BusinessObjectCollectionEnumerator>>();

		protected sealed class BusinessObjectCollectionEnumerator : Disposable, IEnumerator<BusinessObject>
		{
			public BusinessObjectCollectionEnumerator(BusinessObjectCollection collection)
			{
				this.collection = collection;
				this.inner = ((IEnumerable<BusinessObject>)collection.Elements).GetEnumerator();

				if (IsSafeForEnumerationModificationWarning)
				{
					collection.RegisterEnumerable(this);
				}
			}

			bool IsSafeForEnumerationModificationWarning
				=> collection.Factory?.IsOwnedByCurrentThread ?? false;

			public BusinessObject Current => inner.Current;
			public void Reset() => inner.Reset();
			object IEnumerator.Current => Current;

			StackTrace modifiedStackTrace;
			public void NotifyCollectionModified()
			{
				if (modifiedStackTrace == null)
				{
					modifiedStackTrace = new StackTrace();
				}
			}

			bool alreadyReportedModifiedCollection;
			public bool MoveNext()
			{
				if (modifiedStackTrace != null && !alreadyReportedModifiedCollection)
				{
					ErrorReporter.ReportOnce("EnumerationCockUp_" + this.collection.GetType().Name, "Collection was modified while enumerating. Modification StackTrace: " + modifiedStackTrace.ToString());
					alreadyReportedModifiedCollection = true;
				}

				return inner.MoveNext();
			}

			protected override void Dispose(bool isDisposing)
			{
				if (!disposed && isDisposing)
				{
					if (IsSafeForEnumerationModificationWarning)
					{
						collection.UnregisterEnumerable(this);
					}

					this.inner.Dispose();
					disposed = true;
				}
			}

			readonly BusinessObjectCollection collection;
			readonly IEnumerator<BusinessObject> inner;
			bool disposed;
		}

		#endregion

		#region IBusiness Members

		public bool CanContinueWithSave
		{
			get { return CanContinueWithSaveCore; }
		}

		protected virtual bool CanContinueWithSaveCore
		{
			get { return true; }
		}

		uint IBusinessObjectState.LastChangeNumber
		{
			get { return lastChangeNumber; }
		}
		uint lastChangeNumber;

		void IBusiness.Delete()
		{
			RemoveAndDeleteAll();
		}

		string IBusiness.TableName
		{
			get { return BusinessObjectFactory.GetTableNameFromType(TypeOfElements); }
		}

		bool IBusinessObjectState.HasChangesNotIncludingChildren
		{
			get { return fHasChangesFromDelete; }
		}

		void IBusiness.ValidateIfQuickAndImprovesPreSaveValidationPerformance()
		{
			// potentially validate one object on a rolling basis?
		}

		IBusiness[] IBusiness.Children
		{
			get { return Elements.ToArray(); }
		}

		void IBusiness.NotifyRegisteredChildEditable()
		{
			NotifyRegisteredChildEditable();
		}

		protected virtual void NotifyRegisteredChildEditable()
		{
		}

		bool IBusiness.CanDeleteForDataRefresh => true;

		void IBusiness.DeleteForDataRefresh() => Elements.OfType<IBusiness>().Where(e => e.CanDeleteForDataRefresh)
			.ToList().ForEach(b => b.DeleteForDataRefresh());

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			Elements.CopyTo(array, arrayIndex);
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return Elements; }
		}

		#endregion

		#region IList Members

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		object IList.this[int index]
		{
			get
			{
				if (index >= 0 && index < Elements.Count)
				{
					return Elements[index];
				}
				else
				{
					return null;
				}
			}
			set
			{
				Elements[index] = (BusinessObject)value;
			}
		}

		void IList.Insert(int index, object value)
		{
			var bo = (BusinessObject)value;
			Elements.Insert(index, bo);

			HookupElementToCollection(bo);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
			SetCollectionRelationships(bo);
			OnAdded(bo);
		}

		void IList.RemoveAt(int index)
		{
			BusinessObject removedElement = Elements[index];
			((IList)this).Remove(removedElement);
		}

		void IList.Remove(object value)
		{
			Remove((BusinessObject)value);
		}

		bool IList.Contains(object value)
		{
			return Elements.Contains((BusinessObject)value);
		}

		void IList.Clear()
		{
			foreach (BusinessObject currentElement in this)
			{
				UnHookElementFromCollection(currentElement);
			}

			Elements.Clear();
			FireListResetEvent();
		}

		int IList.IndexOf(object value)
		{
			return Elements.IndexOf((BusinessObject)value);
		}

		int IList.Add(object value)
		{
			Add((BusinessObject)value);
			return Elements.IndexOf((BusinessObject)value);
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		#endregion

		#region IBusinessObjectCollection Members

		ISortable IBusinessObjectCollection.Elements
		{
			get { return Elements; }
		}

		IComparer IBusinessObjectCollection.GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return GetComparerForSort(property, direction);
		}

		int IBusinessObjectCollection.IndexOf(IBusiness bizObj, int startIndex, int countToSearchFromStartIndex)
		{
			return Elements.IndexOf((BusinessObject)bizObj, startIndex, countToSearchFromStartIndex);
		}

		void IBusinessObjectCollection.ApplySort(SortInfo sortInfo)
		{
			Sort(sortInfo);
		}

		PropertyDescriptor IBusinessObjectCollection.ListPropertyDescriptor { get; set; }
		object IBusinessObjectCollection.Parent { get; set; }

		#endregion

		#region ISortable Members

		void ISortable.ApplySort(IComparer comparer)
		{
			Sort(comparer);
		}

		#endregion

		#region IBindingList Members

		internal BusinessObjectCollectionSorter Sorter
		{
			get
			{
				if (fSorter == null)
				{
					fSorter = new BusinessObjectCollectionSorter(this);
				}
				return fSorter;
			}
		}

		BusinessObjectCollectionSorter fSorter;

		#region ListChanged management

		ListChangedEventHandler ListChanged;

		protected sealed class ListChangedSuspender : IDisposable
		{
			internal ListChangedSuspender(BusinessObjectCollection collection)
			{
				this.collection = collection;
				this.changesMadeWhenSuspensionStarted = collection.changesNotRaisedBecauseListChangeSuspended;
				collection.listChangeSemaphore++;
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			readonly BusinessObjectCollection collection;
			bool disposed;
			readonly long changesMadeWhenSuspensionStarted;

			public void Dispose()
			{
				if (!disposed)
				{
					try
					{
						collection.listChangeSemaphore--;
						bool listChangedWhileWeWereSuspended = collection.changesNotRaisedBecauseListChangeSuspended != changesMadeWhenSuspensionStarted;
						if (listChangedWhileWeWereSuspended)
						{
							collection.FireListResetEvent();
						}
					}
					finally
					{
						DisposableLeakListener.Instance.UnRegisterDisposable(this);
						disposed = true;
					}
				}
			}
		}

		public IDisposable SuspendListChanged()
		{
			if (forceNotSuspendListChanged)
			{
				return new DisposableAction(() => { });
			}

			var suspendingEventArgs = new ListChangedSuspendingEventArgs(GetAdditionalListChangedSuspenders());
			if (ListChangedSuspending != null)
			{
				ListChangedSuspending(this, suspendingEventArgs);
			}

			var suspenders = suspendingEventArgs.DisposableList;
			if (suspenders != null && suspenders.Count > 0)
			{
				suspenders.Add(new ListChangedSuspender(this));
				return suspenders;
			}
			else
			{
				return new ListChangedSuspender(this);
			}
		}

		bool forceNotSuspendListChanged;
		public IDisposable ForceNotSuspendListChanged()
		{
			forceNotSuspendListChanged = true;
			return new DisposableAction(() => forceNotSuspendListChanged = false);
		}

		protected virtual DisposableList GetAdditionalListChangedSuspenders()
		{
			return null;
		}

		public class ListChangedSuspendingEventArgs : EventArgs
		{
			[SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
			public ListChangedSuspendingEventArgs(DisposableList disposableList)
			{
				this.DisposableList = disposableList;
			}

			[SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
			public DisposableList DisposableList;
		}

		internal event EventHandler<ListChangedSuspendingEventArgs> ListChangedSuspending;

		int listChangeSemaphore;
		internal long changesNotRaisedBecauseListChangeSuspended;

		bool IBusinessObjectCollectionInternals.IsListChangedSuspended
		{
			get { return IsListChangedSuspended; }
		}

		internal virtual bool IsListChangedSuspended
		{
			get { return listChangeSemaphore != 0; }
		}

		event ListChangedEventHandler IBindingList.ListChanged
		{
			add { ListChanged += value; }
			remove { ListChanged -= value; }
		}

		#endregion

		public virtual IDisposable SuspendAdditionallyForImport()
		{
			return DisposableAction.NoAction;
		}

		[SuppressThreadStaticFieldMessage]
		static readonly ListChangedEventArgs ResetEventArgs = new ListChangedEventArgs(ListChangedType.Reset, -1);

		bool inOnListChanged;

		void OnListChanged(BusinessObject bizO)
		{
			if (!IsListChangedSuspended && !inOnListChanged)
			{
				var oldChangesNotRaisedBecauseListChangeSuspended = changesNotRaisedBecauseListChangeSuspended;

				try
				{
					inOnListChanged = true;

					if (ListChanged != null)
					{
						int index = Elements.IndexOf(bizO);
						if (index >= 0)
						{
							ListChanged(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index));
						}
					}
				}
				finally
				{
					inOnListChanged = false;
					if (changesNotRaisedBecauseListChangeSuspended != oldChangesNotRaisedBecauseListChangeSuspended)
					{
						this.FireListResetEvent();
					}
				}
			}
			else
			{
				changesNotRaisedBecauseListChangeSuspended++;
			}
		}

		void OnListChanged(ListChangedEventArgs e)
		{
			if (!IsListChangedSuspended)
			{
				if (ListChanged != null)
				{
					ListChanged(this, e);
				}
			}
			else
			{
				changesNotRaisedBecauseListChangeSuspended++;
			}
		}

		bool IBindingTracked.IsBound => ListChanged.IsBound(doRecursiveCheck: false);

		protected void FireListResetEvent()
		{
			FireListResetEventInternal();
		}

		internal virtual void FireListResetEventInternal()
		{
			OnListChanged(ResetEventArgs);
		}

		bool IBindingList.AllowEdit
		{
			get { return !ReadOnly; }
		}

		public bool AllowNew
		{
			get { return !ReadOnly && AllowNewCore; }
		}

		protected virtual bool AllowNewCore
		{
			get { return true; }
		}

		public bool AllowRemove
		{
			get { return !ReadOnly && AllowRemoveCore; }
		}

		protected virtual bool AllowRemoveCore
		{
			get { return true; }
		}

		bool IBindingList.SupportsChangeNotification
		{
			get { return true; }
		}

		bool IBindingList.SupportsSearching
		{
			get { return false; }
		}

		protected virtual bool AllowSort
		{
			get { return true; }
		}

		bool IBindingList.SupportsSorting
		{
			get { return AllowSort; }
		}

		object IBindingList.AddNew()
		{
			BusinessObject bizObj = null;

			try
			{
				if (!inCancelEdit) // In .net 2.0 RelatedCurrencyManager.ParentManager_CurrentItemChanged annoyingly calls AddNew().CancelEdit() which causes re-entrency problems, so IBindingList.AddNew will return null to overcome this performance issue
				{
					bizObj = AddNewUncommittedBusinessObject();
					OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Elements.IndexOf(bizObj)));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
#if DEBUG
				if (TestingState.IsRunningTests)
				{
					throw;
				}
#endif
				var errorMessage = new StringBuilder(ex.Message);
				errorMessage.Append((NoResString)" Collection type is: ").Append(GetType().FullName);
				errorMessage.Append((NoResString)". Business Object Type is: ").Append(bizObj != null ? bizObj.GetType().FullName : "null");
				ErrorReporter.ReportOnce(errorMessage.ToString(), ex);
			}

			return bizObj;
		}

		bool IBindingList.IsSorted
		{
			get { return Sorter.IsSorted; }
		}

		ListSortDirection IBindingList.SortDirection
		{
			get { return Sorter.SortDirection; }
		}

		PropertyDescriptor IBindingList.SortProperty
		{
			get { return Sorter.SortProperty; }
		}

		void IBindingList.RemoveSort()
		{
			Sorter.RemoveSort();
			SortState.RemoveSort();
			OnSortChanged();
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			NotifyEnumerators();
			RunFetchForSort(property);
			Sorter.ApplySort(property, direction);
			SortState.ApplySort(property, direction);
			SortState.ClearSortDescriptionsBeforeNextSort();
			OnSortChanged();
		}

		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException();
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IBindingListView Members

		#region Advanced Sorting

		bool IBindingListView.SupportsAdvancedSorting
		{
			get { return ((IBindingList)this).SupportsSorting; }
		}

		Dictionary<string, string> guidListMapper;

		HashSet<string> isTimeSet;

		void IBindingListView.ApplySort(ListSortDescriptionCollection sorts)
		{
			ApplySortCore(sorts);
		}

		internal virtual void ApplySortCore(ListSortDescriptionCollection sorts)
		{
			NotifyEnumerators();

			SortState.ClearSortDescriptionsBeforeNextSort();

			List<PropertyDescriptor> propertyDescriptors = new List<PropertyDescriptor>(sorts.Count);
			foreach (ListSortDescription sortDescription in sorts)
			{
				propertyDescriptors.Add(sortDescription.PropertyDescriptor);
			}
			RunFetchForSort(propertyDescriptors.ToArray());

			Sorter.ApplySorts(sorts, SortState);
			SortState.ClearSortDescriptionsBeforeNextSort();

			OnSortChanged();
		}

		public void RemoveSorts() => Sorter.RemoveSorts();

		ListSortDescriptionCollection IBindingListView.SortDescriptions
		{
			get { return SortState.SortDescriptions; }
		}

		SortState SortState
		{
			get
			{
				if (sortState == null)
				{
					sortState = new SortState();
				}

				return sortState;
			}
		}

		SortState sortState;

		#endregion

		#region Filtering

		bool IBindingListView.SupportsFiltering
		{
			get { return false; }
		}

		string IBindingListView.Filter
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		void IBindingListView.RemoveFilter()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#endregion

		#region IEnumerable

		IEnumerator<BusinessObject> IEnumerable<BusinessObject>.GetEnumerator()
		{
			return GetNewEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetNewEnumerator();
		}

		internal virtual IEnumerator<BusinessObject> GetNewEnumerator()
		{
			if (!hasCheckedFetchHints)
			{
				hasCheckedFetchHints = true;
				var columns = GetColumnsToFetchForEnumerate();
				if (columns.Any())
				{
					foreach (BusinessObject bizO in this.ToArray())
					{
						bizO.FetchStrategy.FetchForView(columns);
					}
				}
			}

			var types = $"typeOfElements:{this.TypeOfElements.FullName} type:{this.GetType().FullName}";
			Factory?.ThreadSentry.EnsureCurrentThreadIsOwner(types);
			return new BusinessObjectCollectionEnumerator(this);
		}

		#endregion

		#region Indexer Error String

		[ThreadStatic]
		static string fIndexerErrorExceptionMessage;
		public static string IndexerErrorExceptionMessage
		{
			get
			{
				return fIndexerErrorExceptionMessage ?? (fIndexerErrorExceptionMessage = (NoResString)@"Please put a typed indexer in your business object collection.
E.g., Replace MyBusinessObject with your Business Object type

	public MyBusinessObject this[int Index]
	{
		get { return (MyBusinessObject) Elements[Index]; }
	}
");
			}
		}

		#endregion

		#region INeedTable

		ZDataTable INeedTable.Table
		{
			get { return Table; }
		}

		#endregion

		#region INeedDataSet Members

		DataSet INeedDataSet.Data
		{
			get { return Table.DataSet; }
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

		string IFindBoxListProvider.CodeFromPrimaryKey(ZGuid pK)
		{
			return FindBoxListProvider.CodeFromPrimaryKey(pK);
		}

		IBusinessObjectCollection IFindBoxListProvider.List
		{
			get { return FindBoxListProvider.List; }
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

		#region Swapping out factory

		public void SwapFactoryAndRemoveAll(BusinessObjectFactory newFactory)
		{
			RemoveAllButLeaveRelationshipsIntact();
			factory = newFactory;
		}

		#endregion

		#region Yucky Events - DO NOT EXPOSE AS PUBLIC

		internal event ElementChangedHandler ElementAdded;
		internal event ElementChangedHandler ElementRemoving;
		internal delegate void ElementChangedHandler(BusinessObject elementChanged);

		internal event EventHandler SortChanged;

		event EventHandler IBusinessObjectCollection.SortChanged
		{
			add { SortChanged += value; }
			remove { SortChanged -= value; }
		}

		void OnSortChanged()
		{
			if (sortChangedSuspendCount == 0)
			{
				if (SortChanged != null)
				{
					SortChanged(this, null);
				}
			}
		}

		int sortChangedSuspendCount;

		#endregion

		#endregion

		#region INotificationProvider

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

		#region IFilterBusinessObjectDefaultsProvider

		public FilterBusinessObjectDefaults FilterBusinessObjectDefaults
		{
			get { return filterBusinessObjectDefaults ?? (filterBusinessObjectDefaults = new FilterBusinessObjectDefaults()); }
		}
		FilterBusinessObjectDefaults filterBusinessObjectDefaults;

		#endregion

		#region IBusinessObjectCollectionTestingMembers Members
#if DEBUG
		ZQuery IBusinessObjectCollectionTestingMembers.AdditionalFilter
		{
			get { return AdditionalFilter; }
		}

		ZQuery IBusinessObjectCollectionTestingMembers.GetAdditionalFilter()
		{
			return additionalFilter;
		}

		string IBusinessObjectCollectionTestingMembers.GetGuidListMapping(string columnName)
		{
			string result = null;
			if (guidListMapper != null)
			{
				guidListMapper.TryGetValue(columnName, out result);
			}
			return result;
		}
#endif
		#endregion

		#region ICancelAddNew Members

		void ICancelAddNew.CancelNew(int index)
		{
			if (index >= 0 && index < Elements.Count)
			{
				BusinessObject cancelledCollectionElement = Elements[index];
				CancelNew(cancelledCollectionElement);
			}
		}

		void CancelNew(BusinessObject cancelledCollectionElement)
		{
			if (cancelledCollectionElement == NonCommittedCollectionElement)
			{
				inCancelEdit = true;
				try
				{
					BusinessObject cancelledElement = NonCommittedCollectionElement;
					Remove(cancelledElement);
					DeleteOnCancelOfElement(cancelledElement);
				}
				finally
				{
					inCancelEdit = false;
				}
			}
		}
		bool inCancelEdit;

		void ICancelAddNew.EndNew(int index)
		{
			if (index != -1)
			{
				if (index < Elements.Count)
				{
					BusinessObject committedCollectionElement = Elements[index];
					if (committedCollectionElement == NonCommittedCollectionElement && committedCollectionElement is BusinessObject)
					{
						if (committedCollectionElement.HasChanges)
						{
							INeedRow rowProvider = committedCollectionElement;
							if (rowProvider != null)
							{
								AddRowToDataTableIfNotNullAndDetached(committedCollectionElement);
							}
							NonCommittedCollectionElement = null;
							BusinessObject bizObj = committedCollectionElement;
							OnAdded(bizObj);
							OnNonCommittedAdded(bizObj);
						}
						else // element has no user edits, so ditch it
						{
							((ICancelAddNew)this).CancelNew(index);
						}
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Called whenever a non-committed element has been added to the collection
		/// </summary>
		protected virtual void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
		}

		#region EnsureIsNotNewUsageOfThisObsoleteCollection
#if DEBUG

		void EnsureIsNotNewUsageOfThisObsoleteCollection()
		{
			var type = GetType();
			if (!type.Name.StartsWith("Mock") &&
				!type.FullName.StartsWith("Castle.Proxies.") &&
				!EnsureIsNotNewUsageOfThisObsoleteCollection(type))
			{
				throw new InvalidOperationException("BusinessObjectCollection is obsolete. Consider inheriting newly created collection class " + GetType().FullName + " from ActiveBusinessObjectCollection as an alternative.");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1081:DoNotUseSubClassOfTypeofBusinessObjectCollection", Justification = "Baseline")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Refactoring is needed in future")]
		static bool EnsureIsNotNewUsageOfThisObsoleteCollection(Type collectionType)
		{
			bool result;
			switch (collectionType.FullName)
			{
				case "Enterprise.ZArchitecture.GUI.Testing.ZBindingContextTest+DummyBusinessObjectWithTypeDeciderCollection":
				case "Enterprise.Packing.Business.PackableItemCollection": // ActiveBusinessObjectCollection does not support interfaces
				case "Enterprise.ZArchitecture.Business.RelatedJobCollection": // ActiveBusinessObjectCollection does not support interfaces
				case "CargoWise.EntityFramework.NonPersistentBusinessObjectCollection":
				case "Enterprise.ZArchitecture.Business.Testing.ModuleTextFilterTest+DummyBizoCollectionThatImplementsICodeDescPairList":
				case "CargoWise.EntityFramework.BusinessObjectCollection`1":
				case "Enterprise.ZArchitecture.Business.Testing.DummyWithZAddressCollection":
				case "CargoWise.EntityFramework.DependentBusinessObjectCollection":
				case "Enterprise.ZArchitecture.Business.StmNoteCollection":
				case "Enterprise.ZArchitecture.GUI.Internal.Testing.OrganisationEmdeddedModulePopupTest+TestList":
				case "CargoWise.EntityFramework.NonPersistentBusinessObjectCollection`1":
				case "Enterprise.ZArchitecture.DataMapping.CustomMapPairListWrapperCollection":
				case "Enterprise.ZArchitecture.Business.Testing.DummyBusinessObjectCollection":
				case "Enterprise.ZArchitecture.Business.Testing.DummyChildBusinessObjectCollection":
				case "Enterprise.ZArchitecture.Business.Testing.DummyBusinessObjectWithActiveFilterCollection":
				case "Enterprise.ZArchitecture.Business.StmALogCollection":
				case "Enterprise.ZArchitecture.Business.StmALogCollectionWithRelatedElements":
				case "CargoWise.EntityFramework.DependentBusinessObjectCollection`2":
				case "Enterprise.ZArchitecture.Business.Testing.DummyDependentBusinessObjectCollection":
				case "Enterprise.ZArchitecture.Business.Testing.DummyBusinessObjectInListList":
				case "Enterprise.ZArchitecture.Business.StmModuleFilterUserDataCollection":
				case "Enterprise.ZArchitecture.Business.StmActivityLogCollection":
				case "Enterprise.ZArchitecture.Business.StmActivityLogCollectionByStaff":
				case "Enterprise.ZArchitecture.Business.StmActivityLogCollectionByParent":
				case "CargoWise.EntityFramework.BusinessObjectFactoryStatisticCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.DummyWithCodeDescriptionPairListCollection":
				case "Enterprise.ZArchitecture.Business.StmNoteNonDependentCollection`1":
				case "Enterprise.ZArchitecture.Business.StmNoteNonDependentCollection":
				case "CargoWise.EntityFramework.SubsetBusinessObjectCollection":
				case "CargoWise.EntityFramework.BusinessObjectCollectionView":
				case "Enterprise.ZArchitecture.Business.StmNoteCollectionViewNonPrivateNotesWithContext":
				case "Enterprise.ZArchitecture.Business.LogsForNominatedEvent":
				case "Enterprise.ZArchitecture.DependentBusinessObjectCollectionTest+DummyDependentBusinessObjectCollectionThatUsesMaster":
				case "Enterprise.ZArchitecture.Modules.Testing.DummyBusinessObjectCollectionWithIRelationshipAdderForController":
				case "Enterprise.ZArchitecture.GUI.Testing.ZDataGridViewFindBoxCellTest+DummyOrgBusinessObjectCollection":
				case "Enterprise.ZArchitecture.DataMapping.ImportWizardMappingCollection":
				case "Enterprise.ZArchitecture.DataMapping.ImportWizardPreviewLineCollection":
				case "CargoWise.EntityFramework.NonPersistentBusinessObjectCollectionTest+DummyNonPersistentBusinessObjectCollection":
				case "CargoWise.EntityFramework.NonPersistentBusinessObjectCollectionTest+DummyGenericNonPersistentBusinessObjectCollection":
				case "CargoWise.EntityFramework.NonPersistentBusinessObjectCollectionTest+DummyGenericNonPersistentBusinessObjectWithRowCollection":
				case "CargoWise.EntityFramework.Testing.BusinessObjectCollectionTest+ListChangedSuspendableCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+DummyWithOverriddenTypeOfElementsCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+OnLoadedTestCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+DummyBusinessObjectCollectionForAdditionalFilterNotMetError":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+DummyCollectionWithOverridenName":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+NullTableCollection":
				case "CargoWise.EntityFramework.Testing.BusinessObjectCollectionTest+VanillaBusinessObjectCollection":
				case "CargoWise.EntityFramework.Testing.BusinessObjectCollectionTest+DummyNonPersistentCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+DummyBusinessObjectCollectionWithRelationshipFilter":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+TestBusinessObjectCollectionWithFetchFromLocalCacheOnly":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+ValidationTestCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+RemoveTestCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionTest+DummyBusinessObjectCollectionForRemoveAndDeleteAll":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionElementTypeForCollectionTypeTest+DummyStringCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionElementTypeForCollectionTypeTest+DummyInt32Collection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionElementTypeForCollectionTypeTest+DummyCollectionWithNewdIndexerBase":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollectionElementTypeForCollectionTypeTest+DummyCollectionWithNewdIndexer":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectCollection_IFindBoxListProviderTest+DummyBusinessObjectCollectionWithAutoCompleteOnComit":
				case "Enterprise.ZArchitecture.Testing.BODocDataProviderCollectionHelperTest+TestBOCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.GridTestFieldBusinessObjectCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.ZGuidFindBoxTest+DummyBizOWithCodeOverrideCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.ZGuidFindBoxTest+BizOWithNoCodePropertyCollection":
				case "CargoWise.EntityFramework.FlatHierarchyBusinessObjectCollection":
				case "Enterprise.ZArchitecture.Modules.Testing.DummyBusinessObjectCollectionWithIInitialiseFilterBusinessObject":
				case "Enterprise.ZArchitecture.ZGrid+TestClickCopyToNewRowMenuItem+DummyBOWithITemplateCopyableChildren+ChildCollection":
				case "Enterprise.ZArchitecture.Business.StmNoteCollectionView":
				case "Enterprise.ZArchitecture.Business.Testing.StmNoteCollectionViewTest+SpecialStmNoteDependantCollection":
				case "Enterprise.ZArchitecture.GUI.ZOrganisationFindBox+Test+TestCollection":
				case "Enterprise.ZArchitecture.DataMapping.CustomMapPairList":
				case "Enterprise.ZArchitecture.Business.Testing.DummySubsetCollection":
				case "Enterprise.ZArchitecture.Business.Testing.DummyDependantBusinessObjectSubsetCollection":
				case "Enterprise.ZArchitecture.Business.StmEventCollection":
				case "Enterprise.ZArchitecture.Business.StmALogDependentCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.ZDisplayGridTest+NonDependentDummyDependentCollection":
				case "Enterprise.ZArchitecture.GUI.Internal.Testing.ZDataGridViewModuleNewHandlerTest+DummyChildBusinessObjectCollectionWithRelationshipSet":
				case "Enterprise.ZArchitecture.TypeDescription.Testing.BusinessObjectPropertyDescriptorProviderTest+DummyBusinessObjectWithTypeDeciderCollection":
				case "Enterprise.ZArchitecture.Business.StmNoteCollectionWithRelatedElements":
				case "Enterprise.ZArchitecture.Modules.Testing.DummyBusinessObjectCollectionWithValidateForController":
				case "Enterprise.ZArchitecture.Business.StmALogCollectionView":
				case "Enterprise.ZArchitecture.Business.StmModuleFilterCollection":
				case "Enterprise.ZArchitecture.Business.Internal.Testing.DummyBusinessObjectWithCalculatedCodePropertyCollection":
				case "CargoWise.EntityFramework.Testing.TestCompositeCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+MyDummyChildBusinessObjectCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+NonPersistentBizOForTestCollection":
				case "Enterprise.ZArchitecture.GUI.Testing.ZDetailsTabPagesTest+ATestDummyCollection":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectTest+DummyDependentBusinessObjectCollectionThatAccessesParentOnRemove":
				case "Enterprise.ZArchitecture.Testing.BusinessObjectTest+TestOverflowDummyCollection":
				case "Enterprise.ZArchitecture.Business.DocWrappers.DocumentWrapperCollection":
				case "Enterprise.ZArchitecture.Business.DocWrappers.Testing.WrapperTypeNameAttributeTest+TestWrapperCollection":
				case "Enterprise.ZArchitecture.Business.DocWrappers.Testing.WrapperTypeNameAttributeTest+TestWrapperWropperCollection":
				case "Enterprise.ZArchitecture.Business.Testing.DataRefreshBusTest+DummyBusinessObjectCollectionWithMatchingCollectionFilter":
				case "Enterprise.ZArchitecture.Business.Internal.FilterStripCollection":
				case "Enterprise.ZArchitecture.GUI.TemporaryOrganisationCreatorTest+TestCollection":
				case "CargoWise.EntityFramework.ManyToManyBusinessObjectCollection":
				case "CargoWise.EntityFramework.ManyToManyBusinessObjectCollection+PivotCollection":
				case "CargoWise.EntityFramework.ManyToManyBusinessObjectCollection`2":
				case "Enterprise.ZArchitecture.Testing.DummyMToNCollection":
				case "Enterprise.ZArchitecture.Testing.ManyToManyBusinessObjectCollectionTest+DummyMToNCollectionWithNoRelationshipFilter":
				case "Enterprise.ZArchitecture.Testing.ManyToManyBusinessObjectCollectionTest+DeleteCheckedCollection":
				case "Enterprise.ZArchitecture.Testing.ManyToManyBusinessObjectCollectionTest+DummyMToNCollectionThatUsesMaster":
				case "Enterprise.ZArchitecture.Business.DocWrappers.DocumentWrapperCollection`1":
				case "Enterprise.ZArchitecture.Business.DocWrappers.Testing.DocumentWrapperCollectionTest_ForCoreFunctionality+TestBOCollection":
				case "Enterprise.ZArchitecture.Business.DocWrappers.Testing.DocumentWrapperCollectionTest_ForCoreFunctionality+DocTestBOWrapperCollection":
				case "CargoWise.EntityFramework.BusinessObjectCollectionView`1":
				case "Enterprise.ZArchitecture.Business.Testing.DummyViewCollection":
				case "Enterprise.ZArchitecture.Business.Testing.DummyLookups+DummyIOrgHeaderBusinessObjectCollection":
				case "Enterprise.ZArchitecture.DataMapping.MassUpdateMatchingFilterCollection":
				case "Enterprise.Registry.Business.OpportunityStageCollection":
				case "Enterprise.Registry.Business.Testing.NonPersistentBusinessObjectRegistryDataTypeTestCase`1+DummyRelatedBusinessObjectCollection":
				case "Enterprise.Registry.Business.PrincipalBrandingCollection":
				case "Enterprise.Registry.Business.FreightPacksRegistryObjectCollection":
				case "Enterprise.Registry.Business.PaymentAuthorisationSettingsCollection":
				case "Enterprise.Registry.Business.ClientAndAgentBrandingCollection":
				case "Enterprise.Registry.Business.DocumentBrandingCollection":
				case "Enterprise.Registry.Business.ClientTariffAndLevelCollection":
				case "Enterprise.Registry.Business.GPSMessageTokenCollection":
				case "Enterprise.Registry.Business.ChargeGroupAndChargeCodeCollection":
				case "Enterprise.Registry.Business.EmailFieldCollection":
				case "Enterprise.Registry.Business.EmailSubjectFieldCollection":
				case "Enterprise.Registry.Business.IncoTermChargeCodesCollection":
				case "Enterprise.Registry.Business.InvoiceCopyCollection":
				case "Enterprise.Registry.Business.PortAuthoritySettingCollection":
				case "Enterprise.Registry.Business.RegistryImageCollection":
				case "Enterprise.Registry.Business.Customs.EntryChargeTypeSettingCollection":
				case "Enterprise.Registry.Business.OrgCodeElementCollection":
				case "Enterprise.Registry.Business.LocationsChargesCollection":
				case "Enterprise.Registry.Business.ReportOrderCollection":
				case "Enterprise.Registry.Business.CountryExportStatementSettingCollection":
				case "Enterprise.Registry.Business.Customs.US.CommercialChargeCollection":
				case "Enterprise.Registry.Business.AgentDocumentBrandCollection":
				case "Enterprise.Registry.Business.ExportStatementSettingCollection":
				case "Enterprise.Registry.Business.CreditCardFeeCollection":
				case "Enterprise.Registry.Business.CustomNoteTypeItemCollection":
				case "Enterprise.Registry.Business.Testing.DummyRegistryBusinessObjectCollection":
				case "Enterprise.Registry.Business.Customs.US.DeliveryTermCollection":
				case "Enterprise.Registry.Business.Customs.AU.DefaultPremiseIDCollection":
				case "Enterprise.Registry.Business.CodeDescriptionBoolCollection":
				case "Enterprise.Registry.Business.SystemDefinableCodeDescriptionBoolCollection":
				case "Enterprise.Registry.Business.OpportunityTypeCollection":
				case "Enterprise.Registry.Business.LandedCostingGroupCollection":
				case "Enterprise.Registry.Business.EmailSignatureFieldCollection":
				case "Enterprise.Registry.Business.DefaultRoundingsCollection":
				case "Enterprise.Registry.Business.ParentCodeDescriptionBoolCollection":
				case "Enterprise.Registry.Business.OrgCodeOrgTypeCollection":
				case "Enterprise.Registry.Business.CodeSelectionCollection":
				case "Enterprise.Registry.Business.ShipmentInspectionTypeCollection":
				case "Enterprise.Registry.Business.DefaultContainerModesCollection":
				case "Enterprise.Registry.Business.CountryListCollection":
				case "Enterprise.Registry.Business.CustomNoteModuleAndCountryCollection":
				case "Enterprise.Registry.Business.LegTypeCollection":
				case "Enterprise.Registry.Business.HouseBillOfLadingTypeCollection":
				case "Enterprise.Registry.Business.Customs.AU.ContingencyDataEmailAddressCollection":
				case "Enterprise.Registry.Business.CartageLegMessageCollection":
				case "Enterprise.Registry.Business.ChargeCodeCollection":
				case "Enterprise.Registry.Business.Internal.DecimalLineCollection":
				case "Enterprise.Registry.Business.HybridDocumentBrandCollection":
				case "Enterprise.Registry.Business.HouseBillOfLadingTermsAndConditionsCollection":
				case "Enterprise.Registry.Business.PortAuthorityPortCollection":
				case "Enterprise.ResourceStrings.Business.HelpDataStringCollection":
				case "Enterprise.MasterFiles.Business.OrgSalesAssociatedActivityCollection": // ActiveBusinessObjectCollection does not support multiple BizObj types
				case "Enterprise.MasterFiles.Business.RefTimeZoneCollection":
				case "Enterprise.MasterFiles.Business.RefExchangeRateDependentCollection":
				case "Enterprise.MasterFiles.Business.RefCountryStatesCollection":
				case "Enterprise.MasterFiles.Business.WorkflowItemCollectionView":
				case "Enterprise.MasterFiles.Business.Testing.ProcessTaskBaseCollectionViewTest+TestProcessTaskBaseCollectionView":
				case "Enterprise.MasterFiles.Business.OrgTradeDetailByCompetitorCollection":
				case "Enterprise.MasterFiles.Business.OrgPartRelationCollection":
				case "Enterprise.MasterFiles.Business.OrgHeaderCollection":
				case "Enterprise.MasterFiles.Business.OrganisationsFindBoxCollection":
				case "Enterprise.MasterFiles.Business.DebtorOrCreditorCollection":
				case "Enterprise.MasterFiles.Business.OrgCompanyDataCollection":
				case "Enterprise.MasterFiles.Business.GlbTimeAllocationCollectionView":
				case "Enterprise.MasterFiles.Business.GlbStaffAndResourceCollection":
				case "Enterprise.MasterFiles.Business.GlbSecurityCollectionView":
				case "Enterprise.MasterFiles.Business.GlbCertificatesDependentCollection":
				case "Enterprise.MasterFiles.Business.DocumentFieldDefinitionCollection":
				case "Enterprise.MasterFiles.Business.RefCountryRulesCollection":
				case "Enterprise.MasterFiles.Business.RefZoneHeaderCollection":
				case "Enterprise.MasterFiles.Business.AccPaymentApprovalCollection":
				case "Enterprise.MasterFiles.Business.WorkflowItemCollectionIncludingRelatedView":
				case "Enterprise.MasterFiles.Business.MilestoneCollectionIncludingRelatedView":
				case "Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendeeCollection":
				case "Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendeeContactCollection":
				case "Enterprise.MasterFiles.Business.SimilarOrgMatchForApprovalCollection":
				case "Enterprise.MasterFiles.Business.OrgLandedCostingPrefsCollection":
				case "Enterprise.MasterFiles.Business.WarehouseClientCollection":
				case "Enterprise.MasterFiles.Business.WarehouseClientCollectionTest+OrganisationsForTest":
				case "Enterprise.MasterFiles.Business.PackDepotCollection":
				case "Enterprise.MasterFiles.Business.OrganisationsFindBoxCollectionTest+OrganisationsForTest":
				case "Enterprise.MasterFiles.Business.ForwarderOrBrokerOrCarrierOrServicesCollection":
				case "Enterprise.MasterFiles.Business.ShippingProviderCollection":
				case "Enterprise.MasterFiles.Business.ConsortiumShippingProviderCollection":
				case "Enterprise.MasterFiles.Business.ConsignorCollection":
				case "Enterprise.MasterFiles.Business.OrgCreditorGroupCollection":
				case "Enterprise.MasterFiles.Business.OrgAgentRelationshipCollection":
				case "Enterprise.MasterFiles.Business.LocationCollection":
				case "Enterprise.MasterFiles.Business.GlbStaffResourceTimeCollection":
				case "Enterprise.MasterFiles.Business.GlbTimeAllocationCollection":
				case "Enterprise.MasterFiles.Business.LicencedModuleCollection":
				case "Enterprise.MasterFiles.Business.MilestoneOrTriggerCollectionView":
				case "Enterprise.MasterFiles.Business.RefTimeZoneRuleCollection":
				case "Enterprise.MasterFiles.Business.ActiveProcessQueueLogCollectionView":
				case "Enterprise.MasterFiles.Business.OrgInvoiceTypeCollection":
				case "Enterprise.MasterFiles.Business.TransportClientCollection":
				case "Enterprise.MasterFiles.Business.RailShippingProviderCollection":
				case "Enterprise.MasterFiles.Business.ForwarderCollection":
				case "Enterprise.MasterFiles.Business.BondedWarehouseCollection":
				case "Enterprise.MasterFiles.Business.OrgCustomLabelsCollection":
				case "Enterprise.MasterFiles.Business.OrgCountryDataDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgBrandOrRelatedNameCollection":
				case "Enterprise.MasterFiles.Business.OrgAddressCollection":
				case "Enterprise.MasterFiles.Business.GlbGroupCollection":
				case "Enterprise.MasterFiles.Business.GlbCompanyCampaignCollection":
				case "Enterprise.MasterFiles.Business.ExchangeRateWrapperCollection":
				case "Enterprise.MasterFiles.Business.RefCountryRequiredDocumentCollection":
				case "Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkCollectionReadOnlyView":
				case "Enterprise.MasterFiles.Business.MergeOrgElementCollection`1":
				case "Enterprise.MasterFiles.Business.MergeOrgAddressCollection":
				case "Enterprise.MasterFiles.Business.SplitStmUpgradeCollection":
				case "Enterprise.MasterFiles.Business.OrgSecurityCollection":
				case "Enterprise.MasterFiles.Business.DocDeliveryContactCollection":
				case "Enterprise.MasterFiles.Business.TransportShippingProviderCollection":
				case "Enterprise.MasterFiles.Business.SeaShippingProviderCollection":
				case "Enterprise.MasterFiles.Business.ContainerYardCollection":
				case "Enterprise.MasterFiles.Business.AirCTOCollection":
				case "Enterprise.MasterFiles.Business.JobRequiredDocumentCollection":
				case "Enterprise.MasterFiles.Business.GlbGroupManyToManyCollection":
				case "Enterprise.MasterFiles.Business.GlbDeptChargesDependentCollection":
				case "Enterprise.MasterFiles.Business.RefCountryZoneCollection":
				case "Enterprise.MasterFiles.Business.AccChargeTypeOverrideCollection":
				case "Enterprise.MasterFiles.Business.StmTemplateCollection":
				case "Enterprise.MasterFiles.Business.OrganisationManyToManyCollection":
				case "Enterprise.MasterFiles.Business.RefCountryCollection":
				case "Enterprise.MasterFiles.Business.ProcessTaskCollectionView":
				case "Enterprise.MasterFiles.Business.Testing.ProcessTaskCollectionViewTest+TestProcessTaskCollectionView":
				case "Enterprise.MasterFiles.Business.ProcessQueueLogCollectionBase":
				case "Enterprise.MasterFiles.Business.ActiveProcessQueueLogCollection":
				case "Enterprise.MasterFiles.Business.OrgSupplierLinkCollection":
				case "Enterprise.MasterFiles.Business.OrgSecurityContactsCollection":
				case "Enterprise.MasterFiles.Business.OrgSalesCallCollection":
				case "Enterprise.MasterFiles.Business.TemporaryOrgRemover.TemporaryOrgCollection":
				case "Enterprise.MasterFiles.Business.ShipsAgencyPrincipalCollection":
				case "Enterprise.MasterFiles.Business.ShipsAgencyPrincipalCollectionWithSecurityCheck":
				case "Enterprise.MasterFiles.Business.DebtorCollection":
				case "Enterprise.MasterFiles.Business.OrgDebtorGroupBankDefaultCollection":
				case "Enterprise.MasterFiles.Business.GlbReleaseNoteReadCollectionByStaff":
				case "Enterprise.MasterFiles.Business.GlbDeptChargesCollection":
				case "Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnModeDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgCollectionNoteCollection":
				case "Enterprise.MasterFiles.Business.AccPeriodManagementCollection":
				case "Enterprise.MasterFiles.Business.SpecProvManyToManyCollection":
				case "Enterprise.MasterFiles.Business.UNDGqdtCollection":
				case "Enterprise.MasterFiles.Business.UNDGObsCollection":
				case "Enterprise.MasterFiles.Business.RefServiceLevelCollection":
				case "Enterprise.MasterFiles.Business.RefPremisesGateCodeCollection":
				case "Enterprise.MasterFiles.Business.SeaCTOCollection":
				case "Enterprise.MasterFiles.Business.CTOCollection":
				case "Enterprise.MasterFiles.Business.OrgCusCodeCollection":
				case "Enterprise.MasterFiles.Business.JobDocAddressDependentCollection":
				case "Enterprise.MasterFiles.Business.GlbPortDeliveryTimeCollection":
				case "Enterprise.MasterFiles.Business.OrgProfitSharePartyCollection":
				case "Enterprise.MasterFiles.Business.AccGLAccountDescriptorCollection":
				case "Enterprise.MasterFiles.Business.SubstanceManyToManyStowSegCollection":
				case "Enterprise.MasterFiles.Business.RefCurrencyCollection":
				case "Enterprise.MasterFiles.Business.OrgSalesCollection":
				case "Enterprise.MasterFiles.Business.OrgLandedCostingPrefChargesCollection":
				case "Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroupCollection":
				case "Enterprise.MasterFiles.Business.LocalTransportCollection":
				case "Enterprise.MasterFiles.Business.BrokerCollection":
				case "Enterprise.MasterFiles.Business.OrgContactAttributeCollection":
				case "Enterprise.MasterFiles.Business.OrgAddressCapabilityCollection":
				case "Enterprise.MasterFiles.Business.GlbSecurityAllowedPrincipalsView":
				case "Enterprise.MasterFiles.Business.StmTemplateFilteredCollection":
				case "Enterprise.MasterFiles.Business.DaylightSavingTimeZoneCollection":
				case "Enterprise.MasterFiles.Business.RefTimeZoneStartRuleCollection":
				case "Enterprise.MasterFiles.Business.RefEquipmentCollection":
				case "Enterprise.MasterFiles.Business.OrgBuyerLinkCollection":
				case "Enterprise.MasterFiles.Business.OrgPatternMatchCollection":
				case "Enterprise.MasterFiles.Business.Testing.OrgPatternMatchCollectionTest+TestOrgPatternMatchCollection":
				case "Enterprise.MasterFiles.Business.OrgAddressDependentCollection":
				case "Enterprise.MasterFiles.Business.Testing.OrgPatternMatchCollectionTest+AddressCollectionForTest":
				case "Enterprise.MasterFiles.Business.OrgOpportunityDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgMatchApprovalCollection":
				case "Enterprise.MasterFiles.Business.CompetitorCollection":
				case "Enterprise.MasterFiles.Business.OrgDebtorGroupCollection":
				case "Enterprise.MasterFiles.Business.JobDocAddressCollectionForPlugin":
				case "Enterprise.MasterFiles.Business.JobChargeCollection":
				case "Enterprise.MasterFiles.Business.GlbStaffManyToManyCollection":
				case "Enterprise.MasterFiles.Business.GlbResourceCollection":
				case "Enterprise.MasterFiles.Business.GlbCompanyCollection":
				case "Enterprise.MasterFiles.Business.ActiveUsersCollection":
				case "Enterprise.MasterFiles.Business.RefCityTownCollection":
				case "Enterprise.MasterFiles.Business.RefNMFCCollection":
				case "Enterprise.MasterFiles.Business.MergeOrgContactCollection":
				case "Enterprise.MasterFiles.Business.AccTransactionHeaderCollection":
				case "Enterprise.MasterFiles.Business.TransactionCollection":
				case "Enterprise.MasterFiles.Business.AccGLHeaderCollection":
				case "Enterprise.MasterFiles.Business.AccChargeTaxOverrideCollection":
				case "Enterprise.MasterFiles.Business.SubstanceManyToManySpecProvCollection":
				case "Enterprise.MasterFiles.Business.UNDGPropsCollection":
				case "Enterprise.MasterFiles.Business.StmUpgradeCollectionView":
				case "Enterprise.MasterFiles.Business.StmMenuItemCollection":
				case "Enterprise.MasterFiles.Business.DocAutoDeliverStmMenuItemCollection":
				case "Enterprise.MasterFiles.Business.RefLocoMapCollection":
				case "Enterprise.MasterFiles.Business.ProcessQueueCollection":
				case "Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendeeStaffCollection":
				case "Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendeeOtherCollection":
				case "Enterprise.MasterFiles.Business.OrgAppointedAgentPortsDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgExclusiveGatewayServiceCollection":
				case "Enterprise.MasterFiles.Business.GlbStaffCollection":
				case "Enterprise.MasterFiles.Business.GlbStaffForGroupCollection":
				case "Enterprise.MasterFiles.Business.GlbSecurityChangeOthersView":
				case "Enterprise.MasterFiles.Business.GlbDepartmentCollection":
				case "Enterprise.MasterFiles.Business.GlbBranchDependentCollection":
				case "Enterprise.MasterFiles.Business.RefDomesticCartageZoneCollection":
				case "Enterprise.MasterFiles.Business.AccTransactionLinesCollection":
				case "Enterprise.MasterFiles.Business.AccGLAggregateCollection":
				case "Enterprise.MasterFiles.Business.RefUNLOCOCollection":
				case "Enterprise.MasterFiles.Business.RefTimeZoneSetCollection":
				case "Enterprise.MasterFiles.Business.CusRefPacksCollection":
				case "Enterprise.MasterFiles.Business.OrgSupplierPartCollection":
				case "Enterprise.MasterFiles.Business.DepotCollection":
				case "Enterprise.MasterFiles.Business.OrgCompanyDataDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgColdCallRegisterCollection":
				case "Enterprise.MasterFiles.Business.GlbReleaseNoteCollection":
				case "Enterprise.MasterFiles.Business.GlbBranchCollection":
				case "Enterprise.MasterFiles.Business.StmChangeLogCollection":
				case "Enterprise.MasterFiles.Business.AccChargeCodeCollection":
				case "Enterprise.MasterFiles.Business.StowSegManyToManyCollection":
				case "Enterprise.MasterFiles.Business.RefCommodityCodeCollection":
				case "Enterprise.MasterFiles.Business.RefCarrierConsortiumCollection":
				case "Enterprise.MasterFiles.Business.RefAirlineCollection":
				case "Enterprise.MasterFiles.Business.ProcessTaskTemplateCollection":
				case "Enterprise.MasterFiles.Business.Testing.ProcessTaskTemplateCollectionTest+TestProcessTaskTemplateCollection":
				case "Enterprise.MasterFiles.Business.OrgRateTariffLevelCollection":
				case "Enterprise.MasterFiles.Business.OrgProfitShareDetailsCollection":
				case "Enterprise.MasterFiles.Business.OrgProfitShareDetailsDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgProfitShareDetailsGenericCollection":
				case "Enterprise.MasterFiles.Business.OrgProfitShareDetailsClientSpecificCollection":
				case "Enterprise.MasterFiles.Business.OrgPatternMatchOverrideCollection":
				case "Enterprise.MasterFiles.Business.OrgOpportunityValueCollection":
				case "Enterprise.MasterFiles.Business.FumigationContractorCollection":
				case "Enterprise.MasterFiles.Business.ConsigneeCollection":
				case "Enterprise.MasterFiles.Business.AirCTOAndDepotCollection":
				case "Enterprise.MasterFiles.Business.OrgDocumentCollection":
				case "Enterprise.MasterFiles.Business.HierarchicalContactItemCollection":
				case "Enterprise.MasterFiles.Business.RequiredDocToBulkUpdateCollection":
				case "Enterprise.MasterFiles.Business.GlbStaffHolidayCollection":
				case "Enterprise.MasterFiles.Business.WorkflowTriggerCollectionView":
				case "Enterprise.MasterFiles.Business.Testing.WorkflowTriggerCollectionViewTest+TestWorkflowTriggerCollectionView":
				case "Enterprise.MasterFiles.Business.StmMenuTemplatePivotCollection":
				case "Enterprise.MasterFiles.Business.OrgQueryClaimDependentCollection":
				case "Enterprise.MasterFiles.Business.BaseRefPacksCollection":
				case "Enterprise.MasterFiles.Business.RefExchangeRateCollection":
				case "Enterprise.MasterFiles.Business.ExceptionCollectionView":
				case "Enterprise.MasterFiles.Business.ProcessQueueLogCollection":
				case "Enterprise.MasterFiles.Business.Testing.ProcessQueueLogCollectionTest+ProcessQueueLogCollectionForTest":
				case "Enterprise.MasterFiles.Business.OrgPartLocationCollection":
				case "Enterprise.MasterFiles.Business.OrgStaffAssignmentsCollection":
				case "Enterprise.MasterFiles.Business.Testing.OrgStaffAssignmentsCollectionNonCompanySpecific":
				case "Enterprise.MasterFiles.Business.OrgPartUnitCollection":
				case "Enterprise.MasterFiles.Business.AirShippingProviderCollection":
				case "Enterprise.MasterFiles.Business.OrgContactDependentCollection":
				case "Enterprise.MasterFiles.Business.RefZoneCountryCollection":
				case "Enterprise.MasterFiles.Business.EDICommunicationsModeDependentCollection":
				case "Enterprise.MasterFiles.Business.AccGroupsCollection":
				case "Enterprise.MasterFiles.Business.AccGLAccountDescriptorDependentCollection":
				case "Enterprise.MasterFiles.Business.AccChequeBookCollection":
				case "Enterprise.MasterFiles.Business.AccBankAccountCollection":
				case "Enterprise.MasterFiles.Business.AccEPaymentBeneficiaryCollection":
				case "Enterprise.MasterFiles.Business.StmUpgradeCollection":
				case "Enterprise.MasterFiles.Business.RefVesselCollection":
				case "Enterprise.MasterFiles.Business.ActiveServiceLevelCollection":
				case "Enterprise.MasterFiles.Business.RefDocTypeCollection":
				case "Enterprise.MasterFiles.Business.RefDocTypeFindboxCollection":
				case "Enterprise.MasterFiles.Business.ProcessTaskExtraResourceCollection":
				case "Enterprise.MasterFiles.Business.SeaCTOAndDepotCollection":
				case "Enterprise.MasterFiles.Business.CTOOrDepotOrWarehouseCollection":
				case "Enterprise.MasterFiles.Business.CustomFormsCollection":
				case "Enterprise.MasterFiles.Business.OrgContainerDetentionDependentCollection`1":
				case "Enterprise.MasterFiles.Business.OrgCountryDataAddressDependentCollection":
				case "Enterprise.MasterFiles.Business.RefTimeZoneEndRuleCollection":
				case "Enterprise.MasterFiles.Business.RefCountryStatesDependentCollection":
				case "Enterprise.MasterFiles.Business.MilestoneCollectionView":
				case "Enterprise.MasterFiles.Business.ActiveProcessQueueCollection":
				case "Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkCollection":
				case "Enterprise.MasterFiles.Business.SalesOrganisationCollection":
				case "Enterprise.MasterFiles.Business.CustomDocumentsCollection":
				case "Enterprise.MasterFiles.Business.OrgContactCollection":
				case "Enterprise.MasterFiles.Business.OrgAddressCapabilityWrapperCollection":
				case "Enterprise.MasterFiles.Business.JobHeaderCollection":
				case "Enterprise.MasterFiles.Business.GlbSecurityCollection":
				case "Enterprise.MasterFiles.Business.RefUNLOCOZoneCollection":
				case "Enterprise.MasterFiles.Business.AccWithholdingCollection":
				case "Enterprise.MasterFiles.Business.NonConsortiumVesselCollection":
				case "Enterprise.MasterFiles.Business.OrgTradeDetailCollection":
				case "Enterprise.MasterFiles.Business.OrgMiscServCollection":
				case "Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPortsDependentCollection":
				case "Enterprise.MasterFiles.Business.JobRequiredDocumentDependentCollection":
				case "Enterprise.MasterFiles.Business.JobRequiredDocumentAddInfoCollection":
				case "Enterprise.MasterFiles.Business.GlbGroupForStaffCollection":
				case "Enterprise.MasterFiles.Business.GlbCompanyCampaignItemContactDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgCollectionCallCollection":
				case "Enterprise.MasterFiles.Business.ConsignorForWebCollection":
				case "Enterprise.MasterFiles.Business.ConsigneeForWebCollection":
				case "Enterprise.MasterFiles.Business.JobChargeAttribCollection":
				case "Enterprise.MasterFiles.Business.AccTaxRateCollection":
				case "Enterprise.MasterFiles.Business.AccountDetailsDependentCollection":
				case "Enterprise.MasterFiles.Business.UNDGSubstanceCollection":
				case "Enterprise.MasterFiles.Business.RefCarrierConsortiumRefVesselDependentCollection":
				case "Enterprise.MasterFiles.Business.OrgOpportunityCollection":
				case "Enterprise.MasterFiles.Business.JobDocAddressCollection":
				case "Enterprise.MasterFiles.Business.GlbHolidayDependentCollection":
				case "Enterprise.MasterFiles.Business.EmailToContactBusinessObjectCollection":
				case "Enterprise.MasterFiles.Business.OrgCarrierServiceLevelCollection":
				case "Enterprise.MasterFiles.Business.UNDGStowSegCollection":
				case "Enterprise.MasterFiles.Business.UNDGSpecProvCollection":
				case "Enterprise.MasterFiles.Business.RefContainerCollection":
				case "Enterprise.MasterFiles.Business.ExceptionCollectionIncludingRelatedView":
				case "Enterprise.MasterFiles.Business.UnpackDepotCollection":
				case "Enterprise.MasterFiles.Business.LineHaulShippingProviderCollection":
				case "Enterprise.MasterFiles.Business.CreditorCollection":
				case "Enterprise.MasterFiles.Business.GlbPersonLanguageCollection":
				case "Enterprise.MasterFiles.Business.GlbBranchExtraPortsDependentCollection":
				case "Enterprise.MasterFiles.Business.StmFieldChangeLogCollection":
				case "Enterprise.MasterFiles.Business.RefZoneUNLOCOCollection":
				case "Enterprise.MasterFiles.Business.AccQueryClaimCollection":
				case "Enterprise.Services.OperationalActions.Support.Testing.OperationalActionTestFieldSupporterListTest+DummyChildCollectionForAction":
				case "Enterprise.Services.OperationalActions.Support.Testing.OperationalActionTestFieldSupporterListTest+DummyChildCollectionForActionSub":
				case "Enterprise.GPS.GPSSupporterActivityCollection":
				case "Enterprise.MasterFiles.GUI.Testing.DocAddressesPlugInTest+JobDocAddressParentForTestingCollection":
				case "Enterprise.MasterFiles.GUI.ZDocAddressControlTest+DummyWithDocAddressCollection":
				case "Enterprise.MasterFiles.Module.Testing.ModuleGridTaskStatusChangerTest+DummyWithWorkflowCollection":
				case "Enterprise.MasterFiles.Module.OrgMatchApprovalModule+ModuleOrgMatchApprovalCollection":
				case "Enterprise.ZArchitecture.Web.Business.Utilities.GridLayoutElementCollection":
				case "Enterprise.ZArchitecture.Web.Business.Utilities.Testing.WebCollectionSorterTest+DummyBusinessObjectWithRelatedDummyCollection":
				case "Enterprise.ZArchitecture.Web.Business.Utilities.Testing.WebCollectionSorterTest+DummyBusinessObjectWithNullPropertyCollection":
				case "Enterprise.ZArchitecture.Web.GUI.WebControls.ZDropDownList+Test+DummyNonPersistentBusinessObjectCollection":
				case "Enterprise.ZArchitecture.Web.GUI.WebControls.ZDataGrid+ZDataGridContextEnabledTest+DummyBusinessObjectWithRefCountryLookupCollection":
				case "Enterprise.Scheduler.Business.StmScheduleTaskCollection":
				case "Enterprise.Scheduler.Business.StmScheduleTaskRecipientDependentCollection":
				case "Enterprise.DeniedPartyScreening.Business.ComplianceRuleCollection": // ActiveBusinessObjectCollection does not support multiple foreign key relationships
				case "Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipientDependentCollection":
				case "Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldCountryCollection":
				case "Enterprise.DocumentEngine.Business.StmMenuTemplatePivotBaseCollection":
				case "Enterprise.DocumentEngine.Business.StmMenuDocumentConfigItemCollection":
				case "Enterprise.DocumentEngine.Business.StmTemplateBaseCollection":
				case "Enterprise.DocumentEngine.Business.StmMenuItemBaseCollection":
				case "Enterprise.DocumentEngine.ReportCollection":
				case "Enterprise.DocumentEngine.DocumentPack":
				case "Enterprise.DocumentEngine.Testing.DocumentPackTest+TestableDocumentPack":
				case "Enterprise.DocumentEngine.DocumentMenu.StmMenuDocumentConfigForm+PreviewDocumentPack":
				case "Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldDependentCollection":
				case "Enterprise.DocumentEngine.DocumentCommandCollection":
				case "Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueueCollection":
				case "Enterprise.DocumentEngine.Scheduler.Business.StmPrintJobCollection":
				case "Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskDependentCollection":
				case "Enterprise.DocumentEngine.Testing.DocumentOutputTest+PackingLineCollection":
				case "Enterprise.DocumentEngine.Business.StmMenuMenuPivotBaseCollection":
				case "Enterprise.DocumentEngine.DeliverableCollectionView":
				case "Enterprise.DocumentEngine.DeliverableCollectionViewTest+DeliverableCollectionViewForTesting":
				case "Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing.TotalFunctionExtractorTest+MyWrapperCollection":
				case "Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumnCollection":
				case "Enterprise.DocumentEngine.CustomisableDocumentConfiguration.TemplateSectionCollectionView":
				case "Enterprise.DocumentEngine.Business.DocumentStmMenuEDocsDependentCollectionView":
				case "Enterprise.DocumentEngine.Unit_Testing_Utility_Classes.BusinessObjectCollectionForTesting":
				case "Enterprise.DocumentEngine.Unit_Testing_Utility_Classes.BusinessObjectCollectionForTestingWithNoFactoryOnlyConstructor":
				case "Enterprise.DocumentEngine.Unit_Testing_Utility_Classes.DocumentWrapperCollectionForTestingNotAllowNew":
				case "Enterprise.DocumentEngine.Unit_Testing_Utility_Classes.DocumentWrapperCollectionForTesting":
				case "Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldWrapperCollection":
				case "Enterprise.DocumentEngine.DataProviders.Testing.MethodInfoChainLinkTest+DeskWrapperCollection":
				case "Enterprise.DocumentEngine.DataProviders.Testing.BusinessObjectReflectorTest+DocBravoCollection":
				case "Enterprise.DocumentEngine.DataProviders.Testing.BusinessObjectReflectorTest+DocCharlieCollection":
				case "Enterprise.DocumentEngine.RuntimeOptions.UserControlProviderList":
				case "Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing.FormatFunctionExtractorTest+MyWrapperCollection":
				case "Enterprise.DocumentEngine.DeliveryInstructionsCollection":
				case "Enterprise.DocumentEngine.Testing.PrintTaskTest+TestableDocumentPackWithContacts":
				case "Enterprise.DocumentEngine.Testing.PrintTaskTest+MockDocumentPack":
				case "Enterprise.DocumentEngine.Testing.PrintTaskTest+TestableDocumentPack":
				case "Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldCollection":
				case "Enterprise.DocumentEngine.Business.StmMenuDocumentConfigDependentCollection":
				case "Enterprise.DocumentEngine.Scheduler.Business.StmPrintJobMergedCollection":
				case "Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskCollection":
				case "Enterprise.DocumentEngine.ReportCommandCollection":
				case "Enterprise.DocumentEngine.Testing.ReportAnalyserTest+TestableDocumentPack":
				case "Enterprise.DocumentEngine.DocumentStmMenuEDocsDependentCollection":
				case "Enterprise.DocumentEngine.Testing.UtilityClasses.DocumentPackAlwaysIncludesSections":
				case "Enterprise.DocumentEngine.CustomisableDocumentConfiguration.TemplateSectionCollection":
				case "Enterprise.DocumentEngine.WebReportCommandCollection":
				case "Enterprise.Services.OperationalActions.Business.OperationalActionMethodApplicatorCollection":
				case "Enterprise.Services.OperationalActions.Business.MenuEditableBusinessObjectCollection`1":
				case "Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptorCollection":
				case "Enterprise.Services.OperationalActions.Business.OperationalActionDocumentPivotCollection":
				case "Enterprise.Services.OperationalActions.Business.OperationalActionMethodDescriptorCollection":
				case "Enterprise.Services.OperationalActions.Business.Testing.BaseUpdateNodeTest+DummyChildCollection":
				case "Enterprise.Services.OperationalActions.Business.OperationalActionCollection":
				case "Enterprise.Services.OperationalActions.Business.RunnerFieldCollection":
				case "Enterprise.Services.OperationalActions.Business.OperationalActionDocumentPivotView":
				case "Enterprise.Services.OperationalActions.Module.Testing.DummyModuleWithActionsSupport`1+Collection":
				case "Enterprise.MarketingManager.Business.GlbCompanyCampaignAttachmentItemCollection":
				case "Enterprise.MarketingManager.Business.GlbCompanyCampaignBudgetItemCollection":
				case "Enterprise.MarketingManager.Business.GlbCompanyCampaignItemCampaignDependentCollection":
				case "Enterprise.MarketingManager.Business.CampaignContactCollection":
				case "Enterprise.MarketingManager.Business.Voting.ExamSurveySubAnswerCollection":
				case "Enterprise.MarketingManager.Business.Voting.VoteExamSurveyAnswerCollection":
				case "Enterprise.MarketingManager.Business.Voting.VoteExamSurveyAnswerWrapperCollection":
				case "Enterprise.MarketingManager.Business.Voting.VoteExamSurveySubmittedAnswerCollection":
				case "Enterprise.MarketingManager.Business.Voting.Testing.VoteExamSurveyQuestionTest+QuestionSetCollectionForTest":
				case "Enterprise.MarketingManager.Business.Voting.VoteExamSurveyQuestionCollection":
				case "Enterprise.MarketingManager.Business.Voting.VoteExamSurveySubQuestionCollection":
				case "Enterprise.MailManager.Business.MailAttachmentCollection":
				case "Enterprise.MailManager.Business.MailItemCollection":
				case "Enterprise.MailManager.Business.StandardMailItemCollection":
				case "Enterprise.MailManager.Business.MailRecipientCollection":
				case "Enterprise.Registry.GUI.MarkUpPercentagesCollection":
				case "Enterprise.Registry.GUI.OrgHeaderCodeListCollection":
				case "Enterprise.Registry.GUI.AccChargeCodeModuleForRegistry+AccChargeCodeCollectionForRegistry":
				case "Enterprise.Registry.GUI.ZContactBusinessObject+FilteredOrgContactCollection":
				case "Enterprise.Registry.GUI.DepartmentMappingCollection":
				case "Enterprise.Registry.GUI.HAWBDocumentPivotCollection":
				case "Enterprise.Registry.GUI.AccChargeCodeListCollection":
				case "Enterprise.Messaging.Business.EDIInterchangeEDIMessageCollection":
				case "Enterprise.Customs.AU.PRA.Business.PRAContainerCollection":
				case "Enterprise.Messaging.Business.EDIInterchangeCollection":
				case "Enterprise.Accounting.Integration.JobProfitLossCollection":
				case "Enterprise.Messaging.Business.EDIMessageAttachDependentCollection":
				case "Enterprise.Messaging.Business.EDIMessageCollection":
				case "Enterprise.Messaging.Business.NonDependentEDIMessageCollection":
				case "Enterprise.Messaging.Business.EDIMessageFlattenedCollection":
				case "Enterprise.Messaging.Business.EDIMessageCollectionView":
				case "Enterprise.DataTransfer.Business.XmlDataImporter+ImportedBusinessObjectCollection":
				case "Enterprise.DataTransfer.GUI.XmlDataTransferDirector+ImportedBusinessObjectCollection":
				case "Enterprise.DataTransfer.Xml.Testing.ValueObjectDataAdapterTest`2+TestBusinessObjectCollection":
				case "Enterprise.Warehouse.Environment.Business.WhsWarehouseCollection":
				case "Enterprise.Warehouse.Environment.Module.WhsRowTypedBusinessObjectCollection":
				case "Enterprise.Warehouse.Environment.Module.WhsAreaTypedBusinessObjectCollection":
				case "Enterprise.Rating.Business.RatingHeaderCollection":
				case "Enterprise.Rating.Business.RateLinesCollection":
				case "Enterprise.Rating.Business.QuoteFormatEntryCollection":
				case "Enterprise.Rating.Business.RateEntryCollection":
				case "Enterprise.Rating.Business.WHSRateEntryCollection":
				case "Enterprise.Rating.Business.AIRRateEntryCollection":
				case "Enterprise.Rating.Business.LCLRateEntryCollection":
				case "Enterprise.Rating.Business.RateAttachmentCollection":
				case "Enterprise.Rating.Business.QuotationStmTemplateCollection":
				case "Enterprise.Rating.Business.QuoteCollection":
				case "Enterprise.Rating.Business.UnacceptedQuotesCollection":
				case "Enterprise.Rating.Business.RateTransportZonesCollection":
				case "Enterprise.Rating.Business.RelatedRateLinesCollection":
				case "Enterprise.Rating.Business.ORGRateEntryCollection":
				case "Enterprise.Rating.Business.Testing.ORGRateEntryCollectionTest+TstORGRateEntryCollection":
				case "Enterprise.Rating.Business.CSTRateEntryCollection":
				case "Enterprise.Rating.Business.JobStorageCollection":
				case "Enterprise.Rating.Business.UNPRateEntryCollection":
				case "Enterprise.Rating.Business.AutoRateInfoCollection":
				case "Enterprise.Rating.Business.AutoRateInfoCollectionReadOnlyView":
				case "Enterprise.Rating.Business.CompanyTariffCollection":
				case "Enterprise.Rating.Business.SimpleRateLinesCollection":
				case "Enterprise.Rating.Business.TRARateEntryCollection":
				case "Enterprise.Rating.Business.SimpleRateEntryCollection":
				case "Enterprise.Rating.Business.FCLRateEntryCollection":
				case "Enterprise.Rating.Business.RateCollection":
				case "Enterprise.Rating.Business.RateOneOffShipmentCollection":
				case "Enterprise.Rating.Business.RateOneOffShipmentExRateCollection":
				case "Enterprise.Rating.Business.CartageZoneCollection":
				case "Enterprise.Rating.Business.PACRateEntryCollection":
				case "Enterprise.Rating.Business.CostsComparerEntryCollection":
				case "Enterprise.Rating.Business.RateLineItemsCollection":
				case "Enterprise.Rating.Business.RelatedRateLineItemsCollection":
				case "Enterprise.Rating.Business.CostingCollection":
				case "Enterprise.Rating.Business.RateOneOffContainersCollection":
				case "Enterprise.Rating.Business.SummaryRateEntryCollection":
				case "Enterprise.Rating.Business.RateTransportProviderCollection":
				case "Enterprise.Rating.Business.QuoteFormatTableCollection":
				case "Enterprise.Rating.Business.DSTRateEntryCollection":
				case "Enterprise.Rating.Business.Testing.DSTRateEntryCollectionTest+TstDSTRateEntryCollection":
				case "Enterprise.Rating.Business.RateAttachmentSetCollection":
				case "Enterprise.Rating.Business.RateLineItemMapperView":
				case "Enterprise.Rating.Business.RateTariffDiscountCollection":
				case "Enterprise.Rating.Business.UpdateRateCollection":
				case "Enterprise.Freight.Business.BaseDependentContainerCollection":
				case "Enterprise.Freight.Business.ContainerCollection":
				case "Enterprise.Freight.Business.Shipment+EmptyReadOnlyContainerCollection":
				case "Enterprise.Freight.Business.TopLevelShipmentCollection":
				case "Enterprise.Freight.Business.ContainerLegCollection":
				case "Enterprise.Freight.Business.ShipmentContainerCollection":
				case "Enterprise.Freight.Business.SailingContainerCollection":
				case "Enterprise.Freight.Business.ManyToManyShipmentCollection":
				case "Enterprise.Freight.Business.MainFormConsolCollection":
				case "Enterprise.Freight.Business.SlotAllocationDependentCollection":
				case "Enterprise.Freight.Business.PackLineCollection":
				case "Enterprise.Freight.Business.RoutingCollection":
				case "Enterprise.Freight.Business.ShipmentRoutingCollection":
				case "Enterprise.Freight.Business.ContainerToSelectFromForPrintingCollection":
				case "Enterprise.Freight.Business.ConsolCollection":
				case "Enterprise.Freight.Business.ContainerLegToSelectFromForPrintingCollection":
				case "Enterprise.Freight.Business.ContainerNonDependentCollection":
				case "Enterprise.Freight.Business.PackLineNonDependentCollection":
				case "Enterprise.Freight.Business.InternalPackLineNonDependentCollection":
				case "Enterprise.Freight.Business.PackLinesForSailingsCollection":
				case "Enterprise.Freight.Business.UnAllocatedPackLinesView":
				case "Enterprise.Freight.Business.UnAllocatedPackLinesForSailing":
				case "Enterprise.Freight.Business.ContainerCommodityCodeCollection":
				case "Enterprise.Freight.Business.BaseJobSailingCollection":
				case "Enterprise.Freight.Business.VoyageExRateDependentCollection":
				case "Enterprise.Freight.Business.JobServiceDependentCollection":
				case "Enterprise.Freight.Business.ContainerLegNonDependentCollection":
				case "Enterprise.Freight.Business.SailingsByCountryView":
				case "Enterprise.Freight.Business.FreightJobMawbLink":
				case "Enterprise.Freight.Business.BasePackLineManyToManyCollection":
				case "Enterprise.Freight.Business.PackLineManyToManyCollection":
				case "Enterprise.Freight.Business.ShipmentCollection":
				case "Enterprise.Freight.Business.CoLoadShipmentCollection":
				case "Enterprise.Freight.Business.PackLocationNonDependentCollection":
				case "Enterprise.Freight.Business.Testing.RoutingCollectionForTest":
				case "Enterprise.Freight.Business.JobSailingCollection":
				case "Enterprise.Freight.Business.CharterStateSpecificCollection":
				case "Enterprise.Freight.Business.CharterSailingCollection":
				case "Enterprise.Freight.Business.CoLoadShipmentCollectionView":
				case "Enterprise.Freight.Business.CollectionDependantOrgContactCollection":
				case "Enterprise.Freight.Business.VoyageCountryDependentCollection":
				case "Enterprise.Freight.Business.JobVoyageCollection":
				case "Enterprise.Freight.Business.VoyageDestinationDependentCollection":
				case "Enterprise.Freight.Business.TransportCollection":
				case "Enterprise.Freight.Business.ConsolTransportCollection":
				case "Enterprise.Freight.Business.InnerPackLineCollection":
				case "Enterprise.Freight.Business.Testing.FreightBaseContainerDependentCollectionBOCollectionTest+FreightBaseContainerDependentCollectionForTest":
				case "Enterprise.Freight.Business.NonCharterSailingCollection":
				case "Enterprise.Freight.Business.VoyageOriginByCountryView":
				case "Enterprise.Freight.Business.UnAllocatedLCLShipmentView":
				case "Enterprise.Freight.Business.OuterPackLineCollection":
				case "Enterprise.Freight.Business.MostInterestingTransportBindingCollection":
				case "Enterprise.Freight.Business.CusEntryNumCollection":
				case "Enterprise.Freight.Business.ServiceToSelectFromForPrintingCollection":
				case "Enterprise.Freight.Business.ModuleShipmentCollection":
				case "Enterprise.Freight.Business.Testing.UnAllocatedPackLinesViewForTest":
				case "Enterprise.Freight.Business.OrderItemCollection":
				case "Enterprise.Freight.Business.PackLineCollectionCalculator+SubPackLineCollectionView":
				case "Enterprise.Freight.Business.CommonContainerManyToManyCollection":
				case "Enterprise.Freight.Business.VoyageOriginDependentCollection":
				case "Enterprise.Freight.Business.ConsolShipmentCollection":
				case "Enterprise.Freight.Business.PackLocationCollection":
				case "Enterprise.Freight.Business.JobMawbCollection":
				case "Enterprise.Freight.Business.DebtorToSelectFromForPrintingCollection":
				case "Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyageCollection":
				case "Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPairCollection":
				case "Enterprise.Freight.SailingDataVendor.Business.Testing.VesselRoutingPortPairCollectionTest+TestVesselRoutingPortPairCollection":
				case "Enterprise.Freight.Booking.Business.BookingContainerCollection":
				case "Enterprise.Freight.Forwarding.Business.ConsolForwardingShipmentCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingConsolCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingConsolManyToManyCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingModuleConsolCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingContainerManyToManyCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingShipmentContainerCollection_TEMP":
				case "Enterprise.Freight.Forwarding.Business.ForwardingShipmentContainerCollection":
				case "Enterprise.Freight.Forwarding.Business.DeliveryAgentToPrintCollection":
				case "Enterprise.Freight.Forwarding.Business.DeliveryAgentToSelectFromForPrintingCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.CIMEDIMessageCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ExportAWBAccountingInformationCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ConsolExportAWBAccountingInformationCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ExportAWBOtherChargesCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ConsolExportAWBOtherChargesCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ShipmentExportAWBOtherChargesCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLineCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ConsolExportAWBRateLineCollection":
				case "Enterprise.Freight.Forwarding.Business.AWB.ShipmentExportAWBRateLineCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.OrderToBulkUpdateCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.OrderContainerCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.PreAdviceOrderContainerCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanningCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.OrderDeliveryLineCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.RelatedOrderLineDeliverContainerCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.OrderLineNonDependantCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.OrderCollection":
				case "Enterprise.Freight.Forwarding.Orders.Business.OrderPreAdviceDependentCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingInnerPackLineCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingPackLineCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingPackLineManyToManyCollection":
				case "Enterprise.Freight.Forwarding.Business.QuickPODCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingConsolShipmentCollection":
				case "Enterprise.Freight.Forwarding.Business.ConsolForwardingShipmentCollectionWithBookingConversion":
				case "Enterprise.Freight.Forwarding.Business.ForwardingModuleShipmentCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingShipmentCollection_TEMP":
				case "Enterprise.Freight.Forwarding.Business.ForwardingShipmentCollection":
				case "Enterprise.Freight.Forwarding.Business.ShipmentNumberEntries":
				case "Enterprise.Freight.Forwarding.Business.TopLevelForwardingShipmentCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingContainerContainerLegDivotCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingContainerLegContainerDivotCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingTransportLegContainerDivotCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingPackLineDivotDependentCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingPackLineTransportLegManyToManyCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingTransportLegManyToManyCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingTransportLegPackLineDivotCollection":
				case "Enterprise.Freight.Forwarding.Business.ForwardingTransportCollection":
				case "Enterprise.Freight.Forwarding.Business.PreAdviceTransportCollection":
				case "Enterprise.Freight.Forwarding.DataTransfer.ImportedOrderLineDeliveryCollection":
				case "Enterprise.Freight.Forwarding.DataTransfer.ImportedOrderCollection":
				case "Enterprise.Freight.Forwarding.DataTransfer.ImportedOrderLineCollection":
				case "Enterprise.Freight.QuotedBookings.Business.ViewQuotedBookingCollection":
				case "Enterprise.Freight.QuotedBookings.Business.QuotedBookingContainerDependentCollection":
				case "Enterprise.Freight.Agency.Business.AgencyShipmentCollection":
				case "Enterprise.Freight.Agency.Business.AgencyBookingCollection":
				case "Enterprise.Freight.Agency.Business.AgencyShipmentContainerManyToManyCollection":
				case "Enterprise.Freight.Agency.Business.AgencyBookingContainerManyToManyCollection":
				case "Enterprise.Freight.Agency.Business.AgencyAllocationItemCollection`2":
				case "Enterprise.Freight.Agency.Business.AgencyOriginDependentCollection":
				case "Enterprise.Freight.Agency.Business.AgencyShipmentPackLineCollection":
				case "Enterprise.Freight.Agency.Business.AgencyShipmentPackLineManyToManyCollection":
				case "Enterprise.Freight.Agency.Business.BillOfLadingPackLineManyToManyCollection":
				case "Enterprise.Freight.Agency.Business.AgencyShipmentContainerDependentCollection":
				case "Enterprise.Freight.Agency.Business.AgencyBookingContainerDependentCollection":
				case "Enterprise.Freight.Agency.Business.AgencyBookingPackLineManyToManyCollection":
				case "Enterprise.Freight.Agency.Business.BillOfLadingList":
				case "Enterprise.Freight.Agency.Business.AgencyBookingPackLineCollection":
				case "Enterprise.Freight.Agency.Business.BillOfLadingContainerDependentCollection":
				case "Enterprise.Freight.Agency.Business.AgencySchedule+OrgHeaderCollectionWithoutNew":
				case "Enterprise.Freight.Agency.Business.BillOfLadingBookingContainerManyToManyCollection":
				case "Enterprise.Freight.Agency.Business.AgencyPrincipalCollection":
				case "Enterprise.Freight.Agency.Business.PortMessageHostCollection":
				case "Enterprise.Freight.Agency.Business.BillOfLadingPackLineCollection":
				case "Enterprise.Freight.Agency.Business.PortAuthorityFilterIssueCollection":
				case "Enterprise.Freight.Agency.Business.BillOfLadingCollection":
				case "Enterprise.Freight.Agency.Business.AgencySailingDependentCollection":
				case "Enterprise.Customs.Business.BaseJobDeclarationCollection":
				case "Enterprise.Customs.Business.CusSeaManOBLHeaderConsignorCollection":
				case "Enterprise.Customs.Business.BasePackingGroupCollection":
				case "Enterprise.Customs.Business.AllCusEntryLineCollection":
				case "Enterprise.Customs.Business.NonPersistentCusContainerCollection":
				case "Enterprise.Customs.Business.BaseJobComInvHeaderChargeCollection":
				case "Enterprise.Customs.Business.CommonNonApportionedChargeCollection":
				case "Enterprise.Customs.Business.CommonNonApportionedChargeCollection`1":
				case "Enterprise.Customs.Business.BaseGroupInvoiceChargeCollection":
				case "Enterprise.Customs.Business.CusSeaManArrivalPortCollection":
				case "Enterprise.Customs.Business.BaseBillContainerCollection":
				case "Enterprise.Customs.Business.CusContainerInvoiceLinePivotCollection":
				case "Enterprise.Customs.Business.OrgSupplierPartCollection":
				case "Enterprise.Customs.Business.ExportDeclarationCollection":
				case "Enterprise.Customs.Business.Testing.TestMessageCollection":
				case "Enterprise.Customs.Business.TestCollection":
				case "Enterprise.Customs.Business.BaseInvoiceLineChargeCollection":
				case "Enterprise.Customs.Business.ExportCustomsManifestLinesCollection":
				case "Enterprise.Customs.Business.CusSeaManTranHeadCollection":
				case "Enterprise.Customs.Business.ComInvHeaderReconciliationCollection":
				case "Enterprise.Customs.Business.CusHAWBDependentCollection":
				case "Enterprise.Customs.Business.CusEntryInstructionCollection":
				case "Enterprise.Customs.Business.GlobalCusEntryLineCollection":
				case "Enterprise.Customs.Business.CusBondDetailCollection`1":
				case "Enterprise.Customs.Business.CusUnderbondUnionCollectionParentCollection":
				case "Enterprise.Customs.Business.GenericCusOutturnCollection`2":
				case "Enterprise.Customs.Business.Testing.CusOutturnCollectionTest+DummyCusOutturnCollection":
				case "Enterprise.Customs.Business.EntryLineStatusFilterCollection":
				case "Enterprise.Customs.Business.CusEntryLineCollection":
				case "Enterprise.Customs.Business.Testing.CusEntryLineCollectionTest+TestCusEntryLineCollection":
				case "Enterprise.Customs.Business.ComInvLineCollection":
				case "Enterprise.Customs.Business.BillTypeViewCollection":
				case "Enterprise.Customs.Business.AdditionalLineLinkInvoiceLineCollection":
				case "Enterprise.Customs.Business.CommonApportionedChargeCollection":
				case "Enterprise.Customs.Business.CommonApportionedChargeCollection`1":
				case "Enterprise.Customs.Business.GenericCusUnderbondCollection`1":
				case "Enterprise.Customs.Business.CusUnderbondCollection":
				case "Enterprise.Customs.Business.Testing.CusUnderbondCollectionWithProvider":
				case "Enterprise.Customs.Business.CusOutturnHeaderCusOutturnCollection":
				case "Enterprise.Customs.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.Business.BaseInvoiceHeaderCollection":
				case "Enterprise.Customs.Business.Testing.BaseInvoiceLineCompleteCollectionTest+TestInvoiceHeaderCollection":
				case "Enterprise.Customs.Business.BaseGroupHeaderInvoiceLineViewCollection":
				case "Enterprise.Customs.Business.DocumentCusContainerCollection":
				case "Enterprise.Customs.Business.TariffBulkChange+TariffBulkChangeOldTariffCollection":
				case "Enterprise.Customs.Business.TariffBulkChange+TariffBulkChangeNewTariffCollection":
				case "Enterprise.Customs.Business.TariffBulkChange+TariffBulkChangerPartCollection":
				case "Enterprise.Customs.Business.BaseClassificationCollection":
				case "Enterprise.Customs.Business.TariffBulkChange+TBCClassificationCollection":
				case "Enterprise.Customs.Business.EntryHeaderInvoiceLineSubsetCollection":
				case "Enterprise.Customs.Business.CusInvPackDetailCollection":
				case "Enterprise.Customs.Business.CusInvPackDetailCollection`2":
				case "Enterprise.Customs.Business.InvoiceCusInvPackDetailCollection":
				case "Enterprise.Customs.Business.BaseApportionedChargeCollection":
				case "Enterprise.Customs.Business.InvoiceLinesForEntryLineCollection":
				case "Enterprise.Customs.Business.BaseDeclarationLevelPackingGroupCollection":
				case "Enterprise.Customs.Business.BillCollection":
				case "Enterprise.Customs.Business.Testing.BaseDeclarationLevelPackingGroupCollectionTest+TestHouseBillCollection":
				case "Enterprise.Customs.Business.Testing.BaseDeclarationLevelPackingGroupCollectionTest+TestPackGroupCollection":
				case "Enterprise.Customs.Business.BaseCusClassPartPivotCollection":
				case "Enterprise.Customs.Business.GroupHeaderCollection":
				case "Enterprise.Customs.Business.BaseInvoiceChargeCollection":
				case "Enterprise.Customs.Business.BaseCusContainerCollection":
				case "Enterprise.Customs.Business.CodeInfoCollection":
				case "Enterprise.Customs.Business.BillLevelInvoiceCollection":
				case "Enterprise.Customs.Business.CusSupImpClassOverrideCollection":
				case "Enterprise.Customs.Business.LowestBillCollection":
				case "Enterprise.Customs.Business.MultiJobDeclarationCollection":
				case "Enterprise.Customs.Business.CusUnderbondUnionCollection":
				case "Enterprise.Customs.Business.AdditionalLineLinkEntryLineCollection":
				case "Enterprise.Customs.Business.InvoiceHeaderWithNoDeclarationCollection":
				case "Enterprise.Customs.Business.CusSeaManOBLHeaderConsigneeCollection":
				case "Enterprise.Customs.Business.BaseDeclarationLevelPackageCollection":
				case "Enterprise.Customs.Business.ClassificationCollection":
				case "Enterprise.Customs.Business.BaseJobComInvoiceLineViewCollection":
				case "Enterprise.Customs.Business.CusSeaManOBLDetailCollection":
				case "Enterprise.Customs.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.Business.CusEntryHeaderChargesCollection":
				case "Enterprise.Customs.Business.ComInvOrderReconciliationCollection":
				case "Enterprise.Customs.Business.BaseJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.Business.CusSeaManOBLHeaderCollection":
				case "Enterprise.Customs.Business.CusUnderbondCusOutturnCollection":
				case "Enterprise.Customs.Business.BasePackingGroupContainerCollection":
				case "Enterprise.Customs.Business.CusContainersInvoiceLinesCollection":
				case "Enterprise.Customs.Business.BaseGroupInvoiceAllInvoiceHeaderCollection":
				case "Enterprise.Customs.Business.BaseGroupInvoiceDirectChildInvoiceHeaderCollection":
				case "Enterprise.Customs.Business.AllChargesCollection":
				case "Enterprise.Customs.Business.BaseInvoiceLineApportionedChargeCollection":
				case "Enterprise.Customs.Business.CusEntryPayInfoCollection":
				case "Enterprise.Customs.Business.BasePackageCollection":
				case "Enterprise.Customs.Business.CusLineTariffDetailCollection":
				case "Enterprise.Customs.Business.CusInvPackCollection`2":
				case "Enterprise.Customs.Business.BillCollectionForEntry":
				case "Enterprise.Customs.Business.BaseJobComInvoiceGroupHeaderSingleElementCollection":
				case "Enterprise.Customs.Business.CusSeaManOBLHeaderCollectionView":
				case "Enterprise.Customs.Business.CusEntryNumCollection":
				case "Enterprise.Customs.Business.EntryCollectionWithPassedEntries":
				case "Enterprise.Customs.Business.ChildBillCollection":
				case "Enterprise.Customs.Business.JobDeclarationLookups+AttachInvoiceCollection":
				case "Enterprise.Customs.Business.Testing.JobDeclarationLookupsTest+AttachInvoiceCollectionForTesting":
				case "Enterprise.Customs.Business.Testing.CusUnderbondUnionCollectionWithProvider":
				case "Enterprise.Customs.Business.BaseCusContainer+JobServiceCollectionWrapper":
				case "Enterprise.Customs.Business.InvoiceLineCusInvPackCollection":
				case "Enterprise.Customs.Business.ActiveCusEntryHeaderCollection":
				case "Enterprise.Customs.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.Business.CusSeaManSlotOrgCollection":
				case "Enterprise.Customs.Business.InvoiceLineDependentCollection":
				case "Enterprise.Customs.Business.InvoiceLineCusInvPackDetailCollection":
				case "Enterprise.Customs.Business.CusCodeDataCollection`1":
				case "Enterprise.Customs.Business.CusEntryHeaderMessageStatusSubsetCollection":
				case "Enterprise.Customs.Business.Testing.CusEntryHeaderMessageStatusSubsetCollectionTest+DummyEntryHeaderMessageStatusCollection":
				case "Enterprise.Customs.Business.InvoiceCusInvPackCollection":
				case "Enterprise.Customs.DataTransfer.Testing.DeclarationXmlValueObjectSerializerTest+ImportedBusinessObjectCollection":
				case "Enterprise.Customs.SG.Declaration.Business.TariffCommodityCollection":
				case "Enterprise.Customs.SG.Declaration.Business.SGCusClassPartPivotCollection":
				case "Enterprise.Customs.SG.Declaration.Business.SupplierPartManyToManyCollection":
				case "Enterprise.Customs.SG.Declaration.Business.DutyCollection":
				case "Enterprise.Customs.SG.Declaration.Business.TariffCollection":
				case "Enterprise.Customs.SG.Declaration.Business.TariffCommoditiesCollection":
				case "Enterprise.Customs.SG.V4.Business.SGPlacesCombinedCollection":
				case "Enterprise.Customs.SG.Declaration.Business.ClassificationCollection":
				case "Enterprise.Freight.CFS.Business.CFSServiceDependentCollection":
				case "Enterprise.Freight.CFS.Business.TallyServiceDependentCollection":
				case "Enterprise.Freight.CFS.Business.GatePassLoadListConsolManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.CFSShipmentList":
				case "Enterprise.Freight.CFS.Business.CFSRequiredDocumentDependentCollection":
				case "Enterprise.Freight.CFS.Business.PackUnpackRequiredDocumentDependentCollection":
				case "Enterprise.Freight.CFS.Business.CFSPackLineManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.TallyPackLineManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.CFSShipmentDependentCollection":
				case "Enterprise.Freight.CFS.Business.CFSInnerPackLineCollection":
				case "Enterprise.Freight.CFS.Business.GatePassContainerLegToSelectFromForPrintingCollection":
				case "Enterprise.Freight.CFS.Business.CFSLoadListConsolCollection":
				case "Enterprise.Freight.CFS.Business.GatePassInnerPackLineCollection":
				case "Enterprise.Freight.CFS.Business.CFSContainerManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.TallyContainerManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.GatePassContainerManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.GatePassRequiredDocumentDependentCollection":
				case "Enterprise.Freight.CFS.Business.GatePassServiceDependentCollection":
				case "Enterprise.Freight.CFS.Business.CFSContainerCollection":
				case "Enterprise.Freight.CFS.Business.TallyContainerDependentCollection":
				case "Enterprise.Freight.CFS.Business.GatePassContainerCollection":
				case "Enterprise.Freight.CFS.Business.CFSShipmentContainerCollection":
				case "Enterprise.Freight.CFS.Business.CFSShipmentCollection":
				case "Enterprise.Freight.CFS.Business.PackUnpackShipmentCollection":
				case "Enterprise.Freight.CFS.Business.TallyInnerPackLineCollection":
				case "Enterprise.Freight.CFS.Business.CFSPackLineCollection":
				case "Enterprise.Freight.CFS.Business.GatePassPackLineCollection":
				case "Enterprise.Freight.CFS.Business.CFSContainerRegistrationList":
				case "Enterprise.Freight.CFS.Business.TallyContainerCollection":
				case "Enterprise.Freight.CFS.Business.PackUnpackLoadListConsolCollection":
				case "Enterprise.Freight.CFS.Business.PackLineForShipmentAndContainerCollection":
				case "Enterprise.Freight.CFS.Business.TallyContainerShipmentContainerCollection":
				case "Enterprise.Freight.CFS.Business.GatePassPackLineManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.CFSContainerLegToSelectFromForPrintingCollection":
				case "Enterprise.Freight.CFS.Business.PackUnpackShipmentList":
				case "Enterprise.Freight.CFS.Business.GatePassShipmentCollection":
				case "Enterprise.Freight.CFS.Business.PackUnpackLoadListConsolManyToManyCollection":
				case "Enterprise.Freight.CFS.Business.GatePassShipmentDependentCollection":
				case "Enterprise.Freight.CFS.Business.GatePassShipmentContainerCollection":
				case "Enterprise.Freight.CFS.Business.CFSPackLocationCollection":
				case "Enterprise.Freight.CFS.Business.GatePassLoadListConsolCollection":
				case "Enterprise.Freight.CFS.Business.GatePassShipmentList":
				case "Enterprise.Freight.CFS.Business.TallyPackLocationCollection":
				case "Enterprise.Freight.CFS.Business.CFSUnallocatedPackLinesView":
				case "Enterprise.Freight.CFS.Business.CFSPackLineNonDependentCollection":
				case "Enterprise.Freight.CFS.Business.PackUnpackShipmentDependentCollection":
				case "Enterprise.Freight.CFS.Business.TallyPackLineCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CMRDeclarationQuestionsCollection":
				case "Enterprise.Customs.AU.Declaration.Business.DrawbackCusEntryLineCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.ReferenceFiles.EXDOCAqisPlaceCollection":
				case "Enterprise.Customs.AU.Declaration.Business.QuarantineNonPersistentCusContainerCollection":
				case "Enterprise.Customs.AU.Declaration.Business.ImportManifestGridCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.EDIUserCodeCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.EstablishmentCodeCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.ReferenceFiles.EXDOCCutCodeCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.ReferenceFiles.EXDOCProductTypeCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.ReferenceFiles.EXDOCSupplementaryCodeCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.ExporterNumberCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.QuarantineExDocEstablishmentAndTimeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusOutturnHeaderDepotCusOutturnCollection":
				case "Enterprise.Customs.AU.CFS.Business.TallyOutturnHeaderOutturnCollection":
				case "Enterprise.Customs.AU.Declaration.Business.DeclarationLevelPackageCollection":
				case "Enterprise.Customs.AU.Declaration.Business.PackageCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceLineDependentCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.AU.Declaration.Business.SailingBillOfLadingCollection":
				case "Enterprise.Customs.AU.Declaration.Business.TemporaryManifestsCollection":
				case "Enterprise.Customs.AU.CFS.Business.CFSTallyContainerOutturnCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.QuarantineExdocLineCompleteCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AllCusEntryLineCollection":
				case "Enterprise.Customs.AU.CFS.Business.CFSShipmentWrapperDepotCusOutturnCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUExportTariffBulkChange+TariffBulkChangeOldTariffCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUExportTariffBulkChange+TariffBulkChangeNewTariffCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPartCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUExportTariffBulkChange+TariffBulkChangerPartCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUExportTariffBulkChange+TBCClassificationCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange+TariffBulkChangeOldTariffCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange+TariffBulkChangeNewTariffCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange+TariffBulkChangerPartCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange+TBCClassificationCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.CTOCusHAWBAndPartShipCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.CTOCusHAWBCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.CTOCusHAWBModuleCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.CusHAWBCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.ModuleHAWBCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.CusHAWBCollectionNonDependent":
				case "Enterprise.Customs.AU.AirCargo.Business.CusHAWBCollectionWithOneBill":
				case "Enterprise.Customs.AU.AirCargo.Business.CTOCusMAWBCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.CusMAWBCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.ModuleMAWBCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.CusPartShipCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.MAWBCusPartShipCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.HoldPrealertChooserCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISSingleValueCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISCommodityCodeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISConcernTypeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISDocumentCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISEntityIdCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISPackageCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISPermitIdCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISPremisesIdAndProcessingTypeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AQISProducerCodeCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRAqisCommodityCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRAqisConcernCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRAqisDocumentTypeCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRAqisPremisesCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRAqisProcessingTypeCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRAqisProducerCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRAqisEntityCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRBerthCodeCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRCodeListsCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRCommunityProtectionProfileCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRCommunityProtectionRiskCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRInstrumentTariffGroupCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRInstrumentCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRLodgementQuestionCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRPreferenceRulePeriodSnapshotCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRPreferenceSchemePeriodCountryCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRPreferenceSchemePeriodSnapshotCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRPreferenceSchemeRuleCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRRefundReasonCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRSACThesaurusCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRTariffClassificationCharacteristicCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRTariffRatePeriodCharacteristicCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRTariffRatePeriodSnapshotCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRTreatmentRatePeriodCharacteristicCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRTreatmentRatePeriodSnapshotCollection":
				case "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRTreatmentSnapshotCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusContainerCollection":
				case "Enterprise.Customs.AU.Declaration.Business.PackingGroupCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AllCPDecQuestionsViewCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AllEntryLineCPDecQuestion":
				case "Enterprise.Customs.AU.Declaration.Business.Testing.AllEntryLineCPDecQuestionTest+TestEntryLinesCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CachedAnsweredQuestions":
				case "Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDecCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CMROrgCusEntryCPDecCollection":
				case "Enterprise.Customs.AU.Declaration.Business.LineDefaultQuestions":
				case "Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderChargesCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusEntryLineCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceLinesForEntryLineCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusOutturnHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.DepotCusOutturnCollection":
				case "Enterprise.Customs.AU.Declaration.Business.DepotCusUnderbondCusOutturnCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManArrivalPortCollection":
				case "Enterprise.Customs.AU.Declaration.Business.BaseCusSeaManOBLDetailCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManOBLDetailCargoLineCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManOBLDetailCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManOBLHeaderCargoLineCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManOBLHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManOBLHeaderCollectionView":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManSlotOrgCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusSeaManTranHeadCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AirOrStandAloneCusUnderbondCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusUnderbondCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusUnderbondUnionCollection":
				case "Enterprise.Customs.AU.Declaration.Business.DepotCusUnderbondCusOutturnHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.DepotCusUnderbondCollection":
				case "Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLinesCollection":
				case "Enterprise.Customs.AU.Declaration.Business.BillCollection":
				case "Enterprise.Customs.AU.Declaration.Business.GroupInvoiceChargeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceApportionedChargeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceChargeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceLineApportionedChargeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceLineChargeCollection":
				case "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.GroupInvoiceAllInvoiceHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.GroupInvoiceDirectInvoiceHeaderCollection":
				case "Enterprise.Customs.AU.Declaration.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.AU.Declaration.Business.JobDeclaration+JobComInvoiceGroupHeaderSingleElementCollection":
				case "Enterprise.Customs.AU.Declaration.Business.JobDeclarationCollection":
				case "Enterprise.Customs.AU.AirCargo.Business.JobRelatedWayBillCollection":
				case "Enterprise.Customs.AU.Declaration.Business.EFTPaymentInformationCollection":
				case "Enterprise.Customs.AU.Declaration.Business.MessageAttacheeSelectionCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CollectionDependentOrgContactCollection":
				case "Enterprise.Customs.AU.Declaration.Business.ClassificationCollection":
				case "Enterprise.Customs.AU.Declaration.Business.ExportClassificationCollection":
				case "Enterprise.Customs.AU.Declaration.Business.ImportClassificationCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CusClassPartPivotCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUCAHECCCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUCChapterAHECCCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUCChapterCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUCClassCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUCChapterAUCClassCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AUCSectionCollection":
				case "Enterprise.Customs.AU.Declaration.Business.Testing.TestAUCSectionCollection":
				case "Enterprise.Customs.AU.Declaration.Business.AURefPacksCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCAContainerCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CMRCusSCAContainerCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCAContainerCollectionView":
				case "Enterprise.Customs.AU.SeaCargo.Business.LegacyCusSCAContainerCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCADepotContainerCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCADepotContainerList":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCADepotHouseCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCADepotHouseList":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCAHouseCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CMRCusSCAHouseCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.LegacyCusSCAHouseCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCAOceanBillCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCAPivotCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CMRCusSCAPivotCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCAPivotCollectionForContainers":
				case "Enterprise.Customs.AU.SeaCargo.Business.CMRCusSCAPivotCollectionForContainers":
				case "Enterprise.Customs.AU.SeaCargo.Business.CusSCAPivotCollectionForHouseBill":
				case "Enterprise.Customs.AU.SeaCargo.Business.CMRCusSCAPivotCollectionForHouseBill":
				case "Enterprise.Customs.AU.SeaCargo.Business.LegacyCusSCAPivotCollectionForContainers":
				case "Enterprise.Customs.AU.SeaCargo.Business.LegacyCusSCAPivotCollectionForHouseBill":
				case "Enterprise.Customs.AU.SeaCargo.Business.SeaCargoDepotContainerCollection":
				case "Enterprise.Customs.AU.SeaCargo.Business.SeaCargoDepotShipmentCollection":
				case "Enterprise.Customs.AU.Declaration.Business.CustomsVoyageDestinationWrapperCollection":
				case "Enterprise.Customs.AU.Business.EXDOC.QuarantineExDocShipsCompartmentCollection":
				case "Enterprise.Customs.HK.Business.TraxonMessageCollection":
				case "Enterprise.Customs.NZ.Business.CodeDataPairCollection":
				case "Enterprise.Customs.NZ.Business.OtherInfoCollection":
				case "Enterprise.Customs.NZ.Business.HeaderOtherInfoCollection":
				case "Enterprise.Customs.NZ.Business.LineOtherInfoCollection":
				case "Enterprise.Customs.NZ.Business.PermitCodeCollection":
				case "Enterprise.Customs.NZ.Business.ProhibitedCodeCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.CusContainerCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.CusEntryHeaderChargeCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.ActiveCusEntryHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.CusEntryHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.CusEntryLineFeeCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.CusEntryLineCollection":
				case "Enterprise.Customs.NZ.Business.MasterFiles.OrgSupplierPartCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.BillCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.InvoiceLineDependentCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.DeclarationLevelPackingGroupCollection":
				case "Enterprise.Customs.NZ.Business.Express.CusHAWBDependentCollection":
				case "Enterprise.Customs.NZ.Business.Express.CusMAWBCollection":
				case "Enterprise.Customs.NZ.Business.Express.NZCMessageCollection":
				case "Enterprise.Customs.NZ.Business.NZTariffBulkChange+TariffBulkChangeOldTariffCollection":
				case "Enterprise.Customs.NZ.Business.NZTariffBulkChange+TariffBulkChangeNewTariffCollection":
				case "Enterprise.Customs.NZ.Business.NZTariffBulkChange+TariffBulkChangerPartCollection":
				case "Enterprise.Customs.NZ.Business.NZTariffBulkChange+TBCClassificationCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.InvoiceChargeCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.InvoiceHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.InvoiceLineViewCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.ParentLineCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.JobDeclarationCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.JobDeclarationCollectionECIWriteOff":
				case "Enterprise.Customs.NZ.Business.Declaration.NZCMessageCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.DeclarationLevelPackageCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.PackageCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.PackingGroupCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.CusEntryHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeaderCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.OutwardReport.CusEntryNumberCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.OutwardReport.EDIMessageCollection":
				case "Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OrderedShipments":
				case "Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.NZDocsMAFCSCommodityCollection":
				case "Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.NZDocsMAFCSContainerCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCClassificationChapterCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCClassificationDutyRateCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NonDependentNZCClassificationSectionCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.FamilyMemberCollectionForBinding":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NonDependentNZCClassificationCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCClassificationCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCConcessionClassificationLinkCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCConcessionDutyRateCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NonDependentNZCConcessionCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCCountryCollection":
				case "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCCustomsExchangeRateCollection":
				case "Enterprise.Customs.NZ.Business.MasterFiles.CusClassificationCollection":
				case "Enterprise.Customs.NZ.Business.MasterFiles.CusClassificationsForPivot":
				case "Enterprise.Customs.NZ.Business.MasterFiles.OrgSupplierPart+NonPersistentFakeyCollection":
				case "Enterprise.Customs.SG.V4.Business.ClassificationCollection":
				case "Enterprise.Customs.SG.V4.Business.CALicenceNumberCollection":
				case "Enterprise.Customs.SG.V4.Business.CASCCode1Collection":
				case "Enterprise.Customs.SG.V4.Business.CASCCode2Collection":
				case "Enterprise.Customs.SG.V4.Business.CASCCode3Collection":
				case "Enterprise.Customs.SG.V4.Business.ProductCodeCollection":
				case "Enterprise.Customs.SG.V4.Business.CusLineTariffDetailCollection":
				case "Enterprise.Customs.SG.V4.Business.ChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeInvoiceHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeGroupInvoiceDirectInvoiceHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeGroupInvoiceAllInvoiceHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.SupportingDocumentCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeCusContainerCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeCusEntryLineFeeCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeAllCusEntryLineCollection":
				case "Enterprise.Customs.SG.V4.Business.AllCusEntryLineCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeCusEntryLineCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeHouseBillCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeGroupInvoiceChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.GroupInvoiceChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeInvoiceApportionChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.InvoiceApportionChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeInvoiceChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.InvoiceChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeInvoiceLineApportionChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.InvoiceLineApportionChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeInvoiceLineChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.InvoiceLineChargeCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.GroupInvoiceAllInvoiceHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeInvoiceLineCompleteCollection":
				case "Enterprise.Customs.SG.V4.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeInvoiceLineViewCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeJobComInvoiceLineViewCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeJobDeclarationCollection":
				case "Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDEDIMessageCollection":
				case "Enterprise.Customs.SG.V4.Business.CMDMessaging.CusEntryNumberWrapperCollection":
				case "Enterprise.Customs.SG.V4.Business.CusContainerCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeCusEntryHeaderChargesCollection":
				case "Enterprise.Customs.SG.V4.Business.CusEntryHeaderChargesCollection":
				case "Enterprise.Customs.SG.V4.Business.TypeSafeCusEntryHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.SG.V4.Business.CusEntryLineCollection":
				case "Enterprise.Customs.SG.V4.Business.BillCollection":
				case "Enterprise.Customs.SG.V4.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.GroupInvoiceDirectInvoiceHeaderCollection":
				case "Enterprise.Customs.SG.V4.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.SG.V4.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.SG.V4.Business.JobDeclarationCollection":
				case "Enterprise.Customs.SG.V4.Business.SGCDutyCollection":
				case "Enterprise.Customs.SG.V4.Business.SGCLocoCollection":
				case "Enterprise.Customs.SG.V4.Business.SGCPlacesCollection":
				case "Enterprise.Customs.SG.V4.Business.SGCPlacesEditableCollection":
				case "Enterprise.Customs.SG.V4.Business.SGCTariffCollection":
				case "Enterprise.Customs.SG.V4.Business.SGCTariffCommodityCollection":
				case "Enterprise.Customs.SG.V4.Business.SGCTariffCommodityDependantCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeClassificationCollection":
				case "Enterprise.Customs.MY.Business.ClassificationCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeCusClassificationCollection":
				case "Enterprise.Customs.MY.Business.CusClassificationCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeCusContainerCollection":
				case "Enterprise.Customs.MY.Business.CusContainerCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeCusEntryHeaderChargesCollection":
				case "Enterprise.Customs.MY.Business.CusEntryHeaderChargesCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeCusEntryHeaderCollection":
				case "Enterprise.Customs.MY.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeCusEntryLineFeeCollection":
				case "Enterprise.Customs.MY.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeAllCusEntryLineCollection":
				case "Enterprise.Customs.MY.Business.AllCusEntryLineCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeCusEntryLineCollection":
				case "Enterprise.Customs.MY.Business.CusEntryLineCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeBillCollection":
				case "Enterprise.Customs.MY.Business.BillCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeGroupInvoiceChargeCollection":
				case "Enterprise.Customs.MY.Business.GroupInvoiceChargeCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeInvoiceApportionChargeCollection":
				case "Enterprise.Customs.MY.Business.InvoiceApportionChargeCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeInvoiceChargeCollection":
				case "Enterprise.Customs.MY.Business.InvoiceChargeCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeInvoiceLineApportionChargeCollection":
				case "Enterprise.Customs.MY.Business.InvoiceLineApportionChargeCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeInvoiceLineChargeCollection":
				case "Enterprise.Customs.MY.Business.InvoiceLineChargeCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.MY.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeInvoiceHeaderCollection":
				case "Enterprise.Customs.MY.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeInvoiceLineCompleteCollection":
				case "Enterprise.Customs.MY.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeInvoiceLineViewCollection":
				case "Enterprise.Customs.MY.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeJobComInvoiceLineViewCollection":
				case "Enterprise.Customs.MY.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.MY.Business.TypeSafeJobDeclarationCollection":
				case "Enterprise.Customs.MY.Business.JobDeclarationCollection":
				case "Enterprise.Customs.MY.Business.EDIMessageCollectionForAllShipments":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeClassificationCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeCusClassificationCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeCusContainerCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeCusEntryLineFeeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeAllCusEntryLineCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.AllCusEntryLineCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeCusEntryLineCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.BillContainerCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.ChildBillCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.BillCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeGroupInvoiceChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.GroupInvoiceChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeInvoiceApportionChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.InvoiceApportionChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeInvoiceChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.InvoiceChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeInvoiceLineApportionChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.InvoiceLineApportionChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeInvoiceLineChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.InvoiceLineChargeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.BillLevelInvoiceCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeInvoiceHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeJobComInvoiceHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeInvoiceLineCompleteCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeInvoiceLineViewCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeJobComInvoiceLineViewCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeJobDeclarationCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.ClassificationCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.CusClassificationCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.CusContainerCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeCusEntryHeaderChargesCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.CusEntryHeaderChargesCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.TypeSafeCusEntryHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.CusEntryLineCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.JobComInvoiceHeaderCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs._CustomsTemplate_.Business.JobDeclarationCollection":
				case "Enterprise.Customs.US.Business.AllPackagesParentBillCollection":
				case "Enterprise.Customs.Universal.RefCusTariffCollection":
				case "Enterprise.Customs.US.Business.USCTeamSpecialistCollection":
				case "Enterprise.Customs.US.Business.USCAntiDumpingRateCollection":
				case "Enterprise.Customs.US.Business.TypeSafeJobDeclarationCollection":
				case "Enterprise.Customs.US.Business.JobDeclarationCollection":
				case "Enterprise.Customs.US.Business.TypeSafeInvoiceHeaderCollection":
				case "Enterprise.Customs.US.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs.US.Business.TypeSafeCusContainerCollection":
				case "Enterprise.Customs.US.Business.CusContainerCollection":
				case "Enterprise.Customs.US.Business.TypeSafeAllCusEntryLineCollection":
				case "Enterprise.Customs.US.Business.AllCusEntryLineCollection":
				case "Enterprise.Customs.US.Business.USCTariffDutyRateCollection":
				case "Enterprise.Customs.US.Business.GroupInvoiceChargeCollection":
				case "Enterprise.Customs.US.Business.ChildBillCollection":
				case "Enterprise.Customs.US.Business.InvoiceCusInvPackCollection":
				case "Enterprise.Customs.US.Business.MessageActionRelatedRecordWrapperCollection":
				case "Enterprise.Customs.US.Business.BillIssuerOrganisationFindBoxCollection":
				case "Enterprise.Customs.US.Business.OrgSupplierPartCollection":
				case "Enterprise.Customs.US.Business.CusStatementLineChargeCollection":
				case "Enterprise.Customs.US.Business.USCVisaCollection":
				case "Enterprise.Customs.US.Business.InvoiceLineCusInvPackDetailCollection":
				case "Enterprise.Customs.US.Business.CusClassPartPivotCollection":
				case "Enterprise.Customs.US.Business.TypeSafeClassificationCollection":
				case "Enterprise.Customs.US.Business.ClassificationCollection":
				case "Enterprise.Customs.US.Business.InvoiceChargeCollection":
				case "Enterprise.Customs.US.Business.TypeSafeJobComInvoiceLineViewCollection":
				case "Enterprise.Customs.US.Business.TypeSafeSubInvoiceHeaderCollection":
				case "Enterprise.Customs.US.Business.CusEntryLineCollection":
				case "Enterprise.Customs.US.Business.FeeCusCodeDataCollection":
				case "Enterprise.Customs.US.Business.TypeSafeCusClassificationCollection":
				case "Enterprise.Customs.US.Business.CusClassificationCollection":
				case "Enterprise.Customs.US.Business.ImportClassificationCollection":
				case "Enterprise.Customs.US.Business.ExportClassificationCollection":
				case "Enterprise.Customs.US.Business.TypeSafeCusEntryHeaderChargesCollection":
				case "Enterprise.Customs.US.Business.CusEntryHeaderChargesCollection":
				case "Enterprise.Customs.US.Business.Testing.USOrganisationTest+DummyWithUSOrganisationCollection":
				case "Enterprise.Customs.US.Business.PackingGroupCollection":
				case "Enterprise.Customs.US.Business.InvoiceCusInvPackDetailCollection":
				case "Enterprise.Customs.US.Business.BillCollection":
				case "Enterprise.Customs.US.Business.BillContainerCollection":
				case "Enterprise.Customs.US.Business.USCRegionDistrictPortCollection":
				case "Enterprise.Customs.US.Business.USCVisaTariffNonDependentCollection":
				case "Enterprise.Customs.US.Business.InvoiceLineCusInvPackCollection":
				case "Enterprise.Customs.US.Business.PackingGroupContainerCollection":
				case "Enterprise.Customs.US.Business.TypeSafeInvoiceLineCompleteCollection":
				case "Enterprise.Customs.US.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.US.Business.AffirmationCodeCollection":
				case "Enterprise.Customs.US.Business.MonthlyDeletedStatementLinesCollection":
				case "Enterprise.Customs.US.Business.ProductClassificationOverrideWrapperCollection":
				case "Enterprise.Customs.US.Business.BillCollectionForEntry":
				case "Enterprise.Customs.US.Business.RelatedDocumentCollection":
				case "Enterprise.Customs.US.Business.InvoiceLineDependentCollection":
				case "Enterprise.Customs.US.Business.NonPersistentCusContainerCollection":
				case "Enterprise.Customs.US.Business.MiscCusCodeDataCollection":
				case "Enterprise.Customs.US.Business.USCFDAProductNumberCollection":
				case "Enterprise.Customs.US.Business.USCAntiDumpingCaseCollection":
				case "Enterprise.Customs.US.Business.TypeSafeJobComInvoiceHeaderCollection":
				case "Enterprise.Customs.US.Business.JobComInvoiceHeaderCollection":
				case "Enterprise.Customs.US.Business.TypeSafeJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.US.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.US.Business.TypeSafeInvoiceLineViewCollection":
				case "Enterprise.Customs.US.Business.DeclarationLevelPackageCollection":
				case "Enterprise.Customs.US.Business.InvoiceApportionChargeCollection":
				case "Enterprise.Customs.US.Business.BillLevelInvoiceCollection":
				case "Enterprise.Customs.US.Business.InvoiceLinesForEntryLineCollection":
				case "Enterprise.Customs.US.Business.CusStatementLineStatusCollection":
				case "Enterprise.Customs.US.Business.PackageCollection":
				case "Enterprise.Customs.US.Business.TypeSafeCusLineTariffDetailCollection":
				case "Enterprise.Customs.US.Business.TypeSafeCusEntryHeaderCollection":
				case "Enterprise.Customs.US.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.US.Business.USCTariffCollection":
				case "Enterprise.Customs.US.Business.InvoiceLineApportionChargeCollection":
				case "Enterprise.Customs.US.Business.USCAffirmationOfComplianceCollection":
				case "Enterprise.Customs.US.Business.BillTypeViewCollection":
				case "Enterprise.Customs.US.Business.USCAntiDumpingBondCashIndicatorCollection":
				case "Enterprise.Customs.US.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.US.Business.DeclarationLevelPackingGroupCollection":
				case "Enterprise.Customs.US.Business.MessageActionRelatedRecordWrapperViewCollection":
				case "Enterprise.Customs.US.Business.ImportMessageSendingActionCollection":
				case "Enterprise.Customs.US.Business.VisaQuotaQuerySendingActionCollection":
				case "Enterprise.Customs.US.Business.AffirmationCodeViewCollection":
				case "Enterprise.Customs.US.Business.USCAntiDumpingTariffCollection":
				case "Enterprise.Customs.US.Business.FlattenEntryLineAndBillForImmediateDeliveryCollection":
				case "Enterprise.Customs.US.Business.TypeSafeCusEntryLineFeeCollection":
				case "Enterprise.Customs.US.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.US.Business.InvoiceLineChargeCollection":
				case "Enterprise.Customs.US.Business.USCForeignPortCollection":
				case "Enterprise.Customs.US.Business.USCCountryCollection":
				case "Enterprise.Customs.US.Business.CusStatementLineCollection":
				case "Enterprise.Customs.US.Business.CusBondDetailCollection":
				case "Enterprise.Customs.US.Business.CusStatementHeaderCollection":
				case "Enterprise.Customs.US.Business.USCVisaTariffCollection":
				case "Enterprise.Customs.US.Business.USCQuotaCollection":
				case "Enterprise.Customs.US.Business.EDIMessageCollection":
				case "Enterprise.Customs.US.Business.HouseBillRefNoCollection":
				case "Enterprise.Customs.US.Business.ActiveCusEntryHeaderCollection":
				case "Enterprise.Customs.US.Business.StatementDeleteAndSendingActionCollection":
				case "Enterprise.Customs.US.Business.CusSupImpClassOverrideCollection":
				case "Enterprise.Customs.US.Business.DispositionCodeAndDateCollection":
				case "Enterprise.Customs.US.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.US.Business.SubInvoiceHeaderCollection":
				case "Enterprise.Customs.US.Business.FDAQtyUQCusCodeDataCollection":
				case "Enterprise.Customs.US.Business.CusLineTariffDetailCollection":
				case "Enterprise.Customs.US.Module.StatementCollection":
				case "Enterprise.Customs.US.Module.USCAffirmationOfComplianceCollection":
				case "Enterprise.Customs.US.Module.QueryTransmitMessageCollection":
				case "Enterprise.Customs.US.Module.AMSBrokerDownloadMQEDIMessageCollection":
				case "Enterprise.Customs.US.Module.USCFIRMSCollection":
				case "Enterprise.Customs.US.Business.ReconInvoiceLineCollection":
				case "Enterprise.Customs.ZA.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.ZA.Business.AdditionalInformationCollection":
				case "Enterprise.Customs.ZA.Business.DutyCollection":
				case "Enterprise.Customs.ZA.Business.CusClassificationCollection":
				case "Enterprise.Customs.ZA.Business.ClassificationsForPivot":
				case "Enterprise.Customs.ZA.Business.CusContainerCollection":
				case "Enterprise.Customs.ZA.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.ZA.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.ZA.Business.CusEntryLineCollection":
				case "Enterprise.Customs.ZA.Business.BillCollection":
				case "Enterprise.Customs.ZA.Business.ApportionedChargeCollection":
				case "Enterprise.Customs.ZA.Business.InvoiceChargeCollection":
				case "Enterprise.Customs.ZA.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.ZA.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs.ZA.Business.JobComInvoiceHeaderCollection":
				case "Enterprise.Customs.ZA.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.ZA.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.ZA.Business.JobDeclarationLookups+ZAImporterCollection":
				case "Enterprise.Customs.ZA.Business.JobDeclarationCollection":
				case "Enterprise.Customs.AE.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.AE.Business.CusClassificationCollection":
				case "Enterprise.Customs.AE.Business.CusContainerCollection":
				case "Enterprise.Customs.AE.Business.TypeSafeCusEntryHeaderCollection":
				case "Enterprise.Customs.AE.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.AE.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.AE.Business.CusEntryLineCollection":
				case "Enterprise.Customs.AE.Business.BillCollection":
				case "Enterprise.Customs.AE.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.AE.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs.AE.Business.JobComInvoiceHeaderCollection":
				case "Enterprise.Customs.AE.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.AE.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.AE.Business.JobDeclarationCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeClassificationCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeCusClassificationCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeCusContainerCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeCusEntryLineFeeCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeAllCusEntryLineCollection":
				case "Enterprise.Customs.GB.Business.AllCusEntryLineCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeCusEntryLineCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeBillCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeGroupInvoiceChargeCollection":
				case "Enterprise.Customs.GB.Business.GroupInvoiceChargeCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeInvoiceApportionChargeCollection":
				case "Enterprise.Customs.GB.Business.InvoiceApportionChargeCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeInvoiceChargeCollection":
				case "Enterprise.Customs.GB.Business.InvoiceChargeCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeInvoiceLineApportionChargeCollection":
				case "Enterprise.Customs.GB.Business.InvoiceLineApportionChargeCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeInvoiceLineChargeCollection":
				case "Enterprise.Customs.GB.Business.InvoiceLineChargeCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeInvoiceHeaderCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeJobComInvoiceHeaderCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeInvoiceLineCompleteCollection":
				case "Enterprise.Customs.GB.Business.InvoiceLineCompleteCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeInvoiceLineViewCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeJobComInvoiceLineViewCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeJobDeclarationCollection":
				case "Enterprise.Customs.GB.Business.ClassificationCollection":
				case "Enterprise.Customs.GB.Business.CusClassificationCollection":
				case "Enterprise.Customs.GB.Business.CusContainerCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeCusEntryHeaderChargesCollection":
				case "Enterprise.Customs.GB.Business.CusEntryHeaderChargesCollection":
				case "Enterprise.Customs.GB.Business.TypeSafeCusEntryHeaderCollection":
				case "Enterprise.Customs.GB.Business.CusEntryHeaderCollection":
				case "Enterprise.Customs.GB.Business.CusEntryLineFeeCollection":
				case "Enterprise.Customs.GB.Business.CusEntryLineCollection":
				case "Enterprise.Customs.GB.Business.BillCollection":
				case "Enterprise.Customs.GB.Business.JobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.GB.Business.InvoiceHeaderCollection":
				case "Enterprise.Customs.GB.Business.JobComInvoiceHeaderCollection":
				case "Enterprise.Customs.GB.Business.InvoiceLineViewCollection":
				case "Enterprise.Customs.GB.Business.JobComInvoiceLineViewCollection":
				case "Enterprise.Customs.GB.Business.JobDeclarationCollection":
				case "Enterprise.Accounting.Business.JobInvoicing.JobInvoicePrintingFilter+JobsOnConsolCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.DependentTransactionLineCollection":
				case "Enterprise.Accounting.Business.CashBook.DirectPayment.DirectPaymentLineCollection":
				case "Enterprise.Accounting.Business.JobInvoicing.IndependentChargeCollectionForBinding":
				case "Enterprise.Accounting.Business.JobInvoicing.ChargeCollection":
				case "Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalLineCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.TransactionHeaderCollection":
				case "Enterprise.Accounting.Business.CashBook.DepositBatch.DepositBatchCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBaseCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceLineCollection":
				case "Enterprise.Accounting.Business.ARAP.ReceiptPayment.APPaymentBatchPosterCollection":
				case "Enterprise.Accounting.Business.Base.AccStatement.StatementCollection":
				case "Enterprise.Accounting.Business.ARAP.ReceiptPayment.ReceiptPaymentBaseCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCollection":
				case "Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseCollection":
				case "Enterprise.Accounting.Business.CashBook.MergedTransactionCollection":
				case "Enterprise.Accounting.Business.Base.Matching.AccTransactionMatchLinkCollection":
				case "Enterprise.Accounting.Business.Base.Matching.OrgLedgerFilterCollection":
				case "Enterprise.Accounting.Business.ARAP.ReceiptPayment.ARReceiptCollection":
				case "Enterprise.Accounting.Business.GeneralLedger.GLBudget.GLBudgetCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.ARTransactionHeaderCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollection":
				case "Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentApprovalCollection":
				case "Enterprise.Accounting.Business.Base.Matching.CurrencySummaryRowCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCostCollectionForImporting":
				case "Enterprise.Accounting.Business.WIPAccrual.WIPAccrualCollection":
				case "Enterprise.Accounting.Business.JobInvoicing.JobManagementCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.CashbookTransactionCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchLineCollection":
				case "Enterprise.Accounting.Business.JobInvoicing.JCJournalLinesCollection":
				case "Enterprise.Accounting.Business.JobInvoicing.JobCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeaderCollection":
				case "Enterprise.Accounting.Business.ARAP.HotCheque.AccHotChequeCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceUpdateConsolCostsCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCostCollection":
				case "Enterprise.Accounting.Business.GeneralLedger.GLBudget.GLBudgetLineDependentCollection":
				case "Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentApprovalItemCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.IMatchingCollection":
				case "Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceCollection":
				case "Enterprise.Accounting.Business.PeriodCollection":
				case "Enterprise.Accounting.Business.ConsolCosting.ApportionmentSplitChargeCollection":
				case "Enterprise.Accounting.Business.GenericJob.GenericJobCollection":
				case "Enterprise.Accounting.Business.CashBook.DirectReceipt.DirectReceiptLineCollection":
				case "Enterprise.Accounting.Business.CashBook.DirectDebitBatch.DirectDebitBatchLineCollection":
				case "Enterprise.Accounting.Business.CashBook.DepositBatch.DepositBatchTransactionLineCollection":
				case "Enterprise.Accounting.Business.CashBook.BankReconTransCollection":
				case "Enterprise.Accounting.Business.Base.Matching.ViewMatchGroupCollection":
				case "Enterprise.Accounting.Business.ARAP.ReceiptPayment.ARReceiptBatchPosterCollection":
				case "Enterprise.Accounting.Business.CashBook.BatchTransactionCollection":
				case "Enterprise.Accounting.Business.Base.Unmatching.UnmatchingRowCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.ITransactionCollection":
				case "Enterprise.Accounting.Business.Base.Matching.TransactionMatchLinkCollectionForUnmatching":
				case "Enterprise.Accounting.Business.JobInvoicing.ProfitShare.ProfitShareShipmentDetailCollection":
				case "Enterprise.Accounting.Business.JobInvoicing.ExchangeRatesCollection":
				case "Enterprise.Accounting.Business.ConsolCosting.JobConsolCostCollection":
				case "Enterprise.Accounting.Business.GenericTransaction.GenericTransactionCollection":
				case "Enterprise.Accounting.Business.CashBook.DirectDebitBatch.DirectDebitBatchHeaderCollection":
				case "Enterprise.Accounting.Business.CashBook.DirectTransactionHeaderBaseCollection":
				case "Enterprise.Accounting.Business.Base.Transaction.TransactionLinesCollection":
				case "Enterprise.Accounting.Business.WIPAccrual.AccrualCollection":
				case "Enterprise.Accounting.Business.JobInvoicing.ProfitShare.ProfitShareDetailCollection":
				case "Enterprise.Accounting.Business.GenericCharge.GenericChargeCollection":
				case "Enterprise.Accounting.Business.CashBook.DepositBatch.DepositBatchModuleCollection":
				case "Enterprise.Accounting.Business.Base.Matching.OrganizationSubBalanceCollection":
				case "Enterprise.Accounting.Business.ARAP.PaymentApproval.APPaymentApprovalWithoutAuthorisationCollection":
				case "Enterprise.LandedCosting.Business.LandCostInputCollection":
				case "Enterprise.LandedCosting.Business.LandedCostHistoryCollection":
				case "Enterprise.LandedCosting.Business.GenericLandedCostHistoryCollection":
				case "Enterprise.LandedCosting.Business.LandedCostingExRateCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsInventoryViewCollection":
				case "Enterprise.Warehouse.Transactions.Business.BondedEntryKeyLookupCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsBondedWarehouseAttributeCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsDocketLineCollectionND":
				case "Enterprise.Warehouse.Transactions.Business.WhsOrderLineWorkCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsStocktakeCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsDocketReferenceCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsOrderCollectionForAttaching":
				case "Enterprise.Warehouse.Transactions.Business.WhsOrderCollectionForPicking":
				case "Enterprise.Warehouse.Transactions.Business.WhsLegacyPickableDocketCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsPickLineCollectionND":
				case "Enterprise.Warehouse.Transactions.Business.WhsPickCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsStocktakeLineCollectionND":
				case "Enterprise.Warehouse.Transactions.Invoicing.WhsInvoiceCollection":
				case "Enterprise.Warehouse.DataTransfer.WDFEDIInterchangeWDFEDIMessageCollection":
				case "Enterprise.Warehouse.Transactions.Business.WhsDocketGenericCollection":
				case "Enterprise.Recruiter.Business.HRRecruitmentJobCampaignCollection":
				case "Enterprise.Recruiter.Business.HRJobRoleCollection":
				case "Enterprise.Recruiter.Business.HRJobAdPlacementDependentCollection":
				case "Enterprise.Recruiter.Business.RatingToTestRatingDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobRoleSkillPivotDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobApplicantSkillRatingApplicantDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobSkillTestDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobApplicationDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobSkillCollection":
				case "Enterprise.Recruiter.Business.SkillTestToTestRatingDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobApplicationInterviewDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobApplicantSkillRatingSkillDependentCollection":
				case "Enterprise.Recruiter.Business.HRJobApplicantCollection":
				case "Enterprise.CustomerService.Business.IncidentApprovalCollection":
				case "Enterprise.DocumentScanning.Business.StorageDocsCollectionViewBase":
				case "Enterprise.DocumentScanning.Business.StorageDocsUnallocatedCollectionView":
				case "Enterprise.DocumentScanning.Business.StorageDocsCollectionBase`1":
				case "Enterprise.DocumentScanning.Business.StorageDocsUnallocatedCollection":
				case "Enterprise.DocumentScanning.Business.StorageDocsCollection":
				case "Enterprise.DocumentScanning.Business.NonPersistentUnallocatedObjectCollection":
				case "Enterprise.DocumentScanning.Business.StorageDocsCollectionBase":
				case "Enterprise.DocumentScanning.Business.StorageDocsDependentCollectionBase":
				case "Enterprise.DocumentScanning.Business.StorageDocsCollectionView":
				case "Enterprise.DocumentScanning.Business.StorageFileCollection":
				case "Enterprise.DocumentScanning.Business.StorageFileCollectionView":
				case "Enterprise.DocumentScanning.Business.StorageMainCollection":
				case "Enterprise.DocumentScanning.Business.Testing.StorageMainCollectionTestClass":
				case "Enterprise.DocumentScanning.GUI.DocumentsZGrid+DocumentsZGridTest+CollectionForTesting":
				case "Enterprise.DocumentWrappers.DocWhsDocketLineCollection":
				case "Enterprise.DocumentWrappers.DocWhsAdjustmentLineCollection":
				case "Enterprise.DocumentWrappers.DocQuotationTrailingPageCollection":
				case "Enterprise.DocumentWrappers.DocOrderLineDeliverContainerCollection":
				case "Enterprise.DocumentWrappers.DocCommodityCollection":
				case "Enterprise.DocumentWrappers.DocBaseWrapperCollection":
				case "Enterprise.DocumentWrappers.DocBaseWrapperCollection`1":
				case "Enterprise.DocumentWrappers.GenericWrappers.Base.GenericWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Base.GenericWrapperCollection`1":
				case "Enterprise.DocumentWrappers.GenericWrappers.Base.Testing.GenericWrapperCollectionNonInheritedTest+MyWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.AddressWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.FreightWrapperCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseCusContainerCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.Testing.DocBaseCusContainerCollectionTestClass":
				case "Enterprise.DocumentWrappers.Customs.AU.DocAuthorityToDealLineConditionDetailsCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.DocPrintPermitContainersCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.AE.Business.DocJobComInvoiceGroupHeaderCollection":
				case "Enterprise.DocumentWrappers.DocPackUnpackContainerRegoCollection":
				case "Enterprise.DocumentWrappers.DocWhsPickLineCollection":
				case "Enterprise.DocumentWrappers.DocWhsOrderCollection":
				case "Enterprise.DocumentWrappers.DocWhsTransferLineCollection":
				case "Enterprise.DocumentWrappers.DocBillofLadingContainerCollection":
				case "Enterprise.DocumentWrappers.DocJobChargeCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseJobComInvoiceHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocJobComInvoiceHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs._CustomsTemplate_.DocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.V4.DocCusEntryLineCollection":
				case "Enterprise.DocumentWrappers.DocWhsInwardsLineCollection":
				case "Enterprise.DocumentWrappers.DocQuotation+DocCFXCollection":
				case "Enterprise.DocumentWrappers.LandedCostDutyRateSummaryCollection":
				case "Enterprise.DocumentWrappers.DocLandedCostInputCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Map.Testing.GenericWrapperMapperTest+ChildWrapperCollection":
				case "Enterprise.DocumentWrappers.Freight.NZ.DocShipmentCollection":
				case "Enterprise.DocumentWrappers.DocPackLinesCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.DocMAFCoverSheetPageCollection":
				case "Enterprise.Customs.AE.Business.DocCusEntryLineCollection":
				case "Enterprise.DocumentWrappers.DocOrganisationCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Testing.VolumeWrapperTest+TestWrapperClassCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseJobComInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocJobComInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseCusEntryHeaderChargesCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocCusEntryHeaderChargesCollection":
				case "Enterprise.DocumentWrappers.DocPaymentItemCollection":
				case "Enterprise.DocumentWrappers.DocOrderLineCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.NoteWrapperCollection":
				case "Enterprise.DocumentWrappers.Testing.DocBaseWrapperCollectionConcreteTest+TestWrapperCollection":
				case "Enterprise.Customs.ZA.Business.DocumentWrappers.DocJobComInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.US.DocJobComInvoiceHeaderCollection":
				case "Enterprise.DocumentWrappers.DocJobExchangeRateCollection":
				case "Enterprise.DocumentWrappers.DocBankAccountCollection":
				case "Enterprise.DocumentWrappers.DocWhsLabelCollection":
				case "Enterprise.DocumentWrappers.DocWhsPalletLabelCollection":
				case "Enterprise.DocumentWrappers.DocWhsPackageLabelCollection":
				case "Enterprise.DocumentWrappers.DocVesselCollection":
				case "Enterprise.DocumentWrappers.DocAppointedAgentPortsCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.ContainerWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Testing.PackQTYWrapperTest+TestWrapperClassCollection":
				case "Enterprise.DocumentWrappers.DocShipmentConsolCollection":
				case "Enterprise.DocumentWrappers.DocOuterPackCollection":
				case "Enterprise.Customs.ZA.Business.DocumentWrappers.DocJobComInvoiceHeaderCollection":
				case "Enterprise.Customs.ZA.Business.DocumentWrappers.DocJobComInvoiceGroupHeaderCollection":
				case "Enterprise.Customs.ZA.Business.DocumentWrappers.DocDA74ContainerCollection":
				case "Enterprise.DocumentWrappers.Customs.General.DocJobComInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.Customs.General.DocCusContainerCollection":
				case "Enterprise.DocumentWrappers.DocFCLContainerCollection":
				case "Enterprise.DocumentWrappers.DocContainerCollection":
				case "Enterprise.DocumentWrappers.DocMatchLinkCollection":
				case "Enterprise.DocumentWrappers.Mapping.Testing.DocumentWrapperMapperTest+ChildWrapperCollection":
				case "Enterprise.DocumentWrappers.Mapping.Testing.BusinessObjectMapperTest+ChildCollection":
				case "Enterprise.DocumentWrappers.DocTranshipmentCollection":
				case "Enterprise.DocumentWrappers.DocJobRequiredDocumentCollection":
				case "Enterprise.DocumentWrappers.DocForwardingPackLinesCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.MiscellaneousDescriptionsCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseCusEntryLineCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocCusEntryLineCollection":
				case "Enterprise.DocumentWrappers.Customs.US.DocJobComInvoiceGroupHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.DocMAFCoverSheetContainerCollection":
				case "Enterprise.DocumentWrappers.DocJobInvoicingJobCollection":
				case "Enterprise.DocumentWrappers.DocWhsDocketContainerCollection":
				case "Enterprise.DocumentWrappers.DocOrgAddressCapapabilityCollection":
				case "Enterprise.DocumentWrappers.DocContactsCollection":
				case "Enterprise.DocumentWrappers.DocLandedCostHistoryCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.ContactWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.RegistrationNumberCodeWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.OrderLineWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Testing.WeightWrapperTest+TestWrapperClassCollection":
				case "Enterprise.DocumentWrappers.Customs.General.DocJobComInvoiceGroupHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.US.DocCusEntryLineCollection":
				case "Enterprise.DocumentWrappers.DocOrderCollection":
				case "Enterprise.DocumentWrappers.DocGroupCollection":
				case "Enterprise.DocumentWrappers.DocCusCodeCollection":
				case "Enterprise.DocumentWrappers.IDocSimpleContainerCollection":
				case "Enterprise.DocumentWrappers.DocTransportCollection":
				case "Enterprise.DocumentWrappers.DocShipmentCollection":
				case "Enterprise.DocumentWrappers.DocForwardingShipmentCollection":
				case "Enterprise.DocumentWrappers.DocIMOBodyCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.DocJobComInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.DocCusContainerCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.Testing.DocBaseJobComInvoiceLineCollectionTestClass":
				case "Enterprise.DocumentWrappers.Customs.Base.Testing.DocBaseJobComInvoiceHeaderCollectionTestClass":
				case "Enterprise.DocumentWrappers.DocBillOfLadingBodySectionCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.CommercialInvoiceWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Map.MacroWrapperCollection":
				case "Enterprise.DocumentWrappers.DocPackLocationForDocumentCollection":
				case "Enterprise.Customs.ZA.Business.DocumentWrappers.DocCusEntryLineCollection":
				case "Enterprise.Customs.ZA.Business.DocumentWrappers.DocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocCMRMessageChargeItemCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.V4.DocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.V4.DocCusContainerCollection":
				case "Enterprise.Customs.AE.Business.DocJobComInvoiceLineCollection":
				case "Enterprise.Customs.AE.Business.DocJobComInvoiceHeaderCollection":
				case "Enterprise.Customs.AE.Business.DocCusContainerCollection":
				case "Enterprise.DocumentWrappers.DocGLAccountCollection":
				case "Enterprise.DocumentWrappers.DocWhsPickLineRequiredCollection":
				case "Enterprise.DocumentWrappers.DocWhsStocktakeLineCollection":
				case "Enterprise.DocumentWrappers.DocOneOffContainerCollection":
				case "Enterprise.DocumentWrappers.DocSalesTradeLanesCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.DocJobComInvoiceHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocBaseCusEntryLineFeeCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocCusEntryLineFeeCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.DocPrintPermitConsigmentDetailsCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.DocPrintPermitConditionsCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.V4.DocJobComInvoiceGroupHeaderCollection":
				case "Enterprise.DocumentWrappers.DocWithholdingTaxRateCollection":
				case "Enterprise.DocumentWrappers.Quotation.DocQuotationLineCollection":
				case "Enterprise.DocumentWrappers.Quotation.DocFreightQuotationLineCollection":
				case "Enterprise.DocumentWrappers.Quotation.DocOriginQuotationLineCollection":
				case "Enterprise.DocumentWrappers.Quotation.DocDestinationQuotationLineCollection":
				case "Enterprise.DocumentWrappers.DocIndexEntryCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.DocCusEntryLineCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.DocCusEntryLineCollectionPaddedForEntryPrint":
				case "Enterprise.DocumentWrappers.Customs.NZ.DocContainerAndPackageInfoCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.DocLandedCostingExchangeRateCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.DocMAFCoverSheetCommodityCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.V4.DocJobComInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.LoadListPackLineCollection":
				case "Enterprise.DocumentWrappers.DocARBatchInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.DocSalesCallsCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.CommercialInvoiceLineWrapperCollection":
				case "Enterprise.DocumentWrappers.Freight.DocBaseConsolCollection":
				case "Enterprise.DocumentWrappers.Freight.Forwarding.DocForwardingConsolCollection":
				case "Enterprise.DocumentWrappers.Customs.US.DocJobComInvoiceLineCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.V4.DocRefundInfoConsigmentDetailsCollection":
				case "Enterprise.DocumentWrappers.Customs.SG.V4.DocJobComInvoiceHeaderCollection":
				case "Enterprise.DocumentWrappers.DocWhsJobChargeCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Map.SyntaxAndFormattingWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.UNDGSubstanceWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.ContainerServiceWrapperCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.DocJobComInvoiceGroupHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocPackingGroupCollection":
				case "Enterprise.DocumentWrappers.Customs.GB.DocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocExportCustomsManifestLineCollection":
				case "Enterprise.DocumentWrappers.DocTransactionLineCollection":
				case "Enterprise.DocumentWrappers.DocWhsDocketLineWithChargesCollection":
				case "Enterprise.DocumentWrappers.DocDocAddressCollection":
				case "Enterprise.DocumentWrappers.IDocContainerCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.OrderWrapperCollection":
				case "Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.DocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.General.DocJobComInvoiceHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.Base.Testing.DocBaseJobComInvoiceGroupHeaderCollectionTestClass":
				case "Enterprise.DocumentWrappers.Customs.AU.DocCusContainerCollection":
				case "Enterprise.DocumentWrappers.Customs.US.DocCusContainerCollection":
				case "Enterprise.DocumentWrappers.DocTransactionHeaderCollection":
				case "Enterprise.DocumentWrappers.DocChargeCodeCollection":
				case "Enterprise.DocumentWrappers.DocAccQueryClaimCollection":
				case "Enterprise.DocumentWrappers.DocWhsOrderLineCollection":
				case "Enterprise.DocumentWrappers.DocStaffCollection":
				case "Enterprise.DocumentWrappers.DocRecommendedAgentsCollection":
				case "Enterprise.DocumentWrappers.DocAddressCollection":
				case "Enterprise.DocumentWrappers.DocLandedCostHeader+EmptyDocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.CustomsEntryWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Map.TableWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.Map.AreaUseageWrapperCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocJobComInvoiceGroupHeaderCollection":
				case "Enterprise.DocumentWrappers.Customs.AU.DocCTOCusHAWBCollection":
				case "Enterprise.DocumentWrappers.DocLoadListPackLineCollection":
				case "Enterprise.DocumentWrappers.DocProfitShareShipmentDetailCollection":
				case "Enterprise.DocumentWrappers.DocTaxRateCollection":
				case "Enterprise.DocumentWrappers.DocJobLineDetailCollection":
				case "Enterprise.DocumentWrappers.DocJobInvoicingJobChargeCollection":
				case "Enterprise.DocumentWrappers.DocAccountingVoucherLineCollection":
				case "Enterprise.DocumentWrappers.DocPaymentApprovalItemCollection":
				case "Enterprise.DocumentWrappers.DocWhsInventoryCollection":
				case "Enterprise.DocumentWrappers.DocWhsPackingSlipLineCollection":
				case "Enterprise.DocumentWrappers.DocRateEntryCollection":
				case "Enterprise.DocumentWrappers.DocOrderLineDeliveryCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.RouteWrapperCollection":
				case "Enterprise.DocumentWrappers.GenericWrappers.PackageWrapperCollection":
				case "Enterprise.Customs.ZA.Business.DocumentWrappers.DocCusContainerCollection":
				case "Enterprise.DocumentWrappers.Customs.US.DocCusEntryHeaderCollection":
				case "Enterprise.Customs.AE.Business.DocCusEntryHeaderCollection":
				case "Enterprise.DocumentWrappers.DocCFSShipmentCollection":
				case "Enterprise.DocumentWrappers.DocARInvoiceLineCollection":
				case "Enterprise.Client.SWL.Shipnet.Business.ShipnetChargeGroupCollection":
				case "Enterprise.Client.SWL.Shipnet.Business.ShipnetChargeCollection":
				case "Enterprise.BatchProcess.Testing.DummyProcessLockCollection":
				case "Enterprise.ServiceManager.Business.ServiceTaskScheduleCollection":
				case "Enterprise.ServiceManager.Business.ServiceTaskLogViewer+LogFileCollection":
				case "Enterprise.ServiceManager.Business.ServiceTaskLogViewer+EventCollection":
				case "Enterprise.ServiceManager.Business.StmServiceHostCollection":
				case "Enterprise.ClientSharedComponents.Registry.EventRegistryBusinessObjectCollection":
				case "Enterprise.ClientSharedComponents.Registry.BranchCodeMappingRegistryBusinessObjectCollection":
				case "Enterprise.ClientSharedComponents.Registry.ChargeCodeMappingRegistryBusinessObjectCollection":
				case "Enterprise.ClientSharedComponents.Registry.DepartmentCodeMappingRegistryBusinessObjectCollection":
				case "Enterprise.ClientSharedComponents.Registry.IDSInterfaceSetupCollection":
				case "Enterprise.Tracking.Business.TrackingTransactionHeaderCollection":
				case "Enterprise.Tracking.Business.TrackingConsolCollection":
				case "Enterprise.Tracking.Business.TrackingConsolManyToManyCollection":
				case "Enterprise.Tracking.Business.ContainerSummaryRowCollection":
				case "Enterprise.Tracking.Business.TrackingContainerStandaloneCollection":
				case "Enterprise.Tracking.Business.TrackingContainerCollection":
				case "Enterprise.Tracking.Business.TrackingContainerManyToManyCollection":
				case "Enterprise.Tracking.Business.TrackingShipmentContainerCollection":
				case "Enterprise.Tracking.Business.DocumentViewCollection":
				case "Enterprise.Tracking.Business.CustomsExchangeRateCollection":
				case "Enterprise.Tracking.Business.TrackingOrderLineCollection":
				case "Enterprise.Tracking.Business.TrackingOrderCollection":
				case "Enterprise.Tracking.Business.TrackingOrderDependentCollection":
				case "Enterprise.Tracking.Business.TrackingPackLineCollection":
				case "Enterprise.Tracking.Business.TrackingPackLineManyToManyCollection":
				case "Enterprise.Tracking.Business.Quotations.TrackingQuoteCollection":
				case "Enterprise.Tracking.Business.ShipDecCollection":
				case "Enterprise.Tracking.Business.TrackingConsolShipmentCollection":
				case "Enterprise.Tracking.Business.TrackingShipmentCollection":
				case "Enterprise.Tracking.Business.WhsTrackingInventorySummaryItemViewCollection":
				case "Enterprise.Tracking.Business.TrackingWhsInventoryCollection":
				case "Enterprise.Tracking.Business.TrackingWhsInwardsLineCollection":
				case "Enterprise.Tracking.Business.TrackingWhsOrderLineCollection":
				case "Enterprise.Tracking.Business.TrackingInventorySummaryCollection":
				case "Enterprise.Tracking.Business.ShipmentDeclarationCollection":
				case "Enterprise.WebCFS.Business.ContainerAvailabilityCollection":
				case "Enterprise.WebCFS.Business.FumigationCollection":
				case "Enterprise.WebCFS.Business.SailingCollection":
				case "Enterprise.Client.EDI.MasterFiles.Business.EDIOrgMiscServCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterpriseCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftwareCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftwareDependentCollection":
				case "Enterprise.Client.EDI.IssueManager.Business.ErrorLogKeyCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.EngineeringTaskCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.NewWorkItemCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeaderCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceCompanyLicenceDatabaseCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabaseCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabaseNonDependentCollection":
				case "Enterprise.Client.EDI.AutoDeploy.Business.UpgradesToClientCollection":
				case "Enterprise.Client.EDI.ReleaseBuilds.Business.ReleaseBuildCollection":
				case "Enterprise.Client.EDI.MasterFiles.Business.EDIOrgCompanyDataDependentCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.IncidentNotesWithoutDescription+IncidentNoteWithoutDescriptionDependentCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.IncidentNotesWithoutDescription+IncidentNoteWithoutDescriptionCollection":
				case "Enterprise.Client.EDI.MasterFiles.Business.EDIOrgSupplierPartCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceCompanyCollection":
				case "Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunityValueCollection":
				case "Enterprise.Client.EDI.IssueManager.Business.ErrorLogOccurrenceCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.IncidentNotes+IncidentNoteDependantCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.IncidentNotes+IncidentNoteCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.ProjectCollection":
				case "Enterprise.ProcessManagement.Business.WorkTaskRelatedItemGenPivotCollection":
				case "Enterprise.ProcessManagement.Business.WorkRequestRelatedItemCollection":
				case "Enterprise.EConversation.Testing.JobConversationTest+DummyRelatedItemCollection":
				case "Enterprise.Client.EDI.MasterFiles.Business.EDIOrgStaffAssignmentsCollection":
				case "Enterprise.Client.EDI.eRouter.Business.eRouterEdiEnterpriseCommunicationCollection":
				case "Enterprise.Client.EDI.IssueManager.Business.ErrorLogCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModulesDependentCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.FeatureRequestRelatedFeatureRequestsCollection":
				case "Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeaderCollection":
				case "Enterprise.Client.EDI.Mail.Business.CustomerServiceMailItemCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnectionDependentCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuoteCollection":
				case "Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollection":
				case "Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogMatchingCollection":
				case "Enterprise.ProcessManagement.Business.WorkItemRelatedItemCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentRelatedWorkItemCollection":
				case "Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServiceQuoteWorkItemCollection":
				case "Enterprise.Client.Wow.WowDocJobComInvoiceLineCollection":
				case "Enterprise.Client.Wow.WoolworthsOrderCollection":
				case "Enterprise.Client.Wow.WowDocLandedCostingCalculatorCollection":
				case "Enterprise.Client.Wow.WoolworthsCusContainerCollection":
				case "Enterprise.Client.Rohlig.ImportAirServiceLevelsCollection":
				case "Enterprise.Client.Hellmann.CSRChargeCollection":
				case "Enterprise.Client.Hellmann.CSREntryCollection":
				case "Enterprise.Client.TNT.QuantumMawbCollection":
				case "Enterprise.Client.TNT.ReadOnlyCusMAWBCollection":
				case "Enterprise.Client.TNT.NZ.NZQuantumMawbCollection":
				case "Enterprise.Client.TNT.AirCargo.XXXAirCargoCollection":
				case "Enterprise.Client.TNT.AirCargo.IQDownAirCargoCollection":
				case "Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection":
				case "Enterprise.Client.UPE.Business.BISIUploadedShipmentChargeCollection":
				case "Enterprise.Client.UPE.Business.UPECusHAWBCollection":
				case "Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection":
				case "Enterprise.Client.UPE.Business.DocCalloutChargeCollection":
				case "Enterprise.Client.UPE.Business.UPEPrintBatchCollection":
				case "Enterprise.Client.UPE.Business.WayBillChildPackageCollection":
				case "Enterprise.Client.UPE.Business.CalloutChargeCollection":
				case "Enterprise.Client.UPE.Business.UPEPrintBatchItemCollection":
				case "Enterprise.Client.UPE.UPEBranchIDsRegistryObjectCollection":
				case "Enterprise.Client.UPE.Business.CalloutCollection":
				case "Enterprise.Client.UPE.Business.ClassifierAllocationCollection":
				case "Enterprise.Client.JAS.Business.JASForwardingConsolShipmentCollection":
				case "Enterprise.Client.JAS.Registry.Business.CognosGlbDepartmentCollectionView":
				case "Enterprise.Client.JAS.Business.Cognos.ManyToManyCognosCreditorCollection":
				case "Enterprise.Client.JAS.Business.Cognos.ManyToManyCognosDebtorCollection":
				case "Enterprise.Client.JAS.Business.Cognos.ManyToManyCreditorExtraInfoCollection":
				case "Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfoCollection":
				case "Enterprise.Client.JAS.Business.Cognos.ManyToManyDebtorExtraInfoCollection":
				case "Enterprise.Client.JAS.Business.Cognos.CognosGroupingFlagsCollection":
				case "Enterprise.Client.GFS.DocGluckShipmentCollection":
				case "Enterprise.Client.MFI.DocWrappers.DocMFIContainerCollection":
				case "Enterprise.Client.DHL.Business.DHLJobDeclarationCollection":
				case "Enterprise.Client.WCB.InvLineFixedCollection":
				case "Enterprise.Client.WCB.InvHeadFixedCollection":
				case "Enterprise.Client.WCB.JobDeclarationFixedCollection":
				case "Enterprise.Client.RHL.Registry.ClientDataExportSettingsRegistryBusinessObjectCollection":
				case "Enterprise.Client.RHL.ImporterMappingRegistryBusinessObjectCollection":
				case "Enterprise.Client.RHL.Registry.RHLOverheadPeriodsCollection":
				case "Enterprise.Client.AUS.Business.ClientAUSProductImportRegistryCollection":
				case "Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMappingCollection":
				case "Enterprise.Client.OSP.DocWrappers.DocOSPPackLinesCollection":
				case "Enterprise.Client.OSP.DocWrappers.DocOSPPaymentItemCollection":
				case "Enterprise.Client.OSP.DocWrappers.DocOSPForwardingShipmentCollection":
				case "Enterprise.Client.OSP.DocWrappers.DocOSPARInvoiceLineCollection":
				case "Enterprise.Client.OSP.DocWrappers.DocOSPContainerCollection":
				case "Enterprise.Client.TIP.DischargePortEmailAddressCollection":
				case "Enterprise.Client.TIP.DocWrappers.DocTIPHeaderLineTransactionCollection":
				case "Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObjectCollection":
				case "Enterprise.Client.DFD.Registry.EventRegistryObjectCollection":
				case "Enterprise.Client.CIS.RefCodeRegistryBusinessObjectCollection":
				case "Enterprise.Client.CIS.ChargeCodeRegistryBusinessObjectCollection":
				case "Enterprise.Client.ELA.Registry.CustNoteTypeRegistryCollection":
				case "Enterprise.Client.ELA.DebtorEmailRegistryBusinessObjectCollection":
				case "Enterprise.Freight.LocalCartage.Module.ModuleCartageLegCollection":
				case "Enterprise.Freight.LocalCartage.Module.ModuleCartageCollection":
				case "Enterprise.Freight.LocalCartage.Business.ModuleCartageRunSheetCollection":
					result = true;
					break;
				default:
					if (collectionType.IsGenericType)
					{
						Type definition = collectionType.GetGenericTypeDefinition();

						// If the class is nested inside a generic class then it is flagged as generic
						// and yet GetGenericTypeDefinition returns the same collectionType resulting in a stack overflow.
						if (definition == collectionType)
						{
							return false;
						}

						result = EnsureIsNotNewUsageOfThisObsoleteCollection(collectionType.GetGenericTypeDefinition()) | EnsureIsNotNewUsageOfThisObsoleteCollection(collectionType.BaseType);
					}
					else if (Array.Find(collectionType.GetInterfaces(), t => t.Name == "IRegistryBusinessObjectCollectionTemplate") != null)
					{
						result = true;
					}
					else if (Array.Find(collectionType.GetInterfaces(), t => t.Name == "IProcessTaskCollection") != null)
					{
						result = true;
					}
					else if (
						collectionType.GetType().Assembly != typeof(BusinessObjectCollection).Assembly &&
						collectionType.BaseType.IsSubclassOf(typeof(BusinessObjectCollection)))
					{
						result = EnsureIsNotNewUsageOfThisObsoleteCollection(collectionType.BaseType);
					}
					else
					{
						result = false;
					}
					break;
			}
			return result;
		}

#endif
		#endregion

		#region SubscribeToChildrenChanges

		public IDisposable SubscribeToChildrenChanges(Action action, string[] fields = null)
		{
			return new CollectionChildrenChangesSubscription(this, action, fields);
		}

		#endregion
	}

	#region Test
#if DEBUG
	public abstract partial class BusinessObjectCollection
	{
		bool subscriptionFired;
		bool subscriptionUpdateAttempted;

		public void StartPublish()
		{
			subscriptionFired = false;
			subscriptionUpdateAttempted = false;
		}

		partial void UpdateSubscriptionStats(bool result)
		{
			subscriptionUpdateAttempted = true;
			if (result)
			{
				subscriptionFired = true;
			}
		}

		public bool SubscriptionFired
		{
			get { return subscriptionFired; }
		}

		public bool SubscriptionUpdateAttempted
		{
			get { return subscriptionUpdateAttempted; }
		}

		public HashedBizOList ElementsForTest
		{
			get { return Elements; }
		}
	}

#endif
	#endregion

}
