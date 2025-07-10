using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportEntryCreationStrategy : EntryCreationStrategy
	{
		public ImportEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, SharedJobMessageTypeList.Codes.Import)
		{
		}

		protected override bool IsActiveCore => Declaration.IsImport;

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			var invoiceHeader = (JobComInvoiceHeader)invoiceLine.InvoiceHeader;
			if (invoiceHeader != null)
			{
				var valuationDecAttached = invoiceHeader.JZ_ValuationDecAttachCode;
				IZType resultValuation;
				switch (valuationDecAttached)
				{
					case ValueDeclarationAttachedCodeList.Codes.Y:
						resultValuation = invoiceHeader.PK;
						break;
					case ValueDeclarationAttachedCodeList.Codes.N:
						resultValuation = new ZString("N");
						break;
					case ValueDeclarationAttachedCodeList.Codes.P:
						resultValuation = new ZString("P" + invoiceHeader.JZ_BlanketValuationDeclarationNumber);
						break;
					default:
						resultValuation = ZGuid.Empty;
						break;
				}
				result.Add(resultValuation);

				result.Add(invoiceHeader.JZ_CU_RelatedHouseBill);
				result.Add(invoiceHeader.JZ_OA_ManufacturerAddress);
				result.Add(invoiceHeader.JZ_IncoTerm);
				result.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
				result.Add(invoiceHeader.JZ_PaymentTerms);
				result.Add(invoiceHeader.JZ_OH_Supplier);
				result.Add(invoiceHeader.JZ_OA_DistributorAddress);
				result.Add(invoiceHeader.JZ_OA_SellerAddress);
				result.Add(invoiceHeader.JZ_OA_ShipperAddress);
				result.Add(invoiceHeader.JZ_OH_SellingAgent);
				result.Add(invoiceHeader.JZ_ValuationCode);

				result.Add(invoiceHeader.JZ_ValuationDecAttachCode);
				result.Add(invoiceHeader.JZ_BlanketValuationDeclarationNumber);
				result.Add(invoiceHeader.JZ_ImportCargoManagementNumber);
				result.Add(invoiceHeader.JZ_OnlineTradeType);
				result.Add(invoiceHeader.JZ_COOStatus);
				if (!invoiceHeader.JZ_Remarks.IsEmpty)
				{
					result.Add(invoiceHeader.PK);
				}
				else
				{
					result.Add(ZGuid.Empty);
				}
			}
			return result;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var result = base.GetKeyForLine(invoiceLine);
			result.Add(invoiceLine.JI_BrandName);
			result.Add(invoiceLine.JI_Model);
			result.Add(invoiceLine.JI_CountryOfOrigin);
			result.Add(invoiceLine.JI_ParentLine);
			result.Add(invoiceLine.JI_PrimaryPreference);
			result.Add(invoiceLine.JI_ZZF_NKTaxType);
			result.Add(invoiceLine.JI_SecondaryPreference);
			result.Add(invoiceLine.AdditionalTariffCode);

			result.Add(invoiceLine.JI_BrandCode);
			result.Add(invoiceLine.JI_COOLabelLocation);
			result.Add(invoiceLine.JI_COOLabelType);
			result.Add(invoiceLine.JI_COOExemptionReason);
			result.Add(invoiceLine.JI_ProductTypeCode);
			result.Add(invoiceLine.JI_MightRequireInspection);
			result.Add(invoiceLine.JI_PostClearanceProcedureGA1);
			result.Add(invoiceLine.JI_PostClearanceProcedureGA2);
			result.Add(invoiceLine.JI_PostClearanceProcedureGA3);
			result.Add(invoiceLine.JI_CourierCargoSelectivityIndicator);
			result.Add(invoiceLine.JI_AdditionalDutyRate);
			result.Add(invoiceLine.JI_AdditionalDutyType);
			result.Add(invoiceLine.JI_DomesticTaxCode);
			result.Add(invoiceLine.JI_DomesticTaxExemptionCode);
			result.Add(invoiceLine.JI_DrawbackUQ);
			result.Add(invoiceLine.JI_VATReductionCode);
			result.Add(invoiceLine.JI_InstallmentCode);
			result.Add(invoiceLine.JI_IsSpecificUseCode);
			result.Add(invoiceLine.JI_UseCode);
			result.Add(invoiceLine.JI_SpecificUseCodeDutyRatePermitNo);
			result.Add(invoiceLine.JI_PCProcedure);

			result.Add(invoiceLine.CertificateOfOriginNo);
			result.Add(invoiceLine.CertificateOfOriginPersonName);
			result.Add(invoiceLine.CertificateOfOriginCriteriaCode);
			result.Add(invoiceLine.CertificateOfOriginIssueDate);
			result.Add(invoiceLine.CertificateOfOriginIssuingCountry);
			result.Add(invoiceLine.CertificateOfOriginAgencyName);
			result.Add(invoiceLine.CertificateOfOriginAreaName);
			result.Add(invoiceLine.CertificateOfOriginStatus);
			result.Add(invoiceLine.CriteriaForDeterminingCountryOfOrigin);
			return result;
		}

		protected override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			if (invoiceLine.JI_CL != entryLine.PK)
			{
				((CusEntryLine)entryLine).CL_FTASequenceNumber = 0;
			}
			return base.LinkInvoiceLineEntryLineAndReturnPivotIfUsed(entryLine, baseInvoiceLine);
		}
	}
}
