using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;
using IContainerAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.Business.NonPersistentCusContainer, string>;
using IContainerAggregationHeader = Enterprise.Customs.IE.Business.AIS.IDocumentAggregationHeader<Enterprise.Customs.Business.NonPersistentCusContainer, string>;
using IPreviousDocumentAggregation = Enterprise.Customs.IE.Business.AIS.IDocumentAggregation<Enterprise.Customs.IE.Business.Declaration.PreviousDocument, CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces.ISimplifiedDeclarationDocumentWritingOff>;
using IPreviousDocumentAggregationHeader = Enterprise.Customs.IE.Business.AIS.IDocumentAggregationHeader<Enterprise.Customs.IE.Business.Declaration.PreviousDocument, CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces.ISimplifiedDeclarationDocumentWritingOff>;
using NonPersistentCusContainer = Enterprise.Customs.Business.NonPersistentCusContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM432GoodsShipmentItemProvider : IIM432GoodsShipmentItemType, IIM432GoodsInformation
		, IPreviousDocumentAggregation
		, IContainerAggregation
	{
		public static IM432GoodsShipmentItemProvider New(IM432GoodsShipmentProvider header, CusEntryLine entryLine) => new IM432GoodsShipmentItemProvider(header, entryLine);

		public IM432GoodsShipmentItemProvider(IM432GoodsShipmentProvider header, CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
			this.header = header;
		}

		readonly IM432GoodsShipmentProvider header;
		CusEntryLine entryLine { get; }

		#region IIM432GoodsShipmentItemType

		public string GoodsItemNumber => entryLine.CL_LineNumber.ToString();

		public IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> DocumentsAuthorisations
		{
			get
			{
				if (documentsAuthorisationsCache == null)
				{
					((IPreviousDocumentAggregationHeader)header).AggregateDocuments();
				}
				return documentsAuthorisationsCache;
			}
		}
		IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> documentsAuthorisationsCache;

		public IIM432GoodsInformation GoodsInformation => this;

		public IReadOnlyCollection<string> ContainerId
		{ 
			get
			{
				if (containerIdCache == null)
				{
					((IContainerAggregationHeader)header).AggregateDocuments();
				}
				return containerIdCache;
			}
		}
		IReadOnlyCollection<string> containerIdCache;

		#endregion

		#region IIM432GoodsInformation

		public decimal GrossMass => entryLine.EffectiveGrossWeight.InKilogramsSafe;

		public IReadOnlyCollection<IPackaging> Packaging => packagingCache ??= AISPackagingProvider.GetCollection(entryLine);
		IReadOnlyCollection<IPackaging> packagingCache;

		#endregion

		#region Document Aggregations

		IEnumerable<PreviousDocument> IPreviousDocumentAggregation.GetDocumentObjects() => entryLine.PreviousDocuments.Cast<PreviousDocument>();

		void IPreviousDocumentAggregation.SetDocuments(IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> documents) => documentsAuthorisationsCache = documents;

		IEnumerable<NonPersistentCusContainer> IContainerAggregation.GetDocumentObjects() => entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Where(cnt => cnt.IsForInvoiceLine);

		void IContainerAggregation.SetDocuments(IReadOnlyCollection<string> documents) => containerIdCache = documents;

		#endregion
	}
}
