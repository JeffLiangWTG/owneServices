using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.ReleaseBuilds.Module
{
	partial class ReleaseBuildFilterControl : ZFilterStripControl
	{
		public ReleaseBuildFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public new ReleaseBuildFilterBusinessObject FilterBusinessObject
		{
			get { return (ReleaseBuildFilterBusinessObject)base.FilterBusinessObject; }
		}
	}
}
