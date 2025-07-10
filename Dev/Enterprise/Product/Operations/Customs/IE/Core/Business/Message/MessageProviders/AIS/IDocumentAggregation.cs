using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public interface IDocumentAggregation<TDocumentBiz, TDocument> where TDocumentBiz : BusinessObject
	{
		IEnumerable<TDocumentBiz> GetDocumentObjects();
		void SetDocuments(IReadOnlyCollection<TDocument> documents);
	}

	public interface IDocumentAggregationHeader<TDocumentBiz, TDocument> : IDocumentAggregation<TDocumentBiz, TDocument> where TDocumentBiz : BusinessObject
	{
		string[] GetDocumentKeys();
		IEnumerable<IDocumentAggregation<TDocumentBiz, TDocument>> GetItemProviders();
		TDocument Create(TDocumentBiz documentBiz);
	}
}
