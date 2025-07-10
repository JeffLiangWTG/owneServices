using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using IContainerAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer, string>;
using IContainerAggregationHeader = Enterprise.Customs.IE.Business.AIS.IDocumentAggregationHeader<Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer, string>;
using TemporaryStorageContainer = Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS313AndTS315GoodsShipmentTypeProvider : ITS313AndTS315GoodsShipmentType, IContainerAggregationHeader
	{
		internal TS313AndTS315GoodsShipmentTypeProvider(TemporaryStorageHeader header)
		{
			this.header = header;
			masterBill = header.MasterBill;
		}
		protected readonly TemporaryStorageHeader header;
		protected readonly TemporaryStorageBill masterBill;

		public ITSGoodsShipmentTypeDocumentsAuthorisations DocumentsAuthorisations
			=> CachedValueHelper.GetValue(ref documentsAuthorisationsCached, () => new TSGoodsShipmentTypeDocumentsAuthorisationsProvider(header));
		CachedValue<ITSGoodsShipmentTypeDocumentsAuthorisations> documentsAuthorisationsCached;

		public IGoodsLocation GoodsLocation
			=> CachedValueHelper.GetValue(ref goodsLocationCached, () => header.GoodsLocation is CusGoodsLocation goodsLocation ? new GoodsLocationProvider(goodsLocation) : null);
		CachedValue<IGoodsLocation> goodsLocationCached;

		public ITS313AndTS315GoodsShipmentTypeTransportInformation TransportInformation
		{
			get
			{
				if (transportInformationCached == null)
				{
					transportInformationCached = new TS313AndTS315GoodsShipmentTypeTransportInformationProvider(header, header.ArrivalTransportMeans);
					this.AggregateDocuments();
				}
				return transportInformationCached;
			}
		}
		TS313AndTS315GoodsShipmentTypeTransportInformationProvider transportInformationCached;

		public ITSPresentation Presentation => CachedValueHelper.GetValue(ref presentation, () => TSPresentationProvider.New(header));
		CachedValue<ITSPresentation> presentation;

		public decimal GrossMass => header.MasterBill.GrossWeightInKilogramsSafe;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> SupplyChainActors => additionalSupplyChainActor ??= masterBill.SupplyChainActors.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray();
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		public IReadOnlyCollection<ITS313AndTS315GoodsShipmentItemType> GoodsShipmentItem => goodsShipmentItemCached ??= masterBill.PackedItems.Select(p => new TS313AndTS315GoodsShipmentItemTypeProvider(this, p)).ToArray();
		IReadOnlyCollection<TS313AndTS315GoodsShipmentItemTypeProvider> goodsShipmentItemCached;

		#region Container Aggregation

		string[] IContainerAggregationHeader.GetDocumentKeys() => new[] { AsycudaContainer.Schema.ACN_ContainerNumber };

		IEnumerable<IContainerAggregation> IContainerAggregationHeader.GetItemProviders() => GoodsShipmentItem.Cast<IContainerAggregation>();

		string IContainerAggregationHeader.Create(TemporaryStorageContainer documentBiz) => documentBiz.ACN_ContainerNumber.ToString();

		IEnumerable<TemporaryStorageContainer> IContainerAggregation.GetDocumentObjects() => Enumerable.Empty<TemporaryStorageContainer>();

		void IContainerAggregation.SetDocuments(IReadOnlyCollection<string> documents) =>
			(transportInformationCached ??= new TS313AndTS315GoodsShipmentTypeTransportInformationProvider(header, header.ArrivalTransportMeans))
			.SetContainerIDs(documents);

		#endregion
	}
}
