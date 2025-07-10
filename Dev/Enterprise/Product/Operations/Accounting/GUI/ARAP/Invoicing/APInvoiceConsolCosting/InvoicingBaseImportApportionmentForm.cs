using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoicingBaseImportApportionmentForm : ZChildForm
	{
		public InvoicingBaseImportApportionmentForm(InvoicingBaseConsolCostImporter costImporter)
			: base(costImporter)
		{
			this.CostImporter = costImporter;
		}

		readonly InvoicingBaseConsolCostImporter CostImporter;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				ConsolCostsToImportGrid.RemoveFromAvailableColumns(AutoJobConsolCost.Schema.E6_GB_CostTaxBranch);
			}
		}

		void CancelImportButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedCosts = ConsolCostsToImportGrid.SelectedElements;
			if (selectedCosts.Length > 0)
			{
				CostImporter.ImportCostsIntoCosting(selectedCosts);
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("b926bd6a-bd4b-4357-967b-34f8a2db6831", "Please select an item in the list to import, or press cancel to not import anything from this screen"), Res.GetString("178b4b82-7827-42ce-a635-cda019dd4e51", "Import Consol Cost"));
			}
		}
	}
}
