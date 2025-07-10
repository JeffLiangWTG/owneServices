using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IEXPDATLine : IAESLine
	{
		string TransactionType { get; }
		string CountryOfExport { get; }
		string CountryOfDestination { get; }
		ZString CommercialReferenceNumber { get; }
		IReadOnlyCollection<IReference> Authorisations { get; }
		ZString RequestedProcedure { get; }
		ZString PreviousProcedure { get; }
		ZString AdditionalProcedure { get; }
		IAESParty Consignor { get; }
		IAESParty Consignee { get; }
		IReadOnlyCollection<ISupplyChainActor> AdditionalSupplyChainActors { get; }
		string CountryOfOrigin { get; }
		ZString OriginFederalState { get; }
		ZString GoodsDescription { get; }
		string CusCode { get; }
		string HarmonizedSystemSubHeadingCode { get; }
		ZString CombinedNomenclatureCode { get; }
		ZString TaricFirstAdditionalCode { get; }
		ZString TaricSecondAdditionalCode { get; }
		IReadOnlyCollection<ZString> TaricOtherAdditionalCodes { get; }
		bool TaricOtherAdditionalCodesSpecified { get; }
		IReadOnlyCollection<string> DangerousGoodsCodes { get; }
		ZDecimal GrossMass { get; }
		ZDecimal NetMass { get; }
		IReadOnlyCollection<IPackage> Packages { get; }
		IReadOnlyCollection<IPreviousDocument> PreviousDocuments { get; }
		IReadOnlyCollection<ISupportingDocument> Documents { get; }
		IReadOnlyCollection<IReference> AdditionalReferences { get; }
		IReadOnlyCollection<IReference> AdditionalInformations { get; }
		ZString TransportChargesPaymentMethod { get; }
		string OutwardProcessingReplacement { get; }
		DateTime OutwardProcessingReimportDate { get; }
		ZString WarehouseLocalReferenceNumber { get; }
		IAuthorisation CustomsWarehousingAuthorisation { get; }
		IReadOnlyCollection<IWarehouseProcedure> WarehouseProcedures { get; }
		bool IsWarehouseProcedure { get; }
		bool IsInwardProcessingProcedure { get; }
		string InwardProcessingSimplyGrantedAuthorisation { get; }
		IAuthorisation InwardProcessingAuthorisation { get; }
		string InwardProcessingCustomsOfficeOfSupervision { get; }
		IReadOnlyCollection<IInwardProcessingProcedure> InwardProcessingProcedures { get; }
		bool ProcedureTransferenceSpecified { get; }

		ZString Annotation { get; }
		IReadOnlyCollection<ZString> ContainerIdentificationNumbers { get; }
		IDeliveryTerms DeliveryTerms { get; }
		ZString WarehouseOwner { get; }
		ZString ProcessingOwner { get; }
		decimal TotalDutiesAndTaxesAmount { get; }
		string TaxType { get; }
		decimal PayableTaxAmount { get; }
		string TaxPaymentMethod { get; }
		decimal TaxBaseTaxRate { get; }
		string TaxBaseMeasurementUnitAndQualifier { get; }
		decimal TaxBaseQuantity { get; }
		decimal TaxBaseAmount { get; }
		decimal TaxBaseTaxAmount { get; }
	}
}
