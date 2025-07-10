using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	public class EDIOrganisationFilterControlTest : TestOrganisationFilterControl
	{
		protected override OrganisationFilterControl GetNewOrganisationFilterControl() => new EDIOrganisationFilterControl();
	}
}
