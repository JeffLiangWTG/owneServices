using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public class AccrualCollection : WIPAccrualCollection, IRecalculateAmountsEventSupporter
	{
		public AccrualCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			SetAllowAddRemoveDefaults();
		}

		public AccrualCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetAllowAddRemoveDefaults();
		}

		public new Accrual this[int index]
		{
			get { return (Accrual)Elements[index]; }
		}

		public virtual new Accrual AddNew()
		{
			return (Accrual)base.AddNew();
		}

		protected new static ZQuery CombineFilterWithStaticFilter(ZQuery filter)
		{
			filter.AddToFilter(AccrualFilterStatic());
			return filter;
		}

		#region Allow AddRemove

		protected override BusinessObject AddNewCore()
		{
			return Base_AddNewCore();
		}

		protected override bool AllowNewCore
		{
			get { return fAllowNewCore; }
		}
		bool fAllowNewCore;

		public void SetAllowNew(bool value)
		{
			fAllowNewCore = value;
		}

		protected override bool AllowRemoveCore
		{
			get { return fAllowRemoveCore; }
		}
		bool fAllowRemoveCore;

		public void SetAllowRemove(bool value)
		{
			fAllowRemoveCore = value;
		}

		void SetAllowAddRemoveDefaults()
		{
			SetAllowNew(true);
			SetAllowRemove(base.AllowRemoveCore);
		}

		#endregion

		#region IRecalculateAmountsEventSupporter Members

		public void RaiseRecalculateAmounts()
		{
			if (RecalculateAmounts != null)
			{
				RecalculateAmounts(this, EventArgs.Empty);
			}
		}

		public event EventHandler RecalculateAmounts;

		#endregion

		#region Implementation

		protected override ZQuery WIPFilter()
		{
			return new ZQuery();
		}

		#endregion
	}
}
