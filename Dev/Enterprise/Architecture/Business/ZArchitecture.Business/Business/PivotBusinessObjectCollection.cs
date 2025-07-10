using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class PivotBusinessObjectCollection<T> : ActiveBusinessObjectCollection<T>, IPivotBusinessObjectCollection
		where T : BusinessObject, IPivotBusinessObject
	{
		protected PivotBusinessObjectCollection(BusinessObject master, ICollectionRelationship relationship, bool includeChildren = true, bool includeParents = true)
			: base(master.Factory, relationship)
		{
			Master = master;
			IncludeChildren = includeChildren;
			IncludeParents = includeParents;
		}

		public BusinessObject Master { get; }
		protected bool IncludeChildren { get; }
		protected bool IncludeParents { get; }

		#region Add

		public T AddChild(BusinessObject child)
		{
			var pivot = AddNew();
			pivot.Relation1ID = Master.PK;
			pivot.Relation2ID = child.PK;
			OnChildAdded(child, pivot);

			return pivot;
		}

		protected virtual void OnChildAdded(BusinessObject child, T pivot)
		{
		}

		public T AddParent(BusinessObject parent)
		{
			var pivot = AddNew();
			pivot.Relation1ID = parent.PK;
			pivot.Relation2ID = Master.PK;
			OnParentAdded(parent, pivot);

			return pivot;
		}

		protected virtual void OnParentAdded(BusinessObject parent, T pivot)
		{
		}

		#endregion

		#region Related

		public IPivotBusinessObject FindRelated(ZGuid pk)
		{
			return this.FirstOrDefault<IPivotBusinessObject>(x => x.Relation1ID == pk || x.Relation2ID == pk);
		}

		public IPivotBusinessObject AddRelatedIfNotExist(BusinessObject related, bool addAlwaysAsParent = false)
		{
			if (related != null && FindRelated(related.PK) == null)
			{
				return IncludeChildren && !addAlwaysAsParent ? AddChild(related) : AddParent(related);
			}

			return null;
		}

		#endregion

		#region Implementation

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (IncludeChildren)
			{
				newElement.Relation1ID = Master.PK;
				SetAdditionalDefaultsForNewChild(newElement);
			}
			else
			{
				newElement.Relation2ID = Master.PK;
				SetAdditionalDefaultsForNewParent(newElement);
			}
			// Note: Any state set here must be part of the collection relationship hash code or an override of GetCollectionState().
			// Otherwise adding an element to this collection may actually call SetDefaultsForNewElementCore for another collection
			// that shares the same index.
			// Index sharing is based on the hash code of the relationship filter and the collection state.
		}

		protected virtual void SetAdditionalDefaultsForNewChild(T newElement)
		{
		}

		protected virtual void SetAdditionalDefaultsForNewParent(T newElement)
		{
		}

		#endregion

		public new IEnumerator<IPivotBusinessObject> GetEnumerator()
		{
			return base.GetEnumerator();
		}
	}
}
