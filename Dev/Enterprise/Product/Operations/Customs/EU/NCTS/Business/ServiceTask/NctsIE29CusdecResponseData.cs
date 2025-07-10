using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class NctsIE29CusdecResponseData
	{
		public NctsIE29CusdecResponseData(BusinessObjectFactory factory)
		{
			this.factory = factory;
			TransitCustomsOffices = new List<CustomsOfficeResponseData>();
			Guarantees = new List<GuaranteeResponseData>();
			GoodsItems = new List<NctsGoodsItemResponseData>();
			Seals = new List<ZString>();
			Authorisations = new List<AuthorisationResponseData>();
		}

		public ZString LocalReferenceNumber { get; set; }
		public ZString MovementReferenceNumber { get; set; }
		public ZString DeclarationType { get; set; }
		public ZString CountryOfDestination { get; set; }
		public ZString AgreedLocationOfGoodsCode { get; set; }
		public ZString AgreedLocationOfGoods { get; set; }
		public ZString AuthorisedLocationOfGoodsCode { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString PlaceOfLoadingCode { get; set; }
		public ZString CountryOfDispatch { get; set; }
		public ZString CustomsSubPlace { get; set; }
		public ZString InlandTransportMode { get; set; }
		public ZString TransportModeAtBorder { get; set; }
		public ZString MeansOfTransportCrossingBorderIdentity { get; set; }
		public ZString MeansOfTransportCrossingBorderNationality { get; set; }
		public ZString MeansOfTransportCrossingBorderType { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString MeansOfTransportAtDepartureIdentity { get; set; }
		public ZString MeansOfTransportAtDepartureNationality { get; set; }
		public ZBool IsContainerised { get; set; }
		public ZString NctsReturnCopy { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString AcceptanceDate { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString IssuingDate { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString DialogLanguageIndicatorAtDeparture { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString NctsAccompanyingDocumentLanguageCode { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString TotalNumberOfItems { get; set; }
		public ZString TotalNumberOfPackages { get; set; }
		public ZString TotalGrossMassInKilograms { get; set; }
		public ZString TotalNettMassInKilograms { get; set; } // calculated - not in message
		public ZString BindingItinerary { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString AuthorisationId { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString DeclarationDate { get; set; }
		public ZString DeclarationPlace { get; set; }
		public ZString SpecificCircumstanceIndicator { get; set; }
		public ZString TransportChargesMoP { get; set; }
		public ZString CommercialReferenceNumber { get; set; }
		public ZBool IsSecurity { get; set; }
		public ZString ConveyanceReferenceNumber { get; set; }
		public ZString PlaceOfUnloadingCode { get; set; }
		public AddressResponseData Principal { get; set; }
		public AddressResponseData Consignor { get; set; }
		public AddressResponseData Consignee { get; set; }
		public ZString AuthorisedConsigneeEori { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString DepartureCustomsOfficeCode { get; set; }
		public List<CustomsOfficeResponseData> TransitCustomsOffices { get; set; }
		public ZString DestinationCustomsOfficeCode { get; set; }
		public AddressResponseData ReturnCopiesCustomsOffice { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString ControlResultControlDate { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString ControlResultControlResultCode { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString ControlResultControlledBy { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString ControlResultTimeLimit { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString RepresentativeName { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString RepresentativeCapacity { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public ZString SealsNumber { get; set; } // TODO - member that doesn't exist on NctsHeader that maybe should ???
		public List<GuaranteeResponseData> Guarantees { get; set; }
		public List<NctsGoodsItemResponseData> GoodsItems { get; set; }
		public List<ZString> Seals { get; set; }
		public ZString Itinerary { get; set; }
		public AddressResponseData Carrier { get; set; }
		public AddressResponseData SecurityConsignor { get; set; }
		public AddressResponseData SecurityConsignee { get; set; }

		#region Phase 5
		public ZDateTime PresentationOfTheGoodsDateAndTime { get; set; }
		public List<AuthorisationResponseData> Authorisations { get; set; }
		public RepresentativeResponseData Representative { get; set; }
		public ZString ReferenceNumberUCR { get; set; }
		#endregion

		public BusinessObjectFactory factory { get; set; }
	}
}
