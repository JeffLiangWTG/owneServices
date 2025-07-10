using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business
{
	public static class KoreaSouthEInvoicingHelper
	{
		public static ZString GetIssueIDFromInvoice(InvoicingBase invoicingBase)
		{
			return invoicingBase.GetTransactionHeaderReferenceToValidateMissingRegistrationNumber(AccTransactionHeaderReferenceTypes.KRI)?.AH1_Reference ?? ZString.Empty;
		}

		public static string GetTaxInvoiceDocumentTypeCode(string complianceSubType, IEnumerable<ZString> taxTypes, Func<ZBool> isAmendment, ZDecimal? localVATAmount)
		{
			if (!string.IsNullOrEmpty(complianceSubType) && FeatureControlHelper.IsKoreaSouthComplianceSubTypeFeatureEnabled)
			{
				return "0" + complianceSubType;
			}

			var mapping = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Value.Cast<CodeDescriptionWithGroup>();
			var typeCodeForNullTaxRateType = mapping.Single(x => x.Code == AccountingMasterFilesConstants.NullTaxRateType.Code).Group;
			var taxTypesForInvoice = mapping.Where(x => x.Group == AccountingMasterFilesConstants.KoreaEInvoicingTypeCodeCategory.Invoice).Select(x => x.Code);
			var taxTypesForTaxInvoice = mapping.Where(x => x.Group == AccountingMasterFilesConstants.KoreaEInvoicingTypeCodeCategory.TaxInvoice).Select(x => x.Code);

			if (ShouldSendAsNonTaxInvoice() || ShouldSendAsInvoice())
			{
				return isAmendment() ? "0401" : "0301";
			}

			if (ShouldSendAsTaxInvoice())
			{
				return localVATAmount?.IsEmpty ?? true
					? isAmendment() ? "0202" : "0102"
					: isAmendment() ? "0201" : "0101";
			}

			return string.Empty;

			bool ShouldSendAsNonTaxInvoice() => taxTypes.IsNullOrEmpty() && typeCodeForNullTaxRateType == AccountingMasterFilesConstants.KoreaEInvoicingTypeCodeCategory.Invoice;

			bool ShouldSendAsInvoice() => taxTypes.Any() && taxTypes.Except(taxTypesForInvoice).IsNullOrEmpty();

			bool ShouldSendAsTaxInvoice() => taxTypes.Intersect(taxTypesForTaxInvoice).Any();
		}
	}
}
