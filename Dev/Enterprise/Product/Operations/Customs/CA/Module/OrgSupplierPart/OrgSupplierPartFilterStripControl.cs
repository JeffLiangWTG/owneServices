using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Module
{
	public class OrgSupplierPartFilterStripControl : Customs.Module.OrgSupplierPartFilterStripControl
	{
		public OrgSupplierPartFilterStripControl(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
