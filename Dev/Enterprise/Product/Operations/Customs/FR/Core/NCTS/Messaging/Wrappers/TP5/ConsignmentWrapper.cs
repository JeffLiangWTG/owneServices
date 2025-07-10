using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class ConsignmentWrapper : IConsignment
	{
		protected ConsignmentWrapper(EU.NCTS.Business.NctsCommonMovementHeader movementHeader, OrgHeader actualConsignee)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
			this.actualConsignee = actualConsignee;
		}

		readonly EU.NCTS.Business.NctsCommonMovementHeader movementHeader;
		readonly OrgHeader actualConsignee;

		public static ConsignmentWrapper New(EU.NCTS.Business.NctsCommonMovementHeader movementHeader, OrgHeader actualConsignee = null) => movementHeader == null ? null : new ConsignmentWrapper(movementHeader, actualConsignee);

		NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = (NctsHeader)movementHeader.Header);
		NctsHeader nctsHeader;
		public string CountryOfDispatch => countryOfDispatch ?? (countryOfDispatch = movementHeader.BM_RN_NKCountryOfDispatch);
		string countryOfDispatch;

		public string CountryOfDestination => countryOfDestination ?? (countryOfDestination = movementHeader.BM_RL_NKDestinationPort);
		string countryOfDestination;

		public bool ContainerIndicator => NctsHeader.Bills.Any(x => x.IsContainerised);

		public string InlandModeOfTransport => inlandModeOfTransport ?? (inlandModeOfTransport = movementHeader is NctsDepartureMovementHeader departureMovementHeader ? departureMovementHeader.InlandTransportModeAtDeparture : string.Empty);
		string inlandModeOfTransport;

		public string ModeOfTransportAtTheBorder => modeOfTransportAtTheBorder ?? (modeOfTransportAtTheBorder = movementHeader.BM_ExportTransportMode);
		string modeOfTransportAtTheBorder;

		public virtual decimal? GrossMass => grossMass ?? (grossMass = movementHeader.BM_GrossWeight);
		decimal? grossMass;

		public string ReferenceNumberUCR => referenceNumberUCR ?? (referenceNumberUCR = movementHeader.BM_UniqueConsignmentReference);
		string referenceNumberUCR;

		public IOrganizationWithContact Carrier => carrier ?? (carrier = movementHeader is NctsDepartureMovementHeader departureMovementHeader ? OrganizationWithContactWrapper.New(departureMovementHeader.Carrier.Organisation) : null);
		IOrganizationWithContact carrier;

		public IOrganizationWithContact Consignor => consignor ?? (consignor = OrganizationWithContactWrapper.New(NctsHeader.Consignor.Organisation));
		IOrganizationWithContact consignor;

		public ICollection<IIncident> Incident => incident ?? (incident = GetEnRouteIncidents());
		ICollection<IIncident> incident;

		public ICollection<IIncident> GetEnRouteIncidents()
		{
			var enRouteEvents = new Collection<IIncident>();
			NctsHeader.EnRouteIncidents.ForEach(x => enRouteEvents.Add(EnRouteIncidentWrapper.New(x)));
			return enRouteEvents;
		}

		public IOrganization Consignee => consignee ?? (consignee = GetConsignee());
		IOrganization consignee;
		IOrganization GetConsignee() => actualConsignee == null ? OrganizationWrapper.New(NctsHeader.Consignee.Organisation) : OrganizationWrapper.New(actualConsignee);

		public ICollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = GetAdditionalSupplyChainActors());
		ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;
		ICollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
		{
			var result = new Collection<IAdditionalSupplyChainActor>();
			NctsHeader.CusSupplyChainActors.Cast<CusReference>().ForEach(x => result.Add(AdditionalSupplyChainActorWrapper.New(x)));
			return result;
		}

		public ICollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = GetTransportEquipments());
		ICollection<ITransportEquipment> transportEquipment;
		protected virtual ICollection<ITransportEquipment> GetTransportEquipments()
		{
			var result = new Collection<ITransportEquipment>();
			if (movementHeader is NctsDepartureMovementHeader departureMovementHeader)
			{
				NctsHeader.DepartureHeaderContainers.ForEach(x => result.Add(TransportEquipmentWrapper.New(x, GetLinkedGoodsItemNumbers(x.BC_ContainerNum))));
			}
			else if (movementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				NctsHeader.ArrivalHeaderContainers.Cast<EU.NCTS.Business.NctsArrivalHeaderContainer>().ForEach(x => result.Add(TransportEquipmentWrapper.New(x, GetArrivalLinkedGoodsItemNumbers(x.BC_ContainerNum))));
			}
			return result;
		}

		protected virtual ICollection<string> GetLinkedGoodsItemNumbers(ZString containerNumber)
		{
			return NctsHeader.Bills.SelectMany(x => x.GoodsItems)
				.Where(x => x.ContainersSelected.Contains(containerNumber))
				.Select(x => x.BY_DeclarationGoodsItemNumber.ToString())
				.ToHashSet();
		}

		protected virtual ICollection<string> GetArrivalLinkedGoodsItemNumbers(ZString containerNumber)
		{
			return NctsHeader.Bills.SelectMany(x => x.ArrivalGoodsItems)
			.Where(x => x.Packages.Cast<EU.NCTS.Business.NctsPackage>().SelectMany((EU.NCTS.Business.NctsPackage x) => x.ContainersSelected).Contains(containerNumber))
			.Select(x => x.BY_DeclarationGoodsItemNumber.ToString())
			.ToHashSet();
		}

		public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = movementHeader is NctsDepartureMovementHeader departureMovementHeader ? LocationOfGoodsWrapper.New(departureMovementHeader.GoodsLocation) : LocationOfGoodsWrapper.New(((NctsArrivalMovementHeader)movementHeader).GoodsLocation));
		ILocationOfGoods locationOfGoods;

		public ICollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
		ICollection<IDepartureTransportMeans> departureTransportMeans;
		protected virtual ICollection<IDepartureTransportMeans> GetDepartureTransportMeans()
		{
			var result = new Collection<IDepartureTransportMeans>();
			if (movementHeader is NctsDepartureMovementHeader departureMovementHeader)
			{
				result.Add(DepartureTransportMeansWrapper.New(departureMovementHeader));
			}
			else if (movementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				arrivalMovementHeader.ArrivalTransportInfos.Cast<EU.NCTS.Business.ArrivalCusTransportMeans>().ForEach(transportMeans => result.Add(ArrivalTransportMeansWrapper.New(transportMeans)));
			}
			return result;
		}

		public ICollection<ICountryOfRoutingofConsignment> CountryOfRoutingofConsignment => countryOfRoutingofConsignment ?? (countryOfRoutingofConsignment = GetCountriesOfRoutingofConsignment());
		ICollection<ICountryOfRoutingofConsignment> countryOfRoutingofConsignment;
		ICollection<ICountryOfRoutingofConsignment> GetCountriesOfRoutingofConsignment()
		{
			var result = new Collection<ICountryOfRoutingofConsignment>();
			NctsHeader.CountriesOfRouting.Cast<EU.NCTS.Business.CountryOfRouting>().ForEach(x => result.Add(CountryOfRoutingofConsignmentWrapper.New(x.CY_Data)));
			return result;
		}

		public ICollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = GetActiveBorderTransportMeans());
		ICollection<IActiveBorderTransportMeans> activeBorderTransportMeans;
		ICollection<IActiveBorderTransportMeans> GetActiveBorderTransportMeans()
		{
			var result = new Collection<IActiveBorderTransportMeans>();
			result.Add(ActiveBorderTransportMeansWrapper.New(movementHeader));
			return result;
		}

		public IPlace PlaceofLoading => placeofLoading ??= PlaceWrapper.New(movementHeader.BM_PortOfPresentationCode, movementHeader.BM_PlaceOfLoading);
		IPlace placeofLoading;

		public IPlace PlaceofUnloading => placeofUnloading ??= PlaceWrapper.New(movementHeader.BM_ForeignDestPortKCode, movementHeader.BM_PlaceOfUnloading);
		IPlace placeofUnloading;

		public ICollection<IDocumentWithComplement> PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocuments());
		ICollection<IDocumentWithComplement> previousDocument;
		ICollection<IDocumentWithComplement> GetPreviousDocuments()
		{
			var result = new Collection<IDocumentWithComplement>();
			NctsHeader.PreviousDocuments.Cast<CusSupportingInfo>().ForEach(document => result.Add(DocumentWithComplementWrapper.New(document)));
			return result;
		}

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocuments());
		ICollection<ISupportingDocument> supportingDocument;
		protected virtual ICollection<ISupportingDocument> GetSupportingDocuments()
		{
			var result = new Collection<ISupportingDocument>();
			NctsHeader.MovementHeader.SupportingDocuments.Cast<EU.NCTS.Business.NctsSupportingDocument>().ForEach(document => result.Add(SupportingDocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportDocuments());
		ICollection<IDocument> transportDocument;
		protected virtual ICollection<IDocument> GetTransportDocuments()
		{
			var result = new Collection<IDocument>();
			NctsHeader.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument).ForEach(document => result.Add(DocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IDocument> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReferences());
		ICollection<IDocument> additionalReference;
		protected virtual ICollection<IDocument> GetAdditionalReferences()
		{
			var result = new Collection<IDocument>();
			NctsHeader.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ForEach(document => result.Add(DocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformation());
		ICollection<IAdditionalInformation> additionalInformation;
		ICollection<IAdditionalInformation> GetAdditionalInformation()
		{
			var result = new Collection<IAdditionalInformation>();
			NctsHeader.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ForEach(document => result.Add(AdditionalInformationWrapper.New(document)));
			return result;
		}

		public ITransportCharges TransportCharges => transportCharges ?? (transportCharges = TransportChargesWrapper.New(movementHeader.BM_MethodOfPayment));
		ITransportCharges transportCharges;

		public ICollection<IHouseConsignment> HouseConsignment => houseConsignment ?? (houseConsignment = GetHouseConsignments());
		ICollection<IHouseConsignment> houseConsignment;
		protected virtual ICollection<IHouseConsignment> GetHouseConsignments()
		{
			var result = new Collection<IHouseConsignment>();
			NctsHeader.Bills.Cast<EU.NCTS.Business.NctsBill>().OrderBy(bill => bill.SequenceNumber).ForEach(bill => result.Add(HouseConsignmentWrapper.New(bill)));
			return result;
		}
	}
}
