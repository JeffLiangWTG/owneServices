using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business
{
	public class WarehouseAdjustmentInventorySelectionHeader : InventorySelectionHeader
	{
		public WarehouseAdjustmentInventorySelectionHeader(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override BaseJobComInvoiceHeader GetMatchingInvoiceHeaderAndPopulateData(InvoiceHeaderGroupingDefinitionProvider invoiceHeaderGroupingDefinitionProvider)
		{
			var invoiceHeader = Declaration.Invoices.FirstOrDefault() ?? Declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			return invoiceHeader;
		}

		protected override void SetFormattedProcedure(JobComInvoiceLine invoiceLine, IWhsDocketLine receiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
		}

		protected override void SetDefaultCustomsProcedureCode(BaseJobComInvoiceLine invoiceLine)
		{
		}

		protected override void FillCharges(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			// no charges for warehouse adjustments
		}
	}
}
