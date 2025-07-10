using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageContracts;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class ConsignmentHouseLevelWrapper : IConsignmentHouseLevel
	{
		ConsignmentHouseLevelWrapper(TemporaryStorageBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly TemporaryStorageBill bill;

		public ICollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformation());
		ICollection<IAdditionalInformation> additionalInformation;

		ICollection<IAdditionalInformation> GetAdditionalInformation()
		{
			var result = new Collection<IAdditionalInformation>();

			bill.AdditionalInfos.Cast<AdditionalInfo>()
				.Where(y => y.IsAnAdditionalInformation)
				.ForEach(additionalInfo => result.Add(AdditionalInformationWrapper.New(additionalInfo)));

			return result;
		}

		public ICollection<IAdditionalReference> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReference());
		ICollection<IAdditionalReference> additionalReference;

		ICollection<IAdditionalReference> GetAdditionalReference()
		{
			var result = new Collection<IAdditionalReference>();

			bill.AdditionalInfos.Cast<AdditionalInfo>()
				.Where(y => y.IsAnAdditionalReference)
				.ForEach(additionalInfo => result.Add(AdditionalReferenceWrapper.New(additionalInfo)));

			return result;
		}

		public ICollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = GetAdditionalSupplyChainActor());
		ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		ICollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActor()
		{
			var result = new Collection<IAdditionalSupplyChainActor>();

			bill.SupplyChainActors.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
				.ForEach(actor => result.Add(AdditionalSupplyChainActorWrapper.New(actor)));

			return result;
		}

		public IConsignee Consignee => consignee ?? (consignee = ConsigneeWrapper.New(bill));
		IConsignee consignee;

		public ICollection<IConsignmentItem> ConsignmentItemHouseLevel => consignmentItemHouseLevel ?? (consignmentItemHouseLevel = GetConsignmentItemHouseLevel());
		ICollection<IConsignmentItem> consignmentItemHouseLevel;

		ICollection<IConsignmentItem> GetConsignmentItemHouseLevel()
			=> bill.PackedItems.Select(packedItem => ConsignmentItemWrapper.New(packedItem)).Cast<IConsignmentItem>().ToCollection();

		public IConsignor Consignor => consignor ?? (consignor = ConsignorWrapper.New(bill));
		IConsignor consignor;

		public INotifyParty NotifyParty => notifyParty ?? (notifyParty = NotifyPartyWrapper.New(bill));
		INotifyParty notifyParty;

		public IPreviousDocument PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocument());
		IPreviousDocument previousDocument;

		IPreviousDocument GetPreviousDocument()
		{
			var previousDocument = bill.PreviousDocuments.FirstOrDefault() ?? bill.Header.PreviousDocuments.FirstOrDefault();
			return PreviousDocumentWrapper.New(previousDocument);
		}

		public IReferenceNumberUCR ReferenceNumberUCR => referenceNumberUCR ?? (referenceNumberUCR = ReferenceNumberUCRWrapper.New(bill.ABL_UCRNumber));
		IReferenceNumberUCR referenceNumberUCR;

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument());
		ICollection<ISupportingDocument> supportingDocument;

		ICollection<ISupportingDocument> GetSupportingDocument()
		{
			var result = new Collection<ISupportingDocument>();

			bill.SupportingDocuments.Cast<SupportingDocument>().ForEach(supportingDocument => result.Add(SupportingDocumentWrapper.New(supportingDocument)));

			return result;
		}

		public decimal? TotalGrossMass => totalGrossMass ?? (totalGrossMass = bill.ABL_GrossWeight);
		decimal? totalGrossMass;

		public IDocument TransportDocument => transportDocument ?? (transportDocument = DocumentWrapper.New(bill));
		IDocument transportDocument;

		public ICollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = GetTransportEquipmentCollection());
		ICollection<ITransportEquipment> transportEquipment;

		ICollection<ITransportEquipment> GetTransportEquipmentCollection()
			=> bill.Packs.Select(s => s.Container)
				.Where(x => x != null)
				.Distinct()
				.Cast<TemporaryStorageContainer>()
				.Select(container => TransportEquipmentWrapper.New(container))
				.Cast<ITransportEquipment>()
				.ToCollection();

		public static ConsignmentHouseLevelWrapper New(TemporaryStorageBill bill) => bill == null ? null : new ConsignmentHouseLevelWrapper(bill);
	}
}
