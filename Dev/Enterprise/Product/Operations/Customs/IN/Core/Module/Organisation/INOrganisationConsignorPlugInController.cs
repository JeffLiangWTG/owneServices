using CargoWise.EntityFramework;
using Enterprise.Customs.IN.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IN.Module;

public class INOrganisationConsignorPlugInController : BaseOrganisationPlugInController
{
	public override ControllerID ID => ControllerIDs.Customs.IN.OrganisationConsignorPlugIn;

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrganisationConsignorPlugIn((OrgHeader)businessEntity);
}
