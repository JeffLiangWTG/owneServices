using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportEntryCreationStrategy : EntryCreationStrategy
	{
		public LocalExportEntryCreationStrategy(JobDeclaration declaration, string messageSubType)
			: base(declaration, messageSubType)
		{
		}

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		protected override bool IsActiveCore
		{
			get
			{
				return Declaration.IsLocalExport &&
					((ElectronicDocumentTypeList.Codes._5DP == CH_MessageTypeToNewEntryHeader && LocalExportTransactionNatureCodeList.Is5DP(Declaration.JE_MessageSubType))
					|| (ElectronicDocumentTypeList.Codes._5DQ == CH_MessageTypeToNewEntryHeader && LocalExportTransactionNatureCodeList.Is5DQ(Declaration.JE_MessageSubType)));
			}
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			var invoiceHeader = (JobComInvoiceHeader)invoiceLine.InvoiceHeader;
			if (invoiceHeader != null)
			{
				result.Add(invoiceHeader.JZ_OH_Supplier);
				result.Add(invoiceHeader.JZ_OH_Manufacturer);
				result.Add(invoiceHeader.JZ_OH_Buyer);
				result.Add(invoiceHeader.JZ_DRWApplicantType);
			}
			return result;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			MergeKey key = new MergeKey();
			key.Add(invoiceLine.PK);
			return key;
		}
	}
}
