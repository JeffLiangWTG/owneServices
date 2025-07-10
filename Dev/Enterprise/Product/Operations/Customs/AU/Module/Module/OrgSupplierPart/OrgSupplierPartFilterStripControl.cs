
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Module
{
	public partial class OrgSupplierPartFilterStripControl : Customs.Module.OrgSupplierPartFilterStripControl
	{
		public OrgSupplierPartFilterStripControl()
		{
			InitializeComponent();
		}

		public OrgSupplierPartFilterStripControl(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
