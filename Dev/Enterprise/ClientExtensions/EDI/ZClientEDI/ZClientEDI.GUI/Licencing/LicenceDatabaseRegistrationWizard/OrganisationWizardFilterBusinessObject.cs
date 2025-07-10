using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;

namespace ZClientEDI.GUI.Licencing
{
	public class OrganisationWizardFilterBusinessObject : EDIOrganisationFilterBusinessObjectCore
	{
		public OrganisationWizardFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "Organisation";
		}
	}
}
