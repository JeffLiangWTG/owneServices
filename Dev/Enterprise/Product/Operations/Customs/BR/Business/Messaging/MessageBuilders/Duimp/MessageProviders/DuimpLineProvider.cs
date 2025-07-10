using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class DuimpLineProvider : IDuimpLine
	{
		DuimpLineProvider(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			declaration = Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
			invoiceLine = Argument.NotNull(entryLine.RandomLine, nameof(entryLine.RandomLine));
			invoiceHeader = Argument.NotNull(invoiceLine.InvoiceHeader, nameof(invoiceLine.InvoiceHeader));
			invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
		}
		readonly CusEntryLine entryLine;
		readonly JobDeclaration declaration;
		readonly JobComInvoiceLine invoiceLine;
		readonly JobComInvoiceHeader invoiceHeader;
		readonly IEnumerable<JobComInvoiceLine> invoiceLines;

		public static DuimpLineProvider New(CusEntryLine entryLine) => entryLine == null ? null : new DuimpLineProvider(entryLine);

		public int ItemNumber => entryLine.CL_LineNumber;

		public string AcquirerIndicator => TypeOfOperationImportList.MapToCustomsCode(declaration.OperationType);

		public string AcquirerRegistrationNumber => declaration.IntermConsignee?.GetCNPJOrCPF();

		public IEnumerable<IMercosulCertificate> MercosulCertificates => fMercosulCertificates ??=
			invoiceLines.SelectMany(x => x.MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>())
				.Select(mercosulDoc => MercosulCertificateProvider.New(mercosulDoc)).ToArray();
		IMercosulCertificate[] fMercosulCertificates;

		public IEnumerable<IAttributeItem> Attributes => fAttributes ??=
			invoiceLines.SelectMany(x => x.Attributes.GetEffectiveAttributes()).Select(x => AttributeProvider.New(x)).ToArray();
		IAttributeItem[] fAttributes;

		public IEnumerable<ILinkedDocument> LinkedDocuments => fLinkedDocuments ??=
			invoiceLines.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>()).Select(x => LinkedDocumentProvider.New(x)).ToArray();
		ILinkedDocument[] fLinkedDocuments;

		public IExchangeHedge ExchangeHedge => fExchangeHedge ??= ExchangeHedgeProvider.New(invoiceHeader);
		IExchangeHedge fExchangeHedge;

		public IEnumerable<ILpco> Lpcos => fLcpos ??=
			invoiceLines.SelectMany(x => x.Permits.Cast<Permit>()).Select(x => LpcoProvider.New(x)).ToArray();
		ILpco[] fLcpos;

		public int ProductCode => ZInt.ParseSafe(invoiceLine.JI_CatalogAuthorityIdentifier, ZInt.Zero);

		public string ProductVersion => invoiceLine.JI_CatalogAuthorityVersion;

		public string ProductRootCNPJ => invoiceLine.GoodsCatalog?.Owner?.GetRootCNPJFromCNPJ() ?? ZString.Empty;

		public string ManufacturerIndicatorCode => ManufacturerIndicatorList.MapToCustomsCode(invoiceLine.JI_ManufacturerIndicator);

		public string BuyerSellerIndicatorCode => RelatedIndicatorList.MapToCustomsCodeDuimp(invoiceHeader.JZ_RelatedIndicator);

		public IMerchandise Merchandise => fMerchandise ??= MerchandiseProvider.New(invoiceLines);
		IMerchandise fMerchandise;

		public ISellingCondition SellingCondition => fSellingCondition ??= SellingConditionProvider.New(invoiceHeader, entryLine);
		ISellingCondition fSellingCondition;

		public IExporter Exporter => fExporter ??= ExporterProvider.New(invoiceHeader);
		IExporter fExporter;

		public IManufacturer Manufacturer => fManufacturer ??= ManufacturerProvider.New(invoiceLine);
		IManufacturer fManufacturer;

		public IEnumerable<IAttributeItem> LegalBasisAttributes => null;

		public IEnumerable<ITaxItem> Taxes => fTaxes ??= invoiceLines.SelectMany(x => x.DuimpTaxRegimes.Select(TaxItemProvider.New)).ToArray();
		ITaxItem[] fTaxes;
	}
}
