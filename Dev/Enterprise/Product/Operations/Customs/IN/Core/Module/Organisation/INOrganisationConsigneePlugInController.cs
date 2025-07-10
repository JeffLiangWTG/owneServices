using CargoWise.EntityFramework;
using Enterprise.Customs.IN.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IN.Module;

public class INOrganisationConsigneePlugInController : BaseOrganisationPlugInController
{
	public override ControllerID ID => ControllerIDs.Customs.IN.OrganisationConsigneePlugIn;

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrganisationConsigneePlugIn((OrgHeader)businessEntity);
}
