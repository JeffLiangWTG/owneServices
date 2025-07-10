using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EDIAdministrationPanelForm : AdministrationPanelForm
	{
		public EDIAdministrationPanelForm(AdministrationPanelManager manager) : base(manager) { }

		protected override ZUserControl GetNewDuplicationOrganizationsUserControl()
		{
			return new EDIDeduplicationOrganizationsUserControl();
		}
	}
}
