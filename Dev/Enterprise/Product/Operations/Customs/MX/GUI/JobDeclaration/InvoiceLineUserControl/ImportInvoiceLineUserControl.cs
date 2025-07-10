using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => true;

		protected override IEnumerable<string> GetDefaultColumnsInOrderCore() => new List<string>
		{
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber,
			JobComInvoiceLine.Schema.JI_Calc_Invoice,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_CC,
			JobComInvoiceLine.Schema.JI_InvoiceQuantity,
			JobComInvoiceLine.Schema.JI_InvoiceUQ,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_LinePrice,
			JobComInvoiceLine.Schema.JI_Description,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
			JobComInvoiceLine.Schema.JI_Weight,
			JobComInvoiceLine.Schema.JI_WeightUQ,
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_Volume,
			JobComInvoiceLine.Schema.JI_VolumeUQ,
			JobComInvoiceLine.Schema.JI_OrderNumber,
			JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
			JobComInvoiceLine.Schema.UnitPrice,
			JobComInvoiceLine.Schema.Observations,
			JobComInvoiceLine.Schema.JI_CustomAttrib1,
			JobComInvoiceLine.Schema.JI_CustomAttrib2,
			JobComInvoiceLine.Schema.JI_CustomAttrib3,
			JobComInvoiceLine.Schema.JI_CustomAttrib4,
			JobComInvoiceLine.Schema.JI_CustomAttrib5,
			JobComInvoiceLine.Schema.JI_CustomAttrib6,
			JobComInvoiceLine.Schema.JI_CustomTextBlob1,
			JobComInvoiceLine.Schema.JI_PartAttrib1,
			JobComInvoiceLine.Schema.JI_PartAttrib2,
			JobComInvoiceLine.Schema.JI_PartAttrib3,
			JobComInvoiceLine.Schema.JI_SerialNumber,
			JobComInvoiceLine.Schema.JI_CEI,
		};
	}
}
