using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IH1ImportCommonDataProvider : IESEDIMessageCollectionProvider
{
}

public interface IH1CommonImportDataProvider : IH1ImportCommonDataProvider
{
	ZString Operation { get; }
	ZString CustomsOfficeOfImport { get; }
	ZString CustomOfficeOfPresentation { get; }
	IH1PartyProviderWithAddress Importer { get; }
	IPartyIdProviderWithContactPerson Declarant { get; }
	ICommonRepresentativeWithContactPerson Representative { get; }
}

public interface IH1ImportDataProvider : IH1CommonImportDataProvider
{
	IH1CommonMRN DataProviderMRN { get; }
	ZString ActivationMode { get; }
	IH1ImportOperation ImportOperation { get; }
	IReadOnlyCollection<ICommonAuthorisation> Authorisations { get; }
	ZString SupervisingCustomOffice { get; }
	IPartyIdProvider PersonPayingCustomsDuty { get; }
	ZString Currency { get; }
}

public interface IH1CommonMRN
{
	ZString MRN { get; }
}

public interface IH1CommonImportOperation
{
	ZString LRN { get; }
	ZString DeclarationType { get; }
}

public interface IH1ImportOperation : IH1CommonImportOperation
{
	ZString AdditionalDeclarationType { get; }
	ZString LanguageCode { get; }
}

public interface IH1PartyProviderWithAddress : IPartyNameProvider
{
	IPartyAddressProvider Address { get; }
}

public interface IH1DeferredPayment
{
	ZString SequenceNumber { get; }
	ZString Payment { get; }
	ZString CcQualifier { get; }
}

public interface ICommonImportH1GoodsShipment
{
	ZString InvoiceCurrency { get; }
	ZDecimal ExchangeRate { get; }
	IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActors { get; }
	IH1PartyProviderWithAddress Exporter { get; }
	ZString CountryOfDispatch { get; }
	IReadOnlyCollection<IH1CommonDocument> PreviousDocuments { get; }
	IReadOnlyCollection<IH1CommonSupportingDocument> SupportingDocuments { get; }
	IReadOnlyCollection<IH1CommonDocument> AdditionalReferences { get; }
	IReadOnlyCollection<IH1CommonDocument> AdditionalInfos { get; }
	IReadOnlyCollection<IH1AdditionalFiscalReference> AdditionalFiscalReferences { get; }
}

public interface IH1Destination
{
	ZString Country { get; }
	ZString Region { get; }
	ZString CcQualifier { get; }
}

public interface IH1Origin
{
	ZString Country { get; }
	ZString PreferentialCountry { get; }
}

public interface IH1CommonDocument : ICommonDocumentSequenceNumber
{
	ZString CcQualifier { get; }
}

public interface IH1CommonSupportingDocument : IH1CommonDocument
{
	ZString IssuingAuthorityName { get; }
	ZString LineNumber { get; }
	ZDateTime DocumentDate { get; }
}

public interface IH1CommonLineSupportingDocument : IH1CommonSupportingDocument
{
	ZString MeasurementUnitAndQualifier { get; }
	ZString CcQualifierForMeasurementUnitAndQualifier { get; }
	ZDecimal Quantity { get; }
	ZString Currency { get; }
	ZDecimal Amount { get; }
}

public interface IH1CommonPreviousDocument : IH1CommonDocument
{
	ZString TypeOfPackages { get; }
	ZString NumberOfPackages { get; }
	ZString MeasurementUnitAndQualifier { get; }
	ZString CcQualifierForMeasurementUnitAndQualifier { get; }
	ZDecimal Quantity { get; }
	ZString GoodsItemId { get; }
}

public interface IH1AdditionalFiscalReference
{
	ZString SequenceNumber { get; }
	ZString Role { get; }
	ZString VAT { get; }
}

public interface ICommonH1Procedure
{
	ZString RequestedCPC { get; }
	ZString PreviousCPC { get; }
}

public interface IH1Procedure : ICommonH1Procedure
{
	IReadOnlyCollection<IH1AdditionalCode> AdditionalProcedures { get; }
}

public interface IH1AdditionalCode
{
	ZString SequenceNumber { get; }
	ZString Code { get; }
	ZString CcQualifier { get; }
}

public interface ICommonImportH1Consigment
{
	ZBool IsContainerised { get; }
	ZDecimal GrossMass { get; }
	ZString ReferenceNumberUCR { get; }
	IReadOnlyCollection<ICommonTransportEquipment> TransportEquipments { get; }
	ICommonLocationOfGoods LocationOfGoods { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments { get; }
}

public interface ICommonH1Commodity
{
	ZString GoodsDescription { get; }
}

public interface IH1CompleteAndSimplifiedCommonImportCommodity : ICommonH1Commodity
{
	ZString CusCode { get; }
	ZString QuotaOrderNumber { get; }
	IH1CommodityCode CommodityCode { get; }
	ICommonGoodsMeasureWithSupUnitsAndSpecified GoodsMeasure { get; }
	ZDecimal InvoiceLineAmountInvoiced { get; }
}

public interface ICommonH1CommodityCode : ICommodityCodeCommon
{
	ZString TaricCode { get; }
}

public interface IH1CommodityCode : ICommonH1CommodityCode
{
	IReadOnlyCollection<ICommonAdditionalCode> TariffAdditionalCodes { get; }
	IReadOnlyCollection<IH1AdditionalCode> NationalAdditionalCodes { get; }
}

public interface IH1CalculationOfTaxes
{
	ZString Preference { get; }
	IReadOnlyCollection<IH1DutiesAndTaxes> DutiesAndTaxes { get; }
}

public interface IH1DutiesAndTaxes
{
	ZString SequenceNumber { get; }
	ZString Type { get; }
	ZString CcQualifier { get; }
	ZString MethodOfPayment { get; }
	IReadOnlyCollection<IH1TaxBase> TaxBases { get; }
}

public interface IH1TaxBase
{
	ZString SequenceNumber { get; }
	ZDecimal Rate { get; }
	ZString MeasurementUnitAndQualifier { get; }
	ZString CcQualifierForMeasurementUnitAndQualifier { get; }
	ZDecimal Quantity { get; }
	ZDecimal Amount { get; }
	ZDecimal TaxAmount { get; }
}

public interface IH1CustomsValuation
{
	ZString ValuationMethod { get; }
	IReadOnlyCollection<IH1AdditionsAndDeductions> AdditionsAndDeductions { get; }
}

public interface IH1AdditionsAndDeductions
{
	ZString SequenceNumber { get; }
	ZString Code { get; }
	ZDecimal Amount { get; }
}

public interface ICommonH1GoodsShipmentItem
{
	ZString DeclarationGoodsItemNumber { get; }
}

public interface ICompleteAndSimplifiedCommonImportH1GoodsShipmentItem : ICommonH1GoodsShipmentItem
{
	ZString ReferenceNumberUCR { get; }
	IReadOnlyCollection<ICommonAuthorisation> Authorisations { get; }
	IH1Procedure Procedure { get; }
	IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActors { get; }
	IH1PartyProviderWithAddress Exporter { get; }
	IH1Origin Origin { get; }
	ZString CountryOfDispatch { get; }
	IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum> Packages { get; }
	IReadOnlyCollection<IH1CommonPreviousDocument> PreviousDocuments { get; }
	IReadOnlyCollection<IH1CommonLineSupportingDocument> SupportingDocuments { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments { get; }
	IReadOnlyCollection<IH1CommonDocument> AdditionalReferences { get; }
	IReadOnlyCollection<IH1CommonDocument> AdditionalInfos { get; }
	IReadOnlyCollection<IH1AdditionalFiscalReference> AdditionalFiscalReferences { get; }
}
