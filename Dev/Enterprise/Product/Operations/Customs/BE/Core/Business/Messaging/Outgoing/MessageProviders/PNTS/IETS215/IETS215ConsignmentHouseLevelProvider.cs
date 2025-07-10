using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class IETS215ConsignmentHouseLevelProvider : IIETS215ConsignmentHouseLevel
{
	public IETS215ConsignmentHouseLevelProvider(TemporaryStorageBill temporaryStorageBill)
	{
		this.temporaryStorageBill = Argument.NotNull(temporaryStorageBill, nameof(temporaryStorageBill));
	}
	readonly TemporaryStorageBill temporaryStorageBill;

	public string ReferenceNumUCR => temporaryStorageBill.ABL_UCRNumber;

	public decimal TotalGrossMass => temporaryStorageBill.ABL_GrossWeight;

	public IPNTSPartyWithNameAndCommunicationsAndAddress Consignor => CachedValueHelper.GetValue(ref consignorCached, GetConsignorCore);
	CachedValue<IPNTSPartyWithNameAndCommunicationsAndAddress> consignorCached;
	IPNTSPartyWithNameAndCommunicationsAndAddress GetConsignorCore() => !temporaryStorageBill.ABL_ShipperName.IsEmpty ? new PNTSPartyWithNameAndCommunicationsAndAddressProvider(temporaryStorageBill.Shipper, temporaryStorageBill.ABL_ShipperName, temporaryStorageBill.ABL_ShipperRegNoType, temporaryStorageBill.ABL_ShipperStreet1, temporaryStorageBill.ABL_ShipperStreet2, temporaryStorageBill.ABL_RN_NKShipperCountry, temporaryStorageBill.ABL_ShipperPostcode, temporaryStorageBill.ABL_ShipperCity) : null;

	public IPNTSPartyWithNameAndCommunicationsAndAddress Consignee => CachedValueHelper.GetValue(ref consigneeCached, GetConsigneeCore);
	CachedValue<IPNTSPartyWithNameAndCommunicationsAndAddress> consigneeCached;
	IPNTSPartyWithNameAndCommunicationsAndAddress GetConsigneeCore() => !temporaryStorageBill.ABL_ConsigneeName.IsEmpty ? new PNTSPartyWithNameAndCommunicationsAndAddressProvider(temporaryStorageBill.Consignee, temporaryStorageBill.ABL_ConsigneeName, temporaryStorageBill.ABL_ConsigneeRegNoType, temporaryStorageBill.ABL_ConsigneeStreet1, temporaryStorageBill.ABL_ConsigneeStreet2, temporaryStorageBill.ABL_RN_NKConsigneeCountry, temporaryStorageBill.ABL_ConsigneePostcode, temporaryStorageBill.ABL_ConsigneeCity) : null;

	public IPNTSPartyWithNameAndCommunicationsAndAddress Notify => CachedValueHelper.GetValue(ref notifyCached, GetNotifyCore);
	CachedValue<IPNTSPartyWithNameAndCommunicationsAndAddress> notifyCached;
	IPNTSPartyWithNameAndCommunicationsAndAddress GetNotifyCore() => !temporaryStorageBill.ABL_NotifyPartyName.IsEmpty ? new PNTSPartyWithNameAndCommunicationsAndAddressProvider(temporaryStorageBill.NotifyParty, temporaryStorageBill.ABL_NotifyPartyName, temporaryStorageBill.ABL_NotifyPartyRegNoType, temporaryStorageBill.ABL_NotifyPartyStreet1, temporaryStorageBill.ABL_NotifyPartyStreet2, temporaryStorageBill.ABL_RN_NKNotifyPartyCountry, temporaryStorageBill.ABL_NotifyPartyPostcode, temporaryStorageBill.ABL_NotifyPartyCity) : null;

	public IPNTSDocument TransportDocument => transportDocument ??= new PNTSDocumentProvider(temporaryStorageBill);
	IPNTSDocument transportDocument;

	public IReadOnlyCollection<IPNTSTransportEquipment> TransportEquipments => transportEquipments ??=
		GetLinkedContainers().Cast<TemporaryStorageContainer>().Select((x) => new PNTSTransportEquipmentProvider(x, temporaryStorageBill)).ToArray<IPNTSTransportEquipment>();
	IReadOnlyCollection<IPNTSTransportEquipment> transportEquipments;

	public IReadOnlyCollection<IPNTSAdditionalInformation> AdditionalInformations => additionalInformations ??=
		temporaryStorageBill.AdditionalInfos.Where(x => x.IsAnAdditionalInformation).Select((x) => new PNTSAdditionalInformationProvider(x)).ToArray<IPNTSAdditionalInformation>();
	IReadOnlyCollection<IPNTSAdditionalInformation> additionalInformations;

	public IReadOnlyCollection<IPNTSAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??=
		temporaryStorageBill.SupplyChainActors.Select((x) => new PNTSAdditionalSupplyChainActorProvider(x)).ToArray<IPNTSAdditionalSupplyChainActor>();
	IReadOnlyCollection<IPNTSAdditionalSupplyChainActor> additionalSupplyChainActors;

	public IReadOnlyCollection<IPNTSDocument> SupportingDocuments => supportingDocuments ??=
		temporaryStorageBill.SupportingDocuments.Select((x) => new PNTSSupportingDocumentProvider(x)).ToArray<IPNTSDocument>();
	IReadOnlyCollection<IPNTSDocument> supportingDocuments;

	public IReadOnlyCollection<IPNTSDocument> AdditionalReferences => additionalReferences ??=
	temporaryStorageBill.AdditionalInfos.Where(x => x.IsAnAdditionalReference).Select((x) => new PNTSAdditionalReferenceProvider(x)).ToArray<IPNTSDocument>();
	IReadOnlyCollection<IPNTSDocument> additionalReferences;

	public IReadOnlyCollection<IIETS215ConsignmentItemHouseLevel> ConsignmentItemHouseLevels => Array.Empty<IIETS215ConsignmentItemHouseLevel>();

	IEnumerable<TemporaryStorageContainer> GetLinkedContainers()
	{
		var distinctContainers = new ArrayList();
		foreach (TemporaryStoragePack package in temporaryStorageBill.Packs)
		{
			if (package.Container is TemporaryStorageContainer container)
			{
				var containerPK = container.PK;
				if (!distinctContainers.Contains(container.PK))
				{
					distinctContainers.Add(containerPK);
					yield return container;
				}
			}
		}
	}
}
