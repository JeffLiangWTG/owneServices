using System;
using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public interface ICusEntryLineCustomsMessageWrapper
{
	int ItemNumber { get; }
	ICustomsProcedure Procedure { get; }
	IReadOnlyCollection<IPreviousDocument> PreviousDocuments { get; }
	IReadOnlyCollection<IAdditionalInformation> AdditionalInformation { get; }
	IReadOnlyCollection<MessageBuilder.ISupportingDocument> SupportingDocuments { get; }
	IEoriTrader Exporter { get; }
	IEoriTrader Seller { get; }
	IEoriTrader Buyer { get; }
	IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors { get; }
	IReadOnlyCollection<IFiscalReference> FiscalReferences { get; }
	IReadOnlyCollection<MessageBuilder.IFee> Fees { get; }
	decimal TotalFeeAmount { get; }
	IReadOnlyCollection<IAdditionOrDeduction> AdditionOrDeductions { get; }
	string RelatedIndicator { get; }
	decimal ItemPrice { get; }
	int ValuationMethod { get; }
	int? Preferences { get; }
	string DestinationStateCode { get; }
	string OriginCountryCode { get; }
	string PreferredOriginCountryCode { get; }
	decimal NetMass { get; }
	decimal? SupplementaryUnit { get; }
	decimal GrossMass { get; }
	string GoodsDescription { get; }
	IReadOnlyCollection<IPackage> Packages { get; }
	string CusCode { get; }
	string NcCode { get; }
	string TaricCode { get; }
	IReadOnlyCollection<string> AdditionalCodes { get; }
	IReadOnlyCollection<string> NationalAdditionalCodes { get; }
	IReadOnlyCollection<string> Containers { get; }
	string ConcessionOrder { get; }
	int TransactionNature { get; }
	decimal StatisticalValue { get; }
	string DestinationCountryCode { get; }
	string DispatchCountryCode { get; }
	DateTime? AcceptanceDate { get; }
	IReadOnlyCollection<IBaseAmount> BaseAmounts { get; }

	#region Export Only

	IReadOnlyCollection<IPreviousDocument> ExportPreviousDocuments { get; }
	IReadOnlyCollection<IAdditionalInformation> ExportAdditionalInformation { get; }
	IReadOnlyCollection<IAdditionalReference> ExportAdditionalReferences { get; }
	IReadOnlyCollection<IAuthorization> ExportAuthorizations { get; }
	IReadOnlyCollection<ITransportDocument> ExportTransportDocuments { get; }
	IEoriTrader ExportConsignor { get; }
	IEoriTrader ExportConsignee { get; }
	string ExportTransportChargesMethodOfPayment { get; }
	string CountryOfDestination { get; }
	string CountryOfExport { get; }
	string ExportCountryOfOrigin { get; }
	string ExportRegionOfDispatch { get; }
	string ExportHsTariffCode { get; }
	string ExportNcTariffCode { get; }
	IReadOnlyCollection<string> DangerousGoodsCodes { get; }
	int? ExportNatureOfTransaction { get; }

	#endregion
}
