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
	public class ConsignmentItemWrapper : IConsignmentItem
	{
		ConsignmentItemWrapper(TemporaryStoragePackedItem item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		readonly TemporaryStoragePackedItem item;

		public ICollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformation());
		ICollection<IAdditionalInformation> additionalInformation;

		ICollection<IAdditionalInformation> GetAdditionalInformation()
		{
			var result = new Collection<IAdditionalInformation>();

			item.AdditionalInfos.Cast<AdditionalInfo>()
				.Where(y => y.IsAnAdditionalInformation)
				.ForEach(additionalInfo => result.Add(AdditionalInformationWrapper.New(additionalInfo)));

			return result;
		}

		public ICollection<IAdditionalReference> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReference());
		ICollection<IAdditionalReference> additionalReference;

		ICollection<IAdditionalReference> GetAdditionalReference()
		{
			var result = new Collection<IAdditionalReference>();

			item.AdditionalInfos.Cast<AdditionalInfo>()
				.Where(y => y.IsAnAdditionalReference)
				.ForEach(additionalInfo => result.Add(AdditionalReferenceWrapper.New(additionalInfo)));

			return result;
		}

		public ICollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = GetAdditionalSupplyChainActor());
		ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		ICollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActor()
		{
			var result = new Collection<IAdditionalSupplyChainActor>();

			item.SupplyChainActors.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
				.ForEach(actor => result.Add(AdditionalSupplyChainActorWrapper.New(actor)));

			return result;
		}

		public ICommodity Commodity => commodity ?? (commodity = CommodityWrapper.New(item));
		ICommodity commodity;

		public string GoodsItemNumber => goodsItemNumber ?? (goodsItemNumber = item.API_LineNo.ToString());
		string goodsItemNumber;

		public ICollection<IPackaging> Packaging => packaging ?? (packaging = GetPackagingCollection());
		ICollection<IPackaging> packaging;

		ICollection<IPackaging> GetPackagingCollection()
			=> item.PackagesPivot.Select(s => PackagingWrapper.New(s.Pack)).Cast<IPackaging>().ToCollection();

		public IPreviousDocument PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocument());
		IPreviousDocument previousDocument;

		IPreviousDocument GetPreviousDocument()
		{
			var previousDocument = item.PreviousDocuments.FirstOrDefault();
			return PreviousDocumentWrapper.New(previousDocument);
		}

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument());
		ICollection<ISupportingDocument> supportingDocument;

		ICollection<ISupportingDocument> GetSupportingDocument()
		{
			var result = new Collection<ISupportingDocument>();

			item.SupportingDocuments.Cast<SupportingDocument>().ForEach(supportingDocument => result.Add(SupportingDocumentWrapper.New(supportingDocument)));

			return result;
		}

		public ICollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = GetTransportEquipmentCollection());
		ICollection<ITransportEquipment> transportEquipment;

		ICollection<ITransportEquipment> GetTransportEquipmentCollection()
			=> item.PackagesPivot.Select(s => s.Pack.Container)
						.Where(x => x != null)
						.Distinct()
						.Cast<TemporaryStorageContainer>()
						.Select(container => TransportEquipmentWrapper.New(container))
						.Cast<ITransportEquipment>()
						.ToCollection();

		public IWeight Weight => weight ?? (weight = WeightWrapper.New(item));
		IWeight weight;

		public static ConsignmentItemWrapper New(TemporaryStoragePackedItem item) => item == null ? null : new ConsignmentItemWrapper(item);
	}
}
