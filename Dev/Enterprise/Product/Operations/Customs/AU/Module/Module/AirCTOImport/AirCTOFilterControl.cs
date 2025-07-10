using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class AirCTOFilterControl : ZFilterStripControl
	{
		public AirCTOFilterControl()
		{
			InitializeComponent();
		}

		public AirCTOFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip() => new AirCTOImportModuleStrip();
	}
}
