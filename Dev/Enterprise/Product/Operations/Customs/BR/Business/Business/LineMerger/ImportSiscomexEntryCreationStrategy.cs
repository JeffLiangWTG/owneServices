using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportSiscomexEntryCreationStrategy : EntryCreationStrategy
	{
		public ImportSiscomexEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, MessageTypeList.Codes.ISW)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var mergeKey = base.GetKeyForLine(baseInvoiceLine);
			if (baseInvoiceLine != null && Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge)
			{
				var invoiceHeader = baseInvoiceLine.InvoiceHeader as JobComInvoiceHeader;
				var invoiceLineBR = baseInvoiceLine as JobComInvoiceLine;
				mergeKey.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
				mergeKey.Add(invoiceHeader.JZ_IncoTerm);
				mergeKey.Add(invoiceHeader.JZ_ValuationCode);
				mergeKey.Add(invoiceHeader.JZ_OA_SupplierAddress);
				mergeKey.Add(invoiceHeader.ExchangeHedgeType);
				mergeKey.Add(invoiceHeader.ExchangeHedgeFinancialInstitution);
				mergeKey.Add(invoiceHeader.ExchangeHedgeReason);
				mergeKey.Add(invoiceHeader.ExchangeHedgeROFBACENNumber);
				mergeKey.Add(invoiceLineBR.TariffDetachConcatenated);
				mergeKey.Add(invoiceLineBR.NVEConcatenated);
				mergeKey.Add(invoiceLineBR.NaladiHs);
				mergeKey.Add(invoiceLineBR.NaladiNcca);
				mergeKey.Add(invoiceLineBR.MercosulForeignDeclarationType);
				mergeKey.Add(invoiceLineBR.JI_PrimaryPreference);
				mergeKey.Add(invoiceLineBR.EffectiveManufacturerAddressPK);
				mergeKey.Add(invoiceLineBR.ChargeTypesConcatenated);
				mergeKey.Add(invoiceLineBR.ICMSTaxRegime);
				mergeKey.Add(invoiceLineBR.ICMSLegalBase);
				mergeKey.Add(invoiceLineBR.JI_ICMSRate);
				mergeKey.Add(invoiceLineBR.JI_ICMSBaseValueReductionPercentage);
				mergeKey.Add(invoiceLineBR.JI_ICMSTotalAmountReductionPercentage);
				mergeKey.Add(invoiceLineBR.JI_ICMSFormula);
				mergeKey.Add(invoiceLineBR.DutyTaxRegime);
				mergeKey.Add(invoiceLineBR.DutyLegalBase);
				mergeKey.Add(invoiceLineBR.JI_TemporaryAdmissionReason);
				mergeKey.Add(invoiceLineBR.ImportLicenseType);
				mergeKey.Add(invoiceLineBR.ImportLicenseNumber);
				mergeKey.Add(invoiceLineBR.ImportLicenseAuthorizationDate);
				mergeKey.Add(invoiceLineBR.IPITaxRegime);
				mergeKey.Add(invoiceLineBR.JI_ComplementaryNote);
				mergeKey.Add(invoiceLineBR.PisCofinsTaxRegime);
				mergeKey.Add(invoiceLineBR.IPITaxBenefitLegalActType);
				mergeKey.Add(invoiceLineBR.IPITaxBenefitLegalActNumber);
				mergeKey.Add(invoiceLineBR.AntidumpingLegalActType);
				mergeKey.Add(invoiceLineBR.AntidumpingLegalActNumber);
				mergeKey.Add(invoiceLineBR.AdditionalTariffConcatenated);
				mergeKey.Add(invoiceLineBR.DutyRateIsOverridden);
				mergeKey.Add(invoiceLineBR.DutyVigentRateValue);
				mergeKey.Add(invoiceLineBR.JI_ManufacturerIndicator);
				mergeKey.Add(invoiceLineBR.JI_ManufacturerIndicator == ManufacturerIndicatorList.Codes._3 ? invoiceLineBR.JI_CountryOfOrigin : ZString.Empty);
				mergeKey.Add(invoiceLineBR.FMMBenefit);
				mergeKey.Add(invoiceLineBR.JI_GoodsApplication);
				mergeKey.Add(invoiceLineBR.JI_GoodsCondition);
				mergeKey.Add(invoiceLineBR.PreviousDocumentConcatenated);
				mergeKey.Add(invoiceLineBR.MercosulForeignDeclarationConcatenated);
				mergeKey.Add(invoiceLineBR.AttachedImportLicenseLine?.JI_CEI ?? ZGuid.Empty);
				mergeKey.Add(invoiceLineBR.ICMSFCPRateValue);
			}
			return mergeKey;
		}

		protected override bool IsActiveCore => Declaration.IsImportSiscomex;
	}
}
