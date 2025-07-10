using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.SWL
{
	public class SWLOrganisationControllerOverride : OrganisationController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SWLOrganisationForm((OrgHeader)businessEntity);
		}
	}
}
