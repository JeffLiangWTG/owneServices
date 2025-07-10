using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class GLConsolidationGroupFilterControl : ZFilterStripControl
	{
		public GLConsolidationGroupFilterControl()
		{
			InitializeComponent();
		}

		public GLConsolidationGroupFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
