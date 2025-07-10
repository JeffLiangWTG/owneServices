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
	public class ConsignmentMasterLevelWrapper : IConsignmentMasterLevel
	{
		ConsignmentMasterLevelWrapper(TemporaryStorageBill bill)
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

		public ICollection<IConsignmentItem> ConsignmentItemMasterLevel => consignmentItemMasterLevel ?? (consignmentItemMasterLevel = GetConsignmentItemMasterLevel());
		ICollection<IConsignmentItem> consignmentItemMasterLevel;

		ICollection<IConsignmentItem> GetConsignmentItemMasterLevel()
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

		public ICollection<IReceptacle> Receptacle => receptacle ?? (receptacle = GetReceptacle());
		ICollection<IReceptacle> receptacle;

		//TODO: waiting to check if it is neccesary
		ICollection<IReceptacle> GetReceptacle()
		{
			return new Collection<IReceptacle>();
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

		public decimal TotalGrossMassValue => totalGrossMassValue.Equals(0M) ? (totalGrossMassValue = TotalGrossMass ?? 0M) : totalGrossMassValue;
		decimal totalGrossMassValue;

		public bool TotalGrossMassValueSpecified => totalGrossMassValueSpecified ? totalGrossMassValueSpecified : totalGrossMassValueSpecified = !TotalGrossMassValue.Equals(0M);
		bool totalGrossMassValueSpecified;

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

		public static ConsignmentMasterLevelWrapper New(TemporaryStorageBill bill) => bill == null ? null : new ConsignmentMasterLevelWrapper(bill);
	}
}
