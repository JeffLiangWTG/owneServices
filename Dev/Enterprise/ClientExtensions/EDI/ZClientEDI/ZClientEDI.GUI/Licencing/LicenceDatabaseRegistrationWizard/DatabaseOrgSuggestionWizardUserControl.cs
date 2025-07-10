using System;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class DatabaseOrgSuggestionWizardUserControl : ZUserControl
	{
		public DatabaseOrgSuggestionWizardUserControl(EdiLicenceDatabaseOrgSuggestionCollection orgSuggestionCollection)
			: base()
		{
			InitializeComponent();
			SetDataBinding(orgSuggestionCollection, null);
		}

		void SetSelectSuggestionButton_Click(object sender, EventArgs e)
		{
			var orgSuggestion = OrgSuggestionGrid.ListManager.GetCurrent() as EdiLicenceDatabaseOrgSuggestion;
			if (orgSuggestion != null)
			{
				DatabaseOrgSuggestionSelected?.Invoke(this, orgSuggestion);
			}
		}

		public event EventHandler<EdiLicenceDatabaseOrgSuggestion> DatabaseOrgSuggestionSelected;
	}
}
