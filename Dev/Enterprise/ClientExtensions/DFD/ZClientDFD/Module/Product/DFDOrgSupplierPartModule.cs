using System;
using Enterprise.Client.DFD.Business.Import;
using Enterprise.Client.DFD.GUI.Import;
using Enterprise.Customs.US.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DFD.Module
{
	class DFDOrgSupplierPartModule : OrgSupplierPartModule
	{
		protected override void AddExtraImportExportMenuItems()
		{
			base.AddExtraImportExportMenuItems();

			AddImportDataMenuItem("DSV U.S. Product Data", (object sender, EventArgs e) =>
			{
				using (var importForm = new USDataImportForm("U.S. Product Data Import"))
				{
					importForm.Importer = new USProductDataImporter();
					ZFormModaliser.ShowDialogWithoutDispose(importForm);
				}
			}, false);
		}
	}
}
