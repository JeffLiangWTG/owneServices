
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class IndependentChargeCollectionForBinding : BusinessObjectCollection<Charge>
	{
		#region Construction

		public IndependentChargeCollectionForBinding(BusinessObjectFactory factory)
			: base(factory)
		{
			SetAllowAddRemoveDefaults();
		}

		public IndependentChargeCollectionForBinding(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
			SetAllowAddRemoveDefaults();
		}

		#endregion

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
	}
}