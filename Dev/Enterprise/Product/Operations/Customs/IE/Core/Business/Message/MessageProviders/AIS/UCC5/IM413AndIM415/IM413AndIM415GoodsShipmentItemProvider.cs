using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using IContainerAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.Business.NonPersistentCusContainer, string>;
using NonPersistentCusContainer = Enterprise.Customs.Business.NonPersistentCusContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentItemProvider : IGoodsShipmentItemType, IContainerAggregation
	{
		public IM413AndIM415GoodsShipmentItemProvider(IM413AndIM415GoodsShipmentProvider header, CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
			this.entryLine = entryLine;
			entryLineWrapper = new EntryLineWrapper(entryLine, entryHeaderWrapper);
			invoiceLine = entryLineWrapper.RandomInvoiceLine;
			this.header = header;
		}
		readonly IM413AndIM415GoodsShipmentProvider header;
		readonly EntryHeaderWrapper entryHeaderWrapper;
		readonly CusEntryLine entryLine;
		readonly EntryLineWrapper entryLineWrapper;
		readonly JobComInvoiceLine invoiceLine;

		public string GoodsItemNumber => entryLine.CL_LineNumber.ToString();

		public IProcedure Procedure => CachedValueHelper.GetValue(ref procedure, () => new ProcedureProvider(invoiceLine));
		CachedValue<IProcedure> procedure;

		public IReadOnlyCollection<string> AdditionalProcedure => additionalProcedureCached ??= invoiceLine.AdditionalProcedureCodesIncludingConcession.Select(code => code.ToString()).ToArray();
		IReadOnlyCollection<string> additionalProcedureCached;

		public IGoodsShipmentItemTypeParties Parties => CachedValueHelper.GetValue(ref parties, () => new IM413AndIM415GoodsShipmentItemTypePartiesProvider(entryLine.RandomLine));
		CachedValue<IGoodsShipmentItemTypeParties> parties;

		public IGoodsShipmentItemTypeDocumentsAuthorisations DocumentsAuthorisations => CachedValueHelper.GetValue(ref documentsAuthorisations, () => new IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProvider(entryLine));
		CachedValue<IGoodsShipmentItemTypeDocumentsAuthorisations> documentsAuthorisations;

		public IGoodsShipmentItemTypeValuationInformation ValuationInformation => CachedValueHelper.GetValue(ref valuationInformationCached, () => new IM413AndIM415GoodsShipmentItemValuationInformationProvider(entryLine, entryHeaderWrapper));
		CachedValue<IGoodsShipmentItemTypeValuationInformation> valuationInformationCached;

		public IGoodsShipmentItemTypeDatesPlaces DatesPlaces => CachedValueHelper.GetValue(ref datesPlacesCached, () => new IM413AndIM415GoodsShipmentItemDatesPlacesProvider(entryLine.RandomLine));
		CachedValue<IGoodsShipmentItemTypeDatesPlaces> datesPlacesCached;

		public IGoodsShipmentItemTypeGoodsInformation GoodsInformation => CachedValueHelper.GetValue(ref goodsInformationCached, () => new IM413AndIM415GoodsShipmentItemGoodsInformationProvider(entryLine));
		CachedValue<IGoodsShipmentItemTypeGoodsInformation> goodsInformationCached;

		public IReadOnlyCollection<string> ContainerIdentificationNumbers
		{
			get
			{
				if (containerIdentificationNumbersCached == null)
				{
					header.AggregateDocuments();
				}
				return containerIdentificationNumbersCached;
			}
		}
		IReadOnlyCollection<string> containerIdentificationNumbersCached;

		public string QuotaOrderNumber => invoiceLine.JI_ConcessionOrder;

		public string TransactionNature => null;

		public decimal StatisticalValue => entryLine.CL_StatisticalValue;

		#region Container Aggregation

		IEnumerable<NonPersistentCusContainer> IContainerAggregation.GetDocumentObjects() => entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Where(cnt => cnt.IsForInvoiceLine);

		void IContainerAggregation.SetDocuments(IReadOnlyCollection<string> documents) => containerIdentificationNumbersCached = documents;

		#endregion
	}
}
