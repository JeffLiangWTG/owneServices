using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Billing
{
	public class AccBillingItemCollection : DependentBusinessObjectCollection<AccBillingItem, AccBillingHeader>
	{
		public AccBillingItemCollection(AccBillingHeader master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var item = child as AccBillingItem;
			if (item != null)
			{
				item.ABI_ABH = Master.PK;
			}
		}
	}
}
