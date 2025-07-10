using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class AirCTOExportFilterStripControl : ZFilterStripControl
	{
		public AirCTOExportFilterStripControl()
		{
			InitializeComponent();
		}

		public AirCTOExportFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip() => new AirCTOExportModuleStrip();
	}
}
