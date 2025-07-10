using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface ICustomsEntryHeader : ICusCommonHeader, ICIQDataHeader
	{
		ZString EntryNumber { get; }
		ZString LocalReferenceNumber { get; }
		ZString DeclarationUnifiedNumber { get; }

		ZString PreEntryNumber { get; }

		ZDateTime ImportOrExportDate { get; }
		ZDateTime DeclarantDate { get; }
		ZDateTime DepartureDate { get; }

		ZString TradePartyCIQ { get; }

		ZString CargoOwnerUSCI { get; }
		ZString CargoOwnerCCD { get; }
		ZString CargoOwnerCIQ { get; }
		ZString CargoOwnerName { get; }

		ZString OverseasPartyCode { get; }
		ZString OverseasPartyName { get; }

		ZString DeclarantUSCI { get; }
		ZString DeclarantCCD { get; }
		ZString DeclarantCIQ { get; }
		ZString DeclarantName { get; }

		ZString VesselName { get; }
		ZString Voyage { get; }
		ZString LocationOfGoods { get; }
		ZString BillOfLading { get; }

		ZString CountryOfTradeCode { get; }
		ZString CountryOfLoadOrDischargeCode { get; }
		ZString PortOfOriginOrDestCode { get; }
		ZString PortOfStopoverCode { get; }

		ZString LicenseNo { get; }

		ZString IncoTermCode { get; }

		ZString DocumentSubmissionTypeCode { get; }

		ZDecimal FreightFeeAmount { get; }
		ZString FreightFeeCurrencyCode { get; }
		ZString FreightFeeMarkCode { get; }

		ZDecimal InsuranceFeeAmount { get; }
		ZString InsuranceFeeCurrencyCode { get; }
		ZString InsuranceFeeMarkCode { get; }

		ZDecimal OtherFeeAmount { get; }
		ZString OtherFeeCurrencyCode { get; }
		ZString OtherFeeMarkCode { get; }

		ZString ContractNo { get; }

		ZString BondedAreaCode { get; }
		ZString FreightYardCode { get; }

		ZInt NoOfPacks { get; }
		ZString PackTypeCode { get; }

		ZDecimal GrossWeightInKG { get; }
		ZDecimal NetWeightInKG { get; }

		ZString Remarks { get; }
		ZString MarksAndNumbers { get; }

		ZString SpecialRelationshipConfirmCode { get; }
		ZString PriceAffectConfirmCode { get; }
		ZString PaymentOfRoyaltyConfirmCode { get; }

		ZString FormulaPricingConfirmCode { get; }
		ZString TemporaryPricingConfirmCode { get; }

		ZString RelatedEntryNumber { get; }
		ZString RelatedManualNumber { get; }

		ZBool IsPaperlessTaxForm { get; }
		ZBool IsAutonomousTaxFiling { get; }
		ZBool IsAssuredInspectClearance { get; }

		IEnumerable<ZString> SpecialBusinessIdentifiers { get; }
		IEnumerable<ICusSupportingDocument> SupportingDocuments { get; }
		IEnumerable<ICusContainer> Containers { get; }
		IEnumerable<ICustomsEntryLine> EntryLines { get; }
	}

	public interface ICusContainer
	{
		ZString ContainerNumber { get; }
		ZString ContainerCode { get; }
		ZBool IsLessContainer { get; }
		ZDecimal TareWeightInKG { get; }
		IEnumerable<ZShort> LinkedEntryLineNos { get; }
	}

	public interface ICusSupportingDocument
	{
		ZString DocumentType { get; }
		ZString DocumentNumber { get; }
		IEnumerable<ZInt> ItemNumbers { get; }
	}
}
