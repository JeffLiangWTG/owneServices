using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class ImportedInvoiceXMLControl : ZUserControl
	{
		public ImportedInvoiceXMLControl()
		{
			InitializeComponent();
		}

		public void HideNumberOfDocuments()
		{
			this.xmlNumberOfDocsCalcEdit.Visible = false;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				if (!GlbCompany.CurrentCompany.GC_IsWHTRegistered)
				{
					xmlLinesGrid.RemoveFromAvailableColumns("WithholdingTaxID");
					xmlLinesGrid.RemoveFromAvailableColumns("OSWHTAmount");
					xmlLinesGrid.RemoveFromAvailableColumns("LocalWHTAmount");
				}

				if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
				{
					placeOfSupplyTextBox.Visible = false;
					xmlLinesGrid.RemoveFromAvailableColumns("PlaceOfSupply");
				}
			}
		}
	}
}
