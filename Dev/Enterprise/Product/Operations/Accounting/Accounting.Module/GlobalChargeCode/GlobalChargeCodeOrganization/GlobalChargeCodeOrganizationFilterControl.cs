using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class GlobalChargeCodeOrganizationFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}

		public GlobalChargeCodeOrganizationFilterControl(IBusinessObjectCollection gridCollection, GlobalChargeCodeOrganizationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
