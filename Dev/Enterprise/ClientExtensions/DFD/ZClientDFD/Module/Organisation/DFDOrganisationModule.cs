using System;
using System.Windows.Forms;
using Enterprise.Client.DFD.Business.Import;
using Enterprise.Client.DFD.GUI.Import;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DFD.Module
{
	public class DFDOrganisationModule : OrganisationModule
	{
		public DFDOrganisationModule()
			: base()
		{
		}

		protected override Control GetNewEmbeddedControl()
		{
			if (GlbCompany.CurrentCompany.Country.Code.Equals(Core.Constants.CountryCodes.UnitedStates))
			{
				AddImportDataMenuItem("DSV U.S. Organisation Data Update", (object sender, EventArgs e) =>
				{
					using (var importForm = new USDataImportForm("U.S. Organisation Data Update"))
					{
						importForm.Importer = new USOrganisationDataImporter();
						ZFormModaliser.ShowDialogWithoutDispose(importForm);
					}
				});
				AddImportDataMenuItem("DSV U.S. Supplier Data", (object sender, EventArgs e) =>
				{
					using (var importForm = new USDataImportForm("U.S. Supplier Data"))
					{
						importForm.Importer = new USSupplierDataImporter();
						ZFormModaliser.ShowDialogWithoutDispose(importForm);
					}
				});
			}
			return base.GetNewEmbeddedControl();
		}
	}
}
