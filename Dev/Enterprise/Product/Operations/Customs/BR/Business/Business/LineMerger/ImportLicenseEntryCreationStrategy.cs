using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseEntryCreationStrategy : EntryCreationStrategy
	{
		public ImportLicenseEntryCreationStrategy(JobDeclaration declaration, bool onlyForGetMergeKey = false)
			: base(declaration, MessageTypeList.Codes.LIC)
		{
			this.onlyForGetMergeKey = onlyForGetMergeKey;
		}
		readonly bool onlyForGetMergeKey;

		protected override bool IsActiveCore => Declaration.IsImportLicense;

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceHeader = baseInvoiceLine.InvoiceHeader as JobComInvoiceHeader;
			var invoiceLine = baseInvoiceLine as JobComInvoiceLine;
			var mergeKey = base.GetKeyForLine(baseInvoiceLine);

			if (baseInvoiceLine != null && Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge)
			{
				mergeKey.Add(invoiceHeader.JZ_IncoTerm);
				mergeKey.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
				mergeKey.Add(invoiceLine.NaladiHs);
				mergeKey.Add(invoiceLine.JI_UsedMaterialRegime);
				mergeKey.Add(invoiceLine.JI_UsedMaterialOperationType);
				mergeKey.Add(invoiceLine.DrawbackModality);
				mergeKey.Add(invoiceLine.DrawbackCANumber);
				mergeKey.Add(invoiceLine.TariffDetachConcatenated);
				mergeKey.Add(invoiceLine.ConsentingProcessConcatenated);
				mergeKey.Add(invoiceLine.EffectiveManufacturerAddressPK);
				mergeKey.Add(invoiceHeader.JZ_OA_SupplierAddress);
				mergeKey.Add(invoiceHeader.ExchangeHedgeType);
				mergeKey.Add(invoiceHeader.ExchangeHedgePaymentMethod);
				mergeKey.Add(invoiceHeader.ExchangeHedgeReason);
				mergeKey.Add(invoiceHeader.ExchangeHedgePaymentDeadline);
				mergeKey.Add(invoiceHeader.ExchangeHedgeFinancialInstitution);
				mergeKey.Add(invoiceLine.DutyTaxRegime);
				mergeKey.Add(invoiceLine.DutyLegalBase);
				mergeKey.Add(invoiceLine.JI_ManufacturerIndicator);
				mergeKey.Add(invoiceLine.JI_ManufacturerIndicator == ManufacturerIndicatorList.Codes._3 ? invoiceLine.JI_CountryOfOrigin : ZString.Empty);
				mergeKey.Add(invoiceLine.JI_SecondaryPreference);
			}
			if (!onlyForGetMergeKey)
			{
				invoiceLine.LastMergeKeyForImportLicenseEntry = mergeKey;
			}
			return mergeKey;
		}
	}
}
