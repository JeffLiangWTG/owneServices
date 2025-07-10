using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationGoodsShipmentProvider : IDeclarationGoodsShipment
	{
		public DeclarationGoodsShipmentProvider(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		readonly CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader => entryLine.RandomLine.InvoiceHeader;
		JobComInvoiceLine invoiceLine => entryLine.RandomLine;

		public IDeclarationOrganization Exporter => fExporter ?? (fExporter = new DeclarationOrganizationProvider(entryLine.Declaration.Supplier));
		IDeclarationOrganization fExporter;

		public IDeclarationOrganization Importer => fImporter ?? (fImporter = new DeclarationOrganizationProvider(invoiceHeader?.Buyer));
		IDeclarationOrganization fImporter;

		public IDeclarationGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => fGoodsAgencyItem ?? (fGoodsAgencyItem = new DeclarationGovernmentAgencyGoodsItemProvider(entryLine));
		IDeclarationGovernmentAgencyGoodsItem fGoodsAgencyItem;

		public IDeclarationNFeInvoice Invoice => fNfeInvoice ?? (fNfeInvoice = new DeclarationNFeInvoiceProvider(invoiceLine));
		IDeclarationNFeInvoice fNfeInvoice;

		public IEnumerable<IDeclarationNFeInvoice> ComplementaryLogisticInvoices => null;

		public string IncotermCode => invoiceHeader?.JZ_IncoTerm ?? string.Empty;
	}
}
