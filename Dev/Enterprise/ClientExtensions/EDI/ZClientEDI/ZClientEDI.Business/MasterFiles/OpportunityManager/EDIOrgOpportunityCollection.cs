using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.OpportunityManager.Business
{
	public class EDIOrgOpportunityCollection : OrgOpportunityCollection
	{
		public EDIOrgOpportunityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EDIOrgOpportunityCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (ItemAdded != null)
			{
				ItemAdded(bizOAdded);
			}
		}

		protected override void OnRemoved(BusinessObject bizOAdded)
		{
			base.OnRemoved(bizOAdded);
			if (ItemRemoved != null)
			{
				ItemRemoved(bizOAdded);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public delegate void ItemCountChangedEventHandler(BusinessObject bizO);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event ItemCountChangedEventHandler ItemAdded;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event ItemCountChangedEventHandler ItemRemoved;
	}
}

