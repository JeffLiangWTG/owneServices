using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.GUI;

public class OrganisationDetailsPlugIn : BaseOrganisationPlugIn
{
	public OrganisationDetailsPlugIn(OrgHeader organisation) : base(organisation)
	{
	}

	public override string Name => Res.GetString("8B80C60F-4789-4CA4-977A-6E376634617A", "India");

	protected override Control GetNewUserControl() => new OrganisationDetailsPlugInUserControl();
}
