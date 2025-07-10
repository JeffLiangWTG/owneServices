using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportEntryCreationStrategy : EntryCreationStrategy
	{
		public ImportEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, MessageTypeList.Codes.CDI)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var mergeKey = base.GetKeyForLine(baseInvoiceLine);
			if (baseInvoiceLine != null && Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge)
			{
				var invoiceHeader = baseInvoiceLine.InvoiceHeader;
				var invoiceLineBR = baseInvoiceLine as JobComInvoiceLine;
				mergeKey.Add(invoiceHeader?.JZ_OH_Supplier ?? ZGuid.Empty);
				mergeKey.Add(invoiceHeader?.JZ_RX_NKInvoice_Currency ?? ZString.Empty);
				mergeKey.Add(invoiceHeader?.JZ_IncoTerm ?? ZString.Empty);
				mergeKey.Add(invoiceHeader?.JZ_ValuationCode ?? ZString.Empty);

				mergeKey.Add(invoiceLineBR.TariffDetachConcatenated);
				mergeKey.Add(invoiceLineBR.NVEConcatenated);
				mergeKey.Add(invoiceLineBR.NaladiHs);
				mergeKey.Add(invoiceLineBR.NaladiNcca);
				mergeKey.Add(invoiceLineBR.FMMBenefit);
			}
			return mergeKey;
		}

		protected override bool IsActiveCore => Declaration.IsImportOnly;
	}
}
