using System;
using CargoWise.EntityFramework;
using Enterprise.Client.KNA.GUI;

namespace Enterprise.Client.KNA.Module
{
	public class KNAOrgSupplierPartModuleOverride : Customs.AU.Module.OrgSupplierPartModule, IDisplayResultsQuery
	{
		public KNAOrgSupplierPartModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("from KNA CSV file", ImportFromKNACSV, false);
			AddExportDataMenuItem("to KNA CSV file", ExportToKNACSV);
		}

		internal void ImportFromKNACSV(object sender, EventArgs e)
		{
			new KNAImportProductsFromCSVForm().Show();
		}

		internal void ExportToKNACSV(object sender, EventArgs e)
		{
			new KNAExportProductsToCSVForm(this).Show();
		}

		public ZQuery GetDisplayQuery()
		{
			return GetDisplayResultsQuery();
		}
	}
}
