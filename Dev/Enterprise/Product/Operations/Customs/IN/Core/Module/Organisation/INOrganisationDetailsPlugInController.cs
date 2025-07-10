using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IN.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IN.Module;

public class INOrganisationDetailsPlugInController : BaseOrganisationPlugInController
{
	public override ControllerID ID => ControllerIDs.Customs.IN.OrganisationDetailsPlugIn;

	public override ResourceStringData PluginTabPageCaption => Res.GetData("CFDE135E-CFD6-4900-8B55-68B47F57578C", "India");

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrganisationDetailsPlugIn((OrgHeader)businessEntity);
}
