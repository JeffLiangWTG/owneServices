using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using AISInterfaces = CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class ConsignmentItemProvider : IConsignmentItem
	{
		public static ConsignmentItemProvider New(TemporaryStoragePackedItem packedItem) => new ConsignmentItemProvider(packedItem);

		public ConsignmentItemProvider(TemporaryStoragePackedItem packedItem)
		{
			this.packedItem = Argument.NotNull(packedItem, nameof(packedItem));
		}
		readonly TemporaryStoragePackedItem packedItem;

		public string GoodsItemNumber => packedItem.SequenceNumber.ToString();

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActorsCached ?? (additionalSupplyChainActorsCached = packedItem.SupplyChainActors.Select(chainActor => new AdditionalSupplyChainActorProvider(chainActor)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActorsCached;

		public ICommodity09 Commodity => CachedValueHelper.GetValue(ref commodityCached, () => Commodity09Provider.New(packedItem));
		CachedValue<ICommodity09> commodityCached;

		public decimal GoodsMeasure => Core.Constants.Weight.ConvertSafe(packedItem.API_GrossWeight, packedItem.API_GrossWeightUQ, Core.Constants.Weight.Kilograms);

		public IReadOnlyCollection<IPackaging> Packagings => packagingCached ?? (packagingCached = packedItem.Pack is AsycudaPack pack ? new Business.PackagingProvider[] { new Business.PackagingProvider(pack.APA_PackUQ, pack.APA_PackQty, pack.APA_MarksAndNumbers) } : Array.Empty<Business.PackagingProvider>());
		IReadOnlyCollection<IPackaging> packagingCached;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = packedItem.PreviousDocuments.Select((element, index) => PreviousDocumentProvider.New(element, index)).ToArray());
		public IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = packedItem.SupportingDocuments.Select((element, index) => DocumentProvider.New(element, index)).ToArray());
		IReadOnlyCollection<IDocument> supportingDocuments;

		IReadOnlyCollection<AISInterfaces.ITransportEquipment> transportEquipments;
		public IReadOnlyCollection<AISInterfaces.ITransportEquipment> TransportEquipments =>
			transportEquipments
			?? (transportEquipments = packedItem.TemporaryStorageLinkPackages.Select(linkPackage => linkPackage.Package?.Container).WhereNotNull().Select(TransportEquipmentProvider.New).ToArray());

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations = packedItem.AdditionalInfos.Where(x => x.IsAnAdditionalInformation).Select(AdditionalInformationProvider.New).ToArray());
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = packedItem.AdditionalInfos.Where(x => x.IsAnAdditionalReference).Select((element, index) => DocumentProvider.New(element, index)).ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;
	}
}
