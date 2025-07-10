using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public partial class LicenceKeyFilterControl : ZFilterStripControl
	{
		public LicenceKeyFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
