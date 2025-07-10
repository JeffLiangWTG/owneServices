using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business
{
	internal class ImportIPRInventorySelectionHeader : ImportInventorySelectionHeader
	{
		readonly CreateDeclarationBizObj createDeclarationBizObj;

		public ImportIPRInventorySelectionHeader(JobDeclaration declaration, CreateDeclarationBizObj createDeclarationBizObj) : base(declaration)
		{
			this.createDeclarationBizObj = createDeclarationBizObj;
		}

		protected override BaseJobComInvoiceHeader GetMatchingInvoiceHeaderAndPopulateData(InvoiceHeaderGroupingDefinitionProvider invoiceHeaderGroupingDefinitionProvider)
		{
			var invoiceHeader = Declaration.Invoices.FirstOrDefault(x =>
									x.JZ_InvoiceNumber == invoiceHeaderGroupingDefinitionProvider.InvoiceNumber
									&& x.JZ_InvoiceDate == invoiceHeaderGroupingDefinitionProvider.InvoiceDate)
								?? Declaration.Invoices.AddNew();

			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeaderGroupingDefinitionProvider.LinePriceCurrency;
			invoiceHeader.JZ_IncoTerm = invoiceHeaderGroupingDefinitionProvider.IncotermCode;
			invoiceHeader.JZ_IncoTermPlace = invoiceHeaderGroupingDefinitionProvider.IncotermPlace;
			invoiceHeader.JZ_ValuationCode = invoiceHeaderGroupingDefinitionProvider.TransNature;
			invoiceHeader.JZ_InvoiceNumber = invoiceHeaderGroupingDefinitionProvider.InvoiceNumber;
			invoiceHeader.JZ_InvoiceDate = invoiceHeaderGroupingDefinitionProvider.InvoiceDate;
			return invoiceHeader;
		}

		protected override void FillInvoiceLineWithInventoryDetails(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute,
			ZDecimal ratio, WhsInventoryWrapper inventoryWrapper = null)
		{
			base.FillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
			if (IsInwardProcessingAVABR)
			{
				invoiceLine.JI_LinePrice = whsBondedWarehouseAttribute.WB_ValueForDuty * ratio;
				(invoiceLine as JobComInvoiceLine).JI_NetPrice = invoiceLine.JI_LinePrice;
			}
		}

		protected override bool ShouldFillFinancialData(EU.Business.Declaration.JobComInvoiceLine invoiceLine) => !IsInwardProcessingAVABR && base.ShouldFillFinancialData(invoiceLine);

		bool IsInwardProcessingAVABR => createDeclarationBizObj.DeclarationType == ImportDeclarationTypeList.Codes.AVABR;
	}
}
