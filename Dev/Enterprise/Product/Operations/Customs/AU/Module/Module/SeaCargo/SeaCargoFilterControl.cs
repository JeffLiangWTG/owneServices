using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class SeaCargoFilterControl : ZFilterStripControl<WorkflowFilterStrip>
	{
		public SeaCargoFilterControl()
		{
			InitializeComponent();
		}

		public SeaCargoFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
