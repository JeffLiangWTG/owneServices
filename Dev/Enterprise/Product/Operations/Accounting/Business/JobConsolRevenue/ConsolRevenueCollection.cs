using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	public class ConsolRevenueCollection : NonPersistentBusinessObjectCollection<ConsolRevenue>
	{
		public ConsolRevenueCollection(ConsolRevenueMaster master, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Master = master;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ConsolRevenue(Master);
		}

		#region Implementation

		readonly ConsolRevenueMaster Master;

		#endregion
	}
}

#if DEBUG

namespace Enterprise.Accounting.Business.ConsolRevenue
{
}

#endif
