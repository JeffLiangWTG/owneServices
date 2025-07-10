using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public partial class AirCargoDepotStandAloneFilterControl : ZFilterStripControl
	{
		public AirCargoDepotStandAloneFilterControl()
		{
			InitializeComponent();
		}

		public AirCargoDepotStandAloneFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
