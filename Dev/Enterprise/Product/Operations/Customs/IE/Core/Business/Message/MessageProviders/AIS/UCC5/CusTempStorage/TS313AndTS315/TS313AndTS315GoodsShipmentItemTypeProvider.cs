using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using IContainerAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer, string>;
using TemporaryStorageContainer = Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS313AndTS315GoodsShipmentItemTypeProvider : ITS313AndTS315GoodsShipmentItemType, IContainerAggregation
	{
		public TS313AndTS315GoodsShipmentItemTypeProvider(TS313AndTS315GoodsShipmentTypeProvider header, TemporaryStoragePackedItem packedItem)
		{
			this.header = header;
			this.packedItem = packedItem;
		}
		readonly TS313AndTS315GoodsShipmentTypeProvider header;
		protected readonly TemporaryStoragePackedItem packedItem;

		public string GoodsItemNumber => packedItem.API_LineNo.ToString();

		public ITSGoodsShipmentItemTypeGoodsInformation GoodsInformation => CachedValueHelper.GetValue(ref goodsInformation, () => TSGoodsShipmentItemTypeGoodsInformationProvider.New(packedItem));
		CachedValue<ITSGoodsShipmentItemTypeGoodsInformation> goodsInformation;

		public IReadOnlyCollection<string> ContainerIds
		{
			get
			{
				if (containerIds == null)
				{
					header.AggregateDocuments();
				}
				return containerIds;
			}
		}
		IReadOnlyCollection<string> containerIds;

		public ITSGoodsShipmentItemTypeDocumentsAuthorisations DocumentsAuthorisations => CachedValueHelper.GetValue(ref documentsAuthorisationsCached, () => new TSGoodsShipmentItemTypeDocumentsAuthorisationsProvider(packedItem));
		CachedValue<ITSGoodsShipmentItemTypeDocumentsAuthorisations> documentsAuthorisationsCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> SupplyChainActors => additionalSupplyChainActorsCached ??= packedItem.SupplyChainActors.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray();
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActorsCached;

		#region Container Aggregation

		IEnumerable<TemporaryStorageContainer> IContainerAggregation.GetDocumentObjects()
		{
			var result = packedItem.TemporaryStorageLinkPackages.Where(link => link.IsLinked).Select(link => link.Package?.Container).WhereNotNull();
			return result;
		}

		void IContainerAggregation.SetDocuments(IReadOnlyCollection<string> documents) => containerIds = documents;

		#endregion
	}
}
