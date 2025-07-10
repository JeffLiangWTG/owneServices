using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.GB.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.Module
{
	public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GBOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
		}
	}
}
