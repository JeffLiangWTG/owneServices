using System;
using System.Linq;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class EdiAgreementAssignmentUserControl : ZUserControl
	{
		public EdiAgreementAssignmentUserControl()
		{
			InitializeComponent();
			this.hasClickingAddLogButtonSecurity = EDISecurityCheckpoints.UserAgreementAddAcceptanceLogs.IsAllowed;
			this.corporateAgreementsGrid.ReadOnly = !EDISecurityCheckpoints.UserAgreementAssignCorporateAgreements.IsAllowed;
			this.corporateAgreementsGrid.SelectedRowsChangedInMouseDown += CorporateAgreementsGrid_SelectedRowsChangedInMouseDown;
			CorporateAgreementsGrid_SelectedRowsChangedInMouseDown(null, EventArgs.Empty);
		}

		readonly bool hasClickingAddLogButtonSecurity;

		void CorporateAgreementsGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			var selectedRows = corporateAgreementsGrid.SelectedElements.Cast<EdiUserAgreementAssignment>().ToArray();
			addAcceptanceButton.Enabled = hasClickingAddLogButtonSecurity && selectedRows.Length == 1;
		}

		EdiUserAgreementAssignmentCollection agreementAssignments => DataSource as EdiUserAgreementAssignmentCollection;

		void AddAcceptanceButton_Click(object sender, EventArgs e)
		{
			var data = corporateAgreementsGrid.SelectedElements.Cast<EdiUserAgreementAssignment>().FirstOrDefault();
			if (data != null)
			{
				if(data.HasChanges)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("e6ef5d2a-57c5-48ea-b7a1-d67b5f6b1907", "The selected assignment was changed, please save form and try again."));
					return;
				}

				if(data.CurrentAgreement == null)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("51c9dd08-e18a-4eb5-9053-9134b19e72f3", "The selected assignment has no agreement, please create a agreement or select another agreement type."));
					return;
				}

				var log = data.AcceptanceLogs.AddNew();
				log.EUL_LE = agreementAssignments.Parent.PK;
				log.EUL_ERA = data.CurrentAgreement.PK;
				log.IsAddedManually = true;
				if (ZFormModaliser.ShowDialogAndDispose(new EdiAddUserAgreementAcceptanceForm(log)) != System.Windows.Forms.DialogResult.OK)
				{
					log.Delete();
				}
			}
		}
	}
}
