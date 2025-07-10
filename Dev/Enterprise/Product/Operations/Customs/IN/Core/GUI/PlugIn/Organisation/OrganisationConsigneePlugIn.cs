using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.GUI;

public class OrganisationConsigneePlugIn : BaseOrganisationPlugIn
{
	public OrganisationConsigneePlugIn(OrgHeader organisation) : base(organisation)
	{
	}

	protected override Control GetNewUserControl() => new OrganisationConsigneePlugInUserControl();
}
