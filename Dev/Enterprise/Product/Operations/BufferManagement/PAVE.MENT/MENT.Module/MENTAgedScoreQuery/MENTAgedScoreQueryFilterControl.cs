using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.Module
{
	partial	class MENTAgedScoreQueryFilterControl : ZFilterStripControl
	{
		public MENTAgedScoreQueryFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
