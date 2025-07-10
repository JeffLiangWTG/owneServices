using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectCollectionView : Integration.IBusinessObjectCollectionView, ISubsetBusinessObjectCollection
	{
		bool IsInDataBinding { get; set; }
		IDisposable SuspendRemovingWhenOnlyOneElementLeftForListChanged();
	}

	/// <summary>
	/// A view presenting a subset of another collection synchronising elements between two collections
	/// </summary>
	public abstract class BusinessObjectCollectionView<TBusinessObject> : SubsetBusinessObjectCollection<TBusinessObject>, IBusinessObjectCollectionView, IBusinessObjectCollectionInternals
		where TBusinessObject : BusinessObject
	{
		protected BusinessObjectCollectionView(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
			HookCollection();
		}

		public override void Add(BusinessObject businessObject)
		{
			InAdd = true;

			try
			{
				if (ShouldWeAddBusinessObjectStraightToView(businessObject))
				{
					base.Add(businessObject);
				}
				else
				{
					AddToFilteredCollection(businessObject);
				}
			}
			finally
			{
				InAdd = false;
			}
		}

		protected override sealed void SwapCollectionToFilterCore(BusinessObjectCollection newCollection)
		{
			UnhookCollection();
			collectionToFilter = newCollection;
			HookCollection();
			Rebuild();
		}

		protected virtual bool ShouldWeAddBusinessObjectStraightToView(BusinessObject businessObject)
		{
			return IsThisPartOfTheCollection(businessObject);
		}

		#region Implementation

		bool InAdd;
		bool InRemove;

		protected override bool AllowNewCore
		{
			get { return CollectionToFilter.AllowNew; }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || CollectionToFilter.ReadOnly; }
		}

		public override void AddGuidListMapping(string propertyName, string listName)
		{
			base.AddGuidListMapping(propertyName, listName);
			collectionToFilter.AddGuidListMapping(propertyName, listName);
		}

		public override void AddIsTime(string propertyName)
		{
			base.AddIsTime(propertyName);
			collectionToFilter.AddIsTime(propertyName);
		}

		internal override void ApplySortCore(ListSortDescriptionCollection sorts)
		{
			if (this.TypeOfElements.IsAssignableFrom(CollectionToFilter.TypeOfElements)) //Don't try to sort the collection we filter if it might contain other business object types that our sort doesn't make sense for
			{
				CollectionToFilter.ApplySortCore(sorts);
			}
			base.ApplySortCore(sorts);
		}

		internal void CollectionToFilter_AfterResort(object sender, EventArgs e)
		{
			if (Sorter.IsSorted)
			{
				Sorter.Resort();
				OnAfterResort();
			}
		}

		protected internal override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return collectionToFilter.GetComparerForSort(property, direction);
		}

		protected override void RebuildCore()
		{
			using (DoNotSuspendListChangedInCollectionToFilter())
			{
				base.RebuildCore();
			}
		}

		#region Handle Sync of Stuff happening in View collection

		protected internal override void SetCollectionRelationships(BusinessObject child)
		{
			using (IsNonCommittedCollectionElement(child) ? CollectionToFilter.OverrideNonCommittedCollectionElementTemporarily(child) : null)
			{
				CollectionToFilter.SetCollectionRelationships(child);
			}
			base.SetCollectionRelationships(child);
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			if (!InRemove)
			{
				CollectionToFilter.RemoveCollectionRelationships(child, forDelete);

				InRemove = true;
				try
				{
					if (CollectionToFilter.Contains(child))
					{
						((IBindingList)CollectionToFilter).Remove(child);
						if (forDelete)
						{
							((IBusinessObjectCollectionInternals)collectionToFilter).HasChangesFromDelete = true;
						}
					}
				}
				finally
				{
					InRemove = false;
				}
			}

			base.RemoveCollectionRelationshipsCore(child, forDelete);
		}

		protected internal override void SetDefaultsForNewChild(BusinessObject child)
		{
			CollectionToFilter.SetupNewElementButDoNotAddIt(child, true);
			base.SetDefaultsForNewChild(child);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			AddToFilteredCollection(bizOAdded);
			base.OnAdded(bizOAdded);
		}

		public override void SetupNewElementButDoNotAddIt(BusinessObject @new, bool setupCollectionRelationships)
		{
			SetDefaultsForNewChild(@new);
			CollectionToFilter.SetupNewElementButDoNotAddIt(@new, setupCollectionRelationships);
		}

		void AddToFilteredCollection(BusinessObject bizObj)
		{
			if (!CollectionToFilter.Contains(bizObj))
			{
				if (IsRebuilding && bizObj.PK == RebuildAddingBizoPk)
				{
					ErrorReporter.ReportOnce("ReAddingBizoToCollectionToFilterWhileRebuilding",
						string.Format("{0} with PK '{1}' has already been in CollectionToFilter of type {2} before rebuilding of collection view of type {3}.",
						bizObj.GetType().FullName, bizObj.PK, CollectionToFilter.GetType().FullName, GetType().FullName));
				}
				else
				{
					CollectionToFilter.Add(bizObj);
				}
			}
		}

		#endregion

		#region Handle Sync of Stuff happening in the Filtered collection

		public IDisposable SuspendRemovingWhenOnlyOneElementLeftForListChanged() => new DisposableAction(() => suspendRemovingWhenOnlyOneElementLeftForListChangedCounter++, () => suspendRemovingWhenOnlyOneElementLeftForListChangedCounter--);
		int suspendRemovingWhenOnlyOneElementLeftForListChangedCounter;
		bool ShouldSuspendRemovingWhenOnlyOneElementLeft => suspendRemovingWhenOnlyOneElementLeftForListChangedCounter > 0 && Elements.Count == 1;

		void CollectionToFilter_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.Reset)
			{
				if (!collectionToFilter.Sorter.IsSorting)
				{
					Rebuild(); // We don't want to rebuild every time the parent collection is sorted because this is expensive and the view has a different order anyway.
				}
				FireListResetEvent();
			}
			else if ((e.ListChangedType == ListChangedType.ItemChanged) && (e.NewIndex != -1))
			{
				var changed = (BusinessObject)((IList)CollectionToFilter)[e.NewIndex];
				if (changed != null)
				{
					bool containsChangedPK = Contains(changed.PK);
					bool isPartOfTheCollection = IsThisPartOfTheCollection(changed);
					if (containsChangedPK && !isPartOfTheCollection)
					{
						if (!InRemove && !ShouldSuspendRemovingWhenOnlyOneElementLeft)
						{
							try
							{
								InRemove = true;
								Remove(changed);
							}
							finally
							{
								InRemove = false;
							}
						}
					}
					else if (!containsChangedPK && isPartOfTheCollection)
					{
						if (!InAdd)
						{
							Add(changed);
						}
					}
				}
			}
		}

		void CollectionToFilter_ElementAdded(BusinessObject elementChanged)
		{
			if (IsThisPartOfTheCollection(elementChanged) && !Contains(elementChanged.PK))
			{
				Add(elementChanged);
			}
		}

		void CollectionToFilter_ElementRemoving(BusinessObject elementChanged)
		{
			if (!InRemove)
			{
				if (Contains(elementChanged.PK))
				{
					InRemove = true;
					try
					{
						Remove(elementChanged, !CollectionToFilter.InRemoveAllButLeaveRelationshipsIntact);
					}
					finally
					{
						InRemove = false;
					}
				}
			}
		}

		internal override bool IsListChangedSuspended
		{
			get { return CollectionToFilter.IsListChangedSuspended || base.IsListChangedSuspended; }
		}

		public IDisposable DoNotSuspendListChangedInCollectionToFilter() => new DisposableAction(() => doNotSuspendListChangedInCollectionToFilterCounter++, () => doNotSuspendListChangedInCollectionToFilterCounter--);
		int doNotSuspendListChangedInCollectionToFilterCounter;
		bool ShouldSuspendListChangedInCollectionToFilter => doNotSuspendListChangedInCollectionToFilterCounter == 0;

		protected override DisposableList GetAdditionalListChangedSuspenders()
		{
			DisposableList list = null;
			try
			{
				list = base.GetAdditionalListChangedSuspenders();
				if (ShouldSuspendListChangedInCollectionToFilter)
				{
					if (list == null)
					{
						list = new DisposableList(1);
					}
					list.Add(collectionToFilter.SuspendListChanged());
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				list?.Dispose();
				throw;
			}

			return list;
		}

		#endregion

		#region Masters Are In Database

		bool IBusinessObjectCollectionInternals.MastersAreInDatabase
		{
			get { return ((IBusinessObjectCollectionInternals)CollectionToFilter).MastersAreInDatabase; }
		}

		#endregion

		#region Enumeration

		internal override IEnumerator<BusinessObject> GetNewEnumerator()
		{
			if (isHooked.HasValue && !isHooked.Value)
			{
				ErrorReporter.ReportOnce("EnumeratingUnhookedBizoCollection", string.Format(CultureInfo.InvariantCulture, "Should not enumerate a deliberately unhooked collection of type [{0}]", this.GetType().FullName));
				HookCollection();
				Rebuild();
			}

			return base.GetNewEnumerator();
		}

		#endregion

		#region Hooking / Unhooking

		public void UnhookCollection()
		{
			CollectionToFilter.ElementAdded -= CollectionToFilter_ElementAdded;
			CollectionToFilter.ElementRemoving -= CollectionToFilter_ElementRemoving;
			CollectionToFilter.AfterResort -= CollectionToFilter_AfterResort;
			((IBindingList)CollectionToFilter).ListChanged -= CollectionToFilter_ListChanged;
			isHooked = false;
		}

		public void HookCollection()
		{
			if (!IsHooked)
			{
				CollectionToFilter.ElementAdded += CollectionToFilter_ElementAdded;
				CollectionToFilter.ElementRemoving += CollectionToFilter_ElementRemoving;
				CollectionToFilter.AfterResort += CollectionToFilter_AfterResort;
				((IBindingList)CollectionToFilter).ListChanged += CollectionToFilter_ListChanged;
				isHooked = true;
			}
		}

		bool? isHooked;

		public bool IsHooked => isHooked.HasValue && isHooked.Value;

		#endregion

		#region IsInDataBinding

		public bool IsInDataBinding { get; set; }

		#endregion

		#endregion
	}
}
