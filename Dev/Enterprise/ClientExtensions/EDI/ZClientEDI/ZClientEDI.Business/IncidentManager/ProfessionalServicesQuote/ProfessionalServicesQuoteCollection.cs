using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID("ProfessionalServicesQuote")]
	public class ProfessionalServicesQuoteCollection : BusinessObjectCollection<ProfessionalServicesQuote>
	{
		public ProfessionalServicesQuoteCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ProfessionalServicesQuoteCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.ProfessionalServicesQuote);
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

