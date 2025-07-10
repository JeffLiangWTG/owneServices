using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module
{
	public partial class HouseBilleManifestFilterStripControl : ZFilterStripControl
	{
		public HouseBilleManifestFilterStripControl()
		{
			InitializeComponent();
		}

		public HouseBilleManifestFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
