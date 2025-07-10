using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICompleteImportH1MessageDataProvider : IH1ImportDataProvider
{
	ZDate RecapitulationPeriod { get; }
	IPartyIdProvider PersonProvidingAGuarantee { get; }
	IReadOnlyCollection<ICompleteImportH1Guarantee> Guarantees { get; }
	IReadOnlyCollection<IH1DeferredPayment> DeferredPayments { get; }
	IReadOnlyCollection<ICompleteImportH1GoodsShipment> GoodsShipments { get; }
}

public interface ICompleteImportH1Guarantee
{
	ZString SequenceNumber { get; }
	ZString GuaranteeType { get; }
	IReadOnlyCollection<ICompleteImportH1GuaranteeReference> GuaranteeReferences { get; }
}

public interface ICompleteImportH1GuaranteeReference
{
	ZString SequenceNumber { get; }
	ZString GRN { get; }
	ZString CcQualifier { get; }
	ZString AccessCode { get; }
	ZString CurrencyCode { get; }
	ZDecimal AmountToBeCovered { get; }
	ZString OtherGuaranteeReference { get; }
	ZString CustomsOfficeOfGuarantee { get; }
}

public interface ICompleteImportH1GoodsShipment : ICommonImportH1GoodsShipment
{
	ZString SequenceNumber { get; }
	ZString NatureOfTransaction { get; }
	ZDecimal TotalAmountInvoiced { get; }
	ZDateTime DateOfAcceptance { get; }
	IH1PartyProviderWithAddress Buyer { get; }
	IH1PartyProviderWithAddress Seller { get; }
	ICommonDeliveryTerms DeliveryTerms { get; }
	IH1Destination Destination { get; }
	IWarehouseCommon Warehouse { get; }
	ICompleteImportH1Consigment Consignment { get; }
	IReadOnlyCollection<ICompleteImportH1GoodsShipmentItem> GoodsShipmentItems { get; }
}

public interface ICompleteImportH1Consigment : ICommonImportH1Consigment
{
	ZString InlandModeOfTransport { get; }
	ZString ModeOfTransportAtBorder { get; }
	ICommonArrivalTransportMeans ArrivalTransportMeans { get; }
	ZString ActiveBorderTransportMeansNationality { get; }
}

public interface ICompleteImportH1GoodsShipmentItem : ICompleteAndSimplifiedCommonImportH1GoodsShipmentItem
{
	ZString SequenceNumber { get; }
	ZDecimal StatisticalValue { get; }
	ZString NatureOfTransaction { get; }
	IH1PartyProviderWithAddress Buyer { get; }
	IH1PartyProviderWithAddress Seller { get; }
	IH1Destination Destination { get; }
	ICompleteImportH1Commodity Commodity { get; }
	IH1CustomsValuation CustomsValuation { get; }
	ZString ValuationAdjustmentIndicator { get; }
}

public interface ICompleteImportH1Commodity : IH1CompleteAndSimplifiedCommonImportCommodity
{
	IH1CalculationOfTaxes CalculationOfTaxes { get; }
}
