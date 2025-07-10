using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.DE.Module
{
	public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
		}
	}
}
