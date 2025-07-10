using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class SeaCargoDepotFilterControl : ZFilterStripControl
	{
		public SeaCargoDepotFilterControl()
		{
			InitializeComponent();
		}

		public SeaCargoDepotFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
