using System;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public interface ITOperator
{
	ZString Country { get; }
	ZString Identifier { get; }
	ZString OperatorIdentity { get; }
	ITContactPerson ContactPerson { get; }
	ITAddress OperatorAddress { get; }
	ZString OperatorName { get; }
}

public interface ITAddress
{
	ZString City { get; }
	ZString Country { get; }
	ZString CountrySubEntity { get; }
	ZString PostalCode { get; }
	ZString StreetAndNumber1 { get; }
	ZString StreetAndNumber2 { get; }
}
public interface ITContactPerson
{
	ZString ContactPersonCommunicationNumber { get; }
	ZString ContactPersonEmail { get; }
	ZString ContactPersonFaxNumber { get; }
	ZString ContactPersonName { get; }
}

public interface ICUSDECMessageDataProvider
{
	ZString FunctionCode { get; }
	ITGoodsDeclarationImport GoodsDeclaration { get; }
	ITGoodsItemImport[] GoodsItem { get; }
	ZString LanguageCode { get; }
	ITOperator MessageSender { get; }
}

public interface ITDeclarant : ITOperator
{
	ZString DeclarantStatus { get; }
}

public interface ITGoodsDeclaration
{
	DateTime AcceptanceDate { get; }
	ZString CommercialReference { get; }
	ITDeclarant Declarant { get; }
	ZString IssuePlace { get; }
	ZDecimal LoadingList { get; }
	ZBool LoadingListSpecified { get; }
	ZString LocalReferenceNumber { get; }
	ZString RegistrationNumber { get; }
	ZString Signature { get; }
	ITtotals Totals { get; }
	ITTransactionNature TransactionNature { get; }
}
public interface ITChargesImport
{
	IMonetaryAmount CustomsCharges { get; }
	IMonetaryAmount VATCharges { get; }
}

public interface ITExchangeRate
{
	ZString Currency { get; }
	ZDecimal ExchangeRate { get; }
	ZBool ExchangeRateSpecified { get; }
}
public interface ITPaymentTaxes
{
	ZString DeferredPayment { get; }
	ZString DeferredPaymentAccountHolder { get; }
	ZString PaymentMethodTaxes { get; }
}
public interface ITPaymentVat
{
	ZString DeferredPaymentVat { get; }
	ZString PaymentMethodVat { get; }
}
public interface ITCustomsImport
{
	ITGoodsLocationImportExport GoodsLocation { get; }
	ZString PresentationCustomsOffice { get; }
	ITSeal[] Seal { get; }
	ZString Seals { get; }
	ZString SupervisingCustomsOffice { get; }
	ZString ValidationOffice { get; }
}

public interface ITSeal
{
	ZString SealAffixed { get; }
	ZString Sealingparty { get; }
}

public interface ITGoodsLocationImportExport
{
	ZString AgreedLocation { get; }
	ZString AuthorisedLocation { get; }
	ZString Precise { get; }
}

public interface ITDV1DeclarationHeader
{
	ITOperator Buyer { get; }
	ITBuyerSellerRelation BuyerSellerRelation { get; }
	ITContract Contract { get; }
	ZString CostRelatedPrevCustDecisions { get; }
	ITPreviousCustomsDecision PreviousCustomsDecision { get; }
	ITRestrictionsAndConditions RestrictionsAndConditions { get; }
	ITRoyaltiesAndResale RoyaltiesAndResale { get; }
	ITOperator Seller { get; }
	ZString Signature { get; }
}

public interface ITBuyerSellerRelation
{
	ZString CloseApproximate { get; }
	ZString CloseApprxDetails { get; }
	ZString Influence { get; }
	ZString Relation { get; }
}
public interface ITContract
{
	DateTime ContractDate { get; }
	ZString ContractNumber { get; }
}

public interface ITPreviousCustomsDecision
{
	DateTime PrevCustDecisionDate { get; }
	ZBool PrevCustDecisionDateSpecified { get; }
	ZString PrevCustDecisionNumber { get; }
}

public interface ITRestrictionsAndConditions
{
	ZString Consideration { get; }
	ZString RestrictCondDetails { get; }
	ZString Restrictions { get; }
}
public interface ITRoyaltiesAndResale
{
	ZString Conditions { get; }
	ZString Resale { get; }
	ZString Royalties { get; }
}
public interface ITTransportMeansImport
{
	ZString BorderMode { get; }
	ZString BorderNationality { get; }
	ITDeliveryTerms DeliveryTerms { get; }
	ZString DepartureIdentity { get; }
	ZString DispatchCountry { get; }
	ZString InlandMode { get; }
}

public interface ITDeliveryTerms
{
	ZString DeliveryTerms { get; }
	ZString DeliveryTermsPlace { get; }
	ZString DeliveryTermsPlaceCode { get; }
}

public interface ITtotals
{
	ZDecimal Items { get; }
	ZBool ItemsSpecified { get; }
	ZDecimal Packages { get; }
	ZBool PackagesSpecified { get; }
	ZDecimal TotalGrossmass { get; }
	ZBool TotalGrossmassSpecified { get; }
	ZDecimal TotalNetmass { get; }
}

public interface ITTransactionNature
{
	ZString TransactionNature1 { get; }
	ZString TransactionNature2 { get; }
}

public interface ITPackagingImportExport
{
	ZString MarksNumber { get; }
	ZDecimal Packages { get; }
	ZString PackageType { get; }
}

public interface ITSupplementaryUnits
{
	ZDecimal SupplementaryUnits { get; }
	ZString SupplementaryUnitsCode { get; }
}

public interface ITArchiveInformation
{
	ZString ArchiveLocationIndicator { get; }
	ZString ArchiveRelatedLocationContent { get; }
	ZString ArchiveSupport { get; }
}

public interface ITDocument
{
	ZString DocumentReference { get; }
	ZString DocumentType { get; }
}

public interface ITDocumentQuantity
{
	ZDecimal Quantity { get; }
	ZString QuantityCode { get; }
}
public interface ITAccompanyingDocument
{
	ZByte[] AttachedDocContent { get; }
	ZString AttachedDocName { get; }
}

public interface ITProducedDocumentImportExport : ITDocument
{
	ITArchiveInformation ArchiveInformation { get; }
	ZString AuthorisationHolderCategory { get; }
	ZString AuthorisationHolderID { get; }
	ZString ComplementaryInformation { get; }
	ITDocumentQuantity DocumentQuantity { get; }
	DateTime ProducedDocumentsInformationDate { get; }
	ZBool ProducedDocumentsInformationDateSpecified { get; }
	ZString ProducedDocumentsValidationOffice { get; }
}
public interface ITAdditionalInformationImportExport
{
	ZString AdditionalInformationContent { get; }
	ZString AdditionalInformationType { get; }
}

public interface ITCustomsTreatmentImport
{
	ITCalculationUnits[] CalculationUnits { get; }
	ITChargesImport ChargesImport { get; }
	ITPreference Preference { get; }
	ITProcedureImport Procedure { get; }
	ZString Quota { get; }
	ITTaxCalculation[] TaxCalculation { get; }
	ZString ValuationMethod { get; }
	ITWarehouse Warehouse { get; }
}
public interface ITWarehouse
{
	ZString WarehouseCountry { get; }
	ITOperator WarehouseDepositor { get; }
	ZString WarehouseIdentity { get; }
	ZString WarehouseType { get; }
}

public interface ITTaxCalculation
{
	ZDecimal TaxDue { get; }
	ZString TaxType { get; }
}

public interface ITProcedureImport
{
	ZString[] NationalProcedureCode { get; }
	ZString[] ProcedureNat { get; }
	ZString ProcedurePart1 { get; }
	ZString ProcedurePart2 { get; }
	ZString ProcedureType { get; }
}

public interface ITPreference
{
	ZString Preference1 { get; }
	ZString Preference2 { get; }
}

public interface ITCalculationUnits
{
	ZString CalculationCode { get; }
	ZDecimal CalculationUnits { get; }
}

public interface IMonetaryAmount
{
	ITExchangeRate ExchangeRate { get; }
	ZDecimal Amount { get; }
}

public interface ITBuyerCosts
{
	IMonetaryAmount Brokerage { get; }
	IMonetaryAmount Commissions { get; }
	IMonetaryAmount Containers { get; }
}

public interface ITBuyerGoodsServices
{
	IMonetaryAmount Engineering { get; }
	IMonetaryAmount MaterialsConsumed { get; }
	IMonetaryAmount MaterialsIncorporated { get; }
	IMonetaryAmount Tools { get; }
}

public interface ITDeliveryCost
{
	IMonetaryAmount Insurance { get; }
	IMonetaryAmount LoadingAndHandling { get; }
	ZString PlaceOfIntro { get; }
	IMonetaryAmount Transport { get; }
}

public interface ITAdditions
{
	ITBuyerCosts BuyerCosts { get; }
	ITBuyerGoodsServices BuyerGoodsServices { get; }
	ITDeliveryCost DeliveryCost { get; }
	IMonetaryAmount Proceeds { get; }
	IMonetaryAmount Royalties { get; }
}
public interface ITCalcBasis
{
	IMonetaryAmount IndirectPayments { get; }
}
public interface ITOtherCharges
{
	IMonetaryAmount OtherCosts { get; }
	ZString Specification { get; }
}

public interface ITDeductions
{
	IMonetaryAmount ChargesAfterImport { get; }
	IMonetaryAmount ImportDutiesAndTaxes { get; }
	ITOtherCharges OtherCharges { get; }
	IMonetaryAmount TransportAfterArrival { get; }
}

public interface ITDV1Detail
{
	ITAdditions Additions { get; }
	ITCalcBasis CalcBasis { get; }
	ITDeductions Deductions { get; }
}
public interface ITGuaranteeReferenceImport
{
	DateTime GuaranteeDate { get; }
	ZBool GuaranteeDateSpecified { get; }
	ZString GuaranteeNumber { get; }
	ZString GuaranteeOffice { get; }
}
public interface ITGuaranteeImport
{
	ZString GuaranteeAccess { get; }
	ZDecimal GuaranteeAmount { get; }
	ZBool GuaranteeAmountSpecified { get; }
	ZString GuaranteeCurrencyCode { get; }
	ZString GuaranteeOtherRef { get; }
	ITGuaranteeReferenceImport GuaranteeReference { get; }
	ZString GuaranteeType { get; }
}

public interface ITPreviousDocumentImportExport : ITDocument
{
	ZString PreviousDocumentArt { get; }
	ZString PreviousDocumentBillOfLoading { get; }
	ZString PreviousDocumentCategory { get; }
	DateTime PreviousDocumentDate { get; }
	ZBool PreviousDocumentDateSpecified { get; }
	ZString PreviousDocumentItem { get; }
	ZString PreviousDocumentLoc { get; }
}
public interface ITGoodsDeclarationImport : ITGoodsDeclaration
{
	ITOperator Carrier { get; }
	ITChargesImport ChargesImport { get; }
	ITOperator Consignee { get; }
	ITOperator Consignor { get; }
	ITCustomsImport Customs { get; }
	ITDV1DeclarationHeader DV1DeclarationHeader { get; }
	ZString InternalCurrencyUnit { get; }
	ITOperator Intracom { get; }
	ITPaymentTaxes PaymentTaxes { get; }
	ITPaymentVat PaymentVat { get; }
	ITOperator Representative { get; }
	ITTransportMeansImport TransportMeans { get; }
	ZString TypePartOne { get; }
	ZString TypePartTwo { get; }
	ZString ValueDetails { get; }
}
public interface ITGoodsItemImport : ITGoodsItem
{
	ITAccompanyingDocument[] AccompanyingDocument { get; }
	ITAdditionalInformationImportExport[] AdditionalInformation { get; }
	ITOperator Consignee { get; }
	ITOperator Consignor { get; }
	ZString CUSCode { get; }
	ITCustomsTreatmentImport CustomsTreatment { get; }
	ZString destinationRegion { get; }
	ITDV1Detail DV1Detail { get; }
	ITGuaranteeImport Guarantee { get; }
	ITOperator Intracom { get; }
	ZString OriginCountry { get; }
	ITPaymentVat PaymentVat { get; }
	ITPreviousDocumentImportExport PreviousDocument { get; }
	IMonetaryAmount Price { get; }
	ITOperator Representative { get; }
	ZDecimal StatisticalValueAdjustment { get; }
	ZBool StatisticalValueAdjustmentSpecified { get; }
	ZString Valuationindicators { get; }
}
public interface ITGoodsItem
{
	ZString[] ChassisNumber { get; }
	ZString CommodityCode { get; }
	ZString[] ContainerIdentifier { get; }
	ITDeliveryTerms DeliveryTerms { get; }
	ZString DestinationCountry { get; }
	ZString FirstAdditionalCommodity { get; }
	ZString GoodsDescription { get; }
	ZDecimal GrossMass { get; }
	ZBool GrossMassSpecified { get; }
	ZString NationalAdditionalCommodity1 { get; }
	ZString NationalAdditionalCommodity2 { get; }
	ZString NationalAdditionalCommodity3 { get; }
	ZDecimal NetMass { get; }
	ITPackagingImportExport[] Packaging { get; }
	ITProducedDocumentImportExport[] ProducedDocument { get; }
	ZString SecondAdditionalCommodity { get; }
	ZDecimal Sequence { get; }
	ITSupplementaryUnits SupplementaryUnits { get; }
	ITTransactionNature TransactionNature { get; }
	ZString Ucr { get; }
	ZString Undg { get; }
}
