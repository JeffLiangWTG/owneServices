using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("Import5FEHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5FEHeader : IImport5FEHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public DateTime DeclarationDate { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public string DeclarantType { get; set; }
		public Organisation Declarant { get; set; }
		public Organisation Payer { get; set; }
		public int TotalAmendedItemCount { get; set; }
		public int TotalAmendedTaxCount { get; set; }
		public string DomesticTaxPenaltyType { get; set; }
		public string DutyPenaltyReducedYN { get; set; }
		public string RefundRequestNumber { get; set; }
		public Import5FEItem[] AmendedItems { get; set; }
		public Import5FETaxItem[] TaxItems { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public decimal BeforeTotalDutyTaxAmount { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public decimal AfterTotalDutyTaxAmount { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public decimal DutyTaxDifference { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public decimal BeforeCustomsValue { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public decimal AfterCustomsValue { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public decimal CustomsValueDifference { get; set; }
		public string DutyPenaltyType { get; set; }
		public string PenaltyExemptionIndicator { get; set; }
		public string PenaltyExemptionReasonCode {  get; set; }
		public string PenaltyExemptionReason { get; set; }
		public int PenaltyExemptionReqSequence { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public decimal PenaltyExemptionAmount { get; set; }

		ZString IImport5FEHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZDate IImport5FEHeader.DeclarationDate => new ZDate(DeclarationDate);
		ZString IImport5FEHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport5FEHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		ZString IImport5FEHeader.DeclarantType => DeclarantType;
		IOrganization IImport5FEHeader.Declarant => Declarant;
		IOrganization IImport5FEHeader.Payer => Payer;
		ZInt IImport5FEHeader.TotalAmendedItemCount => TotalAmendedItemCount;
		ZInt IImport5FEHeader.TotalAmendedTaxCount => TotalAmendedTaxCount;
		ZString IImport5FEHeader.DomesticTaxPenaltyType => DomesticTaxPenaltyType;
		ZString IImport5FEHeader.DutyPenaltyReducedYN => DutyPenaltyReducedYN;
		ZString IImport5FEHeader.RefundRequestNumber => RefundRequestNumber;
		IEnumerable<IImport5FEItem> IImport5FEHeader.AmendedItems => AmendedItems;
		IEnumerable<IImport5FETaxItem> IImport5FEHeader.TaxItems => TaxItems;
		ZDecimal IImport5FEHeader.BeforeTotalDutyTaxAmount => BeforeTotalDutyTaxAmount;
		ZDecimal IImport5FEHeader.AfterTotalDutyTaxAmount => AfterTotalDutyTaxAmount;
		ZDecimal IImport5FEHeader.DutyTaxDifference => DutyTaxDifference;
		ZDecimal IImport5FEHeader.BeforeCustomsValue => BeforeCustomsValue;
		ZDecimal IImport5FEHeader.AfterCustomsValue => AfterCustomsValue;
		ZDecimal IImport5FEHeader.CustomsValueDifference => CustomsValueDifference;
		ZString IImport5FEHeader.DutyPenaltyType => DutyPenaltyType;
		ZString IImport5UASessionDetails.PenaltyExemptionIndicator => PenaltyExemptionIndicator;
		ZString IImport5UASessionDetails.PenaltyExemptionReasonCode => PenaltyExemptionReasonCode;
		ZString IImport5UASessionDetails.PenaltyExemptionReason => PenaltyExemptionReason;
		ZInt IImport5UASessionDetails.DutyPenaltyExemption5UASequenceNumber => PenaltyExemptionReqSequence;
		ZDecimal IImport5UASessionDetails.PenaltyExemptionAmount => PenaltyExemptionAmount;
	}
}
