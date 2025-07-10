using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Module
{
	public partial class OrgSupplierPartFilterStripControl : Customs.Module.OrgSupplierPartFilterStripControl
	{
		public OrgSupplierPartFilterStripControl(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
