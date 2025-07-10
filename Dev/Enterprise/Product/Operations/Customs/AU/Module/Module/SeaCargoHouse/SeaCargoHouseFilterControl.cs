using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class SeaCargoHouseFilterControl : ZFilterStripControl<WorkflowFilterStrip>
	{
		public SeaCargoHouseFilterControl()
		{
			InitializeComponent();
		}

		public SeaCargoHouseFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
