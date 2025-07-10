using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSPresentationGoodItemWrapper : T2LPOUSCommonGoodsItemWrapper, IT2LPOUSPresentationGoodItem
	{
		public T2LPOUSPresentationGoodItemWrapper(CusEntryLine entryLine) : base(entryLine)
		{
			randomLine = entryLine.RandomLine;
		}
		readonly JobComInvoiceLine randomLine;

		public IReadOnlyCollection<IT2LPOUSPresentationPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					var prevDocsList = new List<T2LPOUSPresentationPreviousDocumentWrapper>();
					var totalGrossWeightInKGFromInvoiceLine = entryLine.TotalGrossWeightInKG;
					var firstPackQty = entryLine.FirstPackQty();
					var firstPackType = entryLine.FirstPackType();

					entryLine.PreviousDocuments.ForEach(doc => prevDocsList.Add(new T2LPOUSPresentationPreviousDocumentWrapper((PreviousDocument)doc, totalGrossWeightInKGFromInvoiceLine, firstPackType, firstPackQty)));

					previousDocuments = prevDocsList.AsReadOnly();
				}
				return previousDocuments;
			}
		}
		ReadOnlyCollection<T2LPOUSPresentationPreviousDocumentWrapper> previousDocuments;

		public ZInt T2LT2LFgoodsItemNumber => randomLine.ZG_T2LItemNumber;
	}
}
