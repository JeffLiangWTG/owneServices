using Enterprise.Client.EDI.MasterFiles.Module;
using Enterprise.MasterData.GUI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	class EDIDeduplicationOrganizationsUserControl : DeduplicationOrganizationsUserControl
	{
		protected override FilterStripBusinessObject GetNewDeduplicationOrganisationFilterBusinessObject()
		{
			return new EDIDedupOrgFilterBusinessObject();
		}
	}
}
