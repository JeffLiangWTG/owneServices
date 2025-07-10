using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Module
{
	public partial class NctsMovementFilterControl : EU.NCTS.Module.NctsMovementFilterControl
	{
		public NctsMovementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
