using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmModuleFilterFilterControl : ZFilterStripControl
	{
		public ZStmModuleFilterFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
