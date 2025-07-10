using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class SeaCargoOutturnBillsFilterControl : ZFilterStripControl
	{
		public SeaCargoOutturnBillsFilterControl()
		{
			InitializeComponent();
		}

		public SeaCargoOutturnBillsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
