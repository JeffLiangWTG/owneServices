using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportEntryLine : IImportEntryLine
	{
		public int EntryLineNo { get; set; }
		public string HSDescription { get; set; }
		public string ModelName { get; set; }
		public string BrandCode { get; set; }
		public string BrandName { get; set; }
		public string AdditionalTariffCode { get; set; }
		public string HSCode { get; set; }
		public string SpecificUseCodeDutyRatePermitNo { get; set; }
		public string CertificateOfOriginNo { get; set; }
		public string CertificateOfOriginCriteriaCode { get; set; }
		public DateTime CertificateOfOriginIssueDate { get; set; }
		public string CertificateOfOriginIssuingCountry { get; set; }
		public string CertificateOfOriginAgencyName { get; set; }
		public string CertificateOfOriginAreaName { get; set; }
		public string CertificateOfOriginPersonName { get; set; }
		public string CertificateOfOriginSplitIndicator { get; set; }
		public string CountryOfOrigin { get; set; }
		public string CountryOfOriginDeterminationRule { get; set; }
		public string CountryOfOriginLabelLocation { get; set; }
		public string CountryOfOriginLabelType { get; set; }
		public string CertificateOfOriginExemptionReason { get; set; }
		public string ProductOrMaterialCode { get; set; }
		public int MaterialLineNo { get; set; }
		public string QuantityUQToClaimRefund { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.QuantityToClaimRefund)]
		public decimal QuantityToClaimRefund { get; set; }
		public string MightRequireInspectionIndicator { get; set; }
		public ImportNonGADetail[] NonGADetails { get; set; }
		public string PostClearanceProcedureAgency1 { get; set; }
		public string PostClearanceProcedureAgency2 { get; set; }
		public string PostClearanceProcedureAgency3 { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Weight)]
		public decimal NetWeightInKG { get; set; }
		public string QuantityUnit { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		public decimal Quantity { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal CustomsValueKRW { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal CustomsValueUSD { get; set; }
		public ImportPreviousExpDecLine[] PreviousExpDecLines { get; set; }
		public string CourierCargoSelectivityIndicator { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyRate)]
		public decimal AdValoremDutyRate { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.SpecificDutyRate)]
		public decimal SpecificDutyRate { get; set; }
		public string DutyRateCode { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyRate)]
		public decimal AdditionalDutyRate { get; set; }
		public string AdditionalDutyCode { get; set; }
		public string DutyRateTypeCode { get; set; }
		public string DutyReductionClassification { get; set; }
		public string DutyReductionOrInstallmentCode { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyRate)]
		public decimal DutyReductionRate { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal DutyReductionAmount { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal DutyAmount { get; set; }
		public string DomesticTaxClassification { get; set; }
		public string ExemptionCodeOfLiquorTax { get; set; }
		public string ExemptionCodeOfTransportationTax { get; set; }
		public string DomesticTaxCode { get; set; }
		public string ExemptionCodeOfSpecialConsumptionTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyRate)]
		public decimal DomesticTaxRate { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal SpecialConsumptionTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal LiquorTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TransportationTax { get; set; }
		public string VATRateCode { get; set; }
		public string VATReductionCode { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal ValueForVAT { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal ValueExemptForVAT { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal VATAmount { get; set; }
		public string EducationTaxExemptIndicator { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal EducationTaxAmount { get; set; }
		public string AgricultureTaxClassification { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal AgricultureTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.QtyOrPrice)]
		public decimal DomesticTaxBaseQtyOrPrice { get; set; }
		public ImportInvoiceLine[] InvoiceLines { get; set; }

		public ImportImmediateDelivery[] ImmediateDeliveries { get; set; }

		ZInt IEntryLine.EntryLineNo => EntryLineNo;
		ZInt IImportEntryLine.EntryLineNo => EntryLineNo;
		ZString IImportEntryLine.HSDescription => HSDescription;
		ZString IImportEntryLine.ModelName => ModelName;
		ZString IImportEntryLine.BrandCode => BrandCode;
		ZString IImportEntryLine.BrandName => BrandName;
		ZString IImportEntryLine.AdditionalTariffCode => AdditionalTariffCode;
		ZString IImportEntryLine.HSCode => HSCode;
		ZString IImportEntryLine.SpecificUseCodeDutyRatePermitNo => SpecificUseCodeDutyRatePermitNo;
		ZString IImportEntryLine.CertificateOfOriginNo => CertificateOfOriginNo;
		ZString IImportEntryLine.CertificateOfOriginCriteriaCode => CertificateOfOriginCriteriaCode;
		ZDate IImportEntryLine.CertificateOfOriginIssueDate => (ZDate)CertificateOfOriginIssueDate;
		ZString IImportEntryLine.CertificateOfOriginIssuingCountry => CertificateOfOriginIssuingCountry;
		ZString IImportEntryLine.CertificateOfOriginAgencyName => CertificateOfOriginAgencyName;
		ZString IImportEntryLine.CertificateOfOriginAreaName => CertificateOfOriginAreaName;
		ZString IImportEntryLine.CertificateOfOriginPersonName => CertificateOfOriginPersonName;
		ZString IImportEntryLine.CertificateOfOriginSplitIndicator => CertificateOfOriginSplitIndicator;
		ZString IImportEntryLine.CountryOfOrigin => CountryOfOrigin;
		ZString IImportEntryLine.CountryOfOriginDeterminationRule => CountryOfOriginDeterminationRule;
		ZString IImportEntryLine.CountryOfOriginLabelLocation => CountryOfOriginLabelLocation;
		ZString IImportEntryLine.CountryOfOriginLabelType => CountryOfOriginLabelType;
		ZString IImportEntryLine.CertificateOfOriginExemptionReason => CertificateOfOriginExemptionReason;
		ZString IImportEntryLine.ProductOrMaterialCode => ProductOrMaterialCode;
		ZInt IImportEntryLine.MaterialLineNo => MaterialLineNo;
		ZString IImportEntryLine.QuantityUQToClaimRefund => QuantityUQToClaimRefund;
		ZDecimal IImportEntryLine.QuantityToClaimRefund => QuantityToClaimRefund;
		ZString IImportEntryLine.MightRequireInspectionIndicator => MightRequireInspectionIndicator;
		IEnumerable<IImportNonGADetail> IImportEntryLine.NonGADetails => NonGADetails;
		ZString IImportEntryLine.PostClearanceProcedureAgency1 => PostClearanceProcedureAgency1;
		ZString IImportEntryLine.PostClearanceProcedureAgency2 => PostClearanceProcedureAgency2;
		ZString IImportEntryLine.PostClearanceProcedureAgency3 => PostClearanceProcedureAgency3;
		ZDecimal IImportEntryLine.NetWeightInKG => NetWeightInKG;
		ZString IImportEntryLine.QuantityUnit => QuantityUnit;
		ZDecimal IImportEntryLine.Quantity => Quantity;
		ZDecimal IImportEntryLine.CustomsValueKRW => CustomsValueKRW;
		ZDecimal IImportEntryLine.CustomsValueUSD => CustomsValueUSD;
		IEnumerable<IImportPreviousExpDecLine> IImportEntryLine.PreviousExpDecLines => PreviousExpDecLines;
		ZString IImportEntryLine.CourierCargoSelectivityIndicator => CourierCargoSelectivityIndicator;
		ZDecimal IImportEntryLine.AdValoremDutyRate => AdValoremDutyRate;
		ZDecimal IImportEntryLine.SpecificDutyRate => SpecificDutyRate;
		ZString IImportEntryLine.DutyRateCode => DutyRateCode;
		ZDecimal IImportEntryLine.AdditionalDutyRate => AdditionalDutyRate;
		ZString IImportEntryLine.AdditionalDutyCode => AdditionalDutyCode;
		ZString IImportEntryLine.DutyRateTypeCode => DutyRateTypeCode;
		ZString IImportEntryLine.DutyReductionClassification => DutyReductionClassification;
		ZString IImportEntryLine.DutyReductionOrInstallmentCode => DutyReductionOrInstallmentCode;
		ZDecimal IImportEntryLine.DutyReductionRate => DutyReductionRate;
		ZDecimal IImportEntryLine.DutyReductionAmount => DutyReductionAmount;
		ZDecimal IImportEntryLine.DutyAmount => DutyAmount;
		ZString IImportEntryLine.DomesticTaxClassification => DomesticTaxClassification;
		ZString IImportEntryLine.ExemptionCodeOfLiquorTax => ExemptionCodeOfLiquorTax;
		ZString IImportEntryLine.ExemptionCodeOfTransportationTax => ExemptionCodeOfTransportationTax;
		ZString IImportEntryLine.DomesticTaxCode => DomesticTaxCode;
		ZString IImportEntryLine.ExemptionCodeOfSpecialConsumptionTax => ExemptionCodeOfSpecialConsumptionTax;
		ZDecimal IImportEntryLine.DomesticTaxRate => DomesticTaxRate;
		ZDecimal IImportEntryLine.SpecialConsumptionTax => SpecialConsumptionTax;
		ZDecimal IImportEntryLine.LiquorTax => LiquorTax;
		ZDecimal IImportEntryLine.TransportationTax => TransportationTax;
		ZString IImportEntryLine.VATRateCode => VATRateCode;
		ZString IImportEntryLine.VATReductionCode => VATReductionCode;
		ZDecimal IImportEntryLine.ValueForVAT => ValueForVAT;
		ZDecimal IImportEntryLine.ValueExemptForVAT => ValueExemptForVAT;
		ZDecimal IImportEntryLine.VATAmount => VATAmount;
		ZString IImportEntryLine.EducationTaxExemptIndicator => EducationTaxExemptIndicator;
		ZDecimal IImportEntryLine.EducationTaxAmount => EducationTaxAmount;
		ZString IImportEntryLine.AgricultureTaxClassification => AgricultureTaxClassification;
		ZDecimal IImportEntryLine.AgricultureTax => AgricultureTax;
		ZDecimal IImportEntryLine.DomesticTaxBaseQtyOrPrice => DomesticTaxBaseQtyOrPrice;
		IEnumerable<IImportInvoiceLine> IImportEntryLine.InvoiceLines => InvoiceLines;
		IEnumerable<IInvoiceLine> IEntryLine.InvoiceLines => InvoiceLines;
		IEnumerable<IImportImmediateDelivery> IImportEntryLine.ImmediateDeliveries => ImmediateDeliveries;
	}
}
