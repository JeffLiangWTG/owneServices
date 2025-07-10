using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class ARMatchingFilterControl : MatchingFilterControl
	{
		public ARMatchingFilterControl()
		{
			SetContext(FilteredGridColumnLayoutContext.AR);
			InitializeComponent();
		}

		public ARMatchingFilterControl(IBusinessObjectCollection matchLinks, FilterStripBusinessObject filterBizO)
			: base(matchLinks, filterBizO)
		{
			SetContext(FilteredGridColumnLayoutContext.AR);
			InitializeComponent();
		}	
	}
}

