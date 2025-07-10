using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.GUI;

public class OrganisationConsignorPlugIn : BaseOrganisationPlugIn
{
	public OrganisationConsignorPlugIn(OrgHeader organisation) : base(organisation)
	{
	}

	protected override Control GetNewUserControl() => new OrganisationConsignorPlugInUserControl();
}
