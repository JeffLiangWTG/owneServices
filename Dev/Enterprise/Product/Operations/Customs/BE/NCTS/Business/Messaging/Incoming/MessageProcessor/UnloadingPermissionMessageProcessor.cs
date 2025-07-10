using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public abstract class UnloadingPermissionMessageProcessor<TInterface> : NCTSMessageProcessor<TInterface> where TInterface : class, ICC043CAndIENCTS043CDataProvider
{
	public UnloadingPermissionMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override BusinessObject FindParentOfMessage(BEMessage message, TInterface messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider, NctsMoveHeaderType.Codes.Arrival, null, NctsMovementType.Codes.Arrival);

	protected abstract override void PreProcessMessageWhenBOFoundCore(BEMessage message, TInterface messageDataProvider);

	protected override void ProcessMessageCore(BEMessage message, TInterface messageDataProvider)
	{
		var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		message.EM_Status = EDIMessage.Status.ProcessedOK;

		moveHeader.BM_InBondEntryType = messageDataProvider.DeclarationType;
		moveHeader.BM_EntryDate = messageDataProvider.DeclarationAcceptanceDate;
		moveHeader.BM_TypeOfSecurity = messageDataProvider.Security;
		moveHeader.BM_ReducedDatasetIndicator = messageDataProvider.ReducedDatasetIndicator;

		var consignment = messageDataProvider.Consignment;
		if (consignment != null)
		{
			ProcessConsignment(moveHeader, consignment, nctsHeader);
		}

		CreateCustomsRegistryNumber(nctsHeader);
	}

	static void ProcessConsignment(NctsArrivalMovementHeader moveHeader, ConsignmentXmlProvider consignment, NctsHeader nctsHeader)
	{
		moveHeader.BM_RL_NKDestinationPort = consignment.CountryOfDestination;
		moveHeader.BM_InlandTransportMode = consignment.InlandModeOfTransport;
		moveHeader.BM_GrossWeight = consignment.GrossMass;

		nctsHeader.ArrivalHeaderContainers.RemoveAndDeleteAll();
		nctsHeader.ArrivalMovementHeader.BM_SealQty = 0;
		foreach (var equipment in consignment.TransportEquipments)
		{
			CreateContainer(equipment, nctsHeader, consignment.ContainerIndicator);
		}

		CreateDocumentsOnMovementHeader(moveHeader, consignment);

		foreach (var incident in consignment.Incidents)
		{
			CreateIncident(incident, nctsHeader, consignment.ContainerIndicator);
		}

		foreach (var houseConsignment in consignment.HouseConsignments)
		{
			ProcessHouseConsignment(houseConsignment, nctsHeader, moveHeader);
		}

		LinkEquipmentAndGoodItems(consignment.TransportEquipments, nctsHeader);
	}

	static void CreateCustomsRegistryNumber(NctsHeader nctsHeader)
	{
		var destionationTraderPK = nctsHeader.DestinationTrader.Organisation?.PK;
		if (destionationTraderPK != null)
		{
			var customsRegistryItem = RegistryHelper.GetValidCustomsRegistryForCompany(BECustomsRegistry.Instance.CustomsRegistry.Value, BERegistryDeclarationTypeList.Codes.TransitArrival, nctsHeader.DestinationTrader.Organisation.PK);

			if (customsRegistryItem != null)
			{
				var registryNumber = nctsHeader.Factory.New<CusEntryNumberForCC043CMessage>();
				registryNumber.Parent = nctsHeader.ArrivalMovementHeader;
				registryNumber.CE_EntryType = CusEntryNumberTypes.EU.CustomsRegistry;
				registryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
				registryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				registryNumber.CE_EntryLineReference = BERegistryDeclarationTypeList.Codes.TransitArrival + "-" + nctsHeader.DestinationTrader.Organisation.OH_Code;
				registryNumber.CE_IssueDate = customsRegistryItem.StartingDate;
			}
		}
	}

	static void LinkEquipmentAndGoodItems(IReadOnlyCollection<TransportEquipmentXmlProvider> equipments, NctsHeader nctsHeader)
	{
		foreach (var equipment in equipments)
		{
			var containerNumber = equipment.ContainerIdentificationNumber;
			var dbEquipments = nctsHeader.ArrivalHeaderContainers.Where(x => x.BC_ContainerNum == containerNumber);
			var dbEquipment = dbEquipments.SingleOrDefault();
			if (dbEquipment != null)
			{
				var goodsReferences = equipment.GoodsReferences.Select(x => x.DeclarationGoodsItemNumber).ToHashSet();
				var packages = nctsHeader.Bills
					.SelectMany(x => x.ArrivalGoodsItems)
					.Where(x => goodsReferences
					.Contains(x.BY_DeclarationGoodsItemNumber.ToString()))
					.SelectMany(x => x.Packages)
					.Where(x => !x.IsInDatabase);

				foreach (var package in packages)
				{
					var genPivot = nctsHeader.Factory.New<NctsCusInBondContainerPackageGenPivot>();
					genPivot.XX_Relation1ID = package.PK;
					genPivot.XX_Relation2ID = dbEquipment.PK;
				}
			}
		}
	}

	static void CreateIncident(IncidentXmlProvider incident, NctsHeader parent, ZBool containerIndicator)
	{
		parent.BH_ExportFlag = EventFlagList.Codes.Yes;
		var enRouteIncident = parent.EnRouteIncidents.AddNew();
		enRouteIncident.BN_IncidentCode = incident.Code;
		enRouteIncident.BN_Information = incident.Text;
		enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.CUS;

		if (incident.Endorsement is EndorsementXmlProvider endorsement)
		{
			enRouteIncident.BN_EndorsementDate = endorsement.Date;
			enRouteIncident.BN_EndorsementAuthority = endorsement.Authority;
			enRouteIncident.BN_EndorsementPlace = endorsement.Place;
			enRouteIncident.BN_EndorsementCountryCode = endorsement.Country;
		}

		if (incident.Location is LocationXmlProvider location)
		{
			enRouteIncident.BN_LocationQualifier = location.QualifierOfIdentification;
			enRouteIncident.BN_EventPlace = location.UNLocode;
			enRouteIncident.BN_EventCountryCode = location.Country;
			enRouteIncident.GoodsLocation.CGL_Qualifier = location.QualifierOfIdentification;

			if (location.Address is AddressXmlProvider address)
			{
				enRouteIncident.GoodsLocation.Address.E2_Address1 = address.StreetAndNumber;
				enRouteIncident.GoodsLocation.Address.E2_Postcode = address.Postcode;
				enRouteIncident.GoodsLocation.Address.E2_City = address.City;
			}

			if (!location.UNLocode.IsNullOrEmpty())
			{
				enRouteIncident.GoodsLocation.Unlocode = location.UNLocode;
			}

			if (location.GNSS is GNSSXmlProvider gnss && !gnss.Longitude.IsNullOrEmpty() && !gnss.Latitude.IsNullOrEmpty())
			{
				enRouteIncident.GoodsLocation.Address.E2_Latitude = double.Parse(gnss.Latitude);
				enRouteIncident.GoodsLocation.Address.E2_Longitude = double.Parse(gnss.Longitude);
				enRouteIncident.BN_GeoLocation = ZGeography.CreatePoint(double.Parse(gnss.Longitude), double.Parse(gnss.Latitude));
			}
		}

		if (incident.Transhipment is TranshipmentXmlProvider transhipment && transhipment.TransportMeans is TransportMeansXmlProvider transportMeans)
		{
			enRouteIncident.BN_TransportAtDepartureType = transportMeans.TypeOfIdentification;
			enRouteIncident.BN_TransportAtDepartureID = transportMeans.IdentificationNumber;
			enRouteIncident.BN_RN_NKTransportAtDepartureIDNationality = transportMeans.Nationality;
		}

		foreach (var equipment in incident.TransportEquipments)
		{
			CreateContainer(equipment, enRouteIncident, containerIndicator);
		}
	}

	static void CreateDocumentsOnMovementHeader(NctsArrivalMovementHeader moveHeader, ConsignmentXmlProvider xmlConsignment)
	{
		foreach (var means in xmlConsignment.TransportMeans)
		{
			CreateTransportMean(means, moveHeader);
		}

		foreach (var xmlDocument in xmlConsignment.PreviousDocuments)
		{
			CreateDocument(moveHeader.Header.PreviousDocuments, xmlDocument);
		}

		foreach (var xmlDocument in xmlConsignment.SupportingDocuments)
		{
			CreateDocument(moveHeader.SupportingDocuments, xmlDocument);
		}

		foreach (var xmlDocument in xmlConsignment.TransportDocuments)
		{
			CreateDocument(moveHeader.AdditionalDocuments, xmlDocument, Constants.CusSupportingInfoSubTypes.TransportDocument);
		}

		foreach (var xmlDocument in xmlConsignment.AdditionalInformation)
		{
			CreateDocument(moveHeader.AdditionalDocuments, xmlDocument, Constants.CusSupportingInfoSubTypes.AdditionalInformation);
		}

		foreach (var xmlDocument in xmlConsignment.AdditionalReference)
		{
			CreateDocument(moveHeader.AdditionalDocuments, xmlDocument, Constants.CusSupportingInfoSubTypes.AdditionalReference);
		}
	}

	static void CreateDocument<T>(ICusSupportingInfoCollection<T> documentCollection, DocumentXmlProvider xmlDocument, string subType = null) where T : CusSupportingInfo
	{
		var document = documentCollection.AddNew();
		document.CSI_LineNo = ZInt.ParseEmptyAsZero(xmlDocument.SequenceNumber);
		document.CSI_Code = xmlDocument.Code;
		document.CSI_SubType = subType;
		document.CSI_ReferenceNumber = xmlDocument.ReferenceNumber;
		document.CSI_ReferenceNumber2 = xmlDocument.ReferenceNumber2;
		document.CSI_Description = xmlDocument.Description;
		document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
	}

	static void CreateTransportMean(TransportMeansXmlProvider xmlTransportMean, NctsBill bill)
	{
		var transportMean = bill.ArrivalTransportInfos.AddNew();
		CreateTransportMeanDetail(xmlTransportMean, transportMean);
	}

	static void CreateTransportMean(TransportMeansXmlProvider xmlTransportMean, NctsArrivalMovementHeader moveheader)
	{
		var transportMean = moveheader.ArrivalTransportInfos.AddNew();
		CreateTransportMeanDetail(xmlTransportMean, transportMean);
	}

	static void CreateTransportMeanDetail(TransportMeansXmlProvider xmlTransportMean, ArrivalCusTransportMeans transportMean)
	{
		transportMean.TPM_SequenceNumber = ZShort.ParseSafe(xmlTransportMean.SequenceNumber, ZShort.Zero);
		transportMean.TPM_TypeOfIdentification = xmlTransportMean.TypeOfIdentification;
		transportMean.TPM_IdentificationNumber = xmlTransportMean.IdentificationNumber;
		transportMean.TPM_RN_NKTransportNationality = xmlTransportMean.Nationality;
		transportMean.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
	}

	static void CreateContainer(TransportEquipmentXmlProvider equipment, NctsHeader header, ZBool containerIndicator)
	{
		var container = header.ArrivalHeaderContainers.AddNew();
		container.BC_SequenceNumber = ZShort.Parse(equipment.SequenceNumber);
		container.BC_ContainerNum = equipment.ContainerIdentificationNumber;
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		container.BC_Mode = containerIndicator && !container.BC_ContainerNum.IsEmpty ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;

		foreach (var xmlSeal in equipment.Seals)
		{
			CreateSeal(xmlSeal, container, header);
		}
	}

	static void CreateContainer(TransportEquipmentXmlProvider equipment, EnRouteIncident incident, ZBool containerIndicator)
	{
		NctsContainer container = null;
		if (!equipment.ContainerIdentificationNumber.IsNullOrEmpty())
		{
			container = incident.IncidentContainers.AddNew();
			container.BC_SequenceNumber = ZShort.Parse(equipment.SequenceNumber);
			container.BC_ContainerNum = equipment.ContainerIdentificationNumber;
			container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			container.BC_Mode = containerIndicator ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;

			foreach (var item in equipment.GoodsReferences)
			{
				CreateGoodsReference(item, container);
			}
		}

		foreach (var xmlSeal in equipment.Seals)
		{
			CreateSeal(xmlSeal, equipment.ContainerIdentificationNumber.IsNullOrEmpty() ? null : container, incident);
		}
	}

	static void CreateSeal(SealXmlProvider xmlSeal, NctsArrivalHeaderContainer container, NctsHeader header)
	{
		if (container != null)
		{
			if (container.BC_Seal1.IsEmpty)
			{
				container.BC_Seal1 = xmlSeal.Identifier;
			}
			if (container.BC_Seal2.IsEmpty)
			{
				container.BC_Seal2 = xmlSeal.Identifier;
			}
		}

		var seal = container.Seals.AddNew();
		seal.BK_SequenceNumber = ZShort.Parse(xmlSeal.SequenceNumber);
		seal.BK_SealNumber = xmlSeal.Identifier;
		seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		header.ArrivalMovementHeader.BM_SealQty += 1;
	}

	static void CreateSeal(SealXmlProvider xmlSeal, NctsContainer container, EnRouteIncident incident)
	{
		if (container != null)
		{
			if (container.BC_Seal1.IsEmpty)
			{
				container.BC_Seal1 = xmlSeal.Identifier;
				return;
			}
			if (container.BC_Seal2.IsEmpty)
			{
				container.BC_Seal2 = xmlSeal.Identifier;
				return;
			}
		}

		var seal = container != null ? container.Seals.AddNew() : incident.Seals.AddNew();
		seal.BK_SequenceNumber = ZShort.Parse(xmlSeal.SequenceNumber);
		seal.BK_SealNumber = xmlSeal.Identifier;
	}

	static void CreateParty(JobDocAddress party, PartyXmlProvider providerParty)
	{
		if (providerParty != null)
		{
			var identification = providerParty.IdentificationNumber;
			if (!identification.IsNullOrEmpty())
			{
				party.E2_GovRegNum = identification;
				party.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			}
			party.E2_CompanyName = providerParty.Name;

			var address = providerParty.Address;

			party.E2_Address1 = address.StreetAndNumber;
			party.E2_Postcode = address.Postcode;
			party.E2_City = address.City;
			party.E2_RN_NKCountryCode = address.Country;
			party.E2_AddressOverride = true;
		}
	}

	static void ProcessHouseConsignment(HouseConsignmentXmlProvider houseConsignment, NctsHeader nctsHeader, NctsArrivalMovementHeader arrivalMovementHeader)
	{
		var bill = nctsHeader.Bills.AddNew();
		var moveDetail = bill.MovementDetail;

		moveDetail.B9_BM = arrivalMovementHeader.PK;
		moveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		moveDetail.B9_B9_InBondMoveDetail = ZGuid.Empty;
		moveDetail.B9_SeqNo = houseConsignment.SequenceNumber.ToString();

		ZDecimal grossMass = houseConsignment.GrossMass;
		ZString grossMassUnit = Core.Constants.Weight.Kilograms;

		if (grossMass.DecimalPlaces > 3)
		{
			grossMass *= 1000;
			grossMassUnit = Core.Constants.Weight.Grams;
		}
		bill.B0_Weight = grossMass;
		bill.B0_WeightUQ = grossMassUnit;
		bill.B0_SecurityIndicatorFromExport = houseConsignment.SecurityIndicatorFromExportDeclaration == "1";

		if (houseConsignment.Consignor is PartyXmlProvider consignor)
		{
			CreateParty(moveDetail.ConsignorDocAddress, consignor);
		}

		if (houseConsignment.Consignee is PartyXmlProvider consignee)
		{
			CreateParty(moveDetail.ConsigneeDocAddress, consignee);
		}

		foreach (var departureTransportMean in houseConsignment.DepartureTransportMeans)
		{
			CreateTransportMean(departureTransportMean, bill);
		}

		foreach (var xmlDocument in houseConsignment.PreviousDocuments)
		{
			CreateDocument(bill.PreviousDocuments, xmlDocument);
		}

		foreach (var xmlDocument in houseConsignment.SupportingDocuments)
		{
			CreateDocument(bill.SupportingDocuments, xmlDocument);
		}

		foreach (var xmlDocument in houseConsignment.TransportDocuments)
		{
			CreateDocument(bill.AdditionalDocuments, xmlDocument, Constants.CusSupportingInfoSubTypes.TransportDocument);
		}

		foreach (var xmlDocument in houseConsignment.AdditionalInformations)
		{
			CreateDocument(bill.AdditionalDocuments, xmlDocument, Constants.CusSupportingInfoSubTypes.AdditionalInformation);
		}

		foreach (var xmlDocument in houseConsignment.AdditionalReferences)
		{
			CreateDocument(bill.AdditionalDocuments, xmlDocument, Constants.CusSupportingInfoSubTypes.AdditionalReference);
		}

		foreach (var xmlItem in houseConsignment.ConsignmentItems)
		{
			CreateGoodsItem(bill, xmlItem);
		}
	}

	static void CreateGoodsItem(NctsBill bill, ConsignmentItemXmlProvider xmlItem)
	{
		var goodItem = bill.ArrivalGoodsItems.AddNew();

		goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		goodItem.BY_LineNo = ZShort.Parse(xmlItem.GoodsItemNumber);
		goodItem.BY_DeclarationGoodsItemNumber = xmlItem.DeclarationSequenceNumber;
		goodItem.BY_Type = xmlItem.DeclarationType;
		goodItem.BY_RN_NKCountryOfDestination = xmlItem.CountryOfDestination;
		goodItem.BY_Description = xmlItem.Commodity.DescriptionOfGoods;
		goodItem.BY_CusC4Number = xmlItem.Commodity.CusCode;
		goodItem.BY_HarmonisedTariff = xmlItem.Commodity.CommodityCode;

		foreach (var dangerousGood in xmlItem.Commodity.DangerousGoods)
		{
			CreateDangerousGood(goodItem, dangerousGood);
		}

		ZDecimal grossMass = xmlItem.Commodity.GrossWeight;
		ZString grossMassUnit = Core.Constants.Weight.Kilograms;

		if (grossMass.DecimalPlaces > 3)
		{
			grossMass *= 1000;
			grossMassUnit = Core.Constants.Weight.Grams;
		}
		goodItem.BY_GrossWeight = grossMass;
		goodItem.BY_GrossWeightUnit = grossMassUnit;

		ZDecimal netMass = xmlItem.Commodity.NetWeight;
		ZString netMassUnit = Core.Constants.Weight.Kilograms;

		if (netMass.DecimalPlaces > 3)
		{
			netMass *= 1000;
			netMassUnit = Core.Constants.Weight.Grams;
		}
		goodItem.BY_NetWeight = netMass;
		goodItem.BY_NetWeightUnit = netMassUnit;

		foreach (var xmlPackage in xmlItem.Packaging)
		{
			CreatePackage(goodItem, xmlPackage);
		}

		foreach (var xmlDocument in xmlItem.PreviousDocuments)
		{
			CreateDocument(goodItem.PreviousDocuments, xmlDocument);
		}

		foreach (var xmlDocument in xmlItem.SupportingDocuments)
		{
			CreateDocument(goodItem.SupportingDocuments, xmlDocument);
		}

		foreach (var xmlDocument in xmlItem.TransportDocuments)
		{
			CreateDocument(goodItem.AdditionalInfos, xmlDocument, Constants.CusSupportingInfoSubTypes.TransportDocument);
		}

		foreach (var xmlDocument in xmlItem.AdditionalInformations)
		{
			CreateDocument(goodItem.AdditionalInfos, xmlDocument, Constants.CusSupportingInfoSubTypes.AdditionalInformation);
		}

		foreach (var xmlDocument in xmlItem.AdditionalReferences)
		{
			CreateDocument(goodItem.AdditionalInfos, xmlDocument, Constants.CusSupportingInfoSubTypes.AdditionalReference);
		}
	}

	static void CreateDangerousGood(NctsArrivalCargoDesc goodItem, IDangerousGoodsProvider xmlDangerousGood)
	{
		var dangerousGood = goodItem.UNDGs.AddNew();

		dangerousGood.DI_DG_NKSubs = xmlDangerousGood.UNUmber;
	}

	static void CreatePackage(NctsArrivalCargoDesc goodItem, PackagingXmlProvider xmlPackage)
	{
		var package = goodItem.Packages.AddNew();

		package.B5_SequenceNumber = ZShort.Parse(xmlPackage.SequenceNumber.ToString());
		package.B5_UnitType = xmlPackage.TypeOfPackages;
		package.B5_UnitCount = xmlPackage.NumberOfPackages;
		package.B5_MarksAndNumbers = xmlPackage.ShippingMarks;
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
	}

	static void CreateGoodsReference(GoodsReferenceXmlProvider goodsItem, NctsContainer container)
	{
		var itemNumber = container.ItemNumbers.AddNew();
		itemNumber.CY_Order = ZShort.Parse(goodsItem.SequenceNumber);
		itemNumber.CY_Data = goodsItem.DeclarationGoodsItemNumber;
	}
}
