using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Transport")]
	public class RouteWrapper : GenericWrapper, IMovementLeg
	{
		public RouteWrapper(ZInt legNo, ZString transportMode, ZString additionalTransportMode, ZString vessel, ZString voyageFlight
			, ZString loadUNLOCO, JobDocAddress loadAddress, ZDateTime eTD, ZDateTime aTD
			, ZString destUNLOCO, JobDocAddress destAddress, ZDateTime eTA, ZDateTime aTA
			, ZDateTime availabilityDate
			, ZDateTime fCLReceivalCommences, ZDateTime lCLReceivalCommences, ZDateTime dGFCLReceivalCommences
			, ZDateTime fCLCutOff, ZDateTime lCLCutOff, ZDateTime dGFCLCutOff
			, ZDateTime lCLAvailabilityDate
			, ZDateTime fCLStorageDate, ZDateTime lCLStorageDate
			, OrgHeader carrier, OrgHeader creditor, ZString carrierBookingRef, JobVoyage voyage, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fLegNo = legNo;
			fTransportModeCode = transportMode;
			fAdditionalTransportModeCode = additionalTransportMode;
			fVessel = vessel;
			fVoyageFlight = voyageFlight;
			fLoadUNLOCO = loadUNLOCO;
			fLoadJobDocAddress = loadAddress;
			fETD = eTD;
			fATD = aTD;
			fDestUNLOCO = destUNLOCO;
			fDestJobDocAddress = destAddress;
			fETA = eTA;
			fATA = aTA;
			this.availabilityDate = availabilityDate;
			fCarrierBO = carrier;
			creditorBO = creditor;
			fCarrierBookingRef = carrierBookingRef;
			serviceLevel = new CodeAndDescriptionWrapper(ZString.Empty, new CodeDescriptionPairList(), factory);
			fIsCargoOnly = (voyage == null ? ZBool.False : voyage.JV_IsCargoOnly);
			fFCLReceivalCommences = fCLReceivalCommences;
			fLCLReceivalCommences = lCLReceivalCommences;
			fDGFCLReceivalCommences = dGFCLReceivalCommences;
			fFCLCutOff = fCLCutOff;
			fLCLCutOff = lCLCutOff;
			fDGFCLCutOff = dGFCLCutOff;
			fLCLAvailabilityDate = lCLAvailabilityDate;
			fFCLStorageDate = fCLStorageDate;
			fLCLStorageDate = lCLStorageDate;
			transportType = new CodeAndDescriptionWrapper(ZString.Empty, new CodeDescriptionPairList(), factory);
			forceShowVesselVoyage = false;
		}

		public RouteWrapper(ZInt legNo, ZString transportMode, ZString vessel, ZString voyageFlight
			, ZString loadUNLOCO, ZString destUNLOCO
			, ZDateTime eTD, ZDateTime eTA
			, BusinessObjectFactory factory)
			: this(legNo, transportMode, ZString.Empty, vessel, voyageFlight
				, loadUNLOCO, null, eTD, ZDateTime.Empty
				, destUNLOCO, null, eTA, ZDateTime.Empty
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty
				, null, null, ZString.Empty, null
				, factory)
		{
		}

		public RouteWrapper(ZInt legNo, BaseJobDeclaration declaration, BusinessObjectFactory factory)
			: this(legNo, declaration.JE_TransportMode, ZString.Empty, declaration.JE_VesselName, declaration.JE_VoyageFlightNo
				, declaration.JE_RL_NKPortOfLoading, null, declaration.JE_ExportDate, declaration.JE_ExportDate
				, declaration.JE_RL_NKPortOfArrival, null, declaration.JE_DateOfArrival, declaration.JE_DateOfArrival
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty
				, declaration.ShippingLine, null, declaration.JE_AgentsReference, declaration.Voyage
				, factory)
		{
		}

		public RouteWrapper(ZInt legNo, ZString loadUNLOCO, ZDateTime eTD, ZString destUNLOCO, ZDateTime eTA, BusinessObjectFactory factory)
			: this(legNo, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty
				, loadUNLOCO, null, eTD, ZDateTime.Empty
				, destUNLOCO, null, eTA, ZDateTime.Empty
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty
				, null, null, ZString.Empty, null
				, factory)
		{
		}

		public RouteWrapper(Transport transportBO, BusinessObjectFactory factory)
			: this(transportBO.JW_LegOrder, transportBO.JW_TransportMode, transportBO.JW_AdditionalTransportMode, transportBO.JW_Vessel, transportBO.JW_VoyageFlight
				, transportBO.JW_RL_NKLoadPort, null, transportBO.JW_ETD, transportBO.JW_ATD
				, transportBO.JW_RL_NKDiscPort, null, transportBO.JW_ETA, transportBO.JW_ATA
				, transportBO.JW_TerminalAvailabilityDate
				, transportBO.JW_TerminalReceivalCommences, transportBO.JW_DepotReceivalCommences
				, (transportBO.Sailing != null) ? transportBO.Sailing.JX_JA_DGFCLReceivalCommences : ZDateTime.Empty
				, transportBO.JW_TerminalCutOff, transportBO.JW_DepotCutOff
				, (transportBO.Sailing != null) ? transportBO.Sailing.JX_JA_DGFCLCutOff : ZDateTime.Empty
				, transportBO.JW_DepotAvailabilityDate
				, transportBO.JW_TerminalStorageDate, transportBO.JW_DepotStorageDate
				, transportBO.Carrier
				, transportBO.CreditorAddress != null ? transportBO.CreditorAddress.Header : null
				, transportBO.JW_CarrierBookingReference, transportBO.Voyage
				, factory)
		{
			fLoadOrgAddress = transportBO.DepartureLocation;
			fDestOrgAddress = transportBO.ArrivalLocation;
			transportType = new CodeAndDescriptionWrapper(transportBO.JW_TransportType, transportBO.JW_TransportType_List, Factory);
			serviceLevel = new CodeAndDescriptionWrapper(transportBO.JW_PL_NKCarrierServiceLevel, transportBO.CarrierServiceLevel_List, Factory);
			fCarrierOrgAddress = transportBO.CarrierAddress;
		}

		public RouteWrapper(ZInt legNo, JobDocAddress loadAddress, JobDocAddress destAddress, ZDateTime eTD, ZDateTime eTA, BusinessObjectFactory factory)
			: this(legNo, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty
				, ZString.Empty, loadAddress, eTD, ZDateTime.Empty
				, ZString.Empty, destAddress, eTA, ZDateTime.Empty
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty
				, ZDateTime.Empty
				, ZDateTime.Empty, ZDateTime.Empty
				, null, null, ZString.Empty, null, factory)
		{
		}

		public LocationWrapper Origin
		{
			get { return fOrigin ?? (fOrigin = new LocationWrapper(fLoadUNLOCO, Factory)); }
		}

		public AddressWrapper OriginAddress
		{
			get { return fOriginAddress ?? (fOriginAddress = ((fLoadJobDocAddress == null) ? new AddressWrapper(fLoadOrgAddress, ContactType.NoContactType, Factory) : new AddressWrapper(OrganisationUsageType.Origin, fLoadJobDocAddress, Factory))); }
		}

		public OrganisationWrapper OriginOrganisation
		{
			get { return fOriginOrganisation ?? (fOriginOrganisation = GetOriginOrganisation()); }
		}

		OrganisationWrapper GetOriginOrganisation()
		{
			var organisation = fLoadJobDocAddress == null ? (fLoadOrgAddress == null ? null : fLoadOrgAddress.Header) : fLoadJobDocAddress.Organisation;
			return new OrganisationWrapper(OrganisationUsageType.Origin, organisation, ContactType.NoContactType, Factory);
		}

		public LocationWrapper Destination
		{
			get { return fDestination ?? (fDestination = new LocationWrapper(fDestUNLOCO, Factory)); }
		}

		public AddressWrapper DestinationAddress
		{
			get { return fDestinationAddress ?? (fDestinationAddress = ((fDestJobDocAddress == null) ? new AddressWrapper(fDestOrgAddress, ContactType.NoContactType, Factory) : new AddressWrapper(fDestJobDocAddress, Factory))); }
		}

		public OrganisationWrapper DestinationOrganisation
		{
			get { return fDestinationOrganisation ?? (fDestinationOrganisation = GetDestinationOrganisation()); }
		}

		OrganisationWrapper GetDestinationOrganisation()
		{
			var organisation = fDestJobDocAddress == null ? (fDestOrgAddress == null ? null : fDestOrgAddress.Header) : fDestJobDocAddress.Organisation;
			return new OrganisationWrapper(OrganisationUsageType.Destination, organisation, ContactType.NoContactType, Factory);
		}

		public ZInt LegNo
		{
			get { return fLegNo; }
		}

		public ZDateTime ActualArrival
		{
			get { return fATA; }
		}

		public ZDateTime ActualDeparture
		{
			get { return fATD; }
		}

		public ZDateTime EstimatedArrival
		{
			get { return fETA; }
		}

		public ZDateTime EstimatedDeparture
		{
			get { return fETD; }
		}

		public ZDateTime DateOfArrival
		{
			get { return GetBestValueWithFallback(fATA, fETA); }
		}

		public ZDateTime DateOfDeparture
		{
			get { return GetBestValueWithFallback(fATD, fETD); }
		}

		public ZDateTime AvailabilityDate
		{
			get { return availabilityDate; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return fFCLReceivalCommences; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return fLCLReceivalCommences; }
		}

		public ZDateTime DGFCLReceivalCommences
		{
			get { return fDGFCLReceivalCommences; }
		}

		public ZDateTime FCLCutOff
		{
			get { return fFCLCutOff; }
		}

		public ZDateTime LCLCutOff
		{
			get { return fLCLCutOff; }
		}

		public ZDateTime DGFCLCutOff
		{
			get { return fDGFCLCutOff; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return fLCLAvailabilityDate; }
		}

		public ZDateTime FCLStorageCommences
		{
			get { return fFCLStorageDate; }
		}

		public ZDateTime LCLStorageCommences
		{
			get { return fLCLStorageDate; }
		}

		public OrganisationWrapper Carrier
		{
			get { return fCarrier ?? (fCarrier = new OrganisationWrapper(OrganisationUsageType.Carrier, fCarrierBO, ContactType.ShippingLine, Factory)); }
		}

		public AddressWrapper CarrierAddress
		{
			get { return fCarrierAddress ?? (fCarrierAddress = ((fCarrierOrgAddress == null) ? null : new AddressWrapper(fCarrierOrgAddress, ContactType.NoContactType, Factory))); }
		}

		public OrganisationWrapper Creditor
		{
			get { return creditor ?? (creditor = new OrganisationWrapper(OrganisationUsageType.Creditor, creditorBO, ContactType.Payables, Factory)); }
		}

		public ZString CarriersReference
		{
			get { return fCarrierBookingRef; }
		}

		public CodeAndDescriptionWrapper ServiceLevel
		{
			get { return serviceLevel; }
		}

		readonly CodeAndDescriptionWrapper serviceLevel;

		public TransportWrapper Transport
		{
			get { return fTransport ?? (fTransport = new TransportWrapper(fTransportModeCode, fVessel, fVoyageFlight, fETD, fATD, Factory, ForceShowVesselVoyage)); }
		}

		public ZBool IsCargoOnly
		{
			get { return fIsCargoOnly; }
		}

		public ZBool IsInternational
		{
			get
			{
				return !fLoadUNLOCO.IsEmpty && !fDestUNLOCO.IsEmpty && fLoadUNLOCO.Left(2) != fDestUNLOCO.Left(2);
			}
		}

		public CodeAndDescriptionWrapper TransportType
		{
			get
			{
				return transportType;
			}
		}
		readonly CodeAndDescriptionWrapper transportType;

		#region Implementation
		readonly ZInt fLegNo;
		readonly ZString fTransportModeCode;
		readonly ZString fAdditionalTransportModeCode;
		readonly ZString fVessel;
		readonly ZString fVoyageFlight;
		TransportWrapper fTransport;

		readonly ZString fLoadUNLOCO;
		LocationWrapper fOrigin;
		readonly JobDocAddress fLoadJobDocAddress;
		readonly OrgAddress fLoadOrgAddress;
		AddressWrapper fOriginAddress;
		OrganisationWrapper fOriginOrganisation;
		readonly ZDateTime fETD;
		readonly ZDateTime fATD;

		readonly ZString fDestUNLOCO;
		LocationWrapper fDestination;
		readonly JobDocAddress fDestJobDocAddress;
		readonly OrgAddress fDestOrgAddress;
		AddressWrapper fDestinationAddress;
		OrganisationWrapper fDestinationOrganisation;
		readonly ZDateTime fETA;
		readonly ZDateTime fATA;
		readonly ZDateTime availabilityDate;
		readonly ZDateTime fFCLReceivalCommences;
		readonly ZDateTime fLCLReceivalCommences;
		readonly ZDateTime fDGFCLReceivalCommences;
		readonly ZDateTime fFCLCutOff;
		readonly ZDateTime fLCLCutOff;
		readonly ZDateTime fDGFCLCutOff;
		readonly ZDateTime fLCLAvailabilityDate;
		readonly ZDateTime fFCLStorageDate;
		readonly ZDateTime fLCLStorageDate;

		readonly OrgHeader fCarrierBO;
		readonly OrgHeader creditorBO;
		OrganisationWrapper fCarrier;
		OrganisationWrapper creditor;
		readonly OrgAddress fCarrierOrgAddress;
		AddressWrapper fCarrierAddress;
		readonly ZString fCarrierBookingRef;
		readonly ZBool fIsCargoOnly;
		ZBool forceShowVesselVoyage;

		ZDateTime DepartureDate
		{
			get { return fATD.IsEmpty ? fETD : fATD; }
		}

		#endregion

		ZDateTime IMovementLeg.ArrivalDate
		{
			get { return ActualArrival.IsValid ? ActualArrival : EstimatedArrival; }
		}

		ZDateTime IMovementLeg.DepartureDate
		{
			get { return DepartureDate; }
		}

		ZString IMovementLeg.Discharge
		{
			get { return Destination.UNLOCO; }
		}

		ZString IMovementLeg.Load
		{
			get { return Origin.UNLOCO; }
		}

		ZString IMovementLeg.TransportMode
		{
			get { return fTransportModeCode; }
		}

		public ZString AdditionalTransportMode => fAdditionalTransportModeCode;

		public ZBool ForceShowVesselVoyage
		{
			get { return forceShowVesselVoyage; }
			set { forceShowVesselVoyage = value; }
		}
	}
}
