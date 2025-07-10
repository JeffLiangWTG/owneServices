using System.Collections;
using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// A hierarchy of business object collections flattened into one.
	/// </summary>
	public abstract class FlatHierarchyBusinessObjectCollection : BusinessObjectCollection<BusinessObject>
	{
		protected FlatHierarchyBusinessObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		/// <summary>
		/// All children below the TopOfTree business object.
		/// Each business object returned should be part of another collection.
		/// </summary>
		public abstract BusinessObjectCollection GetAllChildrenOf(BusinessObject topOfTree);

		/// <summary>
		/// Fill the collection with the children of the TopOfTree business object.
		/// </summary>
		public void LoadForTopBusinessObject(BusinessObject topOfTree, BusinessObjectCollection realCollectionToAddTo)
		{
			this.RealCollectionToAddTo = realCollectionToAddTo;

			try
			{
				fIsLoading = true;
				RemoveAllButLeaveRelationshipsIntact();

				foreach (BusinessObject bizO in GetAllChildrenOf(topOfTree))
				{
					Elements.Add(bizO);
					bizO.ReadOnly = ReadOnly;
					HookupElementChangedEvent(bizO);
				}
			}
			finally
			{
				FireListResetEvent();
				fIsLoading = false;
			}
		}

		public int IndexOf(BusinessObject bizO)
		{
			return ((IList)this).IndexOf(bizO);
		}

		#region Implementation

		protected bool fIsLoading;
		protected BusinessObjectCollection RealCollectionToAddTo;

		protected internal override void SetCollectionRelationships(BusinessObject child)
		{
			if (!fIsLoading && RealCollectionToAddTo != null)
			{
				RealCollectionToAddTo.SetCollectionRelationships(child);
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			if (!fIsLoading && RealCollectionToAddTo != null)
			{
				RealCollectionToAddTo.RemoveCollectionRelationships(child, forDelete);
				((IBindingList)RealCollectionToAddTo).Remove(child);
				if (forDelete)
				{
					((IBusinessObjectCollectionInternals)RealCollectionToAddTo).HasChangesFromDelete = true;
				}
			}
		}

		protected internal override void SetDefaultsForNewChild(BusinessObject child)
		{
			if (!fIsLoading && RealCollectionToAddTo != null)
			{
				RealCollectionToAddTo.SetDefaultsForNewChild(child);
				((IBindingList)RealCollectionToAddTo).Add(child);
			}
		}

		#endregion
	}
}
