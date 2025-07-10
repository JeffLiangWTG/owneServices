using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EDIOpportunityManagementForm : OpportunityForm
	{
		public EDIOpportunityManagementForm(EDIOrgOpportunity opportunity)
			: base(opportunity)
		{
			PlugIns.Add(ClientControllerRegistration.OpportunityClientOrgLicence);
			PlugIns.Add(ClientControllerRegistration.OpportunityRelatedProjects);
			PlugIns.Add(ClientControllerRegistration.EDIOpportunityRelatedPSQs);
			AddSalesDiscoveryMenuItem();
		}

		#region Actions Menu

		void AddSalesDiscoveryMenuItem()
		{
			if (BusinessEntity != null)
			{
				ActionsMenuItem.MenuItems.Add(SalesDiscoveryConductorUrlHelper.SalesMenuItem(BusinessEntity));
			}
		}

		#endregion

		protected override OpportunityManagementDetailsControl CreateOpportunityManagementDetailsControl()
		{
			return new EDIOpportunityManagementDetailsControl();
		}
	}
}
