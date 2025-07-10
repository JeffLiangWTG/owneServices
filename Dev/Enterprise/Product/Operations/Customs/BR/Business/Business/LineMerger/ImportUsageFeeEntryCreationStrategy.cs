using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportUsageFeeEntryCreationStrategy : EntryCreationStrategy
	{
		public ImportUsageFeeEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, MessageTypeList.Codes.SUF, true)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var mergeKey = new MergeKey();
			if (baseInvoiceLine != null)
			{
				var invoiceHeader = baseInvoiceLine.InvoiceHeader as JobComInvoiceHeader;
				var invoiceLine = baseInvoiceLine as JobComInvoiceLine;
				mergeKey.Add(invoiceHeader.JZ_IncoTerm);
				mergeKey.Add(invoiceHeader.JZ_ValuationCode);
				mergeKey.Add(invoiceHeader.JZ_OA_SupplierAddress);
				mergeKey.Add(invoiceHeader.ExchangeHedgeType);
				mergeKey.Add(invoiceLine.JI_Tariff);
				mergeKey.Add(invoiceLine.JI_OA_ManufacturerAddress);
				mergeKey.Add(invoiceLine.JI_GoodsApplication);
				mergeKey.Add(invoiceLine.JI_GoodsCondition);
			}
			return GetKeyForHeader(baseInvoiceLine) + mergeKey;
		}

		protected override bool IsValuationDatePartOfMergeKey => false;

		protected override bool IsActiveCore => Declaration.IsImportOnly;
	}
}
