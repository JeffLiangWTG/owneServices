using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class CashAdvanceRequestHeaderCollection : BusinessObjectCollection<CashAdvanceRequestHeader>
	{
		public CashAdvanceRequestHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CashAdvanceRequestHeaderCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
