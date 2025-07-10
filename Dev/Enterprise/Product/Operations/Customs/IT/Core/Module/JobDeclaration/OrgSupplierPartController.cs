using CargoWise.EntityFramework;
using Enterprise.Customs.IT.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IT.Module;

public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
{
	protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
	{
		return new OrgSupplierPartFormCustomsPlugin((Business.OrgSupplierPart)businessEntity);
	}
}
