using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("ImportEntryHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportEntryHeader : IImportEntryHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public string HouseBillNumber { get; set; }
		public bool HouseBillSplitDeclarationIndicator { get; set; }
		public string HouseBillSplitDeclarationReasonCode { get; set; }
		public string HouseBillSplitDeclarationReasonDescription { get; set; }
		public string CargoManagementNo { get; set; }
		public DateTime UnderbondMovementArrivalDate { get; set; }
		public DateTime ArrivalDateAtDischargePort { get; set; }
		public string PaymentType { get; set; }
		public Organisation Declarant { get; set; }
		public Organisation Importer { get; set; }
		public string ImporterType { get; set; }
		public Organisation Payer { get; set; }
		public string FreightForwarderCompanyName { get; set; }
		public string FreightForwarderID { get; set; }
		public Organisation OnlineTradeDistributor { get; set; }
		public Organisation Supplier { get; set; }
		public Organisation Shipper { get; set; }
		public string OnlineTradeType { get; set; }
		public Organisation OnlineTradeSeller { get; set; }
		public Organisation OnlineTradeSellingAgent { get; set; }
		public string DeclarationPlanCode { get; set; }
		public string ImportTypeCode { get; set; }
		public string TradeType { get; set; }
		public string DeclarationProcedureType { get; set; }
		public string CertificateOfOriginIssued { get; set; }
		public string ValueDeclarationAttached { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Weight)]
		public decimal TotalGrossWeightInKG { get; set; }
		public int TotalPackQty { get; set; }
		public string PackType { get; set; }
		public string ArrivalPort { get; set; }
		public string TransportMode { get; set; }
		public string ContainerPackMode { get; set; }
		public string DepartureCountryCode { get; set; }
		public string VesselOrFlightNo { get; set; }
		public string VesselCountryCode { get; set; }
		public string MasterBillNumber { get; set; }
		public string CarrierID { get; set; }
		public string BondedAreaCode { get; set; }
		public string LocationIDInBondedArea { get; set; }
		public string Incoterm { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.ImportTotalInvoiceAmount)]
		public decimal TotalInvoiceAmount { get; set; }
		public string InvoiceAmountCurrency { get; set; }
		public string InvoicePaymentTerm { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal TotalCustomsValueUSD { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal TotalCustomsValueKRW { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.ExchangeRate)]
		public decimal ExchangeRate { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CommercialChargeAmount)]
		public decimal Freight { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CommercialChargeAmount)]
		public decimal Insurance { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CommercialChargeAmount)]
		public decimal AdditionalAmount { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CommercialChargeAmount)]
		public decimal DeductedAmount { get; set; }
		public string CourierCompanyID { get; set; }
		public string AuthorizedImporterRegNo { get; set; }
		public string OwnerReferenceNumber { get; set; }
		public string SouthNorthTradeYN { get; set; }
		public string GoldTradeTransactionYN { get; set; }
		public string BondedFactoryUseCode { get; set; }
		public DateTime BondedFactoryUseDate { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalDutyAmount { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalSpecialConsumptionTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalTransportationTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalLiquorTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalEducationTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalAgricultureTax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalVAT { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalValueForVAT { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalVATExemptionValue { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal PenaltyForLateDeclaration { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal PenaltyForMissedDeclaration { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalPayableAmount { get; set; }
		public string CustomsBrokerCommentCode1 { get; set; }
		public string CustomsBrokerCommentCode2 { get; set; }
		public string CustomsBrokerCommentCode3 { get; set; }
		public string CustomsBrokerComment1 { get; set; }
		public string CustomsBrokerComment2 { get; set; }
		public bool ApplicationForAgreedRate { get; set; }
		public ImportEntryLine[] EntryLines { get; set; }
		public string UnipassDeclarantID { get; set; }
		public ImportContainer[] Containers { get; set; }
		public ImportOnlineOrder[] OnlineOrders { get; set; }
		public string BlanketValuationDeclarationNumber { get; set; }

		ZString IImportEntryHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImportEntryHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImportEntryHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		ZString IImportEntryHeader.HouseBillNumber => HouseBillNumber;
		ZString IImportEntryHeader.HouseBillSplitDeclarationReasonCode => HouseBillSplitDeclarationReasonCode;
		ZString IImportEntryHeader.HouseBillSplitDeclarationReasonDescription => HouseBillSplitDeclarationReasonDescription;
		ZString IImportEntryHeader.CargoManagementNo => CargoManagementNo;
		ZDate IImportEntryHeader.UnderbondMovementArrivalDate => (ZDate)UnderbondMovementArrivalDate;
		ZDate IImportEntryHeader.ArrivalDateAtDischargePort => (ZDate)ArrivalDateAtDischargePort;
		ZString IImportEntryHeader.PaymentType => PaymentType;
		IOrganization IImportEntryHeader.Declarant => Declarant;
		IOrganization IImportEntryHeader.Importer => Importer;
		ZString IImportEntryHeader.ImporterType => ImporterType;
		IOrganization IImportEntryHeader.Payer => Payer;
		ZString IImportEntryHeader.FreightForwarderCompanyName => FreightForwarderCompanyName;
		ZString IImportEntryHeader.FreightForwarderID => FreightForwarderID;
		IOrganization IImportEntryHeader.OnlineTradeDistributor => OnlineTradeDistributor;
		IOrganization IImportEntryHeader.Supplier => Supplier;
		IOrganization IImportEntryHeader.Shipper => Shipper;
		ZString IImportEntryHeader.OnlineTradeType => OnlineTradeType;
		IOrganization IImportEntryHeader.OnlineTradeSeller => OnlineTradeSeller;
		IOrganization IImportEntryHeader.OnlineTradeSellingAgent => OnlineTradeSellingAgent;
		ZString IImportEntryHeader.DeclarationPlanCode => DeclarationPlanCode;
		ZString IImportEntryHeader.ImportTypeCode => ImportTypeCode;
		ZString IImportEntryHeader.TradeType => TradeType;
		ZString IImportEntryHeader.DeclarationProcedureType => DeclarationProcedureType;
		ZString IImportEntryHeader.CertificateOfOriginIssued => CertificateOfOriginIssued;
		ZString IImportEntryHeader.ValueDeclarationAttached => ValueDeclarationAttached;
		ZDecimal IImportEntryHeader.TotalGrossWeightInKG => TotalGrossWeightInKG;
		ZInt IImportEntryHeader.TotalPackQty => TotalPackQty;
		ZString IImportEntryHeader.PackType => PackType;
		ZString IImportEntryHeader.ArrivalPort => ArrivalPort;
		ZString IImportEntryHeader.TransportMode => TransportMode;
		ZString IImportEntryHeader.ContainerPackMode => ContainerPackMode;
		ZString IImportEntryHeader.DepartureCountryCode => DepartureCountryCode;
		ZString IImportEntryHeader.VesselOrFlightNo => VesselOrFlightNo;
		ZString IImportEntryHeader.VesselCountryCode => VesselCountryCode;
		ZString IImportEntryHeader.MasterBillNumber => MasterBillNumber;
		ZString IImportEntryHeader.CarrierID => CarrierID;
		ZString IImportEntryHeader.BondedAreaCode => BondedAreaCode;
		ZString IImportEntryHeader.LocationIDInBondedArea => LocationIDInBondedArea;
		ZString IImportEntryHeader.Incoterm => Incoterm;
		ZDecimal IImportEntryHeader.TotalInvoiceAmount => TotalInvoiceAmount;
		ZString IImportEntryHeader.InvoiceAmountCurrency => InvoiceAmountCurrency;
		ZString IImportEntryHeader.InvoicePaymentTerm => InvoicePaymentTerm;
		ZDecimal IImportEntryHeader.TotalCustomsValueUSD => TotalCustomsValueUSD;
		ZDecimal IImportEntryHeader.TotalCustomsValueKRW => TotalCustomsValueKRW;
		ZDecimal IImportEntryHeader.ExchangeRate => ExchangeRate;
		ZDecimal IImportEntryHeader.Freight => Freight;
		ZDecimal IImportEntryHeader.Insurance => Insurance;
		ZDecimal IImportEntryHeader.AdditionalAmount => AdditionalAmount;
		ZDecimal IImportEntryHeader.DeductedAmount => DeductedAmount;
		ZString IImportEntryHeader.CourierCompanyID => CourierCompanyID;
		ZString IImportEntryHeader.AuthorizedImporterRegNo => AuthorizedImporterRegNo;
		ZString IImportEntryHeader.OwnerReferenceNumber => OwnerReferenceNumber;
		ZString IImportEntryHeader.SouthNorthTradeYN => SouthNorthTradeYN;
		ZString IImportEntryHeader.GoldTradeTransactionYN => GoldTradeTransactionYN;
		ZString IImportEntryHeader.BondedFactoryUseCode => BondedFactoryUseCode;
		ZDateTime IImportEntryHeader.BondedFactoryUseDate => BondedFactoryUseDate;
		ZDecimal IImportEntryHeader.TotalDutyAmount => TotalDutyAmount;
		ZDecimal IImportEntryHeader.TotalSpecialConsumptionTax => TotalSpecialConsumptionTax;
		ZDecimal IImportEntryHeader.TotalTransportationTax => TotalTransportationTax;
		ZDecimal IImportEntryHeader.TotalLiquorTax => TotalLiquorTax;
		ZDecimal IImportEntryHeader.TotalEducationTax => TotalEducationTax;
		ZDecimal IImportEntryHeader.TotalAgricultureTax => TotalAgricultureTax;
		ZDecimal IImportEntryHeader.TotalVAT => TotalVAT;
		ZDecimal IImportEntryHeader.TotalValueForVAT => TotalValueForVAT;
		ZDecimal IImportEntryHeader.TotalVATExemptionValue => TotalVATExemptionValue;
		ZDecimal IImportEntryHeader.PenaltyForLateDeclaration => PenaltyForLateDeclaration;
		ZDecimal IImportEntryHeader.PenaltyForMissedDeclaration => PenaltyForMissedDeclaration;
		ZDecimal IImportEntryHeader.TotalPayableAmount => TotalPayableAmount;
		ZString IImportEntryHeader.CustomsBrokerCommentCode1 => CustomsBrokerCommentCode1;
		ZString IImportEntryHeader.CustomsBrokerCommentCode2 => CustomsBrokerCommentCode2;
		ZString IImportEntryHeader.CustomsBrokerCommentCode3 => CustomsBrokerCommentCode3;
		ZString IImportEntryHeader.CustomsBrokerComment1 => CustomsBrokerComment1;
		ZString IImportEntryHeader.CustomsBrokerComment2 => CustomsBrokerComment2;
		ZBool IImportEntryHeader.ApplicationForAgreedRate => ApplicationForAgreedRate;
		IEnumerable<IImportEntryLine> IImportEntryHeader.EntryLines => EntryLines;
		IEnumerable<IEntryLine> IEntryHeaderWithEntryLines.EntryLines => EntryLines;
		IEnumerable<IImportContainer> IImportEntryHeader.Containers => Containers;
		IEnumerable<IImportOnlineOrder> IImportEntryHeader.OnlineOrders => OnlineOrders;
		ZString IImportEntryHeader.BlanketValuationDeclarationNumber => BlanketValuationDeclarationNumber;
	}
}
