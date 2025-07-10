using System;
using Enterprise.Customs.AU.Module;

namespace Enterprise.Client.AUS.Products
{
	public class AUSOrgSupplierPartModule : OrgSupplierPartModule
	{
		public AUSOrgSupplierPartModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("From &Austin csv-file", ImportFromAustinCSV, false);
			AddExportDataMenuItem("To &Austin csv-file", ExportToAustinCSV);
		}

		#region Import from Austin CSV

		public void ImportFromAustinCSV(object sender, EventArgs e)
		{
			AUSImportProductBusinessObject importObj = new AUSImportProductBusinessObject(Factory);
			new AUSImportProductsFromCSVForm(importObj).Show();
		}

		#endregion

		#region Export to Austin CSV

		public void ExportToAustinCSV(object sender, EventArgs e)
		{
			new ExportProductsToCSVForm().Show();
		}

		#endregion
	}
}
