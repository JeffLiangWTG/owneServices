
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrgSupplierPartControllerOverride : OrgSupplierPartController
	{
		protected override Enterprise.ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new WowOrgSupplierPartForm((AUOrgSupplierPart)businessEntity);
		}
	}
}
