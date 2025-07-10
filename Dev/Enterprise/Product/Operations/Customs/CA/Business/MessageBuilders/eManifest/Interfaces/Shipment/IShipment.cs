namespace Enterprise.Customs.CA.Business.MessageBuilders.eManifest
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.Business.MessageBuilders.eManifest;

	interface IShipment
	{
		/// <summary>
		/// Service Option Id (984 = A8A Import Cargo Report). (M/3)
		/// </summary>
		ZString ServiceOptionId { get; }

		/// <summary>
		/// Mode by which the merchandise crosses the international border. (M/2)
		/// </summary>
		ZString MethodOfTransportation { get; }

		/// <summary>
		/// Estimated Date/Time conveyance will arrive at the first port in CA. (M/CCYYMMDDHHMM; C/ZHHMM)
		/// Condition: ZHHMM = time zone given as offset from Coordinated Universal Time (UTC).
		///			   If time zone is not provided, Eastern Time will be assumed.
		/// </summary>
		ZDateTime EstimatedDateOfArrival { get; }

		/// <summary>
		/// Unique Consignment Reference Number (UCR). (O/35)
		/// </summary>
		ZString UniqueConsignmentReference { get; }

		/// <summary>
		/// A number that uniquely identifies the shipment to the CBSA. (M/25)
		/// Consists of a carrier’s CBSA assigned Carrier Code, followed by a unique reference number assigned by the carrier and cannot contain spaces.
		/// This number cannot be re-used for 3 years.
		/// </summary>
		ZString OriginalCargoControlNumber { get; }

		/// <summary>
		/// An indicator that identifies that CSA goods are being transported. (M/1)
		/// If ‘True’ is specified then CSA BN is required and consignee must be a CSA approved importer.
		/// </summary>
		ZBool CSAIndicator { get; }

		/// <summary>
		/// CBSA business number (BN) for CSA shipments. (C/999999999RM9999)
		/// Condition: Mandatory where Merchandise Type Code = CSA (CSA Indicator = true). 
		/// For CSA shipments, consignee must be a CSA approved importer.
		/// </summary>
		ZString CSABusinessNumber { get; }

		/// <summary>
		/// City and country of loading. State is required when country is US. (M)
		/// </summary>
		IAddress PlaceOfLoading { get; }

		/// <summary>
		/// The Port of Report will be the expected first Canadian Port (CBSA office) of Arrival (FPOA).
		/// CA: (M/4).
		/// </summary>
		ZString PortOfReport { get; }

		/// <summary>
		/// The name of the city/state/country in which the goods are first taken over by the carrier. (C)
		/// State is required when country is US.
		/// Condition: Mandatory if address different than Shipper.
		/// </summary>
		IAddress PlaceOfAcceptance { get; }

		/// <summary>
		/// The CBSA port code where release / acquittal will be effected. (M/4) 
		/// </summary>
		ZString ManifestDestinationCustomsOffice { get; }

		/// <summary>
		/// The CBSA port sub-location code where release / acquittal will be effected. (C/4)
		/// Condition: Must be transmitted to supply warehouse code, if applicable.
		/// </summary>
		ZString ManifestDestinationSubLocationCode { get; }

		/// <summary>
		/// A code that specifies whether or not the shipment is consolidated. (C/1)
		/// Condition: Mandatory for consolidated freight.
		/// </summary>
		ZBool ConsolidatedFreightIndicator { get; }

		/// <summary>
		/// Customs Procedure Code (24 = Imported Goods). (M/2)
		/// </summary>
		ZString CustomsProcedureCode { get; }

		/// <summary>
		/// !!!Future Use!!!
		/// Importer Advance Trade Data (IATD) Exception Code. (M/2)
		/// If no Importer Advance Trade Data is to be filed, a valid code that identifies specific exception.
		/// </summary>
		ZString ImporterAdvanceTradeDataExceptionCode { get; }

		/// <summary>
		/// Used to specify amendment reason for changes after complete submission of Manifest to Customs. (C/2)
		/// Condition: Used to amend a completed Manifest.
		/// </summary>
		ZString AmendmentReasonCode { get; }

		/// <summary>
		/// !!!Future Use!!!
		/// The CBSA identifier of the party that is to receive an electronic notification from the CBSA regarding the shipment. (M/15)
		/// </summary>
		ZString SecondNotifyPartyId { get; }

		/// <summary>
		/// !!!Future Use!!!
		/// Manifest Forward Client Id. (M/15)
		/// </summary>
		ZString ManifestForwardId { get; }

		/// <summary>
		/// Directions for handling a shipment and/or delivery directions for a shipment. (C/50/up to 6)
		/// </summary>
		ZString SpecialInstructions { get; }

		/// <summary>
		/// Shipment-Commodity - multiple occurances if applicable (M/1)
		/// </summary>
		IEnumerable<ICommodity> Commodities { get; }

		/// <summary>
		/// Shipment-Parties - Total count of parties cannot exceed 20 (includes Consignee, Consignor, Delivery Destination and Notify Party)
		/// Consignee/Consignor - required. (M/2)
		/// Delivery Destination - (C/up to 18), Condition: Transmit if different from Consignee or ultimate consignee address.
		/// Notify Party - (C/up to 18), Condition: Transmit if available.
		/// </summary>
		IEnumerable<IParty> Parties { get; }
	}
}
