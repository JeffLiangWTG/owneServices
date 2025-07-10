using System;
using Enterprise.Client.MFI.OrgImportFromMFICSVFile;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.MFI
{
	class MFIOrganisationModuleOverride : OrganisationModule
	{
		public MFIOrganisationModuleOverride()
			: base()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				AddImportDataMenuItem(orgCSVImportMenuName, new EventHandler(OnMFIOrgCSVImport));
			}
		}

		void OnMFIOrgCSVImport(object sender, EventArgs e)
		{
			using (MFIImportOrganisationsFromCSVForm importForm = new MFIImportOrganisationsFromCSVForm())
			{
				importForm.ShowDialog();
			}
		}

		internal const string orgCSVImportMenuName = "MFI Organisation CSV Files";
	}
}
