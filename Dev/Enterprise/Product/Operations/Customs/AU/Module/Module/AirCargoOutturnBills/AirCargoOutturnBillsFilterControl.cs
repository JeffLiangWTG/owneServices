using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class AirCargoOutturnBillsFilterControl : ZFilterStripControl<AirCargoOutturnBillsModuleStrip>
	{
		public AirCargoOutturnBillsFilterControl()
		{
			InitializeComponent();
		}

		public AirCargoOutturnBillsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
