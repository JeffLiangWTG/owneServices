using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportEntryHeader : IMessageDataProvider, IEntryHeaderWithEntryLines
	{
		ZString ImportDeclarationNumber { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A502)]
		ZString HouseBillNumber { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A504)]
		bool HouseBillSplitDeclarationIndicator { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A505)]
		ZString HouseBillSplitDeclarationReasonCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A506)]
		ZString HouseBillSplitDeclarationReasonDescription { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A501)]
		ZString CargoManagementNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A605, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZDate UnderbondMovementArrivalDate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A604)]
		ZDate ArrivalDateAtDischargePort { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A408)]
		ZString PaymentType { get; }
		IOrganization Declarant { get; }
		IOrganization Importer { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A203)]
		ZString ImporterType { get; }
		IOrganization Payer { get; }
		ZString FreightForwarderCompanyName { get; }
		ZString FreightForwarderID { get; }
		IOrganization OnlineTradeDistributor { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A401)]
		IOrganization Supplier { get; }
		IOrganization Shipper { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A621)]
		ZString OnlineTradeType { get; }
		IOrganization OnlineTradeSeller { get; }
		IOrganization OnlineTradeSellingAgent { get; }
		ZString DeclarationPlanCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A402)]
		ZString ImportTypeCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A403, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString TradeType { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A405, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString DeclarationProcedureType { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A406)]
		ZString CertificateOfOriginIssued { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A407)]
		ZString ValueDeclarationAttached { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A801, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZDecimal TotalGrossWeightInKG { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A803)]
		ZInt TotalPackQty { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A804)]
		ZString PackType { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A606)]
		ZString ArrivalPort { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A610)]
		ZString TransportMode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A611)]
		ZString ContainerPackMode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A607)]
		ZString DepartureCountryCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A608)]
		ZString VesselOrFlightNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A609)]
		ZString VesselCountryCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A503)]
		ZString MasterBillNumber { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A614)]
		ZString CarrierID { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A701, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString BondedAreaCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A702, DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal)]
		ZString LocationIDInBondedArea { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A805, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString Incoterm { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A807, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal TotalInvoiceAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A806, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString InvoiceAmountCurrency { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A808, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString InvoicePaymentTerm { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A810, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal TotalCustomsValueUSD { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A809, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal TotalCustomsValueKRW { get; }
		ZDecimal ExchangeRate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A811, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal Freight { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A812, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal Insurance { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A813, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal AdditionalAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A814, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal DeductedAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A615)]
		ZString CourierCompanyID { get; }
		ZString AuthorizedImporterRegNo { get; }
		ZString OwnerReferenceNumber { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A404, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZString SouthNorthTradeYN { get; }
		ZString GoldTradeTransactionYN { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A703)]
		ZString BondedFactoryUseCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A704)]
		ZDateTime BondedFactoryUseDate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A815, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes.CUD)]
		ZDecimal TotalDutyAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A816, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes.IND)]
		ZDecimal TotalSpecialConsumptionTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A818, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes.ENV)]
		ZDecimal TotalTransportationTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A817, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes.ACT)]
		ZDecimal TotalLiquorTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A820, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes._5AB)]
		ZDecimal TotalEducationTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A821, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes.CAP)]
		ZDecimal TotalAgricultureTax { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A819, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes.VAT)]
		ZDecimal TotalVAT { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A825, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal TotalValueForVAT { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A826, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal TotalVATExemptionValue { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A822, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes._5AC)]
		ZDecimal PenaltyForLateDeclaration { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A823, DataItemIDAttribute.ChangeType.DutyTaxRelated, EntryTaxTypeList.Codes._5AY)]
		ZDecimal PenaltyForMissedDeclaration { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A824, DataItemIDAttribute.ChangeType.DutyTaxRelated)]
		ZDecimal TotalPayableAmount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A903)]
		ZString CustomsBrokerCommentCode1 { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A904)]
		ZString CustomsBrokerCommentCode2 { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A905)]
		ZString CustomsBrokerCommentCode3 { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A901)]
		ZString CustomsBrokerComment1 { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.A902)]
		ZString CustomsBrokerComment2 { get; }
		ZBool ApplicationForAgreedRate { get; }

		IEnumerable<IImportContainer> Containers { get; }
		IEnumerable<IImportOnlineOrder> OnlineOrders { get; }
		ZString BlanketValuationDeclarationNumber { get; }
		new IEnumerable<IImportEntryLine> EntryLines { get; }
	}
}
