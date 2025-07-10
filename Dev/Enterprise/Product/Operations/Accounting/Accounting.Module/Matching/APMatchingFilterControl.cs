using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class APMatchingFilterControl : MatchingFilterControl
	{
		public APMatchingFilterControl()
		{
			SetContext(FilteredGridColumnLayoutContext.AP);
			InitializeComponent();
		}

		public APMatchingFilterControl(IBusinessObjectCollection matchLinks, FilterStripBusinessObject filterBizO)
			: base(matchLinks, filterBizO)
		{
			SetContext(FilteredGridColumnLayoutContext.AP);
			InitializeComponent();
		}
	}
}
