using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module
{
	public partial class CDSDISQueryFilterStripControl : ZFilterStripControl
	{
		public CDSDISQueryFilterStripControl()
		{
			InitializeComponent();
		}

		public CDSDISQueryFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
