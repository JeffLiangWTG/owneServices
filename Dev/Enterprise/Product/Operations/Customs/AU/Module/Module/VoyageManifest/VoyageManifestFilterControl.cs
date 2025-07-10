using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Filter control for VoyageManifest.
	/// </summary>
	public partial class VoyageManifestFilterControl : ZFilterStripControl
	{
		public VoyageManifestFilterControl()
		{
			InitializeComponent();
		}

		public VoyageManifestFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
