using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using IContainerAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.Business.NonPersistentCusContainer, string>;
using IContainerAggregationHeader = Enterprise.Customs.IE.Business.AIS.IDocumentAggregationHeader<Enterprise.Customs.Business.NonPersistentCusContainer, string>;
using NonPersistentCusContainer = Enterprise.Customs.Business.NonPersistentCusContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentProvider : IIM413AndIM415GoodsShipmentType, IContainerAggregationHeader
	{
		public IM413AndIM415GoodsShipmentProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
			entryHeader = entryHeaderWrapper.EntryHeader;
		}
		readonly EntryHeaderWrapper entryHeaderWrapper;
		readonly CusEntryHeader entryHeader;

		public IGoodsShipmentTypeDocumentsAuthorisations GoodsShipmentTypeDocumentsAuthorisations => CachedValueHelper.GetValue(ref goodsShipmentTypeDocumentsAuthorisations, () => new IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProvider(entryHeaderWrapper));
		CachedValue<IGoodsShipmentTypeDocumentsAuthorisations> goodsShipmentTypeDocumentsAuthorisations;

		public IGoodsShipmentTypeParties Parties => CachedValueHelper.GetValue(ref goodsShipmentTypeParties, () => new IM413AndIM415GoodsShipmentTypePartiesProvider(entryHeaderWrapper));
		CachedValue<IGoodsShipmentTypeParties> goodsShipmentTypeParties;

		public IGoodsShipmentTypeValuationInformation ValuationInformation => CachedValueHelper.GetValue(ref valuationInformation, () => new GoodsShipmentTypeValuationInformationProvider(entryHeaderWrapper));
		CachedValue<IGoodsShipmentTypeValuationInformation> valuationInformation;

		public IGoodsShipmentTypeDatesPlaces DatesPlaces => CachedValueHelper.GetValue(ref datesPlaces, () => new IM413AndIM415GoodsShipmentTypeDatesPlacesProvider(entryHeaderWrapper));
		CachedValue<IGoodsShipmentTypeDatesPlaces> datesPlaces;

		public IGoodsShipmentTypeTransportInformation TransportInformation
		{
			get
			{
				if (transportInformationCached == null)
				{
					transportInformationCached = new IM413AndIM415GoodsShipmentTransportInformationProvider(entryHeaderWrapper);
					this.AggregateDocuments();
				}
				return transportInformationCached;
			}
		}
		IM413AndIM415GoodsShipmentTransportInformationProvider transportInformationCached;

		public string TransactionNature => entryHeader.RandomHeader.JZ_ValuationCode;

		public IReadOnlyCollection<IGoodsShipmentItemType> GoodsShipmentItem => goodsShipmentItem ??= entryHeader.MergedLines.Select(mergedLine => new IM413AndIM415GoodsShipmentItemProvider(this, mergedLine, entryHeaderWrapper)).ToArray();
		IReadOnlyCollection<IGoodsShipmentItemType> goodsShipmentItem;

		#region Container Aggregation

		string[] IContainerAggregationHeader.GetDocumentKeys() => new[]
		{
			NonPersistentCusContainer.Schema.ContainerNumber
		};

		IEnumerable<IContainerAggregation> IContainerAggregationHeader.GetItemProviders() => GoodsShipmentItem.Cast<IContainerAggregation>();

		string IContainerAggregationHeader.Create(NonPersistentCusContainer container) => container.ContainerNumber;

		IEnumerable<NonPersistentCusContainer> IContainerAggregation.GetDocumentObjects() => Enumerable.Empty<NonPersistentCusContainer>();

		void IContainerAggregation.SetDocuments(IReadOnlyCollection<string> documents) =>
			(transportInformationCached ??= new IM413AndIM415GoodsShipmentTransportInformationProvider(entryHeaderWrapper))
			.SetContainerIDs(documents.ToArray());

		#endregion
	}
}
