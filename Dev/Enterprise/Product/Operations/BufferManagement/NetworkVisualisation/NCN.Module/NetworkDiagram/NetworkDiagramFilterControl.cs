using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.Module
{
	public partial class NetworkDiagramFilterControl : ZFilterStripControl
	{
		public NetworkDiagramFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override bool ShouldInvokeFilterStrips => FilterBusinessObject.ParentModule.AllowNew;
	}
}
