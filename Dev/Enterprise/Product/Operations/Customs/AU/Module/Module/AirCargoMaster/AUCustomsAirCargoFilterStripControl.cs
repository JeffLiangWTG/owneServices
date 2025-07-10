using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public partial class AUCustomsAirCargoFilterStripControl : ZFilterStripControl<AUCustomsAirCargoModuleStrip>
	{
		public AUCustomsAirCargoFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public AUCustomsAirCargoFilterStripControl()
		{
			InitializeComponent();
		}
	}
}
