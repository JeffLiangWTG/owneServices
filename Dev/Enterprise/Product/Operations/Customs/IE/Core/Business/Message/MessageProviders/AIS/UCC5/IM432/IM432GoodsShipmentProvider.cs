using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using IContainerAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.Business.NonPersistentCusContainer, string>;
using IContainerAggregationHeader = Enterprise.Customs.IE.Business.AIS.IDocumentAggregationHeader<Enterprise.Customs.Business.NonPersistentCusContainer, string>;
using IPreviousDocumentAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.IE.Business.Declaration.PreviousDocument, CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces.ISimplifiedDeclarationDocumentWritingOff>;
using IPreviousDocumentAggregationHeader = Enterprise.Customs.IE.Business.AIS.IDocumentAggregationHeader<Enterprise.Customs.IE.Business.Declaration.PreviousDocument, CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces.ISimplifiedDeclarationDocumentWritingOff>;
using NonPersistentCusContainer = Enterprise.Customs.Business.NonPersistentCusContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM432GoodsShipmentProvider : IIM432GoodsShipmentType, IPreviousDocumentAggregationHeader, IContainerAggregationHeader
	{
		public IM432GoodsShipmentProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
			entryHeader = entryHeaderWrapper.EntryHeader;
		}
		readonly EntryHeaderWrapper entryHeaderWrapper;
		readonly CusEntryHeader entryHeader;

		public string MRN => entryHeader.MovementReferenceNumber;

		public IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> DocumentsAuthorisations
		{
			get
			{
				if (documentsAuthorisationsCache == null)
				{
					AISMessageProviderHelper.AggregateDocuments((IPreviousDocumentAggregationHeader)this);
				}
				return documentsAuthorisationsCache;
			}
		}
		ISimplifiedDeclarationDocumentWritingOff[] documentsAuthorisationsCache;

		public IGoodsLocation GoodsLocation => CachedValueHelper.GetValue(ref goodsLocation, () => new GoodsLocationProvider(entryHeaderWrapper.Instruction.GoodsLocation));
		CachedValue<IGoodsLocation> goodsLocation;

		public IReadOnlyCollection<string> ContainerIds
		{
			get
			{
				if (transportInformationCache == null)
				{
					AISMessageProviderHelper.AggregateDocuments((IContainerAggregationHeader)this);
				}
				return transportInformationCache;
			}
		}
		string[] transportInformationCache;

		public IReadOnlyCollection<IIM432GoodsShipmentItemType> GoodsShipmentItems
		{
			get
			{
				if (goodsShipmentItemCache == null)
				{
					goodsShipmentItemCache = entryHeader.MergedLines.Select(line => new IM432GoodsShipmentItemProvider(this, line)).ToArray();
				}
				return goodsShipmentItemCache;
			}
		}
		IReadOnlyCollection<IIM432GoodsShipmentItemType> goodsShipmentItemCache;

		#region PreviousDocument Aggregation

		string[] IPreviousDocumentAggregationHeader.GetDocumentKeys() => new[]
		{
			AdditionalInfo.Schema.CSI_Code,
			AdditionalInfo.Schema.CSI_ReferenceNumber
		};

		IEnumerable<IPreviousDocumentAggregation> IPreviousDocumentAggregationHeader.GetItemProviders() => GoodsShipmentItems.Cast<IPreviousDocumentAggregation>();

		ISimplifiedDeclarationDocumentWritingOff IPreviousDocumentAggregationHeader.Create(PreviousDocument documentBiz) => PreviousDocumentProvider.New(documentBiz);

		IEnumerable<PreviousDocument> IPreviousDocumentAggregation.GetDocumentObjects() => entryHeader.PreviousDocuments;

		void IPreviousDocumentAggregation.SetDocuments(IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> documents) => documentsAuthorisationsCache = documents.ToArray();

		#endregion

		#region Container Aggregation

		string[] IContainerAggregationHeader.GetDocumentKeys() => new[]
		{
			NonPersistentCusContainer.Schema.ContainerNumber
		};

		IEnumerable<IContainerAggregation> IContainerAggregationHeader.GetItemProviders() => GoodsShipmentItems.Cast<IContainerAggregation>();

		string IContainerAggregationHeader.Create(NonPersistentCusContainer container) => container.ContainerNumber;

		IEnumerable<NonPersistentCusContainer> IContainerAggregation.GetDocumentObjects() => Enumerable.Empty<NonPersistentCusContainer>();

		void IContainerAggregation.SetDocuments(IReadOnlyCollection<string> documents) => transportInformationCache = documents.ToArray();

		#endregion
	}
}
