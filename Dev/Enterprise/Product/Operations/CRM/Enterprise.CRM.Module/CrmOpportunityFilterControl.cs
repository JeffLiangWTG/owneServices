using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CRM.Module
{
	public partial class CrmOpportunityFilterControl : ZFilterStripControl
	{
		public CrmOpportunityFilterControl()
		{
			InitializeComponent();
		}

		public CrmOpportunityFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
