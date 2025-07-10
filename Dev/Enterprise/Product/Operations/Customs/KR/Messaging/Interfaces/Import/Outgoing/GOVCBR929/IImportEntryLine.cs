using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportEntryLine : IEntryLine
	{
		[ID()]
		new ZInt EntryLineNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B101)]
		ZString HSDescription { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B102)]
		ZString ModelName { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B103)]
		ZString BrandCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B104)]
		ZString BrandName { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B106, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString AdditionalTariffCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B105, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString HSCode { get; }
		ZString SpecificUseCodeDutyRatePermitNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F101)]
		ZString CertificateOfOriginNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F107)]
		ZString CertificateOfOriginCriteriaCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F104)]
		ZDate CertificateOfOriginIssueDate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F102)]
		ZString CertificateOfOriginIssuingCountry { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F103)]
		ZString CertificateOfOriginAgencyName { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F105)]
		ZString CertificateOfOriginAreaName { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F106)]
		ZString CertificateOfOriginPersonName { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.F108)]
		ZString CertificateOfOriginSplitIndicator { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B201, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString CountryOfOrigin { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B204)]
		ZString CountryOfOriginDeterminationRule { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B202)]
		ZString CountryOfOriginLabelLocation { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B203)]
		ZString CountryOfOriginLabelType { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B205)]
		ZString CertificateOfOriginExemptionReason { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B309, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString ProductOrMaterialCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B310)]
		ZInt MaterialLineNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B308, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString QuantityUQToClaimRefund { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B307, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZDecimal QuantityToClaimRefund { get; }
		ZString MightRequireInspectionIndicator { get; }
		IEnumerable<IImportNonGADetail> NonGADetails { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B107)]
		ZString PostClearanceProcedureAgency1 { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B108)]
		ZString PostClearanceProcedureAgency2 { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B109)]
		ZString PostClearanceProcedureAgency3 { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B303, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZDecimal NetWeightInKG { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B306, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString QuantityUnit { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B305, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZDecimal Quantity { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B301, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal CustomsValueKRW { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B302, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal CustomsValueUSD { get; }
		IEnumerable<IImportPreviousExpDecLine> PreviousExpDecLines { get; }
		ZString CourierCargoSelectivityIndicator { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B403, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal AdValoremDutyRate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B405, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal SpecificDutyRate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B402, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString DutyRateCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B412, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal AdditionalDutyRate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B411, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString AdditionalDutyCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B404, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString DutyRateTypeCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B406, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString DutyReductionClassification { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B407, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString DutyReductionOrInstallmentCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B408, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal DutyReductionRate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B409, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal DutyReductionAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B401, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal DutyAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B504, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString DomesticTaxClassification { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B507, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString ExemptionCodeOfLiquorTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B507, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString ExemptionCodeOfTransportationTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B505, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString DomesticTaxCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B507, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString ExemptionCodeOfSpecialConsumptionTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B506, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal DomesticTaxRate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B501, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal SpecialConsumptionTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B503, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal LiquorTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B502, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal TransportationTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B802, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString VATRateCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B803, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString VATReductionCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B804, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal ValueForVAT { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B805, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal ValueExemptForVAT { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B801, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal VATAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B602, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString EducationTaxExemptIndicator { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B601, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal EducationTaxAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B702, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString AgricultureTaxClassification { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B701, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal AgricultureTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.B410, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal DomesticTaxBaseQtyOrPrice { get; }
		new IEnumerable<IImportInvoiceLine> InvoiceLines { get; }
		IEnumerable<IImportImmediateDelivery> ImmediateDeliveries { get; }
	}
}
